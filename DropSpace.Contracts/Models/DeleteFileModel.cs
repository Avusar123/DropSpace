using System.ComponentModel.DataAnnotations;

namespace DropSpace.Domain.Models
{
    public class DeleteFileModel
    {
        [Required]
        public Guid FileId { get; set; }

        [Required]
        public Guid SessionId { get; set; }
    }
}
