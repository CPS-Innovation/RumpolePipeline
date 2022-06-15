using System;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Options;
using text_extractor.Services.OcrService;

namespace text_extractor.Factories
{
	public class ComputerVisionClientFactory : IComputerVisionClientFactory
	{
        private readonly OcrOptions _options;

        public ComputerVisionClientFactory(IOptions<OcrOptions> options)
        {
            _options = options.Value;
        }

		public ComputerVisionClient Create()
        {
			return new ComputerVisionClient(new ApiKeyServiceClientCredentials(_options.ServiceKey))
			{
				Endpoint = _options.ServiceUrl
			};
		}
	}
}

