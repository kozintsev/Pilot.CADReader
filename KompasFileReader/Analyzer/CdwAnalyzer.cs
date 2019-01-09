using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using KompasFileReader.Model;

namespace KompasFileReader.Analyzer
{
    public class CdwAnalyzer
    {
        private XDocument _xDoc;

        public bool Opened { get; private set; }
        public bool IsCompleted { get; private set; }
        public List<DocProp> Prop { get; private set; }
        public Drawing Drawing { get; private set; }

        public CdwAnalyzer(Stream fileStream)
        {
            IsCompleted = false;
            var z = new ZFile();
            if (!z.IsZip(fileStream)) return;
            z.ExtractFileToMemoryStream(fileStream, "MetaInfo");
            LoadFromMemoryStream(z.OutputMemStream);
            if (!Opened) return;
            try
            {
                RunParseCdw();
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
        }

        private void RunParseCdw()
        {
            if (_xDoc == null)
                return;

            Drawing = new Drawing();
            Prop = new List<DocProp>();

            var properties = _xDoc.Descendants("property");
            foreach (var prop in properties)
            {
                string id = null, val = null, source = null;
                foreach (var attr in prop.Attributes())
                {
                    if (attr.Name == "id") id = attr.Value;
                    if (attr.Name == "value") val = attr.Value;
                    if (attr.Name == "source") source = attr.Value;
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
                    var spcProp = new DocProp
                    {
                        Name = name,
                        Value = val,
                        TypeValue = typeValue,
                        NatureId = natureId,
                        UnitId = unitId,
                        Source = source
                    };
                    Prop.Add(spcProp);
                }
            }

            var sheets = _xDoc.Descendants("sheets");
            foreach (var sheet in sheets)
            {
                var ds = new DrawingSheet();
                  // Возможная ошибка в строке  foreach (var attr in sheet.Attributes()). 
                //Внутрь цикла попасть не можем потому что мы не учли, что тип xml узла может быть не только XElement, но и XComment.
                /*
                foreach (var attr in sheet.Attributes())
                {
                    string strnum;
                    int number;
                    if (attr.Name == "format")
                    {
                        ds.Format = attr.Value;
                    }
                    if (attr.Name == "orientation")
                    {
                        strnum = attr.Value;
                        if (int.TryParse(strnum, out number))
                            ds.Orientation = number;
                    }
                    if (attr.Name == "height")
                    {
                        strnum = attr.Value;
                        if (int.TryParse(strnum, out number))
                            ds.Height = number;
                    }
                    if (attr.Name != "width") continue;
                    strnum = attr.Value;
                    if (int.TryParse(strnum, out number))
                        ds.Width = number;
                }*/
                // Код исправляющий ошибку:
                  foreach (XNode attrs in sheet.Nodes())
                {
                    XElement elm = attrs as XElement;
                    
                    if (elm != null)
                    {
                        string strnum;
                        int number;
                        foreach (XAttribute attr in elm.Attributes())
                        {
                            if (attr.Name == "format")
                                ds.Format = attr.Value;
                            if (attr.Name == "orientation")
                            {
                                strnum = attr.Value;
                                if (int.TryParse(strnum, out number))
                                    ds.Orientation = number;
                            }
                            if (attr.Name == "height")
                            {
                                strnum = attr.Value;
                                if (int.TryParse(strnum, out number))
                                    ds.Height = number;
                            }
                            if (attr.Name != "width") continue;
                            strnum = attr.Value;
                            if (int.TryParse(strnum, out number))
                                ds.Width = number;
                        }
                    }
                }
                Drawing.Sheets.Add(ds);
            }
            // все циклы завершены
            Drawing.ListSpcProps = Prop;
            IsCompleted = true;
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
