using System.Linq;
using System.Collections.Generic;
using Ascon.Pilot.SDK.CadReader.Model;
using System;

namespace Ascon.Pilot.SDK.CadReader.Spc
{
    public class SpcObject : GeneralProp, IGeneralDocEntity
    {
        public List<SpcColumn> Columns;

        public List<SpcDocument> Documents;

        public SpcObject()
        {
            Columns = new List<SpcColumn>();
            Documents = new List<SpcDocument>();
            Columns.Clear();
            Documents.Clear();
            SectionName = string.Empty;
        }
        /// <summary>
        /// Идентификатор из Компас 3D
        /// </summary>
        public string Id { get; set; }

        public string Name
        {
            get
            {
                var column = Columns.FirstOrDefault(prop => prop.Name == "Наименование");
                return column?.Value;
            }
        }

        public string Designation
        {
            get
            {
                var column = Columns.FirstOrDefault(prop => prop.Name == "Обозначение");
                return column?.Value;
            }
        }

        public int SectionNumber { get; set; }

        public string SectionName { get; set; }

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
