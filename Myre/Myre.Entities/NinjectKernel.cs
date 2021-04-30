using System;
using Ninject;
using Ninject.Syntax;

namespace Myre.Entities
{
    /// <summary>
    /// Contains a singleton ninject kernel instance.
    /// </summary>
    public static class NinjectKernel
    {
        private static IKernel? _kernel;
        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IKernel Instance
        {
            get
            {
                if (_kernel == null)
                    _kernel = new StandardKernel();

                return _kernel;
            }
        }
    }
}
