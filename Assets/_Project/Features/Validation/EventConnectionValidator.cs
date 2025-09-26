using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Features.Validation
{
    /// <summary>
    /// 繧､繝吶Φ繝域磁邯壹・讀懆ｨｼ繧定｡後≧繝舌Μ繝・・繧ｿ繝ｼ
    /// </summary>
    public static class EventConnectionValidator
    {
        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蜈ｨ菴薙・繧､繝吶Φ繝域磁邯壹ｒ讀懆ｨｼ
        /// </summary>
        /// <param name="validateCriticalOnly">驥崎ｦ√↑繧､繝吶Φ繝医・縺ｿ繧呈､懆ｨｼ縺吶ｋ縺・/param>
        /// <returns>讀懆ｨｼ邨先棡</returns>
        public static ValidationResult ValidateAllEventConnections(bool validateCriticalOnly = true)
        {
            var result = new ValidationResult();
            
            // GameEvent縺ｮ讀懆ｨｼ
            ValidateGameEvents(result, validateCriticalOnly);
            
            // EventListener縺ｮ讀懆ｨｼ
            ValidateEventListeners(result, validateCriticalOnly);
            
            // 繧ｳ繝槭Φ繝峨う繝吶Φ繝医・讀懆ｨｼ
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
                
                // GameEvent縺碁←蛻・↓險ｭ螳壹＆繧後※縺・ｋ縺九メ繧ｧ繝・け
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
                
                // GameEvent縺瑚ｨｭ螳壹＆繧後※縺・ｋ縺九メ繧ｧ繝・け
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
                
                // Response縺瑚ｨｭ螳壹＆繧後※縺・ｋ縺九メ繧ｧ繝・け
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
    /// 讀懆ｨｼ邨先棡繧呈ｼ邏阪☆繧九け繝ｩ繧ｹ
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
            ServiceHelper.Log($"[Validation] {message}");
        }
        
        /// <summary>
        /// 讀懆ｨｼ邨先棡縺ｮ繧ｵ繝槭Μ繝ｼ繧貞叙蠕・
        /// </summary>
        /// <returns>邨先棡縺ｮ繧ｵ繝槭Μ繝ｼ譁・ｭ怜・</returns>
        public string GetSummary()
        {
            return $"Validation Results: {errors.Count} errors, {warnings.Count} warnings, {infos.Count} info messages";
        }
    }
}

