using System;
using Godot;

namespace BridgeTroll
{
    public partial class InventoryTestRunner : Node
    {
        public static bool RunTests()
        {
            GD.Print("--- Starting Inventory System Tests ---");
            bool allPassed = true;

            try
            {
                allPassed &= TestSlotInitialization();
                allPassed &= TestNonUniqueGoodStacking();
                allPassed &= TestUniqueGoodDistinctSlots();
                allPassed &= TestNonUniqueTransfer();
                allPassed &= TestUniqueTransferAndPersistence();
                allPassed &= TestCharacterGoldIntegration();
                allPassed &= TestWoodResourceAndPlayerStash();
                allPassed &= TestLaborCostAndConstructionTransition();
                allPassed &= TestBuildingDataAndFootprintCells();
                allPassed &= TestBuildingInspectionData();
                allPassed &= TestBuildingUnlockSystem();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception during inventory tests: {ex}");
                return false;
            }

            if (allPassed)
            {
                GD.Print("--- ALL INVENTORY TESTS PASSED ---");
            }
            else
            {
                GD.PrintErr("--- SOME INVENTORY TESTS FAILED ---");
            }

            return allPassed;
        }

        private static bool TestSlotInitialization()
        {
            Inventory inv = new Inventory(4);
            if (inv.SlotCount != 4)
            {
                GD.PrintErr("FAIL: SlotCount should be 4");
                return false;
            }
            if (inv.GetFirstEmptySlotIndex() != 0)
            {
                GD.PrintErr("FAIL: First empty slot should be 0");
                return false;
            }
            GD.Print("PASS: TestSlotInitialization");
            return true;
        }

        private static bool TestNonUniqueGoodStacking()
        {
            Inventory inv = new Inventory(3);
            Gold g1 = new Gold(50);
            Gold g2 = new Gold(30);

            inv.AddItem(g1);
            if (inv.GetGold() != 50)
            {
                GD.PrintErr($"FAIL: Gold should be 50, got {inv.GetGold()}");
                return false;
            }

            // Second gold addition should merge into first slot
            inv.AddItem(g2);
            if (inv.GetGold() != 80)
            {
                GD.PrintErr($"FAIL: Gold should be 80, got {inv.GetGold()}");
                return false;
            }

            if (inv.GetSlot(0).Quantity != 80)
            {
                GD.PrintErr($"FAIL: Slot 0 quantity should be 80, got {inv.GetSlot(0).Quantity}");
                return false;
            }

            if (!inv.GetSlot(1).IsEmpty)
            {
                GD.PrintErr("FAIL: Slot 1 should still be empty because gold stacked into slot 0");
                return false;
            }

            GD.Print("PASS: TestNonUniqueGoodStacking");
            return true;
        }

        private static bool TestUniqueGoodDistinctSlots()
        {
            Inventory inv = new Inventory(4);
            Loincloth cloth1 = new Loincloth("Troll Loincloth", "Green", 3, "Dragon Scales");
            Loincloth cloth2 = new Loincloth("Peasant Loincloth", "Burlap", 1, "Cloth");

            inv.AddItem(cloth1);
            inv.AddItem(cloth2);

            if (inv.GetSlot(0).IsEmpty || inv.GetSlot(0).Item != cloth1)
            {
                GD.PrintErr("FAIL: Slot 0 should contain cloth1");
                return false;
            }

            if (inv.GetSlot(1).IsEmpty || inv.GetSlot(1).Item != cloth2)
            {
                GD.PrintErr("FAIL: Slot 1 should contain cloth2");
                return false;
            }

            if (cloth1.UniqueId == cloth2.UniqueId)
            {
                GD.PrintErr("FAIL: Unique goods must have different UniqueIds");
                return false;
            }

            GD.Print("PASS: TestUniqueGoodDistinctSlots");
            return true;
        }

        private static bool TestNonUniqueTransfer()
        {
            Inventory mobInv = new Inventory(4);
            Inventory chestInv = new Inventory(6);

            mobInv.AddGold(100);
            chestInv.AddGold(25);

            // Transfer 40 gold from mob to chest
            bool success = mobInv.TransferSlotTo(chestInv, 0, 40);
            if (!success)
            {
                GD.PrintErr("FAIL: TransferSlotTo returned false");
                return false;
            }

            if (mobInv.GetGold() != 60)
            {
                GD.PrintErr($"FAIL: mobInv should have 60 gold, got {mobInv.GetGold()}");
                return false;
            }

            if (chestInv.GetGold() != 65)
            {
                GD.PrintErr($"FAIL: chestInv should have 65 gold, got {chestInv.GetGold()}");
                return false;
            }

            GD.Print("PASS: TestNonUniqueTransfer");
            return true;
        }

