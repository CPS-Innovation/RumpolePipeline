using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using text_extractor.Domain.Exceptions;
using text_extractor.Factories;
using text_extractor.Services.SasGeneratorService;

namespace text_extractor.Services.OcrService
{
    public class OcrService : IOcrService
    {
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly ISasGeneratorService _sasGeneratorService;
        private readonly ILogger<OcrService> _log;

        public OcrService(
            IComputerVisionClientFactory computerVisionClientFactory,
            ISasGeneratorService sasGeneratorService, ILogger<OcrService> log)
        {
            _computerVisionClient = computerVisionClientFactory.Create();
            _sasGeneratorService = sasGeneratorService;
            _log = log;
        }

        public async Task<AnalyzeResults> GetOcrResultsAsync(string blobName)
        {
            var sasLink = await _sasGeneratorService.GenerateSasUrlAsync(blobName);
            _log.LogInformation("SasLink is:" + sasLink);

            try
            {
                var textHeaders = await _computerVisionClient.ReadAsync(sasLink);


                string operationLocation = textHeaders.OperationLocation;
                await Task.Delay(500);

                const int numberOfCharsInOperationId = 36;
                string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

                ReadOperationResult results;

                while (true)
                {
                    results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));

                    if (results.Status == OperationStatusCodes.Running ||
                        results.Status == OperationStatusCodes.NotStarted)
                    {
                        await Task.Delay(500);
                    }
                    else
                    {
                        break;
                    }
                }

                return results.AnalyzeResult;
            }
            catch(Exception ex)
            {
                throw new OcrServiceException(ex.Message);
            }
        }
    }
}