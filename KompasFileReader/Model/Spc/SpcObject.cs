using System.Linq;
using System.Collections.Generic;
using System;

namespace KompasFileReader.Model.Spc
{
    public class SpcObject : GeneralProp, IGeneralDocEntity
    {
        public List<SpcColumn> Columns;
        
        public List<AddColumn> AColumns;

        public List<SpcDocument> Documents;

        public SpcObject()
        {
            Columns = new List<SpcColumn>();
            AColumns = new List<AddColumn>();
            Documents = new List<SpcDocument>();
            AColumns.Clear();
            Columns.Clear();
            Documents.Clear();
            SectionName = string.Empty;
        }
        /// <summary>
        /// Идентификатор из Компас 3D
        /// </summary>
        public string Id { get; set; }
        
        public string Format
        {
            get
            {
                var column = Columns.FirstOrDefault(prop => prop.Name == "Формат");
                return column?.Value;
            }
        }

        public string Position
        {
            get
            {
                var column = Columns.FirstOrDefault(prop => prop.Name == "Позиция");
                return column?.Value;
            }
        }
        public new string Name
        {
            get
            {
                var column = Columns.FirstOrDefault(prop => prop.Name == "Наименование");
                return column?.Value;
            }
        }
        
        public new string Primechan
        {
            get
            {
                var column = Columns.FirstOrDefault(prop => prop.Name == "Примечание");
                return column?.Value;
            }
        }
        
        public new string Designation
        {
            get
            {
                var column = Columns.FirstOrDefault(prop => prop.Name == "Обозначение");
                return column?.Value;
            }
        }
        
        public new string Massa
        {
            get
            {
                var column = AColumns.FirstOrDefault(prop => prop.Name == "Масса");
                return column?.Value;
            }
        }
        
        public new string IDMat
        {
            get
            {
                var column = AColumns.FirstOrDefault(prop => prop.Name == "ID материала");
                return column?.Value;
            }
        }

        public new string ObozMat
        {
            get
            {
                var column = AColumns.FirstOrDefault(prop => prop.Name == "Обозначение материала");
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
