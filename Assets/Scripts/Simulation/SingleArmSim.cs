using UnityEngine;

public class SingleArmSim : ISimulator {

    [SerializeField]
    protected float weight;

    [SerializeField]
    protected float momInertia;

    [SerializeField]
    protected float length;

    [SerializeField]
    protected float initialSpring;

    [SerializeField]
    protected float damping;

    [SerializeField]
    protected float theta;
    [SerializeField]
    protected float thetaDot;
    [SerializeField]
    protected float thetaDotDot;

    [SerializeField]
    protected GameObject[] tops;
    [SerializeField]
    protected GameObject[] legs;

    [SerializeField]
    protected float springDecayPerSecondPerDamage;

    [SerializeField]
    protected float maxVelocity;

    [SerializeField]
    protected float amp;

    [SerializeField]
    [FMODUnity.EventRef]
    protected string crashEvent;

    [SerializeField]
    [FMODUnity.EventRef]
    protected string creakEvent;

    [SerializeField]
    [FMODUnity.EventRef]
    protected string breakEvent;

    [SerializeField]
    [FMODUnity.EventRef]
    protected string longCreak;

    protected bool isSimulating = true;

    public float timeSinceLastCreak = 0;
    protected int numBreaks;

    [SerializeField]
    protected BraceController[] braces;

    [SerializeField]
    protected float trickCooldown;

    protected float perfectlyBalancedTimer;
    protected float onTheEdgeTimer;
    protected float maxSpeedTimer;
    protected float shakerTimer;

    protected bool edgePrimed;
    protected float perfectlyBalancedTime;
    protected System.Collections.Generic.List<float> shakerList;

    [SerializeField]
    protected float edgeMargin;
    [SerializeField]
    protected float minPerfectlyBalancedTime;
    [SerializeField]
    protected float perfectlyBalancedMargin;
    [SerializeField]
    protected float maxShakerTime;
    [SerializeField]
    protected int maxShakerAmount;

    protected bool side;
    protected bool balancePrimed;

    public void Start() {
        shakerList = new System.Collections.Generic.List<float>();
        perfectlyBalancedTimer = trickCooldown;
        onTheEdgeTimer = trickCooldown;
        maxSpeedTimer = trickCooldown;
        shakerTimer = trickCooldown;
        balancePrimed = false;
    }

    public override void SimulateTick(float time) {
        if(!isSimulating) {
            return;
        }

        CheckScoring(time);

        timeSinceLastCreak += Time.deltaTime;
        if(timeSinceLastCreak > 5f && Mathf.Abs(thetaDot) > 0.15f && Mathf.Abs(theta) > 0.75f) {
            timeSinceLastCreak = 0;
            FMODUnity.RuntimeManager.PlayOneShot(creakEvent);
        }

        if(numBreaks == 0 && initialSpring < 45) {
            numBreaks++;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(1);
            }
        }

