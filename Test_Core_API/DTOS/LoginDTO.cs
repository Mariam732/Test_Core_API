using System.ComponentModel.DataAnnotations;
using Test_Core_API.Model;

namespace Test_Core_API.DTOS
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Enter name")]
        public string UserName { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
