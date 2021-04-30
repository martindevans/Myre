using System;
using System.Collections.Generic;
using Myre.Extensions;

namespace Myre.Entities.Behaviours
{
    internal interface IManagerHandler
    {
        IBehaviourManager Manager { get; set; }
        void Add(Behaviour behaviour);
        void Remove(Behaviour behaviour);
    }

    internal class ManagerHandler<T>
        : IManagerHandler
        where T : Behaviour
    {
        private IBehaviourManager<T>? _manager;

        public IBehaviourManager Manager
        {
            get => _manager!;
            set => _manager = value as IBehaviourManager<T>;
        }

        public void Add(T behaviour)
        {
            if (behaviour.CurrentManager.Handler != null)
                behaviour.CurrentManager.Handler.Remove(behaviour);

            _manager!.Add(behaviour);

            behaviour.CurrentManager = new Behaviour.ManagerBinding(this, typeof(T));
        }

        public void Remove(T behaviour)
        {
            if (behaviour.CurrentManager.Handler != this)
                return;

            behaviour.CurrentManager = default;

            _manager!.Remove(behaviour);
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

        private readonly List<IBehaviourManager> _managers = new();
        private readonly Dictionary<Type, IBehaviourManager> _byType = new();
        private readonly Dictionary<Type, IManagerHandler> _byBehaviour = new();
        private readonly Dictionary<Type, PrivateList> _catagorised = new();

        private static readonly Type _managerHandlerType = typeof(ManagerHandler<>);
        private static readonly Type _listType = typeof(List<>);

        public void Add(IBehaviourManager manager)
        {
            var managerType = manager.GetType();

            _managers.Add(manager);
            _byType[managerType] = manager;

            foreach (var type in manager.GetManagedTypes())
            {
                if (!_byBehaviour.TryGetValue(type, out IManagerHandler handler))
                {
                    var handlerType = _managerHandlerType.MakeGenericType(type);
                    handler = (IManagerHandler)Activator.CreateInstance(handlerType);
                    _byBehaviour[type] = handler;
                }

                handler.Manager = manager;
            }

            CatagoriseManager(manager);
        }

        private void CatagoriseManager(IBehaviourManager manager)
        {
            foreach (var item in manager.GetType().GetImplementedTypes())
            {
                if (_catagorised.TryGetValue(item, out var list))
                    AddToList(list, item, manager);
            }
        }

        private void AddToList(PrivateList list, Type type, IBehaviourManager manager)
        {
            var listType = list.List.GetType();
            var addMethod = listType.GetMethod("Add", new Type[] { type });
            addMethod!.Invoke(list.List, new object[] { manager });
        }

        public bool Remove(IBehaviourManager manager)
        {
            var removed = _managers.Remove(manager);

            if (removed)
            {
                var managerType = manager.GetType();
                _byType.Remove(managerType);

                foreach (var type in manager.GetManagedTypes())
                {
                    var handler = _byBehaviour[type];
                    handler.Manager = null;
                }

                foreach (var type in managerType.GetImplementedTypes())
                {
                    if (_catagorised.TryGetValue(type, out var list))
                        RemoveFromList(list, type, manager);
                }
            }

            return removed;
        }

        private void RemoveFromList(PrivateList list, Type type, IBehaviourManager manager)
        {
            var listType = list.List.GetType();
            var removeMethod = listType.GetMethod("Remove", new Type[] { type });
            removeMethod!.Invoke(list.List, new object[] { manager });
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

        public IManagerHandler? Find(Type behaviourType, IBehaviourManager? manager = null)
        {
            var bt = behaviourType;
            var behaviour = typeof(Behaviour);
            while (behaviour.IsAssignableFrom(behaviourType) && bt != null)
            {
                if (TryGetByBehaviour(bt, out IManagerHandler handler) && (manager == null || handler.Manager == manager))
                    return handler;

                bt = bt.BaseType;
            }

            return null;
        }

        public IReadOnlyList<T> FindByType<T>()
        {
            var type = typeof(T);

            if (!_catagorised.TryGetValue(type, out var list))
            {
                list = CreatePrivateList(type);
                _catagorised[type] = list;
            }

            return (IReadOnlyList<T>)list.List;
        }

        private PrivateList CreatePrivateList(Type type)
        {
            var listType = _listType.MakeGenericType(type);

            PrivateList list;
            list.List = Activator.CreateInstance(listType);

            var addMethod = list.List.GetType().GetMethod("Add", new Type[] { type });
            foreach (var manager in _managers)
            {
                var managerType = manager.GetType();
                if (type.IsAssignableFrom(managerType))
                    addMethod!.Invoke(list.List, new object[] { manager });
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
