using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Class <c>ConnectionId</c> poses and an unambiguous identifier that points to an object placed on a subgrid,
    /// including its slot. It is internally represented in the following format:
    /// LEVEL - SLOT - PLATFORM-COLUMN - PLATFORM-ROW - SUB-COLUMN - SUB-ROW
    /// </summary>
    [Serializable]
    public class ConnectionId
    {
        [SerializeField] private int m_slot;
        [SerializeField] private PlatformId m_platformId;
        [SerializeField] private SubgridId m_subgridId;
        
        public int Slot => m_slot;
        public PlatformId PlatformId => m_platformId;
        public SubgridId SubgridId => m_subgridId;
        
        // Constructors
        public ConnectionId(string connectionId)
        {
            Regex re = new Regex(@"(\d+)(\d)([a-zA-Z])(\d+)([a-zA-Z])(\d+)", RegexOptions.Compiled);
            Match match = re.Matches(connectionId)[0];
            
            m_slot = int.Parse(match.Groups[2].Value);
            m_platformId = new PlatformId(string.Concat(match.Groups[1].Value, match.Groups[3].Value,
                match.Groups[4].Value));
            m_subgridId = new SubgridId(string.Concat(match.Groups[5].Value, match.Groups[6].Value));
        }
        public ConnectionId(int slot, string platformId, string subgridId)
        {
            m_slot = slot;
            m_platformId = new PlatformId(platformId);
            m_subgridId = new SubgridId(subgridId);
        }
        public ConnectionId(int slot, PlatformId platformId, SubgridId subgridId)
        {
            m_slot = slot;
            m_platformId = platformId;
            m_subgridId = subgridId;
        }
        public ConnectionId(int level, int slot, int col, int row, int subRow, int subCol)
        {
            m_slot = slot;
            m_platformId = new PlatformId(level, col, row);
            m_subgridId = new SubgridId(subCol, subRow);
        }

        /// <summary>
        /// Method <c>AsString</c> returns the objects string representation. Can be used for serialisation, as this
        /// string can be used with the class's constructor.
        /// </summary>
        /// <returns>The objects string representation</returns>
        public string AsString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(m_platformId.Level);
            sb.Append(m_slot);
            sb.Append(m_platformId.ColChar);
            sb.Append(m_platformId.Row);
            sb.Append(m_subgridId.ColChar);
            sb.Append(m_subgridId.Row);
            return sb.ToString();
        }
    }
}
