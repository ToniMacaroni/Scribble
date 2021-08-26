using System;
using BS_Utils.Utilities;
using IPA.Utilities;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VRUIControls;

namespace Scribble
{
    internal class BrushBehaviour : MonoBehaviour
    {
        private Vector3 _lastPoint;

        private readonly float _minDistance = 0.01f;
        private bool _pressed;
        public InputManager InputManager;
        public VRPointer Pointer;
        public SaberType SaberType;
        public CustomBrush CurrentBrush;
        public bool EraseMode;
        public Eraser Eraser;
        public GameObject BrushMesh;

        public GameObject MenuHandle;

        private bool MenuHandleActive
        {
            set
            {
                if(MenuHandle)MenuHandle.SetActive(value);
            }
        }

        private void Start()
        {
            InputManager = gameObject.AddComponent<InputManager>();
            InputManager.SaberType = SaberType;
            InputManager.ButtonPressed += OnPress;
            InputManager.ButtonReleased += OnRelease;

            if (SaberType == SaberType.SaberA)
            {
                CurrentBrush = Brushes.BrusheList[0];
                GlobalBrushManager.LeftBrush = this;
            }
            else
            {
                CurrentBrush = Brushes.BrusheList[1];
                GlobalBrushManager.RightBrush = this;
                GlobalBrushManager.ActiveBrush = this;
            }

            GetMenuHandle();

            GameObject meshObj = CreateBrushMesh();
            BrushMesh = meshObj;
            Eraser = gameObject.AddComponent<Eraser>();
            Eraser.Init(this);

            Plugin.Log.Debug("Brush Initialized");
            enabled = false;
        }

        private void OnPress()
        {
            GlobalBrushManager.ActiveBrush = this;
            if (CheckForUI()) return;
            if (ScribbleContainer.Instance == null) return;
            if (!ScribbleContainer.DrawingEnabled) return;
            //MenuHandleActive = false;
            if (EraseMode)
            {
                Eraser.StartErasing();
                return;
            }
            _lastPoint = transform.position;
            ScribbleContainer.Instance.InitPoint(_lastPoint, SaberType, CurrentBrush);
            _pressed = true;
            //UpdateBrushMesh();
            //ShowBrushMesh(true);
        }

        private void OnRelease()
        {
            //MenuHandleActive = true;
            if (EraseMode)
            {
                Eraser.StopErasing();
                return;
            }
            if (!_pressed) return;
            _pressed = false;
            //ShowBrushMesh(false);
            ScribbleContainer.Instance.CheckLine(SaberType);
        }

        private float _time;
        private bool _didUpdateMeshLastFrame;

        private void Update()
        {
            if (_pressed)
            {
                Vector3 position = transform.position;
                var distance = Vector3.Distance(_lastPoint, position);
                if (distance > _minDistance)
                {
                    ScribbleContainer.Instance.AddPoint(position, SaberType);
                    _lastPoint = position;
                }
            }

            if (_time < 0.2f)
            {
                _time += Time.deltaTime;
                return;
            }

            if (EraseMode) return;

            if (CheckForUI())
            {
                ShowBrushMesh(false);
                MenuHandleActive = true;
                _didUpdateMeshLastFrame = false;
            }
            else
            {
                if (!_didUpdateMeshLastFrame)
                {
                    UpdateBrushMesh();
                    _didUpdateMeshLastFrame = true;
                }
                ShowBrushMesh(true);
                MenuHandleActive = false;
            }
        }

        private bool CheckForUI()
        {
            var pointerData = Pointer.GetField<PointerEventData, VRPointer>("_pointerData");
            return pointerData.pointerCurrentRaycast.gameObject;
        }

        private GameObject CreateBrushMesh()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(transform, false);
            go.SetActive(false);
            Material mat = new Material(Effects.GetEffect("standard").Shader);
            mat.color = new Color(1, 1, 1, 0.5f);
            go.GetComponent<MeshRenderer>().material = mat;
            return go;
        }

        public void ShowBrushMesh(bool show)
        {
            var mr = Pointer.vrController.gameObject.GetComponent<MeshRenderer>();
            if (mr) mr.enabled = !show;
            BrushMesh.SetActive(show);
        }

        public void UpdateBrushMesh()
        {
            float size = ScribbleContainer.LineWidth * CurrentBrush.Size;
            BrushMesh.transform.localScale = new Vector3(size, size, size);
            BrushMesh.GetComponent<MeshRenderer>().material.color = CurrentBrush.Color;
        }

        public void GetMenuHandle()
        {
            MenuHandle = transform.Find("MenuHandle").gameObject;
        }
    }
}