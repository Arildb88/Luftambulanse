using System.ComponentModel.DataAnnotations;
namespace Gruppe4NLA.Models.Validation
{
    public class HeightRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (ReportModel)validationContext.ObjectInstance;

            if (value == null)
                return ValidationResult.Success;

            double meters = model.HeightUnit == "feet"
                ? (model.HeightInMeters!.Value * 0.3048)
                : model.HeightInMeters!.Value;

            if (meters < 0 || meters > 500)
            {
                return new ValidationResult("Height must be between 0 and 500 meters.");
            }

            return ValidationResult.Success;
        }
    }
}
