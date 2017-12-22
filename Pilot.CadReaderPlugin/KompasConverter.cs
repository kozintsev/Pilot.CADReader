using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ascon.Pilot.SDK.CadReader.Spc;
using Ascon.Uln.KompasShell;
using NLog;
// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.CadReader
{
    internal class KompasConverter : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static string tmpXps;
        private static string PilotPrinterFolder;
        private readonly KomapsShell _komaps;
        private const string SOURCE_DOC_EXT = ".cdw";
        private const string PDF_EXT = ".pdf";
        private const string XPS_EXT = ".xps";

        public KompasConverter()
        {
            //c:\ProgramData\ASCON\Pilot_Print\tmp.xps
            PilotPrinterFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ASCON\\Pilot_Print\\");
            tmpXps = Path.Combine(PilotPrinterFolder, "tmp.xps");
            _komaps = new KomapsShell();
            var isKompasInit = _komaps.InitKompas(out var message);
            if (!isKompasInit) Logger.Error(message);
        }

        public void KompasConvertToPdf(List<SpcObject> listSpcObject)
        {
            if (listSpcObject == null) return;

            foreach (var spcObject in listSpcObject)
            {
                var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SOURCE_DOC_EXT));
                if (doc == null) continue;
                var fileName = doc.FileName;
                if (!File.Exists(fileName)) continue;
                var pdfFile = Path.GetTempFileName() + PDF_EXT;
                var isConvert = _komaps.ConvertToPdf(fileName, pdfFile, out var message);
                if (!isConvert)
                    continue;
                spcObject.PreviewDocument = pdfFile;
            }
        }

        public void KompasConvertToXps(List<Specification> listSpc)
        {
            if (listSpc == null) return;
            foreach (var spc in listSpc)
            {
                var fileName = spc.FileName;
                if (!File.Exists(fileName)) continue;
                    var isConvert = _komaps.PrintToXps(fileName);
                    if (!isConvert)
                    {
                        continue;
                    }
                    var xpsFile = Path.Combine(PilotPrinterFolder, Guid.NewGuid() + XPS_EXT);
                    Thread.Sleep(1000);
                    if (Utility.IsFileMove(tmpXps, xpsFile))
                        spc.PreviewDocument = xpsFile;
                    Utility.TryDeleteFile(tmpXps);
            }
        }
       
        private static bool IsFileExtension(string name, string ext)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var theExt = Path.GetExtension(name).ToLower();
            return theExt == ext;
        }

        public void Dispose()
        {
            _komaps.ExitKompas();
            _komaps.Dispose();
        }
    }
}
