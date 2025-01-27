using System;
using System.Collections.Generic;
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
        }

        private PlayerData player_data_;

        [Export]
        public PackedScene debug_player_data_scene { get; set; }

        [Export]
        public PackedScene debug_game_board_scene { get; set; }

        [Export]
        public PackedScene debug_scene { get; set; }

        [Export]
        public PackedScene hut_scene { get; set; }

        [Export]
        public PackedScene tent_scene { get; set; }

        [Export]
        public PackedScene water_barrel_scene { get; set; }

        private Dictionary<Building.Type, PackedScene> building_to_scene_map_ = new();

        [Export]
        public State current_state { get; set; } = State.NONE;

        [Export]
        public Building.Type selected_building_type { get; set; } = Building.Type.DEBUG;

        public Button back_button;

        private CanvasLayer canvas_layer_;
        private GameBoard game_board_;
        private TileMapLayer build_mode_grid;
        private Vector2 tile_size_;
        private Node2D buildings_parent;

        private Sprite2D highlight_tile_;
        private bool mouse_in_area_;

        private Building building_cursor_;
        private bool can_place_building_ = true;

        public void EnterNoneState()
        {
            ExitCurrentState();
            current_state = State.NONE;
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
            current_state = State.PLACING_BUILDING;

            building_cursor_ = building_to_scene_map_[selected_building_type]
                .Instantiate<Building>();
            canvas_layer_.AddChild(building_cursor_);
            building_cursor_.MakeFreeCursor();
        }

        private void PlacingBuildingInput(InputEvent @event)
        {
            if (@event.IsActionPressed("left_click") && can_place_building_)
            {
                Building placed_building = building_cursor_;
                placed_building.MakePlacedBuilding();
                canvas_layer_.RemoveChild(building_cursor_);
                buildings_parent.AddChild(placed_building);

                foreach (
                    Vector2I footprint_cell in building_cursor_.footprint_tilemap.GetUsedCells()
                )
                {
                    Vector2I game_board_cell = building_cursor_.game_board_cell + footprint_cell;
                    game_board_.UpdateCell(game_board_cell, Block.Type.Occupied);
                }

                building_cursor_ = null;
                EnterNoneState();
            }
        }

        private void PlacingBuildingState()
        {
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
            building_cursor_.MakeFreeCursor();
            can_place_building_ = true;
            foreach (Vector2I footprint_cell in building_cursor_.footprint_tilemap.GetUsedCells())
            {
                Vector2I game_board_cell = building_cursor_.game_board_cell + footprint_cell;
                if (
                    game_board_.block_map[game_board_cell.X, game_board_cell.Y].type
                    != Block.Type.Clear
                )
                {
                    can_place_building_ = false;
                    building_cursor_.MakeBlockedCursor();
                    break;
                }
            }
        }

        private void ExitPlacingBuildingState()
        {
            if (building_cursor_ is not null)
            {
                building_cursor_.QueueFree();
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

        public override void _Ready()
        {
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

            canvas_layer_ = GetNode<CanvasLayer>("CanvasLayer");
            highlight_tile_ = GetNode<Sprite2D>("CanvasLayer/HighlightTile");
            back_button = GetNode<Button>("Control/BackButton");

            tile_size_ = build_mode_grid.TileSet.TileSize;
            highlight_tile_.Visible = false;

            building_to_scene_map_[Building.Type.DEBUG] = debug_scene;
            building_to_scene_map_[Building.Type.HUT] = hut_scene;
            building_to_scene_map_[Building.Type.TENT] = tent_scene;
            building_to_scene_map_[Building.Type.WATER_BARREL] = water_barrel_scene;

            EnterNoneState();
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

        public override void _Input(InputEvent @event)
        {
            if (current_state == State.PLACING_BUILDING)
            {
                PlacingBuildingInput(@event);
            }

            if (@event.IsActionPressed("CHANGE_BUILD_MODE_STATE"))
            {
                if (current_state == State.NONE)
                {
                    EnterPlacingBuildingState();
                }
                else if (current_state == State.NONE)
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
    }
}
