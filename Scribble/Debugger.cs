using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scribble
{
    public static class Debugger
    {
        public static void DumpShader(Shader shader)
        {
            Plugin.Log.Debug("Shader: " + shader.name);
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                Plugin.Log.Debug("\tProp: " + shader.GetPropertyName(i) + " : " + shader.GetPropertyType(i));
            }
        }

        public static void DumpShaders()
        {
            foreach (Shader shader in Resources.FindObjectsOfTypeAll<Shader>())
            {
                DumpShader(shader);
            }
        }

        public static void DumpMaterial(Material material)
        {
            Plugin.Log.Debug("Material: "+material.name+" ("+material.shader.name+")");
        }

        public static void DumpMaterials()
        {
            foreach (Material material in Resources.FindObjectsOfTypeAll<Material>())
            {
                DumpMaterial(material);
            }
        }

        public static GameObject CreatePoint(Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
            return go;
        }

        public static void PrintChildren(Transform t, string parentname)
        {
            var name = parentname + "/" + t.name;
            var hasMesh = t.gameObject.GetComponent<MeshRenderer>() != null;
            Plugin.Log.Debug(name + ":"+hasMesh);
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                PrintChildren(child, name);
            }
        }
    }
}
