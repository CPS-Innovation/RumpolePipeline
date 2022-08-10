using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Domain.Validation;
using FluentValidation;
using Newtonsoft.Json;

namespace Common.Domain.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(this HttpRequestMessage request)
            where V : AbstractValidator<T>, new()
        {
            var requestObject = await request.GetJsonBody<T>();
            var validator = new V();
            var validationResult = await validator.ValidateAsync(requestObject);

            if (!validationResult.IsValid)
            {
                return new ValidatableRequest<T>
                {
                    Value = requestObject,
                    IsValid = false,
                    Errors = validationResult.Errors
                };
            }

            return new ValidatableRequest<T>
            {
                Value = requestObject,
                IsValid = true
            };
        }

        public static async Task<T> GetJsonBody<T>(this HttpRequestMessage request)
        {
            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(requestBody);
            }

            throw new ArgumentNullException(nameof(request));
        }
    }
}
