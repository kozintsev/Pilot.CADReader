using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ascon.Pilot.SDK.KompasAttrAutoImport.Model;
using Ascon.Pilot.SDK.ObjectCard;
using KompasFileReader.Analyzer;
using KompasFileReader.Model;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.KompasAttrAutoImport
{
    [Export(typeof(IAutoimportHandler))]
    [Export(typeof(IObjectCardHandler))]
    [Export(typeof(ISettingsFeature))]
    public class KompasAttrAutoImport : IAutoimportHandler, IObjectCardHandler, ISettingsFeature
    {
        private IGeneralDocEntity _doc;
        private readonly List<PairPilotKompasAttr> _pairPilotKompasAttrs;
        private const string CDW_EXT = ".cdw";
        private const string SPW_EXT = ".spw";
        public string Key { get; }
        public string Title { get; }
        public FrameworkElement Editor { get; }

        [ImportingConstructor]
        public KompasAttrAutoImport(IPersonalSettings personalSettings)
        {
            Key = "KompasAttrAutoImport-E74EA6D5-C31E-4FE2-84E9-5AB64E503126";
            Title = "Автоимпорт атрибутов из КОМПАС-3D";
            Editor = null;
            _doc = null;
            var setting = new SettingLoader(personalSettings);
            _pairPilotKompasAttrs = GetListPairPilotKompasAttr(setting.Json);
        }

        public bool Handle(string filePath, string sourceFilePath, AutoimportSource autoimportSource)
        {
            _doc = null;
            if (string.IsNullOrWhiteSpace(sourceFilePath)) return false;
            // если исходный файл компас. Проверяем расширения.
            if (!IsFileExtension(sourceFilePath, CDW_EXT) && !IsFileExtension(sourceFilePath, SPW_EXT))
                return false;
            // Тут выполняем анализ документа и извлекаем из него информацию об обознаяении и наименовании
            using (var inputStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                var ms = new MemoryStream();
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(ms);
                ms.Position = 0;
                if (IsFileExtension(sourceFilePath, SPW_EXT))
                {
                    var taskOpenSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(ms));
                    taskOpenSpwFile.Start();
                    taskOpenSpwFile.Wait();
                    if (taskOpenSpwFile.Result.IsCompleted)
                    {
                        var spc = taskOpenSpwFile.Result.GetSpecification;
                        spc.FileName = sourceFilePath;
                        _doc = spc;
                    }
                    else
                    {
                        _doc = null;
                    }
                }

                if (IsFileExtension(sourceFilePath, CDW_EXT))
                {
                    var taskOpenCdwFile = new Task<CdwAnalyzer>(() => new CdwAnalyzer(ms));
                    taskOpenCdwFile.Start();
                    taskOpenCdwFile.Wait();
                    if (taskOpenCdwFile.Result.IsCompleted)
                    {
                        var drawing = taskOpenCdwFile.Result.Drawing;
                        _doc = drawing;
                    }
                    else
                    {
                        _doc = null;
                    }
                }
            }
            Thread.Sleep(2000);
            return false;

        }

        public bool Handle(IAttributeModifier modifier, ObjectCardContext context)
        {
            var isObjectModification = context.EditiedObject != null;
            if (isObjectModification || context.IsReadOnly)
                return false;
            if (_doc == null)
                return false;
            var docProp = _doc.GetProps();
            foreach (var pairPilotKompasAttr in _pairPilotKompasAttrs)
            {
                var val = docProp.FirstOrDefault(x => x.Name == pairPilotKompasAttr.NamePropKompas)?.Value;
                if (val != null)
                    modifier.SetValue(pairPilotKompasAttr.NameAttrPilot, ValueTextClear(val));
            }
            return true;
        }

        public bool OnValueChanged(IAttribute sender, AttributeValueChangedEventArgs args, IAttributeModifier modifier)
        {
            return false;
        }

        public void SetValueProvider(ISettingValueProvider settingValueProvider)
        {

        }

        private static bool IsFileExtension(string name, string ext)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var theExt = Path.GetExtension(name).ToLower();
            return theExt == ext;
        }

        /// <summary>
        /// Очистка строки полученной из спецификации от служибных символов: $| и @/
        /// </summary>
        /// <param name="str">Строка которую нужно очистить</param>
        /// <returns>Очищенная строка</returns>
        private static string ValueTextClear(string str)
        {
            return str?.Replace("$|", "").Replace(" @/", " ").Replace("@/", " ");
        }

        private static List<PairPilotKompasAttr> GetListPairPilotKompasAttr(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<PairPilotKompasAttr>>(json);

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
