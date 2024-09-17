using System.ComponentModel.DataAnnotations;

namespace DropSpace.Models
{
    public class DownloadChunkModel
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public Guid FileId { get; set; }

        [Range(0, long.MaxValue)]
        public long StartWith { get; set; }
    }
}
