using System;

namespace Ascon.Pilot.SDK.CadReader.Model
{
    /// <summary>
    /// Общий интерфейс для документов
    /// </summary>
    public interface IGeneralDocEntity
    {
        string GetName();

        string GetDesignation();

        void SetGlobalId(Guid value);
    }
}
