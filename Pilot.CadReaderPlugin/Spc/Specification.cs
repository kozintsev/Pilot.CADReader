using System.Linq;
using System.Collections.Generic;
using Ascon.Pilot.SDK.CadReader.Model;
using System;

namespace Ascon.Pilot.SDK.CadReader.Spc
{
    internal class Specification : GeneralProp, IGeneralDocEntity
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
