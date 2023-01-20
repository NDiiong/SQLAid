using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SQLAid.Addin.Security
{
    public class Encrypter : IDisposable
    {
        public event EncryptionStatusEvent EncryptionStatus;

        public event DecryptionStatusEvent DecryptionStatus;

        public delegate void EncryptionStatusEvent(long currentBytesRead, long totalBytes);

        public delegate void DecryptionStatusEvent(long currentBytesRead, long totalBytes);

        private readonly byte[] _salt = new byte[8] { 19, 20, 85, 4, 64, 61, 96, 93 };

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                GC.ReRegisterForFinalize(this);
                IsDisposed = true;
            }
        }

        public void DecryptFile(string OriginFullFileName, string DetinyFullFileName, string Password)
        {
            var stream = new FileStream(OriginFullFileName, FileMode.Open, FileAccess.Read);
            var totalBytes = stream.Length;
            var cryptoTransform = GetDecryptionKey(GetEncriptionPassword(Password));
            var cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Read);
            var fileStream = new FileStream(DetinyFullFileName, FileMode.Create, FileAccess.Write);
            var bytesBuffers = new byte[2048];
            var count = 1;
            var currentBytesRead = 0;
            while (count > 0)
            {
                count = cryptoStream.Read(bytesBuffers, 0, bytesBuffers.Length);
                fileStream.Write(bytesBuffers, 0, count);
                currentBytesRead += count;
                DecryptionStatus?.Invoke(currentBytesRead, totalBytes);
            }

            cryptoStream.Close();
            cryptoStream.Dispose();
            fileStream.Close();
            fileStream.Dispose();
        }

        public void EncryptFile(string OriginFullFileName, string DetinyFullFileName, string Password)
        {
            var stream = new FileStream(DetinyFullFileName, FileMode.Create, FileAccess.Write);
            var cryptoTransform = GetEncryptionKey(GetEncriptionPassword(Password));
            var cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Write);
            var fileStream = new FileStream(OriginFullFileName, FileMode.Open, FileAccess.Read);
            var bytesBuffers = new byte[2048];
            var currentBytesRead = 0;
            while (currentBytesRead < fileStream.Length)
            {
                var count = fileStream.Read(bytesBuffers, 0, bytesBuffers.Length);
                cryptoStream.Write(bytesBuffers, 0, count);
                currentBytesRead += count;
                EncryptionStatus?.Invoke(currentBytesRead, fileStream.Length);
            }

            fileStream.Close();
            fileStream.Dispose();
            cryptoStream.Close();
            cryptoStream.Dispose();
        }

        private byte[] GetEncriptionPassword(string Password)
        {
            SHA512 sha512Managed = null;
            try
            {
                sha512Managed = new SHA512Managed();
                return sha512Managed.ComputeHash(Encoding.UTF8.GetBytes(Password));
            }
            finally
            {
                sha512Managed?.Dispose();
            }
        }

        private ICryptoTransform GetDecryptionKey(byte[] Password)
        {
            var aesCryptoServiceProvider = new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CFB,
                Padding = PaddingMode.PKCS7
            };

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(Password, _salt, 65536))
            {
                aesCryptoServiceProvider.Key = rfc2898DeriveBytes.GetBytes(aesCryptoServiceProvider.KeySize / 8);
                aesCryptoServiceProvider.IV = rfc2898DeriveBytes.GetBytes(aesCryptoServiceProvider.BlockSize / 8);
            }

            return aesCryptoServiceProvider.CreateDecryptor();
        }

        private ICryptoTransform GetEncryptionKey(byte[] Password)
        {
            var aesCryptoServiceProvider = new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CFB,
                Padding = PaddingMode.PKCS7
            };

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(Password, _salt, 65536))
            {
                aesCryptoServiceProvider.Key = rfc2898DeriveBytes.GetBytes(aesCryptoServiceProvider.KeySize / 8);
                aesCryptoServiceProvider.IV = rfc2898DeriveBytes.GetBytes(aesCryptoServiceProvider.BlockSize / 8);
            }

            return aesCryptoServiceProvider.CreateEncryptor();
        }
    }
}