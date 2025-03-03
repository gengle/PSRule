﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System;
using System.Management.Automation;
using Xunit;

namespace PSRule
{
    [Trait(LANGUAGE, LANGUAGEELEMENT)]
    public sealed class AssertTests
    {
        private const string LANGUAGE = "Language";
        private const string LANGUAGEELEMENT = "Variable";

        [Fact]
        public void Assertion()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var actual1 = assert.Create(false, "Test reason");
            var actual2 = assert.Create(true, "Test reason");
            Assert.Equal("Test reason", actual1.ToString());
            Assert.False(actual1.Result);
            Assert.Equal(string.Empty, actual2.ToString());
            Assert.True(actual2.Result);

            actual1.WithReason("Alternate reason");
            Assert.Equal("Test reason Alternate reason", actual1.ToString());
            actual1.WithReason("Alternate reason", true);
            Assert.Equal("Alternate reason", actual1.ToString());
        }

        [Fact]
        public void StartsWith()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

            Assert.True(assert.StartsWith(value, "name", new string[] { "123", "ab" }).Result);
            Assert.True(assert.StartsWith(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
            Assert.True(assert.StartsWith(value, "name", new string[] { "ABC" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "123", "cd" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "123", "fg" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "abcdefgh" }).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
            Assert.False(assert.StartsWith(value, "name", new string[] { "123", "cd" }).Result);
        }

        [Fact]
        public void EndsWith()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

            Assert.True(assert.EndsWith(value, "name", new string[] { "123", "fg" }).Result);
            Assert.True(assert.EndsWith(value, "name", new string[] { "123", "fg" }, caseSensitive: true).Result);
            Assert.True(assert.EndsWith(value, "name", new string[] { "EFG" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "123", "cd" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "123", "ab" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "abcdefgh" }).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "EFG" }, caseSensitive: true).Result);
            Assert.False(assert.EndsWith(value, "name", new string[] { "123", "cd" }).Result);
        }

        [Fact]
        public void Contains()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject((name: "name", value: "abcdefg"), (name: "value", value: 123));

            Assert.True(assert.Contains(value, "name", new string[] { "123", "ab" }).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "ab" }, caseSensitive: true).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "ABC" }).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "cd" }).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "fg" }).Result);
            Assert.False(assert.Contains(value, "name", new string[] { "abcdefgh" }).Result);
            Assert.False(assert.Contains(value, "name", new string[] { "ABC" }, caseSensitive: true).Result);
            Assert.True(assert.Contains(value, "name", new string[] { "123", "cd" }).Result);
        }

        [Fact]
        public void Version()
        {
            SetContext();
            var assert = GetAssertionHelper();
            var value = GetObject(
                (name: "version", value: "1.2.3"),
                (name: "version2", value: "1.2.3-alpha.7"),
                (name: "version3", value: "3.4.5-alpha.9"),
                (name: "notversion", value: "x.y.z")
            );

            Assert.True(assert.Version(value, "version", "1.2.3").Result);
            Assert.False(assert.Version(value, "version", "0.2.3").Result);
            Assert.False(assert.Version(value, "version", "2.2.3").Result);
            Assert.False(assert.Version(value, "version", "1.1.3").Result);
            Assert.False(assert.Version(value, "version", "1.3.3").Result);
            Assert.False(assert.Version(value, "version", "1.2.2").Result);
            Assert.False(assert.Version(value, "version", "1.2.4").Result);

            Assert.True(assert.Version(value, "version", "v1.2.3").Result);
            Assert.True(assert.Version(value, "version", "V1.2.3").Result);
            Assert.True(assert.Version(value, "version", "=1.2.3").Result);
            Assert.False(assert.Version(value, "version", "=0.2.3").Result);
            Assert.False(assert.Version(value, "version", "=2.2.3").Result);
            Assert.False(assert.Version(value, "version", "=1.1.3").Result);
            Assert.False(assert.Version(value, "version", "=1.3.3").Result);
            Assert.False(assert.Version(value, "version", "=1.2.2").Result);
            Assert.False(assert.Version(value, "version", "=1.2.4").Result);

            Assert.True(assert.Version(value, "version", "^1.2.3").Result);
            Assert.False(assert.Version(value, "version", "^0.2.3").Result);
            Assert.False(assert.Version(value, "version", "^2.2.3").Result);
            Assert.True(assert.Version(value, "version", "^1.1.3").Result);
            Assert.False(assert.Version(value, "version", "^1.3.3").Result);
            Assert.True(assert.Version(value, "version", "^1.2.2").Result);
            Assert.False(assert.Version(value, "version", "^1.2.4").Result);

            Assert.True(assert.Version(value, "version", "~1.2.3").Result);
            Assert.False(assert.Version(value, "version", "~0.2.3").Result);
            Assert.False(assert.Version(value, "version", "~2.2.3").Result);
            Assert.False(assert.Version(value, "version", "~1.1.3").Result);
            Assert.False(assert.Version(value, "version", "~1.3.3").Result);
            Assert.True(assert.Version(value, "version", "~1.2.2").Result);
            Assert.False(assert.Version(value, "version", "~1.2.4").Result);

            Assert.True(assert.Version(value, "version", "1.x").Result);
            Assert.True(assert.Version(value, "version", "1.X.x").Result);
            Assert.True(assert.Version(value, "version", "1.*").Result);
            Assert.True(assert.Version(value, "version", "*").Result);
            Assert.True(assert.Version(value, "version", "").Result);
            Assert.True(assert.Version(value, "version").Result);
            Assert.False(assert.Version(value, "version", "1.3.x").Result);

            Assert.False(assert.Version(value, "version", ">1.2.3").Result);
            Assert.True(assert.Version(value, "version", ">0.2.3").Result);
            Assert.False(assert.Version(value, "version", ">2.2.3").Result);
            Assert.True(assert.Version(value, "version", ">1.1.3").Result);
            Assert.False(assert.Version(value, "version", ">1.3.3").Result);
            Assert.True(assert.Version(value, "version", ">1.2.2").Result);
            Assert.False(assert.Version(value, "version", ">1.2.4").Result);

            Assert.True(assert.Version(value, "version", ">=1.2.3").Result);
            Assert.True(assert.Version(value, "version", ">=0.2.3").Result);
            Assert.False(assert.Version(value, "version", ">=2.2.3").Result);
            Assert.True(assert.Version(value, "version", ">=1.1.3").Result);
            Assert.False(assert.Version(value, "version", ">=1.3.3").Result);
            Assert.True(assert.Version(value, "version", ">=1.2.2").Result);
            Assert.False(assert.Version(value, "version", ">=1.2.4").Result);

            Assert.False(assert.Version(value, "version", "<1.2.3").Result);
            Assert.False(assert.Version(value, "version", "<0.2.3").Result);
            Assert.True(assert.Version(value, "version", "<2.2.3").Result);
            Assert.False(assert.Version(value, "version", "<1.1.3").Result);
            Assert.True(assert.Version(value, "version", "<1.3.3").Result);
            Assert.False(assert.Version(value, "version", "<1.2.2").Result);
            Assert.True(assert.Version(value, "version", "<1.2.4").Result);

            Assert.True(assert.Version(value, "version", "<=1.2.3").Result);
            Assert.False(assert.Version(value, "version", "<=0.2.3").Result);
            Assert.True(assert.Version(value, "version", "<=2.2.3").Result);
            Assert.False(assert.Version(value, "version", "<=1.1.3").Result);
            Assert.True(assert.Version(value, "version", "<=1.3.3").Result);
            Assert.False(assert.Version(value, "version", "<=1.2.2").Result);
            Assert.True(assert.Version(value, "version", "<=1.2.4").Result);

            Assert.True(assert.Version(value, "version", ">1.0.0").Result);
            Assert.True(assert.Version(value, "version", "<2.0.0").Result);

            Assert.True(assert.Version(value, "version", ">1.2.3-alpha.3").Result);
            Assert.True(assert.Version(value, "version2", ">1.2.3-alpha.3").Result);
            Assert.False(assert.Version(value, "version3", ">1.2.3-alpha.3").Result);

            Assert.False(assert.Version(value, "notversion", null).Result);
            Assert.Throws<RuleRuntimeException>(() => assert.Version(value, "version", "2.0.0<").Result);
            Assert.Throws<RuleRuntimeException>(() => assert.Version(value, "version", "z2.0.0").Result);
        }

        private static void SetContext()
        {
            var context = PipelineContext.New(null, new Configuration.PSRuleOption(), null, null, null, null);
            context.ExecutionScope = ExecutionScope.Condition;
        }

        private static PSObject GetObject(params (string name, object value)[] properties)
        {
            var result = new PSObject();
            for (var i = 0; properties != null && i < properties.Length; i++)
                result.Properties.Add(new PSNoteProperty(properties[i].Item1, properties[i].Item2));

            return result;
        }

        private static Runtime.Assert GetAssertionHelper()
        {
            return new Runtime.Assert();
        }
    }
}
