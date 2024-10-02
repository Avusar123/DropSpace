using System.ComponentModel.DataAnnotations;

namespace DropSpace.Models
{
    public struct InitiateUploadModel
    {

        Nullable<int> id;

        [Required]
        [Range(1, long.MaxValue)]
        public long Size { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Guid SessionId { get; set; }
    }
}
