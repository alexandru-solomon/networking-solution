using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Lithium.Encryption
{
    internal class AesEncryptor
    {
        public AesEncryptor()
        {
            aes = new AesCryptoServiceProvider();
            aes.GenerateKey();
            aes.GenerateIV();
            encryptedData = new MemoryStream();
            ICryptoTransform cryptoTransform = aes.CreateDecryptor();
            encryptStream = new CryptoStream(encryptedData, cryptoTransform, CryptoStreamMode.Write);
            binaryWriter = new BinaryWriter(encryptedData);
        }

        readonly AesCryptoServiceProvider aes;
        readonly MemoryStream encryptedData;
        readonly CryptoStream encryptStream;
        readonly BinaryWriter binaryWriter;

        public void Encrypt(byte[] buffer, int offset, int count)
        {
            aes.GenerateIV();
            binaryWriter.Write(aes.IV);
            encryptStream.Write(datagram, offset, count);
        }
        public void Decrypt(byte[] buffer)
        {
            byte[] IV = binaryReader.ReadBytes(16)
        }
    }
}
