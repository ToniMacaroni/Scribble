using System.Collections.Generic;
using UnityEngine;

namespace Scribble
{
    internal class PrimitiveMeshLoader
    {
        private readonly Dictionary<PrimitiveType, Mesh> _meshes = new Dictionary<PrimitiveType, Mesh>();
        
        public Mesh GetMesh(PrimitiveType type)
        {
            if (!_meshes.ContainsKey(type))
            {
                _meshes[type] = CreateMesh(type);
            }
            return _meshes[type];
        }
        
        private Mesh CreateMesh(PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(go);
            return mesh;
        }
    }
}