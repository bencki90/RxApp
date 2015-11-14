using System;
using System.Xml.Linq;

namespace RxApp
{
    public static class XElementExtensions
    {
        public static String ElementValue(this XElement value, XName name)
        {
            return (String)value.Element(name) ?? String.Empty;
        }
    }
}
