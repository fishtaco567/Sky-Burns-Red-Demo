using UnityEngine;
using System.Collections.Generic;

public class DispatchEventWithDelayExpression : SkyEventExpression {

    private string arg;
    private bool started;
    private float timeToExecute;

    private Dictionary<string, float> variables;
    private Dictionary<string, float> gameVariables;

    public DispatchEventWithDelayExpression(string arg) {
        this.arg = arg;
        started = false;
        timeToExecute = 0;
    }

    public DispatchEventWithDelayExpression(string arg, bool started, float time, Dictionary<string, float> variables, Dictionary<string, float> gameVariables) {
        this.arg = arg;
        this.started = started;
        timeToExecute = time;
        this.variables = new Dictionary<string, float>(variables);
        this.gameVariables = new Dictionary<string, float>(gameVariables);
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        var split = arg.Split(new char[] { ' ' }, 2);

        if(!started) {
            started = true;
            var result = executingProgram.Calculate(split[0]);
            if(result.HasValue) {
                timeToExecute = Time.time + executingProgram.Calculate(split[0]).Value;
            } else {
                return SkyEventExpressionResult.Error;
            }
            executingProgram.asyncExpressions.Add(new DispatchEventWithDelayExpression(arg, true, timeToExecute, executingProgram.variables, executingProgram.gameVariables));
            return SkyEventExpressionResult.Success;
        }

        if(Time.time > timeToExecute) {
            bool success = executingProgram.DispatchEvent(split[1], variables, gameVariables);
            if(success) {
                return SkyEventExpressionResult.Success;
            } else {
                return SkyEventExpressionResult.Error;
            }
        } else {
            return SkyEventExpressionResult.Wait;
        }
    }

    public override void ResetExpression() {
        started = false;
        timeToExecute = 0;
    }

}