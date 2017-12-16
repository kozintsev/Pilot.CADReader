using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Uln.KompasShell;

namespace Pilot.CadReaderPlugin.Tests
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

        public bool IsXpsFile()
        {
            var files = Directory.GetFiles(PilotPrinterFolder);
            foreach (var file in files)
            {
                if (file.Contains(".xps"))
                {
                    var length = new FileInfo(file).Length;
                    if (length > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ClearFolder()
        {
            var files = Directory.GetFiles(PilotPrinterFolder);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [TestMethod]
        public void TestPrintSpwToXpsFile()
        {
            ClearFolder();
            using (var kompas = new KomapsShell())
            {
                const string path = @"\078.505.9.0100.00.SPW";
                kompas.InitKompas(out var result);
                kompas.PrintToXps(StartupPath + path);
                kompas.ExitKompas();
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            Assert.IsTrue(IsXpsFile(), "Tmp xps file not found");
        }

        [TestMethod]
        public void TestPrintCdwOneFormatToXpsFile()
        {
            ClearFolder();
            using (var kompas = new KomapsShell())
            {
                const string path = @"\078.505.0.0102.00.A3.CDW";
                kompas.InitKompas(out var result);
                kompas.PrintToXps(StartupPath + path);
                kompas.ExitKompas();
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            Assert.IsTrue(IsXpsFile(), "Tmp xps file not found");
        }

        [TestMethod]
        public void TestConvertToPdfFile()
        {
            using (var kompas = new KomapsShell())
            {
                const string path = @"\Spc.spw";
                kompas.InitKompas(out var result);
                kompas.ConvertToPdf(StartupPath + path, StartupPath + @"\spc.pdf", out result);
                kompas.ExitKompas();
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            Assert.IsTrue(File.Exists(StartupPath + @"\spc.pdf"), "Pdf file not found");
        }
    }
}
