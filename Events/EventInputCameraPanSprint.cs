namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera sprint input was pressed or let go.
    /// </summary>
    public class EventInputCameraPanSprint
    {
        public bool Start { get; private set; }

        public EventInputCameraPanSprint(bool start)
        {
            Start = start;
        }
    }
}
