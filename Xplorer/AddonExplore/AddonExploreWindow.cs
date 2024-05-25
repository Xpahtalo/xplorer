using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Xplorer.AddonExplore;

internal class AddonExploreWindow : Window {
    private readonly AddonExploreCore _core;

    private string BattleTalkName       => AddonExploreCore.BattleTalkName;
    private bool   BattleTalkRegistered => _core.BattleTalkRegistered;

    public AddonExploreWindow(AddonExploreCore core) : base("Addon Explorer") {
        _core = core;
    }

    public override void Draw() {
        if (BattleTalkRegistered) {
            if (ImGui.Button($"Unhook {BattleTalkName}")) {
                _core.UnregisterBattleTalk();
            }
        } else {
            if (ImGui.Button($"Hook {BattleTalkName}")) {
                _core.RegisterBattleTalk();
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear Event List")) {
            _core.BattleTalkEvents.Clear();
        }

        ImGui.SameLine();
        if (_core.BlockBattleTalk) {
            if (ImGui.Button("Unblock Talk")) {
                _core.BlockBattleTalk = false;
            }
        } else {
            if (ImGui.Button("Block Talk")) {
                _core.BlockBattleTalk = true;
            }
        }

        var i = 0;
        foreach (var talkEvent in _core.BattleTalkEvents) {
            ImGui.TextColored(new Vector4(0, 1, 0, 1), $"{i++}   ");
            ImGui.SameLine();
            ImGuiHelpers.SafeTextWrapped(talkEvent);
        }
    }
}