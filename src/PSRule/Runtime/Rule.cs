﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System;
using System.Management.Automation;

namespace PSRule.Runtime
{
    /// <summary>
    /// A set of rule properties that are exposed at runtime through the $Rule variable.
    /// </summary>
    public sealed class Rule
    {
        public string RuleName
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.RuleName;
            }
        }

        public string RuleId
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.RuleId;
            }
        }

        [Obsolete("Use property on $PSRule instead")]
        public PSObject TargetObject
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetObject;
            }
        }

        [Obsolete("Use property on $PSRule instead")]
        public string TargetName
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetName;
            }
        }

        [Obsolete("Use property on $PSRule instead")]
        public string TargetType
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetType;
            }
        }
    }
}
