﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline
{
    internal class HostContext
    {
        /// <summary>
        /// Determine if running is remote session.
        /// </summary>
        internal bool InSession;

        public HostContext()
        {
            InSession = false;
        }
    }
}
