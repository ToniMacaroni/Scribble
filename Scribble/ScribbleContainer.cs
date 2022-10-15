using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HarmonyLib;
using Scribble.Helpers;
using Scribble.Installers;
using Scribble.Stores;
using SiraUtil.Logging;
using SiraUtil.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Scribble
{
    internal class ScribbleContainer : MonoBehaviour, IDisposable
    {
        public const float LineWidth = 0.001f;
        private const int MinPositionCount = 2;

        public bool DrawingEnabled = true;

        public event Action<bool> OnAnimationStateChanged;

        public bool StopAnimation { get; set; } = false;

        public LineRenderer CurrentLineRendererLeft;
        public LineRenderer CurrentLineRendererRight;
        private readonly List<LinerendererData> _lineRenderers = new List<LinerendererData>();

        private Coroutine _animatedLoadRoutine;
        private SiraLog _logger;
        private EffectStore _effects;
        private SaveSystem _saveSystem;
        private PluginConfig _config;
        private Scene _menuScene;
        private FPSCounter _fpsCounter;

        public bool IsInAnimation => _animatedLoadRoutine != null;

        [Inject]
        private void Construct(SiraLog logger, EffectStore effects, SaveSystem saveSystem, PluginConfig config)
        {
            _logger = logger;
            _effects = effects;
            _saveSystem = saveSystem;
            _config = config;

            if (GetComponent<FPSCounter>() is { } counter) _fpsCounter = counter;
            else _fpsCounter = gameObject.AddComponent<FPSCounter>();
        }

        public void InitMenu(Scene menuScene)
        {
            _menuScene = menuScene;

            SetContext(_config.VisibleDuringPlay);

            //MakeObject(@"C:\Users\Julian\Desktop\LineArt.obj");
            // LoadBundle();
            // return;

            if (_config.FirstTimeLaunch)
            {
                LoadAnimated("first", 0.004f, true);
            }
            else if (!string.IsNullOrEmpty(_config.AutoLoadDrawing))
            {
                var file = _saveSystem.SaveDirectory.GetFile(_config.AutoLoadDrawing);
                if (!file.Exists) return;
                Load(file);
            }
        }

        public void LoadBundle()
        {
            var data = File.ReadAllBytes("/home/julian/Desktop/Bundle/BSData");
            var bundle = AssetBundle.LoadFromMemory(data);
            var prefab = bundle.LoadAsset<GameObject>("MatSphere");
            var go = Instantiate(prefab);
            go.transform.position = new Vector3(0, 1, 0);
            DontDestroyOnLoad(go);
            Debug.Log("Loaded bundle");
        }

        public void SetContext(bool showInGame)
        {
            if (showInGame)
            {
                DontDestroyOnLoad(gameObject);
                return;
            }

            SceneManager.MoveGameObjectToScene(gameObject, _menuScene);
        }

        void Start()
        {
            _logger.Info("Scribble Container initialized");
            UpdateMaterials(1);
        }

        LineRenderer InitLineRenderer(CustomBrush brush, bool disableOnStart = true)
        {
            GameObject go = new GameObject("LineRenderer-" + _lineRenderers.Count);
            go.transform.SetParent(transform);
            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.enabled = !disableOnStart;
            lineRenderer.useWorldSpace = false;
            lineRenderer.widthMultiplier = brush.Size * LineWidth;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            if (brush.TextureMode == CustomBrush.TextureModes.Stretch)
                lineRenderer.textureMode = LineTextureMode.Stretch;
            else if (brush.TextureMode == CustomBrush.TextureModes.Tile)
                lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.material = brush.CreateMaterial(_effects);

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
            var lineRenderer = saberType == SaberType.SaberA ? CurrentLineRendererLeft : CurrentLineRendererRight;
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

        public void SetPositioning(bool worldSpace)
        {
            foreach (var lineRenderer in _lineRenderers)
            {
                lineRenderer.LineRenderer.useWorldSpace = worldSpace;
            }
        }

        public void Erase(Vector3 position, float size)
        {
            for (var index = 0; index < _lineRenderers.Count; index++)
            {
                LinerendererData lineRendererData = _lineRenderers[index];
                Vector3[] positions = new Vector3[lineRendererData.LineRenderer.positionCount];
                lineRendererData.LineRenderer.GetPositions(positions);
                var relPos = lineRendererData.LineRenderer.transform.position;
                for (var i = 0; i < positions.Length; i++)
                {
                    Vector3 point = positions[i] + relPos;
                    if (Vector3.Distance(position, point) < size)
                    {
                        Delete(lineRendererData);
                        break;
                    }
                }
            }
        }

        public void ChangeBrush(Vector3 position, float size, CustomBrush brush)
        {
            for (var index = 0; index < _lineRenderers.Count; index++)
            {
                LinerendererData lineRendererData = _lineRenderers[index];
                Vector3[] positions = new Vector3[lineRendererData.LineRenderer.positionCount];
                lineRendererData.LineRenderer.GetPositions(positions);
                var relPos = lineRendererData.LineRenderer.transform.position;
                for (var i = 0; i < positions.Length; i++)
                {
                    Vector3 point = positions[i] + relPos;
                    if (Vector3.Distance(position, point) < size)
                    {
                        lineRendererData.Brush = brush;
                        ChangeBrush(lineRendererData.LineRenderer, brush);
                        break;
                    }
                }
            }
        }

        public void ChangeBrush(LineRenderer data, CustomBrush brush)
        {
            data.widthMultiplier = brush.Size * LineWidth;
            if (brush.TextureMode == CustomBrush.TextureModes.Stretch)
                data.textureMode = LineTextureMode.Stretch;
            else if (brush.TextureMode == CustomBrush.TextureModes.Tile)
                data.textureMode = LineTextureMode.Tile;
            data.material = brush.CreateMaterial(_effects);
        }

        public void Move(Vector3 pos)
        {
            //transform.position = pos;
            foreach (Transform child in transform)
            {
                child.position += pos;
            }
        }

        // sus model import
        public async void MakeObject(string fileName)
        {
            var data = _saveSystem.LoadPngFromResource("first");
            var brush = data.LineRendererData[0].Brush.ToCustomBrush();
            brush.ColorString = "#34ebd2";
            brush.Glow = 0.8f;
            brush.Size = 3;

            // brush.ColorString = "#34ebd2";
            // brush.Glow = 0;
            // brush.Size = 23;
            // brush.EffectName = "DotBPM";

            var str = File.ReadAllText(fileName);

            var pts = new List<Vector3>();

            var lines = str.Split('\n');

            var multi = 0.65f;

            for (int i = 0; i < lines.Length; i++)
            {
                var l = lines[i];
                if (l.Length < 2 || l[0] != 'v' || l[1] != ' ')
                {
                    if (pts.Count < 1) continue;
                    await AddLineRendererAnim(pts, brush);
                    pts.Clear();
                    continue;
                }

                var strPts = l.Split(' ');
                if (strPts.Length < 4) continue;
                var pt = new Vector3(float.Parse(strPts[1]) * multi, float.Parse(strPts[2]) * multi, float.Parse(strPts[3]) * multi);
                if (pts.Count > 1 && Vector3.Distance(pt, pts[pts.Count - 1]) > 0.1f)
                {
                    await AddLineRendererAnim(pts, brush);
                    pts.Clear();
                    continue;
                }
                pts.Add(pt);
            }
        }

        public void AddLineRenderer(List<Vector3> points, CustomBrush brush)
        {
            var lineRenderer = InitLineRenderer(brush);
            lineRenderer.positionCount =  points.Count;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                var pos = points[i];
                lineRenderer.SetPosition(i, pos);
            }

            lineRenderer.enabled = true;
        }

        public async Task AddLineRendererAnim(List<Vector3> points, CustomBrush brush)
        {
            if (points.Count < 2) return;
            var lineRenderer = InitLineRenderer(brush, false);
            lineRenderer.SetPosition(0, points[0]);
            lineRenderer.SetPosition(1, points[1]);
            for (int i = 2; i < points.Count; i++)
            {
                var pos = points[i];
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(i, pos);
                if(i%10==0) await Task.Yield();
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
            Delete(_lineRenderers.Count - 1);
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
            _saveSystem.SavePng(file, new SaveSystem.DrawingData(_lineRenderers));
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
            if (_config.LoadDrawingsAnimated)
            {
                LoadAnimated(file, 0.004f, false);
                return;
            }

            LoadInstant(file);
        }

        private void LoadInstant(FileInfo file)
        {
            var data = _saveSystem.LoadPng(file);
            if (data == null) return;
            foreach (SaveSystem.SerializableLineRendererData lineRendererData in data.LineRendererData)
            {
                LoadLineRenderer(lineRendererData);
            }
        }

        public void LoadFromResource(string resourceName, bool replace = true, float zOffset = 0)
        {
            var data = _saveSystem.LoadPngFromResource(resourceName);
            if (data == null) return;
            if (replace) Clear();
            foreach (SaveSystem.SerializableLineRendererData lineRendererData in data.LineRendererData)
            {
                LoadLineRenderer(lineRendererData, zOffset);
            }
        }

        public void LoadAnimated(string resourceName, float delay, bool replace, float zOffset = 0)
        {
            _animatedLoadRoutine = StartCoroutine(LoadAnimatedCoroutine(_saveSystem.LoadPngFromResource(resourceName), delay, replace, zOffset));
        }

        public void LoadAnimated(FileInfo file, float delay, bool replace, float zOffset = 0)
        {
            _animatedLoadRoutine = StartCoroutine(LoadAnimatedCoroutine(_saveSystem.LoadPng(file), delay, replace, zOffset));
        }

        IEnumerator LoadAnimatedCoroutine(SaveSystem.DrawingData data, float delay, bool replace, float startDelay = 0)
        {
            if (data == null) yield break;
            if (replace) Clear();

            OnAnimationStateChanged?.Invoke(true);

            var waitForFpsSecs = new WaitForSeconds(0.1f);
            var waitForDelaySecs = new WaitForSeconds(startDelay);

            while (_config.AnimationWaitForStableFPS && _fpsCounter.currentFPS < 25 && !StopAnimation)
            {
                yield return waitForFpsSecs;
            }

            if (startDelay != 0)
            {
                yield return waitForDelaySecs;
            }

            var skipLines = 200 / (float)_fpsCounter.currentFPS;

            foreach (SaveSystem.SerializableLineRendererData lineRendererData in data.LineRendererData)
            {
                var lineRenderer = InitLineRenderer(lineRendererData.Brush.ToCustomBrush(), false);
                lineRenderer.SetPosition(0, lineRendererData.Positions[0].ToVector3());
                lineRenderer.SetPosition(1, lineRendererData.Positions[1].ToVector3());
                for (int i = 2; i < lineRendererData.Positions.Length; i++)
                {
                    var pos = lineRendererData.Positions[i].ToVector3();
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(i, pos);

                    if (!StopAnimation)
                    {
                        if (skipLines >= 1)
                        {
                            if (i % (int)skipLines == 0) yield return null;
                        }
                        else
                        {
                            yield return WaitForFrames((int)(1 / skipLines));
                        }
                    }
                }
            }

            OnAnimationStateChanged?.Invoke(false);
            StopAnimation = false;
            _animatedLoadRoutine = null;
        }

        private IEnumerator WaitForFrames(int frameCount)
        {
            while (frameCount > 0)
            {
                frameCount--;
                yield return null;
            }
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
            if (lineRenderer.positionCount <= MinPositionCount)
            {
                Undo();
            }
        }

        public void OnGameStarted()
        {
            BeatmapObjectSpawnController spawnController = FindObjectOfType<BeatmapObjectSpawnController>();
            // UpdateMaterials(spawnController.currentBpm / 60);
        }

        public void Dispose()
        {
            DestroyImmediate(gameObject);
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
