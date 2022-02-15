// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace Microsoft.TestPlatform.CommunicationUtilities.UnitTests.ObjectModel;

using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class TestDiscoveryEventHandlerTests
{
    private Mock<ITestRequestHandler> _mockClient;
    private TestDiscoveryEventHandler _testDiscoveryEventHandler;

    [TestInitialize]
    public void InitializeTests()
    {
        _mockClient = new Mock<ITestRequestHandler>();
        _testDiscoveryEventHandler = new TestDiscoveryEventHandler(_mockClient.Object);
    }

    [TestMethod]
    public void HandleDiscoveredTestShouldSendTestCasesToClient()
    {
        _testDiscoveryEventHandler.HandleDiscoveredTests(null);
        _mockClient.Verify(th => th.SendTestCases(null), Times.Once);
    }

    [TestMethod]
    public void HandleDiscoveryCompleteShouldInformClient()
    {
        var discoveryCompleteEventArgs = new DiscoveryCompleteEventArgs(0, false);

        _testDiscoveryEventHandler.HandleDiscoveryComplete(discoveryCompleteEventArgs, null);
        _mockClient.Verify(th => th.DiscoveryComplete(discoveryCompleteEventArgs, null), Times.Once);
    }

    [TestMethod]
    public void HandleDiscoveryCompleteShouldNotSendASeparateTestFoundMessageToClient()
    {
        _testDiscoveryEventHandler.HandleDiscoveryComplete(new DiscoveryCompleteEventArgs(0, false), null);

        _mockClient.Verify(th => th.SendTestCases(null), Times.Never);
    }

    [TestMethod]
    public void HandleDiscoveryMessageShouldSendMessageToClient()
    {
        _testDiscoveryEventHandler.HandleLogMessage(TestMessageLevel.Informational, string.Empty);

        _mockClient.Verify(th => th.SendLog(TestMessageLevel.Informational, string.Empty), Times.AtLeast(1));
    }
}
