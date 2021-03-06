﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// Place on the root of a GameObject hierarchy, or a prefab, to signify that it is a complete entity.
    /// 
    /// If this class is derived from, make sure to set its execution order to the last executed script! 
    /// Failure to do so will result in IEntityAwakeHandler receivers to be messaged out of order.
    /// </summary>
    [DisallowMultipleComponent()]
    public class SPEntity : SPComponent
    {

        #region Multiton Interface

        private static EntityPool _pool = new EntityPool();
        public static EntityPool Pool
        {
            get
            {
                return _pool;
            }
        }

        #endregion

        #region Fields

        [System.NonSerialized()]
        private bool _isAwake;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            this.AddTag(SPConstants.TAG_ROOT);
            Pool.AddReference(this);

            base.Awake();

            _isAwake = true;

            Messaging.Broadcast<IEntityAwakeHandler>(this.gameObject, (h) => h.OnEntityAwake(this));
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Pool.RemoveReference(this);
        }

        #endregion

        #region Properties

        public bool IsAwake { get { return _isAwake; } }

        public new string name
        {
            get
            {
                if (_cachedName == null) _cachedName = this.gameObject.name;
                return _cachedName;
            }
            set
            {
                this.gameObject.name = value;
                _cachedName = value;
            }
        }

        #endregion

        #region Methods

        private string _cachedName;
        public bool CompareName(string value)
        {
            if (_cachedName == null)
            {
                _cachedName = this.gameObject.name;
            }
            return _cachedName == value;
        }

        #endregion

        #region Special Types

        public class EntityPool : MultitonPool<SPEntity>
        {

            public EntityPool() : base(ObjectReferenceEqualityComparer<SPEntity>.Default)
            {

            }

            #region EntityMultiton Methods

            public bool IsSource(object obj)
            {
                if (obj is SPEntity) return true;

                return GetFromSource(obj) != null;
            }

            public virtual SPEntity GetFromSource(object obj)
            {
                if (obj == null) return null;

                SPEntity result = obj as SPEntity;
                if (!object.ReferenceEquals(result, null)) return result;

                var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
                if (go == null) return null;

                result = go.GetComponent<SPEntity>();
                if (!object.ReferenceEquals(result, null)) return result;

                result = go.AddOrGetComponent<SPEntityHook>().GetEntity();
                return result;
            }

            public bool GetFromSource(object obj, out SPEntity comp)
            {
                comp = GetFromSource(obj);
                return comp != null;
            }





            public bool IsSource<TSub>(object obj) where TSub : SPEntity
            {
                if (obj is TSub) return true;

                return GetFromSource<TSub>(obj) != null;
            }

            public virtual TSub GetFromSource<TSub>(object obj) where TSub : SPEntity
            {
                if (obj == null) return null;
                if (obj is TSub) return obj as TSub;

                var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
                if (go == null) return null;

                var e = go.GetComponent<SPEntity>();
                if (!object.ReferenceEquals(e, null)) return e as TSub;

                return go.AddOrGetComponent<SPEntityHook>().GetEntity() as TSub;
            }

            public virtual SPEntity GetFromSource(System.Type tp, object obj)
            {
                if (tp == null || obj == null) return null;
                if (obj is SPEntity) return TypeUtil.IsType(obj.GetType(), tp) ? obj as SPEntity : null;

                var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
                if (go == null) return null;

                var e = go.GetComponent<SPEntity>();
                if (!object.ReferenceEquals(e, null)) return TypeUtil.IsType(e.GetType(), tp) ? e : null;

                e = go.AddOrGetComponent<SPEntityHook>().GetEntity();
                if (!object.ReferenceEquals(e, null)) return TypeUtil.IsType(e.GetType(), tp) ? e : null;

                return null;
            }

            public bool GetFromSource<TSub>(object obj, out TSub comp) where TSub : SPEntity
            {
                comp = GetFromSource<TSub>(obj);
                return comp != null;
            }

            public bool GetFromSource(System.Type tp, object obj, out SPEntity comp)
            {
                comp = GetFromSource(tp, obj);
                return comp != null;
            }

            #endregion

        }

        public class SPEntityHook : MonoBehaviour
        {
            #region Fields

            private SPEntity _entity;
            private bool _synced;

            #endregion

            #region CONSTRUCTOR

            private void OnDisable()
            {
                if (_synced)
                {
                    _synced = false;
                    if (_entity != null && !this.transform.IsChildOf(_entity.transform))
                    {
                        _entity = null;
                    }
                }
            }

            #endregion

            #region Methods

            public SPEntity GetEntity()
            {
                if (!_synced)
                {
                    _synced = true;
                    _entity = this.GetComponentInParent<SPEntity>();
                }
                return _entity;
            }

            private void OnTransformParentChanged()
            {
                _synced = false;
                _entity = null;
            }

            #endregion

        }

        #endregion

    }
}
