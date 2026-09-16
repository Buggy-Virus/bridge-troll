using System;
using System.Collections.Generic;

namespace BridgeTroll
{
    /// <summary>
    /// Represents an individual technology node in the tech tree that can be discovered and unlocked.
    /// Supports numerical stat requirements as well as custom functional predicate requirements
    /// for being seen (visible in tech tree) and being unlocked (researched/learned).
    /// </summary>
    public class UnlockableTechnology
    {
        /// <summary>
        /// The specific technology identifier from the TechnologyType enum.
        /// </summary>
        public TechnologyType type { get; set; } = TechnologyType.NONE;

        /// <summary>
        /// Display name of the technology.
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the technology and what it enables.
        /// </summary>
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// The tech tree branch or part this technology belongs to.
        /// </summary>
        public TechTreeBranch branch { get; set; } = TechTreeBranch.INTELLECT;

        /// <summary>
        /// Numerical stat requirements that the troll must possess to unlock this technology.
        /// </summary>
        public TechnologyStatRequirements stat_requirements { get; set; } = new();

        /// <summary>
        /// List of other technologies that must be unlocked before this technology can be unlocked.
        /// </summary>
        public List<TechnologyType> prerequisite_technologies { get; set; } = new();

        /// <summary>
        /// State flag indicating whether the technology has already been unlocked by the troll.
        /// </summary>
        public bool is_unlocked { get; set; } = false;

        /// <summary>
        /// Custom condition function that determines whether this technology can be seen/discovered by the troll.
        /// Allows complex conditions beyond simple stats (e.g. world events, items found, night cycle triggers).
        /// If null, defaults to true.
        /// </summary>
        public Func<Troll, bool> can_see_requirement { get; set; }

        /// <summary>
        /// Custom condition function that determines whether this technology can be unlocked by the troll.
        /// Evaluated in addition to stat requirements and prerequisites.
        /// If null, defaults to true.
        /// </summary>
        public Func<Troll, bool> can_unlock_requirement { get; set; }

        /// <summary>
        /// Optional callback executed when the technology is successfully unlocked.
        /// </summary>
        public Action<Troll> on_unlocked { get; set; }

        public UnlockableTechnology() { }

        public UnlockableTechnology(
            TechnologyType type,
            string name,
            string description,
            TechTreeBranch branch,
            TechnologyStatRequirements statRequirements,
            Func<Troll, bool> canSeeRequirement = null,
            Func<Troll, bool> canUnlockRequirement = null,
            List<TechnologyType> prerequisiteTechnologies = null,
            Action<Troll> onUnlocked = null
        )
        {
            this.type = type;
            this.name = name;
            this.description = description;
            this.branch = branch;
            this.stat_requirements = statRequirements;
            this.can_see_requirement = canSeeRequirement;
            this.can_unlock_requirement = canUnlockRequirement;
            this.prerequisite_technologies = prerequisiteTechnologies ?? new List<TechnologyType>();
            this.on_unlocked = onUnlocked;
        }

        /// <summary>
        /// Determines whether this technology should be visible to the troll in the tech tree.
        /// </summary>
        public virtual bool CanBeSeen(Troll troll)
        {
            if (is_unlocked) return true;
            if (can_see_requirement != null)
            {
                return can_see_requirement(troll);
            }
            return true;
        }

        /// <summary>
        /// Evaluates whether the troll fulfills all requirements (stats, custom conditions, prerequisites)
        /// to unlock this technology.
        /// </summary>
        public virtual bool CanBeUnlocked(Troll troll)
        {
            if (is_unlocked) return false;

            // Check numerical stat requirements
            if (!stat_requirements.MeetsRequirements(troll))
            {
                return false;
            }

            // Check custom functional unlock requirement
            if (can_unlock_requirement != null && !can_unlock_requirement(troll))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unlocks the technology for the troll if all requirements are satisfied.
        /// Returns true if successfully unlocked, false otherwise.
        /// </summary>
        public virtual bool Unlock(Troll troll)
        {
            if (!CanBeUnlocked(troll))
            {
                return false;
            }

            is_unlocked = true;
            on_unlocked?.Invoke(troll);
            return true;
        }
    }
}
