using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;

namespace Pilot.CADReader.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestXMLValid
    {
        private string mediaInfoFile = "MetaInfo.xml";

        [TestMethod]
        public void LoadFromFileTest()
        {
            try
            {
                //var reader = XmlReader.Create(mediaInfoFile);
                //Assert.IsNull(reader as object);
                XDocument xDoc = XDocument.Load(mediaInfoFile);
                Assert.IsNull(xDoc as object);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SpwAnalyzer threw exception: " + ex.Message);
            }
        }
        [TestMethod]
        public void LoadFromMemoryStream()
        {
            bool b = File.Exists(mediaInfoFile);
            Assert.IsFalse(b, "File not found");  
            var fs = new FileStream(mediaInfoFile, FileMode.Open);
            Assert.IsNull(fs as object);
            //MemoryStream memStream = new MemoryStream();
            //memStream.WriteTo(fs);
        }


    }
}
