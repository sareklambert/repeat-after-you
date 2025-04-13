using UnityEngine;

namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera pan input was pressed.
    /// </summary>
    public class EventInputCameraPanKey
    {
        public Vector2 InputVector { get; private set; }

        public EventInputCameraPanKey(Vector2 vector)
        {
            InputVector = vector;
        }
    }
}
