﻿using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Uln.KompasShell;


namespace Ascon.Pilot.SDK.SpwReader
{
    [Export(typeof(IMainMenu))]
    [Export(typeof(IObjectContextMenu))]
    public class SpwReaderPlugin : IObjectContextMenu, IMainMenu
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IObjectModifier _objectModifier;
        private readonly IObjectsRepository _objectsRepository;
        private readonly IFileProvider _fileProvider;
        private readonly IEnumerable<IType> _pilotTypes;
        private const string ADD_INFORMATION_TO_PILOT = "ADD_INFORMATION_TO_PILOT";
        private const string ABOUT_PROGRAM_MENU = "ABOUT_PROGRAM_MENU";
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected;
        // задача для открытия и анализа файла спецификации
        private Task<SpwAnalyzer> _taskOpenSpwFile;
        // список объктов спецификации полученных в ходе парсинга
        private List<SpcObject> _listSpcObject;
        private KomapsShell _komaps;
        private bool _isKompasInit;

        [ImportingConstructor]
        public SpwReaderPlugin(IObjectModifier modifier, IObjectsRepository repository, IPersonalSettings personalSettings, IFileProvider fileProvider)
        {
            _objectModifier = modifier;
            _objectsRepository = repository;
            _fileProvider = fileProvider;
            _pilotTypes = _objectsRepository.GetTypes();
        }

        public void BuildMenu(IMenuHost menuHost)
        {
            var menuItem = menuHost.GetItems().First();
            menuHost.AddSubItem(menuItem, ABOUT_PROGRAM_MENU, "О интеграции с КОМПАС", null, 0);
            menuHost.AddItem(ABOUT_PROGRAM_MENU, "О интеграции с КОМПАС", null, 1);
        }

