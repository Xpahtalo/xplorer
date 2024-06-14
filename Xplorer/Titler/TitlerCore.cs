using System;
using Dalamud.Plugin.Services;
using XivCommon;
using XivCommon.Functions;

namespace Xplorer.Titler;

public class TitlerCore {
    private readonly IClientState    _clientState;
    private readonly ICommandManager _commandManager;
    private readonly IFramework      _framework;
    private readonly IPluginLog      _pluginLog;

    private readonly Chat _chat;

    internal TitlerCore(IClientState clientState, ICommandManager commandManager, IFramework framework,
                        IPluginLog   pluginLog,   XivCommonBase   common) {
        _clientState    = clientState;
        _commandManager = commandManager;
        _framework      = framework;
        _pluginLog      = pluginLog;
        _chat           = common.Functions.Chat;

        _clientState.Login += OnLogin;
    }

    private void OnLogin() {
        _framework.RunOnTick(
            ChangeTitle
          , TimeSpan.FromSeconds(5));
    }

    private void ChangeTitle() {
        try {
            var command = _chat.SanitiseText("/title");
            _chat.SendMessage(command);
            _pluginLog.Info("Changed title");
        }
        catch (Exception ex) {
            _pluginLog.Error("Failed to change title\n{0}", ex);
        }
    }

    internal void Dispose() {
        _clientState.Login -= OnLogin;
    }
}