namespace KompasFileReader.Model
{
    /// <summary>
    /// В общем смысле это свойства всех документов компаса (спецификации и чертежа)
    /// Пример name="Масса" value="10.037" typeValue="double" 
    /// natureId="V16F0ACEB123048408CFC1292992D9C44" unitId="VD53586643AE74C38A1BDF03D34991850"
    /// </summary>
    public class DocProp
    {
         public string Name { get; set; }
         public string Value { get; set; }
         public string TypeValue { get; set; }
         public string NatureId { get; set; }
         public string UnitId { get; set; }
         public string Source { get; set; }
    }
}
