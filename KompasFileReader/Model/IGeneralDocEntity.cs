using System;
using System.Collections.Generic;
using KompasFileReader.Model.Spc;

namespace KompasFileReader.Model
{
    /// <summary>
    /// Общий интерфейс для документов
    /// </summary>
    public interface IGeneralDocEntity
    {
        string GetName();

        string GetDesignation();

        List<DocProp> GetProps();

        void SetGlobalId(Guid value);
    }
}
