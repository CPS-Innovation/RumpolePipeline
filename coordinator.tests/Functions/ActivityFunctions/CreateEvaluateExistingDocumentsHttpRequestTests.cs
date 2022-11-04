using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.DocumentExtraction;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions;

public class CreateEvaluateExistingDocumentsHttpRequestTests
{
    private readonly CreateEvaluateExistingDocumentsHttpRequestActivityPayload _payload;
    private readonly DurableHttpRequest _durableRequest;

    private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

    private readonly CreateEvaluateExistingDocumentsHttpRequest _createEvaluateExistingDocumentsHttpRequest;

    public CreateEvaluateExistingDocumentsHttpRequestTests()
    {
        var fixture = new Fixture();
        _payload = fixture.Create<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>();
        _payload.CaseDocuments = fixture.CreateMany<CaseDocument>(3).ToList();
        _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.test.co.uk"));

        var mockEvaluateDocumentHttpRequestFactory = new Mock<IEvaluateExistingDocumentsHttpRequestFactory>();
        _mockDurableActivityContext = new Mock<IDurableActivityContext>();

        _mockDurableActivityContext.Setup(context => context.GetInput<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>())
            .Returns(_payload);

        mockEvaluateDocumentHttpRequestFactory.Setup(client => client.Create(_payload.CaseId, 
            _payload.CaseDocuments, _payload.CorrelationId)).ReturnsAsync(_durableRequest);

        var mockLogger = new Mock<ILogger<CreateEvaluateExistingDocumentsHttpRequest>>();
        _createEvaluateExistingDocumentsHttpRequest = new CreateEvaluateExistingDocumentsHttpRequest(mockEvaluateDocumentHttpRequestFactory.Object, mockLogger.Object);
    }

    [Fact]
    public async Task Run_ThrowsWhenPayloadIsNull()
    {
        _mockDurableActivityContext.Setup(context => context.GetInput<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>())
            .Returns(default(CreateEvaluateExistingDocumentsHttpRequestActivityPayload));

        await Assert.ThrowsAsync<ArgumentException>(() => _createEvaluateExistingDocumentsHttpRequest.Run(_mockDurableActivityContext.Object));
    }
    
    [Fact]
    public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
    {
        _payload.CaseId = 0;
        _mockDurableActivityContext.Setup(context => context.GetInput<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _createEvaluateExistingDocumentsHttpRequest.Run(_mockDurableActivityContext.Object));
    }
    
    [Fact]
    public async Task Run_WhenCaseDocumentsIsNull_ThrowsArgumentException()
    {
        _payload.CaseDocuments = null;
        _mockDurableActivityContext.Setup(context => context.GetInput<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _createEvaluateExistingDocumentsHttpRequest.Run(_mockDurableActivityContext.Object));
    }
    
    [Fact]
    public async Task Run_WhenCaseDocumentsIsZeroLength_ThrowsArgumentException()
    {
        _payload.CaseDocuments = new List<CaseDocument>();
        _mockDurableActivityContext.Setup(context => context.GetInput<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _createEvaluateExistingDocumentsHttpRequest.Run(_mockDurableActivityContext.Object));
    }
    
    [Fact]
    public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
    {
        _payload.CorrelationId = Guid.Empty;
        _mockDurableActivityContext.Setup(context => context.GetInput<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _createEvaluateExistingDocumentsHttpRequest.Run(_mockDurableActivityContext.Object));
    }

    [Fact]
    public async Task Run_ReturnsDurableRequest()
    {
        var durableRequest = await _createEvaluateExistingDocumentsHttpRequest.Run(_mockDurableActivityContext.Object);

        durableRequest.Should().Be(_durableRequest);
    }
}
