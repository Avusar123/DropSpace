using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DropSpace.Contracts.Models
{
    public class UploadChunkModel
    {
        public byte[] Chunk { get; set; } = [];

        public Guid FileId { get; set; }
    }
}
