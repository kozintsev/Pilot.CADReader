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
            IEnumerable<XElement> elements = xDoc.Descendants("spcObjects");
            foreach (XElement e in elements)
            {

            }

            foreach (XNode node in xDoc.Nodes())
            {
                Debug.WriteLine(node);
            }
        }

       
    }
}
