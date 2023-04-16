using BeatSaberMarkupLanguage.GameplaySetup;
using Scribble.Helpers;
using UnityEngine;

namespace Scribble.Tools
{
    internal class BucketTool : ITool, IBrushConsumer
    {
        public float BucketSize = 0.1f;

        private ScribbleContainer _container;
        private Vector3 _lastPos;

        private BrushMeshDrawer _brushDrawer;
        private Material _brushMaterial;
        private BrushBehaviour.BrushBox _brushBox;

        private Color _brushColor;

        private bool _shouldRender;
        
        MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();

        public void Init(BrushMeshDrawer brushDrawer, ScribbleContainer scribbleContainer, PluginConfig config, SaberType saberType)
        {
            _brushDrawer = brushDrawer;
            _container = scribbleContainer;
            _brushMaterial = brushDrawer.Material;

            ColorUtility.TryParseHtmlString("#ef42f5", out _brushColor);
        }

        public void OnSelected()
        {
            UpdateBrushMesh();
        }

        public void OnDown(Vector3 _)
        {
            UpdateBrushMesh();
            _shouldRender = true;
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
            _shouldRender = false;
        }

        public void OnDeselected()
        {
        }

        public void UpdateBrushMesh()
        {
            _materialPropertyBlock.SetColor(MaterialPropertyHelper.PropColor, _brushColor);
        }

        public void SetBrushBox(BrushBehaviour.BrushBox brushBox)
        {
            _brushBox = brushBox;
        }
        
        public void Render()
        {
            if(!_shouldRender)
                return;
            _brushDrawer.DrawMesh(BucketSize, _materialPropertyBlock);
        }
    }
}