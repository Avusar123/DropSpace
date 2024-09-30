using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using Refit;

namespace DropSpace.FrontEnd.Services
{
    public interface ISessionService
    {
        [Post("/Session/")]
        Task<SessionDto> Create(CreateSessionModel createSessionModel);

        [Get("/Session/{id}")]
        Task<SessionDto> Get(Guid id);

        [Get("/Session/All")]
        Task<List<SessionDto>> GetAll();

        [Delete("/Session/{id}")]
        Task Leave(Guid id);
    }
}
