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
        private string sectionName = String.Empty;
        public List<SpcColumn> Columns;
        
        public SpcObject()
        {
            Columns = new List<SpcColumn>();
            Columns.Clear();
        }


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

        public string SectionName
        {
            get
            {
                return sectionName;
            }

            set
            {
                sectionName = value;
            }
        }
    }
}
