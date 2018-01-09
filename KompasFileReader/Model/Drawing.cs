using System;
using System.Collections.Generic;

namespace KompasFileReader.Model
{
    /// <summary>
    /// Класс чертежа
    /// </summary>
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

        public List<DocProp> GetProps()
        {
            return ListSpcProps;
        }

        public void SetGlobalId(Guid value)
        {
            GlobalId = value;
        }
    }
}
