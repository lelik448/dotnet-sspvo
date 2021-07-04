using System;

namespace SsPvo.Client.Messages.Exceptions
{
    public class SsPvoMessageRequestPreparationException : Exception
    {
        public SsPvoMessageRequestPreparationException()
        {
        }

        public SsPvoMessageRequestPreparationException(string message)
            : base(message)
        {
        }

        public SsPvoMessageRequestPreparationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
