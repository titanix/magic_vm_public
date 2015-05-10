using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MagicVM_Assembler
{
    partial class Assembler
    {
        private byte[] Instr_jmp(int addr)
        {
            byte[] result = new byte[3];
            result[0] = 0x12;
            byte[] tmp = Encode_2bytes_addr(addr);
            result[1] = tmp[0];
            result[2] = tmp[1];

            return result;
        }

        // call the good method thru reflection
        private byte[] Conditional_branch(string mnemonic, int addr, byte reg0, byte reg1)
        {
            MethodInfo method = this.GetType().GetMethod("Instr_" + mnemonic);
            object result = method.Invoke(this, new object[] { reg0, reg1, addr });
            return (byte[])result;
        }

        /// <summary>
        /// Ajout au flux de sortie les opcodes pour le saut conditionnel spécifié par 'opcode'
        /// et encode ses arguments 
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="reg0"></param>
        /// <param name="reg1"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        private byte[] _instr_branch(byte opcode, byte reg0, byte reg1, int addr)
        {
            List<byte> result = new List<byte>();
            result.Add(opcode);
            result.Add(Encode_nul2_reg3_reg3(reg0, reg1));
            result.Add(Encode_2bytes_addr(addr));
            return result.ToArray();
        }

        public byte[] Instr_je(byte reg0, byte reg1, int addr)
        {
            return _instr_branch((byte)0x03, reg0, reg1, addr);
        }

        public byte[] Instr_jl(byte reg0, byte reg1, int addr)
        {
            return _instr_branch((byte)0x07, reg0, reg1, addr);
        }

        public byte[] Instr_jg(byte reg0, byte reg1, int addr)
        {
            return _instr_branch((byte)0x0B, reg0, reg1, addr);
        }

        public byte[] Instr_jle(byte reg0, byte reg1, int addr)
        {
            List<byte> result = new List<byte>();
            result.Add(Instr_jl(reg0, reg1, addr));
            result.Add(Instr_je(reg0, reg1, addr));
            return result.ToArray();
        }

        public byte[] Instr_jge(byte reg0, byte reg1, int addr)
        {
            List<byte> result = new List<byte>();
            result.Add(Instr_jg(reg0, reg1, addr));
            result.Add(Instr_je(reg0, reg1, addr));
            return result.ToArray();
        }

    }
}
