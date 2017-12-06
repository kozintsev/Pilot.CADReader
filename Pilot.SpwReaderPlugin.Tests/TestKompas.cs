using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Uln.KompasShell;

namespace Pilot.SpwReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestKompas
    {
        [TestMethod]
        public void TestPrinterFile()
        {
            using (var kompas = new KomapsShell())
            {
                var startupPath = System.IO.Directory.GetParent(@"./").FullName;
                const string path = @"\Spc.spw";
                string result;
                kompas.InitKompas(out result);
                //kompas.PrintToXps(path, @"spc.xps");
                var b = File.Exists(path);
                if (File.Exists(path))
                    kompas.ConvertToPdf(startupPath + path, startupPath + @"\spc.pdf", out result);
            }
            
        }
    }
}
