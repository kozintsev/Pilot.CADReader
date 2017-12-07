using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ascon.Pilot.SDK.SpwReader.Spc;

namespace Ascon.Pilot.SDK.SpwReader
{
    internal class SpwAnalyzer : Specification
    {
        private XDocument _xDoc;

        public bool Opened { get; private set; }
        public bool IsCompleted { get; private set; }

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

        public Specification GetSpecification
        {
            get
            {
                return this;
            }
        }

        private void RunParsingSpw()
        {
            if (_xDoc == null)
                return;
            SpcSections = new List<SpcSection>();
            ListSpcProps = new List<SpcProp>();
            ListSpcObjects = new List<SpcObject>();

            var properties = _xDoc.Descendants("property");
            foreach (var prop in properties)
            {
                string id = null, val = null;
                foreach (var attr in prop.Attributes())
                {
                    if (attr.Name == "id") id = attr.Value;   
                    if (attr.Name == "value") val = attr.Value;
                }
                var propertyDescriptions = _xDoc.Descendants("propertyDescription");
                foreach (var propertyDescription in propertyDescriptions)
                {
                    string id2 = null, name = null, typeValue = null, 
                        natureId = null, unitId = null;
                    foreach (var attr in propertyDescription.Attributes())
                    {
                        if (attr.Name == "id") id2 = attr.Value;               
                        if (attr.Name == "name") name = attr.Value;
                        if (attr.Name == "typeValue") typeValue = attr.Value;
                        if (attr.Name == "natureId") natureId = attr.Value;
                        if (attr.Name == "unitId") unitId = attr.Value;
                    }
                    if (id == null || id != id2) continue;
                    var spcProp = new SpcProp
                    {
                        Name = name,
                        Value = val,
                        TypeValue = typeValue,
                        NatureId = natureId,
                        UnitId = unitId
                    };
                    ListSpcProps.Add(spcProp);
                }
            }

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
                    if (int.TryParse(strnum, out var number))
                        spcSection.Number = number;
                    isNumber = true;
                }
                if (isName && isNumber)
                    SpcSections.Add(spcSection);
                isName = false;
                isNumber = false;
            }

            var spcObjects = _xDoc.Descendants("spcObjects");      
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
                            foreach (var strnum in from attr in context.Attributes() where attr.Name == "number" select attr.Value)
                            {
                                if (int.TryParse(strnum, out var number))
                                    spcObject.SectionNumber = number;
                            }
                        }
                        if (context.Name.ToString() == "columns")
                        {
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
                        if (context.Name.ToString() != "documents") continue;
                        foreach (var document in context.Elements())
                        {
                            var doc = new SpcDocument();
                            foreach (var attr in document.Attributes().Where(attr => attr.Name == "fileName"))
                            {
                                doc.FileName = attr.Value;
                            }
                            // добавить документы связанные с объектом спецификации
                            spcObject.Documents.Add(doc);
                        }
                    }
                    // добавляем в список объект спецификации
                    ListSpcObjects.Add(spcObject);
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
            foreach (var spcObject in ListSpcObjects)
            {
                // определяем наименование секции спецификации 
                var o = spcObject;
                foreach (var spcSection in SpcSections.Where(spcSection => o.SectionNumber == spcSection.Number))
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
