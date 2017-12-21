using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.CadReader
{
    public static class Utility
    {
        public static bool FuzzySearchSubstring(string str, string search)
        {
            return search != null && str.ToLower().Contains(search.ToLower());
        }

        public static bool TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool IsFileMove(string sourceFileName, string destFileName)
        {
            try
            {
                File.Move(sourceFileName, destFileName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }
}
