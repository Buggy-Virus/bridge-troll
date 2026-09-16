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
    public Troll troll;

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
