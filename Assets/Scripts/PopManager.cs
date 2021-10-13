using UnityEngine;
using System.Collections;

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

}
