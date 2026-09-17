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
        [Signal]
        public delegate void StatsChangedEventHandler();

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

        public CharacterStats stats = new();

        public int base_strength
        {
            get => stats.base_strength;
            set
            {
                if (stats.base_strength != value)
                {
                    stats.base_strength = value;
                    RecalculateDependentStats();
                }
            }
        }

        public int base_intelligence
        {
            get => stats.base_intelligence;
            set
            {
                if (stats.base_intelligence != value)
                {
                    stats.base_intelligence = value;
                    RecalculateDependentStats();
                }
            }
        }

        public int base_charisma
        {
            get => stats.base_charisma;
            set
            {
                if (stats.base_charisma != value)
                {
                    stats.base_charisma = value;
                    RecalculateDependentStats();
                }
            }
        }

        public float base_speed
        {
            get => stats.base_speed;
            set
            {
                if (Math.Abs(stats.base_speed - value) > 0.001f)
                {
                    stats.base_speed = value;
                    RecalculateDependentStats();
                }
            }
        }

        public int total_experience = 0;
        public int level = 1;
        public int current_level { get => level; set => level = value; }
        public int experience_needed = 10;
        public int pending_level_ups = 0;

        public int strength_feat_points = 0;
        public int intelligence_feat_points = 0;
        public int charisma_feat_points = 0;

        public int labor_intelligence = 0;
        public int labor_strength = 0;
        public int labor_charisma = 0;
        public Dictionary<string, int> resources = new(StringComparer.OrdinalIgnoreCase);

        public int GetResource(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return 0;
            resources.TryGetValue(resourceName, out int count);
            return count;
        }

        public void AddResource(string resourceName, int count = 1)
        {
            if (string.IsNullOrEmpty(resourceName) || count <= 0) return;
            if (resources.ContainsKey(resourceName))
                resources[resourceName] += count;
            else
                resources[resourceName] = count;
        }

        public bool HasResource(string resourceName, int count = 1)
        {
            return GetResource(resourceName) >= count;
        }

        public bool SpendResource(string resourceName, int count = 1)
        {
            if (!HasResource(resourceName, count)) return false;
            resources[resourceName] -= count;
            return true;
        }

        public void AddLabor(int intelligence = 0, int strength = 0, int charisma = 0)
        {
            labor_intelligence += intelligence;
            labor_strength += strength;
            labor_charisma += charisma;
        }

        public System.Collections.Generic.Dictionary<int, LevelUpData> level_data = new()
        {
            { 1, new LevelUpData(10, 5) },
            { 2, new LevelUpData(25, 5) },
            { 3, new LevelUpData(50, 5) },
            { 4, new LevelUpData(100, 5) },
            { 5, new LevelUpData(175, 5) },
            { 6, new LevelUpData(275, 5) },
            { 7, new LevelUpData(400, 5) },
            { 8, new LevelUpData(550, 5) },
            { 9, new LevelUpData(750, 5) },
            { 10, new LevelUpData(1000, 5) }
        };

        public System.Collections.Generic.Dictionary<int, int> level_to_experience_needed = new()
        {
            { 1, 10 },
            { 2, 25 },
            { 3, 50 },
            { 4, 100 },
            { 5, 175 },
            { 6, 275 },
            { 7, 400 },
            { 8, 550 },
            { 9, 750 },
            { 10, 1000 }
        };

        public System.Collections.Generic.Dictionary<int, int> level_to_stat_points_earned = new()
        {
            { 1, 5 },
            { 2, 5 },
            { 3, 5 },
            { 4, 5 },
            { 5, 5 },
            { 6, 5 },
            { 7, 5 },
            { 8, 5 },
            { 9, 5 },
            { 10, 5 }
        };

        public int GetExperienceNeededForLevel(int lvl)
        {
            if (level_data.TryGetValue(lvl, out var data))
            {
                return data.experience_needed;
            }
            if (level_to_experience_needed.TryGetValue(lvl, out int exp))
            {
                return exp;
            }
            return Math.Max(1, lvl * 100);
        }

        public int GetStatPointsEarnedForLevel(int lvl)
        {
            if (level_data.TryGetValue(lvl, out var data))
            {
                return data.stat_points_earned;
            }
            if (level_to_stat_points_earned.TryGetValue(lvl, out int pts))
            {
                return pts;
            }
            return 5;
        }

        public bool HasPendingLevelUps() => pending_level_ups > 0;

        public void ConsumePendingLevelUp()
        {
            if (pending_level_ups > 0)
            {
                pending_level_ups--;
            }
        }

        /// <summary>
        /// Recalculates all stored dependent values (damage, max HP, walk/run/limp speed,
        /// scary, courage, surrender HP) based on current underlying stats, updates UI elements,
        /// and emits the StatsChanged signal. Called whenever stats or modifiers are altered.
        /// </summary>
        public virtual void RecalculateDependentStats()
        {
            damage = stats.CalculateDamage();
            max_hit_points = stats.CalculateMaxHitPoints();
            hit_points = Math.Min(hit_points, max_hit_points);

            walk_speed = stats.CalculateWalkSpeed();
            run_speed = stats.CalculateRunSpeed();
            limp_speed = stats.CalculateLimpSpeed();

            scary = stats.CalculateScary();
            courage = stats.CalculateEffectiveCourage();
            surrender_hit_points = stats.CalculateSurrenderHitPoints();

            if (health_bar != null)
            {
                health_bar.MaxValue = max_hit_points;
                health_bar.Value = hit_points;
            }

            EmitSignal(SignalName.StatsChanged);
        }

        public virtual void ApplyStatPoints(int strPoints, int charPoints, int intPoints)
        {
            stats.base_strength += strPoints;
            stats.base_charisma += charPoints;
            stats.base_intelligence += intPoints;

            strength_feat_points += strPoints;
            charisma_feat_points += charPoints;
            intelligence_feat_points += intPoints;

            hit_points += (strPoints * 2);
            RecalculateDependentStats();
        }

        public virtual void OnLevelUpEarned() { }

        public virtual void AwardExperience(int experience_amount)
        {
            total_experience += experience_amount;
            experience_needed -= experience_amount;

            while (experience_needed <= 0)
            {
                pending_level_ups++;
                level++;
                int excess = -experience_needed;
                int nextLevelExpNeeded = Math.Max(1, GetExperienceNeededForLevel(level));
                experience_needed = nextLevelExpNeeded - excess;
                OnLevelUpEarned();
            }
        }

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

            experience_needed = GetExperienceNeededForLevel(level);

            RecalculateDependentStats();
            hit_points = max_hit_points;

            EnterNoneState();
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
