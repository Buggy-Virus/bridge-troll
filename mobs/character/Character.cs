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
            if (hit_points <= 0)
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

        public void MoveTowardsTarget(float speed)
        {
            Velocity = Position.DirectionTo(navigation_agent_.GetNextPathPosition()) * speed;
            if (Velocity.X < 0)
            {
                animated_sprite_.FlipH = false;
            }
            else
            {
                animated_sprite_.FlipH = true;
            }
            MoveAndSlide();
        }

        public void ExitCurrentState()
        {
            Velocity = Vector2.Zero;
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
        }
    }
}
