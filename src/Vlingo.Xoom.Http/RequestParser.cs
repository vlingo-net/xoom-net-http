// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Wire.Message;

namespace Vlingo.Xoom.Http
{
    public class RequestParser
    {
        private readonly VirtualStateParser _virtualStateParser;
        private const string Header = "==========";

        public static RequestParser ParserFor(IConsumerByteBuffer requestContent)
            => new RequestParser(requestContent);

        public bool HasCompleted => _virtualStateParser.HasCompleted;

        public Request FullRequest() => _virtualStateParser.FullRequest();

        public bool HasFullRequest() => _virtualStateParser.HasFullRequest();

        public bool HasMissingContentTimeExpired(long timeLimit) =>
            _virtualStateParser.HasMissingContentTimeExpired(timeLimit);

        public bool IsMissingContent => _virtualStateParser.IsMissingContent;
        
        public string CurrentRequestText => _virtualStateParser.RequestText;

        public void ParseNext(IConsumerByteBuffer requestContent) => _virtualStateParser.Includes(requestContent).Parse();

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Header).Append(" REQUEST PARSER CONTEXT:\n");

            builder.Append(_virtualStateParser.Method?.Name()).Append(" ").Append(_virtualStateParser.Uri).Append("\n");

            foreach (var header in _virtualStateParser.Headers)
            {
                builder.Append(header.Name).Append(": ").Append(header.Value).Append("\n");
            }

            builder.Append("\n").Append(_virtualStateParser.Body != null ? _virtualStateParser.Body.Content : "");

            builder.Append(Header).Append("\n").Append("DANGLING:\n").Append(_virtualStateParser.RequestText).Append("\n");

            builder.Append(Header).Append(" END\n");

