// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vlingo.Xoom.Http.Resource;

public sealed class Action
{
    internal static readonly MatchResults UnmatchedResults = new(null, null, new List<string>(), "");

    private readonly IList<MappedParameter> _additionalParameters;
    private readonly Method _method;
    private readonly string? _uri;
    private readonly string? _originalTo;
    private readonly ToSpec _to;
    private readonly IMapper _mapper;
    private readonly Matchable _matchable;

    internal int Id { get; }

    public IMapper Mapper => _mapper;

    public Method Method => _method;

    public string? Uri => _uri;

    public string? OriginalTo => _originalTo;

    public ToSpec To => _to;

    public Action(
        int id,
        string? method,
        string? uri,
        string? to,
        string? mapper,
        IList<MappedParameter> additionalParameters)
    {
        Id = id;
        _method = method.ToMethod();
        _uri = uri;
        _to = new ToSpec(to);
        _originalTo = to;
        _mapper = mapper == null ? DefaultJsonMapper.Instance : MapperFrom(mapper);
        _additionalParameters = additionalParameters;
        _matchable = new Matchable(uri);
    }

    public Action(
        int id,
        string? method,
        string? uri,
        string? to,
        string? mapper)
        : this(id, method, uri, to, mapper, new List<MappedParameter>())
    { }

    internal MappedParameters Map(Request? request, IList<RawPathParameter> parameters)
    {
        var mapped = new List<MappedParameter>(parameters.Count);
        foreach (var typed in _to.Parameters)
        {
            if (typed.IsBody)
            {
                var body = MapBodyFrom(request);
                mapped.Add(new MappedParameter(typed.Type, body));
            }
            else
            {
                var raw = RawPathParameter.Named(typed.Name, parameters);
                if (raw == null)
                {
                    break;
                }
                var other = MapOtherFrom(raw);
                mapped.Add(new MappedParameter(typed.Type, other));
            }
        }
        mapped.AddRange(_additionalParameters);

        return new MappedParameters(Id, _method, _to.MethodName, mapped);
    }

    private int IndexOfNextSegmentStart(int currentIndex, string path)
    {
        var nextSegmentStart = path.IndexOf("/", currentIndex, StringComparison.InvariantCulture);
        if (nextSegmentStart < currentIndex)
        {
            return path.Length;
        }
        return nextSegmentStart;
    }

    internal MatchResults MatchWith(Method? method, Uri? uri)
    {
        if (_method.Equals(method))
        {
            if (uri == null || !uri.IsAbsoluteUri || uri.Scheme != "http" && uri.Scheme != "https")
            {
                throw new ArgumentException(
                    "In order to match on Uri, it has to be an absolute uri for http or https scheme", nameof(uri));
            }
                
            var path = uri.AbsolutePath;
            var pathCurrentIndex = 0;
            var totalSegments = _matchable.TotalSegments;
            var running = new RunningMatchSegments(totalSegments);
            for (var idx = 0; idx < totalSegments; ++idx)
            {
                var segment = _matchable.PathSegment(idx);
                if (segment.IsPathParameter)
                {
                    running.KeepParameterSegment(pathCurrentIndex);
                    pathCurrentIndex = IndexOfNextSegmentStart(pathCurrentIndex, path);
                }
                else
                {
                    var indexOfSegment = path.IndexOf(segment.Value, pathCurrentIndex, StringComparison.InvariantCulture);
                    if (indexOfSegment == -1 || (pathCurrentIndex == 0 && indexOfSegment != 0))
                    {
                        return UnmatchedResults;
                    }
                    var lastIndex = segment.LastIndexOf(indexOfSegment);
                    running.KeepPathSegment(indexOfSegment, lastIndex);
                    pathCurrentIndex = lastIndex;
                }
            }
            var nextPathSegmentIndex = IndexOfNextSegmentStart(pathCurrentIndex, path);
            if (nextPathSegmentIndex != path.Length)
            {
                if (nextPathSegmentIndex < path.Length - 1)
                {
                    return UnmatchedResults;
                }
            }
            var matchResults = new MatchResults(this, running, ParameterNames, path);
            return matchResults;
        }
        return UnmatchedResults;
    }

    public override int GetHashCode()
        => 31 * (_method.GetHashCode() + _uri!.GetHashCode() + _to!.GetHashCode() + _mapper.GetHashCode() + _matchable.GetHashCode());

