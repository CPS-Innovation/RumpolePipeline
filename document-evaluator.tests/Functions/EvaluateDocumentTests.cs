using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using AutoFixture;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Exceptions.Contracts;
using Common.Handlers;
using Common.Services.DocumentEvaluationService.Contracts;
using Common.Wrappers;
using document_evaluation.Functions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace document_evaluation.tests.Functions;

public class EvaluateDocumentTests
{
    private readonly Fixture _fixture = new();
    private readonly string _serializedEvaluateDocumentRequest;
	private readonly HttpRequestMessage _httpRequestMessage;
	private readonly EvaluateDocumentRequest _evaluateDocumentRequest;
	private readonly EvaluateDocumentResponse _evaluateDocumentResponse;
	private readonly string _serializedEvaluationDocumentResponse;
	private HttpResponseMessage _errorHttpResponseMessage;
	
	private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
	private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
	private readonly Mock<IExceptionHandler> _mockExceptionHandler;
    private readonly Mock<ILogger<EvaluateDocument>> _mockLogger;
    private readonly Mock<IValidatorWrapper<EvaluateDocumentRequest>> _mockValidatorWrapper;
    private readonly Guid _correlationId;
    private readonly Mock<IDocumentEvaluationService> _mockDocumentEvaluationService;

	private readonly EvaluateDocument _evaluateDocument;

	public EvaluateDocumentTests()
	{
		_evaluateDocumentRequest = _fixture.Create<EvaluateDocumentRequest>();
		_serializedEvaluateDocumentRequest = _evaluateDocumentRequest.ToJson();
		_httpRequestMessage = new HttpRequestMessage()
		{
			Content = new StringContent(_serializedEvaluateDocumentRequest)
		};
		_evaluateDocumentResponse = _fixture.Create<EvaluateDocumentResponse>();
		_serializedEvaluationDocumentResponse = _evaluateDocumentResponse.ToJson();
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		
		_mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
		_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
		_mockValidatorWrapper = new Mock<IValidatorWrapper<EvaluateDocumentRequest>>();
		_mockDocumentEvaluationService = new Mock<IDocumentEvaluationService>();
		_mockExceptionHandler = new Mock<IExceptionHandler>();
		_mockLogger = new Mock<ILogger<EvaluateDocument>>();
		_correlationId = _fixture.Create<Guid>();

		_mockAuthorizationValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
		_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<EvaluateDocumentRequest>(_serializedEvaluateDocumentRequest))
			.Returns(_evaluateDocumentRequest);
		_mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<EvaluateDocumentResponse>()))
			.Returns(_serializedEvaluationDocumentResponse);
		_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_evaluateDocumentRequest)).Returns(new List<ValidationResult>());
		
		_evaluateDocument = new EvaluateDocument(
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
		
		var response = await _evaluateDocument.Run(_httpRequestMessage);

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

		var response = await _evaluateDocument.Run(_httpRequestMessage);

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

		var response = await _evaluateDocument.Run(_httpRequestMessage);

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

		var response = await _evaluateDocument.Run(_httpRequestMessage);

		response.Should().Be(_errorHttpResponseMessage);
	}
	
	[Fact]
	public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
	{
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

		var response = await _evaluateDocument.Run(_httpRequestMessage);

		response.Should().Be(_errorHttpResponseMessage);
	}
	
	[Fact]
	public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
	{
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

		var response = await _evaluateDocument.Run(_httpRequestMessage);

		response.Should().Be(_errorHttpResponseMessage);
	}

	[Fact]
	public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
	{
		var validationResults = _fixture.CreateMany<ValidationResult>(2).ToList();
		
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);
		_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_evaluateDocumentRequest)).Returns(validationResults);
		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

		var response = await _evaluateDocument.Run(_httpRequestMessage);

		response.Should().Be(_errorHttpResponseMessage);
	}
	
	[Fact]
	public async Task Run_ReturnsOk()
	{
		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

		_mockDocumentEvaluationService.Setup(s => s.EvaluateDocumentAsync(It.IsAny<EvaluateDocumentRequest>(), It.IsAny<Guid>()))
			.ReturnsAsync(_evaluateDocumentResponse);
		
		var response = await _evaluateDocument.Run(_httpRequestMessage);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task Run_ReturnsExpectedContent()
	{
		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
		
		_mockDocumentEvaluationService.Setup(s => s.EvaluateDocumentAsync(It.IsAny<EvaluateDocumentRequest>(), It.IsAny<Guid>()))
			.ReturnsAsync(_evaluateDocumentResponse);

		var response = await _evaluateDocument.Run(_httpRequestMessage);
		var content = await response.Content.ReadAsStringAsync();
		content.Should().Contain(_serializedEvaluationDocumentResponse);
	}

	[Fact]
	public async Task Run_ReturnsResponseWhenExceptionOccurs()
	{
		_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
		var exception = new Exception();
		_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<EvaluateDocumentRequest>(_serializedEvaluateDocumentRequest))
			.Throws(exception);
		_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
			.Returns(_errorHttpResponseMessage);

		_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
		var response = await _evaluateDocument.Run(_httpRequestMessage);

		response.Should().Be(_errorHttpResponseMessage);
	}
}
