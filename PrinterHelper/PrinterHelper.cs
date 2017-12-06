using System.Linq;
using System.Drawing.Printing;

namespace PrinterHelper
{
    public class PrinterHelper
    {
        public static string GetDefaultPrinterName()
        {
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
            return printers.FirstOrDefault(t => new PrinterSettings() {PrinterName = t}.IsDefaultPrinter);
        }

        public static void SetPilotXpDefault()
        {
            const string PILOT_XPS = "Pilot XPS";
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();

            //foreach (var printers in PrinterSettings.InstalledPrinters)
            //{
            //    if (printers.ToString().Equals(PILOT_XPS))
            //    {
            //        printers.IsDefaultPrinter
            //    }
            //}
        }


    }
}
