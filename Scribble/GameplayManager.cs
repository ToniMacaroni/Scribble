//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Scribble
//{
//    public static class GameplayManager
//    {

//        public static IDifficultyBeatmap Beatmap;

//        public static void OnGameSceneLoaded()
//        {
//            if (!BS_Utils.Plugin.LevelData.IsSet)
//            {
//                Plugin.Log.Debug("Beatmap not set");
//                Beatmap = null;
//                return;
//            }

//            Beatmap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;

//            if(ScribbleContainer.Instance) ScribbleContainer.Instance.OnGameStarted();
//        }
//    }
//}
