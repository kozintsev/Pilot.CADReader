using Ionic.Zip;
using System.IO;
using System.Linq;

namespace Ascon.Pilot.SDK.CADReader
{
    public class ZFile
    {
        private MemoryStream outputMemStream;
        public MemoryStream OutputMemStream
        {
            get
            {
                return outputMemStream;
            }
        }


        public bool IsZip(string archiveFilenameIn)
        {
            return ZipFile.IsZipFile(archiveFilenameIn);
        }

        public bool IsZip(Stream fileStream)
        {
            return ZipFile.IsZipFile(fileStream, false);
        }

        public void ExtractFileToMemoryStream(string archiveFilenameIn, string fileInArchive)
        {
            var ms = new MemoryStream();
            try
            {
                var zip = ZipFile.Read(archiveFilenameIn);
                foreach (var entry in zip.Where(entry => entry.FileName == fileInArchive))
                {
                    entry.Extract(ms);  // extract uncompressed content into a memorystream
                    // the application can now access the MemoryStream here
                    outputMemStream = ms;
                }
            }
            catch
            {
                outputMemStream = null;
            }           
        }

        public void ExtractFileToMemoryStream(Stream fileStream, string fileInArchive)
        {
            var ms = new MemoryStream();
            try
            {
                fileStream.Position = 0;
                var zip = ZipFile.Read(fileStream);
                foreach (var entry in zip.Where(entry => entry.FileName == fileInArchive))
                {
                    entry.Extract(ms);  // extract uncompressed content into a memorystream
                    // the application can now access the MemoryStream here
                    outputMemStream = ms;
                }
            }
            catch
            {
                outputMemStream = null;
            }
        }
    }
}
