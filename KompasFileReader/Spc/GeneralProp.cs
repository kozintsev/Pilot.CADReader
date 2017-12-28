using System;
using System.Collections.Generic;
using System.Linq;

namespace KompasFileReader.Spc
{
    public abstract class GeneralProp
    {
        /// <summary>
        /// Guid from Pilot ICE
        /// </summary>
        public Guid GlobalId;
        /// <summary>
        /// Путь к PDF или XPS документу со вторичным представлением
        /// </summary>
        public string PreviewDocument { get; set; }

        public List<SpcProp> ListSpcProps { get; set; }

        public string Name
        {
            get
            {
                var spcProp = ListSpcProps.FirstOrDefault(prop => prop.Name == "Наименование");
                return spcProp?.Value;
            }
        }

        public string Designation
        {
            get
            {
                var spcProp = ListSpcProps.FirstOrDefault(prop => prop.Name == "Обозначение");
                return spcProp?.Value;
            }
        }

    }
}
