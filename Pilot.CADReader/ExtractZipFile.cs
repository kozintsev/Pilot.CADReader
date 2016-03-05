using Ionic.Zip;
using System;
using System.Diagnostics;
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
            catch(Exception ex)
            {
                outputMemStream = null;
                Debug.WriteLine(ex.Message);
            }           
        }

        public void ExtractFileToMemoryStream(Stream fileStream, string fileInArchive)
        {
            var ms = new MemoryStream();
            try
            {
                fileStream.Position = 0;
                ZipFile zip = ZipFile.Read(fileStream);
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
            catch (Exception ex)
            {
                outputMemStream = null;
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
