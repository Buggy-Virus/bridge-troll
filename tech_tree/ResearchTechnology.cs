using System;
using System.Collections.Generic;

namespace BridgeTroll
{
    /// <summary>
    /// Represents an individual research project in the research tech tree.
    /// Research projects require specific amounts of labor (labor_intelligence, labor_strength,
    /// labor_charisma) and resources/items (such as books, stone, wood, etc.), in addition
    /// to minimum stat thresholds and functional unlock requirements.
    /// </summary>
    public class ResearchTechnology : UnlockableTechnology
    {
        /// <summary>
        /// Total labor required to complete this research.
        /// </summary>
        public ResearchLaborCost labor_cost { get; set; } = new();

        /// <summary>
        /// Resource and item costs required to complete this research (e.g. "book" -> 1).
        /// </summary>
        public Dictionary<string, int> resource_costs { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Accumulated intelligence labor invested into this research project.
        /// </summary>
        public int current_labor_intelligence { get; set; } = 0;

        /// <summary>
        /// Accumulated strength labor invested into this research project.
        /// </summary>
        public int current_labor_strength { get; set; } = 0;

        /// <summary>
        /// Accumulated charisma labor invested into this research project.
        /// </summary>
        public int current_labor_charisma { get; set; } = 0;

        /// <summary>
        /// Accumulated items/resources already invested into this project.
        /// </summary>
        public Dictionary<string, int> current_resources { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public int RemainingLaborIntelligence => Math.Max(0, labor_cost.labor_intelligence - current_labor_intelligence);
        public int RemainingLaborStrength => Math.Max(0, labor_cost.labor_strength - current_labor_strength);
        public int RemainingLaborCharisma => Math.Max(0, labor_cost.labor_charisma - current_labor_charisma);

        public bool IsLaborComplete =>
            RemainingLaborIntelligence <= 0 &&
            RemainingLaborStrength <= 0 &&
            RemainingLaborCharisma <= 0;

        public bool AreResourcesComplete
        {
            get
            {
                foreach (var kvp in resource_costs)
                {
                    current_resources.TryGetValue(kvp.Key, out int invested);
                    if (invested < kvp.Value) return false;
                }
                return true;
            }
        }

        public ResearchTechnology() { }

        public ResearchTechnology(
            TechnologyType type,
            string name,
            string description,
            TechTreeBranch branch,
            ResearchLaborCost laborCost,
            Dictionary<string, int> resourceCosts = null,
            TechnologyStatRequirements statRequirements = default,
            Func<Troll, bool> canSeeRequirement = null,
            Func<Troll, bool> canUnlockRequirement = null,
            List<TechnologyType> prerequisiteTechnologies = null,
            Action<Troll> onUnlocked = null
        ) : base(
            type: type,
            name: name,
            description: description,
            branch: branch,
            statRequirements: statRequirements,
            canSeeRequirement: canSeeRequirement,
            canUnlockRequirement: canUnlockRequirement,
            prerequisiteTechnologies: prerequisiteTechnologies,
            onUnlocked: onUnlocked
        )
        {
            this.labor_cost = laborCost;
            if (resourceCosts != null)
            {
                foreach (var kvp in resourceCosts)
                {
                    this.resource_costs[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Contributes labor units incrementally towards completing this research project.
        /// </summary>
        public void ContributeLabor(int intelligence = 0, int strength = 0, int charisma = 0)
        {
            current_labor_intelligence += intelligence;
            current_labor_strength += strength;
            current_labor_charisma += charisma;
        }

        /// <summary>
        /// Contributes resource or item units incrementally towards completing this research project.
        /// </summary>
        public void ContributeResource(string resourceName, int amount = 1)
        {
            if (string.IsNullOrEmpty(resourceName) || amount <= 0) return;
            if (current_resources.ContainsKey(resourceName))
                current_resources[resourceName] += amount;
            else
                current_resources[resourceName] = amount;
        }

        /// <summary>
        /// Checks whether the troll (and associated player data) has sufficient available labor and resources.
        /// </summary>
        public bool CanAfford(Troll troll)
        {
            if (troll == null) return false;

            int availInt = troll.labor_intelligence + (troll.player_data != null ? troll.player_data.labor_intelligence : 0);
            int availStr = troll.labor_strength + (troll.player_data != null ? troll.player_data.labor_strength : 0);
            int availChar = troll.labor_charisma + (troll.player_data != null ? troll.player_data.labor_charisma : 0);

            if (availInt < RemainingLaborIntelligence) return false;
            if (availStr < RemainingLaborStrength) return false;
            if (availChar < RemainingLaborCharisma) return false;

            foreach (var kvp in resource_costs)
            {
                current_resources.TryGetValue(kvp.Key, out int invested);
                int needed = Math.Max(0, kvp.Value - invested);
                if (needed <= 0) continue;

                int availRes = troll.GetResource(kvp.Key) + (troll.player_data != null ? troll.player_data.GetResource(kvp.Key) : 0);
                if (availRes < needed) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the player data has sufficient available labor and resources.
        /// </summary>
        public bool CanAfford(PlayerData playerData)
        {
            if (playerData == null) return false;
            if (playerData.troll != null) return CanAfford(playerData.troll);

            if (playerData.labor_intelligence < RemainingLaborIntelligence) return false;
            if (playerData.labor_strength < RemainingLaborStrength) return false;
            if (playerData.labor_charisma < RemainingLaborCharisma) return false;

            foreach (var kvp in resource_costs)
            {
                current_resources.TryGetValue(kvp.Key, out int invested);
                int needed = Math.Max(0, kvp.Value - invested);
                if (needed <= 0) continue;

                if (playerData.GetResource(kvp.Key) < needed) return false;
            }

            return true;
        }

        /// <summary>
        /// Deducts remaining labor and resource requirements from the troll and/or player data.
        /// </summary>
        public bool Deduct(Troll troll)
        {
            if (!CanAfford(troll)) return false;

            // Deduct labor
            int needInt = RemainingLaborIntelligence;
            int takeIntFromTroll = Math.Min(troll.labor_intelligence, needInt);
            troll.labor_intelligence -= takeIntFromTroll;
            needInt -= takeIntFromTroll;
            if (needInt > 0 && troll.player_data != null)
            {
                troll.player_data.labor_intelligence -= needInt;
            }
            current_labor_intelligence = labor_cost.labor_intelligence;

            int needStr = RemainingLaborStrength;
            int takeStrFromTroll = Math.Min(troll.labor_strength, needStr);
            troll.labor_strength -= takeStrFromTroll;
            needStr -= takeStrFromTroll;
            if (needStr > 0 && troll.player_data != null)
            {
                troll.player_data.labor_strength -= needStr;
            }
            current_labor_strength = labor_cost.labor_strength;

            int needChar = RemainingLaborCharisma;
            int takeCharFromTroll = Math.Min(troll.labor_charisma, needChar);
            troll.labor_charisma -= takeCharFromTroll;
            needChar -= takeCharFromTroll;
            if (needChar > 0 && troll.player_data != null)
            {
                troll.player_data.labor_charisma -= needChar;
            }
            current_labor_charisma = labor_cost.labor_charisma;

            // Deduct resources
            foreach (var kvp in resource_costs)
            {
                current_resources.TryGetValue(kvp.Key, out int invested);
                int needed = Math.Max(0, kvp.Value - invested);
                if (needed <= 0) continue;

                int fromTroll = Math.Min(troll.GetResource(kvp.Key), needed);
                troll.SpendResource(kvp.Key, fromTroll);
                needed -= fromTroll;
                if (needed > 0 && troll.player_data != null)
                {
                    troll.player_data.SpendResource(kvp.Key, needed);
                }
                current_resources[kvp.Key] = kvp.Value;
            }

            return true;
        }

        /// <summary>
        /// Deducts remaining labor and resource requirements from the player data.
        /// </summary>
        public bool Deduct(PlayerData playerData)
        {
            if (playerData == null) return false;
            if (playerData.troll != null) return Deduct(playerData.troll);

            if (!CanAfford(playerData)) return false;

            playerData.labor_intelligence -= RemainingLaborIntelligence;
            playerData.labor_strength -= RemainingLaborStrength;
            playerData.labor_charisma -= RemainingLaborCharisma;

            current_labor_intelligence = labor_cost.labor_intelligence;
            current_labor_strength = labor_cost.labor_strength;
            current_labor_charisma = labor_cost.labor_charisma;

            foreach (var kvp in resource_costs)
            {
                current_resources.TryGetValue(kvp.Key, out int invested);
                int needed = Math.Max(0, kvp.Value - invested);
                if (needed > 0)
                {
                    playerData.SpendResource(kvp.Key, needed);
                    current_resources[kvp.Key] = kvp.Value;
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluates whether the troll satisfies labor/resource costs, stat requirements, and custom predicates.
        /// </summary>
        public override bool CanBeUnlocked(Troll troll)
        {
            if (is_unlocked) return false;

            if (!IsLaborComplete || !AreResourcesComplete)
            {
                if (!CanAfford(troll))
                {
                    return false;
                }
            }

            return base.CanBeUnlocked(troll);
        }

        /// <summary>
        /// Unlocks the research technology, deducting any outstanding labor and resources.
        /// </summary>
        public override bool Unlock(Troll troll)
        {
            if (!CanBeUnlocked(troll))
            {
                return false;
            }

            if (!IsLaborComplete || !AreResourcesComplete)
            {
                if (!Deduct(troll))
                {
                    return false;
                }
            }

            is_unlocked = true;
            on_unlocked?.Invoke(troll);
            return true;
        }

        /// <summary>
        /// Unlocks the research technology using player data.
        /// </summary>
        public bool Unlock(PlayerData playerData)
        {
            if (playerData?.troll != null)
            {
                return Unlock(playerData.troll);
            }

            if (is_unlocked) return false;
            if (!CanAfford(playerData)) return false;
            if (!Deduct(playerData)) return false;

            is_unlocked = true;
            return true;
        }
    }
}
