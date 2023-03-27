
using System.Security.Cryptography;
using System.IO;

namespace Lithium.Encryption
{
    internal class AesEncryptor
    {
        public AesEncryptor()
        {
            AesService = new AesCryptoServiceProvider();
            AesService.GenerateKey();

            memoryStream = new MemoryStream();
            cryptoStream = new CryptoStream(memoryStream, AesService.CreateDecryptor(), CryptoStreamMode.Write);
        }

        readonly AesCryptoServiceProvider AesService;
        readonly MemoryStream memoryStream;
        readonly CryptoStream cryptoStream;

        public void Encrypt(byte[] buffer, int offset, int size, out byte[] cypherBuffer, int cypherOffset, out int cypherSize)
        {
            memoryStream.Position = cypherOffset;
            memoryStream.SetLength(cypherOffset);

            AesService.GenerateIV();
            memoryStream.Write(AesService.IV);
            cryptoStream.Write(buffer, offset, size);

            cypherSize = (int)memoryStream.Length-cypherOffset;
            cypherBuffer = memoryStream.GetBuffer();
        }
    }
}
