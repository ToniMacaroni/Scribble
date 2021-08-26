using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scribble
{
    public static class ThumbnailHelper
    {
        public static byte[] GetThumbnail(Camera camera, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.Destroy(rt);
            return screenShot.EncodeToPNG();
        }

        public static Camera CreateCamera(Vector3 position, Vector3 rotation)
        {
            //GameObject go = new GameObject("ThumbnailCam");
            GameObject go = GameObject.Instantiate(Camera.main.gameObject);
            go.SetActive(false);
            while (go.transform.childCount > 0)
            {
                Object.DestroyImmediate(go.transform.GetChild(0).gameObject);
            }
            Object.DestroyImmediate(go.GetComponent("AudioListener"));
            Object.DestroyImmediate(go.GetComponent("MeshCollider"));
            var camera = go.GetComponent<Camera>();
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.cullingMask = 1 << 30;
            camera.fieldOfView = 60f;
            camera.enabled = true;
            LIV.SDK.Unity.LIV liv = camera.GetComponent<LIV.SDK.Unity.LIV>();
            if (liv) Object.Destroy(liv);
            go.SetActive(true);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(rotation);
            return go.GetComponent<Camera>();
        }

        public static bool CheckPngData(Stream stream, out long size, bool skip)
        {
            size = 0;
            if (stream == null)
            {
                return false;
            }
            long position = stream.Position;
            try
            {
                byte[] headerBuffer = new byte[8];
                byte[] pngHeader = {137,80,78,71,13,10,26,10};
                stream.Read(headerBuffer, 0, 8);
                for (int i = 0; i < 8; i++)
                {
                    if (headerBuffer[i] != pngHeader[i])
                    {
                        stream.Seek(position, SeekOrigin.Begin);
                        return false;
                    }
                }
                bool flag = true;
                while (flag)
                {
                    byte[] array3 = new byte[4];
                    stream.Read(array3, 0, 4);
                    Array.Reverse(array3);
                    int num = BitConverter.ToInt32(array3, 0);
                    byte[] array4 = new byte[4];
                    stream.Read(array4, 0, 4);
                    int num2 = BitConverter.ToInt32(array4, 0);
                    if (num2 == 1145980233)
                    {
                        flag = false;
                    }
                    if (num + 4 > stream.Length - stream.Position)
                    {
                        stream.Seek(position, SeekOrigin.Begin);
                        return false;
                    }
                    stream.Seek(num + 4, SeekOrigin.Current);
                }
                size = stream.Position - position;
                if (!skip)
                {
                    stream.Seek(position, SeekOrigin.Begin);
                }
            }
            catch (EndOfStreamException ex)
            {
                Plugin.Log.Debug(ex.Message);
                stream.Seek(position, SeekOrigin.Begin);
                return false;
            }
            return true;
        }
    }
}
