using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.CADReader
{
    [Serializable]
    class Settings
    {
        public List<string> typesOfPilot;
        public List<string> typesOfCAD;
        //association

        class Association
        {
            public string typePilot;
            public List<string> typesOfCAD;
        }
    }
}
