using UnityEngine;

namespace Scribble.Tools
{
    internal class Eraser : ITool
    {
        public float EraserSize = 0.1f;

        private ScribbleContainer _container;
        private Vector3 _lastErasePosition;

        private GameObject _brushMesh;
        private Material _brushMaterial;

        public void Init(GameObject brushMesh, ScribbleContainer container, SaberType saberType)
        {
            _brushMesh = brushMesh;
            _container = container;
            _brushMaterial = brushMesh.GetComponent<MeshRenderer>().material;
        }

        public void OnSelected()
        {
            UpdateBrushMesh();
        }

        public void OnDown(Vector3 _)
        {
            UpdateBrushMesh();
        }

        public void OnUpdate(Vector3 position)
        {
            if (Vector3.Distance(_lastErasePosition, position) > 0.1f)
            {
                _lastErasePosition = position;
                _container.Erase(_lastErasePosition, EraserSize);
            }
        }

        public void OnUp(Vector3 _)
        {
            _brushMesh.SetActive(false);
        }

        public void OnDeselected()
        {

        }

        public void UpdateBrushMesh()
        {
            _brushMesh.transform.localScale = new Vector3(EraserSize, EraserSize, EraserSize);
            _brushMaterial.color = new Color(0.98f, 0.15f, 0.01f);
            _brushMesh.SetActive(true);
        }
    }
}
