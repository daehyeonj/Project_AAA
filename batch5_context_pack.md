# Batch-5 Context Pack

Current repo note:

- The filename is historical. The repo is now at `DungeonRun-Batch-15` per `Assets/_Game/Scripts/Core/PrototypeLocalization.cs:39`.
- This pack only keeps the current slices needed for planner, recommendation, readiness, policy, result, and world return.
- Some sections concatenate multiple verbatim minimal slices from the same file to stay compact.

## A. BootEntry.cs

- File: `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
- Why it matters: Top-level state owner. `Boot -> MainMenu -> WorldSim -> DungeonRun -> WorldSim` is gated here, and planner/result visibility is bridged here.
- Relevant symbols: `Update`, `TryConfirmRouteChoice`, `TryCycleCurrentDispatchPolicy`, `ChangeState`, `HandleWorldSimEconomyInput`
- Exact excerpt:

Lines 182-196
```csharp
    public bool IsDungeonPreEliteChoiceVisible => _worldView != null && _worldView.IsDungeonPreEliteChoiceVisible;
    public bool IsDungeonResultPanelVisible => _worldView != null && _worldView.IsDungeonResultPanelVisible;
    public string DungeonRunStateLabel => _worldView != null ? _worldView.DungeonRunStateText : "None";
    public string CurrentDungeonRunLabel => _worldView != null ? _worldView.CurrentDungeonRunText : "None";
    public string CurrentCityRunLabel => _worldView != null ? _worldView.CurrentCityText : "None";
    public string CurrentDungeonDangerLabel => _worldView != null ? _worldView.CurrentDungeonDangerText : "None";
    public string CurrentCityManaShardStockRunLabel => _worldView != null ? _worldView.CityManaShardStockText : "None";

    public string CurrentNeedPressureRunLabel => _worldView != null ? _worldView.NeedPressureText : "None";
    public string CurrentDispatchReadinessRunLabel => _worldView != null ? _worldView.DispatchReadinessText : "None";
    public string CurrentDispatchRecoveryProgressRunLabel => _worldView != null ? _worldView.DispatchRecoveryProgressText : "None";
    public string CurrentDispatchConsecutiveRunLabel => _worldView != null ? _worldView.DispatchConsecutiveCountText : "None";
    public string CurrentDispatchPolicyRunLabel => _worldView != null ? _worldView.DispatchPolicyText : "None";
    public string RecommendedRouteRunLabel => _worldView != null ? _worldView.RecommendedRouteText : "None";
    public string RecommendationReasonRunLabel => _worldView != null ? _worldView.RecommendationReasonText : "None";
```

Lines 327-388 and 471-483
```csharp
    private void Update()
    {
        if (_gameState == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        bool blockKeyboardShortcuts = IsSearchFieldFocused();
        if (keyboard != null)
        {
            if (!blockKeyboardShortcuts && _gameState.CurrentState == GameStateId.WorldSim)
            {
                HandleWorldSimEconomyInput(keyboard);
            }
        }

        if (_gameState.CurrentState == GameStateId.WorldSim)
        {
            if (_worldView != null)
            {
                _worldView.UpdateAutoTick(Time.deltaTime);
            }

            HandleSelectionInput();
            return;
        }

        if (_gameState.CurrentState == GameStateId.DungeonRun && _worldView != null)
        {
            Keyboard dungeonKeyboard = blockKeyboardShortcuts ? null : keyboard;
            _worldView.UpdateDungeonRun(dungeonKeyboard, Mouse.current, Camera.main);
            if (_worldView.ConsumeDungeonRunExitRequest())
            {
                ChangeState(GameStateId.WorldSim);
            }
        }
    }

    public bool TryConfirmRouteChoice()
    {
        return _worldView != null && _worldView.TryConfirmRouteChoice();
    }

    public bool TryCycleCurrentDispatchPolicy()
    {
        return _worldView != null && _worldView.TryCycleCurrentDispatchPolicy();
    }
```

Lines 540-614
```csharp
    private void ChangeState(GameStateId nextState)
    {
        if (_gameState == null || !_gameState.ChangeState(nextState))
        {
            return;
        }

        Debug.Log("[GameState] " + _gameState.LastTransition);
        ApplyBackgroundColor(nextState);
        UpdateCameraControl(nextState);
        UpdateWorldVisibility(nextState);
    }

    private void HandleWorldSimEconomyInput(Keyboard keyboard)
    {
        if (keyboard == null || _worldView == null)
        {
            return;
        }

        if (keyboard.tKey.wasPressedThisFrame)
        {
            _worldView.RunEconomyDay();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            _worldView.ResetRuntimeEconomy();
        }

        if (keyboard.xKey.wasPressedThisFrame && _worldView.TryEnterSelectedCityDungeon(Camera.main))
        {
            ChangeState(GameStateId.DungeonRun);
        }
    }
```
- Notes: Fresh GPT should treat `BootEntry` as the only top-level transition owner.

## B. StaticPlaceholderWorldView.cs

- File: `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.cs`
- Why it matters: WorldSim bridge for selection, day tick/reset, and selected-city planner summary text.
- Relevant symbols: `SelectAtScreenPosition`, `RunEconomyDay`, `UpdateAutoTick`, `ResetRuntimeEconomy`, `GetSelectedNeedPressureText`, `GetSelectedDispatchReadinessText`, `GetSelectedRecommendedRouteText`, `GetSelectedRecommendationReasonText`, `GetSelectedExpeditionDungeonId`, `GetLinkedDungeonIdForCity`
- Exact excerpt:

Lines 221-287
```csharp
    public void SelectAtScreenPosition(Camera worldCamera, Vector2 screenPosition)
    {
        if (_root == null || !_root.activeInHierarchy || worldCamera == null)
        {
            return;
        }

        Vector3 worldPoint = worldCamera.ScreenToWorldPoint(screenPosition);
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(worldPoint.x, worldPoint.y));
        WorldSelectableMarker marker = hit != null ? hit.GetComponent<WorldSelectableMarker>() : null;
        SetSelected(marker);
    }

    public void RunEconomyDay()
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        int previousWorldDayCount = _runtimeEconomyState.WorldDayCount;
        _runtimeEconomyState.RunEconomyDay();
        AdvanceDispatchRecoveryForEconomyDays(previousWorldDayCount, _runtimeEconomyState.WorldDayCount);
    }

    public void UpdateAutoTick(float deltaTime)
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        int previousWorldDayCount = _runtimeEconomyState.WorldDayCount;
        _runtimeEconomyState.UpdateAutoTick(deltaTime);
        AdvanceDispatchRecoveryForEconomyDays(previousWorldDayCount, _runtimeEconomyState.WorldDayCount);
    }

    public void ResetRuntimeEconomy()
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        _runtimeEconomyState.Reset();
        ResetDungeonRunSystems();
    }
