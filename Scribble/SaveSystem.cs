using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Scribble
{
    internal class SaveSystem
    {
        private readonly PluginConfig _config;
        public const int Version = 1;

        public DirectoryInfo SaveDirectory;

        public SaveSystem(PluginDirectories dirs, PluginConfig config)
        {
            _config = config;
            SaveDirectory = dirs.DrawingsDir;
        }

        public DrawingData LoadPng(FileInfo file)
        {
            if (!file.Exists) return null;
            BinaryReader reader = new BinaryReader(file.OpenRead());
            if (!ThumbnailHelper.CheckPngData(reader.BaseStream, out var size, true)) return null;
            return Load(reader);
        }

        public DrawingData LoadPngFromResource(string name)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Scribble.Drawings." + name + ".png");
            if (stream == null) return null;
            BinaryReader reader = new BinaryReader(stream);
            if (!ThumbnailHelper.CheckPngData(reader.BaseStream, out var size, true)) return null;
            return Load(reader);
        }

        public void SavePng(FileInfo file, DrawingData data)
        {
            Camera cam = ThumbnailHelper.CreateCamera(new Vector3(1.3676883f, 1.674314f, -2.518276f), new Vector3(12.04695f, 341.8893f, 3.806141f));
            byte[] pngData = ThumbnailHelper.GetThumbnail(cam, _config.ThumbnailSize, _config.ThumbnailSize);
            cam.gameObject.Destroy();
            file.Directory.Create();
            BinaryWriter writer = new BinaryWriter(file.OpenWrite());
            writer.Write(pngData);
            Save(writer, data);
        }

        public void Save(BinaryWriter writer, DrawingData data)
        {
            try
            {
                writer.Write(Version);

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(writer.BaseStream, data);
                writer.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public DrawingData LoadFromFile(FileInfo file)
        {
            if (!file.Exists) return null;
            return Load(new BinaryReader(file.OpenRead()));
        }

        public DrawingData LoadFromResource(string name)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream == null) return null;
            return Load(new BinaryReader(stream));
        }

        public DrawingData Load(BinaryReader reader)
        {
            try
            {
                int version = reader.ReadInt32();
                if (version != Version)
                {
                    Debug.LogError("Trying to load drawing with another version");
                    return null;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                DrawingData result = (DrawingData)formatter.Deserialize(reader.BaseStream);
                reader.Close();
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return null;
        }

        [Serializable]
        internal class DrawingData
        {
            public SerializableLineRendererData[] LineRendererData;

            public DrawingData(SerializableLineRendererData[] lineRendererData)
            {
                LineRendererData = lineRendererData;
            }

            public DrawingData(List<ScribbleContainer.LinerendererData> linerendererData)
            {
                LineRendererData = new SerializableLineRendererData[linerendererData.Count];
                for (int i = 0; i < LineRendererData.Length; i++)
                {
                    LineRendererData[i] = new SerializableLineRendererData(linerendererData[i]);
                }
            }
        }

        [Serializable]
        internal class SerializableLineRendererData
        {
            public SerializableVector3[] Positions;
            public SerializableBrush Brush;

            public SerializableLineRendererData(SerializableVector3[] positions, SerializableBrush brush)
            {
                Positions = positions;
                Brush = brush;
            }

            public SerializableLineRendererData(ScribbleContainer.LinerendererData data)
            {
                Vector3[] positions = new Vector3[data.LineRenderer.positionCount];
                var transform = data.LineRenderer.transform;
                data.LineRenderer.GetPositions(positions);
                Positions = new SerializableVector3[positions.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    Positions[i] = new SerializableVector3(transform.localToWorldMatrix.MultiplyPoint(positions[i]));
                }
                Brush = new SerializableBrush(data.Brush);
            }
        }

        [Serializable]
        internal class SerializableBrush
        {
            public string ColorString;
            public string TextureName;
            public string EffectName;
            public float Size;
            public float Glow;
            public CustomBrush.TextureModes TextureMode;
            public SerializableVector2 Tiling;

            public SerializableBrush(CustomBrush brush)
            {
                ColorString = brush.ColorString;
                TextureName = brush.TextureName;
                EffectName = brush.EffectName;
                Size = brush.Size;
                Glow = brush.Glow;
                TextureMode = brush.TextureMode;
                Tiling = new SerializableVector2(brush.Tiling);
            }

            public CustomBrush ToCustomBrush()
            {
                CustomBrush brush = new CustomBrush();
                brush.ColorString = ColorString;
                brush.TextureName = TextureName;
                brush.EffectName = EffectName;
                brush.Size = Size;
                brush.Glow = Glow;
                brush.TextureMode = TextureMode;
                brush.Tiling = Tiling.ToVector2();

                return brush;
            }
        }

        [Serializable]
        internal class SerializableVector2
        {
            public float X;
            public float Y;

            public SerializableVector2(float x, float y)
            {
                X = x;
                Y = y;
            }

            public SerializableVector2(Vector2 input) : this(input.x, input.y)
            {
            }

            public Vector2 ToVector2()
            {
                return new Vector2(X, Y);
            }
        }

        [Serializable]
        internal class SerializableVector3
        {
            public float X;
            public float Y;
            public float Z;

            public SerializableVector3(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public SerializableVector3(Vector3 input) : this(input.x, input.y, input.z)
            {
            }

            public Vector3 ToVector3()
            {
                return new Vector3(X, Y, Z);
            }
        }
    }
}
