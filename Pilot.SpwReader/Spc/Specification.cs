using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.SDK.SpwReader.Spc
{
    internal class Specification
    {
        public string Name
        {
            get
            {
                var spcProp = ListSpcProps.FirstOrDefault(prop => prop.Name == "Наименование");
                return spcProp != null ? spcProp.Value : null;
            }
        }

        public string Designation
        {
            get
            {
                var spcProp = ListSpcProps.FirstOrDefault(prop => prop.Name == "Обозначение");
                return spcProp != null ? spcProp.Value : null;
            }
        }
        
        public string CurrentPath { get; set; }
        
        public Specification Children { get; set; }
        
        public Specification Parent { get; set; }

        public List<SpcObject> ListSpcObjects { get; protected set; }

        public List<SpcProp> ListSpcProps  { get; protected set; }

        public List<SpcSection> SpcSections { get; protected set; }

        public IFile File { get; set; }

    }
}
