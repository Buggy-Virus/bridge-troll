using System;
using System.Collections.Generic;

namespace BridgeTroll
{
    /// <summary>
    /// Represents a specialized technology node that grants a major gameplay-altering feat.
    /// In addition to stat requirements and custom functional requirements, feats cost
    /// specific amounts of Strength, Intelligence, and Charisma feat points to unlock.
    /// </summary>
    public class Feat : UnlockableTechnology
    {
        /// <summary>
        /// Identifies the specific feat from the FeatType enum.
        /// </summary>
        public FeatType feat_type { get; set; } = FeatType.NONE;

        /// <summary>
        /// The feat points (Strength, Intelligence, Charisma) required to unlock this feat.
        /// </summary>
        public FeatPointCost feat_cost { get; set; } = new();

        /// <summary>
        /// List of prerequisite feats that must be unlocked prior to this feat.
        /// </summary>
        public List<FeatType> prerequisite_feats { get; set; } = new();

        public Feat() { }

        public Feat(
            FeatType featType,
            string name,
            string description,
            TechTreeBranch branch,
            FeatPointCost featCost,
            TechnologyStatRequirements statRequirements = default,
            Func<Troll, bool> canSeeRequirement = null,
            Func<Troll, bool> canUnlockRequirement = null,
            List<FeatType> prerequisiteFeats = null,
            Action<Troll> onUnlocked = null
        ) : base(
            type: TechnologyType.NONE,
            name: name,
            description: description,
            branch: branch,
            statRequirements: statRequirements,
            canSeeRequirement: canSeeRequirement,
            canUnlockRequirement: canUnlockRequirement,
            prerequisiteTechnologies: null,
            onUnlocked: onUnlocked
        )
        {
            this.feat_type = featType;
            this.feat_cost = featCost;
            this.prerequisite_feats = prerequisiteFeats ?? new List<FeatType>();
        }

        /// <summary>
        /// Evaluates whether the troll satisfies all criteria (feat point costs, stats, and custom predicates).
        /// </summary>
        public override bool CanBeUnlocked(Troll troll)
        {
            if (is_unlocked) return false;

            // Must have enough feat points
            if (!feat_cost.CanAfford(troll))
            {
                return false;
            }

            // Must satisfy base stat requirements and custom functional unlock condition
            return base.CanBeUnlocked(troll);
        }

        /// <summary>
        /// Unlocks the feat, deducting the feat point costs from the troll and applying unlock effects.
        /// </summary>
        public override bool Unlock(Troll troll)
        {
            if (!CanBeUnlocked(troll))
            {
                return false;
            }

            if (!feat_cost.Deduct(troll))
            {
                return false;
            }

            is_unlocked = true;
            on_unlocked?.Invoke(troll);
            return true;
        }
    }
}
