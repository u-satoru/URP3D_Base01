using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Tests.Core.Editor
{
#if UNITY_EDITOR
    public static class SmokeChecks
    {
        [MenuItem("Tools/Tests/Run Core Smoke Checks")]
        public static void Run()
        {
            Debug.Log("[Smoke] Start Core smoke checks");

            // 1) ServiceLocator basic
            ServiceLocator.Clear();
            ServiceLocator.RegisterService<string>("ok");
            var s = ServiceLocator.GetService<string>();
            Debug.Assert(s == "ok", "ServiceLocator basic get/set failed");

            // 2) FeatureFlags toggle (non-destructive)
            bool prev = asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging;
            asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging = !prev;
            Debug.Assert(asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging == !prev, "FeatureFlags toggle failed");
            asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging = prev;

            // 3) CommandInvoker basic history
            var go = new GameObject("__Smoke_CommandInvoker");
            var inv = go.AddComponent<asterivo.Unity60.Core.Commands.CommandInvoker>();
            inv.ExecuteCommand(new DummyCommand());
            Debug.Assert(inv.UndoStackCount == 1, "CommandInvoker did not push to undo stack");
            inv.Undo();
            Debug.Assert(inv.CanRedo, "CommandInvoker did not allow redo after undo");
            Object.DestroyImmediate(go);

            // 4) Service registration via IServiceLocatorRegistrable
            var root = new GameObject("__Smoke_ServicesRoot");
            var scoreSvc = root.AddComponent<asterivo.Unity60.Core.ScoreService>();
            scoreSvc.RegisterServices();
            var resolved = ServiceLocator.GetService<asterivo.Unity60.Core.Services.IScoreService>();
            Debug.Assert(resolved != null, "IScoreService not resolved via ServiceLocator");
            resolved.SetScore(0);
            resolved.AddScore(10);
            Debug.Assert(resolved.CurrentScore == 10, "IScoreService score not updated");
            scoreSvc.UnregisterServices();
            Object.DestroyImmediate(root);

            // 5) GameStateManager + PauseService basic
            var root2 = new GameObject("__Smoke_StatePauseRoot");
            var gsmGo = new GameObject("GSM"); gsmGo.transform.SetParent(root2.transform);
            var pauseGo = new GameObject("Pause"); pauseGo.transform.SetParent(root2.transform);
            var gsm = gsmGo.AddComponent<asterivo.Unity60.Core.GameStateManagerService>();
            var pause = pauseGo.AddComponent<asterivo.Unity60.Core.PauseService>();
            gsm.RegisterServices();
            pause.RegisterServices();
            var igsm = ServiceLocator.GetService<asterivo.Unity60.Core.Services.IGameStateManager>();
            Debug.Assert(igsm != null, "IGameStateManager not resolved");
            igsm.ChangeGameState(GameState.Playing);
            Debug.Assert(igsm.CurrentGameState == GameState.Playing, "GSM state not updated");
            var ipause = ServiceLocator.GetService<asterivo.Unity60.Core.Services.IPauseService>();
            Debug.Assert(ipause != null, "IPauseService not resolved");
            float ts = Time.timeScale;
            ipause.SetPauseState(true);
            Debug.Assert(ipause.IsPaused, "PauseService did not set paused");
            ipause.SetPauseState(false);
            Time.timeScale = ts; // restore
            pause.UnregisterServices();
            gsm.UnregisterServices();
            Object.DestroyImmediate(root2);

            Debug.Log("[Smoke] Core smoke checks passed");
        }

        private class DummyCommand : ICommand
        {
            public bool CanUndo => true;
            public void Execute() { }
            public void Undo() { }
        }
    }
#endif
}

