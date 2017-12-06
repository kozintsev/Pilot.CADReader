using System;
using Kompas6Constants;
using KompasAPI7;

namespace Ascon.Uln.KompasShell
{
    public class PrinterSetting : IPrintJob_PrinterSettings
    {
        public bool InitPrinterSettings(string DeviceName, bool IsPortraitPage, int PaperSize, int PaperLength, int PaperWidth,
            int PaperSource)
        {
            throw new NotImplementedException();
        }

        public bool LoadPrinterConfig(string FileName)
        {
            throw new NotImplementedException();
        }

        public bool SavePrinterConfig(string FileName)
        {
            throw new NotImplementedException();
        }

        public string DeviceName { get; }
        public string Port { get; }
        public bool IsPortraitPage { get; set; }
        public int PaperSize { get; set; }
        public int PaperSource { get; set; }
        public int PaperLength { get; set; }
        public int PaperWidth { get; set; }
        public ksPrinterTypeEnum PrinterType { get; set; }
    }
}
