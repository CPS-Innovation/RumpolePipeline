using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Options;
using text_extractor.Domain;

namespace text_extractor.Factories
{
	public class ComputerVisionClientFactory : IComputerVisionClientFactory
	{
        private readonly ComputerVisionClientOptions _options;

        public ComputerVisionClientFactory(IOptions<ComputerVisionClientOptions> options)
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

