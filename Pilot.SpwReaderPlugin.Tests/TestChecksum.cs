using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pilot.SpwReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestChecksum
    {
        public static string Go(string path)
        {
            var fs = File.OpenRead(path);
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                var fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                var checkSum = md5.ComputeHash(fileData);
                var result = BitConverter.ToString(checkSum).Replace("-", string.Empty);
                return result;
            }
        }
        [TestMethod]
        public void TestChecksumByPiltFile()
        {
            const string md5 = "f167241fffd9dbcea4ab8f08455a2af1";
            const string path = @"Spc.spw";
            var isFile = File.Exists(path);
            Assert.IsTrue(isFile, "File not found");
            if (!isFile) return;
            var md5Calculated = Go(path);
            //Assert.AreNotEqual(md5Calculated.ToLower(), md5, "Invalid algorithm Calculator Md5 Checksum");
            var b = (md5Calculated.ToLower() == md5);
            Assert.IsTrue(md5Calculated.ToLower() == md5, "Invalid algorithm Calculator Md5 Checksum");
        }

    }
}
