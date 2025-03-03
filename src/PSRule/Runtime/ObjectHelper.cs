﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Management.Automation;
using System.Reflection;

namespace PSRule.Runtime
{
    internal static class ObjectHelper
    {
        private sealed class NameTokenStream
        {
            private const char Separator = '.';
            private const char Quoted = '\'';
            private const char OpenIndex = '[';
            private const char CloseIndex = ']';

            private readonly string Name;
            private readonly int Last;

            private bool inQuote = false;
            private bool inIndex = false;

            public int Position = -1;
            public char Current;

            public NameTokenStream(string name)
            {
                Name = name;
                Last = Name.Length - 1;
            }

            /// <summary>
            /// Find the start of the sequence.
            /// </summary>
            /// <returns>Return true when more characters follow.</returns>
            public bool Next()
            {
                if (Position < Last)
                {
                    Position++;
                    if (Name[Position] == Separator)
                    {
                        Position++;
                    }
                    else if (Name[Position] == Quoted)
                    {
                        Position++;
                        inQuote = true;
                    }

                    Current = Name[Position];
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Find the end of the sequence and return the index.
            /// </summary>
            /// <returns>The index of the sequence end.</returns>
            public int IndexOf(out NameTokenType tokenType)
            {
                tokenType = NameTokenType.Field;

                while (Position < Last)
                {
                    Position++;
                    Current = Name[Position];

                    if (inQuote)
                    {
                        if (Current == Quoted)
                        {
                            inQuote = false;
                            return Position - 1;
                        }
                    }
                    else if (Current == Separator)
                    {
                        return Position - 1;
                    }
                    else if (inIndex)
                    {
                        if (Current == CloseIndex)
                        {
                            tokenType = NameTokenType.Index;
                            inIndex = false;
                            return Position - 1;
                        }
                    }
                    else if (Current == OpenIndex)
                    {
                        // Next token will be an Index
                        inIndex = true;

                        // Return end of token
                        return Position - 1;
                    }
                }
                return Position;
            }

            public NameToken Get()
            {
                var token = new NameToken();
                NameToken result = token;

                while (Next())
                {
                    var start = Position;
                    if (start > 0)
                    {
                        token.Next = new NameToken();
                        token = token.Next;
                    }

                    // Jump to the next separator or end
                    var end = IndexOf(out NameTokenType tokenType);
                    token.Type = tokenType;

                    if (tokenType == NameTokenType.Field)
                    {
                        token.Name = Name.Substring(start, end - start + 1);
                    }
                    else
                    {
                        token.Index = int.Parse(Name.Substring(start, end - start + 1));
                    }
                }
                return result;
            }
        }

        public static bool GetField(IBindingContext bindingContext, object targetObject, string name, bool caseSensitive, out object value)
        {
            // Try to load nameToken from cache
            if (bindingContext == null || !bindingContext.GetNameToken(expression: name, nameToken: out NameToken nameToken))
            {
                nameToken = GetNameToken(expression: name);
                if (bindingContext != null)
                    bindingContext.CacheNameToken(expression: name, nameToken: nameToken);
            }
            return GetField(targetObject: targetObject, token: nameToken, caseSensitive: caseSensitive, value: out value);
        }

        private static bool GetField(object targetObject, NameToken token, bool caseSensitive, out object value)
        {
            value = null;
            var baseObject = GetBaseObject(targetObject);
            if (baseObject == null)
                return false;

            var baseType = baseObject.GetType();
            object field = null;
            bool foundField = false;

            // Handle dictionaries and hashtables
            if (token.Type == NameTokenType.Field && baseObject is IDictionary dictionary)
            {
                if (TryDictionary(dictionary, token.Name, caseSensitive, out field))
                    foundField = true;
            }
            // Handle PSObjects
            else if (token.Type == NameTokenType.Field && targetObject is PSObject pso)
            {
                if (TryPropertyValue(pso, token.Name, caseSensitive, out field))
                    foundField = true;
            }
            // Handle all other CLR types
            else if (token.Type == NameTokenType.Field)
            {
                if (TryPropertyValue(targetObject, token.Name, baseType, caseSensitive, out field) || TryFieldValue(targetObject, token.Name, baseType, caseSensitive, out field))
                    foundField = true;
            }
            // Handle Index tokens
            else if (baseType.IsArray && baseObject is Array array && token.Index < array.Length)
            {
                field = array.GetValue(token.Index);
                foundField = true;
            }

            if (foundField)
            {
                if (token.Next == null)
                {
                    value = field;
                    return true;
                }
                else
                {
                    return GetField(targetObject: field, token: token.Next, caseSensitive: caseSensitive, value: out value);
                }
            }
            return false;
        }

        private static bool TryDictionary(IDictionary dictionary, string key, bool caseSensitive, out object value)
        {
            value = null;
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            foreach (var k in dictionary.Keys)
            {
                if (comparer.Equals(key, k))
                {
                    value = dictionary[k];
                    return true;
                }
            }
            return false;
        }

        private static bool TryPropertyValue(object targetObject, string propertyName, Type baseType, bool caseSensitive, out object value)
        {
            value = null;
            var bindingFlags = caseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase;
            var propertyInfo = baseType.GetProperty(propertyName, bindingAttr: bindingFlags | BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null)
                return false;

            value = propertyInfo.GetValue(targetObject);
            return true;
        }

        private static bool TryPropertyValue(PSObject targetObject, string propertyName, bool caseSensitive, out object value)
        {
            value = null;
            var p = targetObject.Properties[propertyName];
            if (p == null)
                return false;

            if (caseSensitive && !StringComparer.Ordinal.Equals(p.Name, propertyName))
                return false;

            value = p.Value;
            return true;
        }

        private static bool TryFieldValue(object targetObject, string fieldName, Type baseType, bool caseSensitive, out object value)
        {
            value = null;
            var bindingFlags = caseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase;
            var fieldInfo = baseType.GetField(fieldName, bindingAttr: bindingFlags | BindingFlags.Instance | BindingFlags.Public);

            if (fieldInfo == null)
                return false;

            value = fieldInfo.GetValue(targetObject);
            return true;
        }

        private static NameToken GetNameToken(string expression)
        {
            var stream = new NameTokenStream(expression);
            return stream.Get();
        }

        private static object GetBaseObject(object value)
        {
            if (value == null)
                return null;

            if (value is PSObject ovalue)
            {
                var baseObject = ovalue.BaseObject;
                if (baseObject != null)
                    return baseObject;
            }
            return value;
        }
    }
}
