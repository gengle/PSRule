// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BenchmarkDotNet.Attributes;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace PSRule.Benchmark
{
    public sealed class TargetObject
    {
        public TargetObject(string name, string message, string value)
        {
            Name = name;
            Message = message;
            Value = value;
        }

        public string Name { get; private set; }

        public string Message { get; private set; }

        public string Value { get; private set; }
    }

    /// <summary>
    /// Define a set of benchmarks for performance testing PSRule internals.
    /// </summary>
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class PSRule
    {
        private PSObject[] _TargetObject;
        private IPipeline _GetPipeline;
        private IPipeline _InvokePipeline;
        private IPipeline _InvokeIfPipeline;
        private IPipeline _InvokeTypePipeline;
        private IPipeline _InvokeSummaryPipeline;
        private IPipeline _InvokeWithinPipeline;
        private IPipeline _InvokeWithinBulkPipeline;
        private IPipeline _InvokeWithinLikePipeline;

        [GlobalSetup]
        public void Prepare()
        {
            PrepareGetPipeline();
            PrepareInvokePipeline();
            PrepareInvokeIfPipeline();
            PrepareInvokeTypePipeline();
            PrepareInvokeSummaryPipeline();
            PrepareInvokeWithinPipeline();
            PrepareInvokeWithinBulkPipeline();
            PrepareInvokeWithinLikePipeline();
            PrepareTargetObjects();
        }

        private void PrepareGetPipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "Benchmark" };
            var builder = PipelineBuilder.Get(GetSource(), option);
            _GetPipeline = builder.Build();
        }

        private void PrepareInvokePipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "Benchmark" };
            var builder = PipelineBuilder.Invoke(GetSource(), option);
            _InvokePipeline = builder.Build();
        }

        private void PrepareInvokeIfPipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "BenchmarkIf" };
            var builder = PipelineBuilder.Invoke(GetSource(), option);
            _InvokeIfPipeline = builder.Build();
        }

        private void PrepareInvokeTypePipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "BenchmarkType" };
            var builder = PipelineBuilder.Invoke(GetSource(), option);
            _InvokeTypePipeline = builder.Build();
        }

        private void PrepareInvokeSummaryPipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "Benchmark" };
            option.Output.As = ResultFormat.Summary;
            var builder = PipelineBuilder.Invoke(GetSource(), option);
            _InvokeSummaryPipeline = builder.Build();
        }

        private void PrepareInvokeWithinPipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "BenchmarkWithin" };
            var builder = PipelineBuilder.Invoke(GetWithinSource(), option);
            _InvokeWithinPipeline = builder.Build();
        }

        private void PrepareInvokeWithinBulkPipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "BenchmarkWithinBulk" };
            var builder = PipelineBuilder.Invoke(GetWithinSource(), option);
            _InvokeWithinBulkPipeline = builder.Build();
        }

        private void PrepareInvokeWithinLikePipeline()
        {
            var option = new PSRuleOption();
            option.Rule.Include = new string[] { "BenchmarkWithinLike" };
            var builder = PipelineBuilder.Invoke(GetWithinSource(), option);
            _InvokeWithinLikePipeline = builder.Build();
        }

        private Source[] GetSource()
        {
            var builder = new RuleSourceBuilder();
            builder.Directory(GetSourcePath("Benchmark.Rule.ps1"));
            return builder.Build();
        }

        private Source[] GetWithinSource()
        {
            var builder = new RuleSourceBuilder();
            builder.Directory(GetSourcePath("Benchmark.Within.Rule.ps1"));
            return builder.Build();
        }

        private string GetSourcePath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), fileName);
        }

        private void PrepareTargetObjects()
        {
            var r = new Random();
            var randomBuffer = new byte[40];
            var targetObjects = new List<PSObject>();
            while (targetObjects.Count < 1000)
            {
                r.NextBytes(randomBuffer);
                var value = (targetObjects.Count & 1) == 1 ? "Microsoft.Compute/virtualMachines" : "Microsoft.Sql/servers/databases";
                var o = new TargetObject(name: targetObjects.Count.ToString(), message: Convert.ToBase64String(randomBuffer), value: value);
                targetObjects.Add(PSObject.AsPSObject(o));
            }

            _TargetObject = targetObjects.ToArray();
        }

        [Benchmark]
        public void Invoke() => RunPipelineTargets(_InvokePipeline);

        [Benchmark]
        public void InvokeIf() => RunPipelineTargets(_InvokeIfPipeline);

        [Benchmark]
        public void InvokeType() => RunPipelineTargets(_InvokeTypePipeline);

        [Benchmark]
        public void InvokeSummary() => RunPipelineTargets(_InvokeSummaryPipeline);

        [Benchmark]
        public void Get() => RunPipelineNull(_GetPipeline);

        [Benchmark]
        public void Within() => RunPipelineTargets(_InvokeWithinPipeline);

        [Benchmark]
        public void WithinBulk() => RunPipelineTargets(_InvokeWithinBulkPipeline);

        [Benchmark]
        public void WithinLike() => RunPipelineTargets(_InvokeWithinLikePipeline);

        [Benchmark]
        public void DefaultTargetNameBinding()
        {
            foreach (var targetObject in _TargetObject)
            {
                PipelineHookActions.BindTargetName(null, false, targetObject);
            }
        }

        [Benchmark]
        public void CustomTargetNameBinding()
        {
            foreach (var targetObject in _TargetObject)
            {
                PipelineHookActions.BindTargetName(
                    propertyNames: new string[] { "TargetName", "Name" },
                    caseSensitive: true,
                    targetObject: targetObject
                );
            }
        }

        [Benchmark]
        public void NestedTargetNameBinding()
        {
            foreach (var targetObject in _TargetObject)
            {
                PipelineHookActions.BindTargetName(
                    propertyNames: new string[] { "TargetName", "Name" },
                    caseSensitive: true,
                    targetObject: targetObject
                );
            }
        }

        private void RunPipelineNull(IPipeline pipeline)
        {
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();
        }

        private void RunPipelineTargets(IPipeline pipeline)
        {
            pipeline.Begin();

            for (var i = 0; i < _TargetObject.Length; i++)
                pipeline.Process(_TargetObject[i]);

            pipeline.End();
        }
    }
}
