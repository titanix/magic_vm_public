using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicVM_Assembler
{
    public enum SymbolType
    {
        Local = 1,
        Export = 2,
        Undefined = 4
    }

    public class LabelInfo
    {
        public SymbolType Type = SymbolType.Undefined;
        public int Address = 0;
        // listes des positions des adresses qui sont liées à ce label qui seont à patcher
        //List<int> PositionToPatch = new List<int>;
        public List<int> PositionToPatch { get; private set; }

        public LabelInfo()
        {
            PositionToPatch = new List<int>();
        }

        public LabelInfo(SymbolType type = SymbolType.Undefined, int address = 0)
            : this()
        {
            Type = type;
            Address = address;
        }
    }

    //public class SymbolTable
}
