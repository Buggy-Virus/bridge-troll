using System;
using System.Collections.Generic;
using System.Linq;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class BuildMode : Node2D
    {
        public enum State
        {
            NONE,
            PLACING_BUILDING,
            DELETING_BUILDING,
        }

        public const string ActionInspectBuilding = "build_mode_inspect_building";
        public const string ActionPlaceBuilding = "build_mode_place_building";
        public const string ActionCancelPlacement = "build_mode_cancel_placement";
        public const string ActionQuickDeleteBuilding = "build_mode_quick_delete_building";
        public const string ActionDeleteModeDeleteBuilding = "build_mode_delete_mode_delete_building";

        [Signal]
        public delegate void BuildModeConfirmedEventHandler();

        [Signal]
        public delegate void BuildModeCancelledEventHandler();

        private PlayerData player_data_;

        [Export]
        public PackedScene debug_player_data_scene { get; set; }

        [Export]
        public PackedScene debug_game_board_scene { get; set; }

        // Data-driven list of available buildings
        [Export]
        public Godot.Collections.Array<BuildingData> available_buildings { get; set; } = new();

        // Legacy scene exports for backwards compatibility
        [Export]
        public PackedScene debug_scene { get; set; }

        [Export]
        public PackedScene hut_scene { get; set; }

        [Export]
        public PackedScene tent_scene { get; set; }

        [Export]
        public PackedScene water_barrel_scene { get; set; }

        private readonly Dictionary<Building.Type, PackedScene> building_to_scene_map_ = new();

        [Export]
        public State current_state { get; set; } = State.NONE;

        [Export]
        public Building.Type selected_building_type { get; set; } = Building.Type.DEBUG;

        public BuildingData selected_building_data { get; set; }

        // UI nodes
        public Button back_button;
        public Button confirm_button;
        public Label cost_counter_label;
        public HBoxContainer building_selection_bar;
        public Button delete_mode_button;

        // Building Stats Inspector UI
        public PanelContainer building_stats_panel;
        public Label stats_building_name_label;
        public Label stats_status_label;
        public VBoxContainer stats_labor_container;
        public Label stats_labor_remaining_label;
        public Button stats_close_button;

        public Building inspected_building { get; private set; }

        private readonly Dictionary<BuildingData, Button> building_button_map_ = new();

        private CanvasLayer canvas_layer_;
        private GameBoard game_board_;
        private TileMapLayer build_mode_grid;
        private Vector2 tile_size_;
        private Node2D buildings_parent;

        private Sprite2D highlight_tile_;
        private bool mouse_in_area_;

        private Building building_cursor_;
        private bool can_place_building_ = true;

        // Potential buildings placed in current build mode session
        private readonly List<Building> potential_buildings_ = new();

        public IReadOnlyList<Building> PotentialBuildings => potential_buildings_;

        public void EnterNoneState()
        {
            ExitCurrentState();
            current_state = State.NONE;
            selected_building_data = null;
            UpdateUIStyles();
        }

        private void NoneState()
        {
            if (mouse_in_area_)
            {
                highlight_tile_.Visible = true;
                highlight_tile_.Position = build_mode_grid.MapToLocal(
                    build_mode_grid.LocalToMap(GetLocalMousePosition())
                );
            }
            else
            {
                highlight_tile_.Visible = false;
            }
        }

        private void ExitNoneState()
        {
            highlight_tile_.Visible = false;
        }

        public void EnterPlacingBuildingState()
        {
            ExitCurrentState();
            CloseBuildingStatsPanel();
            current_state = State.PLACING_BUILDING;

            PackedScene sceneToInstantiate = null;
            if (selected_building_data?.BuildingScene != null)
            {
                sceneToInstantiate = selected_building_data.BuildingScene;
            }
            else if (building_to_scene_map_.ContainsKey(selected_building_type))
            {
                sceneToInstantiate = building_to_scene_map_[selected_building_type];
            }

            if (sceneToInstantiate == null)
            {
                GD.PrintErr($"No scene mapped for placing building!");
                EnterNoneState();
                return;
            }

            building_cursor_ = sceneToInstantiate.Instantiate<Building>();

            if (selected_building_data != null)
            {
                building_cursor_.type = selected_building_data.BuildingType;
                building_cursor_.display_name = selected_building_data.DisplayName;
                building_cursor_.wood_cost = selected_building_data.WoodCost;
                building_cursor_.footprint_tile_size = selected_building_data.FootprintSize;
                building_cursor_.initial_labor_strength = selected_building_data.LaborStrength;
            }

            canvas_layer_.AddChild(building_cursor_);
            building_cursor_.MakeFreeCursor();

            UpdateUIStyles();
        }

        public void EnterDeletingBuildingState()
        {
            ExitCurrentState();
            CloseBuildingStatsPanel();
            current_state = State.DELETING_BUILDING;
            selected_building_data = null;
            UpdateUIStyles();
        }

        public void ToggleDeleteMode()
        {
            if (current_state == State.DELETING_BUILDING)
            {
                EnterNoneState();
            }
            else
            {
                EnterDeletingBuildingState();
            }
        }

        public void SelectBuildingData(BuildingData data)
        {
            if (current_state == State.PLACING_BUILDING && selected_building_data == data)
            {
                EnterNoneState();
                return;
            }

            selected_building_data = data;
            selected_building_type = data.BuildingType;
            EnterPlacingBuildingState();
        }

        public void SelectBuilding(Building.Type type)
        {
            BuildingData match = available_buildings.FirstOrDefault(b => b.BuildingType == type);
            if (match != null)
            {
                SelectBuildingData(match);
                return;
            }

            if (current_state == State.PLACING_BUILDING && selected_building_type == type)
            {
                EnterNoneState();
                return;
            }

            selected_building_type = type;
            EnterPlacingBuildingState();
        }

        private void PlacingBuildingInput(InputEvent @event)
        {
            if (@event.IsActionPressed(ActionCancelPlacement))
            {
                Vector2I cell = build_mode_grid.LocalToMap(GetLocalMousePosition());
                Building pb = GetPotentialBuildingAt(cell);
                if (pb != null)
                {
                    DeletePotentialBuilding(pb);
                    return;
                }

                EnterNoneState();
                return;
            }

            if (@event.IsActionPressed(ActionQuickDeleteBuilding))
            {
                Vector2I cell = build_mode_grid.LocalToMap(GetLocalMousePosition());
                Building pb = GetPotentialBuildingAt(cell);
                if (pb != null)
                {
                    DeletePotentialBuilding(pb);
                    return;
                }
            }

            if (@event.IsActionPressed(ActionPlaceBuilding) && can_place_building_ && building_cursor_ != null)
            {
                Building placed_building = building_cursor_;
                placed_building.MakePotentialBuilding();
                placed_building.TopLevel = false;
                canvas_layer_.RemoveChild(building_cursor_);
                buildings_parent.AddChild(placed_building);

                foreach (Vector2I footprint_cell in placed_building.GetFootprintCells())
                {
                    Vector2I game_board_cell = placed_building.game_board_cell + footprint_cell;
                    game_board_.UpdateCell(game_board_cell, Block.Type.Occupied);
                }

                potential_buildings_.Add(placed_building);
                building_cursor_ = null;
                UpdateCounterUI();

                int nextCost = selected_building_data?.WoodCost ?? Building.GetDefaultWoodCost(selected_building_type);
                if (GetTotalPlannedCost() + nextCost <= GetAvailableWood())
                {
                    EnterPlacingBuildingState();
                }
                else
                {
                    EnterNoneState();
                }
            }
        }

        private void DeletingBuildingInput(InputEvent @event)
        {
            if (@event.IsActionPressed(ActionCancelPlacement))
            {
                EnterNoneState();
                return;
            }

            if (@event.IsActionPressed(ActionDeleteModeDeleteBuilding) || @event.IsActionPressed(ActionQuickDeleteBuilding))
            {
                Vector2I cell = build_mode_grid.LocalToMap(GetLocalMousePosition());
                Building pb = GetPotentialBuildingAt(cell);
                if (pb != null)
                {
                    DeletePotentialBuilding(pb);
                }
            }
        }

        private void NoneStateInput(InputEvent @event)
        {
            if (@event.IsActionPressed(ActionQuickDeleteBuilding))
            {
                Vector2I cell = build_mode_grid.LocalToMap(GetLocalMousePosition());
                Building pb = GetPotentialBuildingAt(cell);
                if (pb != null)
                {
                    DeletePotentialBuilding(pb);
                    return;
                }
            }

            if (@event.IsActionPressed(ActionInspectBuilding))
            {
                Vector2I cell = build_mode_grid.LocalToMap(GetLocalMousePosition());
                Building clickedBuilding = GetBuildingAt(cell);
                if (clickedBuilding != null)
                {
                    InspectBuilding(clickedBuilding);
                }
                else
                {
                    CloseBuildingStatsPanel();
                }
            }
        }

        private void PlacingBuildingState()
        {
            if (building_cursor_ == null) return;

            Vector2 mouse_local_position = GetLocalMousePosition();
            Vector2 current_tile_position = build_mode_grid.MapToLocal(
                build_mode_grid.LocalToMap(mouse_local_position)
            );

            Vector2I footprint_tile_size_is_even = new(
                (building_cursor_.footprint_tile_size.X + 1) % 2,
                (building_cursor_.footprint_tile_size.Y + 1) % 2
            );
            Vector2I current_tile_quandrant = new(
                Math.Sign(
                    current_tile_position.X == mouse_local_position.X
                        ? 1
                        : current_tile_position.X - mouse_local_position.X
                ),
                Math.Sign(
                    current_tile_position.Y == mouse_local_position.Y
                        ? 1
                        : current_tile_position.Y - mouse_local_position.Y
                )
            );
            Vector2 cursor_building_offset_position =
                current_tile_position
                + (-1 * footprint_tile_size_is_even * current_tile_quandrant * tile_size_ / 2);

            building_cursor_.SetOffsetPosition(cursor_building_offset_position);

            building_cursor_.game_board_cell = build_mode_grid.LocalToMap(
                building_cursor_.Position
            );

            can_place_building_ = true;

            int cost = building_cursor_.wood_cost;
            if (cost <= 0)
            {
                cost = selected_building_data?.WoodCost ?? Building.GetDefaultWoodCost(selected_building_type);
            }

            if (GetTotalPlannedCost() + cost > GetAvailableWood())
            {
                can_place_building_ = false;
            }

            if (can_place_building_)
            {
                foreach (Vector2I footprint_cell in building_cursor_.GetFootprintCells())
                {
                    Vector2I game_board_cell = building_cursor_.game_board_cell + footprint_cell;
                    if (
                        game_board_cell.X < 0
                        || game_board_cell.Y < 0
                        || game_board_cell.X >= game_board_.block_map.GetLength(0)
                        || game_board_cell.Y >= game_board_.block_map.GetLength(1)
                        || game_board_.block_map[game_board_cell.X, game_board_cell.Y].type != Block.Type.Clear
                    )
                    {
                        can_place_building_ = false;
                        break;
                    }
                }
            }

            if (can_place_building_)
            {
                building_cursor_.MakeFreeCursor();
            }
            else
            {
                building_cursor_.MakeBlockedCursor();
            }
        }

        private void ExitPlacingBuildingState()
        {
            if (building_cursor_ is not null)
            {
                building_cursor_.QueueFree();
                building_cursor_ = null;
            }
        }

        private void ExitCurrentState()
        {
            if (current_state == State.NONE)
            {
                ExitNoneState();
            }
            else if (current_state == State.PLACING_BUILDING)
            {
                ExitPlacingBuildingState();
            }
        }

        public Building GetBuildingAt(Vector2I cell)
        {
            if (buildings_parent == null) return null;

            foreach (Node child in buildings_parent.GetChildren())
            {
                if (child is Building building && GodotObject.IsInstanceValid(building))
                {
                    Vector2I localCell = cell - building.game_board_cell;
                    if (building.GetFootprintCells().Contains(localCell))
                    {
                        return building;
                    }
                }
            }
            return null;
        }

        public Building GetPotentialBuildingAt(Vector2I cell)
        {
            foreach (Building pb in potential_buildings_)
            {
                if (!GodotObject.IsInstanceValid(pb)) continue;
                Vector2I local_cell = cell - pb.game_board_cell;
                if (pb.GetFootprintCells().Contains(local_cell))
                {
                    return pb;
                }
            }
            return null;
        }

        public void InspectBuilding(Building building)
        {
            if (building == null || !GodotObject.IsInstanceValid(building))
            {
                CloseBuildingStatsPanel();
                return;
            }

            inspected_building = building;

            if (building_stats_panel != null)
            {
                building_stats_panel.Visible = true;
            }

            string displayName = !string.IsNullOrEmpty(building.display_name)
                ? building.display_name
                : Building.GetDefaultDisplayName(building.type);

            if (stats_building_name_label != null)
            {
                stats_building_name_label.Text = displayName;
            }

            if (building.is_under_construction)
            {
                if (stats_status_label != null)
                {
                    stats_status_label.Visible = true;
                    stats_status_label.Text = building.is_potential
                        ? "Status: Planned Construction"
                        : "Status: Under Construction";
                }

                if (stats_labor_container != null)
                {
                    stats_labor_container.Visible = true;
                }

                if (stats_labor_remaining_label != null)
                {
                    stats_labor_remaining_label.Text = $"Remaining Labor: {building.remaining_labor.labor_strength:F0} / {building.total_labor_needed.labor_strength:F0}";
                }
            }
            else
            {
                if (stats_status_label != null)
                {
                    stats_status_label.Visible = false;
                }

                if (stats_labor_container != null)
                {
                    stats_labor_container.Visible = false;
                }
            }
        }

        public void CloseBuildingStatsPanel()
        {
            inspected_building = null;
            if (building_stats_panel != null)
            {
                building_stats_panel.Visible = false;
            }
        }

        public bool DeletePotentialBuilding(Building building)
        {
            if (building == null || !potential_buildings_.Contains(building))
            {
                return false;
            }

            if (inspected_building == building)
            {
                CloseBuildingStatsPanel();
            }

            potential_buildings_.Remove(building);

            if (game_board_ != null)
            {
                foreach (Vector2I footprint_cell in building.GetFootprintCells())
                {
                    Vector2I game_board_cell = building.game_board_cell + footprint_cell;
                    game_board_.UpdateCell(game_board_cell, Block.Type.Clear);
                }
            }

            if (building.GetParent() != null)
            {
                building.GetParent().RemoveChild(building);
            }
            building.QueueFree();

            UpdateCounterUI();
            return true;
        }

        public int GetTotalPlannedCost()
        {
            int total = 0;
            foreach (Building pb in potential_buildings_)
            {
                if (GodotObject.IsInstanceValid(pb))
                {
                    total += pb.wood_cost;
                }
            }
            return total;
        }

        public int GetAvailableWood()
        {
            if (player_data_ == null) return 0;
            player_data_.EnsureInventory();
            return player_data_.inventory.GetWood();
        }

        public void UpdateCounterUI()
        {
            if (cost_counter_label == null) return;

            int stash = GetAvailableWood();
            int plannedCost = GetTotalPlannedCost();
            int remaining = stash - plannedCost;

            cost_counter_label.Text = $"Wood Stash: {stash}  |  Planned Cost: {plannedCost}  |  Remaining After: {remaining}";

            if (remaining < 0)
            {
                cost_counter_label.Modulate = new Color(1f, 0.35f, 0.35f, 1f);
                if (confirm_button != null) confirm_button.Disabled = true;
            }
            else
            {
                cost_counter_label.Modulate = new Color(1f, 1f, 1f, 1f);
                if (confirm_button != null) confirm_button.Disabled = false;
            }
        }

        private void UpdateUIStyles()
        {
            foreach (var kvp in building_button_map_)
            {
                BuildingData data = kvp.Key;
                Button btn = kvp.Value;
                if (btn == null) continue;

                if (current_state == State.PLACING_BUILDING && selected_building_data == data)
                {
                    btn.Text = $"> {data.DisplayName} <\n({data.WoodCost} Wood)";
                }
                else
                {
                    btn.Text = $"{data.DisplayName}\n({data.WoodCost} Wood)";
                }
            }

            if (delete_mode_button != null)
            {
                delete_mode_button.Text = (current_state == State.DELETING_BUILDING)
                    ? "[Delete Active]"
                    : "Delete Mode";
            }
        }

        public void ConfirmBuild()
        {
            int plannedCost = GetTotalPlannedCost();
            int stash = GetAvailableWood();
            if (plannedCost > stash)
            {
                GD.PrintErr("Cannot confirm construction: not enough wood!");
                return;
            }

            if (player_data_ != null && plannedCost > 0)
            {
                player_data_.inventory.RemoveWood(plannedCost);
            }

            foreach (Building pb in potential_buildings_)
            {
                if (GodotObject.IsInstanceValid(pb))
                {
                    pb.MakePlacedBuilding();
                }
            }

            potential_buildings_.Clear();
            CloseBuildingStatsPanel();
            EnterNoneState();

            EmitSignal(SignalName.BuildModeConfirmed);
        }

        public void CancelBuild()
        {
            foreach (Building pb in potential_buildings_)
            {
                if (GodotObject.IsInstanceValid(pb))
                {
                    if (game_board_ != null)
                    {
                        foreach (Vector2I footprint_cell in pb.GetFootprintCells())
                        {
                            Vector2I game_board_cell = pb.game_board_cell + footprint_cell;
                            game_board_.UpdateCell(game_board_cell, Block.Type.Clear);
                        }
                    }

                    if (pb.GetParent() != null)
                    {
                        pb.GetParent().RemoveChild(pb);
                    }
                    pb.QueueFree();
                }
            }

            potential_buildings_.Clear();
            CloseBuildingStatsPanel();
            EnterNoneState();

            EmitSignal(SignalName.BuildModeCancelled);
        }

        public void OnEnterBuildMode()
        {
            if (player_data_ != null)
            {
                player_data_.EnsureInventory();
            }

            SetupDynamicBuildingButtons();
            potential_buildings_.Clear();
            CloseBuildingStatsPanel();
            EnterNoneState();
            UpdateCounterUI();
        }

        private void EnsureInputActions()
        {
            EnsureActionWithMouseButton(ActionInspectBuilding, MouseButton.Left);
            EnsureActionWithMouseButton(ActionPlaceBuilding, MouseButton.Left);
            EnsureActionWithMouseButton(ActionCancelPlacement, MouseButton.Right);
            EnsureActionWithMouseButton(ActionQuickDeleteBuilding, MouseButton.Right);
            EnsureActionWithMouseButton(ActionDeleteModeDeleteBuilding, MouseButton.Left);
        }

        private void EnsureActionWithMouseButton(string actionName, MouseButton mouseButton)
        {
            if (!InputMap.HasAction(actionName))
            {
                InputMap.AddAction(actionName);
                InputEventMouseButton ev = new()
                {
                    ButtonIndex = mouseButton,
                    Pressed = true,
                };
                InputMap.ActionAddEvent(actionName, ev);
            }
        }

        private void BuildDefaultBuildingDataList()
        {
            if (available_buildings.Count == 0)
            {
                BuildingData hut = ResourceLoader.Load<BuildingData>("res://buildings/resources/hut.tres");
                if (hut != null) available_buildings.Add(hut);

                BuildingData tent = ResourceLoader.Load<BuildingData>("res://buildings/resources/tent.tres");
                if (tent != null) available_buildings.Add(tent);

                BuildingData barrel = ResourceLoader.Load<BuildingData>("res://buildings/resources/water_barrel.tres");
                if (barrel != null) available_buildings.Add(barrel);

                BuildingData debug = ResourceLoader.Load<BuildingData>("res://buildings/resources/debug_building.tres");
                if (debug != null) available_buildings.Add(debug);
            }
        }

        private void SetupDynamicBuildingButtons()
        {
            if (building_selection_bar == null) return;

            // Remove any legacy static building buttons from the container except DeleteModeButton
            foreach (Node child in building_selection_bar.GetChildren())
            {
                if (child != delete_mode_button)
                {
                    building_selection_bar.RemoveChild(child);
                    child.QueueFree();
                }
            }

            building_button_map_.Clear();

            // Auto-generate a button for each unlocked BuildingData in available_buildings
            foreach (BuildingData data in available_buildings)
            {
                if (player_data_ != null && !player_data_.IsBuildingUnlocked(data))
                {
                    continue;
                }

                BuildingData capture = data;
                Button btn = new Button
                {
                    Name = $"{capture.Id}Button",
                    CustomMinimumSize = new Vector2(160, 60),
                    Text = $"{capture.DisplayName}\n({capture.WoodCost} Wood)",
                };
                btn.Pressed += () => SelectBuildingData(capture);
                building_selection_bar.AddChild(btn);
                building_button_map_[capture] = btn;
            }

            // Ensure DeleteModeButton is placed at the end of the bar
            if (delete_mode_button != null)
            {
                building_selection_bar.MoveChild(delete_mode_button, -1);
            }
        }

        public override void _Ready()
        {
            EnsureInputActions();

            Node parent = GetParent();
            if (parent is Main)
            {
                player_data_ = parent.GetNode<PlayerData>("PlayerData");
                game_board_ = parent.GetNode<GameBoard>("GameBoard");
                build_mode_grid = game_board_.build_mode_grid;
                buildings_parent = game_board_.buildings_parent;
            }
            else
            {
                player_data_ = debug_player_data_scene.Instantiate<PlayerData>();
                AddChild(player_data_);
                game_board_ = debug_game_board_scene.Instantiate<GameBoard>();
                AddChild(game_board_);
                build_mode_grid = game_board_.build_mode_grid;
                buildings_parent = game_board_.build_mode_grid;
            }

            if (player_data_ != null)
            {
                player_data_.EnsureInventory();
            }

            canvas_layer_ = GetNode<CanvasLayer>("CanvasLayer");
            highlight_tile_ = GetNode<Sprite2D>("CanvasLayer/HighlightTile");

            back_button = GetNodeOrNull<Button>("CanvasLayer/Control/BackButton");
            confirm_button = GetNodeOrNull<Button>("CanvasLayer/Control/ConfirmButton");
            cost_counter_label = GetNodeOrNull<Label>("CanvasLayer/Control/CostCounterPanel/MarginContainer/CostCounterLabel");
            building_selection_bar = GetNodeOrNull<HBoxContainer>("CanvasLayer/Control/BuildingSelectionBar");
            delete_mode_button = GetNodeOrNull<Button>("CanvasLayer/Control/BuildingSelectionBar/DeleteModeButton");

            building_stats_panel = GetNodeOrNull<PanelContainer>("CanvasLayer/Control/BuildingStatsPanel");
            if (building_stats_panel != null)
            {
                stats_close_button = building_stats_panel.GetNodeOrNull<Button>("MarginContainer/VBoxContainer/HeaderHBox/CloseButton");
                stats_building_name_label = building_stats_panel.GetNodeOrNull<Label>("MarginContainer/VBoxContainer/BuildingNameLabel");
                stats_status_label = building_stats_panel.GetNodeOrNull<Label>("MarginContainer/VBoxContainer/StatusLabel");
                stats_labor_container = building_stats_panel.GetNodeOrNull<VBoxContainer>("MarginContainer/VBoxContainer/LaborContainer");
                if (stats_labor_container != null)
                {
                    stats_labor_remaining_label = stats_labor_container.GetNodeOrNull<Label>("LaborRemainingLabel");
                }

                if (stats_close_button != null)
                {
                    stats_close_button.Pressed += CloseBuildingStatsPanel;
                }
            }

            if (back_button != null) back_button.Pressed += CancelBuild;
            if (confirm_button != null) confirm_button.Pressed += ConfirmBuild;
            if (delete_mode_button != null) delete_mode_button.Pressed += ToggleDeleteMode;

            tile_size_ = build_mode_grid.TileSet.TileSize;
            highlight_tile_.Visible = false;

            // Load and build data-driven list of buildings
            BuildDefaultBuildingDataList();

            // Populate legacy map for backwards compatibility
            building_to_scene_map_[Building.Type.DEBUG] = debug_scene;
            building_to_scene_map_[Building.Type.HUT] = hut_scene;
            building_to_scene_map_[Building.Type.TENT] = tent_scene;
            building_to_scene_map_[Building.Type.WATER_BARREL] = water_barrel_scene;

            // Auto-generate UI palette buttons dynamically
            SetupDynamicBuildingButtons();

            EnterNoneState();
            UpdateCounterUI();
        }

        public override void _Process(double delta)
        {
            if (current_state == State.NONE)
            {
                NoneState();
            }
            else if (current_state == State.PLACING_BUILDING)
            {
                PlacingBuildingState();
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (current_state == State.PLACING_BUILDING)
            {
                PlacingBuildingInput(@event);
            }
            else if (current_state == State.DELETING_BUILDING)
            {
                DeletingBuildingInput(@event);
            }
            else if (current_state == State.NONE)
            {
                NoneStateInput(@event);
            }

            if (@event.IsActionPressed("CHANGE_BUILD_MODE_STATE"))
            {
                if (current_state == State.NONE)
                {
                    EnterPlacingBuildingState();
                }
                else
                {
                    EnterNoneState();
                }
            }
        }

        public void _on_build_area_mouse_entered()
        {
            mouse_in_area_ = true;
        }

        public void _on_build_area_mouse_exited()
        {
            mouse_in_area_ = false;
        }

        public void _on_build_area_area_entered(Area2D area) { }

        public void _on_build_area_area_exited(Area2D area) { }
    }
}
