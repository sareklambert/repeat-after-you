using System;
using System.Text;
using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>SubgridId</c> poses and an unambiguous identifier that points to an object's position on a platform's
    /// subgrid. It is internally represented in the following format:
    /// Subgrid-COLUMN - Subgrid-ROW
    /// </summary>
    [Serializable]
    public sealed class SubgridId
    {
        [SerializeField] private char col;
        [SerializeField] private int row;
        
        public int Column => Alphabet.LookupTable[col];
        public char ColChar => col;
        public int Row => row;
        
        public override bool Equals(object obj)
        {
            if (obj is SubgridId other)
            {
                return this.col == other.col && this.row == other.row;
            }
            return false;
        }

        private bool Equals(SubgridId other)
        {
            return col == other.col && row == other.row;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(col, row);
        }

        // Constructors
        public SubgridId(string subgridId)
        {
            col = subgridId[0];
            row = subgridId[1] - '0';  // This is the most efficient way to cast a char to an int. #ASCII-Magic
        }
        public SubgridId(char col, int row)
        {
            this.col = col;
            this.row = row;
        }
        public SubgridId(int col, int row)
        {
            this.col = (char)('A' + col);
            this.row = row;
        }

        public string AsString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(col);
            sb.Append(row);
            return sb.ToString();
        }
    }
}
