using System;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
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

        // Extracts the file contained within a GZip to the target dir.
        // A GZip can contain only one file, which by default is named the same as the GZip except
        // without the extension.
        //
        public void ExtractGZipToMemoryStream(string gzipFileName, string fileInArchive)
        {

            // Use a 4K buffer. Any larger is a waste.    
            byte[] dataBuffer = new byte[4096];

            using (Stream fs = new FileStream(gzipFileName, FileMode.Open, FileAccess.Read))
            {
               
                using (GZipInputStream gzipStream = new GZipInputStream(fs))
                {
                    

                   
                }
            }
        }
        public void ExtractZipToMemoryStream(string archiveFilenameIn, string password, string fileInArchive)
        {
            ZipFile zf = null;
            try
            {
                outputMemStream = new MemoryStream();
                FileStream fs = File.OpenRead(archiveFilenameIn);
                //zf.UseZip64 = UseZip64.On;
                zf = new ZipFile(fs);
                
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.
                    if (entryFileName == fileInArchive) // "MetaInfo.xml"
                    {
                        byte[] buffer = new byte[4096];     // 4K is optimum
                        Stream zipStream = zf.GetInputStream(zipEntry);
                        zipStream.CopyTo(outputMemStream);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
    }
}
