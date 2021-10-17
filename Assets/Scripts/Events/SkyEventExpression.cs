using System.Collections.Generic;
using System;

public abstract class SkyEventExpression {

    public abstract SkyEventExpressionResult Execute(SkyEventProgram executingProgram);

    public virtual void ResetExpression() {
    }

}