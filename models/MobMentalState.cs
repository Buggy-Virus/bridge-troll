using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BridgeTroll
{
    public enum GoalType {
        NONE,
        CROSS,
        VISIT,
        BUSINESS,
        IMMIGRATE,
        FOOD,
        ENTERTAINMENT,
        BUY,
        SELL,
        ATTACK,
        SLAY,
    }

    public struct Thought {

    }

    public class MobMentalState
    {
        public GoalType main_goal;
        public float energy;
        public float hunger;
        public float thirst;
        public float fear;
        public float entertainment_appetite;
        public float boredom;
        public float frustration;
        public List<Thought> thoughts;
    }
}