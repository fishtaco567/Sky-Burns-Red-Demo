public class WaitUntilExpression : SkyEventExpression {

    private string arg;

    public WaitUntilExpression(string arg) {
        this.arg = arg;
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        var result = executingProgram.CalculateBool(arg);
        if(result.HasValue) {
            if(result.Value) {
                return SkyEventExpressionResult.Success;
            } else {
                return SkyEventExpressionResult.Wait;
            }
        } else {
            return SkyEventExpressionResult.Error;
        }
    }

}