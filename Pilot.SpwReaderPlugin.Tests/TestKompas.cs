using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Uln.KompasShell;

namespace Pilot.SpwReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestKompas
    {
        public string StartupPath = Directory.GetParent(@"./").FullName;
        public const string PilotPrinterFolder = @"C:\ProgramData\ASCON\Pilot_Print";

        [TestMethod]
        public void TestPrintXpsFile()
        {
            using (var kompas = new KomapsShell())
            {
                const string path = @"\Spc.spw";
                kompas.InitKompas(out var result);
                kompas.PrintToXps(StartupPath + path);
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            var files = Directory.GetFiles(PilotPrinterFolder);
            var isFile = files.Any(file => file.Contains(".xps"));
            Assert.IsTrue(isFile, "Tmp xps file not found");
            //var length = new FileInfo(tempXps).Length;
            //Assert.IsTrue(length > 0, "Error empty file");
        }

        [TestMethod]
        public void TestConvertToPdfFile()
        {
            using (var kompas = new KomapsShell())
            {
                const string path = @"\Spc.spw";
                kompas.InitKompas(out var result);
                kompas.ConvertToPdf(StartupPath + path, StartupPath + @"\spc.pdf", out result);
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            Assert.IsTrue(File.Exists(StartupPath + @"\spc.pdf"), "Pdf file not found");
        }
    }
}
