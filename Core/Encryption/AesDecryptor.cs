using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Lithium.Encryption
{
    internal class AesDecryptor:IDisposable 
    {
        public AesDecryptor()
        {
            aes = new AesCryptoServiceProvider();
            aes.GenerateKey();
            aes.GenerateIV();
            decryptedStream = new MemoryStream();
            ICryptoTransform cryptoTransform = aes.CreateDecryptor();
            streamDecryptor = new CryptoStream(decryptedStream, cryptoTransform, CryptoStreamMode.Read);
            
            binaryReader = new BinaryReader(decryptedStream);
        }

        readonly AesCryptoServiceProvider aes;
        readonly MemoryStream decryptedStream;
        readonly CryptoStream streamDecryptor;
        readonly BinaryReader binaryReader;

        public void Decrypt(byte[] key, byte[] buffer)
        {
            aes.IV = binaryReader.ReadBytes(16);
            streamDecryptor.Read(buffer);
        }

        public void Dispose()
        {
            streamDecryptor.Dispose();
        }
    }
}
