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
        private XDocument xDoc;
        private bool opened;
        public bool Opened
        {
            get
            {
                return opened;
            }
        }

        public SpwAnalyzer(MemoryStream ms)
        {
            opened = false;
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
                Debug.WriteLine("SpwAnalyzer threw exception: " + ex.Message);
            }
        }
        public void Run()
        {
            if (xDoc == null)
                return;
            SpcSection spcSection;
            List<SpcSection> spcSections = new List<SpcSection>();

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
            List<SpcObject> listSpcObject = new List<SpcObject>();
            foreach (XElement e in spcObjects)
            {
                spcObject = new SpcObject();
                foreach(XElement o in e.Elements())
                {
                    //spcObject.Id = 
                    foreach (var attr in o.Attributes())
                    {
                        if (attr.Name == "id")
                        {
                            spcObject.Id = attr.Value;
                        }
                    }
                    foreach(XElement context in o.Elements())
                    {
                        if(context.Name.ToString() == "section")
                        {

                        }
                        if(context.Name.ToString() == "columnscolumns")
                        {

                        }
                    }
                }
                listSpcObject.Add(spcObject);
            }

            foreach (XNode node in xDoc.Nodes())
            {
                Debug.WriteLine(node);
            }
        }

       
    }
}
