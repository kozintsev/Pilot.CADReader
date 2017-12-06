// Для работы с API Компас Разработал Козинцев О.В. 2013 - 2017
using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
// Библиотеки подключаются из каталога c:\Program Files\ASCON\KOMPAS-3D V16\SDK\C#\Common\
using Kompas6API5;
using Kompas6Constants;
using KompasAPI7;
using Pdf2d_LIBRARY;



namespace Ascon.Uln.KompasShell
{
    public class KomapsShell : IDisposable
    {
        #region Custom declarations
        private ksDocument2D _doc2D;
        private _Application _kompasApp;
        private KompasObject _kompasObj;
        #endregion

        public List<string> Log;

        public KomapsShell()
        {
            Log = new List<string>();
            Log.Clear();
        }

        public bool InitKompas(out string result)
        {
            result = string.Empty;
            try
            {    	
                var t = Type.GetTypeFromProgID("KOMPAS.Application.5");
                _kompasObj = (KompasObject)Activator.CreateInstance(t);               
                _kompasApp = (_Application)_kompasObj.ksGetApplication7();                
                if (_kompasApp == null)
                    return false;
            }
            catch
            {
                result = "Error: Компас не установлен";
                AddLog(result);
                return false;
            }
            result = string.Empty;
            return true;
        }
        /// <summary>
        ///Завершение работы с Компас 
        /// </summary>   
        public void ExitKompas()
        {
            if (_kompasObj == null) return;
            try
            {
                _kompasObj.Quit();
            }
            finally
            {
                Marshal.ReleaseComObject(_kompasObj);
            }
        }

        public bool OpenFileKompas(string fileName, out string result)
        {
            var fileopen = false;
            if (_kompasObj != null)
            {
                _doc2D = (ksDocument2D)_kompasObj.Document2D();
                if (_doc2D != null) fileopen = _doc2D.ksOpenDocument(fileName, false);
                if (!fileopen)
                {
                    result = "Error: Не могу открыть файл";
                    AddLog(result);
                    return false;
                }

                _kompasObj.Visible = true;
                var err = _kompasObj.ksReturnResult();
                if (err != 0) _kompasObj.ksResultNULL();

            }
            else
            {
                result = "Warning: Подключение с Компасом не установлено";
                AddLog(result);
                return false;
            }
            result = string.Empty;
            return true;
        }
        
        public bool ConvertToPdf(string fileName, string outFileName, out string result)
        {
            result = string.Empty;
            #if DEBUG
                AddLog("Запускаем преобразование в PDF " + fileName);
            #endif
            string pathOfConverter = null;
            object valueOfRegistry = null;
            try
            {
                using (var registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).
                    OpenSubKey(@"SOFTWARE\ASCON\KOMPAS-3D\Converters\Pdf2d"))
                {
                    if (registryKey != null)
                    {
                        valueOfRegistry = registryKey.GetValue(@"Path");
                    }
                    if (valueOfRegistry == null)
                    {
                        using (var registryKey64 =
                            RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).
                                OpenSubKey(@"SOFTWARE\ASCON\KOMPAS-3D\Converters\Pdf2d"))
                        {
                            if (registryKey64 != null)
                            {
                                valueOfRegistry = registryKey64.GetValue(@"Path");
                            }
                            if (valueOfRegistry == null)
                            {
                                result = "Error: Не зарегистрирован модуль конвертера документов КОМПАС в формат PDF";
                                AddLog(result);
                                return false;
                            }
                        }
                    }
                    pathOfConverter = valueOfRegistry as string;
                }
            }
            catch
            {
                #if DEBUG
                    AddLog("Исключение при попытки получить путь к Pdf2d.dll из реестра");
                #endif
            }
            if (!File.Exists(pathOfConverter))
            {
                result = "Файл Pdf2d.dll не найден";
                AddLog(result);  
                return false;
            }
            try
            {
                IConverter converter = _kompasApp.Converter[pathOfConverter]; 
                if (converter != null)
                {
                    try
                    {
                        IPdf2dParam pdfPrm = converter.ConverterParameters(0);
                        if (pdfPrm != null)
                        {
                            pdfPrm.ColorType = 0;
                        }
                    }
                    catch
                    {
                        AddLog("Warning: Не возможно установить чёрно-белый формат");
                    }
                    var r = converter.Convert(fileName, outFileName, 0, false);
                if (r == 1) 
                        return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public bool PrintToXps(string fileName, string outFileName)
        {
            // fKompasPrint - TRUE исполь­зуем принтер Компас,
            // -FALSE - умол­чательный принтер Windows.
            var doc = _kompasApp.Documents.Open(fileName, true, true);
            _kompasApp.Visible = true;
            var sheets = doc.LayoutSheets;
            var c = sheets.Count;
            var t = sheets.Type;
            var printJob = _kompasApp.PrintJob;
            if (printJob != null)
            {
                var b = printJob.AddSheets(fileName, sheets, ksSheetsRangeEnum.ksAllSheets);
                printJob.ShowPreviewWindow();
                b = printJob.Execute("Pilot XPS");
            }

            var p = PrinterHelper.GetDefaultPrinterName();
            PrinterHelper.SetPilotXpDefault();
            var r = _kompasObj.ksPrintKompasDocumentEx(fileName, null, 1, false);
            PrinterHelper.SetDefaultPrinter(p);
            return r != 0;
        }

        

        #region Реализаця интерфейса IDisposable
        public void Dispose()
        {
            if (_kompasObj == null) return;
            Marshal.ReleaseComObject(_kompasObj);
            _kompasObj = null;
        }
        #endregion

        private void AddLog(string msg)
        {
            Log.Add(msg);
        }

    }
}
