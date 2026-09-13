using System;
using Godot;

namespace BridgeTroll
{
    public partial class Character
    {
        // None State
        public virtual void UniqueEnterNoneState() { }

        public void EnterNoneState()
        {
            ExitCurrentState();
            animated_sprite_.Play("base");
            state = CharacterState.NONE;
            UniqueEnterNoneState();
        }

        public virtual void UniqueNoneState() { }

        public void NoneState()
        {
            UniqueNoneState();
        }

        public virtual void UniqueExitNoneState() { }

        public void ExitNoneState()
        {
            UniqueExitNoneState();
        }
    }
}
