using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MagicVM_Assembler;

namespace Linker
{
    partial class Linker
    {

        Dictionary<string, LabelInfo> symbolTable = new Dictionary<string, LabelInfo>();
        int offset = 0;
        List<byte> code = new List<byte>();
        int currentCodeSize = 0;
        BinaryReader file;

        static void Main(string[] args)
        {
            BinaryReader fileTmp;
            Linker linker = new Linker();

            if (args.Length < 1)
            {
                Console.WriteLine("Missing arguments");
                Console.WriteLine("usage : linker <file> [<file2> ... <fileN>]");
                Environment.Exit(1);
            }

            string outfile_name = "output.exe";
            string file0 = args[0];
            if(File.Exists(file0))
            {
                FileInfo fi = new FileInfo(file0);
                outfile_name = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length);
                outfile_name += ".mexe";
            }

            foreach (string fileName in args)
            {
                if (File.Exists(fileName))
                {
                    fileTmp = new BinaryReader(File.OpenRead(fileName));
                    linker.ReadSymbolTable(fileTmp);
                    linker.LoadCode();
                    linker.offset += linker.currentCodeSize;
                    linker.offset++; // car on ajoute un octet de garde nul entre le code des différents fichiers

                    fileTmp.Close();
                }
                else
                {
                    Console.WriteLine("Error : file {0} doesn't exists", fileName);
                    Environment.Exit(1);
                }
            }
            linker.Patch();
            linker.WriteOutput(outfile_name);
            if (true)
            {
                StreamWriter c_code = new StreamWriter("code.gen.c");
                c_code.Write(linker.Get_C_Code());
                c_code.Flush();
                c_code.Close();
            }
        }

        private void LoadCode()
        {
            for (int i = 0; i < currentCodeSize; i++)
            {
                code.Add(file.ReadByte());
            }
            code.Add((byte)0x00);
        }

        private void Patch()
        {
            foreach (KeyValuePair<string, LabelInfo> entry in symbolTable)
            {
                byte[] tmpAddr = new byte[2];
                foreach (int position in entry.Value.PositionToPatch)
                {
                    tmpAddr = Encode_2bytes_addr(entry.Value.Address);
                    code[position] = tmpAddr[0];
                    code[position + 1] = tmpAddr[1];
                }
            }
        }

        private void ReadSymbolTable(BinaryReader f)
        {
            file = f;
            int tableSize = file.ReadByte();
            tableSize <<= 8;
            tableSize |= file.ReadByte();    

            currentCodeSize = (int)file.BaseStream.Length - tableSize - 2;

            int i = 0;
            while(i < tableSize)
            {
                i += ReadRecord();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Nombre d'octets lus.</returns>
        private int ReadRecord()
        {
            int recSize = 5;
            LabelInfo record = new LabelInfo();
            switch (file.ReadChar())
            {
                // dans chaque cas, il faut traiter le type du label lu
                // en fonction de celui déjà présent dans la table
                case 'L':
                    record.Type = SymbolType.Local;
                    break;
                case 'E':
                    record.Type = SymbolType.Export;
                    break;
                case 'U':
                    record.Type = SymbolType.Undefined;
                    break;
                default:
                    break;
            }
            record.Address = file.ReadByte();
            record.Address <<= 8;
            record.Address |= file.ReadByte();
            record.Address += offset;
            byte labelLength = file.ReadByte();
            recSize += (int)labelLength;
            char[] c_labelName = new char[labelLength];
            file.Read(c_labelName, 0, labelLength);
            string labelName = new string(c_labelName);

            int nbPatch = file.ReadByte();
            recSize += (2 * nbPatch);

            for (int i = 0; i < nbPatch; i++)
            {
                int temp = file.ReadByte();
                temp <<= 8;
                temp |= file.ReadByte();
                record.PositionToPatch.Add(temp + offset);
            }

            if (symbolTable.ContainsKey(labelName))
            {
                switch (symbolTable[labelName].Type)
                {
                    case SymbolType.Local:
                        proc1(record, labelName);
                        break;
                    case SymbolType.Export:
                        proc2(record, labelName);
                        break;
                    case SymbolType.Undefined:
                        proc3(record, labelName);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                symbolTable.Add(labelName, record);
            }
            return recSize;
        }

        // cas d'un nouveau record lorsqu'un existe avec l'attribut L
        private void proc1(LabelInfo readedRecord, string existingLabel)
        {
            switch (readedRecord.Type)
            {
                case SymbolType.Local:
                    DuplicateError(existingLabel);
                    break;
                case SymbolType.Export:
                    DuplicateError(existingLabel);
                    break;
                case SymbolType.Undefined:
                    //symbolTable.
                    DuplicateError(existingLabel);
                    break;
                default:
                    break;
            }
        }

        // cas d'un nouveau record lorsqu'un existe avec l'attribut E
        private void proc2(LabelInfo readedRecord, string existingLabel)
        {
            switch (readedRecord.Type)
            {
                case SymbolType.Local:
                    DuplicateError(existingLabel);
                    break;
                case SymbolType.Export:
                    DuplicateError(existingLabel);
                    break;
                case SymbolType.Undefined:
                    foreach (int addr in readedRecord.PositionToPatch)
                    {
                        symbolTable[existingLabel].PositionToPatch.Add(addr);
                    }
                    break;
                default:
                    break;
            }
        }

        // cas d'un nouveau record lorsqu'un existe avec l'attribut U
        private void proc3(LabelInfo readedRecord, string existingLabel)
        {
            switch (readedRecord.Type)
            {
                case SymbolType.Local:
                    DuplicateError(existingLabel);
                    break;
                case SymbolType.Export:
                    symbolTable[existingLabel].Type = SymbolType.Export;
                    symbolTable[existingLabel].Address = readedRecord.Address;
                    foreach (int addr in readedRecord.PositionToPatch)
                    {
                        symbolTable[existingLabel].PositionToPatch.Add(addr);
                    }
                    break;
                case SymbolType.Undefined:
                    foreach (int addr in readedRecord.PositionToPatch)
                    {
                        symbolTable[existingLabel].PositionToPatch.Add(addr);
                    }
                    break;
                default:
                    break;
            }
        }

        private void DuplicateError(string arg = "")
        {
            Console.WriteLine("Error symbol {0} already defined.", arg);
            Environment.Exit(1);
        }

        public void WriteOutput(string fileName)
        {
            BinaryWriter outFile = new BinaryWriter(File.Open(fileName, FileMode.Create));
            outFile.Write(code.ToArray());
            outFile.Flush();
            outFile.Close();
        }

        private byte[] Encode_2bytes_addr(int addr)
        {
            byte[] result = new byte[2];
            int tmp = addr & 0xFF00; // MSD
            result[0] = (byte)tmp;
            tmp = addr & 0xFF; // LSB
            result[1] = (byte)tmp;
            return result;
        }
    }
}
