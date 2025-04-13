namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera drag input was pressed or let go.
    /// </summary>
    public class EventInputCameraPanDrag
    {
        public bool Performed { get; private set; }

        public EventInputCameraPanDrag(bool performed)
        {
            Performed = performed;
        }
    }
}
