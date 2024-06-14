using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using XivCommon;
using XivCommon.Functions;
using Xplorer.AddonExplore;
using Xplorer.Titler;
using Xplorer.TravelerHide;

namespace Xplorer;

public sealed class Plugin : IDalamudPlugin {
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly IFramework             _framework;
    private readonly IClientState           _clientState;
    private readonly IObjectTable           _objectTable;
    private readonly IAddonLifecycle        _addonLifecycle;
    private readonly IPluginLog             _pluginLog;
    private readonly Configuration          _configuration;
    private readonly CommandHandler         _commandHandler;
    private readonly XivCommonBase  _common;

    private readonly WindowSystem _windowSystem = new("Xplorer");

    private readonly TravelerHideCore _travelerHide;
    private readonly AddonExploreCore _addonExplore;

    public Plugin(
        DalamudPluginInterface pluginInterface,
        ICommandManager        commandManager,
        IFramework             framework,
        IClientState           clientState,
        IObjectTable           objectTable,
        IPluginLog             pluginLog,
        IAddonLifecycle        addonLifecycle) {
        _pluginInterface = pluginInterface;
        _framework       = framework;
        _clientState     = clientState;
        _objectTable     = objectTable;
        _pluginLog       = pluginLog;
        _addonLifecycle  = addonLifecycle;
        _commandHandler  = new CommandHandler(commandManager, pluginLog);
        _common         = new XivCommonBase(_pluginInterface, Hooks.None);

        _configuration = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        _configuration.Initialize(_pluginInterface);

        _pluginInterface.UiBuilder.Draw += DrawUi;
        _titler = new TitlerCore(_clientState, _commandManager, _framework, _pluginLog, _common);

        _travelerHide = new TravelerHideCore(_framework, _clientState, _objectTable);
        _travelerHide.RegisterSelf(_windowSystem, _commandHandler);

        _addonExplore = new AddonExploreCore(_addonLifecycle, _pluginLog, _framework);
        _addonExplore.RegisterSelf(_windowSystem, _commandHandler);
    }

    public void Dispose() {
        _pluginInterface.UiBuilder.Draw -= DrawUi;

        _travelerHide.Dispose();
        _addonExplore.Dispose();
        _titler.Dispose();
        _travelerHide.Dispose();

        _commandHandler.Dispose();
    }

    private void DrawUi() {
        _windowSystem.Draw();
    }
}