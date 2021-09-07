using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using Scribble.Installers;
using SiraUtil.Zenject;
using Config = IPA.Config.Config;
using Logger = IPA.Logging.Logger;

namespace Scribble
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {

        [Init]
        public Plugin(Logger logger, Config config, Zenjector zenjector, PluginMetadata metadata)
        {
            zenjector.OnApp<PluginAppInstaller>().WithParameters(logger, config.Generated<PluginConfig>(), metadata);
            zenjector.OnMenu<PluginMenuInstaller>();
        }

        [OnEnable]
        public void OnEnable()
        {

        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}
