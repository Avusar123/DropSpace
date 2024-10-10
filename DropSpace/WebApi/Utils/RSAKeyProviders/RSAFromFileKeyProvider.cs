using DropSpace.WebApi.Utils.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace DropSpace.WebApi.Utils.RSAKeyProviders
{
    public class RSAFromFileKeyProvider(IConfiguration configuration) : IRSAKeyProvider
    {
        public RsaSecurityKey GetOrCreateKey()
        {
            var pathToRsa = Path.Combine(configuration.GetValue<string>("KeysFolder")!,
                configuration.GetValue<string>("RSAFileName")!);

            var rsa = RSA.Create();
            
            if (File.Exists(pathToRsa))
            {
                rsa.ImportRSAPrivateKey(
                    File.ReadAllBytes(pathToRsa),
                    out _
                );
            } else
            {
                var key = rsa.ExportRSAPrivateKey();

                File.WriteAllBytes(pathToRsa, key);
            }
            

            return new RsaSecurityKey(rsa);
        }
    }
}
