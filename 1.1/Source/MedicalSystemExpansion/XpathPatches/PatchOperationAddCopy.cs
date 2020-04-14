using System.Linq;
using System.Xml;

using Verse;

namespace OrenoMSE.XpathPatches
{
    internal class PatchOperationAddCopy : PatchOperationPathed
    {
        protected string fromxpath;

        protected override bool ApplyWorker ( XmlDocument xml )
        {
            bool result = false;

            foreach ( XmlNode fromNode in xml.SelectNodes( fromxpath ) )
            {
                foreach ( XmlNode parentNode in xml.SelectNodes( xpath ) )
                {
                    var valNode = fromNode.Clone();

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
            }

            return result;
        }

        private readonly Order order = Order.Append;

        private enum Order
        {
            Append,
            Prepend
        }
    }
}