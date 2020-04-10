using System.Linq;
using System.Xml;

using Verse;

namespace OrenoMSE.XpathPatches
{
    internal class PatchOperationOffsetField : PatchOperationPathed
    {
        private readonly double offset;

        protected override bool ApplyWorker ( XmlDocument xml )
        {
            bool result = false;

            foreach ( XmlNode parentNode in from object x in xml.SelectNodes( xpath )
                                            select (XmlNode)x )
            {
                result = true;

                double oldValue = double.Parse( parentNode.InnerText );

                parentNode.InnerText = string.Format( "{0}", oldValue + offset );
            }

            return result;
        }
    }
}