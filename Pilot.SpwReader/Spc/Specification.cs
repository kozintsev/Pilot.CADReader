using System.Collections.Generic;

namespace Ascon.Pilot.SDK.SpwReader.Spc
{
    class Specification
    {
        public string Name { get; set; }
        public string Designation { get; set; }
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
