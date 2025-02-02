// -----------------------------------------------------------------------
// <copyright file="Diagnoser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DaaS.Leases;
using DaaS.Storage;

namespace DaaS.Diagnostics
{
    public class Diagnoser
    {
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string ProcessCleanupOnCancel { get; internal set; }
        internal Collector Collector { get; set; }
        internal Analyzer Analyzer { get; set; }
        public bool DiagnoserRequiresStorage { get; set; }

        public List<String> GetWarnings()
        {
            var warnings = new List<String>();
            if (!string.IsNullOrEmpty(Collector.Warning))
            {
                if (!Collector.PreValidationSucceeded(out string additionalInfo))
                {
                    if (!string.IsNullOrWhiteSpace(additionalInfo))
                    {
                        warnings.Add(additionalInfo);
                    }
                    else
                    {
                        warnings.Add(Collector.Warning);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Analyzer.Warning))
            {
                warnings.Add(Analyzer.Warning);
            }

            return warnings;
        }

        internal async Task<List<Log>> CollectLogs(DateTime utcStartTime, DateTime utcEndTime, string sessionId, string blobSasUri, string defaultHostName, CancellationToken ct)
        {
            var logs = await Collector.CollectLogs(utcStartTime, utcEndTime, sessionId, blobSasUri, defaultHostName, ct);
            return logs;
        }

        internal async Task<List<Report>> Analyze(Log log, string sessionId, string blobSasUri, string defaultHostName, CancellationToken ct)
        {
            var reports = await Analyzer.Analyze(log, sessionId, blobSasUri, defaultHostName, ct);
            return reports;
        }
    }
}
