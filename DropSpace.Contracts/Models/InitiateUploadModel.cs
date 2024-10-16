using System.ComponentModel.DataAnnotations;

namespace DropSpace.Contracts.Models
{
    public class InitiateUploadModel
    {
        public long Size { get; set; }

        public Guid SessionId { get; set; }

        public Guid FileId { get; set; }
    }
}
