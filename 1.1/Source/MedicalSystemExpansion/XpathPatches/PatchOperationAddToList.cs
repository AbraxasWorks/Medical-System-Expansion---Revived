using System.Linq;
using System.Xml;

using Verse;

namespace OrenoMSE.XpathPatches
{
    internal class PatchOperationAddToList : PatchOperationPathed
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
                    {
                        foreach ( XmlNode node in valNode.ChildNodes )
                        {
                            listNode.AppendChild( parentNode.OwnerDocument.ImportNode( node, true ) );
                        }
                        break;
                    }
                    case Order.Prepend:
                    {
                        for ( int i = valNode.ChildNodes.Count - 1; i >= 0; i-- )
                        {
                            listNode.PrependChild( parentNode.OwnerDocument.ImportNode( valNode.ChildNodes[i], true ) );
                        }
                        break;
                    }
                }

                result = true;

            }


            return result;
        }




        private PatchOperationAddToList.Order order = PatchOperationAddToList.Order.Append;

        private enum Order
        {
            Append,
            Prepend
        }

    }
}
