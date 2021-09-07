using BeatSaberMarkupLanguage.GameplaySetup;
using UnityEngine;

namespace Scribble.Tools
{
    internal class BucketTool : ITool, IBrushConsumer
    {
        public float BucketSize = 0.1f;

        private ScribbleContainer _container;
        private Vector3 _lastPos;

        private GameObject _brushMesh;
        private Material _brushMaterial;
        private BrushBehaviour.BrushBox _brushBox;

        private Color _brushColor;

        public void Init(GameObject brushMesh, ScribbleContainer scribbleContainer, SaberType saberType)
        {
            _brushMesh = brushMesh;
            _container = scribbleContainer;
            _brushMaterial = brushMesh.GetComponent<MeshRenderer>().material;

            ColorUtility.TryParseHtmlString("#ef42f5", out _brushColor);
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
            if (Vector3.Distance(_lastPos, position) > 0.1f)
            {
                _lastPos = position;
                _container.ChangeBrush(_lastPos, BucketSize, _brushBox.CurrentBrush);
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
            _brushMesh.transform.localScale = new Vector3(BucketSize, BucketSize, BucketSize);
            _brushMaterial.color = _brushColor;
            _brushMesh.SetActive(true);
        }

        public void SetBrushBox(BrushBehaviour.BrushBox brushBox)
        {
            _brushBox = brushBox;
        }
    }
}