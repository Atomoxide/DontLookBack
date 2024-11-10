using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Dontlookback;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace DontLookBack
{
    public unsafe class MoveFunction : IDisposable
    {
        //private delegate void TurnObjectDelegate(GameObject *gameObjectPtr, float direction);
        //private Hook<TurnObjectDelegate>? turnObjectHook;
        private delegate void MoveObjectDelegate(GameObject* gameObjectPtr, float x, float y, float z);
        
        private float moveSum;
        public Matrix4x4 View;

        //[Signature("40 53 48 83 EC 20 F3 0F 11 89 B0 00 00 00")]
        //private readonly MoveObjectDelegate? moveObject = null;

        private delegate void RMIWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
        private Hook<RMIWalkDelegate>? walkHook;

        public static MoveFunction Instance { get; private set; } = null!;

        public static void Initialize() { Instance = new MoveFunction(); }
        private MoveFunction()
        {
            Plugin.GameInteropProvider.InitializeFromAttributes(this);
            try
            {
                walkHook = Plugin.GameInteropProvider.HookFromSignature<RMIWalkDelegate>(
                    "E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D",
                    WalkFunc
                    );

                walkHook?.Enable();

                //turnObjectHook = Plugin.GameInteropProvider.HookFromSignature<TurnObjectDelegate>(
                //        "40 53 48 83 EC 20 F3 0F 10 81 C0 00 00 00 48 8B D9 0F 2E C1 7A 02 74 28",
                //        TurnObject
                //    );

                //turnObjectHook?.Enable();
            }
            catch (Exception e)
            {
                Plugin.Logger.Error("Error initiating move function hooks: " + e.Message);
            }
            //this.logger = logger;
            //this.player = player;
            this.moveSum = 0.0f;
        }

        ~MoveFunction()
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

            walkHook?.Disable();
            walkHook?.Dispose();

            //turnObjectHook?.Disable();
            //turnObjectHook?.Dispose();
        }

        public unsafe void WalkFunc(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
        {
            if (walkHook != null)
            {
                *sumForward = this.moveSum;
                if (this.moveSum > 0)
                {
                    var controlCamera = CameraManager.Instance()->GetActiveCamera();
                    var renderCamera = controlCamera != null ? controlCamera->SceneCamera.RenderCamera : null;
                    if (renderCamera == null)
                        return;
                    View = renderCamera->ViewMatrix;
                    var CameraAzimuth = MathF.Atan2(-View.M13, -View.M33);
                    ((GameObject*)Plugin.player.Address)->SetRotation(CameraAzimuth);
                    this.moveSum -= 5f;
                }
                walkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

                //logger.Info("params: " + *sumLeft + " " + *sumForward + " " + *sumTurnLeft + " " + *haveBackwardOrStrafe + " " + *a6 + " " + bAdditiveUnk);
                //var controlCamera = CameraManager.Instance()->GetActiveCamera();
                //var renderCamera = controlCamera != null ? controlCamera->SceneCamera.RenderCamera : null;
                //if (renderCamera == null)
                //return;
                //View = renderCamera->ViewMatrix;
                //var CameraAzimuth = MathF.Atan2(-View.M13, -View.M33);
                //logger.Info("camera: " + CameraAzimuth);
            }

        }

        //public void MoveObject(GameObject* gameObjectPtr, float x, float y, float z)
        //{
        //    if (this.moveObject != null)
        //    {
        //        moveObject(gameObjectPtr, x, y, z);
        //    }
        //    else
        //    {
        //        logger.Info("Move function call unsuccessful");
        //    }
        //}

        public void Move()
        {
            //logger.Info("Turn!");
            this.moveSum = 10f;
        }

        //public unsafe void TurnObject(GameObject* gameObjectPtr, float direction)
        //{
        //    if (player != null && gameObjectPtr == (GameObject*)player.Address)
        //    {
        //        logger.Info("Turn Function called for " + ((GameObject*)player.Address)->EntityId + " to new rotation " + direction);
        //    }
        //    if (direction == ActionCall.Instance.postDirection)
        //    {
        //        direction = ActionCall.Instance.preDirection;
        //    }
        //    this.turnObjectHook?.Original(gameObjectPtr, direction);
        //}
    }
}
