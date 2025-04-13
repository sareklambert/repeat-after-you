namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Rotate selected object input was pressed.
    /// </summary>
    public class EventInputRotateSelectedObject
    {
        public int InputValue { get; private set; }

        public EventInputRotateSelectedObject(int inputValue)
        {
            InputValue = inputValue;
        }
    }
}
