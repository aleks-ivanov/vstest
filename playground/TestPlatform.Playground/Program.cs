﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

#nullable disable

namespace TestPlatform.Playground;

internal class Program
{
    static void Main(string[] args)
    {
        // This project references TranslationLayer, vstest.console, TestHostProvider, testhost and MSTest1 projects, to make sure
        // we build all the dependencies of that are used to run tests via VSTestConsoleWrapper. It then copies the components from
        // their original build locations, to $(TargetDir)\vstest.console directory, and it's subfolders to create an executable
        // copy of TestPlatform that is similar to what we ship.
        //
        // The copying might trigger only on re-build, if you see outdated dependencies, Rebuild this project instead of just Build.
        //
        // Use this as playground for your debugging of end-to-end scenarios, it will automatically attach vstest.console and teshost
        // sub-processes. It won't stop at entry-point automatically, don't forget to set your breakpoints, or remove VSTEST_DEBUG_NOBP
        // from the environment variables of this project.

        var thisAssemblyPath = Assembly.GetEntryAssembly().Location;
        var here = Path.GetDirectoryName(thisAssemblyPath);
        var playground = Path.GetFullPath(Path.Combine(here, "..", "..", "..", ".."));

        var console = Path.Combine(here, "vstest.console", "vstest.console.exe");
        var consoleOptions = new ConsoleParameters
        {
            LogFilePath = Path.Combine(here, "logs", "log.txt"),
            TraceLevel = TraceLevel.Verbose,
        };

        var r = new VsTestConsoleWrapper(console, consoleOptions);

        var sourceSettings = @"
                <RunSettings>
                    <RunConfiguration>
                        <InIsolation>true</InIsolation>
                        <MaxCpuCount>0</MaxCpuCount>
                    </RunConfiguration>
                </RunSettings>
            ";
        var sources = new[] {
            Path.Combine(playground, "MSTest1", "bin", "Debug", "net472", "MSTest1.dll")
        };

        var options = new TestPlatformOptions();
        r.RunTestsWithCustomTestHost(sources, sourceSettings, options, new TestRunHandler(), new DebuggerTestHostLauncher());
    }

    public class PlaygroundTestDiscoveryHandler : ITestDiscoveryEventsHandler, ITestDiscoveryEventsHandler2
    {
        private int _testCasesCount;

        public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
        {
            Console.WriteLine($"[DISCOVERY.PROGRESS]");
            Console.WriteLine(WriteTests(discoveredTestCases));
            _testCasesCount += discoveredTestCases.Count();
        }

        public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase> lastChunk, bool isAborted)
        {
            Console.WriteLine($"[DISCOVERY.COMPLETE] aborted? {isAborted}, tests count: {totalTests}");
            Console.WriteLine("Last chunk:");
            Console.WriteLine(WriteTests(lastChunk));
        }

        public void HandleDiscoveryComplete(DiscoveryCompleteEventArgs discoveryCompleteEventArgs, IEnumerable<TestCase> lastChunk)
        {
            Console.WriteLine($"[DISCOVERY.COMPLETE] aborted? {discoveryCompleteEventArgs.IsAborted}, tests count: {discoveryCompleteEventArgs.TotalCount}, discovered count: {_testCasesCount}");
            Console.WriteLine("Last chunk:");
            Console.WriteLine(WriteTests(lastChunk));
            Console.WriteLine("Fully discovered:");
            Console.WriteLine(WriteSources(discoveryCompleteEventArgs.FullyDiscoveredSources));
            Console.WriteLine("Partially discovered:");
            Console.WriteLine(WriteSources(discoveryCompleteEventArgs.PartiallyDiscoveredSources));
            Console.WriteLine("Not discovered:");
            Console.WriteLine(WriteSources(discoveryCompleteEventArgs.NotDiscoveredSources));
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            Console.WriteLine($"[DISCOVERY.{level.ToString().ToUpper()}] {message}");
        }

        public void HandleRawMessage(string rawMessage)
        {
            Console.WriteLine($"[DISCOVERY.MESSAGE] {rawMessage}");
        }

        private static string WriteTests(IEnumerable<TestCase> testCases)
            => testCases?.Any() == true
                ? "\t" + string.Join("\n\t", testCases.Select(r => r.DisplayName))
                : "\t<empty>";

        private static string WriteSources(IEnumerable<string> sources)
            => sources?.Any() == true
                ? "\t" + string.Join("\n\t", sources)
                : "\t<empty>";
    }

    public class TestRunHandler : ITestRunEventsHandler
    {

        public TestRunHandler()
        {
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            Console.WriteLine($"[{level.ToString().ToUpper()}]: {message}");
        }

        public void HandleRawMessage(string rawMessage)
        {
            Console.WriteLine($"[RUN.MESSAGE]: {rawMessage}");
        }

        public void HandleTestRunComplete(TestRunCompleteEventArgs testRunCompleteArgs, TestRunChangedEventArgs lastChunkArgs, ICollection<AttachmentSet> runContextAttachments, ICollection<string> executorUris)
        {
            Console.WriteLine($"[RUN.COMPLETE]: err: {testRunCompleteArgs.Error}, lastChunk:");
            Console.WriteLine(WriteTests(lastChunkArgs?.NewTestResults));
        }

        public void HandleTestRunStatsChange(TestRunChangedEventArgs testRunChangedArgs)
        {
            Console.WriteLine($"[RUN.PROGRESS]");
            Console.WriteLine(WriteTests(testRunChangedArgs.NewTestResults));
        }

        public int LaunchProcessWithDebuggerAttached(TestProcessStartInfo testProcessStartInfo)
        {
            throw new NotImplementedException();
        }

        private static string WriteTests(IEnumerable<TestResult> testResults)
            => WriteTests(testResults?.Select(t => t.TestCase));

        private static string WriteTests(IEnumerable<TestCase> testCases)
            => testCases?.Any() == true
                ? "\t" + string.Join("\n\t", testCases.Select(r => r.DisplayName))
                : "\t<empty>";
    }

    internal class DebuggerTestHostLauncher : ITestHostLauncher2
    {
        public bool IsDebug => true;

        public bool AttachDebuggerToProcess(int pid)
        {
            return true;
        }

        public bool AttachDebuggerToProcess(int pid, CancellationToken cancellationToken)
        {
            return true;
        }

        public int LaunchTestHost(TestProcessStartInfo defaultTestHostStartInfo)
        {
            return 1;
        }

        public int LaunchTestHost(TestProcessStartInfo defaultTestHostStartInfo, CancellationToken cancellationToken)
        {
            return 1;
        }
    }
}
