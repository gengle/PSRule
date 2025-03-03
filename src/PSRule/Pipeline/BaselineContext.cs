﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Rules;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    internal interface IBindingOption
    {
        bool IgnoreCase { get; }

        FieldMap[] Field { get; }

        string[] TargetName { get; }

        string[] TargetType { get; }
    }

    internal sealed class BaselineContext
    {
        private readonly Dictionary<string, BaselineContextScope> _ModuleScope;

        private BaselineContextScope _Parameter;
        private BaselineContextScope _Explicit;
        private BaselineContextScope _Workspace;
        private BaselineContextScope _Module;
        private BindingOption _Binding;
        private RuleFilter _Filter;
        private Dictionary<string, object> _Configuration;

        internal BaselineContext()
        {
            _ModuleScope = new Dictionary<string, BaselineContextScope>();
        }

        internal enum ScopeType : byte
        {
            Parameter = 0,
            Explicit = 1,
            Workspace = 2,
            Module = 3
        }

        internal sealed class BaselineContextScope
        {
            public readonly ScopeType Type;
            public readonly string ModuleName;

            // Rule
            public string[] Include;
            public string[] Exclude;
            public Hashtable Tag;

            // Configuration
            public Dictionary<string, object> Configuration;

            // Binding
            public bool? IgnoreCase;
            public FieldMap Field;
            public string[] TargetName;
            public string[] TargetType;

            public BaselineContextScope(ScopeType type, string moduleName, IBaselineSpec option)
            {
                Type = type;
                ModuleName = moduleName;
                IgnoreCase = option.Binding?.IgnoreCase;
                Field = option.Binding?.Field;
                TargetName = option.Binding?.TargetName;
                TargetType = option.Binding?.TargetType;
                Include = option.Rule?.Include;
                Exclude = option.Rule?.Exclude;
                Tag = option.Rule?.Tag;
                Configuration = option.Configuration != null ?
                    new Dictionary<string, object>(option.Configuration, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            public BaselineContextScope(ScopeType type, string[] include, Hashtable tag)
            {
                Type = type;
                Include = include;
                Tag = tag;
                Configuration = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private sealed class BindingOption : IBindingOption, IEquatable<BindingOption>
        {
            public BindingOption(bool ignoreCase, FieldMap[] field, string[] targetName, string[] targetType)
            {
                IgnoreCase = ignoreCase;
                Field = field;
                TargetName = targetName;
                TargetType = targetType;
            }

            public bool IgnoreCase { get; }

            public FieldMap[] Field { get; }

            public string[] TargetName { get; }

            public string[] TargetType { get; }

            public override bool Equals(object obj)
            {
                return obj is BindingOption option && Equals(option);
            }

            public bool Equals(BindingOption other)
            {
                return other != null &&
                    IgnoreCase == other.IgnoreCase &&
                    Field == other.Field &&
                    TargetName == other.TargetName &&
                    TargetType == other.TargetType;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine
                {
                    int hash = 17;
                    hash = hash * 23 + (IgnoreCase ? IgnoreCase.GetHashCode() : 0);
                    hash = hash * 23 + (Field != null ? Field.GetHashCode() : 0);
                    hash = hash * 23 + (TargetName != null ? TargetName.GetHashCode() : 0);
                    hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 0);
                    return hash;
                }
            }
        }

        public void UseScope(string moduleName)
        {
            _Module = !string.IsNullOrEmpty(moduleName) && _ModuleScope.TryGetValue(moduleName, out BaselineContextScope scope) ? scope : null;
            _Binding = null;
            _Configuration = null;
            _Filter = null;
        }

        public IResourceFilter RuleFilter()
        {
            if (_Filter != null)
                return _Filter;

            string[] include = _Parameter?.Include ?? _Explicit?.Include ?? _Workspace?.Include ?? _Module?.Include;
            string[] exclude = _Explicit?.Exclude ?? _Workspace?.Exclude ?? _Module?.Exclude;
            Hashtable tag = _Parameter?.Tag ?? _Explicit?.Tag ?? _Workspace?.Tag ?? _Module?.Tag;
            return _Filter = new RuleFilter(include, tag, exclude);
        }

        public IBindingOption GetTargetBinding()
        {
            if (_Binding != null)
                return _Binding;

            bool ignoreCase = _Explicit?.IgnoreCase ?? _Workspace?.IgnoreCase ?? _Module?.IgnoreCase ?? PSRule.Configuration.BindingOption.Default.IgnoreCase.Value;
            FieldMap[] field = new FieldMap[] { _Explicit?.Field, _Workspace?.Field, _Module?.Field };
            string[] targetName = _Explicit?.TargetName ?? _Workspace?.TargetName ?? _Module?.TargetName;
            string[] targetType = _Explicit?.TargetType ?? _Workspace?.TargetType ?? _Module?.TargetType;
            return _Binding = new BindingOption(ignoreCase, field, targetName, targetType);
        }

        public Dictionary<string, object> GetConfiguration()
        {
            if (_Configuration != null)
                return _Configuration;

            return _Configuration = AddConfiguration();
        }

        internal void Add(BaselineContextScope scope)
        {
            if (scope.Type == ScopeType.Module)
                _ModuleScope.Add(scope.ModuleName, scope);
            else if (scope.Type == ScopeType.Explicit)
                _Explicit = scope;
            else if (scope.Type == ScopeType.Workspace)
                _Workspace = scope;
            else if (scope.Type == ScopeType.Parameter)
                _Parameter = scope;
        }

        private Dictionary<string, object> AddConfiguration()
        {
            var result = new Dictionary<string, object>();
            if (_Explicit != null && _Explicit.Configuration.Count > 0)
            {
                foreach (var c in _Explicit.Configuration)
                {
                    result.Add(c.Key, c.Value);
                }
            }
            if (_Workspace != null && _Workspace.Configuration.Count > 0)
            {
                foreach (var c in _Workspace.Configuration)
                {
                    if (!result.ContainsKey(c.Key))
                        result.Add(c.Key, c.Value);
                }
            }
            if (_Module != null && _Module.Configuration.Count > 0)
            {
                foreach (var c in _Module.Configuration)
                {
                    if (!result.ContainsKey(c.Key))
                        result.Add(c.Key, c.Value);
                }
            }
            return result;
        }
    }
}
