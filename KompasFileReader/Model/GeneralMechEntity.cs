using System;

namespace KompasFileReader.Model
{
    public abstract class GeneralMechEntity
    {
        public Guid Guid { get; set; }
        public string Name {get; set;}
        public string Designation { get; set; }
        public string Developer { get; set; }
    }
}
