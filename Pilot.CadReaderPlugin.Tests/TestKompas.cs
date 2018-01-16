using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ascon.Uln.KompasShell;
using KompasFileReader.Analyzer;

namespace Pilot.CadReaderPlugin.Tests
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestKompas
    {
        public string StartupPath = Directory.GetParent(@"./").FullName;
        public const string PilotPrinterFolder = @"C:\ProgramData\ASCON\Pilot_Print";

        public bool IsTmpFile()
        {
            const string tmpXps = @"C:\ProgramData\ASCON\Pilot_Print\tmp.xps";
            if (!File.Exists(tmpXps))
                return false;
            var length = new FileInfo(tmpXps).Length;
            return length > 0;
        }

        public bool IsXpsFile()
        {
            var files = Directory.GetFiles(PilotPrinterFolder);
            return (from file in files where file.Contains(".xps") select new FileInfo(file).Length).Any(length => length > 0);
        }

        public void ClearFolder()
        {
            var files = Directory.GetFiles(PilotPrinterFolder);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [TestMethod]
        public void TestPrintSpwToXpsFile()
        {
            ClearFolder();
            using (var kompas = new KomapsShell())
            {
                const string path = @"\078.505.9.0100.00.SPW";
                kompas.InitKompas(out var result);
                kompas.PrintToXps(StartupPath + path);
                kompas.ExitKompas();
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            Assert.IsTrue(IsXpsFile(), "Tmp xps file not found");
        }

        [TestMethod]
        public void TestPrintCdwOneFormatToXpsFile()
        {
            ClearFolder();
            using (var kompas = new KomapsShell())
            {
                const string path = @"\078.505.0.0102.00.A3.CDW";
                kompas.InitKompas(out var result);
                kompas.PrintToXps(StartupPath + path);
                kompas.ExitKompas();
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            Assert.IsTrue(IsXpsFile(), "Tmp xps file not found");
        }

        [TestMethod]
        public void TestConvertToPdfFile()
        {
            using (var kompas = new KomapsShell())
            {
                const string path = @"\Spc.spw";
                kompas.InitKompas(out var result);
                kompas.ConvertToPdf(StartupPath + path, StartupPath + @"\spc.pdf", out result);
                kompas.ExitKompas();
                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
            Assert.IsTrue(File.Exists(StartupPath + @"\spc.pdf"), "Pdf file not found");
        }

        [TestMethod]
        public void TestSpwReader()
        {
            const string path = @"\078.505.9.0100.00.SPW";
            var fullPath = StartupPath + path;
            using (var inputStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                var ms = new MemoryStream();
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(ms);
                ms.Position = 0;
                var taskOpenSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(ms));
                taskOpenSpwFile.Start();
                taskOpenSpwFile.Wait();
                if (!taskOpenSpwFile.Result.IsCompleted)
                {
                    Assert.Fail("SpwAnalyzer has not result");
                }
                var spc = taskOpenSpwFile.Result.GetSpecification;
                spc.FileName = fullPath;
                Assert.IsTrue(spc.Designation.Contains("078.505.9.0100.00"), "Designation is not equivalent to 078.505.9.0100.00");
            }
        }

        [TestMethod]
        public void TestCdwReader()
        {
            const string path = @"\078.505.0.0102.00.A3.CDW";
            var fullPath = StartupPath + path;
            using (var inputStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                var ms = new MemoryStream();
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(ms);
                ms.Position = 0;

                var taskOpenCdwFile = new Task<CdwAnalyzer>(() => new CdwAnalyzer(ms));
                taskOpenCdwFile.Start();
                taskOpenCdwFile.Wait();
                if (taskOpenCdwFile.Result.IsCompleted)
                {
                    var drawing = taskOpenCdwFile.Result.Drawing;
                    Assert.IsTrue(drawing.Designation.Contains("078.505.0.0102.00"), "Designation is not equivalent to 078.505.9.0100.00");
                }
                else
                {
                    Assert.Fail("SpwAnalyzer has not result");
                }
            }
        }

        [TestMethod]
        public void TestCdwReader2()
        {
            const string path = @"\6013-AR.cdw";
            var fullPath = StartupPath + path;
            using (var inputStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                var ms = new MemoryStream();
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(ms);
                ms.Position = 0;

                var taskOpenCdwFile = new Task<CdwAnalyzer>(() => new CdwAnalyzer(ms));
                taskOpenCdwFile.Start();
                taskOpenCdwFile.Wait();
                if (taskOpenCdwFile.Result.IsCompleted)
                {
                    var drawing = taskOpenCdwFile.Result.Drawing;
                    Assert.IsTrue(drawing.Designation.Contains("ЛСУ-6013-АР"), "Designation is not equivalent to ЛСУ-6013-АР");
                    Assert.IsTrue(drawing.Name.Contains("Локальная сорбционная установка"), "Наименование не соответствует Локальная сорбционная установка");
                }
                else
                {
                    Assert.Fail("SpwAnalyzer has not result");
                }
            }
        }
    }
}
