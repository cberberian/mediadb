using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Lucene.Net.Support;

namespace MediaDatabase.Utils
{
    public class TokenTemplate<T> where T : new()
    {
        private readonly Type               _type            = typeof(T);
        private readonly string             _tokenRegex      = @"\][^\]]*\[";
        private readonly string             _fieldNamesRegex = @"\[[^\]]*\]";
        private readonly List<PropertyInfo> _propList       = new EquatableList<PropertyInfo>();
        private readonly List<FieldInfo>    _fieldList         = new List<FieldInfo>();
        private readonly string             _template;
        private string[]                    _tokens;
        private string[]                    _fieldNames;
        

        public TokenTemplate(string template)
        {
            _template = template;
            DeriveFieldList(template);
        }

        private void DeriveFieldList(string template) 
        {
            
            _tokens = Regex.Matches(template, _tokenRegex)
                            .Cast<Match>()
                            .Select(m => m.Value.TrimStart(']').TrimEnd('['))
                            .Where(x=> !string.IsNullOrEmpty(x))
                            .ToArray();

            _fieldNames = Regex.Matches(template, _fieldNamesRegex)
                            .Cast<Match>()
                            .Select(m => m.Value.TrimStart('[').TrimEnd(']'))
                            .ToArray();

            foreach (var name in _fieldNames)
            {
                if (name == "IGNORE") continue;
                if (name == "INDEX") continue;

                var prop = _type.GetProperty(name);

                if (prop != null)
                {
                    _propList.Add(prop);
                    continue;
                }

                var field = _type.GetField(name);
                
                if (field != null)
                {
                    _fieldList.Add(field);
                }
            }
        }

        public T Tokenize(string input)
        {
            var ret        = new T();
            var currentStr = input;

            for (var index = 0; index < _fieldNames.Length; index++)
            {
                var tokenIndex = index < _tokens.Length ? 
                                 currentStr.IndexOf(_tokens[index], 0, StringComparison.Ordinal) : 
                                 currentStr.Length;
                if (tokenIndex < 0)
                    return ret;
                 
                var val = currentStr.Substring(0, tokenIndex);

                _propList.FirstOrDefault(x=>x.Name== _fieldNames[index])?
                            .SetValue(ret, val);

                _fieldList.FirstOrDefault(x=>x.Name == _fieldNames[index])?
                          .SetValue(ret, val);

                if (index < _tokens.Length)
                    currentStr = currentStr.Substring(tokenIndex + _tokens[index].Length);
            }

//            for (var index = 0; index < values.Length; index++)
//            {
//                if (index >= _fieldNames.Length)
//                {
//                    return ret;
//                }
//
//                _propList.FirstOrDefault(x=>x.Name== _fieldNames[index])?
//                          .SetValue(ret, values[index]);
//                _fieldList.FirstOrDefault(x=>x.Name == _fieldNames[index])?
//                          .SetValue(ret, values[index]);
//            }

                return ret;
        }

        public string DeTokenize(T input)
        {
            var ret = _template;

            foreach (var propertyInfo in _propList)
            {
                var val = propertyInfo.GetValue(input);
                ret     = ret.Replace(propertyInfo.Name, val?.ToString() ?? string.Empty);
            }

            foreach (var propertyInfo in _fieldList)
            {
                var val = propertyInfo.GetValue(input);
                var oldValue = $"[{propertyInfo.Name}]";
                if (val != null)
                    ret = ret.Replace(oldValue, val.ToString());
            }

            return ret;
        }
    }
}