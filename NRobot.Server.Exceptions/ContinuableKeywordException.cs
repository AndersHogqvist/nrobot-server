using System;

namespace NRobot.Server.Exceptions
{
    /// <summary>
    /// Desctiption of ContinuableKeywordException.
    /// </summary>
    public class ContinuableKeywordException : Exception
    {
        public ContinuableKeywordException() { }

        public ContinuableKeywordException(string message)
            : base(message) { }

        public ContinuableKeywordException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
