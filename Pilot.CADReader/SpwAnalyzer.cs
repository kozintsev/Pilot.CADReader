using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Ascon.Pilot.SDK.SpwReader
{
    class SpwAnalyzer
    {
        private List<SpcSection> spcSections;
        private List<SpcObject> listSpcObject;
        private XDocument xDoc;
        private bool opened;
        public bool Opened
        {
            get
            {
                return opened;
            }
        }

        private bool isCompleted;
        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
        }

        public SpwAnalyzer(string fileName)
        {
            isCompleted = false;
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
                opened = false;
                isCompleted = false;
            }
        }

        public SpwAnalyzer(Stream fileStream)
        {
            isCompleted = false;
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
                opened = false;
                isCompleted = false;
            }
        }


        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public List<SpcObject> GetListSpcObject => listSpcObject;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public List<SpcSection> GetListSpcSection => spcSections;

        private void RunParsingSpw()
        {
            if (xDoc == null)
                return;
            spcSections = new List<SpcSection>();

            var sections = xDoc.Descendants("section");
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
                    spcSections.Add(spcSection);
                isName = false;
                isNumber = false;
            }

            var spcObjects = xDoc.Descendants("spcObjects");
            listSpcObject = new List<SpcObject>();
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
                                if (attr.Name == "number")
                                {
                                    var strnum = attr.Value;
                                    var number = 0;
                                    if (int.TryParse(strnum, out number))
                                        spcObject.SectionNumber = number;
                                }
                            }
                        }
                        if (context.Name.ToString() != "columns") continue;
                        foreach (var column in context.Elements())
                        {
                            var col = new SpcColumn();
                            foreach (var attr in column.Attributes())
                            {
                                if (attr.Name == "name")
                                    col.Name = attr.Value.ToString();
                                if (attr.Name == "typeName")
                                    col.TypeName = attr.Value.ToString();
                                if (attr.Name == "value")
                                    col.Value = attr.Value.ToString();
                            }
                            // добавляем колонку спецификации в объект
                            spcObject.Columns.Add(col);
                        }
                    }
                    // добавляем в список объект спецификации
                    listSpcObject.Add(spcObject);
                }
                // парсинг объектов завершён
            }
            // все циклы завершены
            JointSpcNameAndSpcObj();
            // вызываем событие о завершении парсинга.
            isCompleted = true;
        }

        private void JointSpcNameAndSpcObj()
        {
            foreach (var spcObject in listSpcObject)
            {
                // определяем наименование секции спецификации 
                foreach (var spcSection in spcSections.Where(spcSection => spcObject.SectionNumber == spcSection.Number))
                {
                    spcObject.SectionName = spcSection.Name;
                }
            }
        }

        private void LoadFromMemoryStream(MemoryStream ms)
        {
            opened = false;
            isCompleted = false;
            try
            {
                var p = ms.Position;
                ms.Position = 0;
                var reader = new StreamReader(ms);
                var s = reader.ReadToEnd();
                ms.Position = p;
                xDoc = XDocument.Parse(s);
                opened = true;
            }
            catch
            {
                opened = false;
                isCompleted = false;
            }
        }


    }
}
