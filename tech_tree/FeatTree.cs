using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeTroll
{
    /// <summary>
    /// Feat tree system inheriting from the general TechTree.
    /// Manages gameplay-altering feats unlocked using specialized feat points
    /// (Strength, Intelligence, Charisma) earned during level up.
    /// </summary>
    public class FeatTree : TechTree
    {
        /// <summary>
        /// Feats registered in this tree indexed by FeatType.
        /// </summary>
        public Dictionary<FeatType, Feat> feats { get; } = new();

        public FeatTree(string name = "Troll Feat Tree", bool initializeDefaults = true)
            : base(name, false)
        {
            if (initializeDefaults)
            {
                InitializeDefaultFeats();
            }
        }

        /// <summary>
        /// Registers a feat into the feat tree.
        /// </summary>
        public void RegisterFeat(Feat feat)
        {
            if (feat == null || feat.feat_type == FeatType.NONE) return;

            feats[feat.feat_type] = feat;

            if (!branch_technologies.ContainsKey(feat.branch))
            {
                branch_technologies[feat.branch] = new List<UnlockableTechnology>();
            }

            if (!branch_technologies[feat.branch].Contains(feat))
            {
                branch_technologies[feat.branch].Add(feat);
            }
        }

        /// <summary>
        /// Initializes default starter feats across the Might, Intellect, and Influence branches.
        /// </summary>
        public virtual void InitializeDefaultFeats()
        {
            // --- Might Branch Feats ---
            var groundSlam = new Feat(
                featType: FeatType.GROUND_SLAM,
                name: "Ground Slam",
                description: "The troll forcefully smashes the bridge surface, stunning and damaging nearby mobs.",
                branch: TechTreeBranch.MIGHT,
                featCost: new FeatPointCost(strength: 2),
                statRequirements: new TechnologyStatRequirements(strength: 12),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) => troll != null && troll.base_strength >= 12,
                onUnlocked: (troll) =>
                {
                    troll.stats.base_damage += 2;
                    troll.RecalculateDependentStats();
                }
            );
            RegisterFeat(groundSlam);

            var titanGrip = new Feat(
                featType: FeatType.TITAN_GRIP,
                name: "Titan Grip",
                description: "Victims caught in the troll's grapple can no longer squirm or attempt to escape.",
                branch: TechTreeBranch.MIGHT,
                featCost: new FeatPointCost(strength: 3),
                statRequirements: new TechnologyStatRequirements(strength: 14),
                canSeeRequirement: (troll) => IsFeatUnlocked(FeatType.GROUND_SLAM),
                canUnlockRequirement: (troll) => IsFeatUnlocked(FeatType.GROUND_SLAM) && troll.base_strength >= 14,
                prerequisiteFeats: new List<FeatType> { FeatType.GROUND_SLAM }
            );
            RegisterFeat(titanGrip);

            // --- Intellect Branch Feats ---
            var eagleEyeToll = new Feat(
                featType: FeatType.EAGLE_EYE_TOLL,
                name: "Eagle Eye Toll",
                description: "Scouts approaching travelers from a distance to reveal their exact carried gold and items before grappling.",
                branch: TechTreeBranch.INTELLECT,
                featCost: new FeatPointCost(intelligence: 2),
                statRequirements: new TechnologyStatRequirements(intelligence: 12),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) => troll != null && troll.base_intelligence >= 12
            );
            RegisterFeat(eagleEyeToll);

            var runicBridge = new Feat(
                featType: FeatType.RUNIC_BRIDGE,
                name: "Runic Bridge",
                description: "Inscribes protective runes along the bridge path, granting passive health regeneration and magic resistance.",
                branch: TechTreeBranch.INTELLECT,
                featCost: new FeatPointCost(intelligence: 3),
                statRequirements: new TechnologyStatRequirements(intelligence: 14),
                canSeeRequirement: (troll) => IsFeatUnlocked(FeatType.EAGLE_EYE_TOLL),
                canUnlockRequirement: (troll) => IsFeatUnlocked(FeatType.EAGLE_EYE_TOLL) && troll.base_intelligence >= 14,
                prerequisiteFeats: new List<FeatType> { FeatType.EAGLE_EYE_TOLL },
                onUnlocked: (troll) =>
                {
                    troll.stats.base_max_hit_points += 5;
                    troll.RecalculateDependentStats();
                }
            );
            RegisterFeat(runicBridge);

            // --- Influence Branch Feats ---
            var silverTongue = new Feat(
                featType: FeatType.SILVER_TONGUE,
                name: "Silver Tongue",
                description: "Extorting travelers yields double gold without immediately provoking combat or severe kingdom wrath.",
                branch: TechTreeBranch.INFLUENCE,
                featCost: new FeatPointCost(charisma: 2),
                statRequirements: new TechnologyStatRequirements(charisma: 12),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) => troll != null && troll.base_charisma >= 12
            );
            RegisterFeat(silverTongue);

            var terrifyingPresence = new Feat(
                featType: FeatType.TERRIFYING_PRESENCE,
                name: "Terrifying Presence",
                description: "Weak mobs (e.g. peasants, traders) surrender instantly upon entering the bridge.",
                branch: TechTreeBranch.INFLUENCE,
                featCost: new FeatPointCost(charisma: 3),
                statRequirements: new TechnologyStatRequirements(charisma: 14),
                canSeeRequirement: (troll) => IsFeatUnlocked(FeatType.SILVER_TONGUE),
                canUnlockRequirement: (troll) => IsFeatUnlocked(FeatType.SILVER_TONGUE) && troll.base_charisma >= 14,
                prerequisiteFeats: new List<FeatType> { FeatType.SILVER_TONGUE },
                onUnlocked: (troll) =>
                {
                    troll.stats.base_scary += 5;
                    troll.RecalculateDependentStats();
                }
            );
            RegisterFeat(terrifyingPresence);
        }

        /// <summary>
        /// Retrieves a feat node by FeatType.
        /// </summary>
        public Feat GetFeat(FeatType featType)
        {
            feats.TryGetValue(featType, out var feat);
            return feat;
        }

        /// <summary>
        /// Checks whether a given feat has already been unlocked.
        /// </summary>
        public bool IsFeatUnlocked(FeatType featType)
        {
            return feats.TryGetValue(featType, out var feat) && feat.is_unlocked;
        }

        /// <summary>
        /// Checks if all prerequisite feats for a given feat are unlocked.
        /// </summary>
        public bool AreFeatPrerequisitesMet(Feat feat)
        {
            if (feat == null || feat.prerequisite_feats == null) return true;
            return feat.prerequisite_feats.All(IsFeatUnlocked);
        }

        /// <summary>
        /// Returns all feats that are visible to the troll.
        /// </summary>
        public List<Feat> GetVisibleFeats(Troll troll, TechTreeBranch? branch = null)
        {
            IEnumerable<Feat> pool = branch.HasValue
                ? feats.Values.Where(f => f.branch == branch.Value)
                : feats.Values;

            return pool.Where(f => f.CanBeSeen(troll)).ToList();
        }

        /// <summary>
        /// Returns all feats that the troll is currently eligible to unlock.
        /// </summary>
        public List<Feat> GetUnlockableFeats(Troll troll, TechTreeBranch? branch = null)
        {
            IEnumerable<Feat> pool = branch.HasValue
                ? feats.Values.Where(f => f.branch == branch.Value)
                : feats.Values;

            return pool.Where(f => !f.is_unlocked && AreFeatPrerequisitesMet(f) && f.CanBeUnlocked(troll)).ToList();
        }

        /// <summary>
        /// Returns all feats that have already been unlocked.
        /// </summary>
        public List<Feat> GetUnlockedFeats(TechTreeBranch? branch = null)
        {
            IEnumerable<Feat> pool = branch.HasValue
                ? feats.Values.Where(f => f.branch == branch.Value)
                : feats.Values;

            return pool.Where(f => f.is_unlocked).ToList();
        }

        /// <summary>
        /// Attempts to unlock a feat for the troll, checking prerequisites, costs, and criteria.
        /// </summary>
        public bool UnlockFeat(FeatType featType, Troll troll)
        {
            if (!feats.TryGetValue(featType, out var feat))
            {
                return false;
            }

            if (feat.is_unlocked)
            {
                return false;
            }

            if (!AreFeatPrerequisitesMet(feat))
            {
                return false;
            }

            return feat.Unlock(troll);
        }
    }
}
