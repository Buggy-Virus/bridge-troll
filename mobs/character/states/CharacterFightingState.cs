using System;
using Godot;

namespace BridgeTroll
{
    public partial class Character
    {
        // Fighting State
        public void StartFightingCharacter(Character target_character)
        {
            fighting_character = target_character;
            target_character.fighting_character ??= this;
            target_character.EnterFightingState();
            GD.Print(target_character);
            GD.Print(target_character.state.ToString());
            EnterFightingState();
        }

        public void _on_animated_sprite_2d_animation_finished()
        {
            if (state == CharacterState.FIGHTING && IsInstanceValid(fighting_character))
            {
                fighting_character.Damage(damage);
            }
        }

        public virtual void UniqueEnterFightingState() { }

        public void EnterFightingState()
        {
            ExitCurrentState();
            state = CharacterState.FIGHTING;

            if (Position.DirectionTo(fighting_character.Position).X < 0)
            {
                animated_sprite_.FlipH = false;
            }
            else
            {
                animated_sprite_.FlipH = true;
            }

            UniqueEnterFightingState();
        }

        public virtual void UniqueFightingState() { }

        public void FightingState()
        {
            if (!IsInstanceValid(fighting_character))
            {
                GD.Print("No Opponent!");
                EnterNoneState();
                return;
            }

            if (fighting_character.hit_points < fighting_character.surrender_hit_points)
            {
                GD.Print("They Surrendered!");
                fighting_character.is_surrendered = true;
                StartGrapplingCharacter(fighting_character);
                return;
            }

            GD.Print("Fighting Character State", fighting_character.state);
            GD.Print("Fighting Character", fighting_character);
            if (fighting_character.state != CharacterState.FIGHTING)
            {
                GD.Print(Name, ": They Left!");
                EnterNoneState();
                return;
            }

            if (!animated_sprite_.IsPlaying())
            {
                animated_sprite_.Play("attack");
            }
            UniqueFightingState();
        }

        public virtual void UniqueExitFightingState() { }

        public void ExitFightingState()
        {
            GD.Print("Exit Fight State Called");
            fighting_character = null;
            UniqueExitFightingState();
        }
    }
}
