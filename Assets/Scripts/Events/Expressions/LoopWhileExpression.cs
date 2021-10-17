using System.Collections.Generic;

public class LoopWhileExpression : SkyEventExpression {

    private const int MAX_LOOPS = 10000;

    private string arg;

    private List<SkyEventExpression> internalExpressions;
    private int executionHead;

    private int numLoops;

    public LoopWhileExpression(string arg, List<SkyEventExpression> internalExpressions) {
        this.arg = arg;
        this.internalExpressions = internalExpressions;
        this.executionHead = 0;
        this.numLoops = 0;
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        while(true) {
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
                var conditionalResult = executingProgram.CalculateBool(arg);
                if(conditionalResult.HasValue) {
                    if(conditionalResult.Value) {
                        executionHead = 0;
                        ResetInternalExpressions();
                        numLoops++;
                    } else {
                        return SkyEventExpressionResult.Success;
                    }
                }
            }

            if(numLoops > MAX_LOOPS) {
                //TODO handle possible infinite loop
                return SkyEventExpressionResult.Error;
            }
        }
    }

    public override void ResetExpression() {
        ResetInternalExpressions();
    }

    private void ResetInternalExpressions() {
        foreach(var instruction in internalExpressions) {
            instruction.ResetExpression();
        }
    }

}