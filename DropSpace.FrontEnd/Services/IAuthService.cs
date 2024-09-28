using DropSpace.Contracts.Models;
using Refit;
namespace DropSpace.FrontEnd.Services
{
    public interface IAuthService
    {
        [Post("/Auth/")]
        Task<string> Login(LoginModel model);

        [Post("/Auth/OneTime")]
        Task<string> OneTimeRegister();

        [Post("/Auth/Register")]
        Task Register(RegisterModel model);

        [Get("/Auth/")]
        Task<string> Refresh();

        [Delete("/Auth/")]
        Task<string> Leave();
    }
}