        private static bool TestUniqueTransferAndPersistence()
        {
            Inventory sourceInv = new Inventory(2);
            Inventory targetInv = new Inventory(2);

            Loincloth magicCloth = new Loincloth("Enchanted Silk Loincloth", "Purple", 5, "Silk");
            magicCloth.Durability = 42;
            string originalId = magicCloth.UniqueId;

            sourceInv.AddItem(magicCloth);
            bool transferred = sourceInv.TransferSlotTo(targetInv, 0);

            if (!transferred)
            {
                GD.PrintErr("FAIL: Failed to transfer unique item");
                return false;
            }

            if (!sourceInv.GetSlot(0).IsEmpty)
            {
                GD.PrintErr("FAIL: Source slot should now be empty");
                return false;
            }

            InventorySlot targetSlot = targetInv.GetSlot(0);
            if (targetSlot.IsEmpty || !(targetSlot.Item is Loincloth received))
            {
                GD.PrintErr("FAIL: Target slot does not contain Loincloth");
                return false;
            }

            if (received.UniqueId != originalId || received.CustomName != "Enchanted Silk Loincloth" || received.Durability != 42 || received.Defense != 5)
            {
                GD.PrintErr("FAIL: Loincloth unique characteristics did not persist across transfer");
                return false;
            }

            GD.Print("PASS: TestUniqueTransferAndPersistence");
            return true;
        }

        private static bool TestCharacterGoldIntegration()
        {
            Peasant peasant = new Peasant();
            peasant.EnsureInventory();

            if (peasant.gold != 10)
            {
                GD.PrintErr($"FAIL: Peasant initial gold should be 10, got {peasant.gold}");
                return false;
            }

            peasant.gold += 15;
            if (peasant.inventory.GetGold() != 25)
            {
                GD.PrintErr($"FAIL: Peasant inventory gold should be 25, got {peasant.inventory.GetGold()}");
                return false;
            }

            peasant.gold -= 5;
            if (peasant.inventory.GetGold() != 20)
            {
                GD.PrintErr($"FAIL: Peasant inventory gold should be 20, got {peasant.inventory.GetGold()}");
                return false;
            }

            GD.Print("PASS: TestCharacterGoldIntegration");
            return true;
        }

        private static bool TestWoodResourceAndPlayerStash()
        {
            Inventory inv = new Inventory(2);
            inv.AddWood(50);
            if (inv.GetWood() != 50)
            {
                GD.PrintErr($"FAIL: Wood count should be 50, got {inv.GetWood()}");
                return false;
            }

            inv.AddWood(30);
            if (inv.GetWood() != 80)
            {
                GD.PrintErr($"FAIL: Wood count should be 80, got {inv.GetWood()}");
                return false;
            }

            inv.RemoveWood(25);
            if (inv.GetWood() != 55)
            {
                GD.PrintErr($"FAIL: Wood count should be 55, got {inv.GetWood()}");
                return false;
            }

            PlayerData pd = new PlayerData();
            pd.EnsureInventory();
            if (pd.wood != 100)
            {
                GD.PrintErr($"FAIL: PlayerData initial wood stash should be 100, got {pd.wood}");
                return false;
            }

            pd.inventory.RemoveWood(40);
            if (pd.wood != 60)
            {
                GD.PrintErr($"FAIL: PlayerData wood after spending 40 should be 60, got {pd.wood}");
                return false;
            }

            GD.Print("PASS: TestWoodResourceAndPlayerStash");
            return true;
        }

