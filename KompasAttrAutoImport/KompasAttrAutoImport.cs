using System.IO;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Ascon.Pilot.SDK.ObjectCard;
using KompasFileReader.Analyzer;
using KompasFileReader.Model;

// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.KompasAttrAutoImport
{
    [Export(typeof(IAutoimportHandler))]
    [Export(typeof(IObjectCardHandler))]
    public class KompasAttrAutoImport : IAutoimportHandler, IObjectCardHandler
    {
        private IGeneralDocEntity _doc;
        private const string SOURCE_DOC_EXT = ".cdw";
        private const string SPW_EXT = "*.spw";

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
                        _doc = spc;
                    }
                }

                if (IsFileExtension(sourceFilePath, SOURCE_DOC_EXT))
                {
                    var taskOpenCdwFile = new Task<CdwAnalyzer>(() => new CdwAnalyzer(ms));
                    taskOpenCdwFile.Start();
                    taskOpenCdwFile.Wait();
                    if (taskOpenCdwFile.Result.IsCompleted)
                    {
                        var drawing = taskOpenCdwFile.Result.Drawing;
                        _doc = drawing;
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
            var des = _doc.GetDesignation();
            var name = _doc.GetName();
            modifier.SetValue("mark", ValueTextClear(des));
            modifier.SetValue("name", name);
            return true;
        }

        public bool OnValueChanged(IAttribute sender, AttributeValueChangedEventArgs args, IAttributeModifier modifier)
        {
            return false;
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
    }
}
