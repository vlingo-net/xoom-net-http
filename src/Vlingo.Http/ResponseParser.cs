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
    public class ResponseParser
    {
        private readonly VirtualStateParser _virtualStateParser;

        public static ResponseParser ParserFor(byte[] requestContent)
            => new ResponseParser(requestContent);
        
        public static ResponseParser ParserForBodyOnly(byte[] requestContent)
            => new ResponseParser(requestContent, true);

        public Response FullResponse() => _virtualStateParser.FullResponse();
        
        public bool HasCompleted => _virtualStateParser.HasCompleted;

        public bool HasFullResponse() => _virtualStateParser.HasFullResponse();

        public bool HasMissingContentTimeExpired(long timeLimit)
            => _virtualStateParser.HasMissingContentTimeExpired(timeLimit);

        public bool IsKeepAliveConnection => _virtualStateParser.IsKeepAliveConnection;

        public bool IsMissingContent => _virtualStateParser.IsMissingContent;
        
        public bool IsStreamContentType => _virtualStateParser.IsStreamContentType;

        public void ParseNext(byte[] responseContent)
            => _virtualStateParser.Includes(responseContent).Parse();

        private ResponseParser(byte[] responseContent)
        {
            _virtualStateParser = new VirtualStateParser().Includes(responseContent).Parse();
        }
        
        private ResponseParser(byte[] responseContent, bool bodyOnly)
        {
            _virtualStateParser = new VirtualStateParser(bodyOnly).Includes(responseContent).Parse();
        }


        //=========================================
        // VirtualStateParser
        //=========================================

        private class VirtualStateParser
        {
            private class OutOfContentException : Exception { private const long SerialVersionUid = 1L; }

            private enum Step { NotStarted, StatusLine, Headers, Body, Completed }

            // DO NOT RESET: (1) contentQueue, (2) position, (3) requestText (4) currentResponseTextLength

            private readonly Queue<string> _contentQueue;
            private int _position;
            private string _responseText;
            private int _currentResponseTextLength;

            // DO NOT RESET: (1) headers, (2) fullResponses

            private Body? _body;
            private bool _bodyOnly;
            private int _contentLength;
            private bool _continuation;
            private Step _currentStep;
            private readonly List<Response> _fullResponses;
            private List<Response>.Enumerator _fullResponsesIterator;
            private bool _availableNext;
            private Headers<ResponseHeader> _headers;
            private bool _keepAlive;
            private long _outOfContentTime;
            private Response.ResponseStatus _status;
            private bool _stream;
            private Version? _version;

            internal VirtualStateParser() : this(false)
            {
            }
            
            internal VirtualStateParser(bool bodyOnly)
            {
                _bodyOnly = bodyOnly;
                _outOfContentTime = 0;
                _contentQueue = new Queue<string>();
                _currentStep = Step.NotStarted;
                _responseText = string.Empty;
                _headers = new Headers<ResponseHeader>(2);
                _fullResponses = new List<Response>(2);

                _keepAlive = false;
                _stream = false;

                Reset();
            }

            internal Response FullResponse()
            {
                if (_fullResponsesIterator.Current == null)
                {
                    _fullResponsesIterator = _fullResponses.GetEnumerator();
                }

                if (_availableNext)
                {
                    _availableNext = false;
                    var response = _fullResponsesIterator.Current;
                    _fullResponses.Remove(response);
                    _fullResponsesIterator = _fullResponses.GetEnumerator();
                    return response;
                }

                if (_fullResponsesIterator.MoveNext())
                {
                    var response = _fullResponsesIterator.Current;
                    _fullResponses.Remove(response);
                    _fullResponsesIterator = _fullResponses.GetEnumerator();
                    return response;
                }

                _fullResponsesIterator.Dispose();
                throw new InvalidOperationException("Response is not completed.");
            }

            internal bool HasFullResponse()
            {
                if (_fullResponsesIterator.Current != null)
                {
                    _availableNext = _fullResponsesIterator.MoveNext();
                    if (!_availableNext)
                    {
                        _fullResponsesIterator.Dispose();
                        return false;
                    }

                    return true;
                }

                if (_fullResponses.Count == 0)
                {
                    _fullResponsesIterator.Dispose();
                    return false;
                }

                return true;
            }

            internal bool HasCompleted
            {
                get
                {
                    if (IsNotStarted && _position >= _currentResponseTextLength && _contentQueue.Count == 0)
                    {
                        _responseText = Compact();
                        return true;
                    }
                    return false;
                }
            }

            internal bool HasMissingContentTimeExpired(long timeLimit)
                => _outOfContentTime + timeLimit < DateExtensions.GetCurrentMillis();

            internal VirtualStateParser Includes(byte[] responseContent)
            {
                _outOfContentTime = 0;
                var responseContentText = Converters.BytesToText(responseContent);
                if (_contentQueue.Count == 0)
                {
                    _responseText = _responseText + responseContentText;
                    _currentResponseTextLength = _responseText.Length;
                }
                else
                {
                    _contentQueue.Enqueue(responseContentText);
                }
                return this;
            }

            internal bool IsKeepAliveConnection => _keepAlive;

            internal bool IsMissingContent => _outOfContentTime > 0;

            internal bool IsStreamContentType => _stream;

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
                        else if (IsStatusLineStep)
                        {
                            ParseStatusLine();
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
                            NewResponse();
                        }
                    }
                    catch (OutOfContentException)
                    {
                        _continuation = true;
                        _outOfContentTime = (long)DateExtensions.GetCurrentMillis();
                        return this;
                    }
                }
                
                PrepareForStream();
                
                return this;
            }

            private string Compact()
            {
                var compact = _responseText.Substring(_position);
                _position = 0;
                _currentResponseTextLength = compact.Length;
                return compact;
            }

            private string NextLine(bool mayBeBlank, string? errorMessage)
            {
                var possibleCarriageReturnIndex = -1;
                var lineBreak = _responseText.IndexOf("\n", _position, StringComparison.OrdinalIgnoreCase);
                if (lineBreak < 0)
                {
                    if (_contentQueue.Count == 0)
                    {
                        _responseText = Compact();
                        throw new OutOfContentException();
                    }
                    _responseText = Compact() + _contentQueue.Dequeue();
                    return NextLine(mayBeBlank, errorMessage);
                }

                if (lineBreak == 0)
                {
                    possibleCarriageReturnIndex = 0;
                }

                var endOfLine = _responseText[lineBreak + possibleCarriageReturnIndex] == '\r' ? lineBreak - 1 : lineBreak;
                var line = _responseText.Substring(_position, endOfLine - _position).Trim();
                _position = lineBreak + 1;
                return line;
            }

            private void NextStep()
            {
                if (IsNotStarted)
                {
                    _currentStep = Step.StatusLine;
                }
                else if (IsStatusLineStep)
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

            private bool IsStatusLineStep => _currentStep == Step.StatusLine;

            private bool IsHeadersStep => _currentStep == Step.Headers;

            private bool IsBodyStep => _currentStep == Step.Body;

            private bool IsCompletedStep => _currentStep == Step.Completed;

            private void ParseBody()
            {
                if (_bodyOnly)
                {
                    _contentLength = _responseText.Length;
                }
                
                _continuation = false;
                if (_contentLength > 0)
                {
                    var endIndex = _position + _contentLength;
                    if (_currentResponseTextLength < endIndex)
                    {
                        if (_contentQueue.Count == 0)
                        {
                            _responseText = Compact();
                            throw new OutOfContentException();
                        }
                        _responseText = Compact() + _contentQueue.Dequeue();
                        ParseBody();
                        return;
                    }
                    _body = Body.From(_responseText.Substring(_position, endIndex - _position));
                    _position += _contentLength;
                }
                else
                {
                    _body = Body.Empty;
                }
                NextStep();
            }

            private void ParseHeaders()
            {
                if (_bodyOnly)
                {
                    NextStep();
                    return;
                }
                
                if (!_continuation)
                {
                    _headers = new Headers<ResponseHeader>(2);
                }
                _continuation = false;
                while (true)
                {
                    var maybeHeaderLine = NextLine(true, null);
                    if (string.IsNullOrEmpty(maybeHeaderLine))
                    {
                        break;
                    }

                    var header = ResponseHeader.From(maybeHeaderLine);
                    _headers.Add(header);
                    if (_contentLength == 0)
                    {
                        var maybeContentLength = header.IfContentLength;
                        if (maybeContentLength > 0)
                        {
                            _contentLength = maybeContentLength;
                        }
                    }
                    
                    if (!_keepAlive && header.IsKeepAliveConnection)
                    {
                        _keepAlive = true;
                    }
                    else if (!_stream && header.IsStreamContentType)
                    {
                        _stream = true;
                    }
                }
                NextStep();
            }

            private void ParseStatusLine()
            {
                if (_bodyOnly)
                {
                    _version = Version.Http1_1;
                    _status = Response.ResponseStatus.Ok;
                    NextStep();
                    return;
                }
                
                _continuation = false;
                var line = NextLine(false, "Response status line is required.");
                var spaceIndex = line.IndexOf(' ');

                try
                {
                    _version = Version.From(line.Substring(0, spaceIndex).Trim());
                    _status = line.Substring(spaceIndex + 1).Trim().ConvertToResponseStatus();

                    NextStep();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Response status line parsing exception:{ex.Message}", ex);
                }
            }
            
            private void PrepareForStream()
            {
                if (!_bodyOnly)
                {
                    if (_keepAlive && _stream)
                    {
                        _bodyOnly = true;
                    }
                }
            }

            private void NewResponse()
            {
                var response = Response.Of(_version, _status, _headers, _body);
                _fullResponses.Add(response);
                Reset();
                NextStep();
            }

            private void Reset()
            {
                // DO NOT RESET: (1) contentQueue, (2) position, (3) responseText, (4) headers, (5) fullResponses

                _body = null;
                _contentLength = 0;
                _continuation = false;
                _outOfContentTime = 0;
                _status = 0;
                _version = null;
            }
        }
    }
}
