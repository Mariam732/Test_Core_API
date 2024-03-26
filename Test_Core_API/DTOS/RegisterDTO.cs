using System.ComponentModel.DataAnnotations;
using Test_Core_API.Model;

namespace Test_Core_API.DTOS
{
    public class RegisterDTO
    {

        [Required(ErrorMessage = "Enter name")]
        [UniqueUsername(ErrorMessage = "Username already exists !!!")]
        public string UserName { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [UniqueEmail(ErrorMessage = "Email already exists !!!")]
        public string Email { get; set; }

    }
}
