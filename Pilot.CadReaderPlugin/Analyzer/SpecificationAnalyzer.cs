using System.Collections.Generic;
using Ascon.Pilot.SDK.CadReader.Spc;

namespace Ascon.Pilot.SDK.CadReader.Analyzer
{
    public class SpecificationAnalyzer
    {
        /// <summary>
        /// Анализируем список спецификаций и возвращаем список со связями
        /// </summary>
        /// <param name="listSpec"></param>
        /// <returns></returns>
        public static List<Specification> CreateTree(List<Specification> listSpec)
        {
            if (listSpec == null)
                return null;
            // если спецификаций больше 1 то пыполняем анализ и ищем дочерние и корневые объекты
            foreach (var spc in listSpec)
            {
                foreach (var obj in spc.ListSpcObjects)
                {
                    if (obj.SectionName == "Сборочные единицы" || obj.SectionNumber == 15)
                    {
                        foreach (var s in listSpec)
                        {
                            if (s.Designation.Contains(obj.Designation))
                            {
                                if (s.Parent == null) s.Parent = new List<Specification>();
                                if (spc.Children == null) spc.Children = new List<Specification>();
                                s.Parent.Add(spc);
                                spc.Children.Add(s);
                            }
                        }
                    }
                }
            }
            return listSpec;
        }
    }
}
