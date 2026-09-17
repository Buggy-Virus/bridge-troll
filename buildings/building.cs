using System;
using System.Collections.Generic;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class Building : Node2D
    {
        public enum Type
        {
            DEBUG,
            SIGN,
            BARRICADE,
            FENCE_FRNT,
            FENCE_SIDE,
            FENCE_DIAG,
            WATER_BARREL,
            HUT,
            TENT,
        }

        public enum ConstructionState
        {
            UNDER_CONSTRUCTION,
            BUILT,
        }

        [Signal]
        public delegate void ConstructionFinishedEventHandler();

        [Signal]
        public delegate void LaborAppliedEventHandler(float amountApplied, float remainingLabor);

        public Area2D effect_area;
        public Sprite2D building_art;
        public Sprite2D wrench_overlay;

        [Export]
        public Vector2I footprint_tile_size { get; set; } = Vector2I.One;

        public Vector2I game_board_cell;

        public IEnumerable<Vector2I> GetFootprintCells()
        {
            for (int x = 0; x < footprint_tile_size.X; x++)
            {
                for (int y = 0; y < footprint_tile_size.Y; y++)
                {
                    yield return new Vector2I(x, y);
                }
            }
        }

        [Export]
        public Type type { get; set; } = Type.DEBUG;

        [Export]
        public string display_name { get; set; } = "";

        [Export]
        public int wood_cost { get; set; } = 0;

        public static string GetDefaultDisplayName(Type buildingType)
        {
            return buildingType switch
            {
                Type.HUT => "Hut",
                Type.TENT => "Tent",
                Type.WATER_BARREL => "Water Barrel",
                Type.DEBUG => "Debug Building",
                _ => buildingType.ToString(),
            };
        }

        [Export]
        public float initial_labor_strength { get; set; } = 0f;

        public ConstructionState construction_state { get; set; } = ConstructionState.UNDER_CONSTRUCTION;

        public bool is_under_construction => construction_state == ConstructionState.UNDER_CONSTRUCTION;

        public LaborCost total_labor_needed { get; set; }

        public LaborCost remaining_labor;

        public bool is_potential { get; set; } = false;

        public int hit_points { get; set; } = 1;

        private Vector2 position_offset_;

        public static int GetDefaultWoodCost(Type buildingType)
        {
            return buildingType switch
            {
                Type.HUT => 40,
                Type.TENT => 20,
                Type.WATER_BARREL => 10,
                Type.DEBUG => 5,
                _ => 10,
            };
        }

        public static LaborCost GetDefaultLaborCost(Type buildingType)
        {
            return buildingType switch
            {
                Type.HUT => new LaborCost(50f),
                Type.TENT => new LaborCost(30f),
                Type.WATER_BARREL => new LaborCost(15f),
                Type.DEBUG => new LaborCost(10f),
                _ => new LaborCost(20f),
            };
        }

        public void SetOffsetPosition(Vector2 new_position)
        {
            Position = new_position - position_offset_;
        }

        public Vector2 GetOffsetPosition()
        {
            return Position + position_offset_;
        }

        public void EnsureWrenchOverlay()
        {
            if (wrench_overlay == null)
            {
                wrench_overlay = GetNodeOrNull<Sprite2D>("WrenchOverlay");
                if (wrench_overlay == null)
                {
                    wrench_overlay = new Sprite2D
                    {
                        Name = "WrenchOverlay",
                        ZIndex = 10,
                    };
                    try
                    {
                        Texture2D wrenchTex = GD.Load<Texture2D>("res://art_placeholders/icons/wrench.svg");
                        wrench_overlay.Texture = wrenchTex;
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Failed to load wrench texture: {ex.Message}");
                    }

                    if (building_art != null)
                    {
                        wrench_overlay.Position = building_art.Position;
                    }
                    else
                    {
                        wrench_overlay.Position = position_offset_;
                    }

                    wrench_overlay.Scale = new Vector2(0.6f, 0.6f);
                    AddChild(wrench_overlay);
                }
            }
            wrench_overlay.Visible = is_under_construction;
        }

        public void ApplyLabor(float amount)
        {
            if (!is_under_construction) return;

            remaining_labor.ApplyLabor(amount);
            EmitSignal(SignalName.LaborApplied, amount, remaining_labor.labor_strength);

            if (remaining_labor.IsComplete)
            {
                FinishConstruction();
            }
        }

        public void ApplyLabor(LaborCost labor)
        {
            ApplyLabor(labor.labor_strength);
        }

        public void FinishConstruction()
        {
            construction_state = ConstructionState.BUILT;
            remaining_labor.labor_strength = 0f;
            if (wrench_overlay != null)
            {
                wrench_overlay.Visible = false;
            }
            EmitSignal(SignalName.ConstructionFinished);
        }

        public void MakeFreeCursor()
        {
            building_art.Modulate = new(0.7f, 1, 0.7f, 0.8f);
            TopLevel = true;
        }

        public void MakeBlockedCursor()
        {
            building_art.Modulate = new(1, 0.5f, 0.5f, 0.8f);
        }

        public void MakePotentialBuilding()
        {
            is_potential = true;
            construction_state = ConstructionState.UNDER_CONSTRUCTION;
            building_art.Modulate = new Color(0.75f, 0.88f, 1.0f, 0.65f);
            EnsureWrenchOverlay();
            if (wrench_overlay != null)
            {
                wrench_overlay.Visible = true;
            }
        }

        public void MakePlacedBuilding()
        {
            is_potential = false;
            building_art.Modulate = new Color(1f, 1f, 1f, 1f);
            EnsureWrenchOverlay();
            if (wrench_overlay != null)
            {
                wrench_overlay.Visible = is_under_construction;
            }
        }

        public virtual void UniqueReady() { }

        public override void _Ready()
        {
            effect_area = GetNodeOrNull<Area2D>("EffectArea");
            building_art = GetNodeOrNull<Sprite2D>("Sprite2D");

            Vector2 tile_size = new Vector2(16, 16);
            position_offset_ = (Vector2)footprint_tile_size * tile_size / 2f;

            if (string.IsNullOrEmpty(display_name))
            {
                display_name = GetDefaultDisplayName(type);
            }
            if (wood_cost <= 0)
            {
                wood_cost = GetDefaultWoodCost(type);
            }
            if (total_labor_needed.labor_strength <= 0f)
            {
                if (initial_labor_strength > 0f)
                {
                    total_labor_needed = new LaborCost(initial_labor_strength);
                }
                else
                {
                    total_labor_needed = GetDefaultLaborCost(type);
                }
                remaining_labor = total_labor_needed;
            }
            hit_points = 1;

            EnsureWrenchOverlay();

            UniqueReady();
        }
    }
}
