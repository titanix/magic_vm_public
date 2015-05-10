using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicVM_Assembler
{
    public static class MyExtensionMethods
    {
        public static List<Byte> Add(this List<Byte> me, byte[] tab)
        {
            List<Byte> temp = me;
            foreach (byte b in tab)
            {
                temp.Add(b);
            }
            return temp;
        }

        public static List<Byte> Write(this List<Byte> me, byte b)
        {
            me.Add(b);
            return me;
        }
    }
}
