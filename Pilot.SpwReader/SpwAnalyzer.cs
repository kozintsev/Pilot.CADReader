using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Ascon.Pilot.SDK.SpwReader
{
    class SpwAnalyzer
    {
        private List<SpcSection> _spcSections;
        private List<SpcObject> _listSpcObject;
        private XDocument _xDoc;

        public bool Opened { get; private set; }
        public bool IsCompleted { get; private set; }

        public SpwAnalyzer(string fileName)
        {
            IsCompleted = false;
            var z = new ZFile();
            if (!z.IsZip(fileName)) return;
            z.ExtractFileToMemoryStream(fileName, "MetaInfo");
            LoadFromMemoryStream(z.OutputMemStream);
            if (!Opened) return;
            try
            {
                RunParsingSpw();
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
        }

        public SpwAnalyzer(Stream fileStream)
        {
            IsCompleted = false;
            var z = new ZFile();
            if (!z.IsZip(fileStream)) return;
            z.ExtractFileToMemoryStream(fileStream, "MetaInfo");
            LoadFromMemoryStream(z.OutputMemStream);
            if (!Opened) return;
            try
            {
                RunParsingSpw();
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
        }


        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public List<SpcObject> GetListSpcObject => _listSpcObject;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public List<SpcSection> GetListSpcSection => _spcSections;

        private void RunParsingSpw()
        {
            if (_xDoc == null)
                return;
            _spcSections = new List<SpcSection>();

            var sections = _xDoc.Descendants("section");
            bool isName = false, isNumber = false;
            foreach (var section in sections)
            {
                var spcSection = new SpcSection();
                foreach (var attr in section.Attributes())
                {
                    if (attr.Name == "name")
                    {
                        spcSection.Name = attr.Value;
                        isName = true;
                    }
                    if (attr.Name != "number") continue;
                    var strnum = attr.Value;
                    var number = 0;
                    if (int.TryParse(strnum, out number))
                        spcSection.Number = number;
                    isNumber = true;
                }
                if (isName && isNumber)
                    _spcSections.Add(spcSection);
                isName = false;
                isNumber = false;
            }

            var spcObjects = _xDoc.Descendants("spcObjects");
            _listSpcObject = new List<SpcObject>();
            foreach (var e in spcObjects)
            {
                foreach (var o in e.Elements())
                {
                    var spcObject = new SpcObject();
                    foreach (var attr in o.Attributes().Where(attr => attr.Name == "id"))
                        spcObject.Id = attr.Value;

                    foreach (var context in o.Elements())
                    {
                        if (context.Name.ToString() == "section")
                        {
                            foreach (var attr in context.Attributes())
                            {
                                if (attr.Name != "number") continue;
                                var strnum = attr.Value;
                                int number;
                                if (int.TryParse(strnum, out number))
                                    spcObject.SectionNumber = number;
                            }
                        }
                        if (context.Name.ToString() != "columns") continue;
                        foreach (var column in context.Elements())
                        {
                            var col = new SpcColumn();
                            foreach (var attr in column.Attributes())
                            {
                                if (attr.Name == "name")
                                    col.Name = attr.Value;
                                if (attr.Name == "typeName")
                                    col.TypeName = attr.Value;
                                if (attr.Name == "value")
                                    col.Value = attr.Value;
                            }
                            // добавляем колонку спецификации в объект
                            spcObject.Columns.Add(col);
                        }
                    }
                    // добавляем в список объект спецификации
                    _listSpcObject.Add(spcObject);
                }
                // парсинг объектов завершён
            }
            // все циклы завершены
            JointSpcNameAndSpcObj();
            // вызываем событие о завершении парсинга.
            IsCompleted = true;
        }

        private void JointSpcNameAndSpcObj()
        {
            foreach (var spcObject in _listSpcObject)
            {
                // определяем наименование секции спецификации 
                foreach (var spcSection in _spcSections.Where(spcSection => spcObject.SectionNumber == spcSection.Number))
                {
                    spcObject.SectionName = spcSection.Name;
                }
            }
        }

        private void LoadFromMemoryStream(Stream ms)
        {
            Opened = false;
            IsCompleted = false;
            try
            {
                var p = ms.Position;
                ms.Position = 0;
                var reader = new StreamReader(ms);
                var s = reader.ReadToEnd();
                ms.Position = p;
                _xDoc = XDocument.Parse(s);
                Opened = true;
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
        }


    }
}
