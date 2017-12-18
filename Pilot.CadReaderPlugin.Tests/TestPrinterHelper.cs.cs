using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Uln.KompasShell;

namespace Pilot.CadReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestPrinterHelper
    {
        [TestMethod]
        public void TestGerDefaultPrinter()
        {
            PrinterHelper.SetPilotXpDefault();
            var printerName = PrinterHelper.GetDefaultPrinterName();
            Assert.IsTrue(string.Equals(printerName, "Pilot XPS"), "Print XPS not found");
        }

    }
}
