using System;
using System.Collections.Generic;
using BridgeTroll;
using Godot;
using Godot.Collections;

namespace BridgeTroll
{
    public enum VictimOption
    {
        RELEASE,
        EXTORT,
        PRESS,
        ACCEPT,
        MUG,
        ROB,
        EAT,
    }

    public partial class Troll : Character
    {
        public DecisionPopup decision_popup_;
        public List<VictimOption> decision_list = new();

        public override void UniqueReady()
        {
            walk_speed = 600.0f;
            run_speed = 10000.0f;

            max_hit_points = 100;
            hit_points = 100;

            scary = 10;

            decision_popup_ = GetNode<DecisionPopup>("DecisionPopup");
            target_position = Position;
            decision_popup_.Visible = false;
        }

        public override void UniqueEnterGrapplingState()
        {
            decision_popup_.Visible = true;

            decision_list.Add(VictimOption.RELEASE);
            if (!grappled_character.options_experienced.Contains(VictimOption.EXTORT))
            {
                decision_list.Add(VictimOption.EXTORT);
            }

            if (
                grappled_character.options_experienced.Contains(VictimOption.EXTORT)
                && grappled_character.options_experienced.Contains(VictimOption.PRESS)
            )
            {
                decision_list.Add(VictimOption.PRESS);
            }

            if (grappled_character.offered_payment > 0)
            {
                decision_list.Add(VictimOption.ACCEPT);
            }

            if (
                !grappled_character.options_experienced.Contains(VictimOption.MUG)
                && !grappled_character.is_surrendered
            )
            {
                decision_list.Add(VictimOption.MUG);
            }

            if (
                !grappled_character.options_experienced.Contains(VictimOption.ROB)
                && grappled_character.is_surrendered
            )
            {
                decision_list.Add(VictimOption.ROB);
            }

            if (
                !grappled_character.options_experienced.Contains(VictimOption.EAT)
                && grappled_character.is_surrendered
            )
            {
                decision_list.Add(VictimOption.EAT);
            }
        }

        public override void UniqueExitGrapplingState()
        {
            decision_popup_.Visible = false;
            decision_list = new();
        }

        public void StartExtortCharacter(Character victim) { }

        public void StartMugCharacter(Character victim)
        {
            StartFightingCharacter(victim);
        }

        public void StartEatCharacter(Character victim) { }

        public void StartRobCharacter(Character victim)
        {
            gold += victim.gold;
            victim.gold = 0;
        }
    }
}
