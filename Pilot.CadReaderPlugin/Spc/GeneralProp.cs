using System;

namespace Ascon.Pilot.SDK.CadReader.Spc
{
    public abstract class GeneralProp
    {
        /// <summary>
        /// Guid from Pilot ICE
        /// </summary>
        public Guid GlobalId;
        /// <summary>
        /// Путь к PDF или XPS документу со вторичным представлением
        /// </summary>
        public string PreviewDocument { get; set; }
    }
}
