using System;

namespace Ascon.Pilot.SDK.CadReader.Model
{
    public interface IGeneralDocEntity
    {
        string GetName();

        string GetDesignation();

        void SetGlobalId(Guid value);
    }
}
