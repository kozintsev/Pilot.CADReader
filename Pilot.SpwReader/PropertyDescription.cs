namespace Ascon.Pilot.SDK.SpwReader
{
    class PropertyDescription
    {
        private int id;
        private string name;
        private string typeValue;
        private string natureId;

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

        public string TypeValue
        {
            get
            {
                return typeValue;
            }

            set
            {
                typeValue = value;
            }
        }

        public string NatureId
        {
            get
            {
                return natureId;
            }

            set
            {
                natureId = value;
            }
        }
    }
}
