using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class GameBoard : Node2D
    {
        // TODO: these shouldn't be hard coded
        private int height = 68;
        private int width = 120;
        private int bridge_start_row = 20;
        private int bridge_end_row = 40;
        private Vector2I clear_tile = new(0, 8);
        private Vector2I occupied_tile = new(0, 9);
        private Vector2I random_bridge_tile_range = new(9, 8);
        private Vector2I random_side_tile_range = new(4, 2);

        public TileMapLayer mob_navigation_layer;
        public TileMapLayer build_mode_grid;
        public Node2D buildings_parent;
        public TileMapLayer bridge_tile_map;
        public TileMapLayer side_tile_map;

        public Block[,] block_map;

        public void UpdateNavigationCell(int x, int y)
        {
            if (block_map[x, y].type == Block.Type.Clear)
            {
                mob_navigation_layer.SetCell(new(x, y), 0, clear_tile, 0);
            }
            else
            {
                mob_navigation_layer.SetCell(new(x, y), 0, occupied_tile, 0);
            }
        }

        public void UpdateNavigationCell(Vector2I cell)
        {
            if (block_map[cell.X, cell.Y].type == Block.Type.Clear)
            {
                mob_navigation_layer.SetCell(cell, 0, clear_tile, 0);
            }
            else
            {
                mob_navigation_layer.SetCell(cell, 0, occupied_tile, 0);
            }
        }

        public void UpdateCell(Vector2I cell, Block.Type new_block_type)
        {
            block_map[cell.X, cell.Y].type = new_block_type;
            UpdateNavigationCell(cell);
        }

        public void UpdateNavigationLayer()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    UpdateNavigationCell(x, y);
                }
            }
        }

        public void RandomizeTiles(TileMapLayer tile_map, Vector2I random_tile_range)
        {
            foreach (Vector2I cell in tile_map.GetUsedCells())
            {
                tile_map.SetCell(
                    cell,
                    0,
                    new Vector2I(
                        (int)(GD.Randi() % random_tile_range.X),
                        (int)(GD.Randi() % random_tile_range.Y)
                    ),
                    0
                );
            }
        }

        public override void _Ready()
        {
            mob_navigation_layer = GetNode<TileMapLayer>("MobNavigationLayer");
            build_mode_grid = GetNode<TileMapLayer>("BuildModeGrid");
            buildings_parent = GetNode<Node2D>("Buildings");
            bridge_tile_map = GetNode<TileMapLayer>("BridgeTiles");
            side_tile_map = GetNode<TileMapLayer>("SideTiles");

            block_map = new Block[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    block_map[i, j] = new Block();
                    if (j >= bridge_start_row && j < bridge_end_row)
                    {
                        block_map[i, j].type = Block.Type.Clear;
                    }
                }
            }

            RandomizeTiles(bridge_tile_map, random_bridge_tile_range);
            RandomizeTiles(side_tile_map, random_side_tile_range);

            UpdateNavigationLayer();
        }
    }
}
