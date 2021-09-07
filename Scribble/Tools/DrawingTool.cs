using UnityEngine;

namespace Scribble.Tools
{
    internal class DrawingTool : ITool, IBrushConsumer
    {
        private const float MinDistance = 0.01f;

        private Vector3 _lastPoint;
        private GameObject _brushMesh;
        private Material _brushMaterial;
        private ScribbleContainer _scribbleContainer;
        private SaberType _saberType;
        private BrushBehaviour.BrushBox _brushBox;

        private bool _isSelected;

        public void Init(GameObject brushMesh, ScribbleContainer scribbleContainer, SaberType saberType)
        {
            _brushMesh = brushMesh;
            _scribbleContainer = scribbleContainer;
            _saberType = saberType;

            _brushMaterial = _brushMesh.GetComponent<MeshRenderer>().material;
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
            float size = ScribbleContainer.LineWidth * _brushBox.CurrentBrush.Size;
            _brushMesh.transform.localScale = new Vector3(size, size, size);
            _brushMaterial.color = _brushBox.CurrentBrush.Color;
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
    }
}