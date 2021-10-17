public class VariableExpression : SkyEventExpression {

    private string arg;

    public VariableExpression(string arg) {
        this.arg = arg;
    }

    public override SkyEventExpressionResult Execute(SkyEventProgram executingProgram) {
        var argsSplit = arg.Split(new char[] { '=' }, 2);
        var result = executingProgram.Calculate(argsSplit[1].Trim());
        if(result.HasValue) {
            executingProgram.variables[argsSplit[0].Trim()] = result.Value;
            return SkyEventExpressionResult.Success;
        } else {
            return SkyEventExpressionResult.Error;
        }
    }

}