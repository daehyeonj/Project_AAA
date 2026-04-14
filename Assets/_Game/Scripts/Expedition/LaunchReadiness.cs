public sealed partial class LaunchReadiness
{
    public bool IsReady;
    public string ReadinessKey = "none";
    public string ReadinessText = "None";
    public string DispatchPolicyText = "None";
    public string NeedPressureText = "None";
    public string RecoveryProgressText = "None";
    public string RecoveryEtaText = "None";
    public string RecommendationReasonText = "None";
    public string ExpectedNeedImpactText = "None";
    public string RecommendedActionText = "None";
    public GateResult GateResult = new GateResult();
}
