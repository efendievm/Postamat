using System;

namespace Postamat.Exceptions
{
    public class IncorrectPhoneNumber : Exception
    {
        public string Number { get; private set; }
        public IncorrectPhoneNumber(string Number) : base($"Invalid phone number: {Number}") => this.Number = Number;
    }
}