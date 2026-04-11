using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class StaticPlaceholderWorldView
{
    private const int ExploreLaneCount = 3;
    private static readonly Vector3[] ExploreLaneCardPositions =
    {
        new Vector3(-1.85f, 1.95f, 0f),
        new Vector3(-1.85f, 0f, 0f),
        new Vector3(-1.85f, -1.95f, 0f)
    };

    private static readonly string[] ExploreLaneLabels =
    {
        "Upper Rail",
        "Middle Rail",
        "Lower Rail"
    };

    private SpriteRenderer _panelShellBackdropRenderer;
    private SpriteRenderer _panelShellTrackRenderer;
    private SpriteRenderer _panelShellSidebarRenderer;
    private SpriteRenderer _panelShellHeaderRenderer;
    private readonly SpriteRenderer[] _panelShellLaneRenderers = new SpriteRenderer[ExploreLaneCount];
    private readonly SpriteRenderer[] _panelShellLaneAccentRenderers = new SpriteRenderer[ExploreLaneCount];
    private readonly TextMesh[] _panelShellLaneLabelMeshes = new TextMesh[ExploreLaneCount];
    private readonly TextMesh[] _panelShellLaneHintMeshes = new TextMesh[ExploreLaneCount];
    private TextMesh _panelShellTitleMesh;
    private TextMesh _panelShellSubtitleMesh;
    private TextMesh _panelShellSidebarMesh;
    private TextMesh _panelShellFooterMesh;
    private string _hoverPanelLaneOptionId = string.Empty;
    private string _selectedPanelLaneOptionId = string.Empty;
    private string _currentLaneSelectionText = "None";
    private string _currentPanelDeltaSummaryText = "None";
    private bool _startPanelResolved;
    private int _panelThreatTicks;
    private int _panelDiscoveryFlags;
    private int _panelExtractionTicks;

    public string CurrentLaneSelectionText => string.IsNullOrEmpty(_currentLaneSelectionText) ? "None" : _currentLaneSelectionText;
    public string CurrentPanelDeltaSummaryText => string.IsNullOrEmpty(_currentPanelDeltaSummaryText) ? "None" : _currentPanelDeltaSummaryText;

    public void SetDungeonPanelLaneHover(string optionId)
    {
        if (_dungeonRunState != DungeonRunState.Explore)
        {
            if (!string.IsNullOrEmpty(_hoverPanelLaneOptionId))
            {
                _hoverPanelLaneOptionId = string.Empty;
                RefreshSelectionPrompt();
                RefreshDungeonPresentation();
            }
            return;
        }

        PrototypeDungeonPanelOption option = FindCurrentExplorePanelOption(optionId);
        string nextOptionId = option != null && !option.IsBlocked ? option.OptionId : string.Empty;
        if (_hoverPanelLaneOptionId == nextOptionId)
        {
            return;
        }

        _hoverPanelLaneOptionId = nextOptionId;
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    public bool TryTriggerDungeonPanelLaneOption(string optionId)
    {
        return TryTriggerCurrentPanelLaneOption(optionId);
    }

    private void ResetDungeonPanelShellState()
    {
        _hoverPanelLaneOptionId = string.Empty;
        _selectedPanelLaneOptionId = string.Empty;
        _currentLaneSelectionText = "None";
        _currentPanelDeltaSummaryText = "None";
        _startPanelResolved = false;
        _panelThreatTicks = 0;
        _panelDiscoveryFlags = 0;
        _panelExtractionTicks = 0;
    }

    private void UpdateDungeonExploreMouseInteraction(Mouse mouse, Camera worldCamera)
    {
        if (_dungeonRunState != DungeonRunState.Explore || mouse == null || worldCamera == null)
        {
            _hoverPanelLaneOptionId = string.Empty;
            return;
        }

        PrototypeDungeonPanelOption[] options = GetCurrentExplorePanelOptions();
        if (options.Length <= 0)
        {
            _hoverPanelLaneOptionId = string.Empty;
            return;
        }

        float depth = Mathf.Abs(worldCamera.transform.position.z - _dungeonRoot.transform.position.z);
        Vector2 screenPoint = mouse.position.ReadValue();
        Vector3 worldPoint3 = worldCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
        Vector2 worldPoint = new Vector2(worldPoint3.x, worldPoint3.y);
        string hoveredOptionId = string.Empty;

        for (int i = 0; i < options.Length && i < _panelShellLaneRenderers.Length; i++)
        {
            SpriteRenderer renderer = _panelShellLaneRenderers[i];
            PrototypeDungeonPanelOption option = options[i];
            if (renderer == null || option == null || option.IsBlocked)
            {
                continue;
            }

            Bounds bounds = renderer.bounds;
            const float hitPadding = 0.18f;
            if (worldPoint.x >= bounds.min.x - hitPadding &&
                worldPoint.x <= bounds.max.x + hitPadding &&
                worldPoint.y >= bounds.min.y - hitPadding &&
                worldPoint.y <= bounds.max.y + hitPadding)
            {
                hoveredOptionId = option.OptionId;
                break;
            }
        }

        if (_hoverPanelLaneOptionId != hoveredOptionId)
        {
            _hoverPanelLaneOptionId = hoveredOptionId;
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
        }

        if (!string.IsNullOrEmpty(hoveredOptionId) && mouse.leftButton.wasPressedThisFrame)
        {
            TryTriggerCurrentPanelLaneOption(hoveredOptionId);
        }
    }

    private PrototypeDungeonPanelOption[] GetCurrentExplorePanelOptions()
    {
        if (_dungeonRunState != DungeonRunState.Explore)
        {
            return System.Array.Empty<PrototypeDungeonPanelOption>();
        }

        PrototypeDungeonPanelContext context = BuildCurrentDungeonPanelContext();
        return context != null && context.Options != null ? context.Options : System.Array.Empty<PrototypeDungeonPanelOption>();
    }

    private PrototypeDungeonPanelOption FindCurrentExplorePanelOption(string optionId)
    {
        if (string.IsNullOrEmpty(optionId))
        {
            return null;
        }

        PrototypeDungeonPanelOption[] options = GetCurrentExplorePanelOptions();
        for (int i = 0; i < options.Length; i++)
        {
            PrototypeDungeonPanelOption option = options[i];
            if (option != null && option.OptionId == optionId)
            {
                return option;
            }
        }

        return null;
    }

    private PrototypeDungeonPanelOption GetCurrentExplorePanelOptionByIndex(int laneIndex)
    {
        PrototypeDungeonPanelOption[] options = GetCurrentExplorePanelOptions();
        return laneIndex >= 0 && laneIndex < options.Length ? options[laneIndex] : null;
    }

    private int ResolveLaneIndexFromOption(PrototypeDungeonPanelOption option)
    {
        if (option == null)
        {
            return -1;
        }

        if (option.LaneIndex >= 0 && option.LaneIndex < ExploreLaneCount)
        {
            return option.LaneIndex;
        }

        for (int i = 0; i < ExploreLaneCount; i++)
        {
            if (_panelShellLaneRenderers[i] != null && option.OptionId == _panelShellLaneRenderers[i].gameObject.name)
            {
                return i;
            }
        }

        return -1;
    }

    private bool TryTriggerCurrentPanelLaneOption(string optionId)
    {
        if (_dungeonRunState != DungeonRunState.Explore)
        {
            return false;
        }

        PrototypeDungeonPanelOption option = FindCurrentExplorePanelOption(optionId);
        if (option == null || option.IsBlocked)
        {
            return false;
        }

        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        string stageKindKey = ResolveCurrentPanelStageKindKey(room);
        int laneIndex = ResolveLaneIndexFromOption(option);
        string laneLabel = !string.IsNullOrEmpty(option.LaneLabel)
            ? option.LaneLabel
            : laneIndex >= 0 && laneIndex < ExploreLaneLabels.Length
                ? ExploreLaneLabels[laneIndex]
                : "Lane";

        _selectedPanelLaneOptionId = option.OptionId;
        _hoverPanelLaneOptionId = string.Empty;
        _currentLaneSelectionText = laneLabel + " | " + option.OptionLabel;
        _runTurnCount += 1;

        int chipDamage;
        int extraLoot;
        string deltaSummary = ApplyCurrentPanelLaneDelta(room, stageKindKey, laneIndex, out chipDamage, out extraLoot);
        _currentPanelDeltaSummaryText = deltaSummary;
        AppendRoomPathLabel((room != null ? room.DisplayName : ResolvePanelStageLabel(stageKindKey, null)) + " [" + laneLabel + "]");

        switch (stageKindKey)
        {
            case "start_room":
                _startPanelResolved = true;
                CaptureLatestDungeonChoiceOutcome(new PrototypeDungeonChoiceOutcome
                {
                    PanelId = room != null ? room.RoomId : "start_room",
                    StageKindKey = "start_room",
                    SelectedOptionId = option.OptionId,
                    SelectedOptionLabel = option.OptionLabel,
                    OutcomeKindKey = "lane_progress",
                    TransitionStageKindKey = "explore",
                    IsConfirmed = true,
                    SummaryText = "Panel advanced via " + laneLabel + ": " + option.OptionLabel + ".",
                    DeltaSummaryText = deltaSummary
                });
                AppendBattleLog("Moved through " + (room != null ? room.DisplayName : "Start Panel") + " via " + option.OptionLabel + ".");
                SetBattleFeedbackText(option.OptionLabel + " opens the next route panel.");
                RefreshRoomSequenceState(true);
                break;
            case "skirmish_room":
                CaptureLatestDungeonChoiceOutcome(new PrototypeDungeonChoiceOutcome
                {
                    PanelId = room != null ? room.RoomId : "skirmish_room",
                    StageKindKey = "skirmish_room",
                    SelectedOptionId = option.OptionId,
                    SelectedOptionLabel = option.OptionLabel,
                    OutcomeKindKey = "lane_pressure",
                    TransitionStageKindKey = "skirmish_battle",
                    IsConfirmed = true,
                    SummaryText = "Battle approach chosen: " + option.OptionLabel + ".",
                    DeltaSummaryText = deltaSummary
                });
                AppendBattleLog("Approached " + (room != null ? room.DisplayName : "Skirmish") + " via " + option.OptionLabel + ".");
                if (room != null)
                {
                    _playerGridPosition = room.MarkerPosition;
                }
                TryStartEncounter();
                return true;
            case "cache_room":
                ResolveCachePanelOption(room, option, deltaSummary, extraLoot);
                break;
            case "shrine_room":
                CaptureLatestDungeonChoiceOutcome(new PrototypeDungeonChoiceOutcome
                {
                    PanelId = room != null ? room.RoomId : "shrine_room",
                    StageKindKey = "shrine_room",
                    SelectedOptionId = option.OptionId,
                    SelectedOptionLabel = option.OptionLabel,
                    OutcomeKindKey = "event_gate",
                    TransitionStageKindKey = "shrine_choice",
                    IsConfirmed = true,
                    SummaryText = "Shrine approach chosen: " + option.OptionLabel + ".",
                    DeltaSummaryText = deltaSummary
                });
                AppendBattleLog("Approached " + GetCurrentEventTitleText() + " via " + option.OptionLabel + ".");
                OpenEventChoicePanel();
                return true;
            case "preparation_room":
                CaptureLatestDungeonChoiceOutcome(new PrototypeDungeonChoiceOutcome
                {
                    PanelId = room != null ? room.RoomId : "preparation_room",
                    StageKindKey = "preparation_room",
                    SelectedOptionId = option.OptionId,
                    SelectedOptionLabel = option.OptionLabel,
                    OutcomeKindKey = "preparation_gate",
                    TransitionStageKindKey = "pre_elite_choice",
                    IsConfirmed = true,
                    SummaryText = "Preparation approach chosen: " + option.OptionLabel + ".",
                    DeltaSummaryText = deltaSummary
                });
                AppendBattleLog("Entered " + GetCurrentPreEliteTitleText() + " via " + option.OptionLabel + ".");
                OpenPreEliteChoicePanel();
                return true;
            case "elite_room":
                CaptureLatestDungeonChoiceOutcome(new PrototypeDungeonChoiceOutcome
                {
                    PanelId = room != null ? room.RoomId : "elite_room",
                    StageKindKey = "elite_room",
                    SelectedOptionId = option.OptionId,
                    SelectedOptionLabel = option.OptionLabel,
                    OutcomeKindKey = "elite_gate",
                    TransitionStageKindKey = "elite_battle",
                    IsConfirmed = true,
                    SummaryText = "Elite approach chosen: " + option.OptionLabel + ".",
                    DeltaSummaryText = deltaSummary
                });
                AppendBattleLog("Approached " + _eliteName + " via " + option.OptionLabel + ".");
                if (room != null)
                {
                    _playerGridPosition = room.MarkerPosition;
                }
                TryStartEncounter();
                return true;
            case "extraction_panel":
                CaptureLatestDungeonChoiceOutcome(new PrototypeDungeonChoiceOutcome
                {
                    PanelId = "extraction_panel",
                    StageKindKey = "extraction_panel",
                    SelectedOptionId = option.OptionId,
                    SelectedOptionLabel = option.OptionLabel,
                    OutcomeKindKey = "extraction_route",
                    TransitionStageKindKey = "result_panel",
                    IsConfirmed = true,
                    SummaryText = "Extraction route chosen: " + option.OptionLabel + ".",
                    DeltaSummaryText = deltaSummary
                });
                AppendBattleLog("Extraction route chosen: " + option.OptionLabel + ".");
                FinishDungeonRun(RunResultState.Clear, BattleState.Victory, true, _carriedLootAmount, ActiveDungeonPartyText + " cleared " + _currentDungeonName + " and returned with " + BuildLootAmountText(_carriedLootAmount) + ".");
                return true;
        }

        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }

    private void ResolveCachePanelOption(DungeonRoomTemplateData room, PrototypeDungeonPanelOption option, string deltaSummary, int extraLoot)
    {
        if (_activeChest == null || _activeChest.IsOpened)
        {
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return;
        }

        _activeChest.IsOpened = true;
        _chestOpenedCount += 1;
        int totalReward = _activeChest.RewardAmount + Mathf.Max(0, extraLoot);
        _chestLootAmount += totalReward;
        _carriedLootAmount += totalReward;
        CaptureLatestDungeonChoiceOutcome(new PrototypeDungeonChoiceOutcome
        {
            PanelId = room != null ? room.RoomId : "cache_room",
            StageKindKey = "cache_room",
            SelectedOptionId = option.OptionId,
            SelectedOptionLabel = option.OptionLabel,
            OutcomeKindKey = "cache_claim",
            TransitionStageKindKey = "explore",
            IsConfirmed = true,
            SummaryText = "Cache claimed via " + option.OptionLabel + ": " + BuildLootAmountText(totalReward) + ".",
            DeltaSummaryText = deltaSummary
        });
        AppendBattleLog("Opened " + (room != null ? room.DisplayName : "Cache") + " via " + option.OptionLabel + " for " + BuildLootAmountText(totalReward) + ".");
        SetBattleFeedbackText((room != null ? room.DisplayName : "Cache") + " yielded " + BuildLootAmountText(totalReward) + ".");
        RefreshRoomSequenceState(true);
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private string ApplyCurrentPanelLaneDelta(DungeonRoomTemplateData room, string stageKindKey, int laneIndex, out int chipDamage, out int extraLoot)
    {
        chipDamage = 0;
        extraLoot = 0;
        int discoveryGain = 0;
        int threatGain = 0;
        int extractionGain = 0;

        switch (laneIndex)
        {
            case 0:
                discoveryGain = 1;
                break;
            case 1:
                extractionGain = stageKindKey == "start_room" || stageKindKey == "preparation_room" ? 1 : 0;
                break;
            case 2:
                threatGain = 1;
                if (stageKindKey == "cache_room")
                {
                    extraLoot = 1;
                }
                if (stageKindKey == "skirmish_room" || stageKindKey == "elite_room" || stageKindKey == "cache_room")
                {
                    chipDamage = ApplyPanelChipDamage(1);
                }
                break;
        }

        if (stageKindKey == "shrine_room" && laneIndex == 1)
        {
            discoveryGain = 0;
            extractionGain = 0;
        }

        _panelDiscoveryFlags += discoveryGain;
        _panelThreatTicks += threatGain;
        _panelExtractionTicks += extractionGain;
        return BuildPanelLaneDeltaSummary(discoveryGain, threatGain, extractionGain, chipDamage, extraLoot);
    }

    private int ApplyPanelChipDamage(int damageAmount)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || damageAmount <= 0)
        {
            return 0;
        }

        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null || member.IsDefeated || member.CurrentHp <= 0)
            {
                continue;
            }

            int before = member.CurrentHp;
            member.CurrentHp = Mathf.Max(1, member.CurrentHp - damageAmount);
            int applied = before - member.CurrentHp;
            if (applied > 0)
            {
                AppendBattleLog(member.DisplayName + " took " + applied + " chip damage while moving through the panel shell.");
            }
            return applied;
        }

        return 0;
    }

    private string BuildPanelLaneDeltaSummary(int discoveryGain, int threatGain, int extractionGain, int chipDamage, int extraLoot)
    {
        List<string> parts = new List<string>(5);
        if (discoveryGain > 0)
        {
            parts.Add("Discovery +" + discoveryGain);
        }
        if (threatGain > 0)
        {
            parts.Add("Threat +" + threatGain);
        }
        if (extractionGain > 0)
        {
            parts.Add("Extraction +" + extractionGain);
        }
        if (chipDamage > 0)
        {
            parts.Add("HP -" + chipDamage);
        }
        if (extraLoot > 0)
        {
            parts.Add("Loot +" + BuildLootAmountText(extraLoot));
        }
        if (parts.Count <= 0)
        {
            parts.Add("Supply steady");
        }
        return string.Join(" | ", parts.ToArray());
    }

    private void RefreshDungeonPanelShellPresentation(DungeonRoomTemplateData currentRoom)
    {
        EnsureDungeonPanelShellVisuals();
        PrototypeDungeonPanelContext context = BuildCurrentDungeonPanelContext();
        PrototypeDungeonPanelOption[] options = context != null && context.Options != null ? context.Options : System.Array.Empty<PrototypeDungeonPanelOption>();
        bool showLaneOptions = _dungeonRunState == DungeonRunState.Explore;

        if (_panelShellBackdropRenderer != null)
        {
            _panelShellBackdropRenderer.color = new Color(0.06f, 0.08f, 0.11f, 0.96f);
        }
        if (_panelShellTrackRenderer != null)
        {
            _panelShellTrackRenderer.color = new Color(0.10f, 0.14f, 0.19f, 0.96f);
        }
        if (_panelShellSidebarRenderer != null)
        {
            _panelShellSidebarRenderer.color = new Color(0.08f, 0.11f, 0.16f, 0.94f);
        }
        if (_panelShellHeaderRenderer != null)
        {
            _panelShellHeaderRenderer.color = new Color(0.16f, 0.22f, 0.30f, 0.98f);
        }

        SetPanelText(_panelShellTitleMesh, context != null ? context.Template.StageLabel : "Dungeon Panel", 72, 0.15f, TextAlignment.Center);
        SetPanelText(_panelShellSubtitleMesh, BuildExploreShellSubtitleText(context), 44, 0.12f, TextAlignment.Center);
        SetPanelText(_panelShellSidebarMesh, BuildExploreShellSidebarText(context), 38, 0.10f, TextAlignment.Left);
        SetPanelText(_panelShellFooterMesh, BuildExploreShellFooterText(context), 36, 0.10f, TextAlignment.Center);

        for (int i = 0; i < _panelShellLaneRenderers.Length; i++)
        {
            SpriteRenderer laneRenderer = _panelShellLaneRenderers[i];
            SpriteRenderer accentRenderer = _panelShellLaneAccentRenderers[i];
            TextMesh labelMesh = _panelShellLaneLabelMeshes[i];
            TextMesh hintMesh = _panelShellLaneHintMeshes[i];
            PrototypeDungeonPanelOption option = i < options.Length ? options[i] : null;
            bool visible = showLaneOptions && option != null;
            if (laneRenderer != null) laneRenderer.gameObject.SetActive(visible);
            if (accentRenderer != null) accentRenderer.gameObject.SetActive(visible);
            if (labelMesh != null) labelMesh.gameObject.SetActive(visible);
            if (hintMesh != null) hintMesh.gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            bool isHovered = _hoverPanelLaneOptionId == option.OptionId;
            bool isSelected = _selectedPanelLaneOptionId == option.OptionId;
            Color baseColor = option.IsBlocked
                ? new Color(0.16f, 0.18f, 0.22f, 0.72f)
                : isSelected
                    ? new Color(0.24f, 0.38f, 0.56f, 0.96f)
                    : isHovered
                        ? new Color(0.20f, 0.31f, 0.45f, 0.94f)
                        : new Color(0.14f, 0.20f, 0.28f, 0.90f);
            Color accentColor = option.IsBlocked
                ? new Color(0.32f, 0.32f, 0.32f, 0.45f)
                : i == 0
                    ? new Color(0.46f, 0.78f, 0.86f, 1f)
                    : i == 1
                        ? new Color(0.80f, 0.74f, 0.44f, 1f)
                        : new Color(0.88f, 0.54f, 0.36f, 1f);

            laneRenderer.color = baseColor;
            accentRenderer.color = accentColor;
            SetPanelText(labelMesh, option.OptionLabel, 52, 0.13f, TextAlignment.Left);
            SetPanelText(hintMesh, BuildLaneHintText(option), 36, 0.095f, TextAlignment.Left);
        }
    }

    private string BuildExploreShellSubtitleText(PrototypeDungeonPanelContext context)
    {
        string dungeonText = context != null && !string.IsNullOrEmpty(context.DungeonLabel) ? context.DungeonLabel : "Dungeon";
        string routeText = context != null && !string.IsNullOrEmpty(context.RouteLabel) ? context.RouteLabel : "Route";
        string progressText = context != null && !string.IsNullOrEmpty(context.ProgressSummaryText) ? context.ProgressSummaryText : GetRoomProgressText();
        return dungeonText + " | " + routeText + " | Panel " + progressText;
    }

    private string BuildExploreShellSidebarText(PrototypeDungeonPanelContext context)
    {
        List<string> lines = new List<string>(8);
        if (context != null)
        {
            lines.Add("Type: " + SafePanelText(context.RoomTypeLabel));
            lines.Add("Goal: " + SafePanelText(context.NextMajorGoalText));
            lines.Add("Party: " + BuildTotalPartyHpSummary() + " | " + GetPartyConditionText());
            lines.Add("Threat: " + SafePanelText(context.ThreatPressureSummaryText));
            lines.Add("Supply: " + SafePanelText(context.SupplyPressureSummaryText));
            lines.Add("Discovery: " + SafePanelText(context.DiscoverySummaryText));
            lines.Add("Extraction: " + SafePanelText(context.ExtractionPressureSummaryText));
            lines.Add("Lane: " + SafePanelText(CurrentLaneSelectionText));
        }
        else
        {
            lines.Add("Dungeon panel inactive.");
        }

        return string.Join("\n", lines.ToArray());
    }

    private string BuildExploreShellFooterText(PrototypeDungeonPanelContext context)
    {
        if (_dungeonRunState != DungeonRunState.Explore)
        {
            return SafePanelText(CurrentSelectionPromptText);
        }

        return "[1] Upper  [2] Middle  [3] Lower  |  Click lane  |  [Q] Retreat\n" +
               "Delta: " + SafePanelText(CurrentPanelDeltaSummaryText) + " | Last choice: " + SafePanelText(context != null ? context.LatestChoiceOutcomeSummaryText : "None");
    }

    private string BuildLaneHintText(PrototypeDungeonPanelOption option)
    {
        if (option == null)
        {
            return "None";
        }

        string riskText = SafePanelText(option.RiskHintText);
        string rewardText = SafePanelText(option.RewardHintText);
        if (string.IsNullOrEmpty(riskText) || riskText == "None")
        {
            return rewardText;
        }
        if (string.IsNullOrEmpty(rewardText) || rewardText == "None")
        {
            return riskText;
        }
        return riskText + " | " + rewardText;
    }

    private string SafePanelText(string text)
    {
        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private void EnsureDungeonPanelShellVisuals()
    {
        if (_panelShellBackdropRenderer != null)
        {
            return;
        }

        _panelShellBackdropRenderer = CreateDungeonToken(_dungeonRoot.transform, "PanelShellBackdrop", new Vector3(0f, 0f, 0f), new Vector2(16.6f, 8.9f), 0).GetComponent<SpriteRenderer>();
        _panelShellTrackRenderer = CreateDungeonToken(_dungeonRoot.transform, "PanelShellTrack", new Vector3(-1.75f, 0f, 0f), new Vector2(10.6f, 6.95f), 1).GetComponent<SpriteRenderer>();
        _panelShellSidebarRenderer = CreateDungeonToken(_dungeonRoot.transform, "PanelShellSidebar", new Vector3(4.55f, 0f, 0f), new Vector2(4.15f, 6.95f), 2).GetComponent<SpriteRenderer>();
        _panelShellHeaderRenderer = CreateDungeonToken(_dungeonRoot.transform, "PanelShellHeader", new Vector3(-1.75f, 3.38f, 0f), new Vector2(10.6f, 1.05f), 3).GetComponent<SpriteRenderer>();

        _panelShellTitleMesh = CreatePanelText(_dungeonRoot.transform, "PanelShellTitle", new Vector3(-1.75f, 3.46f, -0.2f), TextAnchor.MiddleCenter, 4);
        _panelShellSubtitleMesh = CreatePanelText(_dungeonRoot.transform, "PanelShellSubtitle", new Vector3(-1.75f, 2.92f, -0.2f), TextAnchor.MiddleCenter, 4);
        _panelShellSidebarMesh = CreatePanelText(_dungeonRoot.transform, "PanelShellSidebarText", new Vector3(3.18f, 2.35f, -0.2f), TextAnchor.UpperLeft, 4);
        _panelShellFooterMesh = CreatePanelText(_dungeonRoot.transform, "PanelShellFooterText", new Vector3(-1.75f, -3.42f, -0.2f), TextAnchor.MiddleCenter, 4);

        for (int i = 0; i < ExploreLaneCount; i++)
        {
            Vector3 lanePosition = ExploreLaneCardPositions[i];
            _panelShellLaneRenderers[i] = CreateDungeonToken(_dungeonRoot.transform, "LaneCard_" + i, lanePosition, new Vector2(9.6f, 1.55f), 4).GetComponent<SpriteRenderer>();
            _panelShellLaneAccentRenderers[i] = CreateDungeonToken(_dungeonRoot.transform, "LaneAccent_" + i, lanePosition + new Vector3(-4.45f, 0f, -0.05f), new Vector2(0.26f, 1.35f), 5).GetComponent<SpriteRenderer>();
            _panelShellLaneLabelMeshes[i] = CreatePanelText(_dungeonRoot.transform, "LaneLabel_" + i, lanePosition + new Vector3(-3.95f, 0.32f, -0.2f), TextAnchor.MiddleLeft, 6);
            _panelShellLaneHintMeshes[i] = CreatePanelText(_dungeonRoot.transform, "LaneHint_" + i, lanePosition + new Vector3(-3.95f, -0.30f, -0.2f), TextAnchor.MiddleLeft, 6);
        }
    }

    private TextMesh CreatePanelText(Transform parent, string name, Vector3 localPosition, TextAnchor anchor, int sortingOrder)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = localPosition;
        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.anchor = anchor;
        textMesh.alignment = anchor == TextAnchor.MiddleCenter ? TextAlignment.Center : TextAlignment.Left;
        textMesh.characterSize = 0.10f;
        textMesh.fontSize = 48;
        textMesh.color = new Color(0.92f, 0.95f, 0.98f, 1f);
        MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = sortingOrder;
        }
        return textMesh;
    }

    private void SetPanelText(TextMesh textMesh, string text, int fontSize, float characterSize, TextAlignment alignment)
    {
        if (textMesh == null)
        {
            return;
        }

        textMesh.text = string.IsNullOrEmpty(text) ? "None" : text;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = characterSize;
        textMesh.alignment = alignment;
    }
}
