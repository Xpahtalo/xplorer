using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static Dalamud.Plugin.Services.IAddonLifecycle;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace Xplorer.AddonExplore;

internal sealed class AddonExploreCore : IDisposable {
    private const bool OpenOnLoad = false;

    private readonly IFramework      _framework;
    private readonly IAddonLifecycle _addonLifecycle;
    private readonly IPluginLog      _pluginLog;

    private readonly AddonExploreWindow    _mainWindow;
    private readonly List<IHandlerCommand> _commands;

    internal const string       BattleTalkName  = "_BattleTalk";
    private const  int          BattleTalkArray = 34;
    internal       bool         BattleTalkRegistered;
    internal       List<string> BattleTalkEvents;

    private bool _blockBattleTalk;


    internal bool BlockBattleTalk {
        get => _blockBattleTalk;
        set {
            const AddonEvent blockEvent = AddonEvent.PreRequestedUpdate;
            try {
                if (value) {
                    _addonLifecycle.RegisterListener(blockEvent, BattleTalkName, OnBattleTalkPostRequest);
                } else {
                    _addonLifecycle.UnregisterListener(blockEvent, BattleTalkName, OnBattleTalkPostRequest);
                }

                _blockBattleTalk = value;
            }
            catch {
                // ignored
            }
        }
    }

    internal AddonExploreCore(IAddonLifecycle addonLifecycle, IPluginLog pluginLog, IFramework framework) {
        _addonLifecycle = addonLifecycle;
        _pluginLog      = pluginLog;
        _framework      = framework;
        _mainWindow     = new AddonExploreWindow(this);
        _commands = new List<IHandlerCommand> {
            new SimpleCommand("/xaddon", "Open the Addon Explore window.", true, _mainWindow.Toggle),
        };

        BattleTalkEvents     = [];
        BattleTalkRegistered = false;
        _blockBattleTalk     = false;
    }

    internal void RegisterBattleTalk() {
        RegisterAllEvents(BattleTalkName, OnBattleTalk);
        BattleTalkRegistered = true;
    }

    internal void UnregisterBattleTalk() {
        UnregisterAllEvents(BattleTalkName);
        BattleTalkRegistered = false;
    }

    private void RegisterAllEvents(string addonName, AddonEventDelegate eventDelegate) {
        foreach (var eventType in Enum.GetValues<AddonEvent>()) {
            _pluginLog.Verbose($"Registering addon event {addonName} - {eventType}");
            _addonLifecycle.RegisterListener(eventType, addonName, eventDelegate);
        }
    }

    private void UnregisterAllEvents(string addonName) {
        foreach (var eventType in Enum.GetValues<AddonEvent>()) {
            _pluginLog.Verbose("Unregistering addon event {0} - {1}", addonName, eventType);
            _addonLifecycle.UnregisterListener(eventType, addonName);
        }
    }

    private unsafe void OnBattleTalk(AddonEvent type, AddonArgs? args) {
        string? s = null;

        switch (args) {
//             case AddonSetupArgs setup:
//                 s = $"""
//                      {type}
//                      {setup}
//                      {BuildAtkValuesString(setup.AtkValueSpan)}
//                      """;
//                 break;
//             case AddonUpdateArgs update:
//                 break;
//             case AddonDrawArgs draw:
//                 break;
//             case AddonFinalizeArgs finalize:
//                 s = finalize.ToString();
//                 break;
//             case AddonRequestedUpdateArgs request:
//                 var sb         = new StringBuilder();
//                 var stringData = (StringArrayData**)request.StringArrayData;
//                 var array      = stringData[BattleTalkArray];
//                 var size       = array->AtkArrayData.Size;
//                 if (size == 0) {
//                     break;
//                 }
//
//                 sb.Append(request);
//                 sb.AppendLine();
//                 sb.Append("  Size: ");
//                 sb.Append(size);
//                 sb.Append(' ');
//                 for (var i = 0; i < size; i++) {
//                     sb.Append($"\n[{i}] ");
//                     sb.Append(MemoryHelper.ReadStringNullTerminated((IntPtr)array->ManagedStringArray[i]));
//                 }
//
//                 s = sb.ToString();
//                 break;
//             case AddonRefreshArgs refresh:
//                 s = BuildAtkValuesString(refresh.AtkValueSpan);
//                 break;
            case AddonReceiveEventArgs receive:
                s = $"""
                     {receive}
                     EventType:  {receive.AtkEventType}
                     EventParam: {receive.EventParam}
                     """;
                break;
        }

        if (!s.IsNullOrWhitespace()) {
            BattleTalkEvents.Add(s);
        }
    }

