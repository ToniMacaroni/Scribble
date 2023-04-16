using Scribble.Helpers;
using UnityEngine;

namespace Scribble.Tools
{
    internal class DrawingTool : ITool, IBrushConsumer
    {
        private const float MinDistance = 0.01f;

        private Vector3 _lastPoint;
        private BrushMeshDrawer _brushDrawer;
        private Material _brushMaterial;
        private ScribbleContainer _scribbleContainer;
        private SaberType _saberType;
        private BrushBehaviour.BrushBox _brushBox;
        
        private MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();

        private bool _isSelected;

        public void Init(BrushMeshDrawer brushDrawer, ScribbleContainer scribbleContainer, PluginConfig config, SaberType saberType)
        {
            _brushDrawer = brushDrawer;
            _scribbleContainer = scribbleContainer;
            _saberType = saberType;

            _brushMaterial = _brushDrawer.Material;
        }

        public void OnSelected()
        {
            _isSelected = true;
            UpdateBrushMesh();
        }

        public void OnDown(Vector3 pos)
        {
            _lastPoint = pos;
            _scribbleContainer.InitPoint(_lastPoint, _saberType, _brushBox.CurrentBrush);
            UpdateBrushMesh();
        }

        public void OnUpdate(Vector3 pos)
        {
            var distance = Vector3.Distance(_lastPoint, pos);
            if (distance > MinDistance)
            {
                _scribbleContainer.AddPoint(pos, _saberType);
                _lastPoint = pos;
            }
        }

        public void OnUp(Vector3 pos)
        {
            _scribbleContainer.CheckLine(_saberType);
        }

        public void UpdateBrushMesh()
        {
            _materialPropertyBlock.SetColor(MaterialPropertyHelper.PropColor, _brushBox.CurrentBrush.Color);
        }

        public void OnDeselected()
        {
            _isSelected = false;
        }

        public void SetBrushBox(BrushBehaviour.BrushBox brushBox)
        {
            _brushBox = brushBox;
            _brushBox.OnBrushChanged += OnBrushChanged;
        }

        private void OnBrushChanged()
        {
            if(_isSelected) UpdateBrushMesh();
        }

        public void Render()
        {
            _brushDrawer.DrawMesh(ScribbleContainer.LineWidth * _brushBox.CurrentBrush.Size, _materialPropertyBlock);
        }
    }
}