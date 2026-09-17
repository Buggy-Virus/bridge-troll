using System.Collections.Generic;
using System.Linq;

namespace BridgeTroll
{
    public enum ItemType
    {
        NONE = 0,

        // Non-unique goods (fungible, stackable)
        GOLD = 1,
        WOOD = 2,

        // Unique goods (non-fungible, distinct state)
        LOINCLOTH = 100,
    }

    public static class ItemTypeRegistry
    {
        public static readonly IReadOnlyList<ItemType> NonUniqueItemTypes = new[]
        {
            ItemType.GOLD,
            ItemType.WOOD,
        };

        public static readonly IReadOnlyList<ItemType> UniqueItemTypes = new[]
        {
            ItemType.LOINCLOTH,
        };

        public static bool IsNonUnique(ItemType type) => NonUniqueItemTypes.Contains(type);

        public static bool IsUnique(ItemType type) => UniqueItemTypes.Contains(type);
    }
}
