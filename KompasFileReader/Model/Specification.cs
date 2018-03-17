using System;
using System.Collections.Generic;
using KompasFileReader.Model.Spc;

namespace KompasFileReader.Model
{
    /// <summary>
    /// Класс для спецификации (свойства храняться в папке Spc)
    /// </summary>
    public class Specification : GeneralProp, IGeneralDocEntity
    {
        
        /// <summary>
        /// Путь к файлу спецификации на диске
        /// </summary>
        public string FileName { get; set; }
        
        public int IdStyle { get; set; }
        
        public List<Specification> Children { get; set; }
        
        public List<Specification> Parent { get; set; }

        public List<SpcObject> ListSpcObjects { get; protected set; }

        public List<SpcSection> SpcSections { get; protected set; }

        public string GetName()
        {
            return Name;
        }

        public string GetDesignation()
        {
            return Designation;
        }

        public List<DocProp> GetProps()
        {
            return ListSpcProps;
        }

        public void SetGlobalId(Guid value)
        {
            GlobalId = value;
        }
    }
}
