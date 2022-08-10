using System;
using System.Linq;
using Common.Domain.Validation;

namespace Common.Domain.Extensions
{
    public static class ValidatableRequestExtensions
    {
        public static string FlattenErrors<T>(this ValidatableRequest<T> request)
        {
            var errorsThrown = request.Errors.Select(e => new
            {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            });

            return string.Join(Environment.NewLine, errorsThrown);
        }
    }
}
