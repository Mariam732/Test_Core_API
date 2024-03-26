using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test_Core_API.Model
{

    [Table("Users")]
    public class User
    {

        [Key]

        [Required(ErrorMessage = "Enter name")]
        public string UserName { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]

        [UniqueEmail(ErrorMessage = "Email already exists !!!")]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime LastLoginTime { get; set; }
    }
}
