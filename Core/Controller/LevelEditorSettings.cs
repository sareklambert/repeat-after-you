using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    /// <summary>
    /// Defines level dimensions and performance settings.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelEditorSettings", menuName = "LevelEditor/LevelEditorSettings", order = 0)]
    public class LevelEditorSettings : ScriptableObject
    {
        public int gridMouseRaycastLength = 2000;
        public float gridCellUnitSizeSub = 0.5f;
        public int layerAmount = 10;
        public int subCellsPerMainCell = 8;
        public float gridSizeHalf = 24 * 4 * 0.5f; // 24 cells * 4 units, half of that
        public Vector2Int gridPositionOffsetMain = new Vector2Int(12, 11);
        public Vector2Int gridPositionOffsetSub = new Vector2Int(96, 95);
    }
}
