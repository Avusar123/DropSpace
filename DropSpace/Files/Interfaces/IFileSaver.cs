namespace DropSpace.Files.Interfaces
{
    public interface IFileSaver
    {
        Task<bool> CanFit(long size);

        Task SaveData(string hash, Stream stream);

        Task<bool> ContainsFileWithHash(string hash);

        Task<bool> IsFileCompleted(string hash);

        Task<FileStream> GetFileStream(string hash, FileMode fileMode); 

        Task DeleteAsync(string hash);

        Task<List<string>> GetAllFilesHash();

        Task<DateTime> GetFileCreatedTime(string hash);
    }
}
