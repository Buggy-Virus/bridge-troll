using System;
using Godot;

namespace BridgeTroll
{
    public partial class Character
    {
        // Hunting State
        public float catch_range = 20;

        public void StartHuntingCharacter(Character hunt_character)
        {
            target_character = hunt_character;
            EnterHuntingState();
        }

        public virtual void ResolveCatchingTarget()
        {
            if (!target_character.is_aggressive && !target_character.is_defensive)
            {
                StartGrapplingCharacter(target_character);
            }
            else
            {
                StartFightingCharacter(target_character);
            }
        }

        public virtual void UniqueEnterHuntingState() { }

        public void EnterHuntingState()
        {
            ExitCurrentState();
            animated_sprite_.Play("walk");
            state = CharacterState.HUNTING;
            UniqueEnterHuntingState();
        }

        public virtual void UniqueHuntingState() { }

        public void HuntingState()
        {
            if (!IsInstanceValid(target_character))
            {
                EnterNoneState();
                return;
            }
            if (Position.DistanceTo(target_character.Position) < catch_range)
            {
                ResolveCatchingTarget();
                return;
            }
            navigation_agent_.TargetPosition = target_character.Position;
            MoveTowardsTarget(walk_speed);
            UniqueHuntingState();
        }

        public virtual void UniqueExitHuntingState() { }

        public void ExitHuntingState()
        {
            target_character = null;
            UniqueExitHuntingState();
        }
    }
}
