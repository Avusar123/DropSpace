using System.ComponentModel.DataAnnotations;

namespace DropSpace.Models
{
    public class InitiateUploadModel
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long Size { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Guid SessionId { get; set; }
    }
}
