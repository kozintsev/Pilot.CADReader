using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Ascon.Pilot.SDK.CADReader
{
    class SpwAnalyzer
    {
        private bool xmlvalid;
        public bool Xmlvalid
        {
            get
            {
                return xmlvalid;
            }
            set
            {
                xmlvalid = value;
            }
        }

        public SpwAnalyzer(MemoryStream ms)
        {
            xmlvalid = true;
            try
            {
                XmlReader reader = XmlReader.Create(ms);
                XDocument xDoc = XDocument.Load(reader);

            }
            catch
            {
                xmlvalid = false;
            }  
        }

       
    }
}
