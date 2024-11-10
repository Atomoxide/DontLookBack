using Dalamud.Configuration;
using Dalamud.Hooking;
using Dalamud.Plugin;
using Dontlookback;
using DontLookBack;
using System;

namespace Dontlookback;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsTurnedOn { get; set; } = false;

    public void UpdateStatus()
    {
        if (IsTurnedOn)
        {
            MoveFunction.Initialize();
            Plugin.Logger.Info("plugin turned on");
            Plugin.Chat.Print("[Don't Look Back] Turned on");
        }
        else if (!IsTurnedOn)
        {
            MoveFunction.Instance?.Dispose();
            Plugin.Logger.Info("plugin turned off");
            Plugin.Chat.Print("[Don't Look Back] Turned off");
        }
    }
}
