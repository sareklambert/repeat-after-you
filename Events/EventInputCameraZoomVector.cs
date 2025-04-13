using UnityEngine;

namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera zoom input was used.
    /// </summary>
    public class EventInputCameraZoomVector
    {
        public Vector2 InputVector { get; private set; }

        public EventInputCameraZoomVector(Vector2 vector)
        {
            InputVector = vector;
        }
    }
}
