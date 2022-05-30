using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sider.Services
{
    internal static class Preprocessors
    {

        internal static string InterpolateString(this string value, IDictionary<string, object> variables)
        {
            value = value.Trim();

            var matches = Regex.Matches(value, @"\${(.*?)}");

            if (matches.Any())
            {
                var scriptBoulder = new StringBuilder();
                var lastIndex = 0;

                for (var i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    if (match.Success)
                    {
                        var group = match.Groups[0];
                        var variableName = match.Groups[1].Captures[0].Value;

                        if (variables.ContainsKey(variableName))
                        {
                            if (group.Index - lastIndex > 0)
                            {
                                scriptBoulder.Append(value.AsSpan(lastIndex, group.Index - lastIndex));
                            }

                            scriptBoulder.Append(variables[variableName]);
                            lastIndex = group.Index + group.Length;
                        }
                        else if (variableName == "nbsp")
                        {
                            if (group.Index - lastIndex > 0)
                            {
                                // ?
                                scriptBoulder.Append(variables[value.Substring(lastIndex, group.Index - lastIndex)]);
                            }

                            scriptBoulder.Append('\u0160');
                            lastIndex = group.Index + group.Length;
                        }
                    }
                }

                if (lastIndex < value.Length)
                {
                    scriptBoulder.Append(value.AsSpan(lastIndex, value.Length - lastIndex));
                }

                return scriptBoulder.ToString();
            }
            else
            {
                return value;
            }
        }

        internal static (string script, List<object> argv) InterpolateScript(this string value, IDictionary<string, object> variables)
        {
            value = value.Trim();

            var argl = 0; // length of arguments
            var argv = new List<object>();
            var matches = Regex.Matches((string)value, @"\${(.*?)}");

            if (matches.Any())
            {
                var scriptBoulder = new StringBuilder();
                var variablesUsed = new Dictionary<string, int>();

                var lastIndex = 0;
                for (var i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];

                    if (match.Success)
                    {
                        var group = match.Groups[0];
                        var variableName = match.Groups[1].Captures[0].Value;

                        if (variables.ContainsKey(variableName))
                        {
                            if (group.Index - lastIndex > 0)
                            {
                                scriptBoulder.Append(value.AsSpan(lastIndex, group.Index - lastIndex));
                            }

                            if (!variablesUsed.ContainsKey(variableName))
                            {
                                variablesUsed[variableName] = argl;
                                argv.Add(variables[variableName]);
                                argl++;
                            }

                            scriptBoulder.Append($"arguments[{variablesUsed[variableName]}]");
                            lastIndex = group.Index + group.Length;
                        }
                    }
                }

                if (lastIndex < value.Length)
                {
                    scriptBoulder.Append(value.AsSpan(lastIndex, value.Length - lastIndex));
                }

                return (scriptBoulder.ToString(), argv);
            }
            else
            {
                return (value, argv);
            }
        }
    }
}