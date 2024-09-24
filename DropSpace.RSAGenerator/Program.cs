
using System.Security.Cryptography;

var rsaKey = RSA.Create().ExportRSAPrivateKey();

File.WriteAllBytes("key", rsaKey);