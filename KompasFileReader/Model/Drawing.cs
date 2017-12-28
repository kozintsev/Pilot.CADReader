using System;
using System.Collections.Generic;
using KompasFileReader.Spc;

namespace KompasFileReader.Model
{
    public class Drawing : GeneralProp, IGeneralDocEntity
    {
        public List<DrawingSheet> Sheets { get; set; }

        public Drawing()
        {
            Sheets = new List<DrawingSheet>();
        }

        public string GetName()
        {
            return Name;
        }

        public string GetDesignation()
        {
            return Designation;
        }

        public void SetGlobalId(Guid value)
        {
            GlobalId = value;
        }
    }
}
