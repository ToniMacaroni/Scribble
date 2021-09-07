using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Scribble.Installers
{
    internal class MenuInitializer : IInitializable
    {
        private readonly ScribbleContainer _container;

        public MenuInitializer(ScribbleContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            _container.InitMenu(SceneManager.GetActiveScene());
        }
    }
}