        private static bool TestLaborCostAndConstructionTransition()
        {
            LaborCost lc = new LaborCost(50f);
            if (lc.labor_strength != 50f || lc.IsComplete)
            {
                GD.PrintErr($"FAIL: LaborCost initial strength should be 50, got {lc.labor_strength}");
                return false;
            }

            lc.ApplyLabor(20f);
            if (lc.labor_strength != 30f)
            {
                GD.PrintErr($"FAIL: LaborCost after applying 20 should be 30, got {lc.labor_strength}");
                return false;
            }

            lc.ApplyLabor(35f);
            if (lc.labor_strength != 0f || !lc.IsComplete)
            {
                GD.PrintErr($"FAIL: LaborCost after applying 35 should be 0 and complete, got {lc.labor_strength}");
                return false;
            }

            // Verify building default labor costs
            if (Building.GetDefaultLaborCost(Building.Type.HUT).labor_strength != 50f ||
                Building.GetDefaultLaborCost(Building.Type.TENT).labor_strength != 30f ||
                Building.GetDefaultLaborCost(Building.Type.WATER_BARREL).labor_strength != 15f ||
                Building.GetDefaultLaborCost(Building.Type.DEBUG).labor_strength != 10f)
            {
                GD.PrintErr("FAIL: Building default labor costs do not match expected values");
                return false;
            }

            // Test building construction transition
            Building building = new Building();
            building.type = Building.Type.HUT;
            building.total_labor_needed = Building.GetDefaultLaborCost(Building.Type.HUT);
            building.remaining_labor = building.total_labor_needed;

            if (!building.is_under_construction)
            {
                GD.PrintErr("FAIL: Building should initially be under construction");
                building.QueueFree();
                return false;
            }

            building.ApplyLabor(20f);
            if (building.remaining_labor.labor_strength != 30f || !building.is_under_construction)
            {
                GD.PrintErr($"FAIL: Building labor should be 30 and still under construction, got {building.remaining_labor.labor_strength}");
                building.QueueFree();
                return false;
            }

            bool completionSignalFired = false;
            building.ConstructionFinished += () => completionSignalFired = true;

            building.ApplyLabor(30f);
            if (building.remaining_labor.labor_strength != 0f || building.is_under_construction || building.construction_state != Building.ConstructionState.BUILT)
            {
                GD.PrintErr($"FAIL: Building should be BUILT and not under construction, got {building.construction_state}");
                building.QueueFree();
                return false;
            }

            if (!completionSignalFired)
            {
                GD.PrintErr("FAIL: ConstructionFinished signal did not fire");
                building.QueueFree();
                return false;
            }

            building.QueueFree();

            GD.Print("PASS: TestLaborCostAndConstructionTransition");
            return true;
        }

        private static bool TestBuildingDataAndFootprintCells()
        {
            BuildingData data = new BuildingData("hut", "Hut", Building.Type.HUT, 40, 50f, new Vector2I(3, 2), null);
            if (data.Id != "hut" || data.DisplayName != "Hut" || data.WoodCost != 40 || data.LaborStrength != 50f || data.FootprintSize != new Vector2I(3, 2))
            {
                GD.PrintErr("FAIL: BuildingData properties mismatch");
                return false;
            }

            Building hut = new Building();
            hut.footprint_tile_size = new Vector2I(3, 2);
            var cells = System.Linq.Enumerable.ToList(hut.GetFootprintCells());
            if (cells.Count != 6)
            {
                GD.PrintErr($"FAIL: 3x2 footprint should have 6 cells, got {cells.Count}");
                hut.QueueFree();
                return false;
            }

            if (!cells.Contains(new Vector2I(0, 0)) || !cells.Contains(new Vector2I(2, 1)))
            {
                GD.PrintErr("FAIL: 3x2 footprint missing expected cells");
                hut.QueueFree();
                return false;
            }

            Building tent = new Building();
            tent.footprint_tile_size = new Vector2I(5, 2);
            var tentCells = System.Linq.Enumerable.ToList(tent.GetFootprintCells());
            if (tentCells.Count != 10)
            {
                GD.PrintErr($"FAIL: 5x2 footprint should have 10 cells, got {tentCells.Count}");
                tent.QueueFree();
                hut.QueueFree();
                return false;
            }

            hut.QueueFree();
            tent.QueueFree();

            GD.Print("PASS: TestBuildingDataAndFootprintCells");
            return true;
        }

