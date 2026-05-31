using System.Text;
using System.Text.RegularExpressions;

namespace PhysExtractor.src
{
    public struct Vector3
    {
        public float x, y, z;
    }

    public struct Triangle
    {
        public Vector3 p1, p2, p3;
    }

    public struct Edge
    {
        public byte next, twin, origin, face;
    }

    public class KV3Parser
    {
        private readonly string _data;
        private readonly Dictionary<string, object> _parsedData;

        public KV3Parser(string data)
        {
            _data = data;
            _parsedData = ParseKV3(data);
        }

        public string GetValue(string path)
        {
            try
            {
                var parts = ParsePath(path);
                object current = _parsedData;

                foreach (var part in parts)
                {
                    if (part == null || part.Key == null)
                        continue;

                    if (part.IsArray)
                    {
                        if (current is Dictionary<string, object> dict && dict.TryGetValue(part.Key, out object? arrayValue))
                        {
                            if (arrayValue is List<object> list && part.Index < list.Count)
                            {
                                current = list[part.Index];
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        if (current is Dictionary<string, object> dict && dict.TryGetValue(part.Key ?? "", out object? value))
                        {
                            current = value;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }

                var result = current?.ToString() ?? "";

                if (result.StartsWith("\"") && result.EndsWith("\""))
                {
                    result = result.Substring(1, result.Length - 2);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing path '{path}': {ex.Message}");
                return "";
            }
        }

        private class PathPart
        {
            public string? Key { get; set; }
            public bool IsArray { get; set; }
            public int Index { get; set; }
        }

        private List<PathPart> ParsePath(string path)
        {
            var parts = new List<PathPart>();
            var segments = path.Split('.');

            foreach (var segment in segments)
            {
                var match = Regex.Match(segment, @"([^[]+)\[(\d+)\]");
                if (match.Success)
                {
                    parts.Add(new PathPart
                    {
                        Key = match.Groups[1].Value,
                        IsArray = true,
                        Index = int.Parse(match.Groups[2].Value)
                    });
                }
                else
                {
                    parts.Add(new PathPart
                    {
                        Key = segment,
                        IsArray = false
                    });
                }
            }

            return parts;
        }

        private Dictionary<string, object> ParseKV3(string content)
        {
            // Remove comments and metadata header
            var cleanContent = RemoveCommentsAndMetadata(content);

            // Find the main object
            var mainObjectStart = cleanContent.IndexOf('{');
            if (mainObjectStart == -1)
                return new Dictionary<string, object>();

            var tokens = Tokenize(cleanContent.Substring(mainObjectStart));
            return ParseObject(tokens, 0).Item1;
        }

        private string RemoveCommentsAndMetadata(string content)
        {
            var lines = content.Split('\n');
            var result = new StringBuilder();
            bool inMainContent = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Start of main content
                if (trimmed.StartsWith("{") && !inMainContent)
                {
                    inMainContent = true;
                    result.AppendLine(line);
                    continue;
                }

                if (inMainContent)
                {
                    // Remove single line comments
                    if (trimmed.StartsWith("//"))
                        continue;

                    // Remove inline comments
                    var commentIndex = line.IndexOf("//");
                    if (commentIndex >= 0)
                    {
                        result.AppendLine(line.Substring(0, commentIndex));
                    }
                    else
                    {
                        result.AppendLine(line);
                    }
                }
            }

            return result.ToString();
        }

        private List<string> Tokenize(string content)
        {
            var tokens = new List<string>();
            var current = new StringBuilder();
            bool inString = false;
            bool inByteArray = false;
            int i = 0;

            while (i < content.Length)
            {
                char c = content[i];

                if (inString)
                {
                    current.Append(c);
                    if (c == '"' && (i == 0 || content[i - 1] != '\\'))
                    {
                        inString = false;
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else if (inByteArray)
                {
                    current.Append(c);
                    if (c == ']')
                    {
                        inByteArray = false;
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    switch (c)
                    {
                        case '"':
                            if (current.Length > 0)
                            {
                                tokens.Add(current.ToString());
                                current.Clear();
                            }
                            current.Append(c);
                            inString = true;
                            break;

                        case '#':
                            if (i + 1 < content.Length && content[i + 1] == '[')
                            {
                                if (current.Length > 0)
                                {
                                    tokens.Add(current.ToString());
                                    current.Clear();
                                }
                                current.Append(c);
                                inByteArray = true;
                            }
                            else
                            {
                                current.Append(c);
                            }
                            break;

                        case '{':
                        case '}':
                        case '[':
                        case ']':
                        case '=':
                        case ',':
                            if (current.Length > 0)
                            {
                                tokens.Add(current.ToString().Trim());
                                current.Clear();
                            }
                            tokens.Add(c.ToString());
                            break;

                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            if (current.Length > 0)
                            {
                                var token = current.ToString().Trim();
                                if (!string.IsNullOrEmpty(token))
                                {
                                    tokens.Add(token);
                                }
                                current.Clear();
                            }
                            break;

                        default:
                            current.Append(c);
                            break;
                    }
                }
                i++;
            }

            if (current.Length > 0)
            {
                var token = current.ToString().Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    tokens.Add(token);
                }
            }

            return tokens.Where(t => !string.IsNullOrEmpty(t)).ToList();
        }

        private (Dictionary<string, object>, int) ParseObject(List<string> tokens, int index)
        {
            var obj = new Dictionary<string, object>();

            if (index >= tokens.Count || tokens[index] != "{")
                return (obj, index);

            index++; // Skip opening brace

            while (index < tokens.Count && tokens[index] != "}")
            {
                if (tokens[index] == ",")
                {
                    index++;
                    continue;
                }

                // Parse key
                var key = tokens[index].Trim('"');
                index++;

                // Skip equals sign
                if (index < tokens.Count && tokens[index] == "=")
                    index++;

                // Parse value
                var (value, newIndex) = ParseValue(tokens, index);
                if (value == null)
                    continue;

                obj[key] = value;
                index = newIndex;
            }

            if (index < tokens.Count && tokens[index] == "}")
                index++;

            return (obj, index);
        }

        private (List<object>, int) ParseArray(List<string> tokens, int index)
        {
            var array = new List<object>();

            if (index >= tokens.Count || tokens[index] != "[")
                return (array, index);

            index++; // Skip opening bracket

            while (index < tokens.Count && tokens[index] != "]")
            {
                if (tokens[index] == ",")
                {
                    index++;
                    continue;
                }

                var (value, newIndex) = ParseValue(tokens, index);
                if (value == null)
                    continue;

                array.Add(value);
                index = newIndex;
            }

            if (index < tokens.Count && tokens[index] == "]")
                index++;

            return (array, index);
        }

        private (object?, int) ParseValue(List<string> tokens, int index)
        {
            if (index >= tokens.Count)
                return (null, index);

            var token = tokens[index];

            if (token == "{")
            {
                return ParseObject(tokens, index);
            }
            else if (token == "[")
            {
                return ParseArray(tokens, index);
            }
            else if (token.StartsWith("#["))
            {
                // Byte array - extract the content between #[ and ]
                var byteData = token.Substring(2, token.Length - 3);
                return (byteData, index + 1);
            }
            else if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                // String value
                return (token.Substring(1, token.Length - 2), index + 1);
            }
            else
            {
                // Numeric or unquoted value
                return (token, index + 1);
            }
        }
    }
}