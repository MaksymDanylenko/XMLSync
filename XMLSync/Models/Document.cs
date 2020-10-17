using System.Text;
using System.Xml;

namespace XMLSync.Models
{

    // XML-документ
    class Document
    {
        public XmlDocument XmlDocument { get; set; } //Объект XML-документа

        public StringBuilder Elements { get; private set; } //XML-документ для вывода в форму

        // Конструктор для создания документа
        public Document(string path)
        {
            this.XmlDocument = new XmlDocument();
            this.XmlDocument.Load(path);
            this.Elements = this.CreateElementsList(this.XmlDocument.DocumentElement, 0, "");
        }

        // Создание строки для вывода в форму
        // Параметры: XmlNode xmlElement - узел, int level - уровень вложения, string prefix - префикс для вывода
        private StringBuilder CreateElementsList(XmlNode xmlElement, int level, string prefix)
        {
            var elList = new StringBuilder(); // Возвращаемая строка

            var elementName = prefix + xmlElement.Name; // Имя элемента

            // Если элемент - текст узла, добавить в строку представления значение текста
            if (xmlElement.Name.Equals("#text"))
                elList.Append(elementName + "=" + xmlElement.Value + "\n");
            else
                elList.Append(elementName + "\n");

            // Добавления аттрибутов узла в представление
            if (xmlElement.Attributes != null)
            {
                if (level == 0)
                    elList.Append(CreateAttributesList(xmlElement.Attributes, level + 1, "  "));
                else
                    elList.Append(CreateAttributesList(xmlElement.Attributes, level + 1, "  |" + prefix));
            }

            // Добавления дочерних элементов узла в представление
            if (xmlElement.HasChildNodes)
            {
                for (int i = 0; i < xmlElement.ChildNodes.Count; i++)
                {
                    if (level == 0)
                        elList.Append(CreateElementsList(xmlElement.ChildNodes[i], level + 1, "  "));
                    else
                        elList.Append(CreateElementsList(xmlElement.ChildNodes[i], level + 1, "  |" + prefix));
                }
            }

            // Возвращение результирующей строки
            return elList;
        }

        // Создание строки, которая представляет аттрибуты узла
        // Параметры: XmlAttributeCollection attributes - коллекция аттрибутов,
        // int level - уровень вложения, string prefix - префикс для вывода
        private StringBuilder CreateAttributesList(XmlAttributeCollection attributes, int level, string prefix)
        {
            // Представление возвращаемого списка аттрибутов узла
            var attrList = new StringBuilder();

            // Добавление атрибутов и их значений
            foreach (XmlAttribute attribute in attributes)
                attrList.Append(prefix + "#attr: " + attribute.Name + "=" + attribute.Value + "\n");

            // Возвращение результирующей строки
            return attrList;
        }
    }
}
