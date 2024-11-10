using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dontlookback;
using ImGuiNET;

namespace Dontlookback.Windows;

public class MainWindow : Window, IDisposable
{
    //private string GoatImagePath;
    private Plugin Plugin;
    private string yoshipPath;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string cat)
        : base("About##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        Size = new Vector2(375, 330);
        SizeCondition = ImGuiCond.Always;

        Plugin = plugin;
        yoshipPath = cat;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (Plugin.Configuration.IsTurnedOn)
        {
            ImGui.Text($"Don't Look Back is [ENABLED]");
        }
        else
        {
            ImGui.Text($"Don't Look Back is [DISABLED]");
        }

        ImGui.Spacing();

        var yoshipImg = Plugin.TextureProvider.GetFromFile(yoshipPath).GetWrapOrDefault();
        if (yoshipImg != null)
        {
            ImGui.Image(yoshipImg.ImGuiHandle, new Vector2(yoshipImg.Width / 2, yoshipImg.Height / 2));
        }
        else
        {
            ImGui.Text("Image not found.");
        }
    }
}