            return builder.ToString();
        }

        private RequestParser(IConsumerByteBuffer requestContent) =>
            _virtualStateParser = new VirtualStateParser().Includes(requestContent).Parse();

        //=========================================
        // VirtualStateParser
        //=========================================

        private class VirtualStateParser
        {
            private enum Step
            {
                NotStarted,
                RequestLine,
                Headers,
                Body,
                Completed
            };

            // DO NOT RESET: (1) contentQueue, (2) position, (3) requestText

            private readonly Queue<ContentPacket> _contentQueue;
            private int _position;
            private string _requestText;


            // DO NOT RESET: (1) headers, (2) fullRequests

            private int _contentLength;
            private int _contentExtraLength;
            private bool _continuation;
            private Step _currentStep;
            private readonly List<Request> _fullRequests;
            private List<Request>.Enumerator _fullRequestsIterator;
            private bool _availableNext;
            private long _outOfContentTime;
            private Version? _version;
            
            internal Body? Body { get; private set; }
            internal Method? Method { get; private set; }
            internal Uri? Uri { get; private set; }
            internal Headers<RequestHeader> Headers { get; private set; }

            internal VirtualStateParser()
            {
                _outOfContentTime = 0;
                _contentQueue = new Queue<ContentPacket>();
                _currentStep = Step.NotStarted;
                _requestText = string.Empty;
                Headers = new Headers<RequestHeader>(2);
                _fullRequests = new List<Request>(2);

                Reset();
            }

            internal Request FullRequest()
            {
                if (_fullRequestsIterator.Current == null)
                {
                    _fullRequestsIterator = _fullRequests.GetEnumerator();
                }

                if (_availableNext)
                {
                    _availableNext = false;
                    var request = _fullRequestsIterator.Current;
                    _fullRequests.Remove(request);
                    _fullRequestsIterator = _fullRequests.GetEnumerator();
                    return request;
                }

                if (_fullRequestsIterator.MoveNext())
                {
                    var request = _fullRequestsIterator.Current;
                    _fullRequests.Remove(request);
                    _fullRequestsIterator = _fullRequests.GetEnumerator();
                    return request;
                }

                _fullRequestsIterator.Dispose();
                throw new InvalidOperationException(
                    $"{ResponseStatus.BadRequest}\n\nRequest is not completed: {Method.Name()} {Uri}");
            }

            internal bool HasFullRequest()
            {
                if (_fullRequestsIterator.Current != null)
                {
                    _availableNext = _fullRequestsIterator.MoveNext();
                    if (!_availableNext)
                    {
                        _fullRequestsIterator.Dispose();
                        return false;
                    }

                    return true;
                }

                if (_fullRequests.Count == 0)
                {
                    _fullRequestsIterator.Dispose();
                    return false;
                }

                return true;
            }

            internal bool HasCompleted
            {
                get
                {
                    if (IsNotStarted && _position >= _requestText.Length && _contentQueue.Count == 0)
                    {
                        _requestText = Compact();
                        return true;
                    }

                    return false;
                }
            }

            internal bool HasMissingContentTimeExpired(long timeLimit)
                => _outOfContentTime + timeLimit < DateExtensions.GetCurrentMillis();

            internal VirtualStateParser Includes(IConsumerByteBuffer requestContent)
            {
                _outOfContentTime = 0;
                var requestContentText = Converters.BytesToText(requestContent.ToArray());
                var utf8ExtraLength = requestContent.Remaining - requestContentText.Length;
                if (_contentQueue.Count == 0)
                {
                    _contentExtraLength += (int) utf8ExtraLength;
                    _requestText = _requestText + requestContentText;
                }
                else
                {
                    _contentQueue.Enqueue(new ContentPacket(requestContentText, (int) utf8ExtraLength));
                }

                return this;
            }

            internal bool IsMissingContent => _outOfContentTime > 0;

            internal string RequestText => _requestText;

            internal VirtualStateParser Parse()
            {
                var isOutOfContent = false;
                while (!HasCompleted)
                {
                    if (IsNotStarted)
                    {
                        isOutOfContent = NextStep();
                    }
                    else if (IsRequestLineStep)
                    {
                        isOutOfContent = ParseRequestLine();
                    }
                    else if (IsHeadersStep)
                    {
                        isOutOfContent = ParseHeaders();
                    }
                    else if (IsBodyStep)
                    {
                        isOutOfContent = ParseBody();
                    }
                    else if (IsCompletedStep)
                    {
                        _continuation = false;
                        isOutOfContent = NewRequest();
                    }

                    if (isOutOfContent)
                    {
                        _continuation = true;
                        _outOfContentTime = (long) DateExtensions.GetCurrentMillis();
                        return this;
                    }
                }

                return this;
            }

            private string Compact()
            {
                var compact = _requestText.Substring(_position);
                _position = 0;
                return compact;
            }

            private Optional<string> NextLine(string errorResult, string errorMessage)
            {
                var possibleCarriageReturnIndex = -1;
                var lineBreak = _requestText.IndexOf('\n', _position);
                if (lineBreak < 0)
                {
                    if (_contentQueue.Count == 0)
                    {
                        _requestText = Compact();
                        return Optional.Empty<string>();
                    }

                    var packet = _contentQueue.Dequeue();
                    _contentExtraLength += packet.Utf8ExtraLength;
                    _requestText = $"{Compact()}{packet.Content}";
                    return NextLine(errorResult, errorMessage);
                }

                if (lineBreak == 0)
                {
                    possibleCarriageReturnIndex = 0;
                }

                var endOfLine = _requestText[lineBreak + possibleCarriageReturnIndex] == '\r'
                    ? lineBreak - 1
                    : lineBreak;
                var line = _requestText.Substring(_position, endOfLine - _position).Trim();
                _position = lineBreak + 1;
                return Optional.Of(line);
            }

            private bool NextStep()
            {
                if (IsNotStarted)
                {
                    _currentStep = Step.RequestLine;
                }
                else if (IsRequestLineStep)
                {
                    _currentStep = Step.Headers;
                }
                else if (IsHeadersStep)
                {
                    _currentStep = Step.Body;
                }
                else if (IsBodyStep)
                {
                    _currentStep = Step.Completed;
                }
                else if (IsCompletedStep)
                {
                    _currentStep = Step.NotStarted;
                }

                return false;
            }

            private bool IsNotStarted => _currentStep == Step.NotStarted;

            private bool IsRequestLineStep => _currentStep == Step.RequestLine;

            private bool IsHeadersStep => _currentStep == Step.Headers;

            private bool IsBodyStep => _currentStep == Step.Body;

            private bool IsCompletedStep => _currentStep == Step.Completed;

            private bool ParseBody()
            {
                _continuation = false;
                if (_contentLength > 0)
                {
                    var endIndex = _position + _contentLength;
                    if (_requestText.Length + _contentExtraLength < endIndex)
                    {
                        if (_contentQueue.Count == 0)
                        {
                            _requestText = Compact();
                            return true;
                        }

                        var packet = _contentQueue.Dequeue();
                        _requestText = $"{Compact()}{packet.Content}";
                        _contentExtraLength += packet.Utf8ExtraLength;
                        ParseBody();
                    }
                    else
                    {
                        Body = Body.From(_requestText.Substring(_position, endIndex - _position - _contentExtraLength));
                        _position += _contentLength - _contentExtraLength;
                        NextStep();
                    }
                }
                else
                {
                    Body = Body.Empty;
                    NextStep();
                }

                return false;
            }

            private bool ParseHeaders()
            {
                if (!_continuation)
                {
                    Headers = new Headers<RequestHeader>(2);
                }

                _continuation = false;
                while (true)
                {
                    var maybeHeaderLine = NextLine(ResponseStatus.BadRequest.GetDescription(),
                        "\n\nHeader is required.");
                    if (!maybeHeaderLine.IsPresent)
                    {
                        return true;
                    }

                    var headerLine = maybeHeaderLine.Get();

                    if (string.IsNullOrEmpty(headerLine))
                    {
                        break;
                    }
                    
                    var header = RequestHeader.FromString(headerLine);
                    Headers.Add(header);
                    if (_contentLength == 0)
                    {
                        var maybeContentLength = header.IfContentLength;
                        if (maybeContentLength > 0)
                        {
                            _contentLength = maybeContentLength;
                        }
                    }
                }

                if (Headers.Count == 0)
                {
                    throw new ArgumentException(ResponseStatus.BadRequest.GetDescription() +
                                                "\n\nHeader is required.");
                }

                return NextStep();
            }

            private bool ParseRequestLine()
            {
                _continuation = false;
                var maybeLine = NextLine(ResponseStatus.BadRequest.GetDescription(),
                    "\n\nRequest line is required.");
                if (!maybeLine.IsPresent)
                {
                    return true;
                }

                var line = maybeLine.Get();
                
                var parts = line.Split(' ');

                try
                {
                    Method = ParseSpecificRequestLinePart(parts, 1, "Method").ToMethod();
                    Uri = ParseSpecificRequestLinePart(parts, 2, "URI/path").ToMatchableUri();
                    _version = Version.From(ParseSpecificRequestLinePart(parts, 3, "HTTP/version"));

                    return NextStep();
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        $"{ResponseStatus.BadRequest.GetDescription()}\n\nParsing exception: {e.Message}", e);
                }
            }

            private string ParseSpecificRequestLinePart(string[] parts, int part, string name)
            {
                var partCount = 0;
                for (var idx = 0; idx < parts.Length; ++idx)
                {
                    if (parts[idx].Length > 0)
                    {
                        if (++partCount == part)
                        {
                            return parts[idx];
                        }
                    }
                }

                throw new ArgumentException(
                    $"{ResponseStatus.BadRequest.GetDescription()}\n\nRequest line part missing: {name}");
            }

            private bool NewRequest()
            {
                var request = new Request(Method, Uri, _version, Headers, Body);
                _fullRequests.Add(request);
                Reset();
                return NextStep();
            }

            private void Reset()
            {
                // DO NOT RESET: (1) contentQueue, (2) position, (3) requestText, (4) headers, (5) fullRequests

                Body = null;
                _contentLength = 0;
                _contentExtraLength = 0;
                _continuation = false;
                Method = null;
                _outOfContentTime = 0;
                _version = null;
                Uri = null;
            }
        }
    }
}