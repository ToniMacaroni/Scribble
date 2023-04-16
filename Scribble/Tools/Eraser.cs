using Scribble.Helpers;
using UnityEngine;

namespace Scribble.Tools
{
    internal class Eraser : ITool
    {
        public float EraserSize = 0.1f;

        private ScribbleContainer _container;
        private Vector3 _lastErasePosition;

        private BrushMeshDrawer _brushDrawer;
        private Material _brushMaterial;
        
        private bool _shouldRender;
        
        MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();

        public void Init(BrushMeshDrawer brushDrawer, ScribbleContainer scribbleContainer, PluginConfig config, SaberType saberType)
        {
            _brushDrawer = brushDrawer;
            _container = scribbleContainer;
            _brushMaterial = brushDrawer.Material;
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
            if (Vector3.Distance(_lastErasePosition, position) > 0.1f)
            {
                _lastErasePosition = position;
                _container.Erase(_lastErasePosition, EraserSize);
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
            _materialPropertyBlock.SetColor(MaterialPropertyHelper.PropColor, new Color(0,0,0,0));
        }

        public void Render()
        {
            if(!_shouldRender)
                return;
            _brushDrawer.DrawMesh(EraserSize, _materialPropertyBlock);
        }
    }
}
