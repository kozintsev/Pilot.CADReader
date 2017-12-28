using System.Collections.Generic;

namespace KompasFileReader.Model
{
    public class Drawing : GeneralMechEntity
    {
        public List<DrawingSheet> Sheets { get; set; }

        public Drawing()
        {
            Sheets = new List<DrawingSheet>();
        }
    }
}
