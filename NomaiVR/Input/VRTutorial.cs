﻿using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class VRTutorial: MonoBehaviour {
        static TutorialState _tutorialState;

        void Start () {
            NomaiVR.Log("Start VRTutorial");
            SetTutorialState(TutorialState.LOOK);

            SteamVR_Actions.default_Look.onChange += OnLookChange;
            SteamVR_Actions.default_Move.onChange += OnMoveChange;
            SteamVR_Actions.default_PrimaryAction.onChange += OnPrimaryChange;
        }

        void SetTutorialState (TutorialState state) {
            _tutorialState = state;

            if (state == TutorialState.LOOK) {
                SteamVR_Actions.default_Look.ShowOrigins();
            } else if (state == TutorialState.MOVE) {
                SteamVR_Actions.default_Move.ShowOrigins();
            } else if (state == TutorialState.INTERACT) {
                SteamVR_Actions.default_PrimaryAction.ShowOrigins();
            } else if (state == TutorialState.JUMP) {
                SteamVR_Actions.default_PrimaryAction.HideOrigins();
            }
        }

        void GoToMoveState () {
            SetTutorialState(TutorialState.MOVE);
        }

        void GoToInteractState () {
            SetTutorialState(TutorialState.INTERACT);
        }

        void GoToJumpState () {
            SetTutorialState(TutorialState.JUMP);
        }

        void OnLookChange (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
            if (_tutorialState == TutorialState.LOOK && delta.magnitude > 0.1f) {
                Invoke(nameof(GoToMoveState), 3);
            }
        }

        void OnMoveChange (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
            if (_tutorialState == TutorialState.MOVE && delta.magnitude > 0.1f) {
                Invoke(nameof(GoToInteractState), 3);
            }
        }

        private void OnPrimaryChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            if (_tutorialState == TutorialState.INTERACT && newState) {
                Invoke(nameof(GoToJumpState), 3);
            }
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<JumpPromptTrigger>("OnTriggerEnter", typeof(Patches), nameof(ShowJumpPrompt));
                NomaiVR.Pre<JumpPromptTrigger>("OnTriggerExit", typeof(Patches), nameof(HideJumpPrompt));
            }

            static void ShowJumpPrompt () {
                if (_tutorialState == TutorialState.JUMP) {
                    SteamVR_Actions.default_Jump.ShowOrigins();
                }
            }

            static void HideJumpPrompt () {
                SteamVR_Actions.default_Jump.HideOrigins();
            }
        }

        enum TutorialState {
            LOOK,
            MOVE,
            INTERACT,
            JUMP,
        }
    }
}
