using System.ComponentModel.DataAnnotations;

namespace DropSpace.Contracts.Models
{
    public class CreateSessionModel
    {
        [MaxLength(15, ErrorMessage = "Превышен лимит названия сессии!")]
        [Required(ErrorMessage = "Это поле обязательно!")]
        public string Name { get; set; }
    }
}
