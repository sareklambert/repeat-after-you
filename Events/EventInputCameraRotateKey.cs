namespace Plamb.Events.LevelEditor
{
    /// <summary>
    /// Camera direct rotate input was pressed.
    /// </summary>
    public class EventInputCameraRotateKey
    {
        public float InputValue { get; private set; }

        public EventInputCameraRotateKey(float value)
        {
            InputValue = value;
        }
    }
}
