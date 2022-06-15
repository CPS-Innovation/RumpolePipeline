using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using text_extractor.Domain.Exceptions;
using text_extractor.Services.SasGeneratorService;

namespace text_extractor.Services.OcrService
{
    public class OcrService : IOcrService
    {
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly ISasGeneratorService _sasGeneratorService;

        public OcrService(IOptions<OcrOptions> ocrOptions, ISasGeneratorService sasGeneratorService)
        {
            _computerVisionClient = Authenticate(ocrOptions.Value.ServiceUrl, ocrOptions.Value.ServiceKey);
            _sasGeneratorService = sasGeneratorService;
        }

        private ComputerVisionClient Authenticate(string endpoint, string key)
        {
            var client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint, };
            return client;
        }

        public async Task<AnalyzeResults> GetOcrResults(string blobName)
        {
            var sasLink = await _sasGeneratorService.GenerateSasUrl(blobName);

            try
            {
                var textHeaders = await _computerVisionClient.ReadAsync(sasLink);

                string operationLocation = textHeaders.OperationLocation;
                await Task.Delay(500); //TODO do we need this?

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