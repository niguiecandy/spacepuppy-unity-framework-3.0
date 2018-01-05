﻿#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Spawn.Events
{

    public class i_Spawn : AutoTriggerable, IObservableTrigger, ISpawnPoint
    {

        public const string TRG_ONSPAWNED = "OnSpawned";
        
        #region Fields

        [SerializeField()]
        [Tooltip("If left empty the default SpawnPool will be used instead.")]
        private SpawnPool _spawnPool;
        
        [SerializeField]
        private Transform _spawnedObjectParent;

        [SerializeField()]
        [WeightedValueCollection("Weight", "Prefab")]
        [Tooltip("Objects available for spawning. When spawn is called with no arguments a prefab is selected at random, unless a ISpawnSelector is available on the SpawnPoint.")]
        private List<PrefabEntry> _prefabs;

        [SerializeField()]
        private SPEvent _onSpawnedObject = new SPEvent(TRG_ONSPAWNED);

        #endregion

        #region Properties

        public SpawnPool SpawnPool
        {
            get { return _spawnPool; }
            set { _spawnPool = value; }
        }

        public List<PrefabEntry> Prefabs
        {
            get { return _prefabs; }
        }
        
        public SPEvent OnSpawnedObject
        {
            get { return _onSpawnedObject; }
        }

        #endregion

        #region Methods

        public GameObject Spawn()
        {
            if (!this.CanTrigger) return null;

            if (_prefabs == null || _prefabs.Count == 0) return null;

            if(_prefabs.Count == 1)
            {
                return this.Spawn(_prefabs[0].Prefab);
            }
            else
            {
                return this.Spawn(_prefabs.PickRandom((o) => o.Weight).Prefab);
            }
        }

        private GameObject Spawn(GameObject prefab)
        {
            if (prefab == null) return null;

            var pool = _spawnPool != null ? _spawnPool : SpawnPool.DefaultPool;
            var go = pool.Spawn(prefab, this.transform.position, this.transform.rotation, _spawnedObjectParent);
            
            if (_onSpawnedObject != null && _onSpawnedObject.Count > 0)
                _onSpawnedObject.ActivateTrigger(this, go);

            return go;
        }

        public GameObject Spawn(int index)
        {
            if (!this.enabled) return null;

            if (_prefabs == null || index < 0 || index >= _prefabs.Count) return null;
            return this.Spawn(_prefabs[index].Prefab);
        }

        public GameObject Spawn(string name)
        {
            if (!this.enabled) return null;

            if (_prefabs == null) return null;
            for (int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].Prefab != null && _prefabs[i].Prefab.name == name) return this.Spawn(_prefabs[i].Prefab);
            }
            return null;
        }

        #endregion


        #region ITriggerable Interface

        public override bool CanTrigger
        {
            get { return base.CanTrigger && _prefabs != null && _prefabs.Count > 0; }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (arg is string)
            {
                return this.Spawn(arg as string) != null;
            }
            else if (ConvertUtil.ValueIsNumericType(arg))
            {
                return this.Spawn(ConvertUtil.ToInt(arg)) != null;
            }
            else
            {
                return this.Spawn() != null;
            }
        }

        #endregion

        #region IObserverableTarget Interface

        SPEvent[] IObservableTrigger.GetEvents()
        {
            return new SPEvent[] { _onSpawnedObject };
        }

        #endregion

        #region ISpawnPoint Interface

        SPEvent ISpawnPoint.OnSpawned
        {
            get { return _onSpawnedObject; }
        }

        void ISpawnPoint.Spawn()
        {
            this.Spawn();
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct PrefabEntry
        {
            public float Weight;
            public GameObject Prefab;
        }

        #endregion

    }

}