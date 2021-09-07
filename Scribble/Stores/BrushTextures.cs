using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Scribble.Stores
{
    internal class BrushTextures : IInitializable
    {
        public List<BrushTexture> Textures = new List<BrushTexture>();

        public void Initialize()
        {
            LoadTextures();
        }

        public void LoadTextures()
        {
            LoadTexturesFromResources();
            LoadTexturesFromDirectory();
        }

        public void LoadTexturesFromResources()
        {
            AddTexture("standard", CommonHelpers.LoadTextureFromResources("Textures.standard.png", false));

            AddTexture("pogchamp", CommonHelpers.LoadTextureFromResources("Textures.pogchamp.png"));
            AddTexture("brush", CommonHelpers.LoadTextureFromResources("Textures.brush.png"));
        }

        public void LoadTexturesFromDirectory()
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
                Texture2D tex = CommonHelpers.LoadTextureFromFile(fileInfo.FullName);
                AddTexture(name, tex);
            }
        }

        private void AddTexture(string name, Texture2D tex)
        {
            tex.wrapMode = TextureWrapMode.Repeat;
            Textures.Add(new BrushTexture(name, tex));
        }

        public Texture2D GetTexture(string name)
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
