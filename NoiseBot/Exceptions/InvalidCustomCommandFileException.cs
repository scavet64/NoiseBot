using System;
using System.Collections.Generic;
using System.Text;

namespace NoiseBot.Exceptions
{
    class InvalidCustomCommandFileException : Exception
    {
        public InvalidCustomCommandFileException(string message) : base(message) { }
    }
}
