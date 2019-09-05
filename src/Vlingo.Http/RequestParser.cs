// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Wire.Message;

namespace Vlingo.Http
{
    public class RequestParser
    {
        private readonly VirtualStateParser _virtualStateParser;

        public static RequestParser ParseFor(byte[] requestContent)
            => new RequestParser(requestContent);

        public bool HasCompleted => _virtualStateParser.HasCompleted;

        public Request FullRequest => _virtualStateParser.FullRequest;

        public bool HasFullRequest => _virtualStateParser.HasFullRequest;

        public bool HasMissingContentTimeExpired(long timeLimit) => _virtualStateParser.HasMissingContentTimeExpired(timeLimit);

        public bool IsMissingContent => _virtualStateParser.IsMissingContent;

        public void ParseNext(byte[] requestContent) => _virtualStateParser.Includes(requestContent).Parse();

        private RequestParser(byte[] requestContent)
        {
            _virtualStateParser = new VirtualStateParser().Includes(requestContent).Parse();
        }

        //=========================================
        // VirtualStateParser
        //=========================================

        private class VirtualStateParser
        {
            private class OutOfContentException : Exception { private const long serialVersionUID = 1L; }

            private enum Step { NotStarted, RequestLine, Headers, Body, Completed };

            // DO NOT RESET: (1) contentQueue, (2) position, (3) requestText

            private readonly Queue<string> _contentQueue;
            private int _position;
            private string _requestText;


            // DO NOT RESET: (1) headers, (2) fullRequests

            private Body _body;
            private int _contentLength;
            private bool _continuation;
            private Step _currentStep;
            private List<Request> _fullRequests;
            private int _fullRequestsIteratorPos;
            private Headers<RequestHeader> _headers;
            private Method _method;
            private DateTime _outOfContentTime;
            private Uri _uri;
            private Version _version;

            internal VirtualStateParser()
            {
                _fullRequestsIteratorPos = -1;
                _outOfContentTime = DateTime.MinValue;
                _contentQueue = new Queue<string>();
                _currentStep = Step.NotStarted;
                _requestText = string.Empty;
                _headers = new Headers<RequestHeader>(2);
                _fullRequests = new List<Request>(2);

                Reset();
            }

            internal Request FullRequest {
                get
                {
                    if(_fullRequestsIteratorPos < 0)
                    {
                        _fullRequestsIteratorPos = 0;
                    }

                    if (HasNextFullRequest())
                    {
                        var req = _fullRequests[_fullRequestsIteratorPos];
                        _fullRequests.RemoveAt(_fullRequestsIteratorPos);
                        return req;
                    }

                    throw new InvalidOperationException($"{Response.ResponseStatus.BadRequest}\n\nRequest is not completed: {_method} {_uri}");
                }
            }

