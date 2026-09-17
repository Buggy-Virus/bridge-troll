using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeTroll
{
    /// <summary>
    /// Research tech tree child class of TechTree.
    /// Manages research technologies that require labor (labor_intelligence, labor_strength,
    /// labor_charisma) and physical resources/items (e.g. books, stone, wood, gold) in addition
    /// to stat requirements and unlock criteria.
    /// </summary>
    public class ResearchTechTree : TechTree
    {
        /// <summary>
        /// Research projects registered in this tree indexed by TechnologyType.
        /// </summary>
        public Dictionary<TechnologyType, ResearchTechnology> research_technologies { get; } = new();

        public ResearchTechTree(string name = "Research Tech Tree", bool initializeDefaults = true)
            : base(name, false)
        {
            if (initializeDefaults)
            {
                InitializeDefaultResearchTechnologies();
            }
        }

        /// <summary>
        /// Registers a research project into the research tech tree and the underlying tech tree.
        /// </summary>
        public void RegisterResearchTechnology(ResearchTechnology researchTech)
        {
            if (researchTech == null || researchTech.type == TechnologyType.NONE) return;

            research_technologies[researchTech.type] = researchTech;
            RegisterTechnology(researchTech);
        }

        /// <summary>
        /// Initializes default research projects across the Intellect, Might, and Influence branches.
        /// </summary>
        public virtual void InitializeDefaultResearchTechnologies()
        {
            // Reading: Intellect threshold of 4, requires 10 units of labor_intelligence
            var readingResearch = new ResearchTechnology(
                type: TechnologyType.READING,
                name: "Reading",
                description: "Allows the troll to decipher human books, road signs, scrolls, and accounting ledgers.",
                branch: TechTreeBranch.INTELLECT,
                laborCost: new ResearchLaborCost(intelligence: 10),
                statRequirements: new TechnologyStatRequirements(intelligence: 4),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) => troll != null && troll.base_intelligence >= 4,
                onUnlocked: (troll) =>
                {
                    troll.base_intelligence += 1;
                }
            );
            RegisterResearchTechnology(readingResearch);

            // Writing: Requires Reading, 15 labor_intelligence, 1 book item, and int 6
            var writingResearch = new ResearchTechnology(
                type: TechnologyType.WRITING,
                name: "Writing",
                description: "Enables scribing treaties, road signage, and official bridge toll vouchers. Requires 1 book item and 15 labor_intelligence.",
                branch: TechTreeBranch.INTELLECT,
                laborCost: new ResearchLaborCost(intelligence: 15),
                resourceCosts: new Dictionary<string, int> { { "book", 1 } },
                statRequirements: new TechnologyStatRequirements(intelligence: 6),
                canSeeRequirement: (troll) => IsTechnologyUnlocked(TechnologyType.READING),
                canUnlockRequirement: (troll) => IsTechnologyUnlocked(TechnologyType.READING) && troll.base_intelligence >= 6,
                prerequisiteTechnologies: new List<TechnologyType> { TechnologyType.READING }
            );
            RegisterResearchTechnology(writingResearch);

            // Accounting: Requires Reading, 20 labor_intelligence, 5 labor_charisma, 2 book items, and int 8
            var accountingResearch = new ResearchTechnology(
                type: TechnologyType.ACCOUNTING,
                name: "Accounting",
                description: "Advanced ledger-keeping for tracking caravan tolls and treasury investments. Requires 2 book items, 20 labor_intelligence, and 5 labor_charisma.",
                branch: TechTreeBranch.INTELLECT,
                laborCost: new ResearchLaborCost(intelligence: 20, charisma: 5),
                resourceCosts: new Dictionary<string, int> { { "book", 2 } },
                statRequirements: new TechnologyStatRequirements(intelligence: 8, charisma: 6),
                canSeeRequirement: (troll) => IsTechnologyUnlocked(TechnologyType.READING),
                canUnlockRequirement: (troll) => IsTechnologyUnlocked(TechnologyType.READING) && troll.base_intelligence >= 8,
                prerequisiteTechnologies: new List<TechnologyType> { TechnologyType.READING }
            );
            RegisterResearchTechnology(accountingResearch);

            // Basic Toolmaking: Might branch research requiring labor_strength, stone, and wood
            var toolmakingResearch = new ResearchTechnology(
                type: TechnologyType.BASIC_TOOLMAKING,
                name: "Basic Toolmaking",
                description: "Craft rudimentary stone and wood tools to fortify the bridge. Requires 20 labor_strength, 5 stone, and 5 wood.",
                branch: TechTreeBranch.MIGHT,
                laborCost: new ResearchLaborCost(strength: 20),
                resourceCosts: new Dictionary<string, int> { { "stone", 5 }, { "wood", 5 } },
                statRequirements: new TechnologyStatRequirements(strength: 10),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) => troll != null && troll.base_strength >= 10
            );
            RegisterResearchTechnology(toolmakingResearch);

            // Intimidation: Influence branch research requiring labor_charisma
            var intimidationResearch = new ResearchTechnology(
                type: TechnologyType.INTIMIDATION,
                name: "Intimidation",
                description: "Study traveller psychology to project an overwhelming presence. Requires 15 labor_charisma.",
                branch: TechTreeBranch.INFLUENCE,
                laborCost: new ResearchLaborCost(charisma: 15),
                statRequirements: new TechnologyStatRequirements(charisma: 8),
                canSeeRequirement: (troll) => true,
                canUnlockRequirement: (troll) => troll != null && troll.base_charisma >= 8
            );
            RegisterResearchTechnology(intimidationResearch);
        }

        /// <summary>
        /// Retrieves a research technology by TechnologyType.
        /// </summary>
        public ResearchTechnology GetResearchTechnology(TechnologyType type)
        {
            research_technologies.TryGetValue(type, out var tech);
            return tech;
        }

        /// <summary>
        /// Returns all research technologies visible to the troll.
        /// </summary>
        public List<ResearchTechnology> GetVisibleResearch(Troll troll, TechTreeBranch? branch = null)
        {
            IEnumerable<ResearchTechnology> pool = branch.HasValue
                ? research_technologies.Values.Where(r => r.branch == branch.Value)
                : research_technologies.Values;

            return pool.Where(r => r.CanBeSeen(troll)).ToList();
        }

        /// <summary>
        /// Returns all research technologies that the troll is currently eligible to unlock.
        /// </summary>
        public List<ResearchTechnology> GetUnlockableResearch(Troll troll, TechTreeBranch? branch = null)
        {
            IEnumerable<ResearchTechnology> pool = branch.HasValue
                ? research_technologies.Values.Where(r => r.branch == branch.Value)
                : research_technologies.Values;

            return pool.Where(r => !r.is_unlocked && ArePrerequisitesMet(r) && r.CanBeUnlocked(troll)).ToList();
        }

        /// <summary>
        /// Attempts to unlock a research technology using player data directly.
        /// </summary>
        public bool UnlockTechnology(TechnologyType type, PlayerData playerData)
        {
            if (playerData?.troll != null)
            {
                return UnlockTechnology(type, playerData.troll);
            }

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

            if (tech is ResearchTechnology researchTech)
            {
                return researchTech.Unlock(playerData);
            }

            return false;
        }
    }
}
