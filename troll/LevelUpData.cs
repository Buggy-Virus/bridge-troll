using System;

namespace BridgeTroll
{
    /// <summary>
    /// Holds the configuration data for a character level up,
    /// including total necessary experience and stat points earned.
    /// </summary>
    public struct LevelUpData
    {
        public int experience_needed;
        public int stat_points_earned;

        public LevelUpData(int experienceNeeded, int statPointsEarned = 5)
        {
            experience_needed = experienceNeeded;
            stat_points_earned = statPointsEarned;
        }
    }
}
