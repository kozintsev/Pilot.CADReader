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
            if (!printers.Any(printer => printer.Equals(pilotXps))) return;
            SetDefaultPrinter(pilotXps);
        }

        public static void SetMicrosoftXpDefault()
        {
            const string printerName = "Microsoft XPS Document Writer";
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
            if (!printers.Any(printer => printer.Equals(printerName))) return;
            SetDefaultPrinter(printerName);
        }


    }
}
