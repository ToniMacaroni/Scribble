using Scribble.Helpers;
using UnityEngine;

namespace Scribble.Tools
{
    internal class ScaleTool : ITool
    {
        private BrushMeshDrawer _brushDrawer;
        private ScribbleContainer _scribbleContainer;
        private SaberType _saberType;
        private Material _brushMaterial;
        private PluginConfig _config;

        private Vector3 _grabPoint;

        private Color _brushColor;
        
        private MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();
        
        public void Init(BrushMeshDrawer brushDrawer, ScribbleContainer scribbleContainer, PluginConfig config, SaberType saberType)
        {
            _brushDrawer = brushDrawer;
            _scribbleContainer = scribbleContainer;
            _saberType = saberType;
            _config = config;

            _brushMaterial = _brushDrawer.Material;

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
            _scribbleContainer.Scale((position - _grabPoint)*_config.ScaleToolMultiplier);
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
            _materialPropertyBlock.SetColor(MaterialPropertyHelper.PropColor, _brushColor);
        }
        
        public void Render()
        {
            _brushDrawer.DrawMesh(0.05f, _materialPropertyBlock);
        }
    }
}