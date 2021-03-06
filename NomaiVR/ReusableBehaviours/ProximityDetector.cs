﻿using System;
using UnityEngine;

namespace NomaiVR {
    class ProximityDetector: MonoBehaviour {
        public float minDistance = 0.15f;
        public float exitThreshold = 0.01f;
        public Transform other;
        public Action onEnter;
        public Action onExit;
        public Vector3 localOffset;
        public bool isInside;

        void Update () {
            if (!other.gameObject.activeSelf) {
                return;
            }

            var offset = transform.TransformVector(localOffset);
            var distance = Vector3.Distance(transform.position + offset, other.position);

            if (!isInside && distance <= minDistance) {
                if (onEnter != null) {
                    onEnter.Invoke();
                }
                isInside = true;
            }

            if (isInside && distance > minDistance + exitThreshold) {
                if (onExit != null) {
                    onExit.Invoke();
                }
                isInside = false;
            }
        }
    }
}
