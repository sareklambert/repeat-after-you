using UnityEngine;

/// <summary>
/// This class defines a generic singleton for other classes to inherit from.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : Component
{
    // Define global instance variable
    private static T m_instance;
    public static T Instance
    {
        get
        {
            // Return the global instance; Set up one if there is none
            if (!m_instance)
            {
                m_instance = (T)FindObjectOfType(typeof(T));
                if (!m_instance)
                {
                    SetupInstance();
                }
            }
            return m_instance;
        }
    }
    
    // Make sure the instance is unique
    public virtual void Awake()
    {
        RemoveDuplicates();
    }
    
    // Creates the singleton instance
    private static void SetupInstance()
    {
        // Make sure there isn't another instance already
        m_instance = (T)FindObjectOfType(typeof(T));
        if (m_instance) return;
        
        // Set up a new game object with the singleton component
        GameObject gameObj = new GameObject
        {
            name = typeof(T).Name
        };
        m_instance = gameObj.AddComponent<T>();
        DontDestroyOnLoad(gameObj);
    }
    
    // Removes any other instance of the object
    private void RemoveDuplicates()
    {
        if (!m_instance)
        {
            m_instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
