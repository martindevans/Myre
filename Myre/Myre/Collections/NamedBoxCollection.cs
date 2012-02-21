using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Collections
{
    public class NamedBoxCollection
        :BoxedValueStore<string>, INamedDataProvider
    {
        public Box<T> Get<T>(string name, bool create = true)
        {
            return base.Get<T>(name, default(T), create);
        }
    }
}
