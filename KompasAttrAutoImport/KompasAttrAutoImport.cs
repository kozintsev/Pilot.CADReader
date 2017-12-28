using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Ascon.Pilot.SDK.ObjectCard;
using KompasFileReader.Analyzer;
using KompasFileReader.Spc;

// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.KompasAttrAutoImport
{
    [Export(typeof(IAutoimportHandler))]
    [Export(typeof(IObjectCardHandler))]
    public class KompasAttrAutoImport : IAutoimportHandler, IObjectCardHandler
    {

        private readonly IObjectModifier _modifier;
        private readonly IPilotDialogService _dialogService;
        private const string SOURCE_DOC_EXT = ".cdw";
        private const string SPW_EXT = "*.spw";

        [ImportingConstructor]
        public KompasAttrAutoImport(IObjectModifier modifier, IPilotDialogService dialogService)
        {
            _modifier = modifier;
            _dialogService = dialogService;
        }


        public bool Handle(string filePath, string sourceFilePath, AutoimportSource autoimportSource)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath)) return false;
            // если исходный файл компас. Проверяем расширения.
            if (!IsFileExtension(sourceFilePath, SOURCE_DOC_EXT) && !IsFileExtension(sourceFilePath, SPW_EXT))
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
                    }
                }

                if (IsFileExtension(sourceFilePath, SOURCE_DOC_EXT))
                {
                    var taskOpenSpwFile = new Task<CdwAnalyzer>(() => new CdwAnalyzer(ms));
                    taskOpenSpwFile.Start();
                    taskOpenSpwFile.Wait();
                    if (taskOpenSpwFile.Result.IsCompleted)
                    {
                        var drawing = taskOpenSpwFile.Result.Drawing;
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

            var parent = context.Parent;
            var sourceAttr = context.DisplayAttributes.FirstOrDefault(a => a.Type == AttributeType.String);
            if (sourceAttr == null)
                return false;

            var sourceValue = parent.Attributes.FirstOrDefault(a => a.Key == sourceAttr.Name);
            if (sourceValue.Value == null)
                return false;

            var targetAttr = context.DisplayAttributes.FirstOrDefault(a => a.Type == AttributeType.String);
            if (targetAttr == null)
                return false;

            var valueToSet =
                $"Parent is {sourceValue.Value}; IsDocument:{context.Parent.Type.HasFiles}; Can be mount:{context.Parent.Type.IsMountable}";

            modifier.SetValue(targetAttr.Name, valueToSet);
            return true;
        }

        public bool OnValueChanged(IAttribute sender, AttributeValueChangedEventArgs args, IAttributeModifier modifier)
        {
            var currentAttributeValues = string.Empty;
            foreach (var displayAttribute in args.Context.DisplayAttributes)
            {
                currentAttributeValues += displayAttribute.Name == sender.Name
                    ? args.NewValue
                    : displayAttribute.Name + ": " + args.Context.AttributesValues[displayAttribute.Name] + Environment.NewLine;
            }

            if (args.Context.Type.Name == "Document" && sender.Name == "Sheet_number")
            {
                var newNameAttrValue = "Sheet no " + args.NewValue + "; " + (args.Context.EditiedObject == null ? " New object " : " Existed object");
                modifier.SetValue("Name", newNameAttrValue);
                return true;
            }

            return false;
        }

        private static string Localize(AutoimportSource autoimportSource)
        {
            switch (autoimportSource)
            {
                case AutoimportSource.Unknown:
                    return "Unknown";
                case AutoimportSource.PilotXps:
                    return "Pilot XPS printer";
                case AutoimportSource.UserFolder:
                    return "user auto-import directory";
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool IsFileExtension(string name, string ext)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var theExt = Path.GetExtension(name).ToLower();
            return theExt == ext;
        }

        /// <summary>
        /// Метод получает информацию из спецификации и возвращает экземпляр класса Specification
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <returns>Объект спецификации</returns>
        private static Specification GetInformationFromKompas(string filename)
        {
            using (var inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var ms = new MemoryStream();
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(ms);
                ms.Position = 0;
                var taskOpenSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(ms));
                taskOpenSpwFile.Start();
                taskOpenSpwFile.Wait();
                if (!taskOpenSpwFile.Result.IsCompleted)
                    return null;
                var spc = taskOpenSpwFile.Result.GetSpecification;
                spc.FileName = filename;
                return spc;
            }
        }
    }
}
