using System;
using Godot;

namespace BridgeTroll
{
    public partial class Character
    {
        // Walking State
        public void SetTargetPosition(Vector2 position)
        {
            target_position = position;
        }

        public virtual void UniqueEnterWalkingState() { }

        public void EnterWalkingState()
        {
            ExitCurrentState();
            animated_sprite_.Play("walk");
            state = CharacterState.WALKING;
            UniqueEnterWalkingState();
        }

        public virtual void UniqueWalkingState() { }

        public void WalkingState()
        {
            navigation_agent_.TargetPosition = target_position;
            if (Position.DistanceTo(target_position) < navigation_agent_.TargetDesiredDistance)
            {
                EnterNoneState();
                return;
            }
            MoveTowardsTarget(walk_speed);
            UniqueWalkingState();
        }

        public virtual void UniqueExitWalkingState() { }

        public void ExitWalkingState()
        {
            animated_sprite_.Stop();
            UniqueExitWalkingState();
        }
    }
}
