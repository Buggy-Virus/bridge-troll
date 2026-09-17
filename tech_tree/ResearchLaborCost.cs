using System;

namespace BridgeTroll
{
    /// <summary>
    /// Represents the labor costs in intelligence, strength, and charisma labor units
    /// required to research a technology.
    /// </summary>
    public struct ResearchLaborCost
    {
        public int labor_intelligence;
        public int labor_strength;
        public int labor_charisma;

        public ResearchLaborCost(int intelligence = 0, int strength = 0, int charisma = 0)
        {
            labor_intelligence = intelligence;
            labor_strength = strength;
            labor_charisma = charisma;
        }

        public bool IsZero => labor_intelligence <= 0 && labor_strength <= 0 && labor_charisma <= 0;

        public override string ToString()
        {
            return $"Labor Cost: INT={labor_intelligence}, STR={labor_strength}, CHAR={labor_charisma}";
        }
    }
}
