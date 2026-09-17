using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeTroll
{
    /// <summary>
    /// Manages the technology progression system. Can represent either a unified tech tree
    /// with three main branches (Might, Intellect, Influence) or separate modular trees.
    /// </summary>
    public class TechTree
    {
        public string tree_name { get; set; } = "Main Tech Tree";

        /// <summary>
        /// All registered technologies in this tech tree indexed by TechnologyType.
        /// </summary>
        public Dictionary<TechnologyType, UnlockableTechnology> technologies { get; } = new();

        /// <summary>
        /// Technologies organized by their respective tech tree branch.
        /// </summary>
        public Dictionary<TechTreeBranch, List<UnlockableTechnology>> branch_technologies { get; } = new()
        {
            { TechTreeBranch.MIGHT, new List<UnlockableTechnology>() },
            { TechTreeBranch.INTELLECT, new List<UnlockableTechnology>() },
            { TechTreeBranch.INFLUENCE, new List<UnlockableTechnology>() }
        };

        public TechTree(string name = "Main Tech Tree", bool initializeDefaults = true)
        {
            tree_name = name;
            if (initializeDefaults)
            {
                InitializeDefaultTechnologies();
            }
        }

        /// <summary>
        /// Registers an unlockable technology into the tech tree.
        /// </summary>
        public virtual void RegisterTechnology(UnlockableTechnology technology)
        {
            if (technology == null || technology.type == TechnologyType.NONE) return;

            technologies[technology.type] = technology;

            if (!branch_technologies.ContainsKey(technology.branch))
            {
                branch_technologies[technology.branch] = new List<UnlockableTechnology>();
            }

            if (!branch_technologies[technology.branch].Contains(technology))
            {
                branch_technologies[technology.branch].Add(technology);
            }
        }

        /// <summary>
        /// Initializes the starting technology lineup, beginning with Reading in the Intellect branch.
        /// </summary>
        public virtual void InitializeDefaultTechnologies()
        {
            // First Technology: Reading
            var readingTech = new UnlockableTechnology(
                type: TechnologyType.READING,
                name: "Reading",
                description: "Allows the troll to decipher human books, road signs, scrolls, and accounting ledgers.",
                branch: TechTreeBranch.INTELLECT,
                statRequirements: new TechnologyStatRequirements(intelligence: 10),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) =>
                {
                    // Custom condition: requires base intelligence of at least 10
                    return troll != null && troll.base_intelligence >= 10;
                },
                onUnlocked: (troll) =>
                {
                    // When reading is learned, slightly boosts intelligence
                    troll.base_intelligence += 1;
                }
            );
            RegisterTechnology(readingTech);

            // Additional foundational branch starters for Might and Influence
            var intimidationTech = new UnlockableTechnology(
                type: TechnologyType.INTIMIDATION,
                name: "Intimidation",
                description: "The troll learns to roar fiercely and strike dread into passing travellers.",
                branch: TechTreeBranch.MIGHT,
                statRequirements: new TechnologyStatRequirements(strength: 10),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) => troll != null && troll.base_strength >= 10,
                onUnlocked: (troll) =>
                {
                    troll.stats.base_scary += 2;
                }
            );
            RegisterTechnology(intimidationTech);

            var accountingTech = new UnlockableTechnology(
                type: TechnologyType.ACCOUNTING,
                name: "Accounting",
                description: "Better calculate tolls and recognize the true worth of coins and goods.",
                branch: TechTreeBranch.INFLUENCE,
                statRequirements: new TechnologyStatRequirements(intelligence: 10, charisma: 10),
                canSeeRequirement: (troll) => IsTechnologyUnlocked(TechnologyType.READING),
                canUnlockRequirement: (troll) =>
                {
                    return IsTechnologyUnlocked(TechnologyType.READING)
                        && troll != null
                        && troll.base_charisma >= 10;
                },
                prerequisiteTechnologies: new List<TechnologyType> { TechnologyType.READING }
            );
            RegisterTechnology(accountingTech);
        }

        /// <summary>
        /// Returns all technologies that meet the visibility requirement for the troll.
        /// Optionally filter by branch.
        /// </summary>
        public List<UnlockableTechnology> GetVisibleTechnologies(Troll troll, TechTreeBranch? branch = null)
        {
            IEnumerable<UnlockableTechnology> pool = branch.HasValue && branch_technologies.ContainsKey(branch.Value)
                ? branch_technologies[branch.Value]
                : technologies.Values;

            return pool.Where(t => t.CanBeSeen(troll)).ToList();
        }

        /// <summary>
        /// Returns all technologies that the troll currently meets all criteria to unlock.
        /// Optionally filter by branch.
        /// </summary>
        public List<UnlockableTechnology> GetUnlockableTechnologies(Troll troll, TechTreeBranch? branch = null)
        {
            IEnumerable<UnlockableTechnology> pool = branch.HasValue && branch_technologies.ContainsKey(branch.Value)
                ? branch_technologies[branch.Value]
                : technologies.Values;

            return pool.Where(t => !t.is_unlocked && ArePrerequisitesMet(t) && t.CanBeUnlocked(troll)).ToList();
        }

        /// <summary>
        /// Returns all technologies that have already been unlocked.
        /// </summary>
        public List<UnlockableTechnology> GetUnlockedTechnologies(TechTreeBranch? branch = null)
        {
            IEnumerable<UnlockableTechnology> pool = branch.HasValue && branch_technologies.ContainsKey(branch.Value)
                ? branch_technologies[branch.Value]
                : technologies.Values;

            return pool.Where(t => t.is_unlocked).ToList();
        }

        /// <summary>
        /// Checks whether a given technology has been unlocked.
        /// </summary>
        public bool IsTechnologyUnlocked(TechnologyType type)
        {
            return technologies.TryGetValue(type, out var tech) && tech.is_unlocked;
        }

        /// <summary>
        /// Checks if all prerequisite technologies for a given technology are already unlocked.
        /// </summary>
        public virtual bool ArePrerequisitesMet(UnlockableTechnology technology)
        {
            if (technology == null || technology.prerequisite_technologies == null) return true;
            return technology.prerequisite_technologies.All(IsTechnologyUnlocked);
        }

        /// <summary>
        /// Attempts to unlock a specific technology for the troll during night or progression cycles.
        /// </summary>
        public virtual bool UnlockTechnology(TechnologyType type, Troll troll)
        {
            if (!technologies.TryGetValue(type, out var tech))
            {
                return false;
            }

            if (tech.is_unlocked)
            {
                return false;
            }

            if (!ArePrerequisitesMet(tech))
            {
                return false;
            }

            return tech.Unlock(troll);
        }

        /// <summary>
        /// Retrieves a specific technology node by type.
        /// </summary>
        public virtual UnlockableTechnology GetTechnology(TechnologyType type)
        {
            technologies.TryGetValue(type, out var tech);
            return tech;
        }
    }
}
