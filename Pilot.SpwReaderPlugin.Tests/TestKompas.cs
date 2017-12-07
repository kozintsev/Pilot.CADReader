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

        public bool IsTmpFile()
        {
            const string tmpXps = @"C:\ProgramData\ASCON\Pilot_Print\tmp.xps";
            if (!File.Exists(tmpXps))
                return false;
            var length = new FileInfo(tmpXps).Length;
            return length > 0;
        }

        [TestMethod]
        public void TestPrintSpwToXpsFile()
        {
            using (var kompas = new KomapsShell())
            {
                const string path = @"\078.505.9.0100.00.SPW";
                kompas.InitKompas(out var result);
                kompas.PrintToXps(StartupPath + path);
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            var isFile = false;
            var files = Directory.GetFiles(PilotPrinterFolder);
            foreach (var file in files)
            {
                if (file.Contains(".xps"))
                {
                    var length = new FileInfo(file).Length;
                    if (length > 0)
                    {
                        isFile = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(isFile, "Tmp xps file not found");
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
