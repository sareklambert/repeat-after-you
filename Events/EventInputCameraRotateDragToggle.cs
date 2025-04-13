namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera rotate drag input was pressed or let go.
    /// </summary>
    public class EventInputCameraRotateDragToggle
    {
        public bool Start { get; private set; }

        public EventInputCameraRotateDragToggle(bool start)
        {
            Start = start;
        }
    }
}