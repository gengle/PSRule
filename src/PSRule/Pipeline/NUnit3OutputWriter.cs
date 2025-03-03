﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSRule.Pipeline
{
    internal sealed class NUnit3OutputWriter : PipelineWriter
    {
        private readonly StringBuilder _Builder;
        private readonly List<InvokeResult> _Result;

        internal NUnit3OutputWriter(WriteOutput output)
            : base(output)
        {
            _Builder = new StringBuilder();
            _Result = new List<InvokeResult>();
        }

        public override void Write(object o, bool enumerate)
        {
            if (!(o is InvokeResult result))
                return;

            _Result.Add(result);
        }

        public override void End()
        {
            base.Write(Serialize(_Result.ToArray()), false);
        }

        private string Serialize(IEnumerable<InvokeResult> o)
        {
            _Builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");

            float time = o.Sum(r => r.Time);
            var total = o.Sum(r => r.Total);
            var error = o.Sum(r => r.Error);
            var fail = o.Sum(r => r.Fail);

            _Builder.Append($"<test-results xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"nunit_schema_2.5.xsd\" name=\"PSRule\" total=\"{total}\" errors=\"{error}\" failures=\"{fail}\" not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\" date=\"{DateTime.UtcNow.ToString("yyyy-MM-dd")}\" time=\"{TimeSpan.FromMilliseconds(time).ToString()}\">");
            _Builder.Append($"<environment user=\"{Environment.UserName}\" machine-name=\"{Environment.MachineName}\" cwd=\"{Configuration.PSRuleOption.GetWorkingPath()}\" user-domain=\"{Environment.UserDomainName}\" platform=\"{Environment.OSVersion.Platform}\" nunit-version=\"2.5.8.0\" os-version=\"{Environment.OSVersion.Version}\" clr-version=\"{Environment.Version.ToString()}\" />");
            _Builder.Append($"<culture-info current-culture=\"{System.Threading.Thread.CurrentThread.CurrentCulture.ToString()}\" current-uiculture=\"{System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()}\" />");

            foreach (var result in o)
            {
                if (result.Total == 0)
                    continue;

                var records = result.AsRecord();
                var testCases = records.Select(r => new TestCase(name: string.Concat(r.TargetName, " -- ", r.RuleName), description: r.Info.Synopsis, success: r.IsSuccess(), executed: r.IsProcessed(), time: r.Time))
                    .ToArray();
                var failedCount = testCases.Count(r => !r.Success);
                var fixture = new TestFixture(name: records[0].TargetName, description: "", success: result.IsSuccess(), executed: result.IsProcessed(), time: result.Time, asserts: failedCount, testCases: testCases);

                VisitFixture(fixture: fixture);
            }

            _Builder.Append("</test-results>");
            return _Builder.ToString();
        }

        private void VisitFixture(TestFixture fixture)
        {
            _Builder.Append($"<test-suite type=\"TestFixture\" name=\"{fixture.Name}\" executed=\"{fixture.Executed}\" result=\"{(fixture.Success ? "Success" : "Failure")}\" success=\"{fixture.Success}\" time=\"{fixture.Time.ToString()}\" asserts=\"{fixture.Asserts}\" description=\"{fixture.Description}\"><results>");

            foreach (var testCase in fixture.Results)
            {
                VisitTestCase(testCase: testCase);
            }

            _Builder.Append("</results></test-suite>");
        }

        private void VisitTestCase(TestCase testCase)
        {
            _Builder.Append($"<test-case description=\"{testCase.Description}\" name=\"{testCase.Name}\" time=\"{testCase.Time.ToString()}\" asserts=\"0\" success=\"{testCase.Success}\" result=\"{(testCase.Success ? "Success" : "Failure")}\" executed=\"{testCase.Executed}\" />");
        }

        private sealed class TestFixture
        {
            public readonly string Name;
            public readonly string Description;
            public readonly bool Success;
            public readonly bool Executed;
            public readonly float Time;
            public readonly int Asserts;
            public readonly TestCase[] Results;

            public TestFixture(string name, string description, bool success, bool executed, long time, int asserts, TestCase[] testCases)
            {
                Name = name;
                Description = description;
                Success = success;
                Executed = executed;
                Time = time / 1000f;
                Asserts = asserts;
                Results = testCases;
            }
        }

        private sealed class TestCase
        {
            public readonly string Name;
            public readonly string Description;
            public readonly bool Success;
            public readonly bool Executed;
            public readonly float Time;

            public TestCase(string name, string description, bool success, bool executed, long time)
            {
                Name = name;
                Description = description;
                Success = success;
                Executed = executed;
                Time = time / 1000f;
            }
        }
    }
}
