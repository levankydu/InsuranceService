using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace test0000001.Attributes
{
    public class StartDateValidation : ValidationAttribute, IClientModelValidator
    {
        private static readonly DateTime _validDate = DateTime.Today.AddDays(3);
        private readonly string _errorMsg = "Start date cannot be less than " + _validDate.ToString("dd MMMM yyyy");
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var valueString = value != null ? value.ToString() : null;

            if (string.IsNullOrWhiteSpace(valueString))
            {
                // No value, so return success.
                return ValidationResult.Success;
            }

            // Convert to date time.
            if (!DateTime.TryParse(valueString, out DateTime startDate))
            {
                // Not a valid date, so return error.
                return new ValidationResult("Unable to convert the input date to a valid date");
            }

            // Minimum date
            if (startDate <= _validDate)
            {
                // Under minimum date, so return error.
                return new ValidationResult(_errorMsg);
            }

            // Return success
            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-startdate", _errorMsg);
        }

    }
}