            internal bool HasFullRequest
            {
                get
                {
                    if(_fullRequestsIteratorPos >= 0)
                    {
                        if (!HasNextFullRequest())
                        {
                            _fullRequestsIteratorPos = -1;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    if(_fullRequests.Count == 0)
                    {
                        _fullRequestsIteratorPos = -1;
                        return false;
                    }

                    return true;
                }
            }

            internal bool HasCompleted
            {
                get
                {
                    if(IsNotStarted && _position >= _requestText.Length && _contentQueue.Count == 0)
                    {
                        _requestText = Compact();
                        return true;
                    }
                    return false;
                }
            }

            internal bool HasMissingContentTimeExpired(long timeLimit)
                => (DateTime.UtcNow - _outOfContentTime).TotalMilliseconds > timeLimit;

            internal VirtualStateParser Includes(byte[] requestContent)
            {
                _outOfContentTime = DateTime.MinValue;
                
                var requestContentText = Converters.BytesToText(requestContent);
                if(_contentQueue.Count == 0)
                {
                    _requestText = _requestText + requestContentText;
                }
                else
                {
                    _contentQueue.Enqueue(requestContentText);
                }

                return this;
            }

            internal bool IsMissingContent => _outOfContentTime > DateTime.MinValue;

            internal VirtualStateParser Parse()
            {
                while (!HasCompleted)
                {
                    try
                    {
                        if (IsNotStarted)
                        {
                            NextStep();
                        }
                        else if (IsRequestLineStep)
                        {
                            ParseRequestLine();
                        }
                        else if (IsHeadersStep)
                        {
                            ParseHeaders();
                        }
                        else if (IsBodyStep)
                        {
                            ParseBody();
                        }
                        else if (IsCompletedStep)
                        {
                            _continuation = false;
                            NewRequest();
                        }
                    }
                    catch(OutOfContentException)
                    {
                        _continuation = true;
                        _outOfContentTime = DateTime.UtcNow;
                        return this;
                    }
                    catch(Exception ex)
                    {
                        throw ex;
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

            private string NextLine(string errorResult, string errorMessage)
            {
                var possibleCarriageReturnIndex = -1;
                var lineBreak = _requestText.IndexOf("\n", _position, StringComparison.InvariantCultureIgnoreCase);
                if(lineBreak < 0)
                {
                    if (_contentQueue.Count == 0)
                    {
                        _requestText = Compact();
                        throw new OutOfContentException();
                    }
                    _requestText = Compact() + _contentQueue.Dequeue();
                    return NextLine(errorResult, errorMessage);
                }
                else if (lineBreak == 0)
                {
                    possibleCarriageReturnIndex = 0;
                }

                var endOfLine = _requestText[lineBreak + possibleCarriageReturnIndex] == '\r' ? lineBreak - 1 : lineBreak;
                var line = _requestText.Substring(_position, endOfLine - _position).Trim();
                _position = lineBreak + 1;
                return line;
            }

            private void NextStep()
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
            }

            private bool IsNotStarted => _currentStep == Step.NotStarted;

            private bool IsRequestLineStep => _currentStep == Step.RequestLine;

            private bool IsHeadersStep => _currentStep == Step.Headers;

            private bool IsBodyStep => _currentStep == Step.Body;

            private bool IsCompletedStep => _currentStep == Step.Completed;

            private bool HasNextFullRequest() => _fullRequestsIteratorPos < _fullRequests.Count - 1;

            private void ParseBody()
            {
                _continuation = false;
                if (_contentLength > 0)
                {
                    var endIndex = _position + _contentLength;
                    if (_requestText.Length < endIndex)
                    {
                        if (_contentQueue.Count == 0)
                        {
                            _requestText = Compact();
                            throw new OutOfContentException();
                        }
                        _requestText = Compact() + _contentQueue.Dequeue();
                        ParseBody();
                    }
                    else
                    {
                        _body = Body.From(_requestText.Substring(_position, endIndex - _position));
                        _position += _contentLength;
                        NextStep();
                    }
                }
                else
                {
                    _body = Body.Empty;
                    NextStep();
                }
            }

            private void ParseHeaders()
            {
                if (!_continuation)
                {
                    _headers = new Headers<RequestHeader>(2);
                }
                _continuation = false;
                while (true)
                {
                    var maybeHeaderLine = NextLine(Response.ResponseStatus.BadRequest.GetDescription(), "\n\nHeader is required.");
                    if (string.IsNullOrEmpty(maybeHeaderLine))
                    {
                        break;
                    }

                    var header = RequestHeader.FromString(maybeHeaderLine);
                    _headers.Add(header);
                    if (_contentLength == 0)
                    {
                        var maybeContentLength = header.IfContentLength;
                        if (maybeContentLength > 0)
                        {
                            _contentLength = maybeContentLength;
                        }
                    }
                }
                if (_headers.Count == 0)
                {
                    throw new ArgumentException(Response.ResponseStatus.BadRequest.GetDescription() + "\n\nHeader is required.");
                }
                NextStep();
            }

            private void ParseRequestLine()
            {
                _continuation = false;
                var line = NextLine(Response.ResponseStatus.BadRequest.GetDescription(), "\n\nRequest line is required.");
                var parts = line.Split(' ');

                try
                {
                    _method = Method.From(ParseSpecificRequestLinePart(parts, 1, "Method"));
                    _uri = ParseSpecificRequestLinePart(parts, 2, "URI/path").ToMatchableUri();
                    _version = Version.From(ParseSpecificRequestLinePart(parts, 3, "HTTP/version"));

                    NextStep();
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"{Response.ResponseStatus.BadRequest.GetDescription()}\n\nParsing exception: {e.Message}", e);
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
                throw new ArgumentException($"{Response.ResponseStatus.BadRequest.GetDescription()}\n\nRequest line part missing: {name}");
            }

            private void NewRequest()
            {
                var request = new Request(_method, _uri, _version, _headers, _body);
                _fullRequests.Add(request);
                Reset();
                NextStep();
            }

            private void Reset()
            {
                // DO NOT RESET: (1) contentQueue, (2) position, (3) requestText, (4) headers, (5) fullRequests

                _body = null;
                _contentLength = 0;
                _continuation = false;
                _method = null;
                _outOfContentTime = DateTime.MinValue;
                _version = null;
                _uri = null;
            }
        }
    }
}
