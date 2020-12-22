using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shortcodes
{
    public struct Arguments : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _arguments;

        public Arguments(Dictionary<string, string> arguments)
        {
            _arguments = arguments;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            if (_arguments == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();
            }

            return ((IEnumerable<KeyValuePair<string, string>>)_arguments).GetEnumerator();
        }

        public ICollection<string> Keys => _arguments.Keys;

        public string Named(string index)
        {
            if (_arguments == null)
            {
                return null;
            }

            if (_arguments.TryGetValue(index, out string result))
            {
                return result;
            }

            return null;
        }

        public string NamedOrDefault(string name)
        {
            return Named(name) ?? Named("0");
        }

        public string NamedOrAt(string name, int index)
        {
            return Named(name) ?? Named(index.ToString());
        }

        public string At(int index)
        {
            return Named(index.ToString());
        }

        public int Count => _arguments.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_arguments == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();
            }

            return ((IEnumerable<KeyValuePair<string, string>>)_arguments).GetEnumerator();
        }

        public bool Any()
        {
            return _arguments != null && _arguments.Count > 0;
        }
    }
}
