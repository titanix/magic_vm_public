using System;
using System.IO;
using System.Collections.Generic;

namespace MagicVM_Assembler
{
    partial class Assembler
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Not enough arguments.\n");
                Console.WriteLine("Usage : mvm-asm <source> <out_name>");
                Environment.Exit(1);
            }
            
            if (args.Length < 2)
            {
                string name = args[0].Split(new char[] { '.' })[0];
                name += ".mobj";
                args = new string[] { args[0], name };
            }
            
            Assembler mainClass = new Assembler();
            mainClass.NewCompile(args);
        }

        Dictionary<string, byte> registerCode = new Dictionary<string, byte>();
        public int lineNumber = 0;
        List<byte> outfile = new List<byte>();
        
        public Assembler()
        {
            registerCode.Add("pv0", 0);
            registerCode.Add("pv1", 1);
            registerCode.Add("g0", 2);
            registerCode.Add("g1", 3);
            registerCode.Add("g2", 4);
            registerCode.Add("dk0", 5);
            registerCode.Add("dk1", 6);
            registerCode.Add("ph0", 7);
        }

        private void Second_label_pass()
        {
#if TEMP2
            foreach (string label in missingLabel.Keys)
            {
                int addr = 0;
                if (labelList.TryGetValue(label, out addr))
                {
                    byte[] addrCodes = Encode_2bytes_addr(addr);
                    outfile[missingLabel[label]] = addrCodes[0];
                    outfile[missingLabel[label] + 1] = addrCodes[1];
                }
                else
                {
                    Console.WriteLine("Error, use undefined label '{0}'", label);
                    Environment.Exit(1);
                }
            }
#endif
        }

        private byte Instr_nop()
        {
            return (byte)0x04; 
        }

        private byte Instr_trc()
        {
            return (byte)0x08;
        }

        private byte ParseByte(string str)
        {
            byte res = 0;
            if (!Byte.TryParse(str, out res))
            {
                Console.WriteLine("Error with '{0}' at line {1}", str, lineNumber);
                Environment.Exit(1);
            }
            return res;
        }

        private int ParseInt(string str)
        {
            int res = 0;
            if (!Int32.TryParse(str, out res))
            {
                Console.WriteLine("Error with '{0}' at line {1}", str, lineNumber);
                Environment.Exit(1);
            }
            return res;
        }

        private byte ParseRegister(string reg)
        {
            byte register = 0;
            if (!registerCode.TryGetValue(reg, out register))
            {
                Console.WriteLine("Error, register '{0}' doesn't exist at line {1}", reg, lineNumber);
                Environment.Exit(1);
            }
            return register;
        }

        private bool IsLabel(string str)
        {
            if (str.Length > 0)
            {
                return str[str.Length - 1] == ':';
            }   
            return false;
        }

    }
}
