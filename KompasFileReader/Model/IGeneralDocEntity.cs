using System;
using System.Collections.Generic;
using KompasFileReader.Spc;

namespace KompasFileReader.Model
{
    /// <summary>
    /// Общий интерфейс для документов
    /// </summary>
    public interface IGeneralDocEntity
    {
        string GetName();

        string GetDesignation();

        List<SpcProp> GetProps();

        void SetGlobalId(Guid value);
    }
}
