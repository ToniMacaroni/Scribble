using UnityEngine;

namespace Scribble.Tools
{
    internal class MoveTool : ITool
    {
        private GameObject _brushMesh;
        private ScribbleContainer _scribbleContainer;
        private SaberType _saberType;
        private Material _brushMaterial;

        private Vector3 _grabPoint;

        private Color _brushColor;

        public void Init(GameObject brushMesh, ScribbleContainer scribbleContainer, SaberType saberType)
        {
            _brushMesh = brushMesh;
            _scribbleContainer = scribbleContainer;
            _saberType = saberType;

            _brushMaterial = _brushMesh.GetComponent<MeshRenderer>().material;

            ColorUtility.TryParseHtmlString("#429ef5", out _brushColor);
        }

        public void OnSelected()
        {
            UpdateBrushMesh();
        }

        public void OnDown(Vector3 position)
        {
            _grabPoint = position;
        }

        public void OnUpdate(Vector3 position)
        {
            _scribbleContainer.Move(position - _grabPoint);
            _grabPoint = position;
        }

        public void OnUp(Vector3 position)
        {
        }

        public void OnDeselected()
        {
        }

        public void UpdateBrushMesh()
        {
            float size = 0.05f;
            _brushMesh.transform.localScale = new Vector3(size, size, size);
            _brushMaterial.color = _brushColor;
        }
    }
}