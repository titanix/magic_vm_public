using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// classe partielle pour les opérations mathématiques
namespace MagicVM_Assembler
{
    partial class Assembler
    {
        private byte[] Instr_add(string reg, string nb)
        {
            byte register = ParseRegister(reg);
            List<byte> result = new List<byte>();
            int number = 0;
            
            if (!Int32.TryParse(nb, out number))
            {
                Console.WriteLine("Error parsing {0} at line {1}", nb, lineNumber);
                Environment.Exit(1);
            }

            if (number <= 31)
            {
                result.Add((byte)0x09);
                result.Add(Encode_scal5_reg3((byte)number, register));
            }
            else
            {
                //result.Add(Encode_scal5_reg3(31, register));
                //result.Add(Instr_add(reg, (number - 31).ToString()));
                for (int i = 0; i < number / 31; i++)
                {
                    result.Add((byte)0x09);
                    result.Add(Encode_scal5_reg3(31, register));
                }
                result.Add((byte)0x09);
                result.Add(Encode_scal5_reg3((byte)(number % 31), register));
            }
            return result.ToArray();
        }

        private byte[] Instr_sub(string reg, string nb)
        {
            byte register = ParseRegister(reg);
            byte number = ParseByte(nb);
            List<byte> result = new List<byte>();

            if (number <= 31)
            {
                result.Add((byte)0x1D);
                result.Add(Encode_scal5_reg3(number, register));
            }
            else
            {
                result.Add((byte)0x1D);
                result.Add(Encode_scal5_reg3(31, register));
                result.Add(Instr_sub(reg, (number - 31).ToString()));
            }
            return result.ToArray();
        }

        private byte[] Instr_addreg(string reg0, string reg1)
        {
            byte register0 = ParseRegister(reg0);
            byte register1 = ParseRegister(reg1);
            List<byte> result = new List<byte>();

            result.Add((byte)0x19);
            result.Add(Encode_nul2_reg3_reg3(register0, register1));
            return result.ToArray();
        }

        private byte[] Instr_subreg(string reg0, string reg1)
        {
            byte register0 = ParseRegister(reg0);
            byte register1 = ParseRegister(reg1);
            List<byte> result = new List<byte>();

            result.Add((byte)0x21);
            result.Add(Encode_nul2_reg3_reg3(register0, register1));
            return result.ToArray();
        }
    }
}
