using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Uln.KompasShell;

namespace Pilot.SpwReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestPrinterHelper
    {
        [TestMethod]
        public void TestGerDefaultPrinter()
        {
            var printerName = PrinterHelper.GetDefaultPrinterName();
            PrinterHelper.SetPilotXpDefault();
            printerName = PrinterHelper.GetDefaultPrinterName();
            var b = string.Equals(printerName, "Pilot XPS");
            Assert.IsTrue(string.Equals(printerName, "Pilot XPS"), "Print XPS not found");
        }

    }
}