        if(numBreaks == 1 && initialSpring < 35) {
            numBreaks++;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(2);
            }
        }

        if(numBreaks == 2 && initialSpring < 25) {
            numBreaks++;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(3);
            }
        }

        if(numBreaks == 3 && initialSpring < 15) {
            numBreaks++;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);
        }

        if(numBreaks == 4 && initialSpring < 5) {
            numBreaks++;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);
        }

        thetaDot = Mathf.Clamp(thetaDot, -maxVelocity, maxVelocity);
        //Verlet Integration

        var halfThetaDot = thetaDot + 0.5f * thetaDotDot * time;
        theta += halfThetaDot * time;

        float horizForce = 0;
        foreach(float impulse in impulses) {
            horizForce += impulse;
        }

        for(int i = smoothedImpulses.Count - 1; i >= 0; i--) {
            var curImpulse = smoothedImpulses[i];
            curImpulse.time -= time;
            horizForce += curImpulse.impulse * Mathf.Clamp(theta * theta, 0.2f, 1.5f);
            
            if(curImpulse.time <= 0) {
                smoothedImpulses.RemoveAt(i);
            } else {
                smoothedImpulses[i] = curImpulse;
            }
        }

        foreach(var force in forces.Values) {
            horizForce += force * time;
        }

        float moment = weight * Mathf.Sin(theta) * length + horizForce * Mathf.Cos(theta) * length - theta * initialSpring;

        thetaDotDot = moment / momInertia - damping * thetaDot;
        thetaDot = halfThetaDot + 0.5f * thetaDotDot * time;

        foreach(var top in tops) {
            top.transform.position = transform.position + new Vector3(Mathf.Sin(theta * amp) * length, Mathf.Cos(theta * amp) * length, 0);
        }
        foreach(var leg in legs) {
            leg.transform.rotation = Quaternion.Euler(0, 0, -theta * Mathf.Rad2Deg);
        }
        foreach(var ind in indicators) {
            ind.transform.position = new Vector3(transform.position.x + Mathf.Sin(theta * amp) * length, ind.transform.position.y, ind.transform.position.z);
        }

        repairIndicator.transform.position = Vector3.Lerp(repairIndicator.transform.position, 
            new Vector3(repairIndicatorBase.position.x,
            repairIndicatorBase.position.y - repairIndicatorSize / 2 + repairIndicatorSize * (initialSpring / 62f),
            repairIndicator.transform.position.z),
            3f * Time.deltaTime);

        if((theta <= (-70 * Mathf.Deg2Rad) || theta >= (70 * Mathf.Deg2Rad)) && BeatmapController.Instance.songIsRunning) {
            FMODUnity.RuntimeManager.PlayOneShot(crashEvent);
            StartCoroutine(EndCoroutine(BeatmapController.Instance.GetTimeLeft()));
            BeatmapController.Instance.EndSong(true);
        }

        if(theta <= (-Mathf.PI / 2) || theta >= (Mathf.PI / 2)) {
            isSimulating = false;
        }
    }

    private void CheckScoring(float time) {
        if(shakerList == null) {
            shakerList = new System.Collections.Generic.List<float>();
        }

        perfectlyBalancedTimer += time;
        onTheEdgeTimer += time;
        maxSpeedTimer += time;
        shakerTimer += time;

        var eulerAngle = theta * Mathf.Rad2Deg;

        if(eulerAngle < 0) {
            if(side == true) {
                shakerList.Add(0);
            }
            side = false;
        } else {
            if(side == false) {
                shakerList.Add(0);
            }
            side = true;
        }

        for(int i = shakerList.Count - 1; i >= 0; i--) {
            shakerList[i] += time;
            if(shakerList[i] > maxShakerTime) {
                shakerList.RemoveAt(i);
            }
        }

        if(shakerList.Count >= maxShakerAmount && shakerTimer > trickCooldown) {
            BeatmapController.Instance.score.Shaker();
            shakerTimer = 0;
            shakerList.Clear();
        }
        if(Mathf.Abs(eulerAngle) > edgeMargin) {
            edgePrimed = true;
        } else if(edgePrimed && onTheEdgeTimer > trickCooldown) {
            BeatmapController.Instance.score.OnTheEdge();
            onTheEdgeTimer = 0;
            edgePrimed = false;
        }

        if(maxVelocity - Mathf.Abs(thetaDot) < Mathf.Epsilon && maxSpeedTimer > trickCooldown) {
            BeatmapController.Instance.score.MaxSpeed();
            maxSpeedTimer = 0;
        }

        if(Mathf.Abs(eulerAngle) < perfectlyBalancedMargin) {
            if(balancePrimed) {
                perfectlyBalancedTime += time;
            }
        } else {
            perfectlyBalancedTime = 0;
            balancePrimed = true;
        }

        if(perfectlyBalancedTime > minPerfectlyBalancedTime && perfectlyBalancedTimer > trickCooldown) {
            BeatmapController.Instance.score.PerfectlyBalanced();
            perfectlyBalancedTimer = 0;
        }
    }

    public System.Collections.IEnumerator EndCoroutine(float time) {
        yield return new WaitForSeconds(4f);
        GameManager.Instance.ToFailScreen(time);
        BeatmapController.Instance.ExecuteFailPrograms();
    }

    public override void OnDamage(float damage) {
        base.OnDamage(damage);

        initialSpring -= damage * springDecayPerSecondPerDamage * Time.deltaTime;
        initialSpring = Mathf.Max(initialSpring, 0);
    }

    public override void Reset() {
        base.Reset();

        numBreaks = 0;
        isSimulating = true;
        theta = 0;
        thetaDot = 0;
        thetaDotDot = 0;
        initialSpring = 55;
        
        shakerList = new System.Collections.Generic.List<float>();
        perfectlyBalancedTimer = trickCooldown;
        onTheEdgeTimer = trickCooldown;
        maxSpeedTimer = trickCooldown;
        shakerTimer = trickCooldown;
        balancePrimed = false;
        edgePrimed = false;

        foreach(var b in braces) {
            b.SetStage(0);
        }
    }

    public override void Stop() {
        isSimulating = false;
    }

    public override float GetAngle() {
        return theta * Mathf.Rad2Deg;
    }

    public override float GetRepair() {
        return initialSpring / 55;
    }

    public override void Repair(float i) {
        base.Repair(i);

        initialSpring += i;
        initialSpring = Mathf.Clamp(initialSpring, 0, 62);

        if(numBreaks == 2 && initialSpring > 25) {
            numBreaks--;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(2);
            }
        }

        if(numBreaks == 1 && initialSpring > 35) {
            numBreaks--;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(1);
            }
        }

        if(numBreaks == 0 && initialSpring > 45) {
            numBreaks--;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(0);
            }
        }
    }

    public override void SetRepair(float i) {
        base.SetRepair(i);

        initialSpring = i;
        initialSpring = Mathf.Clamp(initialSpring, 0, 62);

        if(numBreaks == 2 && initialSpring > 25) {
            numBreaks--;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(2);
            }
        }

        if(numBreaks == 1 && initialSpring > 35) {
            numBreaks--;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(1);
            }
        }

        if(numBreaks == 0 && initialSpring > 45) {
            numBreaks--;
            FMODUnity.RuntimeManager.PlayOneShot(breakEvent);

            foreach(var b in braces) {
                b.SetStage(0);
            }
        }
    }

}