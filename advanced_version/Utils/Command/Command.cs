using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils
{
    public abstract class Command
    {
        public bool Concurrent { get; }

        protected Command(bool concurrent)
        {
            Concurrent = concurrent;
        }

        abstract public Task ExecuteAsync(List<string> arguments);

    }
}
