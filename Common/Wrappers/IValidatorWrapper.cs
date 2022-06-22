using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace common.Wrappers
{
    public interface IValidatorWrapper<TRequest>
    {
        ICollection<ValidationResult> Validate(TRequest request);
    }
}
