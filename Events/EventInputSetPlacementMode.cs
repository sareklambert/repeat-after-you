using Plamb.LevelEditor;

namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Placement mode was changed.
    /// </summary>
    public class EventInputSetPlacementMode
    {
        public PlacementMode Mode { get; private set; }

        public EventInputSetPlacementMode(PlacementMode mode)
        {
            Mode = mode;
        }
    }
}
