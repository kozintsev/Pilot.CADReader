using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Pilot.SDK.CadReader;

namespace Pilot.CadReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestChecksum
    {
        [TestMethod]
        public void TestChecksumByPiltFile()
        {
            const string md5 = "f167241fffd9dbcea4ab8f08455a2af1";
            const string path = @"Spc.spw";
            var isFile = File.Exists(path);
            Assert.IsTrue(isFile, "File not found");
            var md5Calculated = CalculatorMd5Checksum.Go(path);
            Assert.IsTrue(md5Calculated.ToLower() == md5, "Invalid algorithm Calculator Md5 Checksum");
        }

    }
}
