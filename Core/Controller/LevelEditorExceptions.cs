using System;

namespace Plamb.LevelEditor.Core
{
    public class InvalidConnectionError : Exception
    {
        public InvalidConnectionError()
        {
            
        }

        public InvalidConnectionError(string message) : base(message)
        {
            
        }

        public InvalidConnectionError(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
