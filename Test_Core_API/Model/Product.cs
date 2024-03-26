using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test_Core_API.Model
{

    [Table("Products")]

    public class Product
    {


        [Key]
        public string ProductCode { get; set; }


        [Required(ErrorMessage = "Enter Category of Product")]
        public string Category { get; set; }


        [Required(ErrorMessage = "Enter Name of Product")]
        public string Name { get; set; }


        public string Image { get; set; }


        [Required(ErrorMessage = "You Should Enter price")]
        public double Price{ get; set; }


        [Required(ErrorMessage = "You Should enter minimum quantity")]
        public int MinimumQuantity { get; set; }


        [Required(ErrorMessage = "Enter Discount Rate")]
        public double DiscountRate { get; set; }





    }
}
