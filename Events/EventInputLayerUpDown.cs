namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Layer switch input was pressed.
    /// </summary>
    public class EventInputLayerUpDown
    {
        public int InputValue { get; private set; }

        public EventInputLayerUpDown(int inputValue)
        {
            InputValue = inputValue;
        }
    }
}
