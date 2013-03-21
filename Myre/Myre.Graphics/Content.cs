using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics
{
    public static class Content
    {
        private static ResourceContentManager _manager;
        private static ContentManager _content;

        public static void Initialise(IServiceProvider services)
        {
#if WINDOWS
            _manager = new ResourceContentManager(services, x86Resources.ResourceManager);
#else
            manager = new ResourceContentManager(services, XboxResources.ResourceManager);
#endif
        }

        public static T Load<T>(string resource)
        {
            if (_manager == null)
                throw new Exception("Myre.Graphics.Content.Initialise() must be called before Myre.Graphics can load its' resources.");

            if (_content != null)
            {
                try
                {
                    return _content.Load<T>(Path.Combine("Myre.Graphics", resource));
                }
                catch (ContentLoadException)
                {
                }
            }

            return _manager.Load<T>(resource);
        }

        public static void InjectDefaultContentManager(ContentManager manager)
        {
            _content = manager;
        }
    }
}
