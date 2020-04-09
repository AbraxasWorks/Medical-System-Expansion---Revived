using System.Linq;
using System.Xml;

using Verse;

namespace OrenoMSE.XpathPatches
{
    internal class PatchOperationAddOrMergeCopy : PatchOperationPathed
    {

        protected string fromxpath;

        protected override bool ApplyWorker ( XmlDocument xml )
        {

            XmlNode fromNode = xml.SelectSingleNode( fromxpath );
            if ( fromNode == null ) return false;


            bool result = false;
            foreach ( XmlNode parentNode in xml.SelectNodes( xpath ) )
            {

                var valNode = fromNode.Clone();

                var potentialMerge = ( from XmlNode x in parentNode.ChildNodes where x.Name == valNode.Name select x ).FirstOrDefault() ;
                if ( potentialMerge == null )
                {
                    result = true;
                    // add the node normally
                    switch ( order )
                    {
                        case Order.Append:
                        {
                            parentNode.AppendChild( parentNode.OwnerDocument.ImportNode( valNode, true ) );
                            break;
                        }
                        case Order.Prepend:
                        {
                            parentNode.PrependChild( parentNode.OwnerDocument.ImportNode( valNode, true ) );
                            break;
                        }
                    }
                }
                else if ( this.mergeIfExisting )
                {

                    result = true;
                    // do the merge
                    switch ( order )
                    {
                        case Order.Append:
                        {
                            foreach ( XmlNode node in valNode.ChildNodes )
                            {
                                potentialMerge.AppendChild( parentNode.OwnerDocument.ImportNode( node, true ) );
                            }
                            break;
                        }
                        case Order.Prepend:
                        {
                            foreach ( XmlNode node in valNode.ChildNodes.Cast<XmlNode>().Reverse() )
                            {
                                potentialMerge.PrependChild( parentNode.OwnerDocument.ImportNode( node, true ) );
                            }
                            break;
                        }
                    }
                }
            }

            return result;
        }


        private readonly bool mergeIfExisting = true;

        private readonly Order order = Order.Append;

        private enum Order
        {
            Append,
            Prepend
        }

    }
}
