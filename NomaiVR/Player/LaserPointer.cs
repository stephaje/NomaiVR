﻿using System;
using UnityEngine;

namespace NomaiVR {
    class LaserPointer: MonoBehaviour {
        static FirstPersonManipulator _manipulator;
        public static Transform Laser;

        void Awake () {
            Laser = new GameObject("Laser").transform;
            Laser.gameObject.AddComponent<FollowTarget>();
            Laser.transform.parent = Hands.RightHand;
            Laser.transform.localPosition = new Vector3(0f, -0.05f, 0.01f);
            Laser.transform.localRotation = Quaternion.Euler(45f, 0, 0);

            var lineRenderer = Laser.gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 3 });
            lineRenderer.endColor = new Color(1, 1, 1, 0.5f);
            lineRenderer.startColor = Color.clear;
            lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.01f;

            FindObjectOfType<FirstPersonManipulator>().enabled = false;
            _manipulator = Laser.gameObject.AddComponent<FirstPersonManipulator>();

            DisableReticule();
        }

        void Update () {
            if (Laser.gameObject.activeSelf && Common.IsUsingAnyTool()) {
                Laser.gameObject.SetActive(false);
            } else if (!Laser.gameObject.activeSelf && !Common.IsUsingAnyTool()) {
                Laser.gameObject.SetActive(true);
            }
        }

        void DisableReticule () {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var rootObject in rootObjects) {
                if (rootObject.name == "Reticule") {
                    rootObject.SetActive(false);
                    return;
                }
            }
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<InteractZone>("UpdateInteractVolume", typeof(Patches), nameof(Patches.PatchUpdateInteractVolume));
                NomaiVR.Pre<InteractZone>("OnEntry", typeof(Patches), nameof(Patches.InteractZoneEntry));
                NomaiVR.Pre<InteractZone>("OnExit", typeof(Patches), nameof(Patches.InteractZoneExit));
                NomaiVR.Pre<ToolModeSwapper>("Update", typeof(Patches), nameof(Patches.ToolModeUpdate));
                NomaiVR.Pre<ItemTool>("UpdateIsDroppable", typeof(Patches), nameof(Patches.PreUpdateIsDroppable));
                NomaiVR.Post<ItemTool>("UpdateIsDroppable", typeof(Patches), nameof(Patches.PostUpdateIsDroppable));
            }

            static bool PatchUpdateInteractVolume (
                InteractZone __instance,
                float ____viewingWindow,
                ref bool ____focused
            ) {
                float num = 2f * Vector3.Angle(Laser.forward, __instance.transform.forward);
                var swapper = Locator.GetToolModeSwapper();
                var allowInteraction = swapper.IsInToolMode(ToolMode.None) || swapper.IsInToolMode(ToolMode.Item);
                ____focused = allowInteraction && num <= ____viewingWindow;
                var Base = __instance as SingleInteractionVolume;

                var method = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action) Activator.CreateInstance(typeof(Action), __instance, ftn);

                func();

                return false;
            }

            static bool InteractZoneEntry (GameObject hitObj, InteractZone __instance) {
                if (hitObj.CompareTag("PlayerDetector")) {
                    _manipulator.OnEnterInteractZone(__instance);
                }
                return false;
            }

            static bool InteractZoneExit (GameObject hitObj, InteractZone __instance) {
                if (hitObj.CompareTag("PlayerDetector")) {
                    _manipulator.OnExitInteractZone(__instance);
                }
                return false;
            }

            static void ToolModeUpdate (ref FirstPersonManipulator ____firstPersonManipulator) {
                if (____firstPersonManipulator != _manipulator) {
                    ____firstPersonManipulator = _manipulator;
                }
            }

            static Quaternion _cameraRotation;
            static Vector3 _cameraPosition;

            static void PreUpdateIsDroppable () {
                var camera = Locator.GetPlayerCamera();
                _cameraRotation = camera.transform.rotation;
                _cameraPosition = camera.transform.position;
                camera.transform.position = Laser.position;
                camera.transform.forward = Laser.forward;
            }

            static void PostUpdateIsDroppable () {
                var camera = Locator.GetPlayerCamera();
                camera.transform.position = _cameraPosition;
                camera.transform.rotation = _cameraRotation;
            }
        }

    }
}
