using System;

namespace BridgeTroll
{
    /// <summary>
    /// Encapsulates the minimum base numerical stats required for a troll to unlock a technology.
    /// </summary>
    public struct TechnologyStatRequirements
    {
        public int required_intelligence;
        public int required_strength;
        public int required_charisma;
        public int required_level;

        public TechnologyStatRequirements(
            int intelligence = 0,
            int strength = 0,
            int charisma = 0,
            int level = 0
        )
        {
            required_intelligence = intelligence;
            required_strength = strength;
            required_charisma = charisma;
            required_level = level;
        }

        /// <summary>
        /// Evaluates whether the given troll meets these stat requirements.
        /// </summary>
        public bool MeetsRequirements(Troll troll)
        {
            if (troll == null) return false;
            return troll.base_intelligence >= required_intelligence
                && troll.base_strength >= required_strength
                && troll.base_charisma >= required_charisma
                && troll.level >= required_level;
        }

        public override string ToString()
        {
            return $"Reqs: INT>={required_intelligence}, STR>={required_strength}, CHAR>={required_charisma}, LVL>={required_level}";
        }
    }
}
