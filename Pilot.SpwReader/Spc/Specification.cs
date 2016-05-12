using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.SDK.SpwReader.Spc
{
    class Specification
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

        public List<SpcObject> ListSpcObjects { get; set; }

        public List<SpcProp> ListSpcProps  { get; set; }

        public Specification()
        {
            ListSpcObjects = new List<SpcObject>();
            ListSpcObjects.Clear();
        }

    }
}
