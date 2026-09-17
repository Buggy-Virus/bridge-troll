using System;

namespace BridgeTroll
{
    /// <summary>
    /// Represents the cost in feat points (Strength, Intelligence, Charisma)
    /// required to unlock a feat.
    /// </summary>
    public struct FeatPointCost
    {
        public int strength_cost;
        public int intelligence_cost;
        public int charisma_cost;

        public FeatPointCost(int strength = 0, int intelligence = 0, int charisma = 0)
        {
            strength_cost = strength;
            intelligence_cost = intelligence;
            charisma_cost = charisma;
        }

        /// <summary>
        /// Checks whether the troll currently has sufficient feat points of all required types.
        /// </summary>
        public bool CanAfford(Troll troll)
        {
            if (troll == null) return false;
            return troll.strength_feat_points >= strength_cost
                && troll.intelligence_feat_points >= intelligence_cost
                && troll.charisma_feat_points >= charisma_cost;
        }

        /// <summary>
        /// Deducts the feat point costs from the troll. Returns true if successful.
        /// </summary>
        public bool Deduct(Troll troll)
        {
            if (!CanAfford(troll)) return false;

            troll.strength_feat_points -= strength_cost;
            troll.intelligence_feat_points -= intelligence_cost;
            troll.charisma_feat_points -= charisma_cost;
            return true;
        }

        public override string ToString()
        {
            return $"Cost: STR Feat: {strength_cost}, INT Feat: {intelligence_cost}, CHAR Feat: {charisma_cost}";
        }
    }
}
