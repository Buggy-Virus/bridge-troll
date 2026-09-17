using System;

namespace BridgeTroll
{
    public struct LaborCost
    {
        public float labor_strength;

        public LaborCost(float laborStrength)
        {
            labor_strength = Math.Max(0f, laborStrength);
        }

        public bool IsComplete => labor_strength <= 0f;

        public void ApplyLabor(float amount)
        {
            labor_strength = Math.Max(0f, labor_strength - amount);
        }

        public void ApplyLabor(LaborCost labor)
        {
            ApplyLabor(labor.labor_strength);
        }

        public static LaborCost operator -(LaborCost a, LaborCost b)
        {
            return new LaborCost(Math.Max(0f, a.labor_strength - b.labor_strength));
        }

        public static LaborCost operator +(LaborCost a, LaborCost b)
        {
            return new LaborCost(a.labor_strength + b.labor_strength);
        }

        public override string ToString()
        {
            return $"Labor(strength: {labor_strength})";
        }
    }
}
