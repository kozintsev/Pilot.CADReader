﻿using Ionic.Zip;
using System.IO;
using System.Linq;

namespace Ascon.Pilot.SDK.SpwReader
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


        public bool IsZip(Stream fileStream)
        {
            return ZipFile.IsZipFile(fileStream, false);
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