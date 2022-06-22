using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace common.Wrappers
{
    public class ValidatorWrapper<TRequest> : IValidatorWrapper<TRequest>
    {
        public ICollection<ValidationResult> Validate(TRequest request)
        {
            var validationResults = new Collection<ValidationResult>();
            Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);
            return validationResults;
        } 
    }
}
