using System;

namespace BridgeTroll
{
    /// <summary>
    /// Represents the underlying numerical characteristics (stats) of a character
    /// and provides logic and equations for calculating dependent values.
    /// </summary>
    public class CharacterStats
    {
        // ===================================================================
        // Underlying Stat Variables
        // ===================================================================

        /// <summary> Base physical power and strength of the character. </summary>
        public int base_strength = 10;

        /// <summary> Base mental capacity and intelligence of the character. </summary>
        public int base_intelligence = 10;

        /// <summary> Base social appeal and persuasiveness of the character. </summary>
        public int base_charisma = 10;

        /// <summary> Base movement speed stat. </summary>
        public float base_speed = 100.0f;

        /// <summary> Base maximum hit points (vitality) stat. </summary>
        public int base_max_hit_points = 10;

        /// <summary> Alias for base_max_hit_points representing base hit points. </summary>
        public int base_hit_points
        {
            get => base_max_hit_points;
            set => base_max_hit_points = value;
        }

        /// <summary> Base surrender hit points threshold stat. </summary>
        public int base_surrender_hit_points = 3;

        /// <summary> Base attack damage stat. </summary>
        public int base_damage = 1;

        /// <summary> Base courage stat; affects bravery and surrender threshold. </summary>
        public int base_courage = 1;

        /// <summary> Base scariness rating stat; affects intimidation. </summary>
        public int base_scary = 0;

        // ===================================================================
        // Constructors
        // ===================================================================

        public CharacterStats() { }

        public CharacterStats(
            int baseStrength,
            int baseIntelligence,
            int baseCharisma,
            float baseSpeed,
            int baseMaxHitPoints,
            int baseSurrenderHitPoints,
            int baseDamage,
            int baseCourage,
            int baseScary
        )
        {
            this.base_strength = baseStrength;
            this.base_intelligence = baseIntelligence;
            this.base_charisma = baseCharisma;
            this.base_speed = baseSpeed;
            this.base_max_hit_points = baseMaxHitPoints;
            this.base_surrender_hit_points = baseSurrenderHitPoints;
            this.base_damage = baseDamage;
            this.base_courage = baseCourage;
            this.base_scary = baseScary;
        }

        // ===================================================================
        // Static Calculation Functions (Equations for dependent values)
        // ===================================================================

        /// <summary> Calculates limp speed as 0.5 * base speed. </summary>
        public static float CalculateLimpSpeed(float baseSpeed) => 0.5f * baseSpeed;
        public static float calculate_limp_speed(float baseSpeed) => CalculateLimpSpeed(baseSpeed);

        /// <summary> Calculates walk speed as 1.0 * base speed. </summary>
        public static float CalculateWalkSpeed(float baseSpeed) => 1.0f * baseSpeed;
        public static float calculate_walk_speed(float baseSpeed) => CalculateWalkSpeed(baseSpeed);

        /// <summary> Calculates run speed as 2.0 * base speed. </summary>
        public static float CalculateRunSpeed(float baseSpeed) => 2.0f * baseSpeed;
        public static float calculate_run_speed(float baseSpeed) => CalculateRunSpeed(baseSpeed);

        /// <summary> Calculates max hit points scaled by base strength. </summary>
        public static int CalculateMaxHitPoints(int baseHp, int baseStrength) =>
            baseHp + Math.Max(0, baseStrength - 10);
        public static int calculate_max_hit_points(int baseHp, int baseStrength) =>
            CalculateMaxHitPoints(baseHp, baseStrength);

        /// <summary> Calculates effective surrender hit points adjusted by base courage. </summary>
        public static int CalculateSurrenderHitPoints(int baseSurrenderHp, int baseMaxHp, int baseCourage) =>
            Math.Clamp(baseSurrenderHp - (baseCourage - 1), 0, baseMaxHp);
        public static int calculate_surrender_hit_points(int baseSurrenderHp, int baseMaxHp, int baseCourage) =>
            CalculateSurrenderHitPoints(baseSurrenderHp, baseMaxHp, baseCourage);

        /// <summary> Calculates effective damage (currently equal to base strength). </summary>
        public static int CalculateDamage(int baseStrength) => baseStrength;
        public static int CalculateDamage(int baseDamage, int baseStrength) => baseStrength;
        public static int calculate_damage(int baseStrength) => CalculateDamage(baseStrength);
        public static int calculate_damage(int baseDamage, int baseStrength) => CalculateDamage(baseStrength);

        /// <summary> Calculates effective scariness considering base strength and base scariness. </summary>
        public static int CalculateScary(int baseScary, int baseStrength) =>
            baseScary + (baseStrength > 15 ? (baseStrength - 15) / 2 : 0);
        public static int calculate_scary(int baseScary, int baseStrength) =>
            CalculateScary(baseScary, baseStrength);

        /// <summary> Calculates effective courage based on base courage and base strength. </summary>
        public static int CalculateEffectiveCourage(int baseCourage, int baseStrength) =>
            baseCourage + (baseStrength / 10);
        public static int calculate_effective_courage(int baseCourage, int baseStrength) =>
            CalculateEffectiveCourage(baseCourage, baseStrength);

        // ===================================================================
        // Instance Calculation Functions
        // ===================================================================

        /// <summary> Calculates limp speed (0.5 * base speed) for this stats instance. </summary>
        public float CalculateLimpSpeed() => CalculateLimpSpeed(base_speed);
        public float calculate_limp_speed() => CalculateLimpSpeed();

        /// <summary> Calculates walk speed (1.0 * base speed) for this stats instance. </summary>
        public float CalculateWalkSpeed() => CalculateWalkSpeed(base_speed);
        public float calculate_walk_speed() => CalculateWalkSpeed();

        /// <summary> Calculates run speed (2.0 * base speed) for this stats instance. </summary>
        public float CalculateRunSpeed() => CalculateRunSpeed(base_speed);
        public float calculate_run_speed() => CalculateRunSpeed();

        /// <summary> Calculates max hit points for this stats instance. </summary>
        public int CalculateMaxHitPoints() => CalculateMaxHitPoints(base_max_hit_points, base_strength);
        public int calculate_max_hit_points() => CalculateMaxHitPoints();

        /// <summary> Calculates effective surrender hit points for this stats instance. </summary>
        public int CalculateSurrenderHitPoints() =>
            CalculateSurrenderHitPoints(base_surrender_hit_points, base_max_hit_points, base_courage);
        public int calculate_surrender_hit_points() => CalculateSurrenderHitPoints();

        /// <summary> Calculates effective damage for this stats instance. </summary>
        public int CalculateDamage() => CalculateDamage(base_strength);
        public int calculate_damage() => CalculateDamage();

        /// <summary> Calculates effective scariness for this stats instance. </summary>
        public int CalculateScary() => CalculateScary(base_scary, base_strength);
        public int calculate_scary() => CalculateScary();

        /// <summary> Calculates effective courage for this stats instance. </summary>
        public int CalculateEffectiveCourage() => CalculateEffectiveCourage(base_courage, base_strength);
        public int calculate_effective_courage() => CalculateEffectiveCourage();
    }
}
