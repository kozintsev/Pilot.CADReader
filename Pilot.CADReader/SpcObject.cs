using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.CADReader
{
    class SpcObject
    {
        private string id;
        private string name;
        private int sectionNumber;

        public List<SpcColumn> Columns = null;
        

        public string Id
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

        public int SectionNumber
        {
            get
            {
                return sectionNumber;
            }

            set
            {
                sectionNumber = value;
            }
        }
    }
}
