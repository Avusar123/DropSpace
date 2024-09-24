using Microsoft.IdentityModel.Tokens;

namespace DropSpace.RSAKeyProviders
{
    public interface IRSAKeyProvider
    {
        RsaSecurityKey GetKey();
    }
}
