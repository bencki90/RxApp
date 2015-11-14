using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RxApp
{
    class HelionBaseReader
    {
        private XDocument xDoc;
        private DataTable dataTable;
        public String topic;
        public String titleLike;

        public HelionBaseReader(String fileContent, DataTable dataTable)
        {
            xDoc = XDocument.Parse(fileContent);
            this.dataTable = dataTable;
            topic = null;
            titleLike = null;
        }

        public List<String> ReadTopics()
        {
            return (from xElem in xDoc.Root.Descendants("seriatematyczna")
                    where !String.IsNullOrWhiteSpace((String)xElem)
                    select xElem.Value).Distinct().Concat(new String[] { Resources.EmptyTopic }).OrderBy(x => x).ToList();
        }

        public IEnumerable<DataRow> ReadBooks()
        {
            IEnumerable<XElement> filteredBooks = xDoc.Root.Element("lista").Elements("ksiazka");

            if (topic != null)
            {
                filteredBooks = filteredBooks.Where(x => (String)x.XPathSelectElement("serietematyczne/seriatematyczna") == topic);
            }
            if (titleLike != null)
            {
                filteredBooks = filteredBooks.Where(x => x.ElementValue("tytul").Contains(titleLike));
            }

            int i = 0;
            foreach (var book in filteredBooks.OrderBy(x => (String)x.Element("tytul")))
            {
                var dataRow = dataTable.NewRow();
                dataRow[Resources.ColumnNameNumber] = ++i;
                dataRow[Resources.ColumnNameAuthor] = (String)book.Element("autor");
                dataRow[Resources.ColumnNameCover] = (String)book.Element("oprawa");
                dataRow[Resources.ColumnNamePrice] = Decimal.Parse((String)book.Element("cenadetaliczna"), System.Globalization.CultureInfo.InvariantCulture);
                dataRow[Resources.ColumnNameTitle] = (String)book.Element("tytul");
                dataRow[Resources.ColumnNameTopic] = (String)book.XPathSelectElement("serietematyczne/seriatematyczna");
                yield return dataRow;
            }
        }
    }
}
