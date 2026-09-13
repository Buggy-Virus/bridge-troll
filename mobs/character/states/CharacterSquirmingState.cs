using System;
using Godot;

namespace BridgeTroll
{
    public partial class Character
    {
        // Squirming State
        public int OfferGoldAmount()
        {
            return gold / 4;
        }

        public virtual void UniqueEnterSquirmingState() { }

        public void EnterSquirmingState()
        {
            ExitCurrentState();
            animated_sprite_.Play("squirm");
            state = CharacterState.SQUIRMING;
            UniqueEnterSquirmingState();
        }

        public virtual void UniqueSquirmingState() { }

        public void SquirmingState()
        {
            if (!IsInstanceValid(grappling_character))
            {
                EnterNoneState();
                return;
            }

            if (grappling_character.state != CharacterState.GRAPPLING)
            {
                EnterNoneState();
                return;
            }

            UniqueSquirmingState();
        }

        public virtual void UniqueExitSquirmingState() { }

        public void ExitSquirmingState()
        {
            grappling_character = null;
            UniqueExitSquirmingState();
        }
    }
}
