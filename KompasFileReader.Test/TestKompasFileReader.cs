using System.IO;
using System.Threading.Tasks;
using KompasFileReader.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KompasFileReader.Test
{
    [TestClass]
    [DeploymentItem(@"TestSourceFiles\")]
    public class TestKompasFileReader
    {
        public string StartupPath = Directory.GetParent(@"./").FullName;

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
                    if (!drawing.Name.Contains("Макро")) // для файлов после конвертации из ранних версий компас
                        Assert.IsTrue(drawing.Name.Contains("Локальная сорбционная установка"), "Наименование не соответствует Локальная сорбционная установка");
                }
                else
                {
                    Assert.Fail("SpwAnalyzer has not result");
                }
            }
        }
        //Тест для проверки считывания Id спецификации. Если null, то тест не пройден.
        [TestMethod]
        public void TestCdwWithSpwReader()
        {
            const string path = @"\079.25.00.00.000.cdw";
            var fullPath = StartupPath + path;
            using (var inputStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                var ms = new MemoryStream();
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(ms);
                ms.Position = 0;
                var taskOpenCdwWithSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(ms));
                taskOpenCdwWithSpwFile.Start();
                taskOpenCdwWithSpwFile.Wait();
                if (!taskOpenCdwWithSpwFile.Result.IsCompleted)
                {
                    Assert.Fail("CdwWithSpwAnalyzer has not result");
                }
                var spc = taskOpenCdwWithSpwFile.Result.GetSpecification;
                Assert.IsTrue(taskOpenCdwWithSpwFile.Result.IdStyle == 1, "CdwWithSpwAnalyzer has not result");
            }
        }
    }
}
