using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.CADReader
{
    class SpcSection
    {
        private int number;
        private string name;

        public int Number
        {
            get
            {
                return number;
            }

            set
            {
                number = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
    }
}
