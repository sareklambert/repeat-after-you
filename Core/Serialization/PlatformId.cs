using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>PlatformId</c> poses and an unambiguous identifier that points to a platform's position.
    /// It is internally represented in the following format:
    /// LEVEL - Platform-COLUMN - Platform-ROW
    /// </summary>
    [Serializable]
    public sealed class PlatformId
    {
        [SerializeField] private char col;
        [SerializeField] private int row;
        [SerializeField] private int level;

        public int Column => Alphabet.LookupTable[col];

        public char ColChar => col;
        public int Level => level;
        public int Row => row;

        // Constructors
        public PlatformId(string platformId)
        {
            Regex re = new Regex(@"(\d)([a-zA-Z])(\d{1,2})", RegexOptions.Compiled);
            MatchCollection matches = re.Matches(platformId);

            int.TryParse(matches[0].Groups[1].Value, out level);
            char.TryParse(matches[0].Groups[2].Value, out col);
            int.TryParse(matches[0].Groups[3].Value, out row);
        }
        public PlatformId(int level, int col, int row)
        {
            this.level = level;
            this.col = (char)('A' + col);
            this.row = row;
        }
        public PlatformId(int level, char col, int row)
        {
            this.level = level;
            this.col = col;
            this.row = row;
        }

        public override bool Equals(object obj)
        {
            if (obj is PlatformId other)
            {
                return this.level == other.level && this.col == other.col && this.row == other.row;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                // Numbers 17 and 23 are to sort of "randomize" the hash code so it is less likely to generate the
                // same one when we shouldn't
                int hash = 17;
                
                // Suitable nullity checks etc., of course :)
                hash = hash * 23 + level.GetHashCode();
                hash = hash * 23 + col.GetHashCode();
                hash = hash * 23 + row.GetHashCode();
                return hash;
            }
        }

        public string AsString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(level);
            sb.Append(col);
            sb.Append(row);
            return sb.ToString();
        }
    }
}
