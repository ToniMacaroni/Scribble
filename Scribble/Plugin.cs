using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using Scribble.Installers;
using SiraUtil.Zenject;
using Config = IPA.Config.Config;

namespace Scribble
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {

        [Init]
        public Plugin(Config config, Zenjector zenjector, PluginMetadata metadata)
        {
            zenjector.Install<PluginAppInstaller>(Location.App, config.Generated<PluginConfig>(), metadata);
            zenjector.Install<PluginMenuInstaller>(Location.Menu);
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
