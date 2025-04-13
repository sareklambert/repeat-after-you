using System;
using System.Collections.Generic;
using Plamb.LevelEditor.Placeables;
using UnityEngine;

namespace Plamb.LevelEditor.Core
{
    [Serializable]
    public class TableObject
    {
        public string id;
        public LevelEditorObject prefab;
        
        public Sprite displaySprite;
        public string displayName;
        public string description;
        public string filter;
    }
    
    [CreateAssetMenu(fileName = "LookupTable", menuName = "LevelEditor/LookupTable", order = 1)]
    public class LookupTableObject : ScriptableObject
    {
        public List<TableObject> objectList = new List<TableObject>();
    }
}
