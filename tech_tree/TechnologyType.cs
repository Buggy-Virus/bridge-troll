using System;

namespace BridgeTroll
{
    /// <summary>
    /// Identifiers for all unlockable technologies in the game.
    /// </summary>
    public enum TechnologyType
    {
        NONE = 0,
        READING = 1,
        WRITING = 2,
        ACCOUNTING = 3,
        INTIMIDATION = 4,
        BASIC_TOOLMAKING = 5,
        MASONRY = 6,
        ALCHEMY = 7,
        BRIDGE_REINFORCEMENT = 8
    }

    /// <summary>
    /// Represents the three main parts / branches of the tech tree.
    /// </summary>
    public enum TechTreeBranch
    {
        MIGHT = 0,       // Physical strength, combat prowess, intimidation
        INTELLECT = 1,   // Knowledge, literacy (reading), crafting, alchemy
        INFLUENCE = 2    // Negotiation, tolls, trade, kingdom relations
    }
}
