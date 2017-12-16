using System;
using System.IO;
using System.Security.Cryptography;

namespace Ascon.Pilot.SDK.CadReader
{
    static class CalculatorMd5Checksum
    {
        public static string Go(string path)
        {
            if (path == null)
                return null;
            try
            {
                if (!File.Exists(path))
                    return null;
                var fs = File.OpenRead(path);
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    var fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, (int)fs.Length);
                    var checkSum = md5.ComputeHash(fileData);
                    var result = BitConverter.ToString(checkSum).Replace("-", string.Empty).ToLower();
                    return result;
                }
            }
            catch (Exception)
            {

                return null;
            }
            
        }
    }
}
