using Ionic.Zip;
using System;

using System.IO;

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
            set
            {
                outputMemStream = value;
            }
        }

        public bool IsZip(string archiveFilenameIn)
        {
            return ZipFile.IsZipFile(archiveFilenameIn);
        }

        public void ExtractZipToMemoryStream(string archiveFilenameIn, string fileInArchive)
        {
            var ms = new MemoryStream();
            try
            {
                ZipFile zip = ZipFile.Read(archiveFilenameIn);
                foreach (ZipEntry entry in zip)
                {
                    if (entry.FileName == fileInArchive)
                    {
                        entry.Extract(ms);  // extract uncompressed content into a memorystream
                        // the application can now access the MemoryStream here
                        outputMemStream = ms;
                    }
                               
                }
            }
            catch
            {
                outputMemStream = null;
            }           
        }
    }
}
