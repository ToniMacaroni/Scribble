using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IPA.Utilities;
using Scribble.Installers;
using Scribble.Stores;
using Scribble.Tools;
using SiraUtil.Logging;
using SiraUtil.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using VRUIControls;
using Zenject;

namespace Scribble
{
    internal class BrushBehaviour : MonoBehaviour
    {
        public event Action BrushSelected;

        public SaberType SaberType { get; private set; }

        public CustomBrush CurrentBrush
        {
            get => _brushBox.CurrentBrush;
            set => _brushBox.CurrentBrush = value;
        }

        public ITool SelectedTool
        {
            get => _selectedTool;
            set
            {
                if (_selectedTool == value) return;
                PreviousTool = _selectedTool;
                _selectedTool = value;
                _selectedTool?.OnSelected();
            }
        }

        public ITool PreviousTool { get; set; }

        private BrushMeshDrawer _brushMeshDrawer;
        private VRPointer _pointer;
        private InputManager _inputManager;

        private SiraLog _logger;
        private EffectStore _effects;
        private BrushStore _brushStore;
        private ScribbleContainer _container;
        private PluginConfig _config;
        private bool _isFpfc;
        private Transform _transform;
        private Dictionary<string, ITool> _tools;

        private float _time;
        private bool _pressed;
        private bool _shouldRender;

        private GameObject _menuHandle;
        private MeshRenderer _pointerRenderer;
        private ITool _selectedTool;
        private readonly BrushBox _brushBox = new BrushBox();

        private static List<Type> _toolTypes;

        private bool MenuHandleActive
        {
            set
            {
                if (_menuHandle) _menuHandle.SetActive(value);
            }
        }
        
        private bool PointerActive
        {
            set
            {
                if (_pointerRenderer) _pointerRenderer.enabled = value;
            }
        }

        [Inject]
        public void Construct(
            EffectStore effects,
            BrushStore brushStore,
            SiraLog logger,
            ScribbleContainer container,
            LaunchOptions launchOptions,
            BrushMeshDrawer brushMeshDrawer,
            PluginConfig config)
        {
            _effects = effects;
            _brushStore = brushStore;
            _logger = logger;
            _container = container;
            _config = config;

            _isFpfc = launchOptions.FPFC;
            _transform = transform;
            _brushMeshDrawer = brushMeshDrawer;
        }

        private void Awake()
        {
            enabled = false;
        }

        public void Init(SaberType saberType, VRPointer pointer)
        {
            if (pointer is null)
            {
                _logger.Error($"Pointer is null for {saberType}");
                return;
            }

            SaberType = saberType;
            _pointer = pointer;
            _pointerRenderer = _pointer.vrController.GetComponent<MeshRenderer>();

            _toolTypes ??= Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(ITool).IsAssignableFrom(x) && x!=typeof(ITool))
                .ToList();

            _tools = new Dictionary<string, ITool>();

            _brushMeshDrawer.SetTransform(_transform);

            foreach (var toolType in _toolTypes)
            {
                var tool = (ITool)Activator.CreateInstance(toolType);
                (tool as IBrushConsumer)?.SetBrushBox(_brushBox);
                tool.Init(_brushMeshDrawer, _container, _config, saberType);
                _tools.Add(toolType.Name, tool);
            }

            if (_isFpfc)
            {
                _inputManager = gameObject.AddComponent<MouseInputManager>();
                ((MouseInputManager) _inputManager).ButtonIdx = saberType == SaberType.SaberA ? 0 : 1;
            }
            else
            {
                _inputManager = gameObject.AddComponent<VrInputManager>();
                ((VrInputManager)_inputManager).SaberType = saberType;
            }

            _inputManager.ButtonPressed += OnPress;
            _inputManager.ButtonReleased += OnRelease;

            if (SaberType == SaberType.SaberA)
            {
                CurrentBrush = _brushStore.BrushList[0];
            }
            else
            {
                CurrentBrush = _brushStore.BrushList[1];
                BrushSelected?.Invoke();
            }

            SelectedTool = GetTool<DrawingTool>();
            SelectedTool.OnSelected();

            _menuHandle = _transform.Find("MenuHandle").gameObject;

            _logger.Info($"CurrentBrush initialized for {name}");
        }

        public void SetTool(string toolName)
        {
            if (_tools.TryGetValue(toolName, out var tool))
            {
                SelectedTool = tool;
            }
        }

        public T GetTool<T>() where T : ITool
        {
            return (T)_tools[typeof(T).Name];
        }

        public void SwitchToPreviousTool()
        {
            if (PreviousTool == null) return;
            SelectedTool = PreviousTool;
        }

        private void OnPress()
        {
            BrushSelected?.Invoke();

            if (CheckForUI()) return;
            if (!_container.DrawingEnabled) return;

            SelectedTool?.OnDown(GetPos());

            _pressed = true;
            SetRenderState(true);
        }

        private void OnRelease()
        {
            SetRenderState(false);

            if (!_pressed) return;

            SelectedTool?.OnUp(GetPos());

            _pressed = false;
        }

        private void Update()
        {
            if (_shouldRender)
            {
                SelectedTool?.Render();
            }
            
            if (_pressed)
            {
                SelectedTool?.OnUpdate(GetPos());
            }

            if (_time < 0.2f)
            {
                _time += Time.deltaTime;
                return;
            }

            // visual queue for when you are able to draw
            SetRenderState(!CheckForUI());
        }

        private Vector3 GetPos()
        {
            if (!_isFpfc) return _transform.position;
            return _transform.position + _transform.forward * 2;
        }

        private bool CheckForUI()
        {
            var pointerData = PointerEventDataAcc(ref _pointer);
            if (!(pointerData.pointerCurrentRaycast.gameObject is { } go)) return false;
            return go.GetComponent<UIBehaviour>() is { };
        }

        private void SetRenderState(bool shouldRender)
        {
            _shouldRender = shouldRender;
            MenuHandleActive = !shouldRender;
            PointerActive = !shouldRender;
        }

        public static void Install(DiContainer container)
        {
            container.BindFactory<GameObject, BrushBehaviour, Factory>().FromFactory<CustomFactory>();
        }

        private static readonly FieldAccessor<VRPointer, PointerEventData>.Accessor PointerEventDataAcc
            = FieldAccessor<VRPointer, PointerEventData>.GetAccessor("_pointerData");

        internal class Factory : PlaceholderFactory<GameObject, BrushBehaviour>{}

        internal class CustomFactory : IFactory<GameObject, BrushBehaviour>
        {
            private readonly DiContainer _container;

            public CustomFactory(DiContainer container)
            {
                _container = container;
            }

            public BrushBehaviour Create(GameObject go)
            {
                return _container.InstantiateComponent<BrushBehaviour>(go);
            }
        }

        internal class BrushBox
        {
            public CustomBrush CurrentBrush
            {
                get => _currentBrush;
                set
                {
                    if (_currentBrush != null && _currentBrush == value) return;
                    _currentBrush = value;
                    OnBrushChanged?.Invoke();
                }
            }

            public event Action OnBrushChanged;

            private CustomBrush _currentBrush;
        }
    }
}