        private static bool TestBuildingInspectionData()
        {
            // 1. Display name mapping
            if (Building.GetDefaultDisplayName(Building.Type.HUT) != "Hut" ||
                Building.GetDefaultDisplayName(Building.Type.TENT) != "Tent" ||
                Building.GetDefaultDisplayName(Building.Type.WATER_BARREL) != "Water Barrel" ||
                Building.GetDefaultDisplayName(Building.Type.DEBUG) != "Debug Building")
            {
                GD.PrintErr("FAIL: GetDefaultDisplayName mappings incorrect");
                return false;
            }

            // 2. Under construction inspection data
            Building building = new Building();
            building.type = Building.Type.HUT;
            building.display_name = "Chief's Hut";
            building.initial_labor_strength = 50f;
            building.total_labor_needed = new LaborCost(50f);
            building.remaining_labor = new LaborCost(50f);
            building.construction_state = Building.ConstructionState.UNDER_CONSTRUCTION;
            building.game_board_cell = new Vector2I(10, 5);
            building.footprint_tile_size = new Vector2I(3, 2);

            if (!building.is_under_construction)
            {
                GD.PrintErr("FAIL: Building should be under construction");
                building.QueueFree();
                return false;
            }

            string expectedLaborString = $"Remaining Labor: {building.remaining_labor.labor_strength:F0} / {building.total_labor_needed.labor_strength:F0}";
            if (expectedLaborString != "Remaining Labor: 50 / 50")
            {
                GD.PrintErr($"FAIL: Unexpected labor string format: {expectedLaborString}");
                building.QueueFree();
                return false;
            }

            // 3. Coordinate hit testing via footprint
            var footprint = System.Linq.Enumerable.ToList(building.GetFootprintCells());
            Vector2I testCellInside = new Vector2I(11, 6);
            Vector2I localInside = testCellInside - building.game_board_cell;
            if (!footprint.Contains(localInside))
            {
                GD.PrintErr("FAIL: Cell (11,6) should be inside hut footprint");
                building.QueueFree();
                return false;
            }

            Vector2I testCellOutside = new Vector2I(14, 5);
            Vector2I localOutside = testCellOutside - building.game_board_cell;
            if (footprint.Contains(localOutside))
            {
                GD.PrintErr("FAIL: Cell (14,5) should be outside hut footprint");
                building.QueueFree();
                return false;
            }

            // 4. Built building inspection transition
            building.ApplyLabor(50f);
            if (building.is_under_construction)
            {
                GD.PrintErr("FAIL: Building should now be built");
                building.QueueFree();
                return false;
            }

            building.QueueFree();

            GD.Print("PASS: TestBuildingInspectionData");
            return true;
        }

        private static bool TestBuildingUnlockSystem()
        {
            BuildingData unlockedData = new BuildingData("hut", "Hut", Building.Type.HUT, 40, 50f, new Vector2I(3, 2), null, isUnlockedByDefault: true);
            BuildingData lockedData = new BuildingData("water_barrel", "Barrel", Building.Type.WATER_BARREL, 10, 15f, new Vector2I(1, 1), null, isUnlockedByDefault: false);

            PlayerData playerData = new PlayerData();

            // 1. Initial state
            if (!playerData.IsBuildingUnlocked(unlockedData))
            {
                GD.PrintErr("FAIL: Default unlocked building should report unlocked");
                playerData.QueueFree();
                return false;
            }

            if (playerData.IsBuildingUnlocked(lockedData))
            {
                GD.PrintErr("FAIL: Locked building should not report unlocked initially");
                playerData.QueueFree();
                return false;
            }

            if (playerData.IsBuildingUnlocked("water_barrel"))
            {
                GD.PrintErr("FAIL: Unlocked IDs should not contain water_barrel initially");
                playerData.QueueFree();
                return false;
            }

            // 2. Unlock dynamically
            playerData.UnlockBuilding(lockedData);
            if (!playerData.IsBuildingUnlocked(lockedData) || !playerData.IsBuildingUnlocked("water_barrel"))
            {
                GD.PrintErr("FAIL: Building should report unlocked after UnlockBuilding");
                playerData.QueueFree();
                return false;
            }

            // 3. Lock dynamically
            playerData.LockBuilding("water_barrel");
            if (playerData.IsBuildingUnlocked(lockedData) || playerData.IsBuildingUnlocked("water_barrel"))
            {
                GD.PrintErr("FAIL: Building should report locked after LockBuilding");
                playerData.QueueFree();
                return false;
            }

            playerData.QueueFree();

            GD.Print("PASS: TestBuildingUnlockSystem");
            return true;
        }
    }
}
