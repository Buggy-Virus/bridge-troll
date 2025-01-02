using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BridgeTroll;

namespace BridgeTroll
{
    public class Block
    {
        public enum Type
        {
            None,
            Clear,
            Occupied,
        }

        public Type type { get; set; }

        public Block()
        {
            type = Type.None;
        }
    }
}
