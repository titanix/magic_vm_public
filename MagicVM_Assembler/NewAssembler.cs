using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace MagicVM_Assembler
{
    partial class Assembler
    {
        private enum Mode
        {
            Meta,
            Data,
            Code
        }

        private Mode CurrentMode = Mode.Code;
        private StreamReader codeFile;
        private bool NamespaceDeclared = false;

        private Regex RegNamespace = new Regex(@"^namespace (\w+)$");
        private Regex RegExport = new Regex(@"^export (\w+)$");
        private Regex RegDeclareLabel = new Regex(@"^(\w+):$");

        private Dictionary<string, byte> OpcodeDictionary = new Dictionary<string, byte>()
        {
            {"swap", (byte)0x0D},
            {"setreg", (byte)0x15},
            {"and", (byte)0x29},
            {"or", (byte)0x31},
            {"andreg", (byte)0x2D},
            {"orreg", (byte)0x35},
            {"xorreg", (byte)0x11},
            {"push", (byte)0x51}, // ancien nom de pusha
            {"pop", (byte)0x55}, // ancien nom de popa
            {"jmpreg", (byte)0x59},
            {"call", (byte)0x02},
            {"jump", (byte)0x12},
            {"jmp", (byte)0x12},
            {"pusha", (byte)0x51},
            {"pushb", (byte)0x65},
            {"pushw", (byte)0x69},
            {"popt", (byte)0x10},
            {"popa", (byte)0x55},
            {"popb", (byte)0x6D},
            {"popw", (byte)0x71},
            {"setbig", (byte)0x17},
            {"store", (byte)0x4D}
        };

        private void NextMode()
        {
            switch (CurrentMode)
            {
                case Mode.Meta:
                    CurrentMode = Mode.Data;
                    break;
                case Mode.Data:
                    CurrentMode = Mode.Code;
                    break;
                default:
                    break;
            }
        }

        private string RemoveComment(string line)
        {
            if (line.Contains(";"))
            {
                int commaIndex = line.IndexOf(';');
                if (commaIndex > 0)
                {
                    return line.Substring(0, commaIndex - 1);
                }
                else
                {
                    return null;
                }
            }
            return line;
        }

        private void ProcessMeta()
        {
            CurrentMode = Mode.Meta;
            while (CurrentMode == Mode.Meta)
            {
                while (codeFile.BaseStream.CanRead && (!codeFile.EndOfStream))
                {
                    String line = RemoveComment(codeFile.ReadLine());
                    lineNumber++;
                    if (line != null && line.Length > 0)
                    {
                        line = line.ToLower();
                        if (line.Equals(".data"))
                        {
                            ProcessData();
                            break;
                        }
                        if (line.Equals(".code"))
                        {
                            ProcessCode();
                            break;
                        }
                        if (RegNamespace.IsMatch(line))
                        {
                            if (NamespaceDeclared == true)
                            {
                                Console.Error.WriteLine("A namespace is already declared for this file.");
                                Environment.Exit(1);
                            }
                            try
                            {
                                Namespace = RegNamespace.Match(line).Groups[1].Value.ToUpper();
                                NamespaceDeclared = true;
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.Error.WriteLine("No namespace name specified at line {0}", lineNumber);
                                Environment.Exit(1);
                            }
                        }
                        if (RegExport.IsMatch(line))
                        {
                            string symbol = AddNamespace(RegExport.Match(line).Groups[1].Value.ToUpper());
                            if (symbolTable.ContainsKey(symbol))
                            {
                                symbolTable[symbol].Type = SymbolType.Export;
                            }
                            else
                            {
                                symbolTable.Add(symbol, new LabelInfo(SymbolType.Export));
                            }
                        }

                    }
                }
            }
        }

        private void ProcessData()
        {
            CurrentMode = Mode.Data;
        }

        private void ProcessCode()
        {
            CurrentMode = Mode.Code;
            while (codeFile.BaseStream.CanRead && (!codeFile.EndOfStream))
            {
                string line = RemoveComment(codeFile.ReadLine());
                lineNumber++;
                if (String.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                line = line.Trim();
                if (line.Equals(".meta"))
                {
                    ProcessMeta();
                }
                if (line.Equals(".data"))
                {
                    ProcessMeta();
                }

                if (RegDeclareLabel.IsMatch(line))
                {
                    LabelInfo label_info;
                    string label = RegDeclareLabel.Match(line).Groups[1].Value.ToUpper();
                    label = AddNamespace(label);

                    if (symbolTable.TryGetValue(label, out label_info))
                    {
                        if (symbolTable[label].Type == SymbolType.Undefined ||
                            symbolTable[label].Type == SymbolType.Export)
                        {
                            symbolTable[label].Address = outfile.Count;
                            if (symbolTable[label].Type == SymbolType.Undefined)
                            {
                                symbolTable[label].Type = SymbolType.Local;
                            }
                        }
                        else
                        {
                            if (symbolTable[label].Type == SymbolType.Local)
                            {
                                Console.Error.WriteLine("Error : Label already defined.");
                                Environment.Exit(1);
                            }
                        }
                    }
                    else
                    {
                        symbolTable.Add(AddNamespace(label),
                            new LabelInfo(SymbolType.Local, outfile.Count));
                    }

                }
                else // si ce n'est pas un label ça doit être une instruction
                {
                    string[] words = line.Split(' ');
                    string macro = words[0].ToLower();
                    switch (macro)
                    {
                        // opérations sur la mémoire (registres, RAM, etc.)
                        case "set":
                            outfile.Add(Instr_set(words[1], words[2]));
                            break;
                        // opérations arithmétiques
                        case "add":
                            outfile.Add(Instr_add(words[1], words[2]));
                            break;
                        case "addreg":
                            outfile.Add(Instr_addreg(words[1], words[2]));
                            break;
                        case "sub":
                            outfile.Add(Instr_sub(words[1], words[2]));
                            break;
                        // opérations de contrôle
                        case "halt":
                            outfile.Add((byte)0x00);
                            break;
                        case "nop":
                            outfile.Write(Instr_nop());
                            break;
                        case "trc":
                            outfile.Write(Instr_trc());
                            break;
                        // macro
                        case "zero":
                            outfile.Add(Instr_set(words[1], "0"));
                            break;
                        // sauts et branchements
                        case "jmp":
                            goto case "jump";
                        case "jump":
                        case "call":
                            LabelInfo lbl_infos = new LabelInfo();
                            string qualified_label = AddNamespace(words[1]);
                            if (symbolTable.TryGetValue(qualified_label, out lbl_infos))
                            {
                                outfile.Add(Instr_arity1_addr16(lbl_infos.Address, macro));
                                symbolTable[qualified_label].PositionToPatch.Add(outfile.Count - 2);
                            }
                            else
                            {
                                outfile.Add(Instr_arity1_addr16(0x00, macro));
                                LabelInfo tmp = new LabelInfo();
                                tmp.PositionToPatch.Add(outfile.Count - 2);
                                symbolTable.Add(qualified_label, tmp);
                            }
                            break;
                        case "jl":
                            goto case "je";
                        case "jg":
                            goto case "je";
                        case "jle":
                            goto case "je";
                        case "jge":
                            goto case "je";
                        case "je": // je <reg0> <reg1> <addr (label)>
                            //addr = 0;
                            if (words.Length < 4)
                            {
                                Console.WriteLine("Missing argument at line {0}", lineNumber);
                                Environment.Exit(1);
                            }
                            byte reg0 = ParseRegister(words[1]);
                            byte reg1 = ParseRegister(words[2]);
                            qualified_label = AddNamespace(words[3]);
                            if (symbolTable.TryGetValue(qualified_label, out lbl_infos))
                            {
                                outfile.Add(Conditional_branch(macro, lbl_infos.Address, reg0, reg1));
                                symbolTable[qualified_label].PositionToPatch.Add(outfile.Count - 2);
                            }
                            else
                            {
                                outfile.Add(Conditional_branch(macro, 0x00, reg0, reg1));
                                LabelInfo tmp = new LabelInfo();
                                tmp.PositionToPatch.Add(outfile.Count - 2);
                                symbolTable.Add(qualified_label, tmp);
                            }
                            break;

                        case "jne":
                            // JNE reg0 reg1 LBL est réécrit en :
                            //    JE reg0 reg1 adresse_courante + 7
                            //    JUMP LBL
#if TEMP
                        addr = 0;
                        if (words.Length < 4)
                        {
                            Console.WriteLine("Missing argument at line {0}", lineNumber);
                            Environment.Exit(1);
                        }
                        reg0 = ParseRegister(words[1]);
                        reg1 = ParseRegister(words[2]);

                        int offset = outfile.Count;
                        // 4 taille de JE, 3 taille de JUMP
                        outfile.Add(Instr_je(reg0, reg1, offset + 4 + 3));

                        if (labelList.TryGetValue(words[3], out addr))
                        {
                            outfile.Add(Instr_jmp(addr));
                        }
                        else
                        {
                            outfile.Add(Instr_jmp(0x00));
                            missingLabel.Add(words[3], outfile.Count - 2);
                        }

                        //outfile.Add(Instr_nop());
#endif
                            break;

                        case "and":
                        case "or":
                            outfile.Add(Instr_arity2_scal5_reg3(words[1], words[2], macro));
                            break;
                        case "push":
                        case "pop":
                        case "jmpreg":
                        case "pusha":
                        case "pushb":
                        case "pushw":
                        case "popa":
                        case "popb":
                        case "popw":
                            outfile.Add(Instr_arity1_scal5_reg3(words[1], macro));
                            break;
                        case "swap":
                        case "setreg":
                        case "andreg":
                        case "orreg":
                        case "xorreg":
                            outfile.Add(Instr_arity2_nul2_reg3_reg3(words[1], words[2], macro));
                            break;
                        case "subreg":
                            outfile.Add(Instr_subreg(words[1], words[2]));
                            break;
                        case "popt":
                            outfile.Add((byte)0x10);
                            break;
                        case "setbig":
                            outfile.Add(Instr_setbig(words[1], words[2]));
                            break;
                        case "store":
                            outfile.Add(Instr_store(words[1], words[2]));
                            break;
                        default:
                            Console.WriteLine("Error : macro instruction {0} doesn't exist.", macro);
                            Environment.Exit(1);
                            break;
                    }
                }

            }

        }

        private void NewCompile(string[] args)
        {
            codeFile = new StreamReader(args[0]);
            BinaryWriter resultingFile = new BinaryWriter(File.Create(args[1]));
            StreamWriter DEBUGhumanSymbolTable = new StreamWriter("dbgTable.txt");

            ProcessMeta();
            ProcessCode();

            Second_label_pass();
            codeFile.Close();

            byte[] st = GenerateSymbolTable();
            foreach (byte b in Encode_2bytes_addr(st.Length))
            {
                resultingFile.Write(b);
            }
            foreach (byte b in st)
            {
                resultingFile.Write(b);
            }
            foreach (byte b in outfile)
            {
                resultingFile.Write(b);
            }
            resultingFile.Flush();
            resultingFile.Close();

            DEBUGhumanSymbolTable.Write(DEBUGGenerateHumanSymbolTable());
            DEBUGhumanSymbolTable.Flush();
            DEBUGhumanSymbolTable.Close();
        }

        private byte[] Instr_arity2_scal5_reg3(string reg, string nb, string opcode)
        {
            if (!OpcodeDictionary.ContainsKey(opcode))
            {
                Console.Error.WriteLine("Internal error for line {1}. Opcode [{0}] not in OpcodeDictionary", opcode, lineNumber);
            }
            byte[] result = new byte[2];
            result[0] = OpcodeDictionary[opcode];
            result[1] = Encode_scal5_reg3(ParseByte(nb), ParseRegister(reg));
            return result;
        }

        private byte[] Instr_arity1_scal5_reg3(string reg, string opcode)
        {
            if (!OpcodeDictionary.ContainsKey(opcode))
            {
                Console.Error.WriteLine("Internal error for line {1}. Opcode [{0}] not in OpcodeDictionary", opcode, lineNumber);
            }
            byte[] result = new byte[2];
            result[0] = OpcodeDictionary[opcode];
            result[1] = Encode_scal5_reg3(0, ParseRegister(reg));
            return result;
        }

        private byte[] Instr_arity2_nul2_reg3_reg3(string reg0, string reg1, string opcode)
        {
            if (!OpcodeDictionary.ContainsKey(opcode))
            {
                Console.Error.WriteLine("Internal error for line {1}. Opcode [{0}] not in OpcodeDictionary", opcode, lineNumber);
            }
            byte[] result = new byte[2];
            result[0] = OpcodeDictionary[opcode];
            result[1] = Encode_nul2_reg3_reg3(ParseRegister(reg0), ParseRegister(reg1));
            return result;
        }

        private byte[] Instr_arity1_addr16(int addr, string opcode)
        {
            if (!OpcodeDictionary.ContainsKey(opcode))
            {
                Console.Error.WriteLine("Internal error for line {1}. Opcode [{0}] not in OpcodeDictionary", opcode, lineNumber);
            }
            byte[] result = new byte[3];
            result[0] = OpcodeDictionary[opcode];
            byte[] tmp = Encode_2bytes_addr(addr);
            result[1] = tmp[0];
            result[2] = tmp[1];
            return result;
        }

    }
}
