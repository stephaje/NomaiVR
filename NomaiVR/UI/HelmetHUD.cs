﻿using UnityEngine;

namespace NomaiVR {
    public class HelmetHUD: MonoBehaviour {
        private static Transform _thrusterHUD;
        private static Transform _helmet;

        void Awake () {
            _thrusterHUD = GameObject.Find("HUD_Thrusters").transform;

            // Move helmet forward to make it a bit more visible.
            _helmet = FindObjectOfType<HUDHelmetAnimator>().transform;
            _helmet.localPosition = Vector3.forward * 0.2f;
            _helmet.gameObject.AddComponent<SmoothFoolowParentRotation>();


            // Adjust projected HUD.
            var surface = GameObject.Find("HUD_CurvedSurface").transform;
            surface.transform.localScale = Vector3.one * 3.28f;
            surface.transform.localPosition = new Vector3(-0.06f, -0.56f, 0.06f);
            var notifications = FindObjectOfType<SuitNotificationDisplay>().GetComponent<RectTransform>();
            notifications.anchoredPosition = new Vector2(-200, -100);

            var playerHUD = GameObject.Find("PlayerHUD");

            // Fix lock on UI on suit mode.
            var lockOnCanvas = playerHUD.transform.Find("HelmetOffUI/HelmetOffLockOn").GetComponent<Canvas>();
            lockOnCanvas.planeDistance = 10;
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ThrustAndAttitudeIndicator>("LateUpdate", typeof(Patches), nameof(PatchLateUpdate));
                NomaiVR.Post<HUDCamera>("Awake", typeof(Patches), nameof(PostHUDCameraAwake));
            }

            static void PostHUDCameraAwake (Camera ____camera) {
                // Prevent distortion of helmet HUD.
                ____camera.fieldOfView = 60;
            }

            static void PatchLateUpdate () {
                // Fix thruster HUD rotation.
                var rotation = _helmet.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
                _thrusterHUD.transform.rotation = rotation;
            }
        }
    }
}
