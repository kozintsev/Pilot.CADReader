using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Uln.KompasShell;

namespace Pilot.SpwReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestKompas
    {
        public string StartupPath = Directory.GetParent(@"./").FullName;

        [TestMethod]
        public void TestPrintXpsFile()
        {
            using (var kompas = new KomapsShell())
            {
                const string path = @"\Spc.spw";
                kompas.InitKompas(out var result);
                kompas.PrintToXps(StartupPath + path, @"spc.xps");
                Assert.IsTrue(File.Exists(StartupPath + @"\spc.xps"), "Xps file not found");
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            
        }

        [TestMethod]
        public void TestConvertToPdfFile()
        {
            using (var kompas = new KomapsShell())
            {
                const string path = @"\Spc.spw";
                kompas.InitKompas(out var result);
                kompas.ConvertToPdf(StartupPath + path, StartupPath + @"\spc.pdf", out result);
                Assert.IsTrue(File.Exists(StartupPath + @"\spc.pdf"), "Pdf file not found");
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }

        }
    }
}
