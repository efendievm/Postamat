using System;

namespace Postamat.Exceptions
{
    public class IncorrectPostamatNumber : Exception
    {
        public string Number { get; private set; }
        public IncorrectPostamatNumber(string Number) : base($"Invalid postamat number: {Number}") => this.Number = Number;
    }
}
