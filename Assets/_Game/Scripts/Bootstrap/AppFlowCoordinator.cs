public sealed class AppFlowCoordinator
{
    private readonly AppFlowContext _context = new AppFlowContext();

    public AppFlowStage CurrentStage { get; private set; }

    public string LastTransition { get; private set; }

    public AppFlowContext Context => _context;

    public AppFlowCoordinator()
    {
        CurrentStage = AppFlowStage.Boot;
        LastTransition = "(none yet)";
    }

    public bool TryEnterMainMenu()
    {
        if (!TrySetStage(AppFlowStage.MainMenu))
        {
            return false;
        }

        ClearWorldSelection();
        ClearRunContracts(clearLatestResult: true);
        return true;
    }

    public bool TryEnterWorldSim(AppFlowObservedSnapshot snapshot)
    {
        if (!TrySetStage(AppFlowStage.WorldSim))
        {
            return false;
        }

        _context.WorldSelection = snapshot != null ? snapshot.WorldSelection : new AppFlowWorldSelection();
        ClearRunContracts(clearLatestResult: true);
        return true;
    }

    public bool TryEnterExpeditionPrep(AppFlowObservedSnapshot snapshot)
    {
        if (!TrySetStage(AppFlowStage.ExpeditionPrep))
        {
            return false;
        }

        AbsorbSnapshot(snapshot);
        _context.PendingBattle = new AppFlowBattleContext();
        _context.CurrentDungeonRun = new AppFlowDungeonRunContext();
        _context.LatestResult = new AppFlowResultContext();
        return true;
    }

    public bool TryReturnToWorld(AppFlowObservedSnapshot snapshot)
    {
        if (!TrySetStage(AppFlowStage.WorldSim))
        {
            return false;
        }

        AbsorbSnapshot(snapshot);
        _context.WorldSelection = snapshot != null ? snapshot.WorldSelection : new AppFlowWorldSelection();
        ClearRunContracts(clearLatestResult: false);
        return true;
    }

    public bool Synchronize(AppFlowObservedSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return false;
        }

        AbsorbSnapshot(snapshot);
        if (snapshot.HasPendingWorldReturn)
        {
            return false;
        }

        if (CurrentStage == AppFlowStage.WorldSim && snapshot.ObservedStage == AppFlowStage.CityHub)
        {
            return TrySetStage(AppFlowStage.CityHub);
        }

        if (CurrentStage == AppFlowStage.CityHub && snapshot.ObservedStage == AppFlowStage.WorldSim)
        {
            return TrySetStage(AppFlowStage.WorldSim);
        }

        if (CurrentStage == AppFlowStage.ExpeditionPrep && snapshot.ObservedStage == AppFlowStage.DungeonRun)
        {
            return TrySetStage(AppFlowStage.DungeonRun);
        }

        if (CurrentStage == AppFlowStage.ExpeditionPrep && snapshot.ObservedStage == AppFlowStage.BattleScene)
        {
            bool changed = TrySetStage(AppFlowStage.DungeonRun);
            changed |= TrySetStage(AppFlowStage.BattleScene);
            return changed;
        }

        if (CurrentStage == AppFlowStage.ExpeditionPrep && snapshot.ObservedStage == AppFlowStage.ResultPipeline)
        {
            bool changed = TrySetStage(AppFlowStage.DungeonRun);
            changed |= TrySetStage(AppFlowStage.ResultPipeline);
            return changed;
        }

        if (CurrentStage == AppFlowStage.DungeonRun && snapshot.ObservedStage == AppFlowStage.BattleScene)
        {
            return TrySetStage(AppFlowStage.BattleScene);
        }

        if (CurrentStage == AppFlowStage.DungeonRun && snapshot.ObservedStage == AppFlowStage.ResultPipeline)
        {
            return TrySetStage(AppFlowStage.ResultPipeline);
        }

        if (CurrentStage == AppFlowStage.BattleScene && snapshot.ObservedStage == AppFlowStage.DungeonRun)
        {
            ApplyBattleReturn(snapshot);
            return TrySetStage(AppFlowStage.DungeonRun);
        }

        if (CurrentStage == AppFlowStage.BattleScene && snapshot.ObservedStage == AppFlowStage.ResultPipeline)
        {
            ApplyBattleReturn(snapshot);
            bool changed = TrySetStage(AppFlowStage.DungeonRun);
            changed |= TrySetStage(AppFlowStage.ResultPipeline);
            return changed;
        }

        return false;
    }

    private void AbsorbSnapshot(AppFlowObservedSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        _context.WorldSelection = snapshot.WorldSelection ?? new AppFlowWorldSelection();

        if (snapshot.ActiveExpeditionPlan != null && HasText(snapshot.ActiveExpeditionPlan.CityId))
        {
            _context.ActiveExpeditionPlan = snapshot.ActiveExpeditionPlan;
        }

        if (snapshot.CurrentDungeonRun != null && HasText(snapshot.CurrentDungeonRun.DungeonId))
        {
            _context.CurrentDungeonRun = snapshot.CurrentDungeonRun;
        }

        if (snapshot.PendingBattle != null && HasText(snapshot.PendingBattle.EncounterName))
        {
            _context.PendingBattle = snapshot.PendingBattle;
        }

        if (snapshot.LatestResult != null && HasResult(snapshot.LatestResult))
        {
            _context.LatestResult = snapshot.LatestResult;
        }
    }

    private void ApplyBattleReturn(AppFlowObservedSnapshot snapshot)
    {
        AppFlowDungeonRunContext runContext = snapshot != null && snapshot.CurrentDungeonRun != null
            ? snapshot.CurrentDungeonRun
            : new AppFlowDungeonRunContext();
        BattleReturnPayload returnPayload = snapshot != null && snapshot.PendingBattle != null
            ? snapshot.PendingBattle.ReturnPayload
            : null;
        PrototypeBattleResultSnapshot resultSnapshot = snapshot != null && snapshot.PendingBattle != null
            ? snapshot.PendingBattle.ResultSnapshot
            : null;

        if (returnPayload != null && HasText(returnPayload.OutcomeKey) && returnPayload.OutcomeKey != PrototypeBattleOutcomeKeys.None)
        {
            runContext.LastBattleOutcomeKey = returnPayload.OutcomeKey;
        }
        else if (resultSnapshot != null && HasText(resultSnapshot.ResultStateKey) && resultSnapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None)
        {
            runContext.LastBattleOutcomeKey = resultSnapshot.ResultStateKey;
        }

        if (returnPayload != null && HasText(returnPayload.EncounterName))
        {
            runContext.LastEncounterName = returnPayload.EncounterName;
        }
        else if (resultSnapshot != null && HasText(resultSnapshot.EncounterName))
        {
            runContext.LastEncounterName = resultSnapshot.EncounterName;
        }

        _context.CurrentDungeonRun = runContext;
        _context.PendingBattle = new AppFlowBattleContext();
    }

    private void ClearWorldSelection()
    {
        _context.WorldSelection = new AppFlowWorldSelection();
    }

    private void ClearRunContracts(bool clearLatestResult)
    {
        _context.ActiveExpeditionPlan = new AppFlowExpeditionPlan();
        _context.CurrentDungeonRun = new AppFlowDungeonRunContext();
        _context.PendingBattle = new AppFlowBattleContext();
        if (clearLatestResult)
        {
            _context.LatestResult = new AppFlowResultContext();
        }
    }

    private bool TrySetStage(AppFlowStage nextStage)
    {
        if (CurrentStage == nextStage || !IsAllowedTransition(CurrentStage, nextStage))
        {
            return false;
        }

        LastTransition = CurrentStage + " -> " + nextStage;
        CurrentStage = nextStage;
        return true;
    }

    private static bool IsAllowedTransition(AppFlowStage from, AppFlowStage to)
    {
        return from == AppFlowStage.Boot
            ? to == AppFlowStage.MainMenu
            : from == AppFlowStage.MainMenu
                ? to == AppFlowStage.WorldSim
                : from == AppFlowStage.WorldSim
                    ? to == AppFlowStage.MainMenu || to == AppFlowStage.CityHub || to == AppFlowStage.ExpeditionPrep
                    : from == AppFlowStage.CityHub
                        ? to == AppFlowStage.WorldSim || to == AppFlowStage.MainMenu || to == AppFlowStage.ExpeditionPrep
                        : from == AppFlowStage.ExpeditionPrep
                            ? to == AppFlowStage.WorldSim || to == AppFlowStage.DungeonRun
                            : from == AppFlowStage.DungeonRun
                                ? to == AppFlowStage.BattleScene || to == AppFlowStage.ResultPipeline
                                : from == AppFlowStage.BattleScene
                                    ? to == AppFlowStage.DungeonRun
                                    : from == AppFlowStage.ResultPipeline
                                        ? to == AppFlowStage.WorldSim
                                        : false;
    }

    private static bool HasResult(AppFlowResultContext resultContext)
    {
        return resultContext != null &&
               (HasText(resultContext.ExpeditionOutcome != null ? resultContext.ExpeditionOutcome.SourceCityId : string.Empty) ||
                HasText(resultContext.OutcomeReadback != null ? resultContext.OutcomeReadback.SourceCityId : string.Empty) ||
                HasText(resultContext.AppliedWorldStateMarker));
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }
}
