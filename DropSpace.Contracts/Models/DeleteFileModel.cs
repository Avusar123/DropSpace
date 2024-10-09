using System.ComponentModel.DataAnnotations;

namespace DropSpace.Contracts.Models
{
    public class DeleteFileModel
    {
        [Required]
        public Guid FileId { get; set; }

        [Required]
        public Guid SessionId { get; set; }
    }
}
