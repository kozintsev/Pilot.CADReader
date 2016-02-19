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
        private XmlReader reader;
        private XDocument xDoc;
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

        public bool Opened
        {
            get
            {
                return opened;
            }
        }

        private bool opened = false;

        public SpwAnalyzer(MemoryStream ms)
        {
            xmlvalid = true;
            try
            {
                reader = XmlReader.Create(ms);
                if (reader != null)
                    xDoc = XDocument.Load(reader);
                opened = true;
            }
            catch(Exception ex)
            {
                xmlvalid = false;
                opened = false;
                Debug.WriteLine("SpwAnalyzer threw exception: " + ex.Message);
            }
        }
        public void Run()
        {
            if (xDoc == null)
                return;
            foreach (XNode node in xDoc.Nodes())
            {
                Debug.WriteLine(node);
            }
        }

       
    }
}
