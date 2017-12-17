using System;

namespace Ascon.Pilot.SDK.CadReader.Spc
{
    public abstract class GeneralProp
    {
        public Guid GlobalId;
        public string PdfDocument { get; set; }
        public string XpsDocument { get; set; }
    }
}
