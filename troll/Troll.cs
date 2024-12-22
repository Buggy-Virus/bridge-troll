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
        [Export]
        PackedScene reward_floater_packed_scene;

        public DecisionPopup decision_popup_;
        public List<VictimOption> decision_list = new();
        private Node2D floater_parent_;

        public int total_experience = 0;

        public override void UniqueReady()
        {
            walk_speed = 600.0f;
            run_speed = 10000.0f;

            max_hit_points = 100;
            hit_points = 100;

            scary = 10;

            damage = 4;

            decision_popup_ = GetNode<DecisionPopup>("DecisionPopup");
            floater_parent_ = GetNode<Node2D>("FloaterParent");
            target_position = Position;
            decision_popup_.Visible = false;
        }

        public void AwardGold(int gold_amount)
        {
            gold += gold_amount;

            RewardFloater gold_floater = reward_floater_packed_scene.Instantiate<RewardFloater>();
            floater_parent_.AddChild(gold_floater);
            gold_floater.MakeGoldFloater(gold_amount);
        }

        public void AwardExperience(int experience_amount)
        {
            total_experience += experience_amount;

            RewardFloater experience_floater =
                reward_floater_packed_scene.Instantiate<RewardFloater>();
            floater_parent_.AddChild(experience_floater);
            experience_floater.MakeExperienceFloater(experience_amount);
        }

        public override void UniqueEnterGrapplingState()
        {
            decision_popup_.Visible = true;
            UpdateVictimOption();
        }

        public void UpdateVictimOption()
        {
            decision_list.Clear();
            decision_list.Add(VictimOption.RELEASE);
            if (!victim_character.finished_transaction)
            {
                if (!victim_character.options_experienced.Contains(VictimOption.EXTORT))
                {
                    decision_list.Add(VictimOption.EXTORT);
                }

                if (
                    victim_character.options_experienced.Contains(VictimOption.EXTORT)
                    && !victim_character.options_experienced.Contains(VictimOption.PRESS)
                )
                {
                    decision_list.Add(VictimOption.PRESS);
                }

                if (victim_character.offered_payment > 0)
                {
                    decision_list.Add(VictimOption.ACCEPT);
                }

                if (
                    !victim_character.options_experienced.Contains(VictimOption.MUG)
                    && !victim_character.is_surrendered
                )
                {
                    decision_list.Add(VictimOption.MUG);
                }

                if (
                    !victim_character.options_experienced.Contains(VictimOption.ROB)
                    && victim_character.is_surrendered
                )
                {
                    decision_list.Add(VictimOption.ROB);
                }

                if (
                    !victim_character.options_experienced.Contains(VictimOption.EAT)
                    && victim_character.is_surrendered
                )
                {
                    decision_list.Add(VictimOption.EAT);
                }
            }

            decision_popup_.UpdateOfferedPayment(victim_character.offered_payment);
            decision_popup_.UpdateOptions(decision_list);
        }

        public override void UniqueExitGrapplingState()
        {
            decision_popup_.Visible = false;
            decision_list = new();
        }

        public void StartExtortCharacter(Character victim)
        {
            victim.options_experienced.Add(VictimOption.EXTORT);
            victim.offered_payment = victim.OfferGoldAmount();
            UpdateVictimOption();
        }

        public void StartPressCharacter(Character victim)
        {
            victim.options_experienced.Add(VictimOption.PRESS);
            victim.offered_payment += victim.OfferGoldAmount();
            UpdateVictimOption();
        }

        public void StartMugCharacter(Character victim)
        {
            victim.options_experienced.Add(VictimOption.MUG);
            victim.is_defensive = true;
            StartFightingCharacter(victim);
        }

        public void StartEatCharacter(Character victim)
        {
            victim.options_experienced.Add(VictimOption.EAT);
            AwardExperience(victim.max_hit_points);
            Heal(victim.hit_points);
            victim.Die();
            EnterNoneState();
        }

        public void StartAcceptCharacter(Character victim)
        {
            victim.options_experienced.Add(VictimOption.ACCEPT);
            AwardGold(victim.offered_payment);
            AwardExperience(offered_payment / 2);
            victim.gold -= victim.offered_payment;
            victim.offered_payment = 0;
            victim.finished_transaction = true;
            EnterNoneState();
        }

        public void StartRobCharacter(Character victim)
        {
            victim.options_experienced.Add(VictimOption.ROB);
            AwardGold(victim.gold);
            AwardExperience(victim.gold / 2);
            victim.gold = 0;
            victim.finished_transaction = true;
            EnterNoneState();
        }

        public void StartReleaseCharacter(Character victim)
        {
            EnterNoneState();
        }
    }
}
