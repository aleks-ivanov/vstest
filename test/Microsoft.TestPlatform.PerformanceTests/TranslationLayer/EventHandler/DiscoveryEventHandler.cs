﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

#nullable disable

namespace Microsoft.TestPlatform.PerformanceTests.TranslationLayer;

/// <inheritdoc />
public class DiscoveryEventHandler2 : ITestDiscoveryEventsHandler2
{
    /// <summary>
    /// Gets the discovered test cases.
    /// </summary>
    public List<TestCase> DiscoveredTestCases { get; }

    /// <summary>
    /// Gets the metrics.
    /// </summary>
    public IDictionary<string, object> Metrics { get; private set; }

    public DiscoveryEventHandler2()
    {
        DiscoveredTestCases = new List<TestCase>();
    }

    public void HandleRawMessage(string rawMessage)
    {
        // No Op
    }

    public void HandleLogMessage(TestMessageLevel level, string message)
    {
        // No Op
    }

    public void HandleDiscoveryComplete(DiscoveryCompleteEventArgs discoveryCompleteEventArgs, IEnumerable<TestCase> lastChunk)
    {
        if (lastChunk != null)
        {
            DiscoveredTestCases.AddRange(lastChunk);
        }

        Metrics = discoveryCompleteEventArgs.Metrics;
    }

    public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
    {
        if (discoveredTestCases != null)
        {
            DiscoveredTestCases.AddRange(discoveredTestCases);
        }
    }
}
