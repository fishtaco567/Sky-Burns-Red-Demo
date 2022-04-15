using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class ISimulator : MonoBehaviour {

    protected List<float> impulses;
    protected List<(float impulse, float time)> smoothedImpulses;
    protected Dictionary<int, float> forces;

    [SerializeField]
    protected GameObject[] indicators;

    [SerializeField]
    protected GameObject repairIndicator;
    [SerializeField]
    protected Transform repairIndicatorBase;
    [SerializeField]
    protected float repairIndicatorSize;

    public Transform leftPopAnchor;
    public Transform rightPopAnchor;

    public abstract void SimulateTick(float time);

    protected void Awake() {
        impulses = new List<float>();
        smoothedImpulses = new List<(float impulse, float time)>();
        forces = new Dictionary<int, float>();
    }

    public void ApplyBaseImpulse(float impulse) {
        impulses.Add(impulse);
    }

    public void ApplySmoothedImpulse(float impulse, float time) {
        smoothedImpulses.Add((impulse, time));
    }

    public int StartForce(float force) {
        int lowestUnused = 0;
        foreach(int i in forces.Keys) {
            if(lowestUnused <= i) {
                lowestUnused = i + 1; //Lazy but it works
            }
        }

        forces.Add(lowestUnused, force);
        return lowestUnused;
    }

    public void EndForce(int i) {
        forces.Remove(i);
    }

    public void UpdateForce(int i, float force) {
        if(forces.ContainsKey(i)) {
            forces[i] = force;
        }
    }

    public virtual void OnDamage(float damage) {
    
    }

    public virtual void Reset() {
        impulses.Clear();
        smoothedImpulses.Clear();
        forces.Clear();
    }

    public abstract void Stop();

    public abstract float GetAngle();

    public abstract float GetRepair();

    public virtual void Repair(float i) {
    
    }

    public virtual void SetRepair(float i) {
        
    }

}
