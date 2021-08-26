using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scribble
{
    internal class BrushTextures
    {
        public static List<BrushTexture> Textures = new List<BrushTexture>();

        public static void LoadTextures()
        {
            LoadTexturesFromResources();
            LoadTexturesFromDirectory();
        }

        public static void LoadTexturesFromResources()
        {
            AddTexture("standard", Tools.LoadTextureFromResources("Textures.standard.png", false));

            AddTexture("pogchamp", Tools.LoadTextureFromResources("Textures.pogchamp.png"));
            AddTexture("brush", Tools.LoadTextureFromResources("Textures.brush.png"));
        }

        public static void LoadTexturesFromDirectory()
        {
            var dir = new DirectoryInfo("UserData\\Scribble\\Textures");
            if (!dir.Exists)
            {
                dir.Create();
                return;
            }

            foreach (FileInfo fileInfo in dir.GetFiles("*.png"))
            {
                string name = fileInfo.FilenameWithoutExtension();
                Texture2D tex = Tools.LoadTextureFromFile(fileInfo.FullName);
                AddTexture(name, tex);
            }
        }

        private static void AddTexture(string name, Texture2D tex)
        {
            tex.wrapMode = TextureWrapMode.Repeat;
            Textures.Add(new BrushTexture(name, tex));
        }

        public static Texture2D GetTexture(string name)
        {
            if (string.Equals(name, "standard", StringComparison.OrdinalIgnoreCase)) return null;
            return Textures.FirstOrDefault(tex => tex.Name == name)?.Texture;
        }

        public class BrushTexture
        {
            public string Name;
            public Texture2D Texture;

            public BrushTexture(string name, Texture2D texture)
            {
                Name = name;
                Texture = texture;
            }
        }
    }
}
