using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Xplorer.TravelerHide;

namespace Xplorer;

public sealed class Plugin : IDalamudPlugin {
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly IFramework             _framework;
    private readonly IClientState           _clientState;
    private readonly IObjectTable           _objectTable;
    private readonly Configuration          _configuration;
    private readonly CommandHandler         _commandHandler;
    
    private readonly WindowSystem _windowSystem = new("Xplorer");

    private readonly TravelerHideCore _travelerHide;

    public Plugin(
        DalamudPluginInterface pluginInterface,
        ICommandManager        commandManager,
        IFramework             framework,
        IClientState           clientState,
        IObjectTable           objectTable,
        IPluginLog pluginLog)
    {
        _pluginInterface = pluginInterface;
        _framework       = framework;
        _clientState     = clientState;
        _objectTable     = objectTable;
        _commandHandler  = new CommandHandler(commandManager, pluginLog);

        _configuration = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        _configuration.Initialize(_pluginInterface);

        _pluginInterface.UiBuilder.Draw += DrawUi;

        _travelerHide = new TravelerHideCore(_framework, _clientState, _objectTable);
        
        _travelerHide.RegisterSelf(_windowSystem, _commandHandler);
    }

    public void Dispose() {
        _pluginInterface.UiBuilder.Draw -= DrawUi;
        
        _travelerHide.UnregisterSelf(_windowSystem, _commandHandler);
        _travelerHide.Dispose();
        
        _commandHandler.Dispose();
    }

    private void DrawUi() => _windowSystem.Draw();
}
