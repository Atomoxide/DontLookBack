using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dontlookback.Windows;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DontLookBack;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Dalamud.Game.Gui.Toast;
using Dalamud.Hooking;

namespace Dontlookback;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService] internal static IChatGui Chat { get; private set; }
    [PluginService] public static IToastGui Toasts { get; private set; }

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;

    public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    public static IPluginLog Logger { get; private set; } = null!;

    public float currentDirection;

    public bool valid;

    public static IPlayerCharacter player { get; private set; } = null!;

    private const string CommandName = "/dlb";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Don't Look Back");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin(IClientState clientState, IGameInteropProvider gameInteropProvider, IPluginLog logger)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var yoship = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "yoship.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, yoship);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "\n/dlb on: toggle on\n/dlb off: toggle off\n/dlb config: open config window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        ClientState = clientState;
        GameInteropProvider = gameInteropProvider;
        Logger = logger;
        valid = false;
        currentDirection = 0f;
        if (ClientState.LocalPlayer == null)
        {
            Logger.Error("Player not found!");
            this.Dispose();
            return;
        }
        player = ClientState.LocalPlayer!;
        //MoveFunction.Initialize(logger, player);
        ActionCall.Initialize();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        ActionCall.Instance?.Dispose();
        MoveFunction.Instance?.Dispose();
    }

    private unsafe void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui

        //if (player == null)
        //{
        //    Logger.Info("Player Null");
        //}
        //else
        //{
        //    //GameObject* PlayerObj = (GameObject*)player.Address;
        //    //Logger.Info("Player: " + (nint)PlayerObj);
        //    //Logger.Info("Prev Direction: " + ActionCall.Instance.preDirection + ", Post Direction: " + ActionCall.Instance.postDirection);
        //    //PlayerObj->Rotation = ActionCall.Instance.direction;
        //    //MoveFunction.Instance.MoveObject(PlayerObj, (PlayerObj->Position.X)+0.1f, PlayerObj->Position.Y, PlayerObj->Position.Z);
        //    //MoveFunction.Instance.Move();
        //    //MoveFunction.Instance.TurnObject(PlayerObj, 0.0f);

        //}
        if (args == "on" && !Configuration.IsTurnedOn)
        {
            MoveFunction.Initialize();
            Logger.Info("plugin turned on");
            Chat.Print("[Don't Look Back] Turned on.");
            Toasts.ShowQuest("Don't Look Back Turned on",
                    new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
            Configuration.IsTurnedOn = true;
        }
        else if (args == "off" && Configuration.IsTurnedOn)
        {
            MoveFunction.Instance?.Dispose();
            Logger.Info("plugin turned off");
            Chat.Print("[Don't Look Back] Turned off");
            Toasts.ShowQuest("Don't Look Back Turned off",
                    new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
            Configuration.IsTurnedOn = false;
        }
        else if (args == "config")
        {
            ToggleConfigUI();
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
