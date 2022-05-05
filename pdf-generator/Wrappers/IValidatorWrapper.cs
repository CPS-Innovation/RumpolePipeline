using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pdf_generator.Wrappers
{
    public interface IValidatorWrapper<TRequest>
    {
        ICollection<ValidationResult> Validate(TRequest request);
    }
}
