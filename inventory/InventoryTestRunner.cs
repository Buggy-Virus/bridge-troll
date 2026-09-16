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
    }
}