    public override bool Equals(object? other)
    {
        if (other == null || other.GetType() != typeof(Action))
        {
            return false;
        }

        var otherAction = (Action)other;

        return _method.Equals(otherAction._method) && _uri!.Equals(otherAction._uri) && _to!.Equals(otherAction._to);
    }

    public override string ToString()
        => $"Action[Id={Id}, Method={_method}, Uri={_uri}, To={_to}]";

    private IMapper MapperFrom(string mapper)
    {
        try
        {
            var mapperClass = TypeLoader.Load(mapper);
            return (IMapper) Activator.CreateInstance(mapperClass)!;
        }
        catch
        {
            throw new InvalidOperationException($"Cannot load mapper class: {mapper}");
        }
    }

    private int ParameterCount => _matchable.PathSegments.Count(s => s.IsPathParameter);

    private object? MapBodyFrom(Request? request)
    {
        var body = _to.Body;
        if (body != null)
        {
            return _mapper.From(request?.Body?.ToString(), body.BodyType);
        }
        return null;
    }

    private object? MapOtherFrom(RawPathParameter parameter)
    {
        var type = _to.ParameterOf(parameter.Name)?.Type;

        switch (type)
        {
            case "String":
            case "string":
                return parameter.Value;
            case "int":
            case "Integer":
                return int.Parse(parameter.Value);
            case "long":
            case "Long":
                return long.Parse(parameter.Value);
            case "bool":
            case "Boolean":
                return bool.Parse(parameter.Value);
            case "double":
            case "Double":
                return double.Parse(parameter.Value);
            case "short":
            case "Short":
                return short.Parse(parameter.Value);
            case "float":
            case "Float":
                return float.Parse(parameter.Value);
            case "char":
            case "Character":
                return parameter.Value[0];
            case "byte":
            case "Byte":
                return byte.Parse(parameter.Value);
            default:
                return null;
        }
    }

    private IList<string> ParameterNames 
        => _matchable.PathSegments.Where(s => s.IsPathParameter).Select(s => s.Value).ToList();

    //=====================================
    // MappedParameters
    //=====================================

    public class MappedParameters
    {
        public int ActionId { get; }
        public Method HttpMethod { get; }
        public IList<MappedParameter> Mapped { get; private set; }
        public string MethodName { get; }

        public MappedParameters(int actionId, Method httpMethod, string methodName, IList<MappedParameter> mapped)
        {
            ActionId = actionId;
            HttpMethod = httpMethod;
            MethodName = methodName;
            Mapped = mapped;
        }

        public override string ToString()
            => $"MappedParameters[ActionId={ActionId}, HttpMethod={HttpMethod}, MethodName={MethodName}, Mapped={Mapped}]";
    }

    public class MappedParameter
    {
        public string Type { get; }
        public object? Value { get; }

        public MappedParameter(string type, object? value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
            => $"MappedParameter[Type={Type}, Value={Value}]";
    }

    //=====================================
    // MatchResults
    //=====================================

    public class MatchResults
    {
        public Action? Action { get; }
        public bool IsMatched { get; }
        public IList<RawPathParameter> Parameters { get; }

        public override string ToString()
            => $"MatchResults[Action={Action}, Matched={IsMatched}, Parameters={Parameters}]";

        public MatchResults(
            Action? action,
            RunningMatchSegments? running,
            IList<string> parameterNames,
            string path)
        {
            Action = action;
            Parameters = new List<RawPathParameter>(parameterNames.Count);

            if (running == null)
            {
                IsMatched = false;
            }
            else
            {
                var pathLength = 0;
                var total = running.Total;
                for (int idx = 0, parameterIndex = 0; idx < total; ++idx)
                {
                    var segment = running.MatchSegment(idx);

                    if (segment.IsPathParameter)
                    {
                        var pathStartIndex = segment.PathStartIndex;
                        var pathEndIndex = running.NextSegmentStartIndex(idx, path.Length);
                        if (pathStartIndex >= pathEndIndex)
                        {
                            IsMatched = false;
                            return;
                        }
                        var value = path.Substring(pathStartIndex, pathEndIndex - pathStartIndex);
                        if (value.IndexOf("/", StringComparison.InvariantCulture) >= 0 && !"/".Equals(path.Substring(pathEndIndex - 1)))
                        {
                            IsMatched = false;
                            return;
                        }
                        pathLength += value.Length;
                        Parameters.Add(new RawPathParameter(parameterNames[parameterIndex++], value));
                    }
                    else
                    {
                        pathLength += action!._matchable.PathSegment(idx).Value.Length;
                    }
                }
                IsMatched = pathLength == path.Length;
            }
        }

