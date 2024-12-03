using System;
using BridgeTroll;
using Godot;

public partial class BuildingGrid : Node2D
{
	[Export]
	public PackedScene building_1 { get; set; }

	[Export]
	public PackedScene building_2 { get; set; }

	private PackedScene _selected_building;

	[Export]
	public bool build_mode = false;

	[Export]
	public int building_num = 0;

	private TileMapLayer _placing_grid;
	private TileMapLayer _built_grid;

	private Sprite2D _highlight_tile;

	private bool _mouse_in_area;
	private Area2D _current_building_cursor;
	private Sprite2D _current_building_cursor_sprite;
	private Vector2I _current_building_tile_size;
	private Vector2 _tile_size;

	private bool _can_place = true;

	private void HighlightTileOnMouseOver()
	{
		if (!build_mode && _mouse_in_area)
		{
			Vector2 moused_over_tile_position = _placing_grid.MapToLocal(
				_placing_grid.LocalToMap(GetLocalMousePosition())
			);

			_highlight_tile.Position = moused_over_tile_position;
		}
	}

	private Vector2 GetBuildingPosition()
	{
		Vector2 mouse_local_position = GetLocalMousePosition();
		Vector2 moused_over_tile_position = _placing_grid.MapToLocal(
			_placing_grid.LocalToMap(mouse_local_position)
		);

		Vector2 building_position = moused_over_tile_position;
		building_position.X -=
			(-1 * (_current_building_tile_size.X % 2) + 1)
			* Math.Sign(moused_over_tile_position.X - mouse_local_position.X)
			* _tile_size.X
			/ 2;
		building_position.Y -=
			(-1 * (_current_building_tile_size.Y % 2) + 1)
			* Math.Sign(moused_over_tile_position.Y - mouse_local_position.Y)
			* _tile_size.Y
			/ 2;

		return building_position;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_placing_grid = GetNode<TileMapLayer>("PlacingGrid");
		_built_grid = GetNode<TileMapLayer>("BuiltGrid");
		_highlight_tile = GetNode<Sprite2D>("HighlightTile");

		_tile_size = _placing_grid.TileSet.TileSize;

		if (build_mode)
		{
			if (building_num == 0)
			{
				_selected_building = building_1;
			}
			else
			{
				_selected_building = building_2;
			}
			_current_building_cursor = _selected_building.Instantiate<Area2D>();
			GetNode<CanvasLayer>("CanvasLayer").AddChild(_current_building_cursor);

			_current_building_cursor_sprite = _current_building_cursor.GetNode<Sprite2D>(
				"Sprite2D"
			);
			_current_building_cursor_sprite.Modulate = new(0.7f, 1, 0.7f, 1);
			_current_building_cursor.TopLevel = true;

			Vector2 current_building_collision_size = _current_building_cursor
				.GetNode<CollisionShape2D>("CollisionShape2D")
				.Shape.GetRect()
				.Size;

			_current_building_tile_size.X = (int)(current_building_collision_size.X / _tile_size.X);
			_current_building_tile_size.Y = (int)(current_building_collision_size.Y / _tile_size.Y);

			Vector2 altered_collision_size = current_building_collision_size;
			altered_collision_size -= new Vector2(2, 2);

			// Shape2D altered_collision_shape = new RectangleShape2D();
			(
				(RectangleShape2D)
					_current_building_cursor.GetNode<CollisionShape2D>("CollisionShape2D").Shape
			).Size = altered_collision_size;
			GD.Print(
				_current_building_cursor
					.GetNode<CollisionShape2D>("CollisionShape2D")
					.Shape.GetRect()
					.Size
			);
		}
		else
		{
			_highlight_tile.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		if (build_mode)
		{
			_current_building_cursor.Position = GetBuildingPosition();
			_current_building_cursor_sprite.Modulate = new(0.7f, 1, 0.7f, 1);
			_can_place = true;
			foreach (Area2D overlapping_area in _current_building_cursor.GetOverlappingAreas())
			{
				if (overlapping_area.IsInGroup("Buildings"))
				{
					_can_place = false;
					_current_building_cursor_sprite.Modulate = new(1, 0.7f, 0.7f, 1);
					break;
				}
			}
		}
		else
		{
			HighlightTileOnMouseOver();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("left_click") && build_mode && _can_place)
		{
			Area2D placed_building = _selected_building.Instantiate<Area2D>();
			placed_building.Position = GetBuildingPosition();
			AddChild(placed_building);
		}
	}

	public void _on_build_area_mouse_entered()
	{
		_mouse_in_area = true;

		if (build_mode)
		{
			_current_building_cursor.Visible = true;
		}
		else
		{
			_highlight_tile.Visible = true;
		}
	}

	public void _on_build_area_mouse_exited()
	{
		if (build_mode)
		{
			_current_building_cursor.Visible = false;
		}
		else
		{
			_highlight_tile.Visible = false;
		}
	}
}
