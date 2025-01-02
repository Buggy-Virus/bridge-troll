using System;
using System.Collections.Generic;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public enum CharacterType
    {
        DEBUG,
        TROLL,
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

    public enum CharacterState
    {
        NONE,
        ESCAPING,
        FLEEING,
        WALKING,
        WORKING,
        HUNTING,
        FIGHTING,
        GRAPPLING,
        SQUIRMING,
        QUEUEING,
    }

    public partial class Character : CharacterBody2D
    {
        public CharacterType type = CharacterType.DEBUG;
        public string name = "debug";

        public float limp_speed = 50.0f;
        public float walk_speed = 100.0f;
        public float run_speed = 300.0f;

        public int max_hit_points = 10;
        public int hit_points = 10;
        public int surrender_hit_points = 0;
        public int damage = 1;
        public int courage = 1;
        public int scary = 0;
        public bool is_defensive = false;
        public bool is_aggressive = false;

        public int gold = 10;
        public int offered_payment = 0;
        public bool finished_transaction = false;

        public int experience_yield = 10;

        public CharacterState state = CharacterState.NONE;

        public List<VictimOption> options_experienced = new();
        public bool is_surrendered = false;

        public AnimatedSprite2D animated_sprite_;
        public CollisionShape2D collision_shape;
        public ProgressBar health_bar;
        public Area2D selection_area;
        public Area2D scare_area;

        public NavigationAgent2D navigation_agent_;
        public Vector2 target_position = new(0, 0);
        public Character target_character = null;

        public Character scary_character = null;
        public float necessary_away_from_scary = 200;
        public Character victim_character = null;
        public Character grappling_character = null;

        public Character fighting_character = null;

        public void Die()
        {
            GD.Print("Goodbye Cruel World!");
            QueueFree();
        }

        public void Damage(int damage_amount)
        {
            hit_points -= damage_amount;
            health_bar.Value = hit_points;
            if (hit_points < 0)
            {
                Die();
            }
        }

        public void Heal(int heal_amount)
        {
            hit_points += heal_amount;
            hit_points = Math.Min(hit_points, max_hit_points);
            health_bar.Value = hit_points;
        }

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
            }
            MoveTowardsTarget(walk_speed);
            UniqueWalkingState();
        }

        public void UniqueExitWalkingState() { }

        public void ExitWalkingState()
        {
            animated_sprite_.Stop();
            UniqueExitWalkingState();
        }

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

        // Squirming
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

        // Fleeing
        public void CheckForScary()
        {
            foreach (Area2D area in scare_area.GetOverlappingAreas())
            {
                if (
                    area.IsInGroup("Scary_Characters")
                    && ((Character)area.GetParent()).scary > courage
                    && ((Character)area.GetParent()).Velocity != new Vector2(0, 0)
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
            if (Position.DistanceTo(scary_character.Position) > necessary_away_from_scary)
            {
                EnterNoneState();
            }
            Velocity = -Position.DirectionTo(scary_character.Position) * run_speed;
            MoveAndSlide();
            UniqueFleeingState();
        }

        public virtual void UniqueExitFleeingState() { }

        public void ExitFleeingState()
        {
            UniqueExitFleeingState();
        }

        // Fighting
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
            if (state == CharacterState.FIGHTING)
            {
                fighting_character.Damage(damage);
            }
        }

        public virtual void UniqueEnterFightingState() { }

        public void EnterFightingState()
        {
            ExitCurrentState();
            state = CharacterState.FIGHTING;
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

        // General Functions
        public void MoveTowardsTarget(float speed)
        {
            Velocity = Position.DirectionTo(navigation_agent_.GetNextPathPosition()) * speed;
            MoveAndSlide();
        }

        public void ExitCurrentState()
        {
            Velocity = new Vector2(0, 0);
            animated_sprite_.Stop();
            if (state == CharacterState.NONE)
            {
                ExitNoneState();
            }
            else if (state == CharacterState.WALKING)
            {
                ExitWalkingState();
            }
            else if (state == CharacterState.HUNTING)
            {
                ExitHuntingState();
            }
            else if (state == CharacterState.GRAPPLING)
            {
                ExitGrapplingState();
            }
            else if (state == CharacterState.SQUIRMING)
            {
                ExitSquirmingState();
            }
            else if (state == CharacterState.FLEEING)
            {
                ExitFleeingState();
            }
            else if (state == CharacterState.FIGHTING)
            {
                ExitFightingState();
            }
        }

        public virtual void UniqueReady() { }

        public override void _Ready()
        {
            UniqueReady();
            navigation_agent_ = GetNode<NavigationAgent2D>("NavigationAgent2D");
            animated_sprite_ = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            collision_shape = GetNode<CollisionShape2D>("CollisionShape2D");
            selection_area = GetNode<Area2D>("SelectionArea");
            scare_area = GetNode<Area2D>("ScareArea");
            health_bar = GetNode<ProgressBar>("HealthBar");

            EnterNoneState();
            hit_points = max_hit_points;
            health_bar.MaxValue = max_hit_points;
            health_bar.Value = max_hit_points;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (state == CharacterState.NONE)
            {
                NoneState();
            }
            else if (state == CharacterState.WALKING)
            {
                WalkingState();
            }
            else if (state == CharacterState.HUNTING)
            {
                HuntingState();
            }
            else if (state == CharacterState.GRAPPLING)
            {
                GrapplingState();
            }
            else if (state == CharacterState.SQUIRMING)
            {
                SquirmingState();
            }
            else if (state == CharacterState.FLEEING)
            {
                FleeingState();
            }
            else if (state == CharacterState.FIGHTING)
            {
                FightingState();
            }

            {
                if (Velocity.X < 0)
                {
                    animated_sprite_.FlipH = true;
                }
                else
                {
                    animated_sprite_.FlipH = false;
                }
            }
        }
    }
}
