using System.Collections.Generic;
using UnityEngine;

public class LoopForExpression : SkyEventExpression {

    private const int MAX_LOOPS = 10000;

    private string variableName;
    private int min;
    private int max;
    private bool initialized;

    private string minExp;
    private string maxExp;

    private List<SkyEventExpression> internalExpressions;
    private int executionHead;

    public LoopForExpression(string arg, List<SkyEventExpression> internalExpressions) {
        var split1 = arg.Split('=');
        if(split1.Length != 2) {
            min = 0;
            max = 0;
            variableName = "NA";
            return;
        }
        var split2 = split1[1].Split(':');
        if(split2.Length != 2) {
            min = 0;
            max = 0;
            variableName = "NA";
            return;
        }

        variableName = split1[0].Trim();
        minExp = split2[0];
        maxExp = split2[1];

        this.internalExpressions = internalExpressions;
        this.executionHead = 0;
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        if(!initialized) {
            var minResult = executingProgram.Calculate(minExp);
            var maxResult = executingProgram.Calculate(maxExp);

            if(minResult.HasValue && maxResult.HasValue) {
                min = Mathf.RoundToInt(minResult.Value);
                max = Mathf.RoundToInt(maxResult.Value);
            } else {
                //TODO handle error
                return SkyEventExpressionResult.Error;
            }

            executingProgram.variables[variableName] = min;

            initialized = true;
        }

        while(true) {
            if(executingProgram.variables[variableName] >= max) {
                return SkyEventExpressionResult.Success;
            }

            var result = internalExpressions[executionHead].Execute(executingProgram);
            if(result == SkyEventExpressionResult.Success) {
                executionHead++;
            } else if(result == SkyEventExpressionResult.Wait) {
                return SkyEventExpressionResult.Wait;
            } else if(result == SkyEventExpressionResult.Error) {
                //TODO handle error
                executionHead = internalExpressions.Count + 1;
                return SkyEventExpressionResult.Error;
            }

            if(executionHead >= internalExpressions.Count) {
                executingProgram.variables[variableName] = executingProgram.variables[variableName] + 1;
                executionHead = 0;
                ResetInternalExpressions();
            }
        }
    }

    public override void ResetExpression() {
        ResetInternalExpressions();

        initialized = false;
    }

    private void ResetInternalExpressions() {
        foreach(var instruction in internalExpressions) {
            instruction.ResetExpression();
        }
    }

}