using UnityEngine;
using System.Collections.Generic;

public class DispatchEventNowExpression : SkyEventExpression {

    private string arg;

    public DispatchEventNowExpression(string arg) {
        this.arg = arg;
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        bool success = executingProgram.DispatchEvent(arg);
        if(success) {
            return SkyEventExpressionResult.Success;
        } else {
            return SkyEventExpressionResult.Error;
        }
    }

}