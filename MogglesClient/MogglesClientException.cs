using System;

namespace MogglesClient
{
    internal class MogglesClientException: Exception
    {
        public MogglesClientException() { }

        internal MogglesClientException(string message): base(message) { }
    }
}
