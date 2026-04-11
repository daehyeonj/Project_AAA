public sealed class ExpeditionPostRunRevealState
{
    public bool HasLatestResult;
    public bool HasPendingReveal;
    public bool CanOpenExpeditionPrep;
    public string RevealToken = string.Empty;
    public string ResultStateKey = string.Empty;
    public string HeadlineText = "None";
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public ExpeditionResult LatestExpeditionResult = new ExpeditionResult();
    public OutcomeReadback OutcomeReadback = new OutcomeReadback();
    public CityWriteback CityWriteback = new CityWriteback();
    public WorldWriteback WorldWriteback = new WorldWriteback();
}
