﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;
using System;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal abstract class RulePipeline : IDisposable, IPipeline
    {
        protected readonly PipelineContext Context;
        protected readonly Source[] Source;
        protected readonly PipelineReader Reader;
        protected readonly PipelineWriter Writer;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        protected RulePipeline(PipelineContext context, Source[] source, PipelineReader reader, PipelineWriter writer)
        {
            Context = context;
            Source = source;
            Reader = reader;
            Writer = writer;
        }

        #region IPipeline

        public virtual void Begin()
        {
            Reader.Open();
        }

        public virtual void Process(PSObject sourceObject)
        {
            // Do nothing
        }

        public virtual void End()
        {
            Writer.End();
        }

        #endregion IPipeline

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
                _Disposed = true;
            }
        }

        #endregion IDisposable
    }
}
