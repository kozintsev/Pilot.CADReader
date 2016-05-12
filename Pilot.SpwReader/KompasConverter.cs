using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ascon.Pilot.SDK.SpwReader.Spc;
using Ascon.Uln.KompasShell;
using NLog;
// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.SpwReader
{
    class KompasConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private KomapsShell _komaps;
        private bool _isKompasInit;
        private readonly List<SpcObject> _listSpcObject;
        private const string SOURCE_DOC_EXT = ".cdw";

        public KompasConverter(List<SpcObject> listSpcObject)
        {
            _listSpcObject = listSpcObject;
        }

        public void KompasConvertToPdf()
        {
            _komaps = new KomapsShell();
            string message;
            _isKompasInit = _komaps.InitKompas(out message);
            if (!_isKompasInit) Logger.Error(message);
            foreach (var spcObject in _listSpcObject)
            {
                var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SOURCE_DOC_EXT));
                if (doc == null) continue;
                var fileName = doc.FileName;
                if (!File.Exists(fileName)) continue;
                var pdfFile = Path.GetTempFileName() + ".pdf";
                var isConvert = _komaps.ConvertToPdf(fileName, pdfFile, out message);
                if (!isConvert)
                {
                    Logger.Error(message);
                    continue;
                }
                spcObject.PdfDocument = pdfFile;
            }
            _komaps.ExitKompas();
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
