using Scribble.Stores;
using UnityEngine;
using Zenject;

namespace Scribble.Installers
{
    internal class Initializer : IInitializable
    {
        private readonly AssetLoader _assetLoader;
        private readonly EffectStore _effects;
        private readonly BrushTextures _brushTextures;
        private readonly BrushStore _brushStore;
        private readonly ScribbleContainer _scribbleContainer;

        public Initializer(
            AssetLoader assetLoader,
            EffectStore effects,
            BrushTextures brushTextures,
            BrushStore brushStore,
            ScribbleContainer scribbleContainer)
        {
            _assetLoader = assetLoader;
            _effects = effects;
            _brushTextures = brushTextures;
            _brushStore = brushStore;
            _scribbleContainer = scribbleContainer;
        }

        public void Initialize()
        {
            if (!_assetLoader.Load()) return;
            _effects.LoadEffects();
            _brushTextures.LoadTextures();
            _brushStore.LoadBrushes();
        }
    }
}