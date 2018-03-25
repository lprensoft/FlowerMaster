using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowerMaster.Helpers
{
    public sealed class Counter
    {

        private static readonly Lazy<Counter> lazy = new Lazy<Counter>(() => new Counter());
        public static Counter Instance { get { return lazy.Value; } }

        private int Current { get; set; }

        private Counter()
        {
        }

        public void Load( int Value )
        {
            Current = Value;
        }

        public void Decrease()
        {
            Current--;
        }

        public void Reset()
        {
            Current = 0;
        }

        public int Value()
        {
            return Current;
        }
    }
}
