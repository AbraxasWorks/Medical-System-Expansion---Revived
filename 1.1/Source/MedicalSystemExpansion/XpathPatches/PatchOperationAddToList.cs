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
                result = true;

                XmlNode listNode = parentNode.SelectSingleNode( list );
                if ( listNode == null )
                {
                    // Add - Add list if not existing
                    listNode = parentNode.OwnerDocument.CreateElement( list );
                    parentNode.AppendChild( listNode );
                }

                // finally add element to list
                listNode.AppendChild( parentNode.OwnerDocument.ImportNode(valNode.FirstChild, true) );
            }


            return result;
        }

    }
}
