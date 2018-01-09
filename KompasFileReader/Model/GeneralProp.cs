using System;
using System.Linq;
using System.Collections.Generic;

namespace KompasFileReader.Model
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

        public List<DocProp> ListSpcProps { get; set; }

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
