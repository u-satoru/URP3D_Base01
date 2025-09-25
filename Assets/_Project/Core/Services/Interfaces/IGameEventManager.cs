using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Interface for game event management service
    /// Provides centralized event handling and registration
    /// </summary>
    public interface IGameEventManager : IService
    {
        /// <summary>
        /// Register a game event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="gameEvent">Game event instance</param>
        void RegisterEvent(string eventName, GameEvent gameEvent);

        /// <summary>
        /// Register a generic game event
        /// </summary>
        /// <typeparam name="T">Type of event data</typeparam>
        /// <param name="eventName">Name of the event</param>
        /// <param name="gameEvent">Generic game event instance</param>
        void RegisterEvent<T>(string eventName, GameEvent<T> gameEvent);

        /// <summary>
        /// Unregister a game event
        /// </summary>
        /// <param name="eventName">Name of the event to unregister</param>
        void UnregisterEvent(string eventName);

        /// <summary>
        /// Get a registered game event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <returns>Game event instance or null if not found</returns>
        GameEvent GetEvent(string eventName);

        /// <summary>
        /// Get a registered generic game event
        /// </summary>
        /// <typeparam name="T">Type of event data</typeparam>
        /// <param name="eventName">Name of the event</param>
        /// <returns>Generic game event instance or null if not found</returns>
        GameEvent<T> GetEvent<T>(string eventName);

        /// <summary>
        /// Raise a game event by name
        /// </summary>
        /// <param name="eventName">Name of the event to raise</param>
        void RaiseEvent(string eventName);

        /// <summary>
        /// Raise a generic game event by name
        /// </summary>
        /// <typeparam name="T">Type of event data</typeparam>
        /// <param name="eventName">Name of the event to raise</param>
        /// <param name="data">Event data</param>
        void RaiseEvent<T>(string eventName, T data);

        /// <summary>
        /// Check if an event is registered
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <returns>True if event is registered</returns>
        bool HasEvent(string eventName);

        /// <summary>
        /// Get all registered event names
        /// </summary>
        /// <returns>Array of event names</returns>
        string[] GetRegisteredEventNames();

        /// <summary>
        /// Clear all registered events
        /// </summary>
        void ClearAllEvents();

        /// <summary>
        /// Get event statistics for debugging
        /// </summary>
        /// <returns>Event statistics as formatted string</returns>
        string GetEventStatistics();

        /// <summary>
        /// Enable or disable event logging
        /// </summary>
        /// <param name="enabled">Whether to enable event logging</param>
        void SetEventLogging(bool enabled);

        /// <summary>
        /// Check if event logging is enabled
        /// </summary>
        bool IsEventLoggingEnabled();
    }
}