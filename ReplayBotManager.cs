using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTallyLeaderboard;
using UnityEngine;
using static TootTallyCore.APIServices.SerializableClass;

namespace TootTallyReplayViewerBot
{
    public static class ReplayBotManager
    {
        private static GameObject _botObject;
        private static ReplayBotController _botController;
        private static SceneStates _currentScene;
        private static MonoBehaviour _currentController;

        #region HomeManagement
        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        public static void OnLevelSelectControllerStart(HomeController __instance)
        {
            if (_botController == null)
            {
                _botObject = new GameObject("BotController", typeof(ReplayBotController));
                GameObject.DontDestroyOnLoad(_botObject);
                _botController = _botObject.GetComponent<ReplayBotController>();
                _botController.Init();
            }
            _botController.SetController(__instance);
        }
        #endregion

        #region CharSelectManagement
        [HarmonyPatch(typeof(CharSelectController), nameof(CharSelectController.Start))]
        [HarmonyPostfix]
        public static void OnCharSelectControllerStart(CharSelectController __instance)
        {
            _currentScene = SceneStates.CharacterSelectScene;
            _currentController = __instance;
            _botController.SetController(__instance);
        }
        #endregion

        #region LevelSelectManagement
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void OnLevelSelectControllerStart(LevelSelectController __instance)
        {
            _currentScene = SceneStates.LevelSelectScene;
            _currentController = __instance;
            _botController.SetController(__instance);
        }
        #endregion

        #region LeaderboardManagement
        [HarmonyPatch(typeof(LeaderboardFactory), nameof(LeaderboardFactory.CreateLeaderboardRowEntryFromScore))]
        [HarmonyPostfix]
        public static void OnLevelSelectControllerStart(ScoreDataFromDB scoreData)
        {
            if (scoreData.replay_id != "" && scoreData.score != 0)
                _botController.AddScoreDataEntry(scoreData);
        }
        #endregion

        #region LoadController
        [HarmonyPatch(typeof(LoadController), nameof(LoadController.Start))]
        [HarmonyPostfix]
        public static void OnLoadControllerStart(LoadController __instance)
        {
            _currentScene = SceneStates.LoadController;
            _currentController = __instance;
            _botController.SetController(__instance);
        }
        #endregion

        #region GameControllerManagement
        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void OnGameControllerStart(GameController __instance)
        {
            _currentScene = SceneStates.GameScene;
            _currentController = __instance;
            _botController.SetController(__instance);
        }
        #endregion

        #region PointSceneManagement
        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void OnPointSceneStart(PointSceneController __instance)
        {
            _currentScene = SceneStates.PointScene;
            _currentController = __instance;
            _botController.SetController(__instance);
        }
        #endregion


        public enum SceneStates
        {
            HomeScene,
            CharacterSelectScene,
            LevelSelectScene,
            LoadController,
            GameScene,
            PointScene,

        }
    }
}
