using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.SDK.CadReader.Spc
{
    internal class Specification : GeneralProp
    {
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
        
        public string FileName { get; set; }
        
        public Specification Children { get; set; }
        
        public Specification Parent { get; set; }

        public List<SpcObject> ListSpcObjects { get; protected set; }

        public List<SpcProp> ListSpcProps  { get; protected set; }

        public List<SpcSection> SpcSections { get; protected set; }

        public IFile File { get; set; }

    }
}
