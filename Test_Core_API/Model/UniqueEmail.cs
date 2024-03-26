using System.ComponentModel.DataAnnotations;
using Test_Core_API.Entity;

namespace Test_Core_API.Model
{
    public class UniqueEmail : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var context = (Context)validationContext.GetService(typeof(Context));
            var email = (string)value;

            if (context == null)
            {
                throw new InvalidOperationException("Validation context does not contain the database context.");
            }

            // Check if the email address already exists in the database
            var existingUser = context.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

    }
}
