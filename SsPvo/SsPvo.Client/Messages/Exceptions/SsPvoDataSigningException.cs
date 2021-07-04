using System;

namespace SsPvo.Client.Messages.Exceptions
{
    public class SsPvoDataSigningException : Exception
    {
        public SsPvoDataSigningException()
        {
        }

        public SsPvoDataSigningException(string message)
            : base(message)
        {
        }

        public SsPvoDataSigningException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
