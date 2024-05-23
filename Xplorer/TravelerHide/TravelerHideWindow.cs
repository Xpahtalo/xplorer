using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;

namespace Xplorer.TravelerHide;

internal sealed class TravelerHideWindow : Window {
    private readonly TravelerHideCore _core;

    internal string CurrentWorld { get; set; }

    internal TravelerHideWindow(TravelerHideCore core)
        : base("Traveler Hide") {
        _core        = core;
        CurrentWorld = "";
    }

    public override void Update() {
        base.Update();
        if (CurrentWorld.IsNullOrWhitespace()) {
            _core.RequestWorldUpdate();
        }
    }

    public override void Draw() {
        if (!_core.HideTravelers) {
            if (ImGui.Button(
                    $"Hide anyone not from {(!CurrentWorld.IsNullOrWhitespace() ? CurrentWorld : "Unknown")}")) {
                _core.StartHiding();
            }
        } else {
            if (ImGui.Button("Show all players")) {
                _core.StopHiding();
            }
        }
    }
}