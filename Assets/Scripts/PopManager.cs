using UnityEngine;
using System.Collections.Generic;

public class PopManager : Singleton<PopManager> {

    [SerializeField]
    protected GameObject largePerf;
    [SerializeField]
    protected GameObject smallPerf;
    [SerializeField]
    protected GameObject largeGood;
    [SerializeField]
    protected GameObject smallGood;
    [SerializeField]
    protected GameObject largeOk;
    [SerializeField]
    protected GameObject smallOk;
    [SerializeField]
    protected GameObject largeNope;
    [SerializeField]
    protected GameObject smallNope;

    [SerializeField]
    protected GameObject boo;
    [SerializeField]
    protected GameObject heart;

    [SerializeField]
    protected Dictionary<string, GameObject> pops;

    [SerializeField]
    protected Transform popAnchorLeft;

    [SerializeField]
    protected Transform popAnchorRight;

    [SerializeField]
    protected float popSpread;

    [SerializeField]
    protected Vector2 maxVel;
    [SerializeField]
    protected Vector2 minVel;

    public void DoPerfPop(bool left, bool large) {
        var anchor = left ? popAnchorLeft : popAnchorRight;
        var thing = large ? largePerf : smallPerf;
        var inst = ObjectPool.Instance.GetObject(thing);
        inst.transform.position = anchor.position + new Vector3(Random.Range(-popSpread, popSpread), Random.Range(-popSpread, popSpread), 0);
        var comp = inst.GetComponent<PopSim>();
        comp.velocity = new Vector3((left ? -1 : 1) * Random.Range(minVel.x, maxVel.x), Random.Range(minVel.y, maxVel.y), 0);
    }
    public void DoGoodPop(bool left, bool large) {
        var anchor = left ? popAnchorLeft : popAnchorRight;
        var thing = large ? largeGood : smallGood;
        var inst = ObjectPool.Instance.GetObject(thing);
        inst.transform.position = anchor.position + new Vector3(Random.Range(-popSpread, popSpread), Random.Range(-popSpread, popSpread), 0);
        var comp = inst.GetComponent<PopSim>();
        comp.velocity = new Vector3((left ? -1 : 1) * Random.Range(minVel.x, maxVel.x), Random.Range(minVel.y, maxVel.y), 0);
    }
    public void DoOkPop(bool left, bool large) {
        var anchor = left ? popAnchorLeft : popAnchorRight;
        var thing = large ? largeOk : smallOk;
        var inst = ObjectPool.Instance.GetObject(thing);
        inst.transform.position = anchor.position + new Vector3(Random.Range(-popSpread, popSpread), Random.Range(-popSpread, popSpread), 0);
        var comp = inst.GetComponent<PopSim>();
        comp.velocity = new Vector3((left ? -1 : 1) * Random.Range(minVel.x, maxVel.x), Random.Range(minVel.y, maxVel.y), 0);
    }
    public void DoNopePop(bool left, bool large) {
        var anchor = left ? popAnchorLeft : popAnchorRight;
        var thing = large ? largeNope : smallNope;
        var inst = ObjectPool.Instance.GetObject(thing);
        inst.transform.position = anchor.position + new Vector3(Random.Range(-popSpread, popSpread), Random.Range(-popSpread, popSpread), 0);
        var comp = inst.GetComponent<PopSim>();
        comp.velocity = new Vector3((left ? -1 : 1) * Random.Range(minVel.x, maxVel.x), Random.Range(minVel.y, maxVel.y), 0);
    }

    public void DoHeartPop(Vector3 loc) {
        var inst = ObjectPool.Instance.GetObject(heart);
        inst.transform.position = loc;
        var comp = inst.GetComponent<PopSim>();
        comp.velocity = new Vector3((Random.value < 0.5f ? -1 : 1) * Random.Range(minVel.x, maxVel.x), Random.Range(minVel.y, maxVel.y), 0);
    }

    public void DoBooPop(Vector3 loc) {
        var inst = ObjectPool.Instance.GetObject(boo);
        inst.transform.position = loc;
        var comp = inst.GetComponent<PopSim>();
        comp.velocity = new Vector3((Random.value < 0.5f ? -1 : 1) * Random.Range(minVel.x, maxVel.x), Random.Range(minVel.y, maxVel.y), 0);
    }

    public void DoPop(string type, float x, float y, float xVel, float yVel) {
        if(pops.TryGetValue(type, out var prefab)) {
            var inst = ObjectPool.Instance.GetObject(prefab);
            inst.transform.position = new Vector3(x, y, -1.1f);
            var comp = inst.GetComponent<PopSim>();
            comp.velocity = new Vector3(xVel, yVel, 0);
        }
    }

    public void DoPopSide(string type, float xVel, float yVel, bool side) {
        if(pops.TryGetValue(type, out var prefab)) {
            var inst = ObjectPool.Instance.GetObject(prefab);
            var anchor = side ? popAnchorRight : popAnchorLeft;
            inst.transform.position = anchor.position;
            var comp = inst.GetComponent<PopSim>();
            comp.velocity = new Vector3(xVel, yVel, 0);
        }
    }

    public bool HasPop(string type) {
        return pops.ContainsKey(type);
    }

}
