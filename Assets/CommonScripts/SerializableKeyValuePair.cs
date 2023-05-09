using UnityEngine;

[System.Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    [SerializeField]
    private TKey key;
    [SerializeField]
    private TValue value;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }

    public TKey Key
    {
        get
        {
            return key;
        }
        set
        {
            key = value;
        }
    }

    public TValue Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
        }
    }
}
