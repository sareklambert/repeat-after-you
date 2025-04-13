namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera zoom input was pressed or let go.
    /// </summary>
    public class EventInputCameraZoomToggle
    {
        public bool Start { get; private set; }

        public EventInputCameraZoomToggle(bool start)
        {
            Start = start;
        }
    }
}