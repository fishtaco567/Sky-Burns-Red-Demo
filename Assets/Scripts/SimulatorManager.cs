using UnityEngine;

public class SimulatorManager : Singleton<SimulatorManager> {

    public ISimulator currentSim;

    protected void FixedUpdate() {
        currentSim.SimulateTick(Time.fixedDeltaTime / 5);
        currentSim.SimulateTick(Time.fixedDeltaTime / 5);
        currentSim.SimulateTick(Time.fixedDeltaTime / 5);
        currentSim.SimulateTick(Time.fixedDeltaTime / 5);
        currentSim.SimulateTick(Time.fixedDeltaTime / 5);
    }

}