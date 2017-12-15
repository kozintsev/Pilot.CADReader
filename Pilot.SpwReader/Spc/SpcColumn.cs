namespace Ascon.Pilot.SDK.CadReader.Spc
{
    /// <summary>
    /// Пример name="Формат" typeName="format" type="1" number="1" blockNumber="0" value="A1" modified="0"
    /// </summary>
    internal class SpcColumn
    {
        /// <summary>
        /// Пример name="Формат" или name="Наименование" 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Пример typeName="format" typeName="name"
        /// </summary>
        public string TypeName { get; set; }

        public int Type { get; set; }

        public int Number { get; set; }

        public int BlockNumber { get; set; }
        /// <summary>
        /// Пример value="Редуктор" или value="Шпонка 10 x 8 x 40 ГОСТ 23360-78"
        /// </summary>
        public string Value { get; set; }

        public int Modified { get; set; }
    }
}
