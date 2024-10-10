using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DropSpace.Contracts.Models
{
    public class UploadChunkModel
    {
        [Required]
        public byte[] Chunk { get; set; } = [];

        [Required]
        public Guid UploadId { get; set; }
    }
}
