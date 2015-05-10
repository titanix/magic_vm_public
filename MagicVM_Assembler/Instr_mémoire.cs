using System;
using System.Collections.Generic;

namespace MagicVM_Assembler
{
    partial class Assembler
    {
        private byte[] Instr_set(string reg, string nb)
        {
            byte register = ParseRegister(reg);
            List<byte> result = new List<byte>();
            int scal = ParseInt(nb);

            if (scal <= 31)
            {
                result.Add((byte)0x05);
                result.Add(Encode_scal5_reg3((byte)scal, register));
            }
            else
            {
                result.Add(Instr_set(reg, "31"));
                result.Add(Instr_add(reg, (scal - 31).ToString()));
            }
            return result.ToArray();
        }

        private byte[] Instr_setbig(string reg, string n)
        {
            List<byte> result = new List<byte>();
            byte register = ParseRegister(reg);
            int number = ParseInt(n);
            int scal = number >> 16;
            result.Add((byte)0x17);
            result.Add(Encode_scal5_reg3((byte)scal, register));
            result.Add(Encode_2bytes_addr(number));
            return result.ToArray();
        }

        private byte[] Instr_setreg(string reg0, string reg1)
        {
            byte[] result = new byte[2];
            result[0] = (byte)0x15;
            result[1] = Encode_nul2_reg3_reg3(ParseRegister(reg0), ParseRegister(reg1));
            return result;
        }

        private byte[] Instr_swap(string reg0, string reg1)
        {
            byte[] result = new byte[2];
            result[0] = (byte)0x0D;
            result[1] = Encode_nul2_reg3_reg3(ParseRegister(reg0), ParseRegister(reg1));
            return result;
        }

        private byte[] Instr_store(string reg0, string reg1)
        {
            byte[] result = new byte[2];
            result[0] = 0x4D;
            result[1] = Encode_nul2_reg3_reg3(ParseRegister(reg0), ParseRegister(reg1));
            return result;
        }
        
    }
}