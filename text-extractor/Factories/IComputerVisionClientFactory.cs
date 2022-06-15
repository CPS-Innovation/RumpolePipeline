using System;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace text_extractor.Factories
{
	public interface IComputerVisionClientFactory
	{
		ComputerVisionClient Create();
	}
}

