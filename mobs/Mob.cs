using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class Mob : Character
    {
        public enum Type
        {
            DEBUG,
            WAR_MOB,
            PEASANT,
            JOGGER,
            NEERDOWELL,
            TRADER,
            THUG,
            ADVENTURER,
            PRIEST,
            BISHOP,
            MAGE,
            WIZARD,
            TRADESMAN,
            APPRENTICE,
        }

        public float update_exit_position_threshold = 150f;
        public float exit_threshold = 50f;

        public AutoLevelDistribution auto_level_distribution = new(0.34f, 0.33f, 0.33f);

        public void SpendPendingLevelUp()
        {
            if (pending_level_ups <= 0) return;

            int totalStatPoints = GetStatPointsEarnedForLevel(level);
            int strPoints = (int)Math.Round(totalStatPoints * auto_level_distribution.strength_allocation_fraction);
            int charPoints = (int)Math.Round(totalStatPoints * auto_level_distribution.charisma_allocation_fraction);
            int intPoints = (int)Math.Round(totalStatPoints * auto_level_distribution.intelligence_allocation_fraction);

            int remainder = totalStatPoints - (strPoints + charPoints + intPoints);
            if (remainder != 0)
            {
                if (auto_level_distribution.strength_allocation_fraction >= auto_level_distribution.charisma_allocation_fraction &&
                    auto_level_distribution.strength_allocation_fraction >= auto_level_distribution.intelligence_allocation_fraction)
                {
                    strPoints += remainder;
                }
                else if (auto_level_distribution.charisma_allocation_fraction >= auto_level_distribution.intelligence_allocation_fraction)
                {
                    charPoints += remainder;
                }
                else
                {
                    intPoints += remainder;
                }
            }

            ApplyStatPoints(strPoints, charPoints, intPoints);
            ConsumePendingLevelUp();
        }

        public override void OnLevelUpEarned()
        {
            base.OnLevelUpEarned();
            SpendPendingLevelUp();
        }

        public override void UniqueReady()
        {
            surrender_hit_points = 3;
        }

        public override void UniqueEnterNoneState()
        {
            EnterWalkingState();
        }

        private void CheckUpdateExitPosition()
        {
            if (Math.Abs(Position.Y - target_position.Y) > update_exit_position_threshold)
            {
                target_position = new(target_position.X, Position.Y);
            }
        }

        private void CheckExitGameBoard()
        {
            if (Position.DistanceTo(target_position) < exit_threshold)
            {
                QueueFree();
            }
        }

        public override void UniqueWalkingState()
        {
            CheckUpdateExitPosition();
            CheckExitGameBoard();
            CheckForScary();
        }
    }
}
