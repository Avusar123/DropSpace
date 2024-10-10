using Microsoft.IdentityModel.Tokens;

namespace DropSpace.WebApi.Utils.Interfaces
{
    public interface IRSAKeyProvider
    {
        RsaSecurityKey GetOrCreateKey();
    }
}
