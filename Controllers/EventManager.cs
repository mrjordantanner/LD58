using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton Event Manager that handles global events throughout the game.
/// Provides a centralized system for decoupled communication between game systems.
/// </summary>
public class EventManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static EventManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        #endregion

        StartCoroutine(Init());
    }

    #region Declarations

    public string Name { get { return "Event Manager"; } }

    // Dictionary to store event subscriptions
    private Dictionary<string, List<Action<object>>> eventSubscriptions = new Dictionary<string, List<Action<object>>>();
    
    // Dictionary for events with specific data types
    private Dictionary<string, List<Delegate>> typedEventSubscriptions = new Dictionary<string, List<Delegate>>();

    #endregion

    public IEnumerator Init()
    {
        // Initialize event system
        eventSubscriptions.Clear();
        typedEventSubscriptions.Clear();
        
        Debug.Log("EventManager: Initialized");
        yield return new WaitForSecondsRealtime(0);
    }

    #region Generic Event System

    /// <summary>
    /// Subscribe to an event with a callback that receives object data
    /// </summary>
    /// <param name="eventName">Name of the event to subscribe to</param>
    /// <param name="callback">Callback function to execute when event is triggered</param>
    public void Subscribe(string eventName, Action<object> callback)
    {
        if (!eventSubscriptions.ContainsKey(eventName))
        {
            eventSubscriptions[eventName] = new List<Action<object>>();
        }
        
        eventSubscriptions[eventName].Add(callback);
        Debug.Log($"EventManager: Subscribed to '{eventName}'. Total subscribers: {eventSubscriptions[eventName].Count}");
    }

    /// <summary>
    /// Subscribe to an event with a callback that receives no data
    /// </summary>
    /// <param name="eventName">Name of the event to subscribe to</param>
    /// <param name="callback">Callback function to execute when event is triggered</param>
    public void Subscribe(string eventName, Action callback)
    {
        Subscribe(eventName, (data) => callback());
    }

    /// <summary>
    /// Subscribe to a typed event with strongly typed data
    /// </summary>
    /// <typeparam name="T">Type of data the event will carry</typeparam>
    /// <param name="eventName">Name of the event to subscribe to</param>
    /// <param name="callback">Callback function to execute when event is triggered</param>
    public void Subscribe<T>(string eventName, Action<T> callback)
    {
        if (!typedEventSubscriptions.ContainsKey(eventName))
        {
            typedEventSubscriptions[eventName] = new List<Delegate>();
        }
        
        typedEventSubscriptions[eventName].Add(callback);
    }

    /// <summary>
    /// Unsubscribe from an event
    /// </summary>
    /// <param name="eventName">Name of the event to unsubscribe from</param>
    /// <param name="callback">Callback function to remove</param>
    public void Unsubscribe(string eventName, Action<object> callback)
    {
        if (eventSubscriptions.ContainsKey(eventName))
        {
            eventSubscriptions[eventName].Remove(callback);
        }
    }

    /// <summary>
    /// Unsubscribe from an event with no data
    /// </summary>
    /// <param name="eventName">Name of the event to unsubscribe from</param>
    /// <param name="callback">Callback function to remove</param>
    public void Unsubscribe(string eventName, Action callback)
    {
        if (eventSubscriptions.ContainsKey(eventName))
        {
            // Find and remove the wrapper callback
            for (int i = eventSubscriptions[eventName].Count - 1; i >= 0; i--)
            {
                if (eventSubscriptions[eventName][i].Target == callback.Target)
                {
                    eventSubscriptions[eventName].RemoveAt(i);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Unsubscribe from a typed event
    /// </summary>
    /// <typeparam name="T">Type of data the event carries</typeparam>
    /// <param name="eventName">Name of the event to unsubscribe from</param>
    /// <param name="callback">Callback function to remove</param>
    public void Unsubscribe<T>(string eventName, Action<T> callback)
    {
        if (typedEventSubscriptions.ContainsKey(eventName))
        {
            typedEventSubscriptions[eventName].Remove(callback);
        }
    }

    /// <summary>
    /// Trigger an event with object data
    /// </summary>
    /// <param name="eventName">Name of the event to trigger</param>
    /// <param name="data">Data to pass to subscribers</param>
    public void TriggerEvent(string eventName, object data = null)
    {
        Debug.Log($"EventManager: Triggering event '{eventName}' with data: {data}");
        
        if (eventSubscriptions.ContainsKey(eventName))
        {
            var callbacks = new List<Action<object>>(eventSubscriptions[eventName]);
            Debug.Log($"EventManager: Found {callbacks.Count} subscribers for '{eventName}'");
            
            foreach (var callback in callbacks)
            {
                try
                {
                    callback?.Invoke(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventManager: Error in event callback for '{eventName}': {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"EventManager: No subscribers found for event '{eventName}'");
        }
    }

    /// <summary>
    /// Trigger an event with no data
    /// </summary>
    /// <param name="eventName">Name of the event to trigger</param>
    public void TriggerEvent(string eventName)
    {
        TriggerEvent(eventName, null);
    }

    /// <summary>
    /// Trigger a typed event with strongly typed data
    /// </summary>
    /// <typeparam name="T">Type of data to pass</typeparam>
    /// <param name="eventName">Name of the event to trigger</param>
    /// <param name="data">Typed data to pass to subscribers</param>
    public void TriggerEvent<T>(string eventName, T data)
    {
        if (typedEventSubscriptions.ContainsKey(eventName))
        {
            // Create a copy of the list to avoid issues if subscribers modify the list during iteration
            var callbacks = new List<Delegate>(typedEventSubscriptions[eventName]);
            
            foreach (var callback in callbacks)
            {
                try
                {
                    if (callback is Action<T> typedCallback)
                    {
                        typedCallback.Invoke(data);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventManager: Error in typed event callback for '{eventName}': {e.Message}");
                }
            }
        }
    }

    #endregion

    #region Convenience Methods

    /// <summary>
    /// Clear all subscriptions for a specific event
    /// </summary>
    /// <param name="eventName">Name of the event to clear</param>
    public void ClearEvent(string eventName)
    {
        if (eventSubscriptions.ContainsKey(eventName))
        {
            eventSubscriptions[eventName].Clear();
        }
        
        if (typedEventSubscriptions.ContainsKey(eventName))
        {
            typedEventSubscriptions[eventName].Clear();
        }
    }

    /// <summary>
    /// Clear all event subscriptions
    /// </summary>
    public void ClearAllEvents()
    {
        eventSubscriptions.Clear();
        typedEventSubscriptions.Clear();
    }

    /// <summary>
    /// Get the number of subscribers for an event
    /// </summary>
    /// <param name="eventName">Name of the event</param>
    /// <returns>Number of subscribers</returns>
    public int GetSubscriberCount(string eventName)
    {
        int count = 0;
        
        if (eventSubscriptions.ContainsKey(eventName))
        {
            count += eventSubscriptions[eventName].Count;
        }
        
        if (typedEventSubscriptions.ContainsKey(eventName))
        {
            count += typedEventSubscriptions[eventName].Count;
        }
        
        return count;
    }

    /// <summary>
    /// Check if an event has any subscribers
    /// </summary>
    /// <param name="eventName">Name of the event</param>
    /// <returns>True if event has subscribers</returns>
    public bool HasSubscribers(string eventName)
    {
        return GetSubscriberCount(eventName) > 0;
    }

    #endregion

    #region Common Game Events

    // These are examples of common game events that might be used throughout the project
    // You can add more as needed or remove these if not needed

    /// <summary>
    /// Event triggered when the game starts
    /// </summary>
    public const string GAME_STARTED = "GameStarted";
    
    /// <summary>
    /// Event triggered when the game ends
    /// </summary>
    public const string GAME_ENDED = "GameEnded";
    
    /// <summary>
    /// Event triggered when the game is paused
    /// </summary>
    public const string GAME_PAUSED = "GamePaused";
    
    /// <summary>
    /// Event triggered when the game is unpaused
    /// </summary>
    public const string GAME_UNPAUSED = "GameUnpaused";
    
    /// <summary>
    /// Event triggered when the player takes damage
    /// </summary>
    public const string PLAYER_DAMAGED = "PlayerDamaged";
    
    /// <summary>
    /// Event triggered when the player dies
    /// </summary>
    public const string PLAYER_DIED = "PlayerDied";
    
    /// <summary>
    /// Event triggered when the player respawns
    /// </summary>
    public const string PLAYER_RESPAWNED = "PlayerRespawned";
    
    /// <summary>
    /// Event triggered when a collectible is picked up
    /// </summary>
    public const string COLLECTIBLE_PICKED_UP = "CollectiblePickedUp";
    
    /// <summary>
    /// Event triggered when the level changes
    /// </summary>
    public const string LEVEL_CHANGED = "LevelChanged";
    
    // Progression Events
    /// <summary>
    /// Event triggered when a level starts
    /// </summary>
    public const string LEVEL_STARTED = "LevelStarted";
    
    /// <summary>
    /// Event triggered when a level is completed
    /// </summary>
    public const string LEVEL_COMPLETED = "LevelCompleted";
    
    /// <summary>
    /// Event triggered when a level fails
    /// </summary>
    public const string LEVEL_FAILED = "LevelFailed";
    
    /// <summary>
    /// Event triggered when a round starts
    /// </summary>
    public const string ROUND_STARTED = "RoundStarted";
    
    /// <summary>
    /// Event triggered when a round is completed
    /// </summary>
    public const string ROUND_COMPLETED = "RoundCompleted";
    
    /// <summary>
    /// Event triggered when a round fails
    /// </summary>
    public const string ROUND_FAILED = "RoundFailed";

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        // Clean up when the EventManager is destroyed
        ClearAllEvents();
    }

    #endregion
}
