using System;
using BridgeTroll;
using Godot;

public partial class PlayerData : Node2D
{
    public string troll_name = "unknown";

    /// <summary>
    /// Total gold stored in the player's meta treasury across days.
    /// During the day the troll holds gold; at the end of the day it is transferred here.
    /// </summary>
    public int total_gold = 0;

    /// <summary> Backward-compatibility alias for total_gold. </summary>
    public int total_gol
    {
        get => total_gold;
        set => total_gold = value;
    }

    /// <summary>
    /// Reference to the persistent Troll mob.
    /// The Troll mob owns its own experience, level, and stats.
    /// </summary>
    private Troll _troll;
    public Troll troll
    {
        get => _troll;
        set
        {
            _troll = value;
            if (_troll != null)
            {
                _troll.player_data = this;
            }
        }
    }

    /// <summary>
    /// Forwards to the Troll mob's total experience.
    /// </summary>
    public int total_experience
    {
        get => troll != null ? troll.total_experience : 0;
        set
        {
            if (troll != null) troll.total_experience = value;
        }
    }

    /// <summary>
    /// Research tech tree storing unlocked research technologies.
    /// </summary>
    public ResearchTechTree research_tree = new();

    /// <summary>
    /// Available labor units of intelligence, strength, and charisma for research projects.
    /// </summary>
    public int labor_intelligence = 0;
    public int labor_strength = 0;
    public int labor_charisma = 0;

    /// <summary>
    /// Physical resources and items collected for research and building (e.g. books, wood, stone).
    /// </summary>
    public System.Collections.Generic.Dictionary<string, int> resources = new(StringComparer.OrdinalIgnoreCase);

    public int GetResource(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName)) return 0;
        resources.TryGetValue(resourceName, out int count);
        return count;
    }

    public void AddResource(string resourceName, int count = 1)
    {
        if (string.IsNullOrEmpty(resourceName) || count <= 0) return;
        if (resources.ContainsKey(resourceName))
            resources[resourceName] += count;
        else
            resources[resourceName] = count;
    }

    public bool HasResource(string resourceName, int count = 1)
    {
        return GetResource(resourceName) >= count;
    }

    public bool SpendResource(string resourceName, int count = 1)
    {
        if (!HasResource(resourceName, count)) return false;
        resources[resourceName] -= count;
        return true;
    }

    public void AddLabor(int intelligence = 0, int strength = 0, int charisma = 0)
    {
        labor_intelligence += intelligence;
        labor_strength += strength;
        labor_charisma += charisma;
    }

    /// <summary>
    /// Persistent event flags and milestones for the player's campaign.
    /// </summary>
    public EventFlags event_flags = new();

    /// <summary>
    /// Moves gold held by the troll mob during the day into the player's persistent treasury.
    /// </summary>
    public void CollectDayGold(Troll trollInstance = null)
    {
        Troll target = trollInstance ?? troll;
        if (target != null)
        {
            total_gold += target.gold;
            target.gold = 0;
        }
    }

    /// <summary> Adds gold directly to the player's total gold. </summary>
    public void DepositGold(int amount)
    {
        total_gold += amount;
    }

    /// <summary> Deducts gold if the player has enough, returning true if successful. </summary>
    public bool SpendGold(int amount)
    {
        if (total_gold >= amount)
        {
            total_gold -= amount;
            return true;
        }
        return false;
    }

    public override void _Ready() { }

    public override void _Process(double delta) { }
}
