using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.CADReader
{
    class SpcObject
    {
        private int id;
        private string name;

        public List<SpcColumn> Columns;

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
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
