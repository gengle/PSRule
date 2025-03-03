﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSRule
{
    public sealed class InputFormatDeserializerTests
    {
        [Fact]
        public void DeserializeObjectsYaml()
        {
            var actual = PipelineReceiverActions.ConvertFromYaml(GetYamlContent(), PipelineReceiverActions.PassThru).ToArray();

            Assert.Equal(2, actual.Length);
            Assert.Equal("TestObject1", actual[0].Properties["targetName"].Value);
            Assert.Equal("Test", actual[0].PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<string>("kind"));
            Assert.Equal(2, actual[1].PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<int>("value2"));
            Assert.Equal(2, actual[1].PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<PSObject[]>("array").Length);
        }

        [Fact]
        public void DeserialObjectsJson()
        {
            var actual = PipelineReceiverActions.ConvertFromJson(GetJsonContent(), PipelineReceiverActions.PassThru).ToArray();

            Assert.Equal(2, actual.Length);
            Assert.Equal("TestObject1", actual[0].Properties["targetName"].Value);
            Assert.Equal("Test", actual[0].PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<string>("kind"));
            Assert.Equal(2, actual[1].PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<int>("value2"));
            Assert.Equal(2, actual[1].PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<PSObject[]>("array").Length);
        }

        private string GetYamlContent()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromFile.yaml");
            return File.ReadAllText(path);
        }

        private string GetJsonContent()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromFile.json");
            return File.ReadAllText(path);
        }
    }
}