    private unsafe void OnBattleTalkPostRequest(AddonEvent type, AddonArgs args) {
        var sb = new StringBuilder();

        sb.AppendLine($"{type} Encountered.");
        if (args is not AddonRequestedUpdateArgs request) {
            Ignore("Wrong args");
            return;
        }

        var addon = (AtkUnitBase*)request.Addon;
        var count = addon->AtkValuesCount;
        if (count != 0) {
            sb.AppendLine($"AtkCount: {count}");
            var s = BuildAtkValuesString(new Span<AtkValue>(addon->AtkValues, count));
            BattleTalkEvents.Add(s);
        }

        if (addon->IsVisible) {
            Ignore("Visible");
            return;
        }

        var stringData = (StringArrayData**)request.StringArrayData;
        var array      = stringData[BattleTalkArray];

        var text = MemoryHelper.ReadStringNullTerminated((IntPtr)array->ManagedStringArray[1]);
        if (!text.Contains("blessing")) {
            sb.AppendLine($"RootNodeTweenState: {addon->RootNodeTween.State}");
            sb.AppendLine($"VisibilityFlags: {addon->VisibilityFlags}");
            sb.AppendLine($"CollisionNodeCount: {addon->CollisionNodeListCount}");
            Ignore($"Wrong text[{text}]");
            return;
        }

        addon->IsVisible = false;
        AddEvent("Blocking BattleTalk!!!");
        return;

        void Ignore(string reason) {
            AddEvent($"Ignored: {reason}");
        }

        void AddEvent(string result) {
            sb.AppendLine($" - {result}");
            BattleTalkEvents.Add(sb.ToString());
        }
    }

    private static unsafe string BuildAtkValuesString(Span<AtkValue> values) {
        StringBuilder sb = new();
        var           i  = 0;
        foreach (var value in values) {
            sb.Append($"[{i}] = {value.Type}: ");
            sb.AppendLine(AtkValueString(value));
        }

        return sb.ToString();

        string AtkValueString(AtkValue value) {
            return value.Type switch {
                ValueType.Int     => value.Int.ToString(),
                ValueType.Bool    => value.Int != 0 ? "true" : "false",
                ValueType.UInt    => value.UInt.ToString(),
                ValueType.Float   => value.Float.ToString(CultureInfo.InvariantCulture),
                ValueType.String  => MemoryHelper.ReadStringNullTerminated((IntPtr)value.String),
                ValueType.String8 => MemoryHelper.ReadStringNullTerminated((IntPtr)value.String),
                _                 => "?",
            };
        }
    }

    internal void RegisterSelf(WindowSystem windowSystem, CommandHandler commandHandler) {
        windowSystem.AddWindow(_mainWindow);
        commandHandler.RegisterCommands(_commands);


#pragma warning disable CS0162 // Unreachable code detected
        if (OpenOnLoad) {
            _mainWindow.IsOpen = true;
        }
#pragma warning restore CS0162 // Unreachable code detected
    }

    internal void UnregisterSelf(WindowSystem windowSystem, CommandHandler commandHandler) {
        windowSystem.RemoveWindow(_mainWindow);
        commandHandler.UnregisterCommands(_commands);
    }

    public void Dispose() {
        UnregisterAllEvents(BattleTalkName);
    }
}