using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ascon.Pilot.SDK.CadReader.Spc;
using Ascon.Uln.KompasShell;
using NLog;
// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.CadReader
{
    internal class KompasConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static string tmpXps;
        private KomapsShell _komaps;
        private bool _isKompasInit;
        private readonly List<SpcObject> _listSpcObject;
        private const string SOURCE_DOC_EXT = ".cdw";
        private const string PDF_EXT = ".pdf";
        private const string XPS_EXT = ".xps";

        public KompasConverter(List<SpcObject> listSpcObject)
        {
            _listSpcObject = listSpcObject;
            //c:\ProgramData\ASCON\Pilot_Print\tmp.xps
            tmpXps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ASCON\\Pilot_Print\\tmp.xps");
        }

        public void KompasConvertToPdf()
        {
            KompasConvertTo(PDF_EXT);
        }

        public void KompasConvertToXps()
        {
            KompasConvertTo(XPS_EXT);
        }

        private void KompasConvertTo(string type)
        {
            using (_komaps = new KomapsShell())
            {
                _isKompasInit = _komaps.InitKompas(out var message);
                if (!_isKompasInit) Logger.Error(message);
                foreach (var spcObject in _listSpcObject)
                {
                    var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SOURCE_DOC_EXT));
                    if (doc == null) continue;
                    var fileName = doc.FileName;
                    if (!File.Exists(fileName)) continue;
                    if (type == PDF_EXT)
                    {
                        var pdfFile = Path.GetTempFileName() + PDF_EXT;
                        var isConvert = _komaps.ConvertToPdf(fileName, pdfFile, out message);
                        spcObject.PdfDocument = pdfFile;
                        if (!isConvert)
                        {
                            Logger.Error(message);
                            continue;
                        }
                    }
                    if (type == XPS_EXT)
                    {
                        var isConvert = _komaps.PrintToXps(fileName);
                        var xpsFile = Guid.NewGuid() + ".xps";
                        File.Move(tmpXps, xpsFile);
                        spcObject.XpsDocument = xpsFile;
                        File.Delete(tmpXps);
                        if (!isConvert)
                        {
                            Logger.Error(message);
                            continue;
                        }
                    }
                    
                }
                _komaps.ExitKompas();
            }
        }

        private static bool IsFileExtension(string name, string ext)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var theExt = Path.GetExtension(name).ToLower();
            return theExt == ext;
        }
    }
}
