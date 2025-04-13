using System;
using System.Collections;
using System.Collections.Generic;

namespace Plamb.LevelEditor.Core {
    
    [Serializable]
    public class PlatformWrapper : IEnumerable<PropWrapper>
    {
        public int rotation;
        public string platformId;
        public string lookupKey;
        public List<PropWrapper> props;

        // Implementing the GetEnumerator method.
        public IEnumerator<PropWrapper> GetEnumerator()
        {
            return props.GetEnumerator();
        }

        // Implementing the non-generic GetEnumerator method.
        // This is required, as IEnumerable<T> extends IEnumerable.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    [Serializable]
    public class PropWrapper
    {
        public int rotation;
        public string subgridId;
        public string connectionId;
        public string lookupKey;
    }
    
    [Serializable]
    public class LevelDataWrapper : IEnumerable<PlatformWrapper>
    {
        public List<PlatformWrapper> platforms;

        public string levelName;
        public float levelRating;
        public bool levelCompleted;
        public int levelLightingSetting;
        public int revision;
        public string author;
        public string dateTime;
        public string followUpLevel;
        public string loadingDoneImage;
        public string guid;

        // Implementing the GetEnumerator method.
        public IEnumerator<PlatformWrapper> GetEnumerator()
        {
            return platforms.GetEnumerator();
        }

        // Implementing the non-generic GetEnumerator method.
        // This is required, as IEnumerable<T> extends IEnumerable.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    [Serializable]
    public class SerializableDateTime
    {
        public string DateTimeValueString { get; private set; }

        // Don't serialize this value, it's just for easy access to the datetime value
        [NonSerialized]
        public static DateTime DateTimeValue;

        public SerializableDateTime(DateTime dateTime)
        {
            DateTimeValue = dateTime;
            DateTimeValueString = dateTime.ToString("o");  // Convert to a string in ISO 8601 format
        }

        // Call this after deserialization to convert the string back to a DateTime
        public void OnAfterDeserialize()
        {
            DateTimeValue = DateTime.Parse(DateTimeValueString, null,
                System.Globalization.DateTimeStyles.RoundtripKind);
        }
    }
}
