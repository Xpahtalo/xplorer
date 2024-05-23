using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;


namespace Xplorer.TravelerHide;

internal sealed class TravelerHideCore : IDisposable {
    private readonly IFramework          _framework;
    private readonly IClientState        _clientState;
    private readonly IObjectTable        _objectTable;
    private readonly TravelerHideWindow  _mainWindow;
    private readonly List<SimpleCommand> _commands;

    internal bool HideTravelers;

    internal TravelerHideCore(IFramework framework, IClientState clientState, IObjectTable objectTable) {
        _framework   = framework;
        _clientState = clientState;
        _objectTable = objectTable;
        _mainWindow  = new TravelerHideWindow(this);
        _commands = new List<SimpleCommand> {
            new("/thide", "Toggle the Traveler Hide window.", true, _mainWindow.Toggle),
        };

        _framework.Update             += OnFramework;
        _clientState.TerritoryChanged += OnTerritoryChanged;
    }

    internal void RegisterSelf(WindowSystem windowSystem, CommandHandler commandHandler) {
        windowSystem.AddWindow(_mainWindow);
        commandHandler.RegisterCommands(_commands);
    }

    internal void UnregisterSelf(WindowSystem windowSystem, CommandHandler commandHandler) {
        windowSystem.RemoveWindow(_mainWindow);
        commandHandler.UnregisterCommands(_commands);
    }

    internal void StartHiding() {
        HideTravelers = true;
    }

    internal void StopHiding() {
        HideTravelers = false;
        _framework.RunOnTick(ResetVisibility);
    }

    private void ResetVisibility() {
        SetTravellerVisibility(true);
    }

    private unsafe void SetTravellerVisibility(bool visible) {
        foreach (var gameObject in _objectTable.Where(o => o.IsValid() && o.ObjectKind == ObjectKind.Player)) {
            if (gameObject is not PlayerCharacter character ||
                character.HomeWorld.Id == _clientState.LocalPlayer?.CurrentWorld.Id) {
                continue;
            }

            var player = (GameObject*)character.Address;
            var flags  = visible ? (int)VisibilityFlags.None : (int)VisibilityFlags.Invisible;
            player->RenderFlags = flags;
        }
    }

    private void OnFramework(IFramework framework) {
        if (!HideTravelers) {
            return;
        }

        SetTravellerVisibility(false);
    }

    private void OnTerritoryChanged(ushort territoryId) {
        _mainWindow.CurrentWorld = _clientState.LocalPlayer?.CurrentWorld.GameData?.Name ?? "Unknown";
    }

    public void Dispose() {
        _framework.Update -= OnFramework;
    }
}