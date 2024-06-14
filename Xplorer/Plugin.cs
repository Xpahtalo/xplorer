using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using XivCommon;
using Xplorer.AddonExplore;
using Xplorer.Titler;
using Xplorer.TravelerHide;

namespace Xplorer;

public sealed class Plugin : IDalamudPlugin {
    // Dalamud
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly Configuration          _configuration;
    private readonly IAddonLifecycle        _addonLifecycle;
    private readonly IClientState           _clientState;
    private readonly ICommandManager        _commandManager;
    private readonly IFramework             _framework;
    private readonly IObjectTable           _objectTable;
    private readonly IPluginLog             _pluginLog;
    private readonly WindowSystem           _windowSystem;

    // Other Services
    private readonly XivCommonBase  _common;
    private readonly CommandHandler _commandHandler;

    // Cores
    private readonly AddonExploreCore _addonExplore;
    private readonly TitlerCore       _titler;
    private readonly TravelerHideCore _travelerHide;

    public Plugin(
        DalamudPluginInterface pluginInterface,
        IAddonLifecycle        addonLifecycle,
        IClientState           clientState,
        ICommandManager        commandManager,
        IFramework             framework,
        IObjectTable           objectTable,
        IPluginLog             pluginLog
    ) {
        _pluginInterface = pluginInterface;
        _addonLifecycle  = addonLifecycle;
        _clientState     = clientState;
        _commandManager  = commandManager;
        _framework       = framework;
        _objectTable     = objectTable;
        _pluginLog       = pluginLog;

        _common         = new XivCommonBase(_pluginInterface, Hooks.None);
        _windowSystem   = new WindowSystem("Xplorer");
        _commandHandler = new CommandHandler(commandManager, pluginLog);

        _configuration = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        _configuration.Initialize(_pluginInterface);

        _pluginInterface.UiBuilder.Draw += DrawUi;

        _addonExplore = new AddonExploreCore(_addonLifecycle, _pluginLog, _framework);
        _addonExplore.RegisterSelf(_windowSystem, _commandHandler);

        _titler = new TitlerCore(_clientState, _commandManager, _framework, _pluginLog, _common);

        _travelerHide = new TravelerHideCore(_framework, _clientState, _objectTable);
        _travelerHide.RegisterSelf(_windowSystem, _commandHandler);
    }

    public void Dispose() {
        _pluginInterface.UiBuilder.Draw -= DrawUi;

        _addonExplore.Dispose();
        _titler.Dispose();
        _travelerHide.Dispose();

        _commandHandler.Dispose();
    }

    private void DrawUi() {
        _windowSystem.Draw();
    }
}