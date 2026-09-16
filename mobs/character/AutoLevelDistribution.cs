using System;

namespace BridgeTroll
{
    /// <summary>
    /// Defines allocation fractions for auto-distributing stat points upon leveling up
    /// for non-player controlled mobs. The three fractions should add to 1.0.
    /// </summary>
    public struct AutoLevelDistribution
    {
        public float strength_allocation_fraction;
        public float charisma_allocation_fraction;
        public float intelligence_allocation_fraction;

        public AutoLevelDistribution(
            float strengthAllocationFraction,
            float charismaAllocationFraction,
            float intelligenceAllocationFraction
        )
        {
            strength_allocation_fraction = strengthAllocationFraction;
            charisma_allocation_fraction = charismaAllocationFraction;
            intelligence_allocation_fraction = intelligenceAllocationFraction;
        }

        public bool IsValid(float tolerance = 0.01f)
        {
            float sum = strength_allocation_fraction + charisma_allocation_fraction + intelligence_allocation_fraction;
            return Math.Abs(sum - 1.0f) <= tolerance;
        }
    }
}