```

Lines 761-803 and 976-1004
```csharp
    private string GetSelectedNeedPressureText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? BuildNeedPressureText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedDispatchReadinessText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? GetDispatchReadinessText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedRecommendedRouteText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City && !string.IsNullOrEmpty(dungeonId)
            ? BuildRecommendedRouteSummaryText(_selectedMarker.EntityData.Id, dungeonId)
            : "None";
    }

    private string GetSelectedRecommendationReasonText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City && !string.IsNullOrEmpty(dungeonId)
            ? BuildRecommendationReasonText(_selectedMarker.EntityData.Id, dungeonId)
            : "None";
    }

    private string GetSelectedExpeditionDungeonId()
    {
        if (_selectedMarker == null)
        {
            return string.Empty;
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
            ? _selectedMarker.EntityData.Id
            : _selectedMarker.EntityData.Kind == WorldEntityKind.City && _runtimeEconomyState != null
                ? GetLinkedDungeonIdForCity(_selectedMarker.EntityData.Id)
                : string.Empty;
    }
```
- Notes: This file exposes planner data to the world view, but the actual logic it calls lives in the dungeon partial.

## C. StaticPlaceholderWorldView.DungeonRun.cs

- File: `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
- Why it matters: Main planner/readiness/recommendation/policy/result owner. It also now owns `PreEliteChoice` and elite-result identity.
- Relevant symbols: `DungeonRunState`, `DispatchReadinessState`, `DispatchPolicyState`, `TryEnterSelectedCityDungeon`, `UpdateDungeonRun`, `ConsumeDungeonRunExitRequest`, `EnsureDispatchReadinessEntry`, `GetBaseRecommendedRouteId`, `ApplyPolicyBiasToRecommendedRoute`, `BuildRecommendationReasonText`, `BuildExpectedNeedImpactText`, `RefreshDispatchRecommendation`, `TryCycleCurrentDispatchPolicy`, `ApplyDispatchReadinessOnDispatch`, `HandleDungeonRouteChoiceInput`, `OpenPreEliteChoicePanel`, `StartDungeonRunForRoute`, `FinishDungeonRun`, `ResetDungeonRunPresentationState`
- Exact excerpt:

Lines 46-119 and 432-437
```csharp
    private enum DungeonRunState
    {
        None,
        RouteChoice,
        Explore,
        Battle,
        EventChoice,
        PreEliteChoice,
        ResultPanel
    }

    private enum DispatchReadinessState
    {
        Ready,
        Recovering,
        Strained
    }

    private enum DispatchPolicyState
    {
        Safe,
        Balanced,
        Profit
    }

    private readonly Dictionary<string, DispatchReadinessState> _dispatchReadinessByCityId = new Dictionary<string, DispatchReadinessState>();
    private readonly Dictionary<string, int> _dispatchRecoveryDaysRemainingByCityId = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _daysSinceLastDispatchByCityId = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _consecutiveDispatchCountByCityId = new Dictionary<string, int>();
    private readonly Dictionary<string, string> _lastDispatchReadinessChangeByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, DispatchPolicyState> _dispatchPolicyByCityId = new Dictionary<string, DispatchPolicyState>();
```

Lines 1052-1077 and 1916-1957
```csharp
        _dispatchReadinessByCityId.Clear();
        _dispatchRecoveryDaysRemainingByCityId.Clear();
        _daysSinceLastDispatchByCityId.Clear();
        _consecutiveDispatchCountByCityId.Clear();
        _lastDispatchReadinessChangeByCityId.Clear();
        _dispatchPolicyByCityId.Clear();

        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City || string.IsNullOrEmpty(entity.Id))
            {
                continue;
            }

            _dispatchReadinessByCityId[entity.Id] = DispatchReadinessState.Ready;
            _dispatchRecoveryDaysRemainingByCityId[entity.Id] = 0;
            _daysSinceLastDispatchByCityId[entity.Id] = -1;
            _consecutiveDispatchCountByCityId[entity.Id] = 0;
            _lastDispatchReadinessChangeByCityId[entity.Id] = "None";
            _dispatchPolicyByCityId[entity.Id] = DispatchPolicyState.Balanced;
        }

    private string BuildNeedPressureText(string cityId)
    {
        return string.IsNullOrEmpty(cityId)
            ? "None"
            : GetNeedPressureTextForStock(GetCityManaShardStock(cityId));
    }

    private void EnsureDispatchReadinessEntry(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return;
        }

        if (!_dispatchReadinessByCityId.ContainsKey(cityId))
        {
            _dispatchReadinessByCityId[cityId] = DispatchReadinessState.Ready;
        }

        if (!_dispatchRecoveryDaysRemainingByCityId.ContainsKey(cityId))
        {
            _dispatchRecoveryDaysRemainingByCityId[cityId] = 0;
        }

        if (!_daysSinceLastDispatchByCityId.ContainsKey(cityId))
        {
            _daysSinceLastDispatchByCityId[cityId] = -1;
        }

        if (!_consecutiveDispatchCountByCityId.ContainsKey(cityId))
        {
            _consecutiveDispatchCountByCityId[cityId] = 0;
        }
```

Lines 1177-1255 and 1303-1315
```csharp
        public bool TryEnterSelectedCityDungeon(Camera worldCamera)
    {
        if (_runtimeEconomyState == null || _selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City || IsDungeonRunActive)
        {
            return false;
        }

        WorldEntityData city = _selectedMarker.EntityData;
        string dungeonId = GetLinkedDungeonIdForCity(city.Id);
        WorldEntityData dungeon = FindEntity(dungeonId);
        string partyId = dungeon != null ? _runtimeEconomyState.GetIdlePartyIdInCity(city.Id) : string.Empty;
        if (dungeon == null || string.IsNullOrEmpty(partyId))
        {
            return false;
        }

        ResetDungeonRunPresentationState();
        _activeDungeonParty = GetOrCreateDungeonParty(city.Id, partyId);
        _currentHomeCityId = city.Id;
        _currentDungeonId = dungeonId;
        _currentDungeonName = dungeon.DisplayName;
        _dungeonRunState = DungeonRunState.RouteChoice;
        _hoverRouteChoiceId = string.Empty;
        _selectedRouteChoiceId = string.Empty;
        _selectedRouteId = string.Empty;
        _recommendedRouteId = string.Empty;
        _recommendedRouteLabel = "None";
        _recommendedRouteReason = "None";
        _expectedNeedImpactText = "None";
        _pendingDungeonExit = false;
        _recentBattleLogs.Clear();
        RefreshDispatchRecommendation();
        if (!string.IsNullOrEmpty(_recommendedRouteId))
        {
            TryTriggerRouteChoice(_recommendedRouteId);
        }
    }

    public void UpdateDungeonRun(Keyboard keyboard, Mouse mouse, Camera worldCamera)
    {
        if (!IsDungeonRunActive)
        {
            return;
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            if (IsReturnToWorldPressed(keyboard))
            {
                _pendingDungeonExit = true;
            }

            return;
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            if (keyboard != null)
            {
                HandleDungeonRouteChoiceInput(keyboard);
            }
```

```csharp
    public bool ConsumeDungeonRunExitRequest()
    {
        if (!_pendingDungeonExit)
        {
            return false;
        }

        _pendingDungeonExit = false;
        _recentBattleLogs.Clear();
        ResetDungeonRunPresentationState();
        if (_dungeonRoot != null)
        {
            _dungeonRoot.SetActive(false);
```
Lines 2016-2031, 2175-2357, 2424-2493, 4708-4753, and 4936-4968
```csharp
    private DispatchPolicyState GetNextDispatchPolicyState(DispatchPolicyState currentPolicy)
    {
        return currentPolicy == DispatchPolicyState.Safe
            ? DispatchPolicyState.Balanced
            : currentPolicy == DispatchPolicyState.Balanced
                ? DispatchPolicyState.Profit
                : DispatchPolicyState.Safe;
    }

    private string BuildDispatchPolicyText(DispatchPolicyState policy)
    {
        return policy == DispatchPolicyState.Safe
            ? "Safe"
            : policy == DispatchPolicyState.Profit
                ? "Profit"
                : "Balanced";
    }

    private string GetBaseRecommendedRouteId(string cityId, string dungeonId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return string.Empty;
        }

        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        if (needPressure == "Urgent")
        {
            return readiness == DispatchReadinessState.Ready ? RiskyRouteId : GetBalancedRouteId(dungeonId);
        }

        if (needPressure == "Watch")
        {
            return readiness == DispatchReadinessState.Strained ? SafeRouteId : GetBalancedRouteId(dungeonId);
        }

        return SafeRouteId;
    }

    private string ApplyPolicyBiasToRecommendedRoute(string cityId, string dungeonId, string baseRecommendedRouteId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return string.Empty;
        }

        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        DispatchPolicyState policy = GetDispatchPolicyState(cityId);
        if (policy == DispatchPolicyState.Balanced)
        {
            return baseRecommendedRouteId;
        }

        if (policy == DispatchPolicyState.Safe)
        {
            return needPressure == "Urgent" && readiness == DispatchReadinessState.Ready
                ? baseRecommendedRouteId
                : readiness != DispatchReadinessState.Ready || needPressure == "Stable"
                    ? SafeRouteId
                    : baseRecommendedRouteId;
        }

        return readiness == DispatchReadinessState.Ready
            ? RiskyRouteId
            : GetBalancedRouteId(dungeonId);
    }

    private string BuildRecommendationReasonText(string cityId, string dungeonId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        string baseRecommendedRouteId = GetBaseRecommendedRouteId(cityId, dungeonId);
        string finalRecommendedRouteId = ApplyPolicyBiasToRecommendedRoute(cityId, dungeonId, baseRecommendedRouteId);
        return BuildBaseRecommendationReasonText(cityId, dungeonId) + " " + BuildPolicyRecommendationReasonText(cityId, dungeonId, baseRecommendedRouteId, finalRecommendedRouteId);
    }

    private string BuildExpectedNeedImpactText(string cityId, string dungeonId, string routeId)
    {
        if (string.IsNullOrEmpty(dungeonId) || string.IsNullOrEmpty(routeId))
        {
            return "None";
        }

        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        string balancedRouteId = GetBalancedRouteId(dungeonId);
        if (needPressure == "Urgent")
        {
            if (readiness != DispatchReadinessState.Ready)
            {
                return routeId == balancedRouteId
                    ? "Balances recovery with shard income."
                    : "Lower risk, lower return.";
            }

            return routeId == RiskyRouteId
                ? "Best for replenishing stock."
                : "Lower risk, lower return.";
        }

        return readiness == DispatchReadinessState.Ready
            ? routeId == SafeRouteId
                ? "Lower risk, lower return."
                : "Higher reward than the city needs right now."
            : routeId == SafeRouteId
                ? "Safer while the city recovers."
                : "Extra reward, but recovery slows.";
    }

    private void RefreshDispatchRecommendation()
    {
        _recommendedRouteId = GetRecommendedRouteId(_currentHomeCityId, _currentDungeonId);
        DungeonRouteTemplate template = GetRouteTemplateById(_currentDungeonId, _recommendedRouteId);
        _recommendedRouteLabel = template != null ? template.RouteLabel : "None";
        _recommendedRouteReason = BuildRecommendationReasonText(_currentHomeCityId, _currentDungeonId);
        RefreshExpectedNeedImpact();
    }

    public bool TryCycleCurrentDispatchPolicy()
    {
        return _dungeonRunState == DungeonRunState.RouteChoice && !string.IsNullOrEmpty(_currentHomeCityId)
            ? CycleDispatchPolicyForCity(_currentHomeCityId)
            : false;
    }

    private void ApplyDispatchReadinessOnDispatch(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return;
        }

        EnsureDispatchReadinessEntry(cityId);
        int nextConsecutiveDispatchCount = GetConsecutiveDispatchCount(cityId) + 1;
        _consecutiveDispatchCountByCityId[cityId] = nextConsecutiveDispatchCount;
        _daysSinceLastDispatchByCityId[cityId] = 0;
    }

    private void HandleDungeonRouteChoiceInput(Keyboard keyboard)
    {
        if (keyboard == null || _dungeonRunState != DungeonRunState.RouteChoice)
        {
            return;
        }

        if (keyboard.qKey.wasPressedThisFrame)
        {
            TryCycleCurrentDispatchPolicy();
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            TryConfirmRouteChoice();
        }
    }

    private bool TryTriggerPreEliteChoiceFromTile()
    {
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        if (room == null || room.RoomType != DungeonRoomType.Preparation || _preEliteDecisionResolved || _playerGridPosition != room.MarkerPosition)
        {
            return false;
        }

        OpenPreEliteChoicePanel();
        return true;
    }
```

Lines 5122-5313 and 5315-5356
```csharp
    private void StartDungeonRunForRoute(DungeonRouteTemplate template, string partyId)
    {
        if (template == null || string.IsNullOrEmpty(partyId))
        {
            return;
        }

        _activeDungeonParty = GetOrCreateDungeonParty(_currentHomeCityId, partyId);
        if (_activeDungeonParty == null)
        {
            return;
        }

        _activeDungeonParty.ResetForRun();
        _selectedRouteChoiceId = template.RouteId;
        _selectedRouteId = template.RouteId;
        _selectedRouteLabel = template.RouteLabel;
        _selectedRouteRiskLabel = template.RiskLabel;
        _followedRecommendation = template.RouteId == _recommendedRouteId;
        _runResultState = RunResultState.Playing;
        _dungeonRunState = DungeonRunState.Explore;
        _preRunManaShardStock = GetCityManaShardStock(_currentHomeCityId);
        _preRunNeedPressureText = GetNeedPressureTextForStock(_preRunManaShardStock);
        _preRunDispatchReadinessText = GetDispatchReadinessText(_currentHomeCityId);
        _preRunConsecutiveDispatchCount = GetConsecutiveDispatchCount(_currentHomeCityId);
        ApplyDispatchReadinessOnDispatch(_currentHomeCityId);
        RefreshExpectedNeedImpact();
    }

    private void FinishDungeonRun(RunResultState resultState, BattleState battleState, bool success, int lootReturned, string resultSummary)
    {
        int safeReturnedLoot = success ? Mathf.Max(0, lootReturned) : 0;
        _resultLootGained = safeReturnedLoot;
        _resultEventChoiceText = GetSelectedEventChoiceDisplayText();
        _resultPreEliteChoiceText = GetSelectedPreEliteChoiceDisplayText();
        _resultPreEliteHealAmount = _preEliteHealAmount;
        _resultEliteDefeated = _eliteDefeated;
        _resultEliteName = _eliteName;
        _resultEliteRewardLabel = _eliteRewardLabel;
        _resultEliteRewardAmount = _eliteRewardGranted ? _eliteRewardAmount : 0;
        _resultEliteBonusRewardAmount = _eliteBonusRewardGranted ? _eliteBonusRewardGrantedAmount : 0;
        _runResultState = resultState;
        _battleState = battleState;
        _dungeonRunState = DungeonRunState.ResultPanel;

        if (_runtimeEconomyState != null && !string.IsNullOrEmpty(_currentHomeCityId) && !string.IsNullOrEmpty(_currentDungeonId))
        {
            _runtimeEconomyState.ResolveDungeonRun(_currentHomeCityId, _currentDungeonId, DungeonRewardResourceId, safeReturnedLoot, success, string.IsNullOrEmpty(resultSummary) ? ActiveDungeonPartyText + " returned from " + _currentDungeonName + "." : resultSummary, _resultSurvivingMembersText, BuildClearedEncounterSummary(), _resultEventChoiceText, BuildLootBreakdownSummary(), BuildSelectedRouteSummary());
        }

        _resultStockBefore = _preRunManaShardStock;
        _resultStockAfter = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetCityManaShardStock(_currentHomeCityId);
        _resultStockDelta = _resultStockAfter - _resultStockBefore;
        _resultNeedPressureBeforeText = string.IsNullOrEmpty(_preRunNeedPressureText) ? GetNeedPressureTextForStock(_resultStockBefore) : _preRunNeedPressureText;
        _resultNeedPressureAfterText = string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetNeedPressureTextForStock(_resultStockAfter);
        _resultDispatchReadinessBeforeText = string.IsNullOrEmpty(_preRunDispatchReadinessText) ? GetDispatchReadinessText(_currentHomeCityId) : _preRunDispatchReadinessText;
        _resultDispatchReadinessAfterText = string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetDispatchReadinessText(_currentHomeCityId);
    }

    private void ResetDungeonRunPresentationState()
    {
        _activeDungeonParty = null;
        _currentDungeonId = string.Empty;
        _currentDungeonName = "None";
        _currentHomeCityId = string.Empty;
        _battleFeedbackText = "None";
        _enemyIntentText = "None";
        _hoverRouteChoiceId = string.Empty;
        _selectedRouteChoiceId = string.Empty;
        _selectedRouteId = string.Empty;
        _selectedRouteLabel = "None";
        _selectedRouteRiskLabel = "None";
        _recommendedRouteId = string.Empty;
        _recommendedRouteLabel = "None";
        _recommendedRouteReason = "None";
        _expectedNeedImpactText = "None";
        _preRunNeedPressureText = "None";
        _preRunDispatchReadinessText = "None";
        _resultNeedPressureBeforeText = "None";
        _resultNeedPressureAfterText = "None";
        _resultDispatchReadinessBeforeText = "None";
        _resultDispatchReadinessAfterText = "None";
        _hoverEventChoiceId = string.Empty;
        _selectedEventChoiceId = string.Empty;
        _resultEventChoiceText = "None";
        _hoverPreEliteChoiceId = string.Empty;
        _selectedPreEliteChoiceId = string.Empty;
        _resultPreEliteChoiceText = "Pending";
    }
```
Additional exact continuation for `BuildExpectedNeedImpactText` (Lines 2337-2357)
```csharp
        if (needPressure == "Watch")
        {
            if (routeId == balancedRouteId)
            {
                return readiness == DispatchReadinessState.Strained
                    ? "Balances recovery while reducing strain."
                    : "Balanced recovery.";
            }

            return routeId == RiskyRouteId
                ? "Higher return, higher strain."
                : "Lower risk, lower return.";
        }

        return readiness == DispatchReadinessState.Ready
            ? routeId == SafeRouteId
                ? "Lower risk, lower return."
                : "Higher reward than the city needs right now."
            : routeId == SafeRouteId
                ? "Safer while the city recovers."
                : "Extra reward, but recovery slows.";
```
- Notes:
  - Need pressure still comes from city `mana_shard` stock.
  - `DispatchPolicyState` is real per-city runtime state.
  - `FinishDungeonRun()` is the point where result summary, stock delta, readiness delta, and world-facing last-dispatch strings become meaningful.

## D. ManualTradeRuntimeState.cs

- File: `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs`
- Why it matters: Economy runtime source of truth. It resets world state, advances days, records unmet needs, marks parties on runs, and writes run loot back into city stock.
- Relevant symbols: `Reset`, `BeginDungeonRun`, `RunEconomyDay(bool isAutoTick)`, `ApplyEndOfDayShortagePressure`, `ResolveDungeonRun`
- Exact excerpt:

Lines 249-304 and 446-475
```csharp
    public void Reset()
    {
        WorldDayCount = 0;
        TradeStepCount = 0;
        AutoTickCount = 0;
        TickTimer = 0f;
        LastDayProducedTotal = 0;
        LastDayClaimedDungeonOutputsTotal = 0;
        LastDayTradedTotal = 0;
        LastDayProcessedTotal = 0;
        LastDayConsumedTotal = 0;
        LastDayCriticalFulfilledTotal = 0;
        LastDayCriticalUnmetTotal = 0;
        LastDayNormalFulfilledTotal = 0;
        LastDayNormalUnmetTotal = 0;
        LastDayFulfilledTotal = 0;
        LastDayUnmetTotal = 0;
        LastDayShortagesTotal = 0;
        _recentDayLogs.Clear();
        _stockByEntityId.Clear();
        _parties.Clear();
        _recentExpeditionLogs.Clear();
        _lastExpeditionResultByCityId.Clear();
        _lastExpeditionResultByDungeonId.Clear();
        _lastRunLootSummaryByCityId.Clear();
        _lastRunSurvivingMembersByCityId.Clear();
        _lastRunClearedEncountersByCityId.Clear();
        _lastRunEventChoiceByCityId.Clear();
        _lastRunLootBreakdownByCityId.Clear();
        _lastRunRouteByCityId.Clear();
        _lastRunDungeonByCityId.Clear();
    }

    public string BeginDungeonRun(string cityId, string dungeonId)
    {
        if (!TryGetEntity(cityId, out WorldEntityData city) || city.Kind != WorldEntityKind.City ||
            !TryGetEntity(dungeonId, out WorldEntityData dungeon) || dungeon.Kind != WorldEntityKind.Dungeon)
        {
            return string.Empty;
        }

        PartyRuntimeData party = FindIdleParty(cityId);
        if (party == null)
        {
            return string.Empty;
        }

        party.State = PartyState.OnExpedition;
        party.TargetDungeonId = dungeonId;
        party.DaysRemaining = 1;
        party.LastResultSummary = party.PartyId + " entered " + dungeon.DisplayName;
```

Lines 950-977, 1360-1402, and 1614-1669
```csharp
    private void RunEconomyDay(bool isAutoTick)
    {
        WorldDayCount += 1;
        TradeStepCount += 1;
        TickTimer = 0f;

        if (isAutoTick)
        {
            AutoTickCount += 1;
        }

        ResetLastDayTotals();

        WorldEntityData[] entities = _worldData.Entities ?? System.Array.Empty<WorldEntityData>();
        RunLocalProductionPhase(entities);
        RunDungeonClaimPhase(entities);
        RefreshCurrentTradeScanResult();
        RunDirectTradePhase(CurrentTradeScanResult);
        RunLocalProcessingPhase(entities);
        RunLocalConsumptionPhase(entities);

        CurrentUnclaimedDungeonOutputsTotal = CalculateCurrentUnclaimedDungeonOutputsTotal();
        RefreshCurrentTradeScanResult();
        ApplyEndOfDayShortagePressure(CurrentTradeScanResult);
        RunExpeditionPhase();
        RefreshCurrentTradeScanResult();
        AppendRecentDayLog(BuildRecentDayLog());
    }

    private void ApplyEndOfDayShortagePressure(DirectTradeScanResult tradeScanResult)
    {
        if (tradeScanResult == null || tradeScanResult.UnmetCityNeeds == null)
        {
            return;
        }

        for (int i = 0; i < tradeScanResult.UnmetCityNeeds.Length; i++)
        {
            UnmetCityNeedData unmetNeed = tradeScanResult.UnmetCityNeeds[i];
            if (unmetNeed == null || string.IsNullOrEmpty(unmetNeed.CityEntityId) || string.IsNullOrEmpty(unmetNeed.ResourceId))
            {
                continue;
            }

            if (!TryGetEntity(unmetNeed.CityEntityId, out WorldEntityData city) || city.Kind != WorldEntityKind.City)
            {
                continue;
            }

            RecordNeedUnmet(unmetNeed.CityEntityId, unmetNeed.ResourceId, IsHighPriorityNeedResource(city, unmetNeed.ResourceId));
        }
    }

    public void ResolveDungeonRun(string cityId, string dungeonId, string rewardResourceId, int lootReturned, bool success, string resultSummary, string survivingMembersSummary = null, string clearedEncounterSummary = null, string eventChoiceSummary = null, string lootBreakdownSummary = null, string routeSummary = null)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        if (party == null)
        {
            party = FindActivePartyForDungeon(dungeonId);
        }

        if (party == null)
        {
            return;
        }

        string safeResultSummary = string.IsNullOrEmpty(resultSummary) ? party.PartyId + " returned." : resultSummary;
        int safeLootReturned = success && lootReturned > 0 ? lootReturned : 0;

        if (success)
        {
            ExpeditionSuccessCount += 1;
            ExpeditionLootReturnedTotal += safeLootReturned;

            if (!string.IsNullOrEmpty(rewardResourceId) && safeLootReturned > 0)
            {
                SetStock(cityId, rewardResourceId, GetStock(cityId, rewardResourceId) + safeLootReturned);
            }
        }
```
Additional exact continuation for `ApplyEndOfDayShortagePressure` (Lines 1375-1395)
```csharp
            if (!TryGetEntity(unmetNeed.CityEntityId, out WorldEntityData city) || city.Kind != WorldEntityKind.City)
            {
                continue;
            }

            if (GetStock(unmetNeed.CityEntityId, unmetNeed.ResourceId) > 0)
            {
                continue;
            }

            if (GetLastDayShortageCount(unmetNeed.CityEntityId, unmetNeed.ResourceId) > 0)
            {
                continue;
            }

            if (GetResourceDaySummary(unmetNeed.CityEntityId, unmetNeed.ResourceId).Consumed > 0)
            {
                continue;
            }

            RecordNeedUnmet(unmetNeed.CityEntityId, unmetNeed.ResourceId, IsHighPriorityNeedResource(city, unmetNeed.ResourceId));
``` 
- Notes: The real stock writeback is `SetStock(cityId, rewardResourceId, GetStock(cityId, rewardResourceId) + safeLootReturned)`.

## E. PrototypeDebugHUD.cs

- File: `Assets/_Game/Scripts/Core/PrototypeDebugHUD.cs`
- Why it matters: Current planner/result/debug surface. It exposes policy cycle, pre-elite choice UI, selected-city planner summary, and search/language controls.
- Relevant symbols: `OnGUI`, `DrawRouteChoiceSection`, `DrawPreEliteChoiceSection`, `DrawSearchBar`
- Exact excerpt:

Lines 76-109 and 437-479
```csharp
    private void OnGUI()
    {
        CacheBootEntry();
        if (_bootEntry == null)
        {
            _isSearchFieldFocused = false;
            return;
        }

        EnsureExpandedState();
        EnsureStyles();

        List<HudPanel> panels = BuildPanels();
        bool dungeonHudMode = _bootEntry.IsDungeonRunHudMode;
        bool overlayMode = _bootEntry.IsDungeonBattleViewActive || _bootEntry.IsDungeonRouteChoiceVisible || _bootEntry.IsDungeonEventChoiceVisible || _bootEntry.IsDungeonPreEliteChoiceVisible || _bootEntry.IsDungeonResultPanelVisible;

        float panelWidth = overlayMode
            ? Mathf.Clamp(Screen.width * 0.18f, 276f, 308f)
            : Mathf.Clamp(Screen.width * 0.34f, PanelWidthMin, PanelWidthMax);
        panelWidth = Mathf.Min(panelWidth, Screen.width - (Margin * 2f));

        float panelHeight = overlayMode
            ? Mathf.Clamp(Screen.height * 0.18f, 148f, 188f)
            : dungeonHudMode
                ? Mathf.Clamp(Screen.height * 0.34f, 248f, 380f)
                : Mathf.Max(220f, Screen.height - (Margin * 2f));

        Rect dockRect = new Rect(Screen.width - panelWidth - Margin, Margin, panelWidth, panelHeight);
        DrawDockPanel(dockRect, panels);

        if (overlayMode)
        {
            DrawDungeonBattleBottomBar(dockRect);
        }
    }

    private void DrawRouteChoiceSection(Rect rect)
    {
        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredChoiceKey = string.Empty;

        DrawRouteChoiceButton(optionARect, "safe", "[1] " + V(_bootEntry.RouteOption1Label), mousePosition, ref hoveredChoiceKey);
        DrawRouteChoiceButton(optionBRect, "risky", "[2] " + V(_bootEntry.RouteOption2Label), mousePosition, ref hoveredChoiceKey);

        bool policyHovered = policyRect.Contains(mousePosition);
        string policyLabel = "[Q] " + T("CyclePolicy") + " " + V(_bootEntry.CurrentDispatchPolicyRunLabel);
        if (DrawInteractiveButton(policyRect, policyLabel, true, policyHovered, false))
        {
            _bootEntry.TryCycleCurrentDispatchPolicy();
        }

        bool canConfirm = _bootEntry.CanConfirmRouteChoice();
        bool confirmHovered = canConfirm && confirmRect.Contains(mousePosition);
        if (DrawInteractiveButton(confirmRect, "[Enter] Dispatch", canConfirm, confirmHovered, false))
        {
            _bootEntry.TryConfirmRouteChoice();
        }
    }
```

Lines 543-587, 629-689, and 1038-1049
```csharp
    private void DrawPreEliteChoiceSection(Rect rect)
    {
        GUI.Label(promptRect, BuildPanelBody(
            V(_bootEntry.PreElitePromptLabel),
            Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
            Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
            Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
            Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel))), _bodyStyle);
    }

    private void DrawSearchBar(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y + 3f, SearchLabelWidth, rect.height), T("Search"), _bodyStyle);
        Rect fieldRect = new Rect(rect.x + SearchLabelWidth, rect.y, rect.width - SearchLabelWidth - SearchClearButtonWidth - ChipGap, rect.height);
        Rect clearRect = new Rect(fieldRect.xMax + ChipGap, rect.y, SearchClearButtonWidth, rect.height);
        GUI.SetNextControlName(SearchFieldControlName);
        _searchText = GUI.TextField(fieldRect, _searchText ?? string.Empty, _searchFieldStyle);
        if (GUI.Button(clearRect, T("Clear"), _chipStyle))
        {
            _searchText = string.Empty;
            GUI.FocusControl(string.Empty);
            _isSearchFieldFocused = false;
            return;
        }

        _isSearchFieldFocused = GUI.GetNameOfFocusedControl() == SearchFieldControlName;
    }

                Line("CityManaShardStock", V(_bootEntry.SelectedCityManaShardStockLabel)),
                Line("NeedPressure", V(_bootEntry.SelectedNeedPressureLabel)),
                Line("DispatchReadiness", V(_bootEntry.SelectedDispatchReadinessLabel)),
                Line("RecoveryProgress", V(_bootEntry.SelectedDispatchRecoveryProgressLabel)),
                Line("DispatchPolicy", V(_bootEntry.SelectedDispatchPolicyLabel)),
                Line("RecommendedRoute", V(_bootEntry.SelectedRecommendedRouteSummaryLabel)),
                Line("RecommendedRouteForLinkedCity", V(_bootEntry.SelectedRecommendedRouteForLinkedCityLabel)),
                Line("RecommendationReason", V(_bootEntry.SelectedRecommendationReasonLabel)),
                Line("LastDispatchImpact", V(_bootEntry.SelectedLastDispatchImpactLabel)),
                Line("LastDispatchStockDelta", V(_bootEntry.SelectedLastDispatchStockDeltaLabel)),
                Line("LastNeedPressureChange", V(_bootEntry.SelectedLastNeedPressureChangeLabel)),
                Line("LastDispatchReadinessChange", V(_bootEntry.SelectedLastDispatchReadinessChangeLabel)),
```
- Notes: Search focus can block keyboard shortcuts because `BootEntry.Update()` checks `IsSearchFieldFocused()`.

## F. PrototypeLocalization.cs

- File: `Assets/_Game/Scripts/Core/PrototypeLocalization.cs`
- Why it matters: Label source for EN/KR planner/result UI.
- Relevant symbols: `PrototypeLanguage`, `Get`, `GetLanguageDisplayName`, `TranslateValue`
- Exact excerpt:

Lines 1-12, 214-223, 280-285, and 426-439
```csharp
public enum PrototypeLanguage
{
    English,
    Korean
}

public static class PrototypeLocalization
{
    private static readonly Dictionary<string, string> EnglishByKey = new Dictionary<string, string>
    {
```

```csharp
        { "DispatchReadiness", "Dispatch Readiness" },
        { "RecoveryProgress", "Recovery Progress" },
        { "ConsecutiveDispatches", "Consecutive Dispatches" },
        { "DispatchPolicy", "Dispatch Policy" },
        { "CyclePolicy", "Cycle Policy" },
        { "RecommendedRoute", "Recommended Route" },
        { "RecommendedRouteForLinkedCity", "Recommended Route For Linked City" },
        { "RecommendationReason", "Recommendation Reason" },
        { "RecoveryAdvice", "Recovery Advice" },
        { "ExpectedNeedImpact", "Expected Need Impact" },
```

```csharp
        { "PanelDungeonRouteChoice", "Dispatch Planner" },
        { "DungeonRouteControls", "Dungeon Route Controls" },
        { "DungeonRouteControlsValue", "[1] Route 1  [2] Route 2  [Q] Cycle Policy  [Enter] Dispatch  [Esc] Return" },
        { "CurrentRoute", "Current Route" },
        { "RouteRisk", "Route Risk" },
        { "RouteChosen", "Route Chosen" },
```

```csharp
    public static string Get(PrototypeLanguage language, string key)
    {
        Dictionary<string, string> table = language == PrototypeLanguage.Korean ? KoreanByKey : EnglishByKey;
        return table.TryGetValue(key, out string value) ? value : key;
    }

    public static string GetLanguageDisplayName(PrototypeLanguage language)
    {
        return language == PrototypeLanguage.Korean
            ? Get(language, "LanguageKorean")
            : Get(language, "LanguageEnglish");
    }

    public static string TranslateValue(PrototypeLanguage language, string text)
```
- Notes: New planner labels still need key entries in both tables. Dynamic planner reason strings are still hardcoded in the dungeon partial.

## G. WorldData.cs

- File: `Assets/_Game/Scripts/World/WorldData.cs`
- Why it matters: Placeholder authoring source for city ids, dungeon ids, linked city ids, and world routes.
- Relevant symbols: `WorldEntityData`, `WorldRouteData`, `PlaceholderWorldDataFactory`
- Exact excerpt:

Lines 23-40 and 106-184
```csharp
public sealed class WorldEntityData
{
    public string Id { get; }
    public string DisplayName { get; }
    public WorldEntityKind Kind { get; }
    public Vector2 Position { get; }
    public string[] Tags { get; }
    public string PrimaryStatName { get; }
    public int PrimaryStatValue { get; }
    public string[] RelatedResourceIds { get; }
    public string[] ResourceRoleTags { get; }
    public string[] SupplyResourceIds { get; }
    public string[] NeedResourceIds { get; }
    public string[] HighPriorityNeedResourceIds { get; }
    public string[] OutputResourceIds { get; }
    public LocalProcessingRuleData[] ProcessingRules { get; }
    public string LinkedCityId { get; }
```

```csharp
public static class PlaceholderWorldDataFactory
{
    public static WorldData Create()
    {
        WorldEntityData[] entities =
        {
            new WorldEntityData(
                "city-a",
                "City A",
                WorldEntityKind.City,
                new Vector2(-3f, 1f),
                new[] { "city", "starter", "market" },
                "population",
                1200,
                new[] { "grain", "iron_ore", "mana_shard" },
                new[] { "staple-focus", "market-hub", "gateway" },
                new[] { "grain" },
                new[] { "iron_ore" },
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<LocalProcessingRuleData>(),
                string.Empty),
            new WorldEntityData(
                "city-b",
                "City B",
                WorldEntityKind.City,
                new Vector2(3f, -1f),
                new[] { "city", "frontier", "processor" },
                "population",
                850,
                new[] { "grain", "iron_ore", "mana_shard", "refined_mana" },
                new[] { "staple-focus", "frontier-material", "arcane-processing" },
                new[] { "iron_ore" },
                new[] { "grain", "refined_mana" },
                new[] { "grain" },
                System.Array.Empty<string>(),
                new[] { new LocalProcessingRuleData("mana_shard", "refined_mana", 1) },
                string.Empty),
            new WorldEntityData(
                "dungeon-alpha",
                "Dungeon Alpha",
                WorldEntityKind.Dungeon,
                new Vector2(-1f, -2f),
                new[] { "dungeon", "ruins", "low-tier" },
                "dangerLevel",
                3,
                new[] { "mana_shard", "iron_ore" },
                new[] { "arcane-source", "relic-site" },
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                new[] { "mana_shard" },
                System.Array.Empty<LocalProcessingRuleData>(),
                "city-a"),
            new WorldEntityData(
                "dungeon-beta",
                "Dungeon Beta",
                WorldEntityKind.Dungeon,
                new Vector2(4.5f, 2f),
                new[] { "dungeon", "watchpost", "high-risk" },
                "dangerLevel",
                5,
                new[] { "mana_shard", "refined_mana" },
                new[] { "goblin-den", "vault-route" },
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                new[] { "mana_shard" },
                System.Array.Empty<LocalProcessingRuleData>(),
                "city-b")
        };
```
- Notes: Linked city/dungeon pairing still comes from `WorldEntityData.LinkedCityId` on dungeon entries.

## Safest insertion points for Batch-5A

- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs:2175-2357`
- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs:2424-2493`
- `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.cs:761-803`
- `Assets/_Game/Scripts/Core/PrototypeDebugHUD.cs:437-479`
- `Assets/_Game/Scripts/Core/PrototypeLocalization.cs:214-223`

## Risky areas to avoid touching

- Combat-heavy internals in `StaticPlaceholderWorldView.DungeonRun.cs` outside planner/readiness/result slices.
- Room build and visual refresh sections unless the change really needs exploration presentation.
- `ManualTradeRuntimeState.RunEconomyDay(bool)` phases unless the change truly belongs to economy simulation, not planner heuristics.

## Missing evidence still not captured

- Exact elite encounter weighting and full room-generation logic were intentionally not recopied here.
- No separate planner policy authoring asset or ScriptableObject was found in the repo slices checked here.
- No automated test or CI evidence was added here; this file is context-only.



