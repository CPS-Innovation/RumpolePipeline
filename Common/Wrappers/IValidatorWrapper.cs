using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Common.Wrappers
{
    public interface IValidatorWrapper<TRequest>
    {
        ICollection<ValidationResult> Validate(TRequest request);
    }
}
