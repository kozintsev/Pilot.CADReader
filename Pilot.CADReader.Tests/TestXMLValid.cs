using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace Pilot.CADReader.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestXMLValid
    {
        [TestMethod]
        public void LoadFromFileTest()
        {
            try
            {
                XmlReader reader = XmlReader.Create("MetaInfo.xml");
                XDocument xDoc = XDocument.Load(reader);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SpwAnalyzer threw exception: " + ex.Message);
            }
        }


    }
}
