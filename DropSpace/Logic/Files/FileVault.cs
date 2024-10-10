using DropSpace.Logic.Files.Interfaces;

namespace DropSpace.Logic.Files
{
    public class FileVault : IFileVault
    {
        private readonly string pathToFiles;

        public FileVault(IConfiguration configuration)
        {
            pathToFiles = configuration.GetValue<string>("FilesDirPath")
                                    ?? throw new NullReferenceException("Путь не указан!");

            if (!Directory.Exists(pathToFiles))
            {
                Directory.CreateDirectory(pathToFiles);
            }
        }

        public Task<bool> CanFit(long size)
        {
            DriveInfo[] driveInfos = DriveInfo.GetDrives();

            DriveInfo drive = new(Path.GetPathRoot(pathToFiles)!);

            return Task.FromResult(drive.TotalFreeSpace >= size);
        }

        public Task<bool> ContainsFileWithId(string fileId)
        {
            return Task.FromResult(File.Exists(Path.Combine(pathToFiles, fileId)));
        }

        public Task DeleteAsync(string fileId)
        {
            File.Delete(Path.Combine(pathToFiles, fileId));

            return Task.CompletedTask;
        }

        public Task<List<string>> GetAllFileIds()
        {
            var files = Directory.GetFiles(pathToFiles);

            return Task.FromResult(files.Select(filePath => Path.GetFileName(filePath)).ToList());
        }

        public async Task<byte[]> GetFileChunk(string fileId, long startWith, long size)
        {
            int bytesRead;
            int allbytes = 0;
            var fileStream = await GetFileStream(fileId, FileMode.Open);

            byte[] buffer = new byte[4096];

            using var memorystream = new MemoryStream();

            fileStream.Position = startWith;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0 && allbytes < size)
            {
                memorystream.Write(buffer, 0, bytesRead);

                allbytes += bytesRead;
            }

            fileStream.Close();

            return memorystream.ToArray();
        }

        public async Task<FileStream> GetFileStream(string fileId, FileMode fileMode)
        {
            if (!await ContainsFileWithId(fileId))
            {
                File.Create(Path.Combine(pathToFiles, fileId)).Close();
            }

            var pathToFile = Path.Combine(pathToFiles, fileId);

            return File.Open(pathToFile, fileMode);
        }

        public async Task SaveData(string hash, Stream stream)
        {
            int bytesRead;
            var fileStream = await GetFileStream(hash, FileMode.Append);

            var length = (int)fileStream.Length;

            byte[] buffer = new byte[4096];

            fileStream.Position = length;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }

            stream.Close();

            fileStream.Close();
        }
    }
}
