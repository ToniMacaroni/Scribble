using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Scribble
{
    internal class ScribbleContainer : MonoBehaviour
    {
        public static ScribbleContainer Instance;

        public static bool DrawingEnabled = true;

        public static float LineWidth = 0.001f;

        public LineRenderer CurrentLineRendererLeft;
        public LineRenderer CurrentLineRendererRight;
        private readonly List<LinerendererData> _lineRenderers = new List<LinerendererData>();

        private readonly int _minPositionCount = 2;

        private Coroutine _animatedLoadRoutine;

        public bool IsInAnimation => _animatedLoadRoutine != null;

        public static void Create()
        {
            if (Instance) return;
            var container = new GameObject("ScribbleContainer").AddComponent<ScribbleContainer>();
            DontDestroyOnLoad(container);
        }

        void Start()
        {
            Instance = this;

            Plugin.Log.Debug("Scribble Container Initialized");

            UpdateMaterials(1);

            if (Plugin.FirstTimeLaunch)
            {
                LoadAnimated("first", 0.004f, true);
            }
        }

        LineRenderer InitLineRenderer(CustomBrush brush, bool disableOnStart = true)
        {
            GameObject go = new GameObject("LineRenderer-"+_lineRenderers.Count);
            go.transform.SetParent(transform);
            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.enabled = !disableOnStart;
            lineRenderer.widthMultiplier = brush.Size*LineWidth;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            if (brush.TextureMode == CustomBrush.TextureModes.Stretch)
                lineRenderer.textureMode = LineTextureMode.Stretch;
            else if (brush.TextureMode == CustomBrush.TextureModes.Tile)
                lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.material = brush.CreateMaterial();

            _lineRenderers.Add(new LinerendererData(lineRenderer, (CustomBrush)brush.Copy()));
            return lineRenderer;
        }

        public void UpdateMaterials(float bps)
        {
            Shader.SetGlobalFloat("Bpm", bps);
        }

        public void InitPoint(Vector3 point, SaberType saberType, CustomBrush brush)
        {
            var lineRenderer = InitLineRenderer(brush);
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, point);
            lineRenderer.enabled = true;
            if (saberType == SaberType.SaberA) CurrentLineRendererLeft = lineRenderer;
            else CurrentLineRendererRight = lineRenderer;
        }

        public void AddPoint(Vector3 point, SaberType saberType)
        {
            var lineRenderer = saberType==SaberType.SaberA?CurrentLineRendererLeft:CurrentLineRendererRight;
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, point);
        }

        public List<Vector3> GetAllPoints()
        {
            List<Vector3> result = new List<Vector3>();
            foreach (LinerendererData data in _lineRenderers)
            {
                Vector3[] positions = new Vector3[data.LineRenderer.positionCount];
                data.LineRenderer.GetPositions(positions);
                result.AddRange(positions);
            }

            return result;
        }

        public void Erase(Vector3 position, float size)
        {
            for (var index = 0; index < _lineRenderers.Count; index++)
            {
                LinerendererData lineRendererData = _lineRenderers[index];
                Vector3[] positions = new Vector3[lineRendererData.LineRenderer.positionCount];
                lineRendererData.LineRenderer.GetPositions(positions);
                for (var i = 0; i < positions.Length; i++)
                {
                    Vector3 point = positions[i];
                    if (Vector3.Distance(position, point) < size)
                    {
                        Delete(lineRendererData);
                        break;
                    }
                }
            }
        }

        public void LoadLineRenderer(SaveSystem.SerializableLineRendererData data, float zOffset = 0)
        {
            var lineRenderer = InitLineRenderer(data.Brush.ToCustomBrush());
            lineRenderer.positionCount = data.Positions.Length;
            for (int i = 0; i < data.Positions.Length; i++)
            {
                var pos = data.Positions[i].ToVector3();
                pos.z += zOffset;
                lineRenderer.SetPosition(i, pos);
            }

            lineRenderer.enabled = true;
        }

        public void Clear()
        {
            foreach (LinerendererData data in _lineRenderers)
            {
                Destroy(data.LineRenderer);
            }
            _lineRenderers.Clear();
        }

        public void Undo()
        {
            Delete(_lineRenderers.Count-1);
        }

        public void Delete(int index)
        {
            if (index > _lineRenderers.Count - 1) return;
            LineRenderer lr = _lineRenderers[index].LineRenderer;
            _lineRenderers.RemoveAt(index);
            Destroy(lr.gameObject);
        }

        public void Delete(LinerendererData data)
        {
            LineRenderer lr = data.LineRenderer;
            var b = _lineRenderers.Remove(data);
            Destroy(lr);
        }

        public void Save(FileInfo file)
        {
            SetLayer(30);
            SaveSystem.SavePng(file, new SaveSystem.DrawingData(_lineRenderers));
            SetLayer(0);
        }

        private void SetLayer(int layer)
        {
            foreach (var linerendererData in _lineRenderers)
            {
                linerendererData.LineRenderer.gameObject.layer = layer;
            }
        }

        public void Load(FileInfo file)
        {
            var data = SaveSystem.LoadPng(file);
            if(data==null)return;
            foreach (SaveSystem.SerializableLineRendererData lineRendererData in data.LineRendererData)
            {
                LoadLineRenderer(lineRendererData);
            }
        }

        public void LoadFromResource(string resourceName, bool replace = true, float zOffset = 0)
        {
            var data = SaveSystem.LoadPngFromResource(resourceName);
            if (data == null) return;
            if (replace) Clear();
            foreach (SaveSystem.SerializableLineRendererData lineRendererData in data.LineRendererData)
            {
                LoadLineRenderer(lineRendererData, zOffset);
            }
        }

        public void LoadAnimated(string resourceName, float delay, bool replace, float zOffset = 0)
        {
            _animatedLoadRoutine = StartCoroutine(LoadAnimatedCoroutine(resourceName, delay, replace, zOffset));
        }

        IEnumerator LoadAnimatedCoroutine(string resourceName, float delay, bool replace, float zOffset = 0)
        {
            var data = SaveSystem.LoadPngFromResource(resourceName);
            if (data == null) yield break;
            if (replace) Clear();

            foreach (SaveSystem.SerializableLineRendererData lineRendererData in data.LineRendererData)
            {
                var lineRenderer = InitLineRenderer(lineRendererData.Brush.ToCustomBrush(), false);
                lineRenderer.SetPosition(0, lineRendererData.Positions[0].ToVector3() + new Vector3(0, 0, zOffset));
                lineRenderer.SetPosition(1, lineRendererData.Positions[1].ToVector3() + new Vector3(0, 0, zOffset));
                for (int i = 2; i < lineRendererData.Positions.Length;i++)
                {
                    var pos = lineRendererData.Positions[i].ToVector3();
                    pos.z += zOffset;
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(i, pos);
                    var multiplier = i / (float)lineRendererData.Positions.Length;
                    yield return new WaitForSeconds(delay * multiplier);
                }
            }

            _animatedLoadRoutine = null;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void CheckLine(SaberType saberType)
        {
            var lineRenderer = saberType == SaberType.SaberA ? CurrentLineRendererLeft : CurrentLineRendererRight;
            if (lineRenderer.positionCount <= _minPositionCount)
            {
                Undo();
            }
        }

        public void OnGameStarted()
        {
            BeatmapObjectSpawnController spawnController = FindObjectOfType<BeatmapObjectSpawnController>();
            UpdateMaterials(spawnController.currentBpm / 60);
        }

        internal class LinerendererData
        {
            public LineRenderer LineRenderer;
            public CustomBrush Brush;

            public LinerendererData(LineRenderer lineRenderer, CustomBrush brush)
            {
                LineRenderer = lineRenderer;
                Brush = brush;
            }
        }
    }
}
