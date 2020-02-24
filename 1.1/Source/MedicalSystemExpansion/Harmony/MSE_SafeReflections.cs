using System;
using System.Reflection;
using Harmony;

namespace OrenoMSE.Harmony
{
    /*
    // The code devrived from Zombieland mod by pardeike.
    // The license : Free. As in free beer. Copy, learn and be respectful.
    // --- github.com/pardeike/Zombieland/blob/master/README.md ---
    */
    public static class MSE_SafeReflections
    {
        public static Type ToType(this string name)
        {
            var type = AccessTools.TypeByName(name);
            if (type == null) throw new Exception("Cannot find type named '" + name);
            return type;
        }

        public static MethodInfo MethodNamed(this Type type, string name)
        {
            var method = AccessTools.Method(type, name);
            if (method == null) throw new Exception("Cannot find method named '" + name + "' in type " + type.FullName);
            return method;
        }
    }
}
