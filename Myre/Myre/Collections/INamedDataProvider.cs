using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Collections
{
    public interface INamedDataProvider
    {
        Box<T> Get<T>(string name);
    }
}
