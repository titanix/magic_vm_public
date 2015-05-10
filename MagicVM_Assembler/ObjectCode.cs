using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MagicVM_Assembler
{
    partial class Assembler
    {
        string Namespace = "";
        // STRING nom, SYMBOLTYPE type, INT adresse
        Dictionary<string, LabelInfo> symbolTable = new Dictionary<string, LabelInfo>();

        private string AddNamespace(string label)
        {
            if(!String.IsNullOrEmpty(Namespace))
            {
                if (IsUnqualifiedLabel(label))
                {
                    return Namespace + ":" + label;
                }
            }
            return label;
        }

        private bool IsUnqualifiedLabel(string label)
        {
            if (label.Contains(":"))
            {
                return false;
            }
            return true;
        }

        private byte[] GenerateSymbolTable()
        {
            List<byte> tableBin = new List<byte>();
            // 2 octets pour la taille de la table

            // 1 octet pour le type, 2 pour l'adresse
            // 1 pour la taille du nom, N pour le nom
            // 1 pour le nombre de position, 2 octets par position

            foreach (KeyValuePair<string, LabelInfo> lbl in symbolTable)
            {
                switch (lbl.Value.Type)
                {
                    case SymbolType.Local:
                        tableBin.Add((byte)0x4C); // L
                        break;
                    case SymbolType.Export:
                        tableBin.Add((byte)0x45); // E
                        break;
                    case SymbolType.Undefined:
                        tableBin.Add((byte)0x5C); // U
                        break;
                    default:
                        break;
                }
                tableBin.Add(Encode_2bytes_addr(lbl.Value.Address));
                tableBin.Add((byte)lbl.Key.Length);
                foreach (char c in lbl.Key) // TODO tronquer à 256
                {
                    tableBin.Add((byte)c);
                }
                tableBin.Add((byte)lbl.Value.PositionToPatch.Count);
                foreach (int position in lbl.Value.PositionToPatch)
                {
                    tableBin.Add(Encode_2bytes_addr(position));
                }
            }

            return tableBin.ToArray();
        }

        private string DEBUGGenerateHumanSymbolTable()
        {
            StringBuilder str = new StringBuilder();
            str.Append("label\t");
            str.Append("type\t");
            str.Append("adresse\t");
            str.Append("position\t");
            str.Append("\r\n");
            foreach (KeyValuePair<string, LabelInfo> lbl in symbolTable)
            {
                str.Append(lbl.Key + "\t");
                switch (lbl.Value.Type)
                {
                    case SymbolType.Local:
                        str.Append("L\t");
                        break;
                    case SymbolType.Export:
                        str.Append("E\t");
                        break;
                    case SymbolType.Undefined:
                        str.Append("U\t");
                        break;
                    default:
                        break;
                }
                str.Append(lbl.Value.Address + "\t");
                foreach (int pos in lbl.Value.PositionToPatch)
                {
                    str.Append(String.Format("[{0}] ", pos));
                }
                str.Append("\r\n");              
            }
            return str.ToString();
        }

    }
}