using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.DocumentExtraction;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Handlers;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Functions;
using pdf_generator.Handlers;
using pdf_generator.Services.DocumentEvaluationService;
using Xunit;

namespace pdf_generator.tests.Functions;

public class EvaluateExistingDocumentsTests
{
    private readonly Fixture _fixture = new();
    private readonly HttpRequestMessage _httpRequestMessage;
    private readonly string _seralizedEvaluateExistingDocumentsRequest;
    private readonly EvaluateExistingDocumentsRequest _evaluateExistingDocumentsRequest;
    private readonly List<EvaluateDocumentResponse> _evaluateDocumentsResponse;
    private readonly string _serializedResponse;
    private HttpResponseMessage _errorHttpResponseMessage;
    
    private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
    private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
    private readonly Mock<IExceptionHandler> _mockExceptionHandler;
    private readonly Mock<ILogger<EvaluateExistingDocuments>> _mockLogger;
    private readonly Mock<IValidatorWrapper<EvaluateExistingDocumentsRequest>> _mockValidatorWrapper;
    private readonly Guid _correlationId;
    private readonly Mock<IDocumentEvaluationService> _mockDocumentEvaluationService;

    private readonly EvaluateExistingDocuments _evaluateExistingDocuments;

    public EvaluateExistingDocumentsTests()
    {
	    _evaluateExistingDocumentsRequest = _fixture.Create<EvaluateExistingDocumentsRequest>();
	    _seralizedEvaluateExistingDocumentsRequest = _evaluateExistingDocumentsRequest.ToJson();
	    _httpRequestMessage = new HttpRequestMessage()
	    {
		    Content = new StringContent(_seralizedEvaluateExistingDocumentsRequest)
	    };
	    _evaluateDocumentsResponse = _fixture.CreateMany<EvaluateDocumentResponse>(3).ToList();
    	_serializedResponse = _evaluateDocumentsResponse.ToJson();
    	
    	_mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
    	_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
    	_mockValidatorWrapper = new Mock<IValidatorWrapper<EvaluateExistingDocumentsRequest>>();
    	_mockDocumentEvaluationService = new Mock<IDocumentEvaluationService>();
    	_mockExceptionHandler = new Mock<IExceptionHandler>();
    	_mockLogger = new Mock<ILogger<EvaluateExistingDocuments>>();
    	_correlationId = _fixture.Create<Guid>();

    	_mockAuthorizationValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
    		.ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
    	_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<EvaluateExistingDocumentsRequest>(_seralizedEvaluateExistingDocumentsRequest))
    		.Returns(_evaluateExistingDocumentsRequest);
    	_mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<List<EvaluateDocumentResponse>>()))
    		.Returns(_serializedResponse);
    	_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_evaluateExistingDocumentsRequest)).Returns(new List<ValidationResult>());
    	
    	_evaluateExistingDocuments = new EvaluateExistingDocuments(
    						_mockAuthorizationValidator.Object,
    						_mockJsonConvertWrapper.Object,
    						_mockLogger.Object, 
    						_mockValidatorWrapper.Object,
    						_mockDocumentEvaluationService.Object,
    						_mockExceptionHandler.Object);
    }
    
    [Fact]
    public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
    {
	    _errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
	    _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
		    .Returns(_errorHttpResponseMessage);
    	_httpRequestMessage.Content = new StringContent(" ");
    	
    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }

    [Fact]
    public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
    {
        _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
    		.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
    	_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
    	_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
    		.Returns(_errorHttpResponseMessage);
    	_httpRequestMessage.Content = new StringContent(" ");
    	_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }

    [Fact]
    public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
    {
    	_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
    	_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
    		.Returns(_errorHttpResponseMessage);
    	_httpRequestMessage.Content = new StringContent(" ");
    	_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }
    
    [Fact]
    public async Task Run_ReturnsBadRequestWhenContentIsNull()
    {
    	_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
    	_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
    		.Returns(_errorHttpResponseMessage);
    	_httpRequestMessage.Content = null;
    	_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }
    
    [Fact]
    public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
    {
    	_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
    	_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
    		.Returns(_errorHttpResponseMessage);
    	_httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }
    
    [Fact]
    public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
    {
    	_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
    	_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
    		.Returns(_errorHttpResponseMessage);
    	_httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }

    [Fact]
    public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
    {
    	var validationResults = _fixture.CreateMany<ValidationResult>(2).ToList();
    	
    	_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
    	_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
    		.Returns(_errorHttpResponseMessage);
    	_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_evaluateExistingDocumentsRequest)).Returns(validationResults);
    	_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }
    
    [Fact]
    public async Task Run_ReturnsOk()
    {
    	_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
        
        _mockDocumentEvaluationService.Setup(s => s.EvaluateExistingDocumentsAsync(It.IsAny<string>(), It.IsAny<List<CaseDocument>>(), It.IsAny<Guid>()))
	        .ReturnsAsync(_evaluateDocumentsResponse);
        
    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Run_ReturnsExpectedContent()
    {
    	_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
        
        _mockDocumentEvaluationService.Setup(s => s.EvaluateExistingDocumentsAsync(It.IsAny<string>(), It.IsAny<List<CaseDocument>>(), It.IsAny<Guid>()))
	        .ReturnsAsync(_evaluateDocumentsResponse);
        
    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	var content = await response.Content.ReadAsStringAsync();
    	content.Should().Contain(_serializedResponse);
    }

    [Fact]
    public async Task Run_ReturnsResponseWhenExceptionOccurs()
    {
    	_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
    	var exception = new Exception();
    	_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<EvaluateExistingDocumentsRequest>(_seralizedEvaluateExistingDocumentsRequest))
    		.Throws(exception);
    	_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
    		.Returns(_errorHttpResponseMessage);

    	_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
    	var response = await _evaluateExistingDocuments.Run(_httpRequestMessage);

    	response.Should().Be(_errorHttpResponseMessage);
    }
}
