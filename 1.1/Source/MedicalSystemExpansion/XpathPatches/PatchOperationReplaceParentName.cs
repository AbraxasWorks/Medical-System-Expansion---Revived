using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace OrenoMSE.XpathPatches
{
    class PatchOperationReplaceParentName : PatchOperationPathed
    {
        protected string parentxpath;

        protected override bool ApplyWorker ( XmlDocument xml )
        {

            bool result = false;
            
            var possibleReplacements = xml.SelectNodes( parentxpath );

            if ( possibleReplacements.Count != 1 ) return false;

            XmlNode replacementNode = possibleReplacements.Item( 0 );


            foreach ( XmlNode node in from object x in xml.SelectNodes( xpath )
                                            select (XmlNode)x )
            {

                if ( node.Attributes["ParentName"] != null )
                {
                    node.ParentNode.SelectSingleNode();


                }


                result = true;
            }


            return result;
        }
    }
}
