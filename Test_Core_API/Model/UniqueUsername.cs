using System.ComponentModel.DataAnnotations;
using Test_Core_API.Entity;

namespace Test_Core_API.Model
{
    public class UniqueUsername : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var context = (Context)validationContext.GetService(typeof(Context));
            var userName = (string)value;

            if (context == null)
            {
                throw new InvalidOperationException("Validation context does not contain the database context.");
            }

            // Check if the username already exists in the database
            var existingUser = context.Users.FirstOrDefault(u => u.UserName == userName);
            if (existingUser != null)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
