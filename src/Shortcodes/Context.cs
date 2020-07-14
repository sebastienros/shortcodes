using System.Collections;
using System.Collections.Generic;

namespace Shortcodes
{
    public class Context : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> _items;

        private Dictionary<string, object> Items => _items ??= new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                return Items[key];
            }
            set
            {
                Items[key] = value;
            }
        }

        public ICollection<string> Keys => Items.Keys;

        public int Count => Items.Count;

        public void Clear()
        {
            Items.Clear();
        }

        public bool ContainsKey(string key)
        {
            return Items.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return Items.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return Items.TryGetValue(key, out value);
        }

        public T GetOrSetValue<T>(string key, T value)
        {
            if (Items.TryGetValue(key, out var result))
            {
                if (result is T t)
                {
                    return t;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                Items[key] = value;

                return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
