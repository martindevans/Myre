using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Myre.Graphics
{
    public static class Content
    {
        private static ResourceContentManager manager;
        private static ContentManager content;

        public static void Initialise(IServiceProvider services)
        {
#if WINDOWS
            manager = new ResourceContentManager(services, x86Resources.ResourceManager);
#else
            manager = new ResourceContentManager(services, XboxResources.ResourceManager);
#endif
        }

        public static T Load<T>(string resource)
        {
            if (manager == null)
                throw new Exception("Myre.Graphics.Content.Initialise() must be called before Myre.Graphics can load its' resources.");

            if (content != null)
            {
                try
                {
                    return content.Load<T>(Path.Combine("Myre.Graphics", resource));
                }
                catch (ContentLoadException)
                {
                }
            }

            return manager.Load<T>(resource);
        }

        public static void InjectDefaultContentManager(ContentManager manager)
        {
            content = manager;
        }
    }
}
