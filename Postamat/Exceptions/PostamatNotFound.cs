using System;

namespace Postamat.Exceptions
{
    public class PostamatNotFound : Exception
    {
        public string Number { get; private set; }
        public PostamatNotFound(string Number) : base($"Postamat {Number} not found") => this.Number = Number;
    }
}
