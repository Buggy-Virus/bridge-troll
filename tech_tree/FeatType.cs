using System;

namespace BridgeTroll
{
    /// <summary>
    /// Identifiers for feats that drastically alter gameplay mechanics.
    /// </summary>
    public enum FeatType
    {
        NONE = 0,

        // Might / Strength Feats
        GROUND_SLAM = 1,          // Stuns and damages all mobs on the bridge section
        TITAN_GRIP = 2,           // Grappled victims cannot break free or squirm
        UNSTOPPABLE_CHARGE = 3,   // Charge through mobs knocking them aside
        INTIMIDATING_ROAR = 4,    // Massive area scare causing instant panic

        // Intellect Feats
        EAGLE_EYE_TOLL = 10,      // Previews the exact gold and loot mobs carry before grappling
        RUNIC_BRIDGE = 11,        // Imbues the bridge with protective runes
        ARCANE_RESISTANCE = 12,   // Resistance against magical attacks from mages and priests
        ALCHEMICAL_HARVEST = 13,  // Harvesting defeated mobs yields rare alchemical components

        // Influence / Charisma Feats
        SILVER_TONGUE = 20,       // Extortion yields double gold without triggering immediate combat
        TERRIFYING_PRESENCE = 21, // Weak mobs surrender automatically upon entering the bridge
        ROYAL_CHARTER = 22,       // Legitimizes bridge toll collection, reducing king hostility
        EXTORTION_EMPIRE = 23     // Can extort multiple queued mobs simultaneously
    }
}
