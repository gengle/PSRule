﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace PSRule.Rules
{
    public delegate bool RulePrecondition();

    public delegate RuleConditionResult RuleCondition();

    /// <summary>
    /// Define an instance of a rule block. Each rule block has a unique id.
    /// </summary>
    [DebuggerDisplay("{RuleId} @{SourcePath}")]
    public sealed class RuleBlock : ILanguageBlock, IDependencyTarget, IDisposable
    {
        internal RuleBlock(SourceFile source, string ruleName, RuleHelpInfo info, PowerShell condition, TagSet tag, string[] dependsOn, Hashtable configuration)
        {
            Source = source;
            RuleName = ruleName;

            var scriptFileName = Path.GetFileName(Source.Path);

            // Get fully qualified Id, either RuleName or Module\RuleName
            RuleId = RuleHelper.ExpandRuleName(ruleName: ruleName, scriptFileName: scriptFileName, moduleName: Source.ModuleName);

            Info = info;
            Condition = condition;
            Tag = tag;
            DependsOn = dependsOn;
            Configuration = configuration;
        }

        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        public readonly string RuleId;

        /// <summary>
        /// The name of the rule.
        /// </summary>
        public readonly string RuleName;

        /// <summary>
        /// The body of the rule definition where conditions are provided that either pass or fail the rule.
        /// </summary>
        public readonly PowerShell Condition;

        /// <summary>
        /// Other rules that must completed successfully before calling this rule.
        /// </summary>
        public readonly string[] DependsOn;

        /// <summary>
        /// Tags assigned to block. Tags are additional metadata used to select rules to execute and identify results.
        /// </summary>
        public readonly TagSet Tag;

        /// <summary>
        /// Configuration defaults for the rule definition.
        /// </summary>
        /// <remarks>
        /// These defaults are used when the value does not exist in the baseline configuration.
        /// </remarks>
        public readonly Hashtable Configuration;

        public readonly RuleHelpInfo Info;

        public readonly SourceFile Source;

        string ILanguageBlock.SourcePath => Source.Path;

        string ILanguageBlock.Module => Source.ModuleName;

        string IDependencyTarget.RuleId => RuleId;

        string[] IDependencyTarget.DependsOn => DependsOn;

        #region IDisposable

        public void Dispose()
        {
            Condition.Dispose();
        }

        #endregion IDisposable
    }
}
