using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Features.Validation
{
    /// <summary>
    /// イベント接続の検証を行うバリデーター
    /// </summary>
    public static class EventConnectionValidator
    {
        /// <summary>
        /// システム全体のイベント接続を検証
        /// </summary>
        /// <param name="validateCriticalOnly">重要なイベントのみを検証するか</param>
        /// <returns>検証結果</returns>
        public static ValidationResult ValidateAllEventConnections(bool validateCriticalOnly = true)
        {
            var result = new ValidationResult();
            
            // GameEventの検証
            ValidateGameEvents(result, validateCriticalOnly);
            
            // EventListenerの検証
            ValidateEventListeners(result, validateCriticalOnly);
            
            // コマンドイベントの検証
            ValidateCommandEvents(result, validateCriticalOnly);
            
            return result;
        }
        
        private static void ValidateGameEvents(ValidationResult result, bool criticalOnly)
        {
            var gameEvents = Object.FindObjectsByType<GameEvent>(FindObjectsSortMode.None);
            
            foreach (var gameEvent in gameEvents)
            {
                if (gameEvent == null)
                {
                    result.AddError("Null GameEvent found in scene");
                    continue;
                }
                
                // GameEventが適切に設定されているかチェック
                if (string.IsNullOrEmpty(gameEvent.name))
                {
                    result.AddWarning($"GameEvent has no name: {gameEvent.GetInstanceID()}");
                }
            }
            
            result.AddInfo($"Validated {gameEvents.Length} GameEvent objects");
        }
        
        private static void ValidateEventListeners(ValidationResult result, bool criticalOnly)
        {
            var listeners = Object.FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            
            foreach (var listener in listeners)
            {
                if (listener == null)
                {
                    result.AddError("Null GameEventListener found in scene");
                    continue;
                }
                
                // GameEventが設定されているかチェック
                var gameEventField = listener.GetType().GetField("GameEvent", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (gameEventField != null)
                {
                    var gameEventValue = gameEventField.GetValue(listener);
                    if (gameEventValue == null)
                    {
                        result.AddWarning($"GameEventListener '{listener.name}' has no GameEvent assigned");
                    }
                }
                
                // Responseが設定されているかチェック
                if (listener.Response == null)
                {
                    result.AddWarning($"GameEventListener '{listener.name}' has no Response configured");
                }
                else if (listener.Response.GetPersistentEventCount() == 0)
                {
                    result.AddWarning($"GameEventListener '{listener.name}' Response has no persistent listeners");
                }
            }
            
            result.AddInfo($"Validated {listeners.Length} GameEventListener objects");
        }
        
        private static void ValidateCommandEvents(ValidationResult result, bool criticalOnly)
        {
            var commandEvents = Object.FindObjectsByType<CommandGameEvent>(FindObjectsSortMode.None);
            
            foreach (var commandEvent in commandEvents)
            {
                if (commandEvent == null)
                {
                    result.AddError("Null CommandGameEvent found in scene");
                    continue;
                }
                
                if (string.IsNullOrEmpty(commandEvent.name))
                {
                    result.AddWarning($"CommandGameEvent has no name: {commandEvent.GetInstanceID()}");
                }
            }
            
            result.AddInfo($"Validated {commandEvents.Length} CommandGameEvent objects");
        }
    }
    
    /// <summary>
    /// 検証結果を格納するクラス
    /// </summary>
    public class ValidationResult
    {
        private List<string> errors = new List<string>();
        private List<string> warnings = new List<string>();
        private List<string> infos = new List<string>();
        
        public IReadOnlyList<string> Errors => errors;
        public IReadOnlyList<string> Warnings => warnings;
        public IReadOnlyList<string> Infos => infos;
        
        public bool HasErrors => errors.Count > 0;
        public bool HasWarnings => warnings.Count > 0;
        public bool IsValid => !HasErrors;
        
        public void AddError(string message)
        {
            errors.Add(message);
            var eventLogger = asterivo.Unity60.Core.ServiceLocator.GetService<IEventLogger>();
            if (eventLogger != null) {
                eventLogger.LogError($"[Validation] {message}");
            }
        }

        public void AddWarning(string message)
        {
            warnings.Add(message);
            var eventLogger = asterivo.Unity60.Core.ServiceLocator.GetService<IEventLogger>();
            if (eventLogger != null) {
                eventLogger.LogWarning($"[Validation] {message}");
            }
        }
        
        public void AddInfo(string message)
        {
            infos.Add(message);
            EventLogger.LogStatic($"[Validation] {message}");
        }
        
        /// <summary>
        /// 検証結果のサマリーを取得
        /// </summary>
        /// <returns>結果のサマリー文字列</returns>
        public string GetSummary()
        {
            return $"Validation Results: {errors.Count} errors, {warnings.Count} warnings, {infos.Count} info messages";
        }
    }
}
