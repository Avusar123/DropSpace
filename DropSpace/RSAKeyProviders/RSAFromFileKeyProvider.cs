using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace DropSpace.RSAKeyProviders
{
    public class RSAFromFileKeyProvider(IConfiguration configuration) : IRSAKeyProvider
    {
        public RsaSecurityKey GetKey()
        {
            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(
                File.ReadAllBytes(configuration.GetValue<string>("RSALocation")!),
                out _
            );

            return new RsaSecurityKey(rsa);
        }
    }
}
