using System;
using System.Collections.Generic;
using System.Text;

namespace NoiseBot.Exceptions
{
    class InvalidConfigException : Exception
    {
        public InvalidConfigException(string message) : base(message) { }
    }
}
