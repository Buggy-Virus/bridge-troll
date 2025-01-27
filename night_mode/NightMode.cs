using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class NightMode : Node2D
    {
        private PlayerData player_data_;

        [Export]
        public PackedScene debug_player_data_scene { get; set; }

        private Control inventory_interface_;
        private Control troll_stats_interface_;
        private Control gear_interface_;

        public Button next_day_button;
        public Button build_mode_button;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Node parent = GetParent();
            if (parent is Main)
            {
                player_data_ = parent.GetNode<PlayerData>("PlayerData");
            }
            else
            {
                player_data_ = debug_player_data_scene.Instantiate<PlayerData>();
                AddChild(player_data_);
            }

            inventory_interface_ = GetNode<Control>("Control/InventoryInterface");
            troll_stats_interface_ = GetNode<Control>("Control/TrollStatsInterface");
            gear_interface_ = GetNode<Control>("Control/GearInterface");
            next_day_button = GetNode<Button>("Control/NextDayButton");
            build_mode_button = GetNode<Button>("Control/BuildModeButton");

            CloseControl(inventory_interface_);
            CloseControl(troll_stats_interface_);
            CloseControl(gear_interface_);
        }

        private void OpenControl(Control control)
        {
            control.Visible = true;
            control.SetProcessInput(true);
            control.SetProcess(true);
        }

        private void CloseControl(Control control)
        {
            control.Visible = false;
            control.SetProcessInput(false);
            control.SetProcess(false);
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }
    }
}
