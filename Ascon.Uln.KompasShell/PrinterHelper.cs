using System.Linq;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace Ascon.Uln.KompasShell
{
    public class PrinterHelper
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDefaultPrinter(string name);

        public static string GetDefaultPrinterName()
        {
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
            return printers.FirstOrDefault(t => new PrinterSettings() {PrinterName = t}.IsDefaultPrinter);
        }

        public static void SetPilotXpDefault()
        {
            const string pilotXps = "Pilot XPS";
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();

            foreach (var printer in printers)
            {
                if (printer.Equals(pilotXps))
                {
                    //var p = new PrinterSettings {PrinterName = pilotXps};
                    SetDefaultPrinter(pilotXps);
                    return;
                }
            }
        }


    }
}
