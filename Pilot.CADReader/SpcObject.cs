using System.Collections.Generic;

namespace Ascon.Pilot.SDK.SpwReader
{
    class SpcObject
    {
        private string id;
        private string name;
        private int sectionNumber;
        private string sectionName;
        private bool isSynchronized;

        public List<SpcColumn> Columns;
        

        public SpcObject()
        {
            Columns = new List<SpcColumn>();
            Columns.Clear();
            sectionName = string.Empty;
            isSynchronized = false;
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

        public bool IsSynchronized
        {
            get
            {
                return isSynchronized;
            }

            set
            {
                isSynchronized = value;
            }
        }
    }
}
