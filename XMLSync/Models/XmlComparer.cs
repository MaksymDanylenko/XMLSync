using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XMLSync.Models
{
    // Результат сравнения узлов или аттрибутов
    enum CompareCase
    {
        NotCorrespond = 0, // Не соответствуют
        AreCorrespond = 1, // Соответствуют
        AreEqual = 2 // Равны
    }

    static class XmlComparer
    {
        // Сравнение XML-документов
        // Параметры: XmlDocument leftXmlDocument - "левый" документ,
        // XmlDocument rightXmlDocument - "правый" документ
        public static StringBuilder CompareXmlDocuments(XmlDocument leftXmlDocument, XmlDocument rightXmlDocument)
        {
            var compareListRepresent = new StringBuilder(); // Результирующая строка
            var compareList = CompareXmlElements(leftXmlDocument.DocumentElement,
                rightXmlDocument.DocumentElement); // Список результатов сравнения

            // Заполнение результирующей строки
            foreach (CompareCase compareCase in compareList)
            {
                switch (compareCase)
                {
                    case CompareCase.NotCorrespond:
                        compareListRepresent.Append("<>\n");
                        break;
                    case CompareCase.AreCorrespond:
                        compareListRepresent.Append("!=\n");
                        break;
                    case CompareCase.AreEqual:
                        compareListRepresent.Append("=\n");
                        break;
                    default:
                        break;
                }
            }

            // Возвращение результата
            return compareListRepresent;
        }

        // Сравнение узлов
        // Параметры: XmlNode leftXmlElement - узел "левого" документа,
        // XmlNode rightXmlElement - узел "правого" документа
        private static List<CompareCase> CompareXmlElements(XmlNode leftXmlElement, XmlNode rightXmlElement)
        {
            var attrAreEqual = true; // Сохранение истинности в случае равности аттрибутов
            var childsAreEqual = true; // Сохранение истинности в случае равности дочерних элементов
            var hasChildsOrAttributes = false; // Сохранение истинности в случае наличия дочерних элементов или аттрибутов

            var compareList = new List<CompareCase>(); // Список результатов сравнения (результирующий)
            var attrCompareList = new List<CompareCase>(); // Список результатов сравнения аттрибутов
            var childCompareList = new List<CompareCase>(); // Список результатов сравнения дочерних элементов

            var compareCase = CompareCase.NotCorrespond; // Сохранение результата сравнения текущего узла

            // Проверка соответствия имен узлов
            if (leftXmlElement.Name.Equals(rightXmlElement.Name))
            {
                compareCase = CompareCase.AreCorrespond; // Если имена равны, узлы соответсвуют

                // Если элемент - текст и значения равны, узлы равны
                if ((leftXmlElement.Name.Equals("#text") && rightXmlElement.Name.Equals("#text"))
                    && (leftXmlElement.Value.Equals(rightXmlElement.Value)))
                {
                    compareCase = CompareCase.AreEqual;
                }
            }

            // Сравнение аттрибутов элементов
            if (leftXmlElement.Attributes != null)
            {
                hasChildsOrAttributes = true; // Узел содержит аттрибуты

                // Сравнение аттрибутов
                for (int i = 0; i < leftXmlElement.Attributes.Count; i++)
                {
                    var attrComp = CompareXmlAttributes(leftXmlElement.Attributes[i],
                        rightXmlElement.Attributes[i]);
                    attrCompareList.Add(attrComp);

                    if (attrComp != CompareCase.AreEqual)
                        attrAreEqual = false; // Аттрибуты не равны
                }
            }

            // Сравнение дочерних элеметов
            if (leftXmlElement.HasChildNodes)
            {
                hasChildsOrAttributes = true; // Узел содержит дочерние элементы

                // Сравнение дочерних элементов
                for (int i = 0; i < leftXmlElement.ChildNodes.Count; i++)
                {
                    var chCompList = CompareXmlElements(leftXmlElement.ChildNodes[i], rightXmlElement.ChildNodes[i]);
                    childCompareList.AddRange(chCompList);

                    // Если верхние по иерархии дочерние элементы не равны, текущие элементы не равны
                    if (chCompList[0] != CompareCase.AreEqual)
                        childsAreEqual = false;
                }
            }

            // Если узлы соответствуют, проверить на равность
            if (compareCase == CompareCase.AreCorrespond)
                compareCase = childsAreEqual && attrAreEqual && hasChildsOrAttributes ? CompareCase.AreEqual : compareCase;

            compareList.Add(compareCase); // Добавить к результату сравнение узлов
            compareList.AddRange(attrCompareList); // Добавить к результату сравнение аттрибутов
            compareList.AddRange(childCompareList); // Добавить к результату сравнение дочерних элементов

            // Возвращение результата
            return compareList;
        }

        // Сравнение аттрибутов
        // Параметры: XmlAttribute leftXmlAttribute - "левый" аттрибут,
        // XmlAttribute rightXmlAttribute - "правый" аттрибут
        private static CompareCase CompareXmlAttributes(XmlAttribute leftXmlAttribute, XmlAttribute rightXmlAttribute)
        {
            var compareCase = CompareCase.NotCorrespond; // Результат сравнения
            
            // Если имена аттрибутов равны, аттрибуты соосветствуют
            if (leftXmlAttribute.Name.Equals(rightXmlAttribute.Name))
            {
                compareCase = CompareCase.AreCorrespond;

                // Если аттрибуты соответствуют и значения равны, аттрибуты равны
                if (compareCase == CompareCase.AreCorrespond
                    && leftXmlAttribute.Value.Equals(rightXmlAttribute.Value))
                {
                    compareCase = CompareCase.AreEqual;
                }
            }

            // Возвращение результата сравнения аттрибутов
            return compareCase;
        }
    }
}
