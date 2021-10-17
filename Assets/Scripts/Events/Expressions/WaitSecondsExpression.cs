using UnityEngine;

public class WaitSecondsExpression : SkyEventExpression {

    private string arg;
    private bool started;
    private float initialTime;

    public WaitSecondsExpression(string arg) {
        this.arg = arg;
        started = false;
        initialTime = 0;
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        if(!started) {
            started = true;
            var result = executingProgram.Calculate(arg);
            if(result.HasValue) {
                initialTime = Time.time + result.Value;
            } else {
                return SkyEventExpressionResult.Error;
            }
        }

        if(Time.time > initialTime) {
            return SkyEventExpressionResult.Success;
        } else {
            return SkyEventExpressionResult.Wait;
        }
    }

    public override void ResetExpression() {
        started = false;
        initialTime = 0;
    }

}