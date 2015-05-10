using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// classe partielle pour les opérations booléennes
namespace MagicVM_Assembler
{
    partial class Assembler
    {
        private byte[] Instr_and(string reg, string nb)
        {
            byte[] result = new byte[2];
            result[0] = (byte)0x29;
            result[1] = Encode_scal5_reg3(ParseByte(nb), ParseRegister(reg));
            return result;
        }

        private byte[] Instr_andreg(string reg0, string reg1)
        {
            byte[] result = new byte[2];
            result[0] = (byte)0x2D;
            result[1] = Encode_nul2_reg3_reg3(ParseRegister(reg0), ParseRegister(reg1));
            return result;
        }

        private byte[] Instr_xorreg(string reg0, string reg1)
        {
            byte[] result = new byte[2];
            result[0] = (byte)0x11;
            result[1] = Encode_nul2_reg3_reg3(ParseRegister(reg0), ParseRegister(reg1));
            return result;
        }
    }
}