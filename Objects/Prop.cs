using Plamb.LevelEditor.Core;

namespace Plamb.LevelEditor.Placeables
{
    /// <summary>
    /// Class <c>Prop</c> defines objects that can be placed on platforms.
    /// </summary>
    public class Prop : LevelEditorObject
    {
        // Define properties
        public SubgridId SubgridId { get; set; }
    }
}
