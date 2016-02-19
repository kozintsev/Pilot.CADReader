using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.CADReader
{
    class SpcColumn
    {
        private string name;
        private string typeName;
        private int type;
        private int number;
        private int blockNumber;
        private string value;
        private int modified;

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

        public string TypeName
        {
            get
            {
                return typeName;
            }

            set
            {
                typeName = value;
            }
        }

        public int Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

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

        public int BlockNumber
        {
            get
            {
                return blockNumber;
            }

            set
            {
                blockNumber = value;
            }
        }

        public string Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        public int Modified
        {
            get
            {
                return modified;
            }

            set
            {
                modified = value;
            }
        }
    }
}
