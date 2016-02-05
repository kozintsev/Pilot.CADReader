using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Pilot.CADReader
{
    class IconLoader
    {
        public static byte[] GetIcon(string relativePath)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            Application.ResourceAssembly = assembly;
            var uri = new Uri(relativePath, UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            using (var memoryStream = new MemoryStream())
            {
                if (info == null)
                    return null;

                info.Stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
