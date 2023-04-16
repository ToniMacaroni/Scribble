using UnityEngine;

namespace Scribble.Tools
{
    internal interface ITool
    {
        void Init(BrushMeshDrawer brushDrawer, ScribbleContainer scribbleContainer, PluginConfig config, SaberType saberType);

        void OnSelected();

        /// <summary>
        /// Called when on first activation of tool
        /// </summary>
        void OnDown(Vector3 position);

        /// <summary>
        /// Update the tools behaviour here
        /// Called on update
        /// </summary>
        void OnUpdate(Vector3 position);

        /// <summary>
        /// Called when tool is deactivated
        /// </summary>
        void OnUp(Vector3 position);

        void OnDeselected();

        void Render();
    }
}