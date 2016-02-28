using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Ascon.Pilot.SDK.CADReader
{
    class SpwAnalyzer
    {
        //private XmlReader reader;
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

            set
            {
                isCompleted = value;
            }
        }

        

        public event EventHandler ParsingCompletedEvent;

        public List<SpcObject> GetListSpcObject()
        {
            return listSpcObject;
        }

        public List<SpcSection> GetListSpcSection()
        {
            return spcSections;
        }

        public SpwAnalyzer(MemoryStream ms)
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
            catch(Exception ex)
            {
                opened = false;
                isCompleted = false;
                Debug.WriteLine("SpwAnalyzer threw exception: " + ex.Message);
            }
        }
        public void Run()
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
                            SpcColumn col = new SpcColumn();
                            foreach (XElement column in context.Elements())
                            {
                                foreach (var attr in column.Attributes())
                                {
                                    if (attr.Name == "name")
                                        col.Name = attr.Value.ToString();
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
            ParsingCompletedEvent(this, null);
        }

       
    }
}
