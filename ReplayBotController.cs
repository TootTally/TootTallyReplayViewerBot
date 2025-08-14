using BaboonAPI.Utility;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTallyLeaderboard;
using TootTallyLeaderboard.Replays;
using UnityEngine;
using static TootTallyCore.APIServices.SerializableClass;
using static TootTallyReplayViewerBot.ReplayBotManager;

namespace TootTallyReplayViewerBot
{
    public class ReplayBotController : MonoBehaviour
    {
        private float _currentTimer;
        private Action _nextAction;
        private BotState _currentState;
        private SceneStates _currentScene;
        private MonoBehaviour _currentController;
        public bool IsEnabled;
        private List<ScoreDataFromDB> _scoresList;
        private ScoreDataFromDB _currentScore, _previousScore;


        public void Init()
        {
            _scoresList = new List<ScoreDataFromDB>();
            _currentTimer = 0;
            _currentState = BotState.Inactive;
            _currentScene = SceneStates.HomeScene;
            IsEnabled = false;
        }

        public void SetController(MonoBehaviour controller)
        {
            _currentController = controller;
            if (controller is HomeController)
                SetScene(SceneStates.HomeScene);
            else if (controller is CharSelectController)
                SetScene(SceneStates.CharacterSelectScene);
            else if (controller is LevelSelectController)
                SetScene(SceneStates.LevelSelectScene);
            else if (controller is LoadController)
                SetScene(SceneStates.LoadController);
            else if (controller is GameController)
                SetScene(SceneStates.GameScene);
            else if (controller is PointSceneController)
                SetScene(SceneStates.PointScene);

            if (IsEnabled)
                UpdateOnSceneStateChange();
        }

        public void UpdateOnSceneStateChange()
        {
            switch (_currentScene)
            {
                case SceneStates.HomeScene:
                    SetAction(TransitionToCharSelect);
                    WaitForSeconds(1);
                    break;
                case SceneStates.CharacterSelectScene:
                    SetAction(TransitionToLevelSelect);
                    WaitForSeconds(1);
                    break;
                case SceneStates.LevelSelectScene:
                    SetAction(RandomizeSong);
                    WaitForSeconds(2);
                    break;
                case SceneStates.LoadController:
                    SetState(BotState.Transitioning);
                    break;
                case SceneStates.GameScene:
                    SetState(BotState.Replaying);
                    break;
                case SceneStates.PointScene:
                    SetAction(ExitPointScene);
                    WaitForSeconds(10);
                    break;
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(Plugin.Instance.ToggleKey.Value))
            {
                IsEnabled = !IsEnabled;
                if (IsEnabled)
                    UpdateOnSceneStateChange();
            }

            if (!IsEnabled) return;

            if (_currentState == BotState.Waiting)
                if (_currentTimer > 0)
                    _currentTimer -= Time.deltaTime;
                else
                {
                    _currentTimer = 0;
                    SetState(BotState.Transitioning);
                    _nextAction.Invoke();
                }
        }

        public void TransitionToCharSelect()
        {
            (_currentController as HomeController).btnclick1();
        }

        public void TransitionToLevelSelect()
        {
            (_currentController as CharSelectController).clickOk();
        }

        public void ExitPointScene()
        {
            (_currentController as PointSceneController).clickCont();
        }

        public void RandomizeSong()
        {
            (_currentController as LevelSelectController).clickRandomTrack();
            _scoresList.Clear();
            SetAction(PickRandomReplay);
            WaitForSeconds(10);
        }

        public void AddScoreDataEntry(ScoreDataFromDB entry)
        {
            _scoresList.Add(entry);
            if (Plugin.Instance.DebugMode.Value)
                Plugin.LogInfo($"ReplayID {entry.replay_id} was added. Total Replay Count is now {_scoresList.Count}");
        }

        public void PickRandomReplay()
        {
            if (_scoresList.Count == 0)
            {
                RandomizeSong();
                return;
            }
            _previousScore = _currentScore;
            _currentScore = _scoresList[UnityEngine.Random.Range(0, _scoresList.Count)];
            ReplaySystemManager.ResolveLoadReplay(_currentScore.replay_id, _currentController as LevelSelectController);
            _currentState = BotState.Waiting;
            WaitForSeconds(15);
            _nextAction = RandomizeSong;
        }

        public void WaitForSeconds(float seconds)
        {
            SetState(BotState.Waiting);
            if (Plugin.Instance.DebugMode.Value)
                Plugin.LogInfo($"Waiting for {seconds}");
            _currentTimer = seconds;
        }

        public void SetScene(SceneStates sceneStates)
        {
            if (Plugin.Instance.DebugMode.Value)
                Plugin.LogInfo($"Scene is now {sceneStates}");
            _currentScene = sceneStates;
        }

        public void SetState(BotState state)
        {
            if (Plugin.Instance.DebugMode.Value)
                Plugin.LogInfo($"State set to {state}");
            _currentState = state;
        }

        public void SetAction(Action action)
        {
            if (Plugin.Instance.DebugMode.Value)
                Plugin.LogInfo($"Next action set to {action.Method.Name}");
            _nextAction = action;
        }

        public enum BotState
        {
            Waiting,
            Transitioning,
            Replaying,
            SelectingSong,
            Inactive
        }
    }
}
