using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Xplorer.AddonExplore;
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

        _configuration = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        _configuration.Initialize(_pluginInterface);

        _pluginInterface.UiBuilder.Draw += DrawUi;

        _travelerHide = new TravelerHideCore(_framework, _clientState, _objectTable);
        _travelerHide.RegisterSelf(_windowSystem, _commandHandler);

        _addonExplore = new AddonExploreCore(_addonLifecycle, _pluginLog, _framework);
        _addonExplore.RegisterSelf(_windowSystem, _commandHandler);
    }

    public void Dispose() {
        _pluginInterface.UiBuilder.Draw -= DrawUi;

        _travelerHide.Dispose();
        _addonExplore.Dispose();

        _commandHandler.Dispose();
    }

    private void DrawUi() {
        _windowSystem.Draw();
    }
}