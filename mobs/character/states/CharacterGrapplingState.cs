using System;
using Godot;

namespace BridgeTroll
{
    public partial class Character
    {
        // Grappling State
        public void StartGrapplingCharacter(Character target_character)
        {
            victim_character = target_character;
            victim_character.grappling_character = this;
            target_character.EnterSquirmingState();
            EnterGrapplingState();
        }

        public virtual void UniqueEnterGrapplingState() { }

        public void EnterGrapplingState()
        {
            ExitCurrentState();
            animated_sprite_.Play("grapple");
            state = CharacterState.GRAPPLING;
            UniqueEnterGrapplingState();
        }

        public virtual void UniqueGrapplingState() { }

        public void GrapplingState()
        {
            UniqueGrapplingState();
        }

        public virtual void UniqueExitGrapplingState() { }

        public void ExitGrapplingState()
        {
            victim_character = null;
            UniqueExitGrapplingState();
        }
    }
}
