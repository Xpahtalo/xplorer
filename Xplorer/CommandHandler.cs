using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;

namespace Xplorer;

public sealed class CommandHandler : IDisposable {
    private readonly ICommandManager            _commandManager;
    private readonly IPluginLog                 _pluginLog;
    private readonly List<string>               _registeredCommandNames;
    private readonly Dictionary<string, Action> _simpleCommands;

    internal CommandHandler(ICommandManager commandManager, IPluginLog pluginLog) {
        _commandManager         = commandManager;
        _pluginLog              = pluginLog;
        _registeredCommandNames = [];
        _simpleCommands         = [];
    }

    public void Dispose() {
        UnregisterCommands(_registeredCommandNames);
    }

    public void RegisterCommands(IEnumerable<IHandlerCommand> commands) {
        foreach (var command in commands) {
            switch (command) {
                case SimpleCommand simpleCommand:
                    RegisterSimpleCommand(simpleCommand);
                    break;
                case CustomCommand customCommand:
                    RegisterCustomCommand(customCommand);
                    break;
            }
        }
    }

    private void RegisterSimpleCommand(SimpleCommand command) {
        _pluginLog.Verbose("Adding new simple command\n{0}\n{1}", command.Name,
                           command.Description);
        _commandManager.AddHandler(command.Name, new CommandInfo(OnSimpleCommand) {
            HelpMessage = command.Description,
            ShowInHelp  = command.ShowHelp,
        });
        _registeredCommandNames.Add(command.Name);
        _simpleCommands.Add(command.Name.ToLowerInvariant(), command.Action);
    }

    private void RegisterCustomCommand(CustomCommand customCommand) {
        _pluginLog.Verbose("Adding new custom command\n{0}\n{1}");
        _commandManager.AddHandler(customCommand.Name, customCommand.CommandInfo);
        _registeredCommandNames.Add(customCommand.Name);
    }

    private void OnSimpleCommand(string command, string arguments) {
        var key = command.ToLowerInvariant();
        _simpleCommands[key].Invoke();
    }

    public void UnregisterCommands(IEnumerable<IHandlerCommand> commands) {
        UnregisterCommands(commands.Select(command => command.Name));
    }

    private void UnregisterCommands(IEnumerable<string> commandNames) {
        foreach (var name in commandNames) {
            _pluginLog.Debug("Removing command {0}", name);
            _commandManager.RemoveHandler(name);
            _registeredCommandNames.RemoveAll(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
            _simpleCommands.Remove(name.ToLowerInvariant());
        }
    }
}

public interface IHandlerCommand {
    public string Name { get; }
}

public class CustomCommand(string name, CommandInfo commandInfo) : IHandlerCommand {
    public string      Name        => name;
    public CommandInfo CommandInfo => commandInfo;
}

public class SimpleCommand(string name, string description, bool showHelp, Action action) : IHandlerCommand {
    public string Name        => name;
    public string Description => description;
    public bool   ShowHelp    => showHelp;
    public Action Action      => action;
}