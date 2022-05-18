using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
	public interface IGeneratePdfHttpRequestFactory
	{
		Task<DurableHttpRequest> Create(int caseId, string documentId, string fileName);
	}
}

