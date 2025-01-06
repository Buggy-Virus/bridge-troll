using System;
using System.Collections.Generic;

namespace BridgeTroll
{
    public enum CommodityType {
        FOOD_GRAINS,
        FOOD_VEGETABLES,
        FOOD_FRUITS,
        FOOD_PROTEINS,
        FOOD_LUXURY,
        MATERIALS_WOOD,
        MATERIALS_STONE,
        MATERIALS_TEXTILES,
        METALS_MUNDANE,
        METALS_SPECIAL,
        MAGIC_SIMPLE,
        MAGIC_ADVANCED,
    }

    public class Commodity {
        public CommodityType type;
        public float scarcity;
        public float resource_capacity;
        public float productivity;
        public float internal_value;
        public float interal_price;
        public float market_price;
        public float outside_price;
        public float elasticity;
        public float tariff;
        public float labor_allocation;
    }

    public struct LaborTransfer {
        public float time_left;
        public float cost;
        public float labor_allocation_size;
        public CommodityType commodity_exiting;
        public CommodityType commodity_entering;
    }

    public enum BiasType {
        WEALTH,
        EDUCATION,
        MILITARY,
        TRUST,
        LAW,
    }

    public struct Law {

    }

    public enum GovernmentType {
        NORMAL,
        NATIONALIST,
        TOTALITARIAN,
        PROGRESSIVE,
        RELIGIOUS,
        WARMONGER,
        PLUTOCRACY,
    }

    public class KingdomModel
    {
        // Attitudes Towards Troll
        public AttitudeTowardsTroll troll_attitude_peasant;
        public AttitudeTowardsTroll troll_attitude_commoner;
        public AttitudeTowardsTroll troll_attitude_aristocrat;

        // Economic stuff
        public float wealth_general;
        public float wealth_peasant;
        public float wealth_commoner;
        public float wealth_aristocrat;
        public float population;
        public float productivity_base;
        public float nutrition;
        public Dictionary<CommodityType, Commodity> commodities;
        public List<LaborTransfer> labor_transfers;

        // Attitude Towards Other Kingdom
        public float percieved_dependence;
        public float diplomatic_relations;
        public float interpersonal_relations;
        public float hostility;
        public float visitors;
        public float immigrants;
        public Dictionary<BiasType, float> biases;

        // Political State
        public GovernmentType government_type;
        public string current_leader;
        public float discontent;
        public List<Law> laws;

        // Military
        public float military_strength;
        public float mercenary_strength;
        public float mercenary_availability;
        public float adventurer_strength;
        public float adventurer_availability;

        // Mechanics
        public Dictionary<Mob.Type, int> daily_mob_spawns_expected;
        public Dictionary<Mob.Type, int> daily_mob_spawans_max;

        // Takes the events of the day as an argument
        public void UpdateKingdom() {

        }
    }
}