        public int ParameterCount => Parameters.Count;
    }

    //=====================================
    // BodyTypedParameter
    //=====================================

    public class BodyTypedParameter
    {
        public string Name { get; }
        public Type Type { get; }

        public BodyTypedParameter(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
            => $"BodyTypedParameter[MimeType={Type}, Name={Name}]";
    }

    public class RawPathParameter
    {
        public string Name { get; }
        public string Value { get; }

        public RawPathParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static RawPathParameter? Named(string name, IList<RawPathParameter> parameters)
            => parameters.FirstOrDefault(p => string.Equals(name, p.Name));

        public override string ToString()
            => $"RawPathParameter[Name={Name}, Value={Value}]";
    }

    //=====================================
    // MethodParameter
    //=====================================

    public class MethodParameter
    {
        public Type? BodyType { get; }
        public string Name { get; }
        public string Type { get; }

        public MethodParameter(string type, string name, Type? bodyClass)
        {
            Type = type;
            Name = name;
            BodyType = bodyClass;
        }

        public MethodParameter(string type, string name)
            : this(type, name, null)
        {
        }

        public bool IsBody => BodyType != null;

        public override string ToString()
            => $"MethodParameter[MimeType={Type}, Name={Name}]";
    }

    //=====================================
    // RunningMatchSegments
    //=====================================

    public class RunningMatchSegments
    {
        private readonly IList<MatchSegment> _matchSegments;

        public RunningMatchSegments(int totalSegments)
        {
            _matchSegments = new List<MatchSegment>(totalSegments);
        }

        public void KeepParameterSegment(int pathStartIndex)
            => _matchSegments.Add(new MatchSegment(true, pathStartIndex));

        public void KeepPathSegment(int pathStartIndex, int pathEndIndex)
            => _matchSegments.Add(new MatchSegment(false, pathStartIndex));

        public int NextSegmentStartIndex(int index, int maxIndex)
            => index < Total - 1 ? MatchSegment(index + 1).PathStartIndex : maxIndex;

        public MatchSegment MatchSegment(int index)
            => _matchSegments[index];

        public int Total => _matchSegments.Count;

        public override string ToString()
            => $"RunningMatchSegments[MatchSegments={_matchSegments}]";
    }

    //=====================================
    // MatchSegment
    //=====================================

    public class MatchSegment
    {
        public bool IsPathParameter { get; }
        public int PathStartIndex { get; }

        public MatchSegment(bool isPathParameter, int pathStartIndex)
        {
            IsPathParameter = isPathParameter;
            PathStartIndex = pathStartIndex;
        }

        public override string ToString()
            => $"MatchSegment[IsPathParameter={IsPathParameter}, PathStartIndex={PathStartIndex}]";
    }

    //=====================================
    // Matchable
    //=====================================

    public class Matchable
    {
        internal IList<PathSegment> PathSegments { get; }

        public override string ToString()
            => $"Matchable[PathSegments={PathSegments}]";

        public override int GetHashCode()
            => 31 * PathSegments.GetHashCode();

        public Matchable(string? uri)
        {
            PathSegments = Segmented(uri);
        }

        public PathSegment PathSegment(int index)
            => PathSegments[index];

        public int TotalSegments => PathSegments.Count;

        private IList<PathSegment> Segmented(string? uri)
        {
            var segments = new List<PathSegment>();
            var start = uri;
            while (true)
            {
                var openBrace = start?.IndexOf("{");
                if (openBrace.HasValue && openBrace >= 0)
                {
                    var closeBrace = start?.IndexOf("}", openBrace.Value);
                    if (closeBrace > openBrace)
                    {
                        var segment = start?.Substring(0, openBrace.Value);
                        segments.Add(new PathSegment(segment!, false));
                        var parameter = start?.Substring(openBrace.Value + 1, closeBrace.Value - (openBrace.Value + 1));
                        segments.Add(new PathSegment(parameter!, true));
                        start = start?.Substring(closeBrace.Value + 1);
                        if (string.IsNullOrEmpty(start))
                        {
                            break;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"URI has unbalanced brace: {uri}");
                    }
                }
                else
                {
                    segments.Add(new PathSegment(start!, false));
                    break;
                }
            }

            return segments;
        }
    }

