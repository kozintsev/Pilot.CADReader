using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.SDK.SpwReader
{
    class SpcObject
    {
        public List<SpcColumn> Columns;

        public List<SpcDocument> Documents;

        public Guid GlobalId { get; set; }

        public SpcObject()
        {
            Columns = new List<SpcColumn>();
            Documents = new List<SpcDocument>();
            Columns.Clear();
            Documents.Clear();
            SectionName = string.Empty;
            IsSynchronized = false;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public int SectionNumber { get; set; }

        public string SectionName { get; set; }

        public bool IsSynchronized { get; set; }

        public string PdfDocument { get; set; }

    }
}
