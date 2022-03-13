using System;

namespace Postamat.Exceptions
{
    public class PostamatClosed : Exception
    {
        public string Number { get; private set; }
        public PostamatClosed(string Number) : base($"Postamat {Number} is closed") => this.Number = Number;
    }
}
