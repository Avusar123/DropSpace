using System.ComponentModel.DataAnnotations;

namespace DropSpace.Contracts.Models
{
    public class DownloadChunkModel
    {
        public Guid FileId { get; set; }

        public long StartWith { get; set; }
    }
}
