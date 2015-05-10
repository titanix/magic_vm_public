using System;
using System.IO;
using System.Collections.Generic;

namespace MagicVM_Assembler
{
    partial class Assembler
    {
        private byte Encode_scal5_reg3(byte scalaire, byte register)
        {
            byte result = scalaire;
            result <<= 3;
            register &= 0x07; // le masque est là par sécurité
            result |= (register);
            return result;
        }

        private byte Encode_nul2_reg3_reg3(byte reg0, byte reg1)
        {
            byte result = reg1;
            result <<= 3;
            result |= reg0;
            return result;
        }

        private byte[] Encode_2bytes_addr(int addr)
        {
            byte[] result = new byte[2];
            int tmp = (addr & 0xFF00) >> 8; // MSD
            result[0] = (byte)tmp;
            tmp = addr & 0xFF; // LSB
            result[1] = (byte)tmp;
            return result;
        }
    }
}