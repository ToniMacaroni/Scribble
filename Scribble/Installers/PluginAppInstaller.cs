using System;
using System.Linq;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Logging;
using Scribble.Stores;
using SiraUtil;

namespace Scribble.Installers
{
    internal class PluginAppInstaller : Zenject.Installer
    {
        private readonly Logger _logger;
        private readonly PluginConfig _config;

        public PluginAppInstaller(Logger logger, PluginConfig config, PluginMetadata metadata)
        {
            _logger = logger;
            _config = config;

            if (_config.LastUsedVersion == "0.0.0") _config.FirstTimeLaunch = true;
            _config.LastUsedVersion = metadata.HVersion.ToString();
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config);
            Container.Bind<PluginDirectories>().AsSingle();

            var launchOptions = new LaunchOptions();

            if (Environment.GetCommandLineArgs().Any(x => x.ToLower() == "fpfc"))
            {
                launchOptions.FPFC = true;
            }

            Container.BindInstance(launchOptions);

            Container.BindInterfacesAndSelfTo<AssetLoader>().AsSingle();
            Container.Bind<EffectStore>().AsSingle();
            Container.Bind<BrushTextures>().AsSingle();
            Container.BindInterfacesAndSelfTo<BrushStore>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScribbleContainer>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<SaveSystem>().AsSingle();

            Container.BindInterfacesAndSelfTo<Initializer>().AsSingle();
        }
    }
}