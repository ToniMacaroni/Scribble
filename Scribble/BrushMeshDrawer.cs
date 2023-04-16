using UnityEngine;

namespace Scribble
{
    internal class BrushMeshDrawer
    {
        private const string BrushMaterialName = "mat_brush";

        public Mesh Mesh
        {
            get => _mesh;
            set => _mesh = value;
        }

        public Material Material
        {
            get => _material;
            set => _material = value;
        }

        public Transform Parent
        {
            get => _parent;
            set => _parent = value;
        }
        
        public float SizeMultiplier { get; set; } = 0.5f;
        
        public BrushMeshDrawer(AssetLoader assetLoader, PrimitiveMeshLoader primitiveMeshLoader, PluginConfig pluginConfig)
        {
            _pluginConfig = pluginConfig;
            _mesh = primitiveMeshLoader.GetMesh(PrimitiveType.Plane);
            _material = assetLoader.LoadAsset<Material>(BrushMaterialName);
            
            _material.SetFloat(Clip, 0.3589f);
            _material.SetColor(SecondaryColor, new Color(1,1,1,0.5f));
        }

        public void DrawMesh(Vector3 pos, Quaternion rotation, float scale, MaterialPropertyBlock propertyBlock)
        {
            if (_pluginConfig.FPFCEnabled)
                return;
            
            Graphics.DrawMesh(_mesh, Matrix4x4.TRS(pos, rotation, Vector3.one * scale), _material, 0, null, 0, propertyBlock);
        }

        public void DrawMesh(float scale, MaterialPropertyBlock propertyBlock)
        {
            DrawMesh(_parent.position, MeshRotation, scale * SizeMultiplier, propertyBlock);
        }

        public void SetTransform(Transform transform)
        {
            _parent = transform;
        }

        private Mesh _mesh;
        private Material _material;
        private Transform _parent;
        
        private readonly PluginConfig _pluginConfig;

        private static readonly Quaternion MeshRotation = Quaternion.Euler(-90, 0, 0);
        private static readonly int Clip = Shader.PropertyToID("_Clip");
        private static readonly int SecondaryColor = Shader.PropertyToID("_SecondaryColor");
    }
}