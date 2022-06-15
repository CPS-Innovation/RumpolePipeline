using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Services.OcrService
{
	public interface IOcrService
	{
		Task<AnalyzeResults> GetOcrResults(string blobName);
	}
}

