using System;

namespace NRobot.Server.Exceptions
{
    /// <summary>
    /// Desctiption of FatalKeywordException.
    /// </summary>
    public class FatalKeywordException : Exception
    {
        public FatalKeywordException() { }

        public FatalKeywordException(string message)
            : base(message) { }

        public FatalKeywordException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
