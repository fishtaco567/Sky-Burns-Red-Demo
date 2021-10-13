using UnityEngine;

public class DualArmSim : ISimulator {

    [SerializeField]
    protected Vector2 W;

    [SerializeField]
    protected Vector2 momInertia;

    [SerializeField]
    protected Vector2 L;

    [SerializeField]
    protected Vector2 k;

    [SerializeField]
    protected Vector2 damping;

    [SerializeField]
    protected float g;

    [SerializeField]
    protected Vector2 theta;
    [SerializeField]
    protected Vector2 tD;
    [SerializeField]
    protected Vector2 tDD;

    [SerializeField]
    protected GameObject top;
    [SerializeField]
    protected GameObject top2;

    [SerializeField]
    protected GameObject bottom;

    public override void SimulateTick(float time) {
        //Verlet Integration

        var hTd = tD + 0.5f * tDD * time;
        theta += hTd * time;

        float horizForce = 0;
        foreach(float impulse in impulses) {
            horizForce += impulse;
        }

        for(int i = smoothedImpulses.Count - 1; i >= 0; i--) {
            var curImpulse = smoothedImpulses[i];
            curImpulse.time -= time;
            horizForce += curImpulse.impulse;

            if(curImpulse.time <= 0) {
                smoothedImpulses.RemoveAt(i);
            } else {
                smoothedImpulses[i] = curImpulse;
            }
        }

        foreach(var force in forces.Values) {
            horizForce = force * time;
        }

        var rt = new Vector2(theta.x - Mathf.PI, theta.y - Mathf.PI);

        var sinDif = Mathf.Sin(rt.x - rt.y);
        var cosDif = Mathf.Cos(rt.x - rt.y);
        var sinT1 = Mathf.Sin(rt.x);
        var sinT2 = Mathf.Sin(rt.y);

        tDD.x = (W.y * g * sinT2 * cosDif - W.y * sinDif * (L.x * hTd.x * hTd.x * cosDif + L.y * hTd.y * hTd.y) - (W.x + W.y) * g * sinT1 - k.x * theta.x + horizForce * Mathf.Cos(theta.x) * L.x) / 
            (L.x * (W.x + W.y * sinDif * sinDif)) - 
            damping.x * hTd.x;
        tDD.y = ((W.x + W.y) * (L.x * hTd.x * hTd.x * sinDif - g * sinT2 + g * sinT1 * cosDif) + W.y * L.y * hTd.y * hTd.y * sinDif * cosDif - k.y * (theta.y - theta.x) + horizForce * Mathf.Cos(theta.y) * L.y) / 
            (L.y * (W.x + W.y * sinDif * sinDif)) -
            damping.y * hTd.y;

        tD = hTd + 0.5f * tDD * time;

        bottom.transform.position = transform.position;
        top.transform.position = bottom.transform.position + new Vector3(Mathf.Sin(theta.x) * L.x, Mathf.Cos(theta.x) * L.x, 0);
        top2.transform.position = top.transform.position + new Vector3(Mathf.Sin(theta.y) * L.y, Mathf.Cos(theta.y) * L.y, 0);
    }

    public override float GetAngle() {
        return Mathf.Max(theta.x, theta.y);
    }

    public override void Stop() {

    }

}