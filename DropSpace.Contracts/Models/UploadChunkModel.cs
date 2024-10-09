using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DropSpace.Domain.Models
{
    public class UploadChunkModel
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public Guid UploadId { get; set; }
    }
}
