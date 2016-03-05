﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Ascon.Pilot.SDK.CADReader
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
            ZFile z = new ZFile();
            if (z.IsZip(fileName))
            {
                z.ExtractFileToMemoryStream(fileName, "MetaInfo");
                LoadFromMemoryStream(z.OutputMemStream);
                if (Opened)
                {
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
            }
        }

        public SpwAnalyzer(Stream fileStream)
        {
            isCompleted = false;
            ZFile z = new ZFile();
            if (z.IsZip(fileStream))
            {
                z.ExtractFileToMemoryStream(fileStream, "MetaInfo");
                LoadFromMemoryStream(z.OutputMemStream);
                if (Opened)
                {
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
            }
        }


        public List<SpcObject> GetListSpcObject()
        {
            return listSpcObject;
        }

        public List<SpcSection> GetListSpcSection()
        {
            return spcSections;
        }

        public void RunParsingSpw()
        {
            if (xDoc == null)
                return;
            SpcSection spcSection;
            spcSections = new List<SpcSection>();

            IEnumerable<XElement> sections = xDoc.Descendants("section");
            bool isName = false, isNumber = false;
            foreach (XElement section in sections)
            {
                spcSection = new SpcSection();
                foreach (var attr in section.Attributes())
                {
                    if (attr.Name == "name")
                    {
                        spcSection.Name = attr.Value;
                        isName = true;
                    }
                        
                    if (attr.Name == "number")
                    {
                        string strnum = attr.Value;
                        int number = 0;
                        if (Int32.TryParse(strnum, out number))
                            spcSection.Number = number;
                        isNumber = true;
                    }   
                }
                if (isName && isNumber)
                    spcSections.Add(spcSection);
                isName = false;
                isNumber = false;
            }
            
            IEnumerable<XElement> spcObjects = xDoc.Descendants("spcObjects");
            SpcObject spcObject;
            listSpcObject = new List<SpcObject>();
            foreach (XElement e in spcObjects)
            {
                foreach(XElement o in e.Elements())
                {
                    spcObject = new SpcObject();
                    foreach (var attr in o.Attributes())
                        if (attr.Name == "id")
                            spcObject.Id = attr.Value;
                    
                    foreach(XElement context in o.Elements())
                    {
                        if(context.Name.ToString() == "section")
                        {
                            foreach (var attr in context.Attributes())
                            {
                                if (attr.Name == "number")
                                {
                                    string strnum = attr.Value;
                                    int number = 0;
                                    if (Int32.TryParse(strnum, out number))
                                        spcObject.SectionNumber = number;
                                }
                            }
                        }
                        if(context.Name.ToString() == "columns")
                        {
                            foreach (XElement column in context.Elements())
                            {
                                SpcColumn col = new SpcColumn();
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
                    }
                    // добавляем в список объект спецификации
                    listSpcObject.Add(spcObject);
                }
                // парсинг объектов завершён
            }
            // все циклы завершены
            // вызываем событие о завершении парсинга.
            isCompleted = true;
        }

        private void LoadFromMemoryStream(MemoryStream ms)
        {
            opened = false;
            isCompleted = false;
            try
            {
                long p = ms.Position;
                ms.Position = 0;
                var reader = new StreamReader(ms);
                string s = reader.ReadToEnd();
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
