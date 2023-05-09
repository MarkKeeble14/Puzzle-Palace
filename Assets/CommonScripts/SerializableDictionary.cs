using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField]
    private List<SerializableKeyValuePair<TKey, TValue>> dictionary = new List<SerializableKeyValuePair<TKey, TValue>>();

    public List<SerializableKeyValuePair<TKey, TValue>> ToList()
    {
        return dictionary;
    }

    public SerializableKeyValuePair<TKey, TValue> GetEntry(TKey key)
    {
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in dictionary)
        {
            if (kvp.Key.Equals(key))
            {
                return kvp;
            }
        }
        return null;
    }

    private TValue GetValue(TKey key)
    {
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in dictionary)
        {
            if (kvp.Key.Equals(key)) return kvp.Value;
        }
        return default;
    }

    private void SetValue(TKey key, TValue value)
    {
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in dictionary)
        {
            if (kvp.Key.Equals(key))
            {
                kvp.Value = value;
                return;
            }
        }
    }

    public TValue this[TKey key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public void RemoveEntry(TKey key)
    {
        dictionary.Remove(GetEntry(key));
    }

    public bool ContainsKey(TKey key)
    {
        return GetEntry(key) != null;
    }

    public int IndexOf(TKey key)
    {
        return dictionary.IndexOf(GetEntry(key));
    }

    public void Set(TKey key, TValue newValue)
    {
        if (ContainsKey(key))
        {
            for (int i = 0; i < dictionary.Count; i++)
            {
                SerializableKeyValuePair<TKey, TValue> kvp = dictionary[i];
                if (kvp.Key.Equals(key))
                {
                    kvp.Value = newValue;
                    break;
                }
            }
        }
        else
        {
            dictionary.Add(new SerializableKeyValuePair<TKey, TValue>(key, newValue));
        }
    }

    public List<TValue> Values()
    {
        List<TValue> toReturn = new List<TValue>();
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in dictionary)
        {
            toReturn.Add(kvp.Value);
        }
        return toReturn;
    }

    public List<TKey> Keys()
    {
        List<TKey> toReturn = new List<TKey>();
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in dictionary)
        {
            toReturn.Add(kvp.Key);
        }
        return toReturn;
    }

    public List<TKey> KeysWhereValueMeetsCondition(Func<TValue, bool> condition)
    {
        List<TKey> toReturn = new List<TKey>();
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in dictionary)
        {
            if (condition(kvp.Value))
                toReturn.Add(kvp.Key);
        }
        return toReturn;
    }
}
