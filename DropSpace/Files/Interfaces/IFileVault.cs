namespace DropSpace.Files.Interfaces
{
    public interface IFileVault
    {
        Task<bool> CanFit(long size);

        Task SaveData(string fileId, Stream stream);

        Task<byte[]> GetFileChunk(string fileId, long start, long size);

        Task<bool> ContainsFileWithId(string fileId);

        Task<FileStream> GetFileStream(string fileId, FileMode fileMode);

        Task DeleteAsync(string fileId);

        Task<List<string>> GetAllFileIds();
    }
}
