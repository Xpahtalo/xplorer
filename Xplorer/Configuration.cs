using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Xplorer;

[Serializable]
public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized] private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface) {
        PluginInterface = pluginInterface;
    }

    public void Save() {
        PluginInterface!.SavePluginConfig(this);
    }
}