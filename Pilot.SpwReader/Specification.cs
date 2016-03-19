using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.SpwReader
{
    class Specification
    {
        public string Name { get; set; }
        public string Designation { get; set; }
        public string CurrentPath { get; set; }

        public List<SpcObject> ListSpcObjects { get; set; }

        public Specification()
        {
            ListSpcObjects = new List<SpcObject>();
            ListSpcObjects.Clear();
        }

    }
}