        public void OnMenuItemClick(string itemName)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (itemName)
            {
                case ADD_INFORMATION_TO_PILOT:
                    SetInformationOnMenuClick(_selected);
                    break;
                case ABOUT_PROGRAM_MENU:
                    new MessageBox().Show();
                    break;
            }
        }

        public void BuildContextMenu(IMenuHost menuHost, IEnumerable<IDataObject> selection, bool isContext)
        {
            if (isContext)
                return;

            var dataObjects = selection.ToArray();
            if (dataObjects.Count() != 1)
                return;

            var itemNames = menuHost.GetItems().ToList();
            const string indexItemName = "SetInfo";
            var insertIndex = itemNames.IndexOf(indexItemName) + 1;

            _selected = dataObjects.FirstOrDefault();
            if (_selected == null)
                return;
            if (!CheckObjectsType(_selected))
                return; ;

            var icon = IconLoader.GetIcon(@"/Resources/menu_icon.svg");
            menuHost.AddItem(ADD_INFORMATION_TO_PILOT, "Д_обавить информацию из спецификации", icon, insertIndex);
        }
        /// <summary>
        /// Очистка строки полученной из спецификации от служибных символов: $| и @/
        /// </summary>
        /// <param name="str">Строка которую нужно очистить</param>
        /// <returns>Очищенная строка</returns>
        private static string ValueTextClear(string str)
        {
            return str.Replace("$|", "").Replace(" @/", " ").Replace("@/", " ");
        }

        /// <summary>
        /// "Умный" поиск секции в спецификации, возвращает True если передана название секции спецификации по ГОСТ
        /// </summary>
        /// <param name="sectionName">Наименование секции в спецификации, например: Сборочные единицы</param>
        /// <param name="pattern">Шаблон для поиска секции</param>
        /// <returns>True or false</returns>
        private static bool ParsingSectionName(string sectionName, string pattern)
        {
            sectionName = sectionName.ToLower();
            return sectionName.Contains(pattern);
        }

        private static bool IsFileExtension(string name, string ext)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var theExt = Path.GetExtension(name).ToLower();
            return theExt == ext;
        }

        private bool CheckObjectsType(IDataObject selected)
        {
            if (selected.Type.Name != "document") return false;
            var parent = _objectsRepository.GetCachedObject(selected.ParentId);
            return parent.Type.Name == "assembly";
        }

        private void SetInformationOnMenuClick(IDataObject selected)
        {
            if (!CheckObjectsType(selected))
                return;
            var parent = _objectsRepository.GetCachedObject(selected.ParentId);
            var file = GetFileFromPilotStorage(selected);
            if (file == null) return;
            var info = GetInformationByKompas(file);
            if (info == null) return;
            if (!info.Result.IsCompleted) return;
            _komaps = new KomapsShell();
            string message;
            _isKompasInit = _komaps.InitKompas(out message);
            if (!_isKompasInit) Logger.Error(message);
            AddInformationToPilot(parent);
            _komaps.ExitKompas();
        }

        private IFile GetFileFromPilotStorage(IDataObject selected)
        {
            if (selected == null)
                return null;
            var obj = _objectsRepository.GetCachedObject(selected.RelatedSourceFiles.FirstOrDefault());
            var file = obj.Files.FirstOrDefault(f => IsFileExtension(f.Name, ".spw"));
            return file;
        }

        private Task<SpwAnalyzer> GetInformationByKompas(IFile file)
        {
            var inputStream = _fileProvider.OpenRead(file);
            if (inputStream == null)
                return null;
            if (!_fileProvider.Exists(file.Id))
                return null;
            var ms = new MemoryStream();
            inputStream.Seek(0, SeekOrigin.Begin);
            inputStream.CopyTo(ms);
            ms.Position = 0;

            _taskOpenSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(ms));
            _taskOpenSpwFile.Start();
            _taskOpenSpwFile.Wait();
            if (!_taskOpenSpwFile.Result.IsCompleted)
                return null;
            _listSpcObject = _taskOpenSpwFile.Result.GetListSpcObject;
            return _taskOpenSpwFile;
        }

        private void SynchronizeCheck(IDataObject parent)
        {
            bool isName = false, isMark = false;
            var children = parent.TypesByChildren;
            foreach (var obj in children)
            {
                var currentObj = _objectsRepository.GetCachedObject(obj.Key);
                var attrName = string.Empty;
                var attrMark = string.Empty;
                foreach (var a in currentObj.Attributes)
                {
                    if (a.Key == "name")
                        attrName = a.Value.ToString();
                    if (a.Key == "mark")
                        attrMark = a.Value.ToString();

                }
                if (string.IsNullOrEmpty(attrName) && string.IsNullOrEmpty(attrMark))
                    return;
                foreach (var spcObj in _listSpcObject)
                {
                    foreach (var column in spcObj.Columns.Select(col => ValueTextClear(col.Value)))
                    {
                        if (column == attrName)
                            isName = true;
                        if (column == attrMark)
                            isMark = true;
                    }
                    if (isName && isMark)
                    {
                        spcObj.IsSynchronized = true;
                        spcObj.GlobalId = obj.Key;
                    }

                }
            }
        }

        private void AddPdfFileToPilotObject(IObjectBuilder builder, string fileName)
        {         
            if (!_isKompasInit) return;
            if (!File.Exists(fileName)) return;
            var pdfFile = Path.GetTempFileName() + ".pdf";
            string message;
            var isConvert = _komaps.ConvertToPdf(fileName, pdfFile, out message);
            if (!isConvert)
            {
                Logger.Error(message);
                return;
            }
            builder.AddFile(pdfFile);
        }

        private void AddInformationToPilot(IDataObject parent)
        {
            SynchronizeCheck(parent);
            foreach (var spcObject in _listSpcObject)
            {
                if (string.IsNullOrEmpty(spcObject.SectionName)) continue;
                var t = GetTypeBySectionName(spcObject.SectionName);
                if (t == null || spcObject.IsSynchronized) continue;
                var builder = _objectModifier.Create(parent, t);
                foreach (var attr in spcObject.Columns)
                {
                    var val = attr.Value;
                    if (string.IsNullOrEmpty(attr.TypeName) || string.IsNullOrEmpty(val)) continue;
                    // очишаем значение от служебных символов и выражений
                    val = ValueTextClear(val);
                    // в качестве наименование передаётся внутренее имя (а не то которое отображается)
                    int i;
                    if (int.TryParse(val, out i))
                        builder.SetAttribute(attr.TypeName, i);
                    else
                        builder.SetAttribute(attr.TypeName, val);
                }
                spcObject.GlobalId = builder.DataObject.Id;
                var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, ".cdw"));
                if (doc != null)
                {
                    var fileName = doc.FileName;
                    string[] paths = { fileName };
                    var storageObjects = _objectsRepository.GetStorageObjects(paths);
                    var storageObject = storageObjects.FirstOrDefault();
                    if (storageObject != null)
                        builder.AddSourceFileRelation(storageObject.DataObject.Id);
                    AddPdfFileToPilotObject(builder, fileName);
                }
                _objectModifier.Apply();
            }
        }

        private IType GetTypeBySectionName(string sectionName)
        {
            // ReSharper disable once RedundantAssignment
            var title = string.Empty;
            foreach (var itype in _pilotTypes)
            {
                title = itype.Title;
                if (ParsingSectionName(sectionName, "документ") && title == "Документ")
                    return itype;
                if (ParsingSectionName(sectionName, "комплекс") && title == "Комплекс")
                    return itype;
                if (ParsingSectionName(sectionName, "сборочн") && title == "Сборочная единица")
                    return itype;
                if (ParsingSectionName(sectionName, "детал") && title == "Деталь")
                    return itype;
                if (ParsingSectionName(sectionName, "стандарт") && title == "Стандартное изделие")
                    return itype;
                if (ParsingSectionName(sectionName, "прочи") && title == "Прочее изделие")
                    return itype;
                if (ParsingSectionName(sectionName, "материал") && title == "Материал")
                    return itype;
                if (ParsingSectionName(sectionName, "комплект") && title == "Комплект")
                    return itype;
            }
            return null;
        }
    }
}