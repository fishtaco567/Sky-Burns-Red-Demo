using System.Collections.Generic;
using System;

public class IfElseExpression : SkyEventExpression {

    private int executionHead;

    private List<(string arg, List<SkyEventExpression> expressions)> ifElseExpressions;

    private bool hasDecided;
    private int activeIfElse;

    public IfElseExpression() {
        ifElseExpressions = new List<(string arg, List<SkyEventExpression> expressions)>();
        this.executionHead = 0;
        hasDecided = false;
        activeIfElse = 0;
    }

    public void AddIfElse(string arg, List<SkyEventExpression> internalExpressions) {
        ifElseExpressions.Add((arg, internalExpressions));
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        if(!hasDecided) {
            for(int i = 0; i < ifElseExpressions.Count; i++) {
                var ifelse = ifElseExpressions[i];
                var bresult = executingProgram.CalculateBool(ifelse.arg);
                if(bresult.HasValue) {
                    if(bresult.Value) {
                        activeIfElse = i;
                        while(true) {
                            var result = ifelse.expressions[executionHead].Execute(executingProgram);
                            if(result == SkyEventExpressionResult.Success) {
                                executionHead++;
                            } else if(result == SkyEventExpressionResult.Wait) {
                                return SkyEventExpressionResult.Wait;
                            } else if(result == SkyEventExpressionResult.Error) {
                                //TODO handle error
                                executionHead = ifelse.expressions.Count + 1;
                                return SkyEventExpressionResult.Error;
                            }

                            if(executionHead >= ifelse.expressions.Count) {
                                return SkyEventExpressionResult.Success;
                            }
                        }
                    }
                } else {
                    //TODO handle error
                    return SkyEventExpressionResult.Error;
                }
            }
        } else {
            var ifelse = ifElseExpressions[activeIfElse];
            while(true) {
                var result = ifelse.expressions[executionHead].Execute(executingProgram);
                if(result == SkyEventExpressionResult.Success) {
                    executionHead++;
                } else if(result == SkyEventExpressionResult.Wait) {
                    return SkyEventExpressionResult.Wait;
                } else if(result == SkyEventExpressionResult.Error) {
                    //TODO handle error
                    executionHead = ifelse.expressions.Count + 1;
                    return SkyEventExpressionResult.Error;
                }

                if(executionHead >= ifelse.expressions.Count) {
                    return SkyEventExpressionResult.Success;
                }
            }
        }

        return SkyEventExpressionResult.Success;
    }

    public override void ResetExpression() {
        foreach(var ifelse in ifElseExpressions) {
            foreach(var instruction in ifelse.expressions) {
                instruction.ResetExpression();
            }
        }
        executionHead = 0;
        hasDecided = false;
        activeIfElse = 0;
    }

}