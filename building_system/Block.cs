using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BridgeTroll
{
    public class Block
    {
        public enum Type
        {
            None,
            Occupied,
        }

        public Type type { get; set; }

        public Block()
        {
            type = Type.None;
        }
    }
}
