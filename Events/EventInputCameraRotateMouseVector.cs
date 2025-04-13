using UnityEngine;

namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera rotate mouse input was pressed.
    /// </summary>
    public class EventInputCameraRotateMouseVector
    {
        public Vector2 InputVector { get; private set; }

        public EventInputCameraRotateMouseVector(Vector2 vector)
        {
            InputVector = vector;
        }
    }
}