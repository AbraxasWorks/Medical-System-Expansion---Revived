using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Verse;

namespace OrenoMSE.XpathPatches
{
    class PatchOperationOffsetField : PatchOperationPathed
    {

        private double offset;

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
