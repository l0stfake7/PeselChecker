using System;

namespace PeselChecker.Classes
{
    public sealed class PeselNumberException : ApplicationException
    {
        public PeselNumberException(string info)
            : base(info)
        {

        }
    }
}
