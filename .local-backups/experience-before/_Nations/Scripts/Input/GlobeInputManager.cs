using Nations.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Nations.Input
{
    public sealed class GlobeInputManager : MonoBehaviour
    {
        public GlobeCameraController Controller;
        public float DragDeadZone = 3;
        public float RotationThreshold = 5;
        public float TiltThreshold = 12;
        public float ScrollSensitivity = 0.0015f;
        int previousCount, fingerA = -1, fingerB = -1;
        Vector2 previousA, previousB, startA;
        float twistTotal, tiltTotal;
        bool blocked, moved, rotating, tilting, mouseDragging;
        readonly List<RaycastResult> uiHits = new List<RaycastResult>(8);
        PointerEventData pointer;

        void OnEnable() => EnhancedTouchSupport.Enable();
        void OnDisable() { EnhancedTouchSupport.Disable(); if (Controller) Controller.StopMotion(); }
        bool OverUI(Vector2 position)
        {
            if (!EventSystem.current) return false;
            pointer ??= new PointerEventData(EventSystem.current);
            pointer.position = position; uiHits.Clear();
            EventSystem.current.RaycastAll(pointer, uiHits);
            return uiHits.Count > 0;
        }
        void Update()
        {
            var touches = Touch.activeTouches;
            int count = touches.Count;
            if (count > 0)
            {
                mouseDragging = false;
                int a = touches[0].touchId, b = count == 2 ? touches[1].touchId : -1;
                Vector2 pa = touches[0].screenPosition, pb = count == 2 ? touches[1].screenPosition : default;
                if (count != previousCount || a != fingerA || b != fingerB)
                {
                    Controller.EndDrag(false);
                    blocked = count > 2 || OverUI(pa) || (count == 2 && OverUI(pb));
                    previousA = startA = pa; previousB = pb;
                    moved = rotating = tilting = false; twistTotal = tiltTotal = 0;
                    if (count == 1 && !blocked) Controller.BeginDrag();
                }
                else if (!blocked && count == 1)
                {
                    if ((pa - startA).sqrMagnitude > DragDeadZone * DragDeadZone) moved = true;
                    if (moved) Controller.Drag(previousA, pa, Time.unscaledDeltaTime);
                }
                else if (!blocked && count == 2)
                {
                    Vector2 oldSpan = previousB - previousA, span = pb - pa;
                    Vector2 da = pa - previousA, db = pb - previousB;
                    if (oldSpan.magnitude > 20 && span.magnitude > 20)
                    {
                        float pinch = Mathf.Log(oldSpan.magnitude / span.magnitude);
                        float twist = Vector2.SignedAngle(oldSpan, span);
                        twistTotal += twist;
                        float vertical = (da.y + db.y) * 0.5f;
                        bool parallel = Vector2.Dot(da, db) > 0 && Mathf.Abs(da.y - db.y) < Mathf.Abs(vertical) + 2;
                        if (parallel && Mathf.Abs(pinch) < 0.025f) tiltTotal += vertical;
                        if (Mathf.Abs(twistTotal) > RotationThreshold) rotating = true;
                        if (Mathf.Abs(tiltTotal) > TiltThreshold) tilting = true;
                        if (Mathf.Abs(pinch) > 0.001f) Controller.Zoom(pinch, (pa + pb) * 0.5f);
                        Controller.TiltRotate(tilting && parallel ? vertical : 0, rotating ? -twist : 0);
                    }
                }
                previousA = pa; previousB = pb; fingerA = a; fingerB = b; previousCount = count;
                return;
            }
            if (previousCount > 0) { Controller.EndDrag(previousCount == 1 && moved); previousCount = 0; fingerA = fingerB = -1; }
            if (Application.isMobilePlatform || Mouse.current == null) return;
            var mouse = Mouse.current; Vector2 pos = mouse.position.ReadValue();
            bool modifier = Keyboard.current != null && (Keyboard.current.altKey.isPressed || Keyboard.current.shiftKey.isPressed);
            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            {
                blocked = OverUI(pos); previousA = pos; mouseDragging = !blocked;
                if (!blocked) Controller.BeginDrag();
            }
            if (mouseDragging && (mouse.leftButton.isPressed || mouse.rightButton.isPressed))
            {
                Vector2 delta = pos - previousA;
                if (mouse.rightButton.isPressed || modifier) { Controller.EndDrag(false); Controller.TiltRotate(delta.y, -delta.x * 0.2f); }
                else Controller.Drag(previousA, pos, Time.unscaledDeltaTime);
            }
            else if (mouseDragging) { mouseDragging = false; Controller.EndDrag(); }
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0 && !OverUI(pos)) Controller.Zoom(-scroll * ScrollSensitivity, pos);
            previousA = pos;
        }
        void OnApplicationFocus(bool focus)
        {
            if (focus) return;
            previousCount = 0; mouseDragging = false; fingerA = fingerB = -1;
            Controller.StopMotion();
        }
    }
}
