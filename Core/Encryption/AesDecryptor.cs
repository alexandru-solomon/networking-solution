using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Lithium.Encryption
{
    internal class AesDecryptor
    {
        public AesDecryptor()
        {
            aes = new AesCryptoServiceProvider();
            aes.GenerateKey();
            aes.GenerateIV();
            decryptedData = new MemoryStream();
            ICryptoTransform cryptoTransform = aes.CreateDecryptor();
            decryptStream = new CryptoStream(decryptedData, cryptoTransform, CryptoStreamMode.Read);
            binaryReader = new BinaryReader(decryptedData);
        }

        readonly AesCryptoServiceProvider aes;
        readonly MemoryStream decryptedData;
        readonly CryptoStream decryptStream;
        readonly BinaryReader binaryReader;

        public void Decrypt(float key, byte[] buffer)
        {
            aes.IV = binaryReader.ReadBytes(16);
            decryptStream.Read(buffer);
        }
    }
}
