using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Verse;

namespace OrenoMSE.XpathPatches
{
    class PatchOperationAddToList : PatchOperationPathed
    {
        protected string list;

        private XmlContainer value;

        protected override bool ApplyWorker ( XmlDocument xml )
        {
            XmlNode valNode = value.node;
            bool result = false;


            foreach ( XmlNode parentNode in from object x in xml.SelectNodes( xpath )
                                            select (XmlNode)x ) 
            {

                XmlNode listNode = parentNode.SelectSingleNode( list );
                if ( listNode == null )
                {
                    // Add - Add list if not existing
                    listNode = parentNode.OwnerDocument.CreateElement( list );
                    parentNode.AppendChild( listNode );
                }

                // finally add element to list
                switch ( order )
                {
                    case Order.Append:
                        listNode.AppendChild( parentNode.OwnerDocument.ImportNode(valNode.FirstChild, true) );
                        break;
                    case Order.Prepend:
                        listNode.PrependChild( parentNode.OwnerDocument.ImportNode( valNode.FirstChild, true ) );
                        break;
                }

                result = true;

            }


            return result;
        }




        private PatchOperationAddToList.Order order = PatchOperationAddToList.Order.Append;

        private enum Order
        {
            // Token: 0x04004A5B RID: 19035
            Append,
            // Token: 0x04004A5C RID: 19036
            Prepend
        }

    }
}
