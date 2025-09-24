using UnityEngine;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Concrete GameEvent implementations for common data types
    /// </summary>

    /// <summary>
    /// Float value GameEvent. Used for numeric data like HP, scores, time, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Float Game Event", menuName = "asterivo.Unity60/Events/Float Game Event")]
    public class FloatGameEvent : GameEvent<float> { }

    /// <summary>
    /// Int value GameEvent. Used for integer data like levels, item counts, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Int Game Event", menuName = "asterivo.Unity60/Events/Int Game Event")]
    public class IntGameEvent : GameEvent<int> { }

    /// <summary>
    /// Bool value GameEvent. Used for flags and switch states.
    /// </summary>
    [CreateAssetMenu(fileName = "New Bool Game Event", menuName = "asterivo.Unity60/Events/Bool Game Event")]
    public class BoolGameEvent : GameEvent<bool> { }

    /// <summary>
    /// String value GameEvent. Used for text and messages.
    /// </summary>
    [CreateAssetMenu(fileName = "New String Game Event", menuName = "asterivo.Unity60/Events/String Game Event")]
    public class StringGameEvent : GameEvent<string> { }

    /// <summary>
    /// Vector3 value GameEvent. Used for position and direction data.
    /// </summary>
    [CreateAssetMenu(fileName = "New Vector3 Game Event", menuName = "asterivo.Unity60/Events/Vector3 Game Event")]
    public class Vector3GameEvent : GameEvent<Vector3> { }

    /// <summary>
    /// GameObject value GameEvent. Used for object references.
    /// </summary>
    [CreateAssetMenu(fileName = "New GameObject Game Event", menuName = "asterivo.Unity60/Events/GameObject Game Event")]
    public class GameObjectGameEvent : GameEvent<GameObject> { }
}