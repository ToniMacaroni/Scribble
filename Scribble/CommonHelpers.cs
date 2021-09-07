using System;
using System.IO;
using System.Reflection;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scribble
{
    public static class CommonHelpers
    {
        public static Texture2D LoadTextureFromResources(string iconName, bool mipChain = true)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Scribble."+iconName);
            if (stream == null) return null;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            stream.Close();
            return TextureFromBytes(buffer, mipChain);
        }

        public static Sprite LoadSpriteFromResources(string iconName, bool mipChain = true)
        {
            Texture2D tex = LoadTextureFromResources(iconName, mipChain);
            if (tex == null) return null;
            return tex.ToSprite();
        }

        public static Sprite ToSprite(this Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0f, 0f), 100f);
        }

        public static void SetSkew(this ImageView image, float skew)
        {
            SkewAcc(ref image) = skew;
        }

        public static Texture2D LoadTextureFromFile(string filename, bool mipChain = true)
        {
            byte[] data = File.ReadAllBytes(filename);
            return TextureFromBytes(data, mipChain);
        }

        public static Texture2D TextureFromBytes(byte[] data, bool mipChain = true)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain);
            if (!tex.LoadImage(data)) return null;
            return tex;
        }

        public static string FilenameWithoutExtension(this FileInfo file)
        {
            return file.Name.Replace(file.Extension, "");
        }

        public static void Destroy(this Object obj)
        {
            Object.Destroy(obj);
        }

        public static Texture2D MakeSolidTexture(Color color)
        {
            Texture2D tex = new Texture2D(2, 2);
            var fillColorArray = tex.GetPixels();

            for (var i = 0; i < fillColorArray.Length; ++i)
            {
                fillColorArray[i] = color;
            }

            tex.SetPixels(fillColorArray);

            tex.Apply();
            return tex;
        }

        public static object Copy(this object original)
        {
            object instance = Activator.CreateInstance(original.GetType());
            foreach (PropertyInfo property in original.GetType().GetProperties())
            {
                property.SetValue(instance, property.GetValue(original));
            }

            return instance;
        }

        private static readonly FieldAccessor<ImageView, float>.Accessor SkewAcc =
            FieldAccessor<ImageView, float>.GetAccessor("_skew");
    }
}
