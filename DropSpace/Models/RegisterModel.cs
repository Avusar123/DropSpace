using System.ComponentModel.DataAnnotations;

namespace DropSpace.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Почта")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} должен быть не менее {2} и не более {1} символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
}
