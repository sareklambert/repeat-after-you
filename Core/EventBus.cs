using System;
using System.Collections.Generic;

namespace Plamb.Events
{
    /// <summary>
    /// This class defines a generic event bus. Used for sending events between objects.
    /// Events are set up as custom classes to be able to carry any data we need.
    /// </summary>
    public static class EventBus
    {
        // Set up event dictionary
        private static Dictionary<Type, Delegate> m_events = new Dictionary<Type, Delegate>();

        // An object subscribes to an event
        public static void Subscribe<T>(Action<T> listener)
        {
            if (!m_events.ContainsKey(typeof(T))) m_events[typeof(T)] = null;
            m_events[typeof(T)] = (Action<T>)m_events[typeof(T)] + listener;
        }

        // An object unsubscribes from an event
        public static void Unsubscribe<T>(Action<T> listener)
        {
            if (m_events.ContainsKey(typeof(T))) m_events[typeof(T)] = (Action<T>)m_events[typeof(T)] - listener;
        }

        // Invoke the event and inform everyone who subscribed
        public static void Publish<T>(T eventData)
        {
            if (m_events.ContainsKey(typeof(T))) ((Action<T>)m_events[typeof(T)])?.Invoke(eventData);
        }
    }
}