    //=====================================
    // PathSegment
    //=====================================

    public class PathSegment
    {
        public string Value { get; }
        public bool IsPathParameter { get; }

        public PathSegment(string value, bool isPathParameter)
        {
            Value = value;
            IsPathParameter = isPathParameter;
        }

        public int LastIndexOf(int startIndex) => startIndex + Value.Length;

        public override string ToString()
            => $"PathSegment[IsPathParameter={IsPathParameter}, Value={Value}]";
    }

    //=====================================
    // ToSpec
    //=====================================

    public class ToSpec
    {
        public string MethodName { get; }
            
        public IList<MethodParameter> Parameters { get; }

        public string Signature
        {
            get
            {
                var builder = new StringBuilder();

                builder
                    .Append(MethodName)
                    .Append("(")
                    .Append(CommaSeparatedParameters())
                    .Append(")");

                return builder.ToString();
            }
        }

        public ToSpec(string? to)
        {
            var parsed = Parse(to);
            MethodName = FirstLetterToUpperCase(parsed.Item1);
            Parameters = parsed.Item2;
        }

        public override string ToString()
            => $"ToSpec[MethodName={MethodName}, Parameters={Parameters}]";

        public MethodParameter? Body
            => Parameters.FirstOrDefault(p => p.IsBody);

        public MethodParameter? ParameterOf(string name)
            => Parameters.FirstOrDefault(p => string.Equals(name, p.Name));

        private string CommaSeparatedParameters()
        {
            var builder = new StringBuilder();

            var separator = "";

            foreach (var parameter in Parameters)
            {
                builder
                    .Append(separator)
                    .Append(parameter.Type)
                    .Append(" ")
                    .Append(parameter.Name);

                separator = ", ";
            }

            return builder.ToString();
        }

        private Tuple<string, IList<MethodParameter>> Parse(string? to)
        {
            var bad = $"Invalid to declaration: {to}";

            var openParen = to?.IndexOf("(");
            var closeParen = to?.LastIndexOf(")");

            if (!openParen.HasValue || openParen < 0 || !closeParen.HasValue || closeParen < 0)
            {
                throw new InvalidOperationException(bad);
            }

            var methodName = to?.Substring(0, openParen.Value);
            var rawParameters = to?.Substring(openParen.Value + 1, closeParen.Value - (openParen.Value + 1)).Split(',');
            var parameters = new List<MethodParameter>(rawParameters!.Length);

            foreach (var p in rawParameters)
            {
                var rawParameter = p.Trim();
                if (!string.IsNullOrEmpty(rawParameter))
                {
                    if (rawParameter.StartsWith("body:"))
                    {
                        var body = TypeAndName(rawParameter.Substring(5));
                        parameters.Add(new MethodParameter(body[0], body[1], TypeLoader.Load(QualifiedType(body[0]))));
                    }
                    else
                    {
                        var other = TypeAndName(rawParameter);
                        parameters.Add(new MethodParameter(other[0], other[1]));
                    }
                }
            }

            return new Tuple<string, IList<MethodParameter>>(methodName!, parameters);
        }

        private string QualifiedType(string possiblyUnqualifiedType)
        {
            switch (possiblyUnqualifiedType)
            {
                case "string":
                    return "string";
                case "int":
                case "Integer":
                    return "int";
                case "long":
                case "Long":
                    return "long";
                case "bool":
                case "Boolean":
                    return "bool";
                case "double":
                case "Double":
                    return "double";
                case "short":
                case "Short":
                    return "short";
                case "float":
                case "Float":
                    return "float";
                case "char":
                case "Character":
                    return "char";
                case "byte":
                case "Byte":
                    return "byte";
                default:
                    return possiblyUnqualifiedType;
            }
        }

        private string[] TypeAndName(string rawParameter)
        {
            var space = rawParameter.LastIndexOf(' ');
            if (space == -1)
            {
                throw new InvalidOperationException($"Parameter mimeType and name must be separated by space: {rawParameter}");
            }
            var typeName = new string[2];
            typeName[0] = rawParameter.Substring(0, space).Trim();
            typeName[1] = rawParameter.Substring(space + 1).Trim();
            return typeName;
        }
            
        private string FirstLetterToUpperCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("There is no first letter");

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}