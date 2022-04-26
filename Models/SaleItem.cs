using System;
using System.IO;
using System.Xml.Serialization;

namespace Jp.Models
{
    public class SaleItem : SaleItemModel
    {

        public static SaleItem CreateRandomItem()
        {
            SaleItem itm = new();

            itm.Id = Guid.NewGuid();
            itm.ProductType = "apple";
            itm.ProductValue = 12.50d;


            return itm;

        }


        public string ToXml()
        {
            XmlSerializer xmlSerializer = new(GetType());

            using StringWriter textWriter = new();
            xmlSerializer.Serialize(textWriter, this);

            return textWriter.ToString();

        }

        public override string ToString()
        {
            return String.Concat(ProductType.ToString(), ProductValue.ToString());
        }

    }
}
