﻿using System.Linq;
using System.Collections.Generic;
using KompasFileReader.Model;
using System;

namespace KompasFileReader.Spc
{
    public class Specification : GeneralProp, IGeneralDocEntity
    {
        public string Name
        {
            get
            {
                var spcProp = ListSpcProps.FirstOrDefault(prop => prop.Name == "Наименование");
                return spcProp?.Value;
            }
        }

        public string Designation
        {
            get
            {
                var spcProp = ListSpcProps.FirstOrDefault(prop => prop.Name == "Обозначение");
                return spcProp?.Value;
            }
        }
        /// <summary>
        /// Путь к файлу спецификации на диске
        /// </summary>
        public string FileName { get; set; }
        
        public List<Specification> Children { get; set; }
        
        public List<Specification> Parent { get; set; }

        public List<SpcObject> ListSpcObjects { get; protected set; }

        public List<SpcProp> ListSpcProps  { get; protected set; }

        public List<SpcSection> SpcSections { get; protected set; }

        public string GetName()
        {
            return Name;
        }

        public string GetDesignation()
        {
            return Designation;
        }

        public void SetGlobalId(Guid value)
        {
            GlobalId = value;
        }
    }
}