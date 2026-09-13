using System;
using Godot;

namespace BridgeTroll
{
    public partial class Character
    {
        // Fleeing State
        public void CheckForScary()
        {
            foreach (Area2D area in scare_area.GetOverlappingAreas())
            {
                if (
                    area.IsInGroup("Scary_Characters")
                    && ((Character)area.GetParent()).scary > courage
                    && ((Character)area.GetParent()).Velocity.Length() > 0
                )
                {
                    scary_character = (Character)area.GetParent();
                    EnterFleeingState();
                }
            }
        }

        public virtual void UniqueEnterFleeingState() { }

        public void EnterFleeingState()
        {
            ExitCurrentState();
            state = CharacterState.FLEEING;
            animated_sprite_.Play("run");
            UniqueEnterFleeingState();
        }

        public virtual void UniqueFleeingState() { }

        public void FleeingState()
        {
            if (!IsInstanceValid(scary_character))
            {
                EnterNoneState();
                return;
            }

            if (Position.DistanceTo(scary_character.Position) > necessary_away_from_scary)
            {
                EnterNoneState();
                return;
            }

            navigation_agent_.TargetPosition =
                Position
                - 2 * (necessary_away_from_scary * Position.DirectionTo(scary_character.Position));
            MoveTowardsTarget(run_speed);
            UniqueFleeingState();
        }

        public virtual void UniqueExitFleeingState() { }

        public void ExitFleeingState()
        {
            UniqueExitFleeingState();
        }
    }
}
