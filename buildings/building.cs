using System;
using System.Collections.Generic;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class Building : Node2D
    {
        public enum Type {
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

        public Area2D effect_area;
        public Area2D selection_area;
        public TileMapLayer footprint_tilemap;
        public Sprite2D building_art;

        public Vector2I footprint_tile_size;

        public Vector2I game_board_cell;

        public Type type;
        public int hit_points;

        private Vector2 position_offset_;

        public void SetOffsetPosition(Vector2 new_position) {
            Position = new_position - position_offset_;
        }

        public Vector2 GetOffsetPosition() {
            return Position + position_offset_;
        }

        public void MakeFreeCursor() {
            building_art.Modulate = new(0.7f, 1, 0.7f, 1);
            TopLevel = true;
        }

        public void MakeBlockedCursor() {
            building_art.Modulate = new(1, 0.7f, 0.7f, 1);
        }

        public void MakePlacedBuilding() {
            building_art.Modulate = new(1, 1f, 1f, 1);
        }

        public virtual void UniqueReady() { }

        public override void _Ready() {
            effect_area = GetNode<Area2D>("EffectArea");
            selection_area = GetNode<Area2D>("SelectionArea");
            footprint_tilemap = GetNode<TileMapLayer>("TileMapLayer");
            building_art = GetNode<Sprite2D>("Sprite2D");

            foreach (Vector2I cell in footprint_tilemap.GetUsedCells()) {
                if (cell.X + 1 > footprint_tile_size.X) {
                    footprint_tile_size.X = cell.X + 1;
                }

                if (cell.Y + 1 > footprint_tile_size.Y) {
                    footprint_tile_size.Y = cell.Y + 1;
                }
            }

            Vector2 tile_size = footprint_tilemap.TileSet.TileSize;
            position_offset_ = footprint_tile_size * tile_size / 2;

            type = Type.DEBUG;
            hit_points = 1;

            UniqueReady();
        }
    }
}
