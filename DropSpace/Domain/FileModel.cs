﻿namespace DropSpace.Domain
{
    public class FileModel
    {
        public Guid Id { get; set; }

        public long ByteSize { get; set; }

        public string FileName { get; set; }

        public Guid SessionId { get; set; }

        public Session Session { get; set; }

        public bool IsUploaded { get; set; }
    }
}