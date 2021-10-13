using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : Singleton<ObjectPool> {

    [System.Serializable]
    public class Poolable {
        public GameObject prefab;
        public int numPreload;
    }

    [SerializeField]
    protected List<Poolable> poolables;

    [SerializeField]
    protected Dictionary<string, Queue<GameObject>> pools;

    protected void Awake() {
        pools = new Dictionary<string, Queue<GameObject>>();

        foreach(Poolable p in poolables) {
            StartPool(p.prefab, p.numPreload);
        }
    }

    protected Queue<GameObject> StartPool(GameObject prefab, int numPreload) {
        var stack = new Queue<GameObject>(numPreload);
        for(int i = 0; i < numPreload; i++) {
            var thing = Instantiate(prefab);
            thing.name = thing.name.Substring(0, thing.name.Length - 7);
            thing.SetActive(false);

            stack.Enqueue(thing);
        }

        pools.Add(prefab.name, stack);

        return stack;
    }

    public GameObject GetObject(GameObject prefab) {
        if(pools.TryGetValue(prefab.name, out Queue<GameObject> pool)) {
            if(pool.Count != 0) {
                var obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            } else {
                var thing = Instantiate(prefab);
                thing.name = thing.name.Substring(0, thing.name.Length - 7);
                return thing;
            }
        } else {
            var thing = Instantiate(prefab);
            thing.name = thing.name.Substring(0, thing.name.Length - 7);
            return thing;
        }
    }

    public void DestroyObject(GameObject obj) {
        if(pools.TryGetValue(obj.name, out Queue<GameObject> pool)) {
            obj.SetActive(false);


            pool.Enqueue(obj);
        } else {
            var newPool = StartPool(obj, 0);

            obj.SetActive(false);

            newPool.Enqueue(obj);
        }
    }

    public void ReleasePool(GameObject poolType) {
        if(pools.TryGetValue(poolType.name, out Queue<GameObject> pool)) {
            while(pool.Count != 0) {
                Destroy(pool.Dequeue());
            }

            pools.Remove(poolType.name);
        }
    }

}
