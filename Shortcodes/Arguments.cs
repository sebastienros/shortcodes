using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shortcodes
{
    public class Arguments : IEnumerable<KeyValuePair<string, string>>
    {
        private Dictionary<string, string> _arguments;

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

        public string NamedOrDefault(string index)
        {
            return Named(index) ?? Named("0");
        }

        public string At(int index)
        {
            return Named(index.ToString());
        }

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
