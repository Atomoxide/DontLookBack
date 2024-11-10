using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dontlookback;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;

namespace DontLookBack
{
    public unsafe class ActionCall : IDisposable
    {
        //private delegate void ActionTurnDelegate(nint unknownPtr, Vector3* position, uint targetId);
        //private Hook<ActionTurnDelegate>? _onActionTurnHook;

        private delegate void OnActionUsedDelegate(uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);
        private Hook<OnActionUsedDelegate>? onActionUsedHook;

        public static void Initialize() { Instance = new ActionCall(); }

        public static ActionCall Instance { get; private set; } = null!;

        public float preDirection;
        public float postDirection;
        //private readonly IPlayerCharacter? player;
        private ActionCall()
        {
            try
            {
                onActionUsedHook = Plugin.GameInteropProvider.HookFromSignature<OnActionUsedDelegate>(
                    "40 ?? 56 57 41 ?? 41 ?? 41 ?? 48 ?? ?? ?? ?? ?? ?? ?? 48",
                    OnActionUsed
                );
                onActionUsedHook?.Enable();


                //_onActionTurnHook = Plugin.GameInteropProvider.HookFromSignature<ActionTurnDelegate>(
                //        "48 89 5C 24 08 57 48 83 EC 20 F3 0F 10 42 08 49 8B F8",
                //        OnActionTurn
                //    );

                //_onActionTurnHook?.Enable();
            }
            catch (Exception e)
            {
                Plugin.Logger.Error("Error initiating OnloadingHitbox hooks: " + e.Message);
            }
            //this.player = player;


        }


        ~ActionCall()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {
                Plugin.Logger.Info("Already disposed: " + e.Message);
            }

        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            onActionUsedHook?.Disable();
            onActionUsedHook?.Dispose();

            //_onActionTurnHook?.Disable();
            //_onActionTurnHook?.Dispose();
        }

        private void OnActionUsed(uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader,
            IntPtr effectArray, IntPtr effectTrail)
        {
            onActionUsedHook?.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);
            //IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            //if (player == null || sourceId != player.GameObjectId) { return; }
            if (sourceId != Plugin.player.GameObjectId) { return; }

            //MoveFunction.Instance.MoveObject((GameObject*)player.Address, preDirection);
            MoveFunction.Instance.Move();
            
        }

        //private unsafe void OnActionTurn(nint unknownPtr, Vector3* position, uint targetId)
        //{

        //    try
        //    {
        //        //IPlayerCharacter? player = ClientState.LocalPlayer;
        //        if (player != null)
        //        {
        //            GameObject* PlayerObj = (GameObject*)player.Address;
        //            this.preDirection = PlayerObj->Rotation;
        //        }
        //        else
        //        {
        //            this.preDirection = -99.0f;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Plugin.Logger.Error(ex, "An error reading player pre rotation.");
        //    }

        //    this._onActionTurnHook?.Original(unknownPtr, position, targetId);

        //    try
        //    {
        //        //IPlayerCharacter? player = ClientState.LocalPlayer;
        //        if (player != null)
        //        {
        //            GameObject* PlayerObj = (GameObject*)player.Address;
        //            this.postDirection = PlayerObj->Rotation;
        //        }
        //        else
        //        {
        //            this.postDirection = -99.0f;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Plugin.Logger.Error(ex, "An error reading player post rotation.");
        //    }

        //}
    }
}
