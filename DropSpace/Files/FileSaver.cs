using DropSpace.Extensions;
using DropSpace.Files.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;

namespace DropSpace.Files
{
    public class FileSaver : IFileSaver
    {
        private readonly string pathToFiles;

        public FileSaver(IConfiguration configuration)
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
            DriveInfo drive = new(Path.GetPathRoot(pathToFiles)!);

            return Task.FromResult(drive.TotalFreeSpace >= size);
        }

        public Task<bool> ContainsFileWithHash(string hash)
        {
            return Task.FromResult(File.Exists(Path.Combine(pathToFiles, hash)));
        }

        public Task DeleteAsync(string hash)
        {
            File.Delete(Path.Combine(pathToFiles, hash));

            return Task.CompletedTask;
        }

        public Task<List<string>> GetAllFilesHash()
        {
            var files = Directory.GetFiles(pathToFiles);

            return Task.FromResult(files.Select(filePath => Path.GetFileName(filePath)).ToList());
        }

        public async Task<DateTime> GetFileCreatedTime(string hash)
        {
            if (!await ContainsFileWithHash(hash))
            {
                throw new NullReferenceException("Файл не найден!");
            }

            var fileInfo = new FileInfo(Path.Combine(pathToFiles, hash));

            return fileInfo.LastWriteTime;
        }

        public async Task<FileStream> GetFileStream(string hash, FileMode fileMode)
        {
            if (!await ContainsFileWithHash(hash))
            {
                File.Create(Path.Combine(pathToFiles, hash)).Close();
            }

            var pathToFile = Path.Combine(pathToFiles, hash);

            return File.Open(pathToFile, fileMode);
        }

        public async Task<bool> IsFileCompleted(string hash)
        {
            using var fileStream = await GetFileStream(hash, FileMode.Open);

            using SHA256 sha256 = SHA256.Create();

            byte[] buffer = new byte[4096];

            int bytesRead;

            // Читаем файл порциями и обновляем хэш
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
            }

            sha256.TransformFinalBlock(buffer, 0, 0);

            byte[] hashBytes = sha256.Hash!;

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower() == hash;
        }

        public async Task SaveData(string hash, Stream stream)
        {
            int bytesRead;
            using var fileStream = await GetFileStream(hash, FileMode.Append);
            var length = (int)fileStream.Length;

            byte[] buffer = new byte[4096]; // Размер буфера (4 КБ)

            fileStream.Position = length;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();

            stream.Close();
        }
    }
}
