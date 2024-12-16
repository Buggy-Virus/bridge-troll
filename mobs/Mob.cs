using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public enum MobType
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

    public partial class Mob : Character
    {
        public float update_exit_position_threshold = 150f;
        public float exit_threshold = 50f;

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
