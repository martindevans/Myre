using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Myre.Entities.Behaviours
{
    interface IManagerHandler
    {
        IBehaviourManager Manager { get; set; }
        void Add(Behaviour behaviour);
        void Remove(Behaviour behaviour);
    }

    class ManagerHandler<T>
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
            if (behaviour.CurrentManager.Handler != null)
                behaviour.CurrentManager.Handler.Remove(behaviour);

            _manager.Add(behaviour);

            behaviour.CurrentManager.Handler = this;
            behaviour.CurrentManager.ManagedAs = typeof(T);
        }

        public void Remove(T behaviour)
        {
            if (behaviour.CurrentManager.Handler != this)
                return;

            behaviour.CurrentManager.Handler = null;
            behaviour.CurrentManager.ManagedAs = null;

            _manager.Remove(behaviour);
        }

        #region IManagerHandler Members

        void IManagerHandler.Add(Behaviour behaviour)
        {
            Add(behaviour as T);
        }

        void IManagerHandler.Remove(Behaviour behaviour)
        {
            Remove(behaviour as T);
        }

        #endregion
    }

    class BehaviourManagerContainer
        : IEnumerable<IBehaviourManager>
    {
        struct PrivateList
        {
            public object List;
            public object ReadOnly;
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

                handler.Manager = manager;
            }

            CatagoriseManager(manager);
        }

        private void CatagoriseManager(IBehaviourManager manager)
        {
            foreach (var item in IterateBaseTypesAndInterfaces(manager.GetType()))
            {
                PrivateList list;
                if (_catagorised.TryGetValue(item, out list))
                    AddToList(list, item, manager);
            }
        }

        private IEnumerable<Type> IterateBaseTypesAndInterfaces(Type type)
        {
            for (var t = type; t != null; t = t.BaseType)
                yield return t;

            foreach (var t in type.GetInterfaces())
                yield return t;
        }

        private void AddToList(PrivateList list, Type type, IBehaviourManager manager)
        {
            var listType = list.List.GetType();
            var addMethod = listType.GetMethod("Add", new Type[] { type });
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

                foreach (var type in IterateBaseTypesAndInterfaces(managerType))
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
            return _byType.ContainsKey(managerType);
        }

        public bool Contains(IBehaviourManager manager)
        {
            return _managers.Contains(manager);
        }

        public bool ContainsForBehaviour(Type behaviourType)
        {
            return _byBehaviour.ContainsKey(behaviourType);
        }

        public IBehaviourManager Get(Type managerType)
        {
            return _byType[managerType];
        }

        public bool TryGet(Type managerType, out IBehaviourManager manager)
        {
            return _byType.TryGetValue(managerType, out manager);
        }

        public IManagerHandler GetByBehaviour(Type behaviourType)
        {
            return _byBehaviour[behaviourType];
        }

        public bool TryGetByBehaviour(Type behaviourType, out IManagerHandler manager)
        {
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
            while (behaviour.IsAssignableFrom(behaviourType))
            {
                IManagerHandler handler;
                if (TryGetByBehaviour(behaviourType, out handler) && (manager == null || handler.Manager == manager))
                    return handler;

                behaviourType = behaviourType.BaseType;
            }

            return null;
        }

        public ReadOnlyCollection<T> FindByType<T>()
        {
            var type = typeof(T);

            PrivateList list;
            if (!_catagorised.TryGetValue(type, out list))
            {
                list = CreatePrivateList(type);
                _catagorised[type] = list;
            }

            return list.ReadOnly as ReadOnlyCollection<T>;
        }

        private PrivateList CreatePrivateList(Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var readOnlyType = typeof(ReadOnlyCollection<>).MakeGenericType(type);

            PrivateList list;
            list.List = Activator.CreateInstance(listType);
            list.ReadOnly = Activator.CreateInstance(readOnlyType, list.List);

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
