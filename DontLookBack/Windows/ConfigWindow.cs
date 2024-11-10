using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dontlookback;
using ImGuiNET;

namespace Dontlookback.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Don't Look Back Configuration")
    {
        //Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        //        ImGuiWindowFlags.NoScrollWithMouse;

        //Size = new Vector2(232, 90);
        //SizeCondition = ImGuiCond.Always;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 120),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var configValue = configuration.IsTurnedOn;
        if (ImGui.Checkbox("Enable (non persistent)", ref configValue))
        {
            configuration.IsTurnedOn = configValue;
            configuration.UpdateStatus();
        }
    }
}
