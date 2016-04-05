using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Myre.Extensions;

namespace Myre.Entities.Behaviours
{
    [ContractClass(typeof(IManagerHandlerContract))]
    internal interface IManagerHandler
    {
        IBehaviourManager Manager { get; set; }
        void Add(Behaviour behaviour);
        void Remove(Behaviour behaviour);
    }

    [ContractClassFor(typeof(IManagerHandler))]
    internal abstract class IManagerHandlerContract : IManagerHandler
    {
        public abstract IBehaviourManager Manager { get; set; }

        public void Add(Behaviour behaviour)
        {
            Contract.Requires(behaviour != null);
        }

        public void Remove(Behaviour behaviour)
        {
            Contract.Requires(behaviour != null);
        }
    }

    internal class ManagerHandler<T>
        : IManagerHandler
        where T : Behaviour
    {
        private IBehaviourManager<T> _manager;

        public IBehaviourManager Manager
        {
            get { return _manager; }
            set { _manager = value as IBehaviourManager<T>; }
        }

        public void Add(T behaviour)
        {
            Contract.Requires(behaviour != null);

            if (behaviour.CurrentManager.Handler != null)
                behaviour.CurrentManager.Handler.Remove(behaviour);

            _manager.Add(behaviour);

            behaviour.CurrentManager = new Behaviour.ManagerBinding
            {
                Handler = this,
                ManagedAs = typeof(T)
            };
        }

        public void Remove(T behaviour)
        {
            Contract.Requires(behaviour != null);

            if (behaviour.CurrentManager.Handler != this)
                return;

            behaviour.CurrentManager = new Behaviour.ManagerBinding
            {
                Handler = null,
                ManagedAs = null
            };

            _manager.Remove(behaviour);
        }

        #region IManagerHandler Members

        void IManagerHandler.Add(Behaviour behaviour)
        {
            Add((T)behaviour);
        }

        void IManagerHandler.Remove(Behaviour behaviour)
        {
            Remove((T)behaviour);
        }

        #endregion
    }

    internal class BehaviourManagerContainer
        : IEnumerable<IBehaviourManager>
    {
        private struct PrivateList
        {
            public object List;
        }

        private readonly List<IBehaviourManager> _managers;
        private readonly Dictionary<Type, IBehaviourManager> _byType;
        private readonly Dictionary<Type, IManagerHandler> _byBehaviour;
        private readonly Dictionary<Type, PrivateList> _catagorised;

        public BehaviourManagerContainer()
        {
            _managers = new List<IBehaviourManager>();
            _byType = new Dictionary<Type, IBehaviourManager>();
            _byBehaviour = new Dictionary<Type, IManagerHandler>();
            _catagorised = new Dictionary<Type, PrivateList>();
        }

        public void Add(IBehaviourManager manager)
        {
            Contract.Requires(manager != null);

            var managerType = manager.GetType();

            _managers.Add(manager);
            _byType[managerType] = manager;

            foreach (var type in manager.GetManagedTypes())
            {
                IManagerHandler handler;
                if (!_byBehaviour.TryGetValue(type, out handler))
                {
                    var handlerType = typeof(ManagerHandler<>).MakeGenericType(type);
                    handler = (IManagerHandler)Activator.CreateInstance(handlerType);
                    _byBehaviour[type] = handler;
                }

                Contract.Assume(handler != null);
                handler.Manager = manager;
            }

            CatagoriseManager(manager);
        }

        private void CatagoriseManager(IBehaviourManager manager)
        {
            Contract.Requires(manager != null);

            foreach (var item in manager.GetType().GetImplementedTypes())
            {
                PrivateList list;
                if (_catagorised.TryGetValue(item, out list))
                    AddToList(list, item, manager);
            }
        }

        private void AddToList(PrivateList list, Type type, IBehaviourManager manager)
        {
            var listType = list.List.GetType();
            var addMethod = listType.GetMethod("Add", new Type[] { type });
            Contract.Assume(addMethod != null);
            addMethod.Invoke(list.List, new object[] { manager });
        }

        public bool Remove(IBehaviourManager manager)
        {
            var removed = _managers.Remove(manager);

            if (removed)
            {
                var managerType = manager.GetType();
                _byType.Remove(managerType);

                foreach (var type in manager.GetManagedTypes())
                    _byBehaviour[type].Manager = null;

                foreach (var type in managerType.GetImplementedTypes())
                {
                    PrivateList list;
                    if (_catagorised.TryGetValue(type, out list))
                        RemoveFromList(list, type, manager);
                }
            }

            return removed;
        }

        private void RemoveFromList(PrivateList list, Type type, IBehaviourManager manager)
        {
            var listType = list.List.GetType();
            var removeMethod = listType.GetMethod("Remove", new Type[] { type });
            removeMethod.Invoke(list.List, new object[] { manager });
        }

        public bool Contains(Type managerType)
        {
            Contract.Requires(managerType != null);

            return _byType.ContainsKey(managerType);
        }

        public bool Contains(IBehaviourManager manager)
        {
            Contract.Requires(manager != null);

            return _managers.Contains(manager);
        }

        public bool ContainsForBehaviour(Type behaviourType)
        {
            Contract.Requires(behaviourType != null);

            return _byBehaviour.ContainsKey(behaviourType);
        }

        public IBehaviourManager Get(Type managerType)
        {
            Contract.Requires(managerType != null);

            return _byType[managerType];
        }

        public bool TryGet(Type managerType, out IBehaviourManager manager)
        {
            Contract.Requires(managerType != null);

            return _byType.TryGetValue(managerType, out manager);
        }

        public IManagerHandler GetByBehaviour(Type behaviourType)
        {
            Contract.Requires(behaviourType != null);

            return _byBehaviour[behaviourType];
        }

        public bool TryGetByBehaviour(Type behaviourType, out IManagerHandler manager)
        {
            Contract.Requires(behaviourType != null);

            return _byBehaviour.TryGetValue(behaviourType, out manager);
        }

        public void Clear()
        {
            _managers.Clear();
            _byBehaviour.Clear();
            _byType.Clear();
        }

        public IManagerHandler Find(Type behaviourType, IBehaviourManager manager = null)
        {
            if (behaviourType == null)
                throw new ArgumentException("behaviourType");

            var behaviour = typeof(Behaviour);
            while (behaviour.IsAssignableFrom(behaviourType) && behaviourType != null)
            {
                IManagerHandler handler;
                if (TryGetByBehaviour(behaviourType, out handler) && (manager == null || handler.Manager == manager))
                    return handler;

                behaviourType = behaviourType.BaseType;
            }

            return null;
        }

        public IReadOnlyList<T> FindByType<T>()
        {
            var type = typeof(T);

            PrivateList list;
            if (!_catagorised.TryGetValue(type, out list))
            {
                list = CreatePrivateList(type);
                _catagorised[type] = list;
            }

            return list.List as IReadOnlyList<T>;
        }

        private PrivateList CreatePrivateList(Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);

            PrivateList list;
            list.List = Activator.CreateInstance(listType);

            var addMethod = list.List.GetType().GetMethod("Add", new Type[] { type });
            foreach (var manager in _managers)
            {
                var managerType = manager.GetType();
                if (type.IsAssignableFrom(managerType))
                    addMethod.Invoke(list.List, new object[] { manager });
            }

            return list;
        }

        #region IEnumerable<IBehaviourManager> Members

        public IEnumerator<IBehaviourManager> GetEnumerator()
        {
            return _managers.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
