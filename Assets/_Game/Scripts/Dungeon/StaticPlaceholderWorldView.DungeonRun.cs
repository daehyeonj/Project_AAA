using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class StaticPlaceholderWorldView
{
    private const int DungeonGridWidth = 17;
    private const int DungeonGridHeight = 9;
    private const int RecentBattleLogLimit = 3;
    private const int TotalEncounterCount = 3;
    private const int TotalChestCount = 1;
    private const string DungeonRewardResourceId = "mana_shard";
    private const int DungeonRewardAmount = 2;
    private const int GoblinRewardAmount = 3;
    private const int ChestRewardAmount = 2;
    private const int BattleFloatingPopupCount = 8;
    private const float BattleFloatingPopupDuration = 0.78f;
    private const float BattleFloatingPopupRiseDistance = 0.38f;
    private const float BattleInputLockSeconds = 0.18f;
    private const float BattleFeedbackFlashSeconds = 0.24f;
    private const float EnemyIntentTelegraphSeconds = 0.42f;
    private const int ShrineEventBonusLootAmount = 5;
    private const int ShrineEventRecoverAmount = 4;
    private const string SafeRouteId = "safe";
    private const string RiskyRouteId = "risky";
    private static readonly Vector2Int ShrineEventGridPosition = new Vector2Int(8, 4);
    private static readonly Vector2Int Room2EventGridPosition = new Vector2Int(12, 2);
    private static readonly Vector2Int Room2ChestGridPosition = new Vector2Int(14, 2);
    private static readonly Vector2Int Room1EncounterMarkerPosition = new Vector2Int(4, 5);
    private static readonly Vector2Int Room2EncounterMarkerPosition = new Vector2Int(13, 4);
    private static readonly Vector2Int PreparationRoomGridPosition = new Vector2Int(9, 4);
    private static readonly Vector3[] BattleMonsterViewPositions =
    {
        new Vector3(5.2f, 1.05f, 0f),
        new Vector3(4.15f, -0.2f, 0f)
    };

    private static readonly Vector3[] BattlePartyViewPositions =
    {
        new Vector3(-6.0f, 1.15f, 0f),
        new Vector3(-4.85f, 0.25f, 0f),
        new Vector3(-5.75f, -0.75f, 0f),
        new Vector3(-4.6f, -1.65f, 0f)
    };

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

    private enum BattleState
    {
        None,
        PartyActionSelect,
        PartyTargetSelect,
        EnemyTurn,
        Victory,
        Defeat,
        Retreat
    }

    private enum RunResultState
    {
        None,
        Playing,
        Clear,
        Defeat,
        Retreat
    }

    private enum BattleActionType
    {
        None,
        Attack,
        Skill,
        Retreat
    }

    private enum PartySkillType
    {
        SingleHeavy,
        Finisher,
        AllEnemies,
        PartyHeal
    }

    private enum MonsterEncounterRole
    {
        Bulwark,
        Skirmisher,
        Striker
    }

    private enum MonsterTargetPattern
    {
        FirstLiving,
        RandomLiving,
        LowestHpLiving
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

    private enum DungeonRoomType
    {
        Start,
        Skirmish,
        Cache,
        Shrine,
        Preparation,
        Elite
    }

    private sealed class DungeonRoomTemplateData
    {
        public string RoomId;
        public string DisplayName;
        public string RoomTypeLabel;
        public DungeonRoomType RoomType;
        public Vector2Int MarkerPosition;
        public string EncounterId;

        public DungeonRoomTemplateData(string roomId, string displayName, string roomTypeLabel, DungeonRoomType roomType, Vector2Int markerPosition, string encounterId = "")
        {
            RoomId = roomId ?? string.Empty;
            DisplayName = string.IsNullOrEmpty(displayName) ? "Room" : displayName;
            RoomTypeLabel = string.IsNullOrEmpty(roomTypeLabel) ? "Room" : roomTypeLabel;
            RoomType = roomType;
            MarkerPosition = markerPosition;
            EncounterId = encounterId ?? string.Empty;
        }
    }

    private sealed class DungeonRouteTemplate
    {
        public string RouteId;
        public string RouteLabel;
        public string Description;
        public string RiskLabel;
        public string EncounterPreview;
        public string EventFocus;
        public string RewardPreview;
        public int BattleLootAmount;
        public int RecoverAmount;
        public int BonusLootAmount;
        public int ChestRewardAmount;

        public DungeonRouteTemplate(string routeId, string routeLabel, string description, string riskLabel, string encounterPreview, string eventFocus, string rewardPreview, int battleLootAmount, int recoverAmount, int bonusLootAmount, int chestRewardAmount)
        {
            RouteId = routeId ?? string.Empty;
            RouteLabel = string.IsNullOrEmpty(routeLabel) ? "Route" : routeLabel;
            Description = string.IsNullOrEmpty(description) ? "A fixed dungeon path." : description;
            RiskLabel = string.IsNullOrEmpty(riskLabel) ? "Standard" : riskLabel;
            EncounterPreview = string.IsNullOrEmpty(encounterPreview) ? "Two encounters" : encounterPreview;
            EventFocus = string.IsNullOrEmpty(eventFocus) ? "Balanced blessing" : eventFocus;
            RewardPreview = string.IsNullOrEmpty(rewardPreview) ? "Balanced rewards" : rewardPreview;
            BattleLootAmount = battleLootAmount > 0 ? battleLootAmount : DungeonRewardAmount * 4;
            RecoverAmount = recoverAmount > 0 ? recoverAmount : ShrineEventRecoverAmount;
            BonusLootAmount = bonusLootAmount > 0 ? bonusLootAmount : ShrineEventBonusLootAmount;
            ChestRewardAmount = chestRewardAmount > 0 ? chestRewardAmount : StaticPlaceholderWorldView.ChestRewardAmount;
        }
    }

    private sealed class DungeonIdentityTemplate
    {
        public string DungeonId;
        public string DungeonLabel;
        public string DangerLabel;
        public string StyleLabel;
        public DungeonRouteTemplate RouteOption1;
        public DungeonRouteTemplate RouteOption2;

        public DungeonIdentityTemplate(string dungeonId, string dungeonLabel, string dangerLabel, string styleLabel, DungeonRouteTemplate routeOption1, DungeonRouteTemplate routeOption2)
        {
            DungeonId = dungeonId ?? string.Empty;
            DungeonLabel = string.IsNullOrEmpty(dungeonLabel) ? "Dungeon" : dungeonLabel;
            DangerLabel = string.IsNullOrEmpty(dangerLabel) ? "Standard" : dangerLabel;
            StyleLabel = string.IsNullOrEmpty(styleLabel) ? "Balanced" : styleLabel;
            RouteOption1 = routeOption1;
            RouteOption2 = routeOption2;
        }
    }

    private static readonly DungeonRouteTemplate AlphaSafeRouteTemplate = new DungeonRouteTemplate(
        SafeRouteId,
        "Rest Path",
        "Calmer ruins with slime-heavy fights and a stronger shrine recovery.",
        "Low",
        "Slime Pack -> Slime Watch -> Slime Monarch",
        "Recover choice is strongest.",
        "Steady shards with a safer elite bounty.",
        14,
        8,
        3,
        2);

    private static readonly DungeonRouteTemplate AlphaRiskyRouteTemplate = new DungeonRouteTemplate(
        RiskyRouteId,
        "Standard Path",
        "A balanced path with one goblin pair and a volatile elite finish.",
        "Medium",
        "Slime Scouts -> Goblin Pair -> Gel Core",
        "Balanced recover or shard choice.",
        "Balanced shards with a sharper elite payout spike.",
        17,
        6,
        5,
        3);

    private static readonly DungeonRouteTemplate BetaSafeRouteTemplate = new DungeonRouteTemplate(
        SafeRouteId,
        "Guarded Path",
        "A watch path with steady goblin pressure and a guarded elite finish for Beta.",
        "Medium",
        "Scout Pair -> Guarded Vault -> Goblin Captain",
        "Recover is modest, bonus loot is stronger.",
        "Balanced payout with a guarded elite bounty.",
        18,
        4,
        6,
        4);

    private static readonly DungeonRouteTemplate BetaRiskyRouteTemplate = new DungeonRouteTemplate(
        RiskyRouteId,
        "Greedy Path",
        "A vault route full of goblin ambushes and a raider elite guarding the exit.",
        "High",
        "Goblin Scouts -> Vault Ambush -> Raider Chief",
        "Bonus loot is strongest, recovery is weakest.",
        "Highest shard payout with the sharpest elite risk.",
        24,
        2,
        9,
        6);

    private static readonly DungeonIdentityTemplate AlphaDungeonTemplate = new DungeonIdentityTemplate(
        "dungeon-alpha",
        "Dungeon Alpha",
        "Safer",
        "Slime-heavy / Recover-friendly",
        AlphaSafeRouteTemplate,
        AlphaRiskyRouteTemplate);

    private static readonly DungeonIdentityTemplate BetaDungeonTemplate = new DungeonIdentityTemplate(
        "dungeon-beta",
        "Dungeon Beta",
        "Risky",
        "Goblin-heavy / Loot-heavy",
        BetaSafeRouteTemplate,
        BetaRiskyRouteTemplate);
    private sealed class DungeonPartyMemberRuntimeData
    {
        public string MemberId;
        public string DisplayName;
        public string RoleLabel;
        public string RoleTag;
        public string DefaultSkillId;
        public string SkillName;
        public string SkillShortText;
        public PartySkillType SkillType;
        public int PartySlotIndex;
        public int MaxHp;
        public int CurrentHp;
        public int Attack;
        public int Defense;
        public int Speed;
        public int SkillPower;
        public bool IsDefeated;
        public Color ViewColor;

        public DungeonPartyMemberRuntimeData(string memberId, string displayName, string roleLabel, string roleTag, string defaultSkillId, string skillName, string skillShortText, PartySkillType skillType, int partySlotIndex, int maxHp, int attack, int defense, int speed, int skillPower, Color viewColor)
        {
            MemberId = memberId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            RoleLabel = string.IsNullOrEmpty(roleLabel) ? "Adventurer" : roleLabel;
            RoleTag = string.IsNullOrEmpty(roleTag) ? "adventurer" : roleTag;
            DefaultSkillId = string.IsNullOrEmpty(defaultSkillId) ? string.Empty : defaultSkillId;
            SkillName = string.IsNullOrEmpty(skillName) ? "Skill" : skillName;
            SkillShortText = string.IsNullOrEmpty(skillShortText) ? string.Empty : skillShortText;
            SkillType = skillType;
            PartySlotIndex = partySlotIndex >= 0 ? partySlotIndex : 0;
            MaxHp = maxHp > 0 ? maxHp : 1;
            CurrentHp = MaxHp;
            Attack = attack > 0 ? attack : 1;
            Defense = defense >= 0 ? defense : 0;
            Speed = speed >= 0 ? speed : 0;
            SkillPower = skillPower > 0 ? skillPower : Attack + 1;
            IsDefeated = false;
            ViewColor = viewColor.a > 0f ? viewColor : Color.white;
        }

        public void ResetForRun()
        {
            CurrentHp = MaxHp;
            IsDefeated = false;
        }
    }

    private sealed class TestDungeonPartyData
    {
        public string PartyId;
        public string HomeCityId;
        public string DisplayName;
        public PrototypeRpgPartyDefinition PartyDefinition;
        public DungeonPartyMemberRuntimeData[] Members;

        public TestDungeonPartyData(string partyId, string homeCityId, PrototypeRpgPartyDefinition partyDefinition, DungeonPartyMemberRuntimeData[] members)
        {
            PartyId = partyId ?? string.Empty;
            HomeCityId = homeCityId ?? string.Empty;
            PartyDefinition = partyDefinition ?? PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
            DisplayName = PartyDefinition != null && !string.IsNullOrEmpty(PartyDefinition.DisplayName)
                ? PartyDefinition.DisplayName
                : (string.IsNullOrEmpty(PartyId) ? "Test Party" : PartyId);
            Members = members ?? System.Array.Empty<DungeonPartyMemberRuntimeData>();
        }

        public void ResetForRun()
        {
            for (int i = 0; i < Members.Length; i++)
            {
                if (Members[i] != null)
                {
                    Members[i].ResetForRun();
                }
            }
        }
    }

    private sealed class DungeonMonsterRuntimeData
    {
        public string MonsterId;
        public string EncounterId;
        public int RoomIndex;
        public string DisplayName;
        public string MonsterType;
        public int MaxHp;
        public int CurrentHp;
        public int Attack;
        public Vector2Int GridPosition;
        public bool IsDefeated;
        public string RewardResourceId;
        public int RewardAmount;
        public MonsterEncounterRole EncounterRole;
        public MonsterTargetPattern TargetPattern;
        public bool IsElite;
        public int SpecialAttack;
        public string SpecialActionName;
        public int TurnsActed;

        public DungeonMonsterRuntimeData(string monsterId, string encounterId, int roomIndex, string displayName, string monsterType, int hp, int attack, Vector2Int gridPosition, string rewardResourceId, int rewardAmount, MonsterTargetPattern targetPattern, MonsterEncounterRole encounterRole = MonsterEncounterRole.Bulwark, bool isElite = false, int specialAttack = 0, string specialActionName = "")
        {
            MonsterId = monsterId ?? string.Empty;
            EncounterId = encounterId ?? string.Empty;
            RoomIndex = roomIndex;
            DisplayName = displayName ?? string.Empty;
            MonsterType = string.IsNullOrEmpty(monsterType) ? "Monster" : monsterType;
            MaxHp = hp > 0 ? hp : 1;
            CurrentHp = MaxHp;
            Attack = attack > 0 ? attack : 1;
            GridPosition = gridPosition;
            IsDefeated = false;
            RewardResourceId = string.IsNullOrEmpty(rewardResourceId) ? DungeonRewardResourceId : rewardResourceId;
            RewardAmount = rewardAmount > 0 ? rewardAmount : 1;
            EncounterRole = encounterRole;
            TargetPattern = targetPattern;
            IsElite = isElite;
            SpecialAttack = specialAttack > 0 ? specialAttack : Mathf.Max(Attack + 2, Attack);
            SpecialActionName = string.IsNullOrEmpty(specialActionName) ? "Heavy Strike" : specialActionName;
            TurnsActed = 0;
        }
    }

    private sealed class DungeonEncounterRuntimeData
    {
        public string EncounterId;
        public int RoomIndex;
        public string DisplayName;
        public string[] MonsterIds;
        public bool IsEliteEncounter;
        public bool IsCleared;

        public DungeonEncounterRuntimeData(string encounterId, int roomIndex, string displayName, string[] monsterIds, bool isEliteEncounter = false)
        {
            EncounterId = encounterId ?? string.Empty;
            RoomIndex = roomIndex;
            DisplayName = displayName ?? string.Empty;
            MonsterIds = monsterIds ?? System.Array.Empty<string>();
            IsEliteEncounter = isEliteEncounter;
            IsCleared = false;
        }
    }

    private sealed class DungeonChestRuntimeData
    {
        public string ChestId;
        public int RoomIndex;
        public Vector2Int GridPosition;
        public string RewardResourceId;
        public int RewardAmount;
        public bool IsOpened;

        public DungeonChestRuntimeData(string chestId, int roomIndex, Vector2Int gridPosition, string rewardResourceId, int rewardAmount)
        {
            ChestId = chestId ?? string.Empty;
            RoomIndex = roomIndex;
            GridPosition = gridPosition;
            RewardResourceId = string.IsNullOrEmpty(rewardResourceId) ? DungeonRewardResourceId : rewardResourceId;
            RewardAmount = rewardAmount > 0 ? rewardAmount : 1;
            IsOpened = false;
        }
    }

    private sealed class BattleFloatingPopup
    {
        public GameObject Root;
        public TextMesh TextMesh;
    }

    private readonly Dictionary<string, TestDungeonPartyData> _dungeonPartyByCityId = new Dictionary<string, TestDungeonPartyData>();
    private readonly List<string> _recentBattleLogs = new List<string>(RecentBattleLogLimit);
    private readonly Dictionary<string, string> _lastDispatchImpactByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _lastDispatchStockDeltaByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _lastNeedPressureChangeByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, DispatchReadinessState> _dispatchReadinessByCityId = new Dictionary<string, DispatchReadinessState>();
    private readonly Dictionary<string, int> _dispatchRecoveryDaysRemainingByCityId = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _daysSinceLastDispatchByCityId = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _consecutiveDispatchCountByCityId = new Dictionary<string, int>();
    private readonly Dictionary<string, string> _lastDispatchReadinessChangeByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, DispatchPolicyState> _dispatchPolicyByCityId = new Dictionary<string, DispatchPolicyState>();
    private readonly List<DungeonMonsterRuntimeData> _activeMonsters = new List<DungeonMonsterRuntimeData>();
    private readonly List<DungeonEncounterRuntimeData> _activeEncounters = new List<DungeonEncounterRuntimeData>();
    private readonly List<DungeonRoomTemplateData> _plannedRooms = new List<DungeonRoomTemplateData>();
    private readonly List<string> _roomPathHistory = new List<string>();
    private readonly Dictionary<string, SpriteRenderer> _monsterRendererById = new Dictionary<string, SpriteRenderer>();
    private readonly bool[] _partyActedThisRound = new bool[4];

    private GameObject _dungeonRoot;
    private Texture2D _dungeonTexture;
    private Sprite _dungeonSprite;
    private SpriteRenderer[,] _dungeonTileRenderers;
    private GameObject _playerToken;
    private SpriteRenderer _playerRenderer;
    private GameObject _exitToken;
    private SpriteRenderer _exitRenderer;
    private GameObject _chestToken;
    private SpriteRenderer _chestRenderer;
    private GameObject _eventToken;
    private SpriteRenderer _eventRenderer;
    private GameObject _battleViewRoot;
    private SpriteRenderer _battleBackdropRenderer;
    private SpriteRenderer _battleHeaderRenderer;
    private SpriteRenderer _battleStageRenderer;
    private SpriteRenderer _battleCommandPanelRenderer;
    private SpriteRenderer _battlePartyPanelRenderer;
    private SpriteRenderer _battleLogPanelRenderer;
    private readonly SpriteRenderer[] _battleMonsterPlateRenderers = new SpriteRenderer[2];
    private readonly SpriteRenderer[] _battleMonsterViewRenderers = new SpriteRenderer[2];
    private readonly SpriteRenderer[] _battlePartyPlateRenderers = new SpriteRenderer[4];
    private readonly SpriteRenderer[] _battlePartyViewRenderers = new SpriteRenderer[4];
    private readonly BattleFloatingPopup[] _battleFloatingPopups = new BattleFloatingPopup[BattleFloatingPopupCount];
    private readonly float[] _battlePopupStartTimes = new float[BattleFloatingPopupCount];
    private readonly float[] _battlePopupEndTimes = new float[BattleFloatingPopupCount];
    private readonly Vector3[] _battlePopupBasePositions = new Vector3[BattleFloatingPopupCount];
    private readonly Color[] _battlePopupColors = new Color[BattleFloatingPopupCount];
    private readonly float[] _partyFeedbackUntilTimes = new float[4];
    private readonly Color[] _partyFeedbackColors = new Color[4];
    private readonly float[] _monsterFeedbackUntilTimes = new float[2];
    private readonly Color[] _monsterFeedbackColors = new Color[2];
    private TestDungeonPartyData _activeDungeonParty;
    private DungeonChestRuntimeData _activeChest;
    private DungeonMonsterRuntimeData _activeBattleMonster;
    private Vector2Int _playerGridPosition = new Vector2Int(2, 4);
    private Vector2Int _exitGridPosition = new Vector2Int(14, 4);
    private Vector2Int _eventGridPosition = ShrineEventGridPosition;
    private string _currentDungeonId = string.Empty;
    private string _currentDungeonName = "None";
    private string _currentHomeCityId = string.Empty;
    private string _activeEncounterId = string.Empty;
    private string _activeBattleMonsterId = string.Empty;
    private string _hoverBattleMonsterId = string.Empty;
    private string _battleFeedbackText = "None";
    private string _enemyIntentText = "None";
    private string _eliteEncounterId = string.Empty;
    private string _eliteName = "None";
    private string _eliteType = "None";
    private string _eliteRewardLabel = "None";
    private string _pendingEnemyActionLabel = string.Empty;
    private string _resultEliteName = "None";
    private string _resultEliteRewardLabel = "None";
    private string _currentSelectionPrompt = "None";
    private string _resultSurvivingMembersText = "None";
    private string _resultPartyHpSummaryText = "None";
    private string _resultPartyConditionText = "None";
    private string _hoverRouteChoiceId = string.Empty;
    private string _selectedRouteChoiceId = string.Empty;
    private string _selectedRouteId = string.Empty;
    private string _selectedRouteLabel = "None";
    private string _selectedRouteDescription = "None";
    private string _selectedRouteRiskLabel = "None";
    private string _recommendedRouteId = string.Empty;
    private string _recommendedRouteLabel = "None";
    private string _recommendedRouteReason = "None";
    private string _expectedNeedImpactText = "None";
    private string _preRunNeedPressureText = "None";
    private string _preRunDispatchReadinessText = "None";
    private string _resultNeedPressureBeforeText = "None";
    private string _resultNeedPressureAfterText = "None";
    private string _resultDispatchReadinessBeforeText = "None";
    private string _resultDispatchReadinessAfterText = "None";
    private string _hoverEventChoiceId = string.Empty;
    private string _selectedEventChoiceId = string.Empty;
    private string _resultEventChoiceText = "None";
    private string _hoverPreEliteChoiceId = string.Empty;
    private string _selectedPreEliteChoiceId = string.Empty;
    private string _resultPreEliteChoiceText = "Pending";
    private string _currentRoomStepId = string.Empty;
    private string _resultRoomPathSummaryText = "None";
    private DungeonRunState _dungeonRunState = DungeonRunState.None;
    private BattleState _battleState = BattleState.None;
    private RunResultState _runResultState = RunResultState.None;
    private BattleActionType _queuedBattleAction = BattleActionType.None;
    private BattleActionType _hoverBattleAction = BattleActionType.None;
    private int _battleTurnIndex;
    private int _currentActorIndex = -1;
    private int _selectedPartyMemberIndex = -1;
    private int _runTurnCount;
    private int _carriedLootAmount;
    private int _battleLootAmount;
    private int _chestLootAmount;
    private int _eventLootAmount;
    private int _totalEventHealAmount;
    private int _currentRoomIndex = 1;
    private int _clearedEncounterCount;
    private int _chestOpenedCount;
    private int _resultTurnsTaken;
    private int _resultLootGained;
    private int _resultBattleLootGained;
    private int _resultChestLootGained;
    private int _resultEventLootGained;
    private int _resultClearedEncounters;
    private int _resultOpenedChests;
    private int _preRunManaShardStock;
    private int _preRunConsecutiveDispatchCount;
    private int _resultStockBefore;
    private int _resultStockAfter;
    private int _resultStockDelta;
    private int _resultConsecutiveDispatchBefore;
    private int _resultConsecutiveDispatchAfter;
    private int _resultRecoveryEtaDays;
    private int _enemyTurnMonsterCursor = -1;
    private int _pendingEnemyTargetIndex = -1;
    private int _eliteRewardAmount;
    private int _resultEliteRewardAmount;
    private int _preEliteHealAmount;
    private int _resultPreEliteHealAmount;
    private int _eliteBonusRewardPending;
    private int _eliteBonusRewardGrantedAmount;
    private int _resultEliteBonusRewardAmount;
    private int _pendingEnemyAttackPower;
    private bool _pendingEnemyUsedSpecialAttack;
    private bool _eliteRewardGranted;
    private bool _exitUnlocked;
    private bool _pendingDungeonExit;
    private bool _eventResolved;
    private bool _preEliteDecisionResolved;
    private bool _eliteEncounterActive;
    private bool _eliteDefeated;
    private bool _eliteBonusRewardGranted;
    private bool _resultEliteDefeated;
    private float _battleInputLockUntilTime;
    private float _enemyIntentResolveAtTime;
    private bool _wasBattleInputLockedLastFrame;
    private bool _enemyIntentTelegraphActive;
    private bool _followedRecommendation;

    public bool IsDungeonRunActive => _dungeonRunState != DungeonRunState.None;
    public bool IsBattleViewActive => _dungeonRunState == DungeonRunState.Battle;
    public bool IsDungeonRouteChoiceVisible => _dungeonRunState == DungeonRunState.RouteChoice;
    public bool IsDungeonEventChoiceVisible => _dungeonRunState == DungeonRunState.EventChoice;
    public bool IsDungeonPreEliteChoiceVisible => _dungeonRunState == DungeonRunState.PreEliteChoice;
    public bool IsDungeonResultPanelVisible => _dungeonRunState == DungeonRunState.ResultPanel;
    public string DungeonRunStateText => GetDungeonRunStateText();
    public string BattleViewStateText => IsBattleViewActive ? "Active" : "Inactive";
    public string CurrentEncounterNameText => GetCurrentEncounterNameText();
    public string EncounterRoomTypeText => GetEncounterRoomTypeText();
    public string CurrentCityText => string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetHomeCityDisplayName();
    public string CurrentDungeonRunText => string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
    public string CurrentDungeonDangerText => GetCurrentDungeonDangerText();
    public string CityManaShardStockText => GetCurrentCityManaShardStockText();
    public string NeedPressureText => GetCurrentNeedPressureText();
    public string DispatchReadinessText => GetCurrentDispatchReadinessText();
    public string DispatchRecoveryProgressText => GetCurrentDispatchRecoveryProgressText();
    public string DispatchConsecutiveCountText => GetCurrentDispatchConsecutiveCountText();
    public string DispatchPolicyText => GetCurrentDispatchPolicyText();
    public string RecommendedRouteText => string.IsNullOrEmpty(_recommendedRouteLabel) ? "None" : _recommendedRouteLabel;
    public string RecommendationReasonText => string.IsNullOrEmpty(_recommendedRouteReason) ? "None" : _recommendedRouteReason;
    public string RecoveryAdviceText => GetCurrentRecoveryAdviceText();
    public string ExpectedNeedImpactText => string.IsNullOrEmpty(_expectedNeedImpactText) ? "None" : _expectedNeedImpactText;
    public string ActiveDungeonPartyText => _activeDungeonParty != null ? _activeDungeonParty.DisplayName : "None";
    public string CurrentRoomText => GetCurrentRoomText();
    public string TotalPartyHpText => BuildTotalPartyHpSummary();
    public string PartyConditionText => GetPartyConditionText();
    public string SustainPressureText => GetSustainPressureText();
    public string CurrentRoomTypeText => GetCurrentRoomTypeText();
    public string RoomProgressText => GetRoomProgressText();
    public string NextMajorGoalText => GetNextMajorGoalText();
    public string CurrentRouteText => string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel;
    public string CurrentRouteRiskText => string.IsNullOrEmpty(_selectedRouteRiskLabel) ? "None" : _selectedRouteRiskLabel;
    public string CarriedLootText => _carriedLootAmount > 0 ? DungeonRewardResourceId + " x" + _carriedLootAmount : "None";
    public int DungeonRunTurnCount => _runTurnCount;
    public string EncounterProgressText => _clearedEncounterCount + " / " + TotalEncounterCount;
    public string ExitUnlockedText => _exitUnlocked ? "Unlocked" : "Locked";
    public string ChestOpenedText => _chestOpenedCount + " / " + TotalChestCount;
    public string RouteChoiceTitleText => "Dispatch Planner";
    public string RouteChoiceDescriptionText => GetRouteChoiceDescriptionText();
    public string RouteChoicePromptText => GetRouteChoicePromptText();
    public string RouteOption1Text => BuildRouteButtonLabel(_currentDungeonId, SafeRouteId);
    public string RouteOption2Text => BuildRouteButtonLabel(_currentDungeonId, RiskyRouteId);
    public string EventStatusText => GetEventStatusText();
    public string EventChoiceText => GetSelectedEventChoiceDisplayText();
    public string PreEliteChoiceText => GetSelectedPreEliteChoiceDisplayText();
    public string LootBreakdownText => BuildLootBreakdownSummary();
    public string BattleLootText => BuildLootAmountText(_battleLootAmount);
    public string ChestLootText => BuildLootAmountText(_chestLootAmount);
    public string EventLootText => BuildLootAmountText(_eventLootAmount);
    public string EventTitleText => GetCurrentEventTitleText();
    public string EventDescriptionText => GetEventDescriptionText();
    public string EventPromptText => GetEventPromptText();
    public string EventOptionAText => GetCurrentEventOptionAText();
    public string EventOptionBText => GetCurrentEventOptionBText();
    public string PreEliteTitleText => GetCurrentPreEliteTitleText();
    public string PreEliteDescriptionText => GetPreEliteDescriptionText();
    public string PreElitePromptText => GetPreElitePromptText();
    public string PreEliteOptionAText => GetCurrentPreEliteOptionAText();
    public string PreEliteOptionBText => GetCurrentPreEliteOptionBText();
    public string RecentBattleLog1Text => GetRecentBattleLogText(0);
    public string RecentBattleLog2Text => GetRecentBattleLogText(1);
    public string RecentBattleLog3Text => GetRecentBattleLogText(2);
    public string BattleStateText => _battleState == BattleState.None ? "None" : _battleState.ToString();
    public string CurrentBattleActorText => GetCurrentBattleActorText();
    public string BattleMonsterNameText => _activeBattleMonster != null ? _activeBattleMonster.DisplayName : "None";
    public string BattleMonsterHpText => _activeBattleMonster != null ? _activeBattleMonster.CurrentHp + " / " + _activeBattleMonster.MaxHp : "None";
    public string BattleMonster1Text => GetBattleMonsterSummaryText(0);
    public string BattleMonster2Text => GetBattleMonsterSummaryText(1);
    public string CurrentSelectionPromptText => string.IsNullOrEmpty(_currentSelectionPrompt) ? "None" : _currentSelectionPrompt;
    public string BattlePhaseText => GetBattlePhaseText();
    public string BattleCancelHintText => GetBattleCancelHintText();
    public string BattleFeedbackText => string.IsNullOrEmpty(_battleFeedbackText) ? "None" : _battleFeedbackText;
    public string EnemyIntentText => string.IsNullOrEmpty(_enemyIntentText) ? "None" : _enemyIntentText;
    public string EliteStatusText => GetEliteStatusText();
    public string EliteEncounterNameText => GetEliteEncounterNameText();
    public string EliteTypeText => string.IsNullOrEmpty(_eliteType) ? "None" : _eliteType;
    public string EliteHpText => GetEliteHpText();
    public string EliteDefeatedText => _eliteDefeated ? "Yes" : "No";
    public string EliteRewardStatusText => GetEliteRewardStatusText();
    public string EliteRewardHintText => GetEliteRewardHintText();
    public string ResultPanelStateText => _runResultState == RunResultState.None ? "None" : _runResultState.ToString();
    public string ResultPanelCityDispatchedFromText => string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetHomeCityDisplayName();
    public string ResultPanelDungeonChosenText => string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
    public string ResultPanelDungeonDangerText => GetCurrentDungeonDangerText();
    public string ResultPanelRouteChosenText => string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel;
    public string ResultPanelRecommendedRouteText => string.IsNullOrEmpty(_recommendedRouteLabel) ? "None" : _recommendedRouteLabel;
    public string ResultPanelFollowedRecommendationText => _runResultState == RunResultState.None ? "None" : _followedRecommendation ? "Yes" : "No";
    public string ResultPanelManaShardsReturnedText => BuildLootAmountText(_resultLootGained);
    public string ResultPanelStockBeforeText => BuildLootAmountText(_resultStockBefore);
    public string ResultPanelStockAfterText => BuildLootAmountText(_resultStockAfter);
    public string ResultPanelStockDeltaText => BuildSignedLootAmountText(_resultStockDelta);
    public string ResultPanelNeedPressureBeforeText => string.IsNullOrEmpty(_resultNeedPressureBeforeText) ? "None" : _resultNeedPressureBeforeText;
    public string ResultPanelNeedPressureAfterText => string.IsNullOrEmpty(_resultNeedPressureAfterText) ? "None" : _resultNeedPressureAfterText;
    public string ResultPanelDispatchReadinessBeforeText => string.IsNullOrEmpty(_resultDispatchReadinessBeforeText) ? "None" : _resultDispatchReadinessBeforeText;
    public string ResultPanelDispatchReadinessAfterText => string.IsNullOrEmpty(_resultDispatchReadinessAfterText) ? "None" : _resultDispatchReadinessAfterText;
    public string ResultPanelRecoveryEtaText => BuildRecoveryEtaText(_resultRecoveryEtaDays);
    public string ResultPanelRouteRiskText => string.IsNullOrEmpty(_selectedRouteRiskLabel) ? "None" : _selectedRouteRiskLabel;
    public string ResultPanelTurnsTakenText => _resultTurnsTaken.ToString();
    public string ResultPanelLootGainedText => BuildLootAmountText(_resultLootGained);
    public string ResultPanelBattleLootText => BuildLootAmountText(_resultBattleLootGained);
    public string ResultPanelChestLootText => BuildLootAmountText(_resultChestLootGained);
    public string ResultPanelEventLootText => BuildLootAmountText(_resultEventLootGained);
    public string ResultPanelEventChoiceText => string.IsNullOrEmpty(_resultEventChoiceText) ? "None" : _resultEventChoiceText;
    public string ResultPanelSurvivingMembersText => string.IsNullOrEmpty(_resultSurvivingMembersText) ? "None" : _resultSurvivingMembersText;
    public string ResultPanelClearedEncountersText => _resultClearedEncounters + " / " + TotalEncounterCount;
    public string ResultPanelOpenedChestsText => _resultOpenedChests + " / " + TotalChestCount;
    public string ResultPanelEliteDefeatedText => _runResultState == RunResultState.None ? "None" : _resultEliteDefeated ? "Yes" : "No";
    public string ResultPanelEliteNameText => string.IsNullOrEmpty(_resultEliteName) ? "None" : _resultEliteName;
    public string ResultPanelEliteRewardIdentityText => string.IsNullOrEmpty(_resultEliteRewardLabel) ? "None" : _resultEliteRewardLabel;
    public string ResultPanelEliteRewardAmountText => _resultEliteRewardAmount > 0 ? BuildLootAmountText(_resultEliteRewardAmount) : "None";
    public string ResultPanelEliteRewardText => _resultEliteRewardAmount > 0 ? BuildLootAmountText(_resultEliteRewardAmount) : "None";
    public string ResultPanelPreEliteChoiceText => string.IsNullOrEmpty(_resultPreEliteChoiceText) ? "Pending" : _resultPreEliteChoiceText;
    public string ResultPanelPreEliteHealAmountText => _resultPreEliteHealAmount > 0 ? BuildRawHpAmountText(_resultPreEliteHealAmount) : "None";
    public string ResultPanelEliteBonusRewardEarnedText => _runResultState == RunResultState.None ? "None" : _eliteBonusRewardGranted ? "Yes" : "No";
    public string ResultPanelEliteBonusRewardAmountText => _resultEliteBonusRewardAmount > 0 ? BuildLootAmountText(_resultEliteBonusRewardAmount) : "None";
    public string ResultPanelRoomPathSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_resultRoomPathSummaryText) ? "None" : _resultRoomPathSummaryText);
    public string ResultPanelPartyHpSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_resultPartyHpSummaryText) ? "None" : _resultPartyHpSummaryText);
    public string ResultPanelPartyConditionText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_resultPartyConditionText) ? "None" : _resultPartyConditionText);
    public string ResultPanelReturnPromptText => _dungeonRunState == DungeonRunState.ResultPanel ? "[Enter] Return to World" : "None";
    public string SelectedDispatchPolicyText => GetSelectedDispatchPolicyText();
    public string SelectedMonsterCountPreviewText => GetSelectedEncounterCountPreviewText();

    public string GetPartyMemberHpText(int memberIndex)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || memberIndex < 0 || memberIndex >= _activeDungeonParty.Members.Length)
        {
            return "None";
        }

        DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[memberIndex];
        if (member == null)
        {
            return "None";
        }

        bool isIntentTarget = _dungeonRunState == DungeonRunState.Battle &&
                              _battleState == BattleState.EnemyTurn &&
                              _pendingEnemyTargetIndex == memberIndex &&
                              !member.IsDefeated &&
                              member.CurrentHp > 0;
        string statusText = member.IsDefeated
            ? " | KO"
            : isIntentTarget
                ? " | Targeted"
                : _dungeonRunState == DungeonRunState.Battle && _currentActorIndex == memberIndex
                    ? " | Acting"
                    : string.Empty;
        return BuildPartyMemberRuntimeSummary(member, statusText, true);
    }

    private string BuildPartyMemberRuntimeSummary(DungeonPartyMemberRuntimeData member, string statusText, bool includeHp)
    {
        if (member == null)
        {
            return "None";
        }

        string memberIdText = string.IsNullOrEmpty(member.MemberId) ? "none" : member.MemberId;
        string hpText = includeHp ? " | HP " + member.CurrentHp + " / " + member.MaxHp : string.Empty;
        string skillText = string.IsNullOrEmpty(member.SkillName) ? string.Empty : " | Skill " + member.SkillName;
        string safeStatusText = string.IsNullOrEmpty(statusText) ? string.Empty : statusText;
        return member.DisplayName + " | " + member.RoleLabel + " | ID " + memberIdText + hpText + skillText + " | ATK " + member.Attack + " DEF " + member.Defense + " SPD " + member.Speed + safeStatusText;
    }

    public bool IsBattleActionAvailable(string actionKey)
    {
        return IsBattleActionAvailable(ParseBattleActionType(actionKey));
    }

    public bool IsBattleActionHovered(string actionKey)
    {
        BattleActionType action = ParseBattleActionType(actionKey);
        return _dungeonRunState == DungeonRunState.Battle && action != BattleActionType.None && _hoverBattleAction == action;
    }

    public bool IsBattleActionSelected(string actionKey)
    {
        BattleActionType action = ParseBattleActionType(actionKey);
        return _dungeonRunState == DungeonRunState.Battle &&
               _battleState == BattleState.PartyTargetSelect &&
               action != BattleActionType.None &&
               _queuedBattleAction == action;
    }

    public void SetBattleActionHover(string actionKey)
    {
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            _hoverBattleAction = BattleActionType.None;
            return;
        }

        SetHoveredBattleAction(ParseBattleActionType(actionKey));
    }

    public bool TryTriggerBattleAction(string actionKey)
    {
        return TryActivateCurrentBattleAction(ParseBattleActionType(actionKey));
    }

    public bool IsRouteChoiceAvailable(string optionKey)
    {
        string routeId = NormalizeRouteChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.RouteChoice && !string.IsNullOrEmpty(routeId);
    }

    public bool IsRouteChoiceHovered(string optionKey)
    {
        string routeId = NormalizeRouteChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.RouteChoice && !string.IsNullOrEmpty(routeId) && _hoverRouteChoiceId == routeId;
    }

    public bool IsRouteChoiceSelected(string optionKey)
    {
        string routeId = NormalizeRouteChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.RouteChoice && !string.IsNullOrEmpty(routeId) && _selectedRouteChoiceId == routeId;
    }

    public void SetRouteChoiceHover(string optionKey)
    {
        _hoverRouteChoiceId = IsRouteChoiceAvailable(optionKey) ? NormalizeRouteChoiceId(optionKey) : string.Empty;
        RefreshSelectionPrompt();
    }

    public bool TryTriggerRouteChoice(string optionKey)
    {
        if (!IsRouteChoiceAvailable(optionKey))
        {
            return false;
        }

        DungeonRouteTemplate template = GetRouteTemplateById(NormalizeRouteChoiceId(optionKey));
        if (template == null)
        {
            return false;
        }

        _selectedRouteChoiceId = template.RouteId;
        _selectedRouteId = template.RouteId;
        _selectedRouteLabel = template.RouteLabel;
        _selectedRouteDescription = template.Description;
        _selectedRouteRiskLabel = template.RiskLabel;
        _followedRecommendation = template.RouteId == _recommendedRouteId;
        _hoverRouteChoiceId = string.Empty;
        SetBattleFeedbackText("Selected " + template.RouteLabel + ".");
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }

    public bool CanConfirmRouteChoice()
    {
        return _dungeonRunState == DungeonRunState.RouteChoice &&
               _runtimeEconomyState != null &&
               !string.IsNullOrEmpty(_currentHomeCityId) &&
               !string.IsNullOrEmpty(_currentDungeonId) &&
               !string.IsNullOrEmpty(_selectedRouteChoiceId);
    }

    public bool TryConfirmRouteChoice()
    {
        if (!CanConfirmRouteChoice())
        {
            return false;
        }

        DungeonRouteTemplate template = GetRouteTemplateById(_selectedRouteChoiceId);
        if (template == null)
        {
            return false;
        }

        string partyId = _runtimeEconomyState.BeginDungeonRun(_currentHomeCityId, _currentDungeonId);
        if (string.IsNullOrEmpty(partyId))
        {
            return false;
        }

        StartDungeonRunForRoute(template, partyId);
        return true;
    }

    public bool IsEventChoiceAvailable(string optionKey)
    {
        string choiceId = NormalizeEventChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.EventChoice && !_eventResolved && !string.IsNullOrEmpty(choiceId);
    }

    public bool IsEventChoiceHovered(string optionKey)
    {
        string choiceId = NormalizeEventChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.EventChoice && !string.IsNullOrEmpty(choiceId) && _hoverEventChoiceId == choiceId;
    }

    public bool IsEventChoiceSelected(string optionKey)
    {
        string choiceId = NormalizeEventChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.EventChoice && !string.IsNullOrEmpty(choiceId) && _selectedEventChoiceId == choiceId;
    }

    public void SetEventChoiceHover(string optionKey)
    {
        _hoverEventChoiceId = IsEventChoiceAvailable(optionKey) ? NormalizeEventChoiceId(optionKey) : string.Empty;
    }

    public bool TryTriggerEventChoice(string optionKey)
    {
        if (!IsEventChoiceAvailable(optionKey))
        {
            return false;
        }

        string choiceId = NormalizeEventChoiceId(optionKey);
        _selectedEventChoiceId = choiceId;
        _eventResolved = true;

        if (choiceId == "recover")
        {
            int recoveredAmount = ApplyShrineRecovery();
            _totalEventHealAmount += recoveredAmount;
            AppendBattleLog("Party rested at " + GetCurrentEventTitleText() + " and recovered " + recoveredAmount + " HP.");
            SetBattleFeedbackText(GetCurrentEventTitleText() + " restored " + recoveredAmount + " HP.");
        }
        else if (choiceId == "loot")
        {
            int bonusLootAmount = GetCurrentShrineBonusLootAmount();
            _carriedLootAmount += bonusLootAmount;
            _eventLootAmount += bonusLootAmount;
            AppendBattleLog("Party claimed " + bonusLootAmount + " " + DungeonRewardResourceId + " from " + GetCurrentEventTitleText() + ".");
            SetBattleFeedbackText(GetCurrentEventTitleText() + " added " + bonusLootAmount + " " + DungeonRewardResourceId + ".");
        }

        _hoverEventChoiceId = string.Empty;
        _dungeonRunState = DungeonRunState.Explore;
        RefreshRoomSequenceState(true);
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }

    public bool IsPreEliteChoiceAvailable(string optionKey)
    {
        string choiceId = NormalizePreEliteChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.PreEliteChoice && !_preEliteDecisionResolved && !string.IsNullOrEmpty(choiceId);
    }

    public bool IsPreEliteChoiceHovered(string optionKey)
    {
        string choiceId = NormalizePreEliteChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.PreEliteChoice && !string.IsNullOrEmpty(choiceId) && _hoverPreEliteChoiceId == choiceId;
    }

    public bool IsPreEliteChoiceSelected(string optionKey)
    {
        string choiceId = NormalizePreEliteChoiceId(optionKey);
        return _dungeonRunState == DungeonRunState.PreEliteChoice && !string.IsNullOrEmpty(choiceId) && _selectedPreEliteChoiceId == choiceId;
    }

    public void SetPreEliteChoiceHover(string optionKey)
    {
        _hoverPreEliteChoiceId = IsPreEliteChoiceAvailable(optionKey) ? NormalizePreEliteChoiceId(optionKey) : string.Empty;
    }

    public bool TryTriggerPreEliteChoice(string optionKey)
    {
        if (!IsPreEliteChoiceAvailable(optionKey))
        {
            return false;
        }

        string choiceId = NormalizePreEliteChoiceId(optionKey);
        _selectedPreEliteChoiceId = choiceId;
        _preEliteDecisionResolved = true;
        _preEliteHealAmount = 0;
        _eliteBonusRewardPending = 0;
        _eliteBonusRewardGranted = false;
        _eliteBonusRewardGrantedAmount = 0;

        if (choiceId == "recover")
        {
            _preEliteHealAmount = ApplyPreEliteRecovery();
            AppendBattleLog("Chose Recover before " + _eliteName + ".");
            AppendBattleLog("Party recovered " + _preEliteHealAmount + " HP before the elite.");
            AppendBattleLog("Party Condition is now " + GetPartyConditionText() + ". Total Party HP: " + BuildTotalPartyHpSummary() + ".");
            SetBattleFeedbackText("Preparation room restored " + _preEliteHealAmount + " HP.");
        }
        else if (choiceId == "bonus")
        {
            _eliteBonusRewardPending = GetCurrentEliteBonusRewardAmount();
            AppendBattleLog("Chose Bonus Reward before " + _eliteName + ".");
            AppendBattleLog("No recovery. " + _eliteRewardLabel + " bonus is now pending: " + BuildLootAmountText(_eliteBonusRewardPending) + ".");
            SetBattleFeedbackText("No recovery. " + _eliteRewardLabel + " bonus pending: " + BuildLootAmountText(_eliteBonusRewardPending) + ".");
        }

        _hoverPreEliteChoiceId = string.Empty;
        _dungeonRunState = DungeonRunState.Explore;
        RefreshRoomSequenceState(true);
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }

    private void InitializeDungeonRunSystems()
    {
        _dungeonPartyByCityId.Clear();
        _recentBattleLogs.Clear();
        InitializeDispatchRecoveryState();
        ResetDungeonRunPresentationState();
    }

    private void DisposeDungeonRunSystems()
    {
        ClearMonsterVisuals();

        if (_dungeonRoot != null)
        {
            Object.Destroy(_dungeonRoot);
            _dungeonRoot = null;
        }

        if (_dungeonSprite != null)
        {
            Object.Destroy(_dungeonSprite);
            _dungeonSprite = null;
        }

        if (_dungeonTexture != null)
        {
            Object.Destroy(_dungeonTexture);
            _dungeonTexture = null;
        }

        _dungeonTileRenderers = null;
        _playerToken = null;
        _playerRenderer = null;
        _exitToken = null;
        _exitRenderer = null;
        _chestToken = null;
        _chestRenderer = null;
        _eventToken = null;
        _eventRenderer = null;
        _battleViewRoot = null;
        _battleBackdropRenderer = null;
        _battleHeaderRenderer = null;
        _battleStageRenderer = null;
        _battleCommandPanelRenderer = null;
        _battlePartyPanelRenderer = null;
        _battleLogPanelRenderer = null;
        for (int i = 0; i < _battleMonsterPlateRenderers.Length; i++)
        {
            _battleMonsterPlateRenderers[i] = null;
            _battleMonsterViewRenderers[i] = null;
        }

        for (int i = 0; i < _battlePartyPlateRenderers.Length; i++)
        {
            _battlePartyPlateRenderers[i] = null;
            _battlePartyViewRenderers[i] = null;
        }
    }

    private void ResetDungeonRunSystems()
    {
        _dungeonPartyByCityId.Clear();
        _recentBattleLogs.Clear();
        InitializeDispatchRecoveryState();
        ClearMonsterVisuals();
        _activeMonsters.Clear();
        ResetDungeonRunPresentationState();

        if (_dungeonRoot != null)
        {
            _dungeonRoot.SetActive(false);
        }
    }

    private void InitializeDispatchRecoveryState()
    {
        _lastDispatchImpactByCityId.Clear();
        _lastDispatchStockDeltaByCityId.Clear();
        _lastNeedPressureChangeByCityId.Clear();
        _dispatchReadinessByCityId.Clear();
        _dispatchRecoveryDaysRemainingByCityId.Clear();
        _daysSinceLastDispatchByCityId.Clear();
        _consecutiveDispatchCountByCityId.Clear();
        _lastDispatchReadinessChangeByCityId.Clear();
        _dispatchPolicyByCityId.Clear();

        if (_worldData == null || _worldData.Entities == null)
        {
            return;
        }

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
    }

    private void AdvanceDispatchRecoveryForEconomyDays(int previousWorldDayCount, int currentWorldDayCount)
    {
        int daysAdvanced = currentWorldDayCount - previousWorldDayCount;
        if (daysAdvanced <= 0)
        {
            return;
        }

        for (int i = 0; i < daysAdvanced; i++)
        {
            AdvanceDispatchRecoveryOneDay();
        }
    }

    private void AdvanceDispatchRecoveryOneDay()
    {
        if (_dispatchReadinessByCityId.Count == 0)
        {
            return;
        }

        List<string> cityIds = new List<string>(_dispatchReadinessByCityId.Keys);
        for (int i = 0; i < cityIds.Count; i++)
        {
            string cityId = cityIds[i];
            EnsureDispatchReadinessEntry(cityId);

            int daysSinceLastDispatch = _daysSinceLastDispatchByCityId[cityId];
            if (daysSinceLastDispatch >= 0)
            {
                _daysSinceLastDispatchByCityId[cityId] = daysSinceLastDispatch + 1;
            }

            int recoveryDaysRemaining = _dispatchRecoveryDaysRemainingByCityId[cityId];
            if (recoveryDaysRemaining <= 0)
            {
                continue;
            }

            recoveryDaysRemaining -= 1;
            _dispatchRecoveryDaysRemainingByCityId[cityId] = Mathf.Max(0, recoveryDaysRemaining);
            if (recoveryDaysRemaining <= 0)
            {
                _dispatchReadinessByCityId[cityId] = DispatchReadinessState.Ready;
                _consecutiveDispatchCountByCityId[cityId] = 0;
            }
            else if (recoveryDaysRemaining == 1)
            {
                _dispatchReadinessByCityId[cityId] = DispatchReadinessState.Recovering;
            }
            else
            {
                _dispatchReadinessByCityId[cityId] = DispatchReadinessState.Strained;
            }
        }
    }

    public void SetWorldSimVisible(bool isVisible)
    {
        if (isVisible)
        {
            EnsureCreated();
        }

        if (_root != null)
        {
            _root.SetActive(isVisible);
        }
    }

    public void SetDungeonRunVisible(bool isVisible)
    {
        if (isVisible)
        {
            EnsureDungeonVisuals();
            RefreshDungeonPresentation();
        }

        if (_dungeonRoot != null)
        {
            _dungeonRoot.SetActive(isVisible && IsDungeonRunActive);
        }
    }

    public void ConfigureDungeonCamera(Camera worldCamera)
    {
        if (worldCamera == null)
        {
            return;
        }

        worldCamera.orthographic = true;
        worldCamera.transform.position = new Vector3(0f, 0f, -10f);
        worldCamera.orthographicSize = 5.6f;
    }

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
        _selectedRouteLabel = "None";
        _selectedRouteDescription = "None";
        _selectedRouteRiskLabel = "None";
        _recommendedRouteId = string.Empty;
        _recommendedRouteLabel = "None";
        _recommendedRouteReason = "None";
        _expectedNeedImpactText = "None";
        _preRunNeedPressureText = "None";
        _resultNeedPressureBeforeText = "None";
        _resultNeedPressureAfterText = "None";
        _pendingDungeonExit = false;
        _recentBattleLogs.Clear();
        RefreshDispatchRecommendation();
        if (!string.IsNullOrEmpty(_recommendedRouteId))
        {
            TryTriggerRouteChoice(_recommendedRouteId);
        }

        SetBattleFeedbackText("Review the dispatch plan before entering the dungeon.");
        EnsureDungeonVisuals();
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        ConfigureDungeonCamera(worldCamera);
        return true;
    }

    public void UpdateDungeonRun(Keyboard keyboard, Mouse mouse, Camera worldCamera)
    {
        if (!IsDungeonRunActive)
        {
            return;
        }

        UpdateBattleTransientState();

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            ClearBattleHoverState();
            _hoverPreEliteChoiceId = string.Empty;
            if (IsReturnToWorldPressed(keyboard))
            {
                _pendingDungeonExit = true;
            }

            return;
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            ClearBattleHoverState();
            if (keyboard != null)
            {
                HandleDungeonRouteChoiceInput(keyboard);
            }

            return;
        }

        if (_dungeonRunState == DungeonRunState.EventChoice)
        {
            ClearBattleHoverState();
            _hoverPreEliteChoiceId = string.Empty;
            if (keyboard != null)
            {
                HandleDungeonEventChoiceInput(keyboard);
            }

            return;
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            ClearBattleHoverState();
            if (keyboard != null)
            {
                HandleDungeonPreEliteChoiceInput(keyboard);
            }

            return;
        }

        if (_dungeonRunState == DungeonRunState.Explore)
        {
            ClearBattleHoverState();
            if (keyboard != null)
            {
                HandleDungeonExploreInput(keyboard);
            }

            return;
        }

        if (_dungeonRunState == DungeonRunState.Battle)
        {
            UpdateBattleMouseInteraction(mouse, worldCamera);
            if (keyboard != null)
            {
                HandleDungeonBattleInput(keyboard);
            }
        }
    }
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
        }

        return true;
    }

        private string GetSelectedEncounterCountPreviewText()
    {
        if (_selectedMarker == null)
        {
            return "None";
        }

        string dungeonId = _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
            ? _selectedMarker.EntityData.Id
            : _selectedMarker.EntityData.Kind == WorldEntityKind.City
                ? GetLinkedDungeonIdForCity(_selectedMarker.EntityData.Id)
                : string.Empty;

        return string.IsNullOrEmpty(dungeonId) ? "None" : BuildEncounterPreviewSummaryText(dungeonId);
    }

    private DungeonIdentityTemplate GetCurrentDungeonTemplate()
    {
        return GetDungeonTemplateById(_currentDungeonId);
    }

    private DungeonIdentityTemplate GetDungeonTemplateById(string dungeonId)
    {
        return dungeonId == "dungeon-beta" ? BetaDungeonTemplate : AlphaDungeonTemplate;
    }

    private DungeonRouteTemplate GetSelectedRouteTemplate()
    {
        DungeonRouteTemplate template = GetRouteTemplateById(_currentDungeonId, _selectedRouteId);
        return template ?? GetRouteTemplateById(_currentDungeonId, SafeRouteId) ?? AlphaSafeRouteTemplate;
    }

    private DungeonRouteTemplate GetRouteTemplateById(string routeId)
    {
        return GetRouteTemplateById(_currentDungeonId, routeId);
    }

    private DungeonRouteTemplate GetRouteTemplateById(string dungeonId, string routeId)
    {
        DungeonIdentityTemplate dungeonTemplate = GetDungeonTemplateById(dungeonId);
        string normalizedRouteId = NormalizeRouteChoiceId(routeId);
        if (dungeonTemplate == null || string.IsNullOrEmpty(normalizedRouteId))
        {
            return null;
        }

        if (normalizedRouteId == SafeRouteId)
        {
            return dungeonTemplate.RouteOption1;
        }

        if (normalizedRouteId == RiskyRouteId)
        {
            return dungeonTemplate.RouteOption2;
        }

        return null;
    }

    private string GetCurrentDungeonDangerText()
    {
        return string.IsNullOrEmpty(_currentDungeonId)
            ? "None"
            : BuildDungeonDangerSummaryText(_currentDungeonId);
    }

    private string BuildDungeonDangerSummaryText(string dungeonId)
    {
        DungeonIdentityTemplate dungeonTemplate = GetDungeonTemplateById(dungeonId);
        return dungeonTemplate == null
            ? "None"
            : dungeonTemplate.DangerLabel + " | " + dungeonTemplate.StyleLabel;
    }

    private int GetCurrentShrineRecoverAmount()
    {
        DungeonRouteTemplate template = GetSelectedRouteTemplate();
        return template != null ? template.RecoverAmount : ShrineEventRecoverAmount;
    }

    private int GetCurrentShrineBonusLootAmount()
    {
        DungeonRouteTemplate template = GetSelectedRouteTemplate();
        return template != null ? template.BonusLootAmount : ShrineEventBonusLootAmount;
    }

    private string BuildEncounterPreviewSummaryText(string dungeonId)
    {
        DungeonIdentityTemplate dungeonTemplate = GetDungeonTemplateById(dungeonId);
        return dungeonTemplate == null
            ? TotalEncounterCount + " per route"
            : TotalEncounterCount + " per route | " + dungeonTemplate.StyleLabel;
    }

    private string BuildRoutePreviewSummaryText(string routeId)
    {
        return BuildRoutePreviewSummaryText(_currentDungeonId, routeId);
    }

    private string BuildRoutePreviewSummaryText(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            return "None";
        }

        return template.RouteLabel + " | " + template.RiskLabel + " Risk | Rooms: " + BuildRoomPathPreviewText(dungeonId, routeId) + " | Encounters: " + template.EncounterPreview + " | Event: " + template.EventFocus + " | Reward: " + template.RewardPreview;
    }

    private string BuildRouteRewardPreviewText()
    {
        return BuildRouteRewardPreviewText(_currentDungeonId);
    }

    private string BuildRouteRewardPreviewText(string dungeonId)
    {
        DungeonRouteTemplate route1 = GetRouteTemplateById(dungeonId, SafeRouteId);
        DungeonRouteTemplate route2 = GetRouteTemplateById(dungeonId, RiskyRouteId);
        if (route1 == null || route2 == null)
        {
            return "None";
        }

        return route1.RouteLabel + ": Battle " + route1.BattleLootAmount + " / Chest " + route1.ChestRewardAmount + " / Event " + route1.BonusLootAmount + " / Total " + (route1.BattleLootAmount + route1.ChestRewardAmount + route1.BonusLootAmount) +
            " | " + route2.RouteLabel + ": Battle " + route2.BattleLootAmount + " / Chest " + route2.ChestRewardAmount + " / Event " + route2.BonusLootAmount + " / Total " + (route2.BattleLootAmount + route2.ChestRewardAmount + route2.BonusLootAmount);
    }

    private string BuildRouteEventPreviewText()
    {
        return BuildRouteEventPreviewText(_currentDungeonId);
    }

    private string BuildRouteEventPreviewText(string dungeonId)
    {
        DungeonRouteTemplate route1 = GetRouteTemplateById(dungeonId, SafeRouteId);
        DungeonRouteTemplate route2 = GetRouteTemplateById(dungeonId, RiskyRouteId);
        if (route1 == null || route2 == null)
        {
            return "None";
        }

        return route1.RouteLabel + ": Recover +" + route1.RecoverAmount + " HP or Bonus +" + route1.BonusLootAmount + " " + DungeonRewardResourceId +
            " | " + route2.RouteLabel + ": Recover +" + route2.RecoverAmount + " HP or Bonus +" + route2.BonusLootAmount + " " + DungeonRewardResourceId;
    }

    private string BuildSelectedRouteSummary()
    {
        return string.IsNullOrEmpty(_selectedRouteLabel)
            ? "None"
            : _selectedRouteLabel + " | " + _selectedRouteRiskLabel + " Risk";
    }

    private void AddPlannedRoomStep(string roomId, string displayName, string roomTypeLabel, DungeonRoomType roomType, Vector2Int markerPosition, string encounterId = "")
    {
        _plannedRooms.Add(new DungeonRoomTemplateData(roomId, displayName, roomTypeLabel, roomType, markerPosition, encounterId));
    }

    private void BuildPlannedRoomSequence()
    {
        _plannedRooms.Clear();

        string dungeonId = string.IsNullOrEmpty(_currentDungeonId) ? AlphaDungeonTemplate.DungeonId : _currentDungeonId;
        string routeId = string.IsNullOrEmpty(_selectedRouteId) ? SafeRouteId : _selectedRouteId;

        if (dungeonId == "dungeon-beta")
        {
            if (routeId == RiskyRouteId)
            {
                AddPlannedRoomStep("beta-risky-skirmish-1", "Raider Gate", "Skirmish Room", DungeonRoomType.Skirmish, Room1EncounterMarkerPosition, "encounter-room-1");
                AddPlannedRoomStep("beta-risky-cache", "Plunder Cache", "Cache Room", DungeonRoomType.Cache, ShrineEventGridPosition);
                AddPlannedRoomStep("beta-risky-skirmish-2", "Ambush Hall", "Skirmish Room", DungeonRoomType.Skirmish, Room2EncounterMarkerPosition, "encounter-room-2");
                AddPlannedRoomStep("beta-risky-shrine", "War Banner Shrine", "Shrine Room", DungeonRoomType.Shrine, Room2EventGridPosition);
                AddPlannedRoomStep("beta-risky-prep", "War Table", "Preparation Room", DungeonRoomType.Preparation, PreparationRoomGridPosition);
            }
            else
            {
                AddPlannedRoomStep("beta-safe-skirmish-1", "Scout Gate", "Skirmish Room", DungeonRoomType.Skirmish, Room1EncounterMarkerPosition, "encounter-room-1");
                AddPlannedRoomStep("beta-safe-shrine", "Watchfire Shrine", "Shrine Room", DungeonRoomType.Shrine, ShrineEventGridPosition);
                AddPlannedRoomStep("beta-safe-cache", "Guard Cache", "Cache Room", DungeonRoomType.Cache, Room2ChestGridPosition);
                AddPlannedRoomStep("beta-safe-skirmish-2", "Guarded Vault", "Skirmish Room", DungeonRoomType.Skirmish, Room2EncounterMarkerPosition, "encounter-room-2");
                AddPlannedRoomStep("beta-safe-prep", "Guard Muster", "Preparation Room", DungeonRoomType.Preparation, PreparationRoomGridPosition);
            }
        }
        else if (routeId == RiskyRouteId)
        {
            AddPlannedRoomStep("alpha-risky-skirmish-1", "Mixed Front", "Skirmish Room", DungeonRoomType.Skirmish, Room1EncounterMarkerPosition, "encounter-room-1");
            AddPlannedRoomStep("alpha-risky-cache", "Greed Cache", "Cache Room", DungeonRoomType.Cache, ShrineEventGridPosition);
            AddPlannedRoomStep("alpha-risky-skirmish-2", "Goblin Pair Hall", "Skirmish Room", DungeonRoomType.Skirmish, Room2EncounterMarkerPosition, "encounter-room-2");
            AddPlannedRoomStep("alpha-risky-shrine", "Unstable Shrine", "Shrine Room", DungeonRoomType.Shrine, Room2EventGridPosition);
            AddPlannedRoomStep("alpha-risky-prep", "Core Threshold", "Preparation Room", DungeonRoomType.Preparation, PreparationRoomGridPosition);
        }
        else
        {
            AddPlannedRoomStep("alpha-safe-skirmish-1", "Slime Front", "Skirmish Room", DungeonRoomType.Skirmish, Room1EncounterMarkerPosition, "encounter-room-1");
            AddPlannedRoomStep("alpha-safe-shrine", "Rest Shrine", "Shrine Room", DungeonRoomType.Shrine, ShrineEventGridPosition);
            AddPlannedRoomStep("alpha-safe-skirmish-2", "Watch Hall", "Skirmish Room", DungeonRoomType.Skirmish, Room2EncounterMarkerPosition, "encounter-room-2");
            AddPlannedRoomStep("alpha-safe-cache", "Supply Cache", "Cache Room", DungeonRoomType.Cache, Room2ChestGridPosition);
            AddPlannedRoomStep("alpha-safe-prep", "Quiet Antechamber", "Preparation Room", DungeonRoomType.Preparation, PreparationRoomGridPosition);
        }

        AddPlannedRoomStep(dungeonId + "-elite", "Elite Chamber", "Elite Chamber", DungeonRoomType.Elite, new Vector2Int(13, 5), _eliteEncounterId);
    }

    private string BuildRoomPathPreviewText(string dungeonId, string routeId)
    {
        bool isBeta = dungeonId == "dungeon-beta";
        bool isRisky = NormalizeRouteChoiceId(routeId) == RiskyRouteId;

        if (isBeta)
        {
            return isRisky
                ? "Skirmish -> Cache -> Skirmish -> Shrine -> Preparation -> Elite"
                : "Skirmish -> Shrine -> Cache -> Skirmish -> Preparation -> Elite";
        }

        return isRisky
            ? "Skirmish -> Cache -> Skirmish -> Shrine -> Preparation -> Elite"
            : "Skirmish -> Shrine -> Skirmish -> Cache -> Preparation -> Elite";
    }

    private DungeonRoomTemplateData GetCurrentPlannedRoomStep()
    {
        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            DungeonRoomTemplateData room = _plannedRooms[i];
            if (room != null && !IsRoomStepResolved(room))
            {
                return room;
            }
        }

        return null;
    }

    private int GetCurrentPlannedRoomIndex()
    {
        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            DungeonRoomTemplateData room = _plannedRooms[i];
            if (room != null && !IsRoomStepResolved(room))
            {
                return i;
            }
        }

        return -1;
    }

    private DungeonRoomTemplateData GetRoomStepByEncounterId(string encounterId)
    {
        if (string.IsNullOrEmpty(encounterId))
        {
            return null;
        }

        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            DungeonRoomTemplateData room = _plannedRooms[i];
            if (room != null && room.EncounterId == encounterId)
            {
                return room;
            }
        }

        return null;
    }

    private bool IsRoomStepResolved(DungeonRoomTemplateData room)
    {
        if (room == null)
        {
            return true;
        }

        if (room.RoomType == DungeonRoomType.Cache)
        {
            return _activeChest != null && _activeChest.IsOpened;
        }

        if (room.RoomType == DungeonRoomType.Shrine)
        {
            return _eventResolved;
        }

        if (room.RoomType == DungeonRoomType.Preparation)
        {
            return _preEliteDecisionResolved;
        }

        if (room.RoomType == DungeonRoomType.Elite)
        {
            return _eliteDefeated;
        }

        if (room.RoomType == DungeonRoomType.Skirmish)
        {
            DungeonEncounterRuntimeData encounter = GetEncounterById(room.EncounterId);
            return encounter != null && encounter.IsCleared;
        }

        return false;
    }

    private bool IsCurrentRoomType(DungeonRoomType roomType)
    {
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        return room != null && room.RoomType == roomType;
    }

    private void AppendRoomPathLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            return;
        }

        if (_roomPathHistory.Count > 0 && _roomPathHistory[_roomPathHistory.Count - 1] == label)
        {
            return;
        }

        _roomPathHistory.Add(label);
    }

    private string BuildCurrentRoomPathSummary()
    {
        if (_roomPathHistory.Count == 0)
        {
            return "None";
        }

        return string.Join(" -> ", _roomPathHistory.ToArray());
    }

    private void RefreshRoomSequenceState(bool announceChange)
    {
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        string nextStepId = room != null
            ? room.RoomId
            : _exitUnlocked
                ? "exit-route"
                : string.Empty;

        if (_currentRoomStepId == nextStepId)
        {
            return;
        }

        _currentRoomStepId = nextStepId;
        if (!announceChange)
        {
            return;
        }

        string roomLabel = room != null
            ? room.DisplayName
            : _exitUnlocked
                ? "Exit Route"
                : string.Empty;
        if (string.IsNullOrEmpty(roomLabel))
        {
            return;
        }

        AppendRoomPathLabel(roomLabel);
        AppendBattleLog("Entered " + roomLabel + ".");
    }

    private int ApplyPreEliteRecovery()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int recoverAmount = GetCurrentPreEliteRecoverAmount();
        int totalRecovered = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null || member.IsDefeated || member.CurrentHp <= 0)
            {
                continue;
            }

            int before = member.CurrentHp;
            member.CurrentHp = Mathf.Min(member.MaxHp, member.CurrentHp + recoverAmount);
            int restored = member.CurrentHp - before;
            totalRecovered += restored;
            if (restored > 0)
            {
                ShowBattlePopupForPartyMember(i, "+" + restored, new Color(0.45f, 0.92f, 0.58f, 1f));
                FlashPartyMember(i, new Color(0.32f, 0.84f, 0.48f, 1f));
            }
        }

        return totalRecovered;
    }

    private string GetCurrentPreEliteTitleText()
    {
        if (_currentDungeonId == "dungeon-beta")
        {
            return _selectedRouteId == RiskyRouteId ? "War Table" : "Guard Muster";
        }

        return _selectedRouteId == RiskyRouteId ? "Core Threshold" : "Quiet Antechamber";
    }

    private int GetCurrentPreEliteRecoverAmount()
    {
        int amount = GetCurrentShrineRecoverAmount();
        if (_selectedRouteId == SafeRouteId)
        {
            amount += 2;
        }

        if (_currentDungeonId == "dungeon-beta" && _selectedRouteId == SafeRouteId)
        {
            amount += 1;
        }

        return Mathf.Max(1, amount);
    }

    private int GetCurrentEliteBonusRewardAmount()
    {
        int amount = GetCurrentShrineBonusLootAmount();
        if (_selectedRouteId == RiskyRouteId)
        {
            amount += 2;
        }

        if (_currentDungeonId == "dungeon-beta")
        {
            amount += 1;
        }

        return Mathf.Max(2, amount);
    }

    private string GetPreEliteDescriptionText()
    {
        if (_currentDungeonId == "dungeon-beta")
        {
            return _selectedRouteId == RiskyRouteId
                ? "A raider war table offers one last gamble: recover nothing now, or claim a bigger bounty if the chief falls."
                : "A guarded staging room offers one final recovery before the captain, at the cost of a smaller elite payout.";
        }

        return _selectedRouteId == RiskyRouteId
            ? "The chamber hums with unstable energy. Stabilize the party now, or press on for a richer elite reward."
            : "A quiet antechamber lets the party steady itself before the monarch, if they give up the greedier prize.";
    }

    private string GetCurrentPreEliteOptionAText()
    {
        return "Recover Before Elite (+" + GetCurrentPreEliteRecoverAmount() + " HP to each living ally)";
    }

    private string GetCurrentPreEliteOptionBText()
    {
        return "Press On for Bonus Reward (+" + GetCurrentEliteBonusRewardAmount() + " " + DungeonRewardResourceId + " on elite victory)";
    }

    private string GetCurrentEventTitleText()
    {
        if (_currentDungeonId == "dungeon-beta")
        {
            return _selectedRouteId == RiskyRouteId ? "War Banner Shrine" : "Watchfire Shrine";
        }

        return _selectedRouteId == RiskyRouteId ? "Unstable Shrine" : "Rest Shrine";
    }

    private string GetCurrentEventOptionAText()
    {
        string actionLabel = _selectedRouteId == RiskyRouteId ? "Recover at Shrine" : "Rest at Shrine";
        return actionLabel + " (+" + GetCurrentShrineRecoverAmount() + " HP to each living ally)";
    }

    private string GetCurrentEventOptionBText()
    {
        string actionLabel = _selectedRouteId == RiskyRouteId || _currentDungeonId == "dungeon-beta"
            ? "Claim Bonus Loot"
            : "Take Shrine Cache";
        return actionLabel + " (+" + GetCurrentShrineBonusLootAmount() + " " + DungeonRewardResourceId + ")";
    }

    private string GetCurrentRoomTypeText()
    {
        if (!IsDungeonRunActive)
        {
            return "None";
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            return "Planner";
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            return "Run Result";
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return "Preparation Room";
        }

        if (_runTurnCount == 0 && _clearedEncounterCount == 0 && !_eventResolved && _chestOpenedCount == 0 && !_eliteEncounterActive && !_eliteDefeated)
        {
            return "Start Room";
        }

        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        if (room != null)
        {
            return room.RoomTypeLabel;
        }

        return _exitUnlocked ? "Exit Route" : "None";
    }

    private string GetRoomProgressText()
    {
        if (!IsDungeonRunActive)
        {
            return "None";
        }

        int totalRooms = _plannedRooms.Count;
        if (totalRooms <= 0)
        {
            return "0 / 0";
        }

        int currentIndex = GetCurrentPlannedRoomIndex();
        int displayIndex = currentIndex >= 0 ? currentIndex + 1 : totalRooms;
        return displayIndex + " / " + totalRooms;
    }

    private string GetNextMajorGoalText()
    {
        if (!IsDungeonRunActive)
        {
            return "None";
        }

        DungeonRoomTemplateData currentRoom = GetCurrentPlannedRoomStep();
        if (!_preEliteDecisionResolved && currentRoom != null && currentRoom.RoomType == DungeonRoomType.Preparation)
        {
            return "Resolve Preparation";
        }

        return _eliteDefeated ? "Reach Exit" : "Reach Elite";
    }

    private string GetEncounterRoomTypeText()
    {
        DungeonRoomTemplateData room = GetRoomStepByEncounterId(_activeEncounterId);
        return room != null ? room.RoomTypeLabel : GetCurrentRoomTypeText();
    }

    private int GetCityManaShardStock(string cityId)
    {
        return _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? Mathf.Max(0, _runtimeEconomyState.GetStockAmount(cityId, DungeonRewardResourceId))
            : 0;
    }

    private string BuildCityManaShardStockText(string cityId)
    {
        return string.IsNullOrEmpty(cityId)
            ? "None"
            : DungeonRewardResourceId + " x" + GetCityManaShardStock(cityId);
    }

    private string GetNeedPressureTextForStock(int manaShardStock)
    {
        if (manaShardStock <= 1)
        {
            return "Urgent";
        }

        if (manaShardStock <= 4)
        {
            return "Watch";
        }

        return "Stable";
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

        if (!_lastDispatchReadinessChangeByCityId.ContainsKey(cityId))
        {
            _lastDispatchReadinessChangeByCityId[cityId] = "None";
        }

        if (!_dispatchPolicyByCityId.ContainsKey(cityId))
        {
            _dispatchPolicyByCityId[cityId] = DispatchPolicyState.Balanced;
        }
    }

    private DispatchReadinessState GetDispatchReadinessState(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return DispatchReadinessState.Ready;
        }

        EnsureDispatchReadinessEntry(cityId);
        return _dispatchReadinessByCityId[cityId];
    }

    private int GetDispatchRecoveryDaysRemaining(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return 0;
        }

        EnsureDispatchReadinessEntry(cityId);
        return _dispatchRecoveryDaysRemainingByCityId[cityId];
    }

    private int GetDaysSinceLastDispatch(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return -1;
        }

        EnsureDispatchReadinessEntry(cityId);
        return _daysSinceLastDispatchByCityId[cityId];
    }

    private int GetConsecutiveDispatchCount(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return 0;
        }

        EnsureDispatchReadinessEntry(cityId);
        return _consecutiveDispatchCountByCityId[cityId];
    }

    private DispatchPolicyState GetDispatchPolicyState(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return DispatchPolicyState.Balanced;
        }

        EnsureDispatchReadinessEntry(cityId);
        return _dispatchPolicyByCityId[cityId];
    }

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

    private string BuildDispatchReadinessText(DispatchReadinessState readiness)
    {
        return readiness == DispatchReadinessState.Recovering
            ? "Recovering"
            : readiness == DispatchReadinessState.Strained
                ? "Strained"
                : "Ready";
    }

    private string GetDispatchReadinessText(string cityId)
    {
        return string.IsNullOrEmpty(cityId)
            ? "None"
            : BuildDispatchReadinessText(GetDispatchReadinessState(cityId));
    }

    private string BuildDayCountText(int dayCount)
    {
        int safeDayCount = Mathf.Max(0, dayCount);
        return safeDayCount + (safeDayCount == 1 ? " day" : " days");
    }

    private string BuildRecoveryEtaText(int recoveryDaysRemaining)
    {
        return recoveryDaysRemaining > 0 ? BuildDayCountText(recoveryDaysRemaining) : "Ready";
    }

    private int GetRecoveryDaysToReady(string cityId)
    {
        return string.IsNullOrEmpty(cityId) ? 0 : GetDispatchRecoveryDaysRemaining(cityId);
    }

    private string BuildDispatchRecoveryProgressText(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return "None";
        }

        int daysSinceLastDispatch = GetDaysSinceLastDispatch(cityId);
        int recoveryDaysRemaining = GetDispatchRecoveryDaysRemaining(cityId);
        if (daysSinceLastDispatch < 0)
        {
            return "No recent dispatch";
        }

        if (recoveryDaysRemaining > 0)
        {
            return "Ready in " + BuildRecoveryEtaText(recoveryDaysRemaining) + " | Last dispatch " + BuildDayCountText(daysSinceLastDispatch) + " ago";
        }

        return BuildDayCountText(daysSinceLastDispatch) + " since last dispatch";
    }

    private string BuildRecoveryAdviceText(string cityId, string dungeonId)
    {
        if (string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        if (readiness == DispatchReadinessState.Ready)
        {
            return "Ready for dispatch.";
        }

        if (readiness == DispatchReadinessState.Recovering)
        {
            return needPressure == "Urgent"
                ? "Dispatch possible, but the city is recovering. Prefer a balanced route."
                : "Dispatch possible, but the city is recovering. Prefer a safer route.";
        }

        return needPressure == "Urgent"
            ? "High strain. Consider a balanced route instead of the greediest push."
            : "High strain. Consider a safer route or waiting 1 day.";
    }

    private string GetCurrentCityManaShardStockText()
    {
        return string.IsNullOrEmpty(_currentHomeCityId)
            ? "None"
            : BuildCityManaShardStockText(_currentHomeCityId);
    }

    private string GetCurrentNeedPressureText()
    {
        return string.IsNullOrEmpty(_currentHomeCityId)
            ? "None"
            : BuildNeedPressureText(_currentHomeCityId);
    }

    private string GetCurrentDispatchReadinessText()
    {
        return string.IsNullOrEmpty(_currentHomeCityId)
            ? "None"
            : GetDispatchReadinessText(_currentHomeCityId);
    }

    private string GetCurrentDispatchRecoveryProgressText()
    {
        return string.IsNullOrEmpty(_currentHomeCityId)
            ? "None"
            : BuildDispatchRecoveryProgressText(_currentHomeCityId);
    }

    private string GetCurrentDispatchConsecutiveCountText()
    {
        return string.IsNullOrEmpty(_currentHomeCityId)
            ? "0"
            : GetConsecutiveDispatchCount(_currentHomeCityId).ToString();
    }

    private string GetCurrentDispatchPolicyText()
    {
        return string.IsNullOrEmpty(_currentHomeCityId)
            ? "None"
            : BuildDispatchPolicyText(GetDispatchPolicyState(_currentHomeCityId));
    }

    private string GetSelectedDispatchPolicyText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? BuildDispatchPolicyText(GetDispatchPolicyState(_selectedMarker.EntityData.Id))
            : "None";
    }

    private string GetCurrentRecoveryAdviceText()
    {
        return string.IsNullOrEmpty(_currentHomeCityId) || string.IsNullOrEmpty(_currentDungeonId)
            ? "None"
            : BuildRecoveryAdviceText(_currentHomeCityId, _currentDungeonId);
    }

    private string GetBalancedRouteId(string dungeonId)
    {
        return dungeonId == "dungeon-beta" ? SafeRouteId : RiskyRouteId;
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

    private string GetRecommendedRouteId(string cityId, string dungeonId)
    {
        string baseRecommendedRouteId = GetBaseRecommendedRouteId(cityId, dungeonId);
        return ApplyPolicyBiasToRecommendedRoute(cityId, dungeonId, baseRecommendedRouteId);
    }

    private string BuildRecommendedRouteSummaryText(string cityId, string dungeonId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, GetRecommendedRouteId(cityId, dungeonId));
        return template == null
            ? "None"
            : template.RouteLabel + " | " + template.RiskLabel + " Risk";
    }

    private string BuildBaseRecommendationReasonText(string cityId, string dungeonId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        if (needPressure == "Urgent")
        {
            return readiness == DispatchReadinessState.Ready
                ? "Mana Shard stock is low. Recommend the higher reward route."
                : readiness == DispatchReadinessState.Recovering
                    ? "Mana Shard stock is low, but the city is recovering. Recommend a balanced route."
                    : "Mana Shard stock is low, but the city is strained. Recommend a balanced route instead of the greediest push.";
        }

        if (needPressure == "Watch")
        {
            return readiness == DispatchReadinessState.Strained
                ? "Stock needs watching and the city is strained. Recommend a safer route."
                : "Stock needs watching. Recommend a balanced route.";
        }

        return readiness == DispatchReadinessState.Ready
            ? "Stock is stable. Recommend a safer route."
            : readiness == DispatchReadinessState.Recovering
                ? "Stock is stable and the city is recovering. Recommend a safer route."
                : "Stock is stable, but the city is strained. Recommend a safer route or wait 1 day.";
    }

    private string BuildPolicyRecommendationReasonText(string cityId, string dungeonId, string baseRecommendedRouteId, string finalRecommendedRouteId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return "City policy is balanced.";
        }

        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        DispatchPolicyState policy = GetDispatchPolicyState(cityId);
        if (policy == DispatchPolicyState.Safe)
        {
            return needPressure == "Urgent" && readiness == DispatchReadinessState.Ready && finalRecommendedRouteId == baseRecommendedRouteId
                ? "City policy favors safer dispatches, but urgent pressure keeps the base recommendation."
                : "City policy favors safer dispatches.";
        }

        if (policy == DispatchPolicyState.Profit)
        {
            return readiness == DispatchReadinessState.Ready
                ? "City policy favors higher reward dispatches."
                : readiness == DispatchReadinessState.Recovering
                    ? "City policy favors higher reward dispatches, but that bias is reduced because the city is recovering."
                    : "City policy favors higher reward dispatches, but that bias is reduced because the city is strained.";
        }

        return "City policy is balanced.";
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
    }

    private string GetCurrentExpectedNeedImpactText()
    {
        return _expectedNeedImpactText;
    }

    private void RefreshExpectedNeedImpact()
    {
        string routeId = !string.IsNullOrEmpty(_hoverRouteChoiceId)
            ? _hoverRouteChoiceId
            : !string.IsNullOrEmpty(_selectedRouteChoiceId)
                ? _selectedRouteChoiceId
                : !string.IsNullOrEmpty(_selectedRouteId)
                    ? _selectedRouteId
                    : _recommendedRouteId;
        _expectedNeedImpactText = BuildExpectedNeedImpactText(_currentHomeCityId, _currentDungeonId, routeId);
    }

    private string BuildNeedPressureChangeSummary(string beforePressure, string afterPressure)
    {
        string safeBefore = string.IsNullOrEmpty(beforePressure) ? "None" : beforePressure;
        string safeAfter = string.IsNullOrEmpty(afterPressure) ? "None" : afterPressure;
        return safeBefore + " -> " + safeAfter;
    }

    private string BuildDispatchReadinessChangeSummary(string beforeReadiness, string afterReadiness)
    {
        string safeBefore = string.IsNullOrEmpty(beforeReadiness) ? "None" : beforeReadiness;
        string safeAfter = string.IsNullOrEmpty(afterReadiness) ? "None" : afterReadiness;
        return safeBefore + " -> " + safeAfter;
    }

    private string BuildLastDispatchImpactSummary(int stockBefore, int stockAfter, int stockDelta, string beforePressure, string afterPressure)
    {
        return BuildLootAmountText(stockBefore) + " -> " + BuildLootAmountText(stockAfter) + " (" + BuildSignedLootAmountText(stockDelta) + ") | " + BuildNeedPressureChangeSummary(beforePressure, afterPressure);
    }

    private string GetLastDispatchImpactText(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastDispatchImpactByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    private string GetLastDispatchStockDeltaText(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastDispatchStockDeltaByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    private string GetLastNeedPressureChangeText(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastNeedPressureChangeByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    private string GetLastDispatchReadinessChangeText(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastDispatchReadinessChangeByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    private void RefreshDispatchRecommendation()
    {
        _recommendedRouteId = GetRecommendedRouteId(_currentHomeCityId, _currentDungeonId);
        DungeonRouteTemplate template = GetRouteTemplateById(_currentDungeonId, _recommendedRouteId);
        _recommendedRouteLabel = template != null ? template.RouteLabel : "None";
        _recommendedRouteReason = BuildRecommendationReasonText(_currentHomeCityId, _currentDungeonId);
        RefreshExpectedNeedImpact();
    }

    private bool CycleDispatchPolicyForCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return false;
        }

        EnsureDispatchReadinessEntry(cityId);
        _dispatchPolicyByCityId[cityId] = GetNextDispatchPolicyState(GetDispatchPolicyState(cityId));
        if (_dungeonRunState == DungeonRunState.RouteChoice && _currentHomeCityId == cityId)
        {
            string previousRecommendedRouteId = _recommendedRouteId;
            bool shouldFollowRecommendation = string.IsNullOrEmpty(_selectedRouteChoiceId) || _selectedRouteChoiceId == previousRecommendedRouteId;
            RefreshDispatchRecommendation();
            if (shouldFollowRecommendation && !string.IsNullOrEmpty(_recommendedRouteId))
            {
                TryTriggerRouteChoice(_recommendedRouteId);
            }
            else
            {
                _followedRecommendation = !string.IsNullOrEmpty(_selectedRouteChoiceId) && _selectedRouteChoiceId == _recommendedRouteId;
                RefreshSelectionPrompt();
                RefreshDungeonPresentation();
            }

            SetBattleFeedbackText("Dispatch policy set to " + BuildDispatchPolicyText(GetDispatchPolicyState(cityId)) + ".");
        }

        return true;
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

        DispatchReadinessState currentReadiness = GetDispatchReadinessState(cityId);
        if (currentReadiness == DispatchReadinessState.Ready && nextConsecutiveDispatchCount <= 1)
        {
            _dispatchRecoveryDaysRemainingByCityId[cityId] = 1;
            _dispatchReadinessByCityId[cityId] = DispatchReadinessState.Recovering;
            return;
        }

        _dispatchRecoveryDaysRemainingByCityId[cityId] = 2;
        _dispatchReadinessByCityId[cityId] = DispatchReadinessState.Strained;
    }

    private string GetSelectedPreEliteChoiceDisplayText()
    {
        if (!_preEliteDecisionResolved)
        {
            return "Pending";
        }

        if (_selectedPreEliteChoiceId == "recover")
        {
            return _preEliteHealAmount > 0
                ? "Recover Before Elite (+" + _preEliteHealAmount + " HP)"
                : "Recover Before Elite";
        }

        if (_selectedPreEliteChoiceId == "bonus")
        {
            if (_eliteBonusRewardGranted && _eliteBonusRewardGrantedAmount > 0)
            {
                return "Bonus Reward Earned (+" + _eliteBonusRewardGrantedAmount + " " + DungeonRewardResourceId + ")";
            }

            return _eliteBonusRewardPending > 0
                ? "Bonus Reward Pending (+" + _eliteBonusRewardPending + " " + DungeonRewardResourceId + ")"
                : "Bonus Reward";
        }

        return "Pending";
    }

    private string GetPreElitePromptText()
    {
        if (_dungeonRunState != DungeonRunState.PreEliteChoice)
        {
            return "None";
        }

        string hoverLabel = _hoverPreEliteChoiceId == "recover"
            ? "Recover"
            : _hoverPreEliteChoiceId == "bonus"
                ? "Bonus Reward"
                : string.Empty;
        string suffix = string.IsNullOrEmpty(hoverLabel) ? string.Empty : " | Hover: " + hoverLabel;
        return "Make one final choice before " + _eliteName + ": [1] Recover  [2] Bonus Reward or click an option." + suffix;
    }

    private string GetEventDescriptionText()
    {
        if (_currentDungeonId == "dungeon-beta")
        {
            return _selectedRouteId == RiskyRouteId
                ? "A harsh war-banner shrine offers a thinner blessing but a stronger shard lure."
                : "A guarded watchfire shrine steadies the party before the vault skirmishes ahead.";
        }

        return _selectedRouteId == RiskyRouteId
            ? "An unstable shrine trades comfort for greed, pushing the party toward extra shards."
            : "A calm shrine favors recovery and makes the safer routes easier to sustain.";
    }

    private string GetRouteChoiceDescriptionText()
    {
        DungeonRouteTemplate template = GetRouteTemplateById(_currentDungeonId, _hoverRouteChoiceId);
        if (template == null)
        {
            template = GetRouteTemplateById(_currentDungeonId, _selectedRouteChoiceId);
        }

        if (template == null)
        {
            DungeonIdentityTemplate dungeonTemplate = GetCurrentDungeonTemplate();
            return dungeonTemplate == null
                ? "Review the linked dungeon and select a route before dispatching."
                : GetHomeCityDisplayName() + " -> " + dungeonTemplate.DungeonLabel + " | " + dungeonTemplate.DangerLabel + " | " + dungeonTemplate.StyleLabel;
        }

        return template.RouteLabel + " | " + template.Description + " | " + template.EncounterPreview + " | " + template.RewardPreview;
    }

    private string GetRouteChoicePromptText()
    {
        string option1 = BuildRouteButtonLabel(_currentDungeonId, SafeRouteId);
        string option2 = BuildRouteButtonLabel(_currentDungeonId, RiskyRouteId);
        string selectedRouteLabel = string.IsNullOrEmpty(_selectedRouteChoiceId)
            ? "None"
            : BuildRouteButtonLabel(_currentDungeonId, _selectedRouteChoiceId);
        string hoverRouteLabel = string.IsNullOrEmpty(_hoverRouteChoiceId)
            ? string.Empty
            : BuildRouteButtonLabel(_currentDungeonId, _hoverRouteChoiceId);
        string hoverSuffix = string.IsNullOrEmpty(hoverRouteLabel) ? string.Empty : " | Hover: " + hoverRouteLabel;
        string dispatchHint = CanConfirmRouteChoice() ? "[Enter] Dispatch" : "Select a route, then [Enter] Dispatch";
        string policyText = BuildDispatchPolicyText(GetDispatchPolicyState(_currentHomeCityId));
        return dispatchHint + " | [Q] Policy: " + policyText + " | [1] " + option1 + "  [2] " + option2 + "  [Esc] Return | Recommended: " + _recommendedRouteLabel + " | Selected: " + selectedRouteLabel + hoverSuffix;
    }

    private string BuildRouteButtonLabel(string routeId)
    {
        return BuildRouteButtonLabel(_currentDungeonId, routeId);
    }

    private string BuildRouteButtonLabel(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        return template != null ? template.RouteLabel : string.Empty;
    }

    private string NormalizeRouteChoiceId(string optionKey)
    {
        if (string.IsNullOrEmpty(optionKey))
        {
            return string.Empty;
        }

        string normalized = optionKey.Trim().ToLowerInvariant();
        if (normalized == SafeRouteId || normalized == RiskyRouteId)
        {
            return normalized;
        }

        return string.Empty;
    }
    private string GetDungeonRunStateText()
    {
        if (_dungeonRunState == DungeonRunState.None)
        {
            return "None";
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            return "Dispatch Planner";
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return "Pre-Elite Decision";
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel && _runResultState != RunResultState.None)
        {
            return _runResultState.ToString();
        }

        return _dungeonRunState.ToString();
    }

    private string GetCurrentRoomText()
    {
        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            return "Dispatch Planner";
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            return "Run Result";
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return "Preparation Room";
        }

        if (_runTurnCount == 0 && _clearedEncounterCount == 0 && !_eventResolved && _chestOpenedCount == 0 && !_eliteEncounterActive && !_eliteDefeated)
        {
            return "Start Room";
        }

        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        if (room != null)
        {
            return room.DisplayName;
        }

        if (_exitUnlocked)
        {
            return "Exit Route";
        }

        return _currentRoomIndex == 1
            ? "Room 1"
            : _currentRoomIndex == 2
                ? "Room 2"
                : "Corridor";
    }

    private string GetCurrentBattleActorText()
    {
        if (_dungeonRunState == DungeonRunState.Battle && _battleState == BattleState.EnemyTurn && _activeBattleMonster != null)
        {
            return _activeBattleMonster.DisplayName + " | " + _activeBattleMonster.MonsterType;
        }

        if (_activeDungeonParty == null || _currentActorIndex < 0 || _currentActorIndex >= _activeDungeonParty.Members.Length)
        {
            return "None";
        }

        DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[_currentActorIndex];
        return BuildPartyMemberRuntimeSummary(member, string.Empty, true);
    }


    private string GetCurrentEncounterNameText()
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        return encounter != null
            ? encounter.DisplayName
            : _dungeonRunState == DungeonRunState.Battle
                ? _currentDungeonName
                : "None";
    }

    private string GetEliteStatusText()
    {
        if (string.IsNullOrEmpty(_eliteEncounterId))
        {
            return "None";
        }

        return _eliteDefeated
            ? "Defeated"
            : _eliteEncounterActive
                ? "Active"
                : "Pending";
    }

    private string GetEliteEncounterNameText()
    {
        if (!string.IsNullOrEmpty(_eliteName) && _eliteName != "None")
        {
            return _eliteName;
        }

        DungeonEncounterRuntimeData eliteEncounter = GetEliteEncounter();
        return eliteEncounter != null ? eliteEncounter.DisplayName : "None";
    }

    private string GetEliteHpText()
    {
        DungeonMonsterRuntimeData eliteMonster = GetEliteMonster();
        if (eliteMonster != null)
        {
            return eliteMonster.CurrentHp + " / " + eliteMonster.MaxHp;
        }

        return _eliteDefeated ? "0 / 0" : "None";
    }

    private string GetEliteRewardStatusText()
    {
        if (string.IsNullOrEmpty(_eliteRewardLabel) || _eliteRewardLabel == "None")
        {
            return "None";
        }


        if (!_eliteEncounterActive && !_eliteDefeated && _eliteBonusRewardPending <= 0 && _dungeonRunState != DungeonRunState.PreEliteChoice)
        {
            return "None";
        }
        return _eliteRewardGranted ? "Granted" : "Pending";
    }

    private string GetEliteRewardHintText()
    {
        if (string.IsNullOrEmpty(_eliteRewardLabel) || _eliteRewardLabel == "None")
        {
            return "None";
        }

        if (!_eliteEncounterActive && !_eliteDefeated && _eliteBonusRewardPending <= 0 && _dungeonRunState != DungeonRunState.PreEliteChoice)
        {
            return "None";
        }

        string baseReward = _eliteRewardLabel + " (" + BuildLootAmountText(_eliteRewardAmount) + ")";
        if (_eliteBonusRewardGranted && _eliteBonusRewardGrantedAmount > 0)
        {
            return baseReward + " + bonus granted " + BuildLootAmountText(_eliteBonusRewardGrantedAmount);
        }

        if (_eliteBonusRewardPending > 0)
        {
            return baseReward + " + bonus pending " + BuildLootAmountText(_eliteBonusRewardPending);
        }

        return baseReward;
    }

    private bool IsPartyWideEliteSpecial(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        return useSpecial && monster != null && monster.IsElite && monster.MonsterType == "Slime";
    }

    private int GetCurrentTotalPartyHp()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null)
            {
                total += Mathf.Max(0, member.CurrentHp);
            }
        }

        return total;
    }

    private int GetMaxTotalPartyHp()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null)
            {
                total += Mathf.Max(1, member.MaxHp);
            }
        }

        return total;
    }

    private int GetInjuredMemberCount()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && member.CurrentHp < member.MaxHp)
            {
                count += 1;
            }
        }

        return count;
    }

    private float GetPartyHealthRatio()
    {
        int maxTotal = GetMaxTotalPartyHp();
        return maxTotal > 0 ? (float)GetCurrentTotalPartyHp() / maxTotal : 0f;
    }

    private string BuildTotalPartyHpSummary()
    {
        int maxTotal = GetMaxTotalPartyHp();
        if (maxTotal <= 0)
        {
            return "None";
        }

        return GetCurrentTotalPartyHp() + " / " + maxTotal + " (" + GetInjuredMemberCount() + " injured)";
    }

    private string GetPartyConditionText()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return "None";
        }

        float ratio = GetPartyHealthRatio();
        if (ratio >= 0.68f)
        {
            return "Stable";
        }

        if (ratio >= 0.36f)
        {
            return "Worn";
        }

        return "Critical";
    }

    private string GetSustainPressureText()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return "None";
        }

        float ratio = GetPartyHealthRatio();
        if (ratio >= 0.68f)
        {
            return "Low";
        }

        if (ratio >= 0.36f)
        {
            return "Medium";
        }

        return "High";
    }

    private string GetMonsterRoleText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return "None";
        }

        if (monster.IsElite)
        {
            return "Elite";
        }

        switch (monster.EncounterRole)
        {
            case MonsterEncounterRole.Skirmisher:
                return "Skirmisher";
            case MonsterEncounterRole.Striker:
                return "Striker";
            default:
                return "Bulwark";
        }
    }

    private string GetRecentBattleLogText(int index)
    {
        return index >= 0 && index < _recentBattleLogs.Count
            ? _recentBattleLogs[index]
            : "None";
    }

    private string GetBattleMonsterSummaryText(int displayIndex)
    {
        DungeonMonsterRuntimeData monster = GetBattleMonsterAtDisplayIndex(displayIndex);
        if (monster == null)
        {
            return "None";
        }

        bool isSelected = !monster.IsDefeated && _battleState == BattleState.PartyTargetSelect && _activeBattleMonsterId == monster.MonsterId;
        bool isHovered = !monster.IsDefeated && _battleState == BattleState.PartyTargetSelect && _hoverBattleMonsterId == monster.MonsterId;
        bool isActing = !monster.IsDefeated && _battleState == BattleState.EnemyTurn && _activeBattleMonsterId == monster.MonsterId;
        string stateText = monster.IsDefeated ? "Defeated" : isActing ? "Acting" : isSelected ? "Target" : isHovered ? "Hover" : "Alive";
        return monster.DisplayName + " | " + monster.MonsterType + " | " + GetMonsterRoleText(monster) + " | " + monster.CurrentHp + " / " + monster.MaxHp + " | " + stateText;
    }

    private string GetPartyMemberDisplayName(int memberIndex)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || memberIndex < 0 || memberIndex >= _activeDungeonParty.Members.Length)
        {
            return "None";
        }

        DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[memberIndex];
        return member != null ? member.DisplayName : "None";
    }

    private DungeonPartyMemberRuntimeData GetCurrentActorMember()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || _currentActorIndex < 0 || _currentActorIndex >= _activeDungeonParty.Members.Length)
        {
            return null;
        }

        return _activeDungeonParty.Members[_currentActorIndex];
    }

    private int GetFirstLivingPartyMemberIndex()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return -1;
        }

        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && !member.IsDefeated && member.CurrentHp > 0)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetRandomLivingPartyMemberIndex()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return -1;
        }

        int[] livingIndices = new int[_activeDungeonParty.Members.Length];
        int livingCount = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && !member.IsDefeated && member.CurrentHp > 0)
            {
                livingIndices[livingCount] = i;
                livingCount += 1;
            }
        }

        return livingCount > 0 ? livingIndices[Random.Range(0, livingCount)] : -1;
    }

    private int GetLowestHpLivingPartyMemberIndex()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return -1;
        }

        int bestIndex = -1;
        int lowestHp = int.MaxValue;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null || member.IsDefeated || member.CurrentHp <= 0)
            {
                continue;
            }

            if (member.CurrentHp < lowestHp)
            {
                lowestHp = member.CurrentHp;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private int GetMonsterTargetPartyMemberIndex(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return GetFirstLivingPartyMemberIndex();
        }

        return monster.TargetPattern == MonsterTargetPattern.RandomLiving
            ? GetRandomLivingPartyMemberIndex()
            : monster.TargetPattern == MonsterTargetPattern.LowestHpLiving
                ? GetLowestHpLivingPartyMemberIndex()
                : GetFirstLivingPartyMemberIndex();
    }

        private bool ShouldUseEliteSpecialAttack(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return false;
        }

        if (monster.IsElite)
        {
            return monster.TurnsActed > 0 && monster.TurnsActed % 2 == 1;
        }

        return monster.EncounterRole == MonsterEncounterRole.Striker && monster.TurnsActed % 2 == 0;
    }

    private int GetEnemyActionPower(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return 1;
        }

        return useSpecial ? Mathf.Max(1, monster.SpecialAttack) : Mathf.Max(1, monster.Attack);
    }

    private string GetEnemyActionLabel(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return "Attack";
        }

        return useSpecial ? monster.SpecialActionName : "Attack";
    }

        private string BuildEnemyIntentText(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        if (monster == null)
        {
            return "Enemy intent.";
        }

        string targetName = GetPartyMemberDisplayName(targetIndex);
        string actionLabel = GetEnemyActionLabel(monster, useSpecial);
        string roleLabel = GetMonsterRoleText(monster);
        if (monster.IsElite)
        {
            if (IsPartyWideEliteSpecial(monster, useSpecial))
            {
                return monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on all living allies.";
            }

            return useSpecial
                ? monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ". Focused execution incoming."
                : monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ".";
        }

        if (monster.EncounterRole == MonsterEncounterRole.Striker)
        {
            return monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ".";
        }

        return monster.TargetPattern == MonsterTargetPattern.LowestHpLiving
            ? monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on lowest HP target: " + targetName + "."
            : monster.TargetPattern == MonsterTargetPattern.RandomLiving
                ? monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on random target: " + targetName + "."
                : monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ".";
    }

    private DungeonEncounterRuntimeData GetActiveEncounter()
    {
        return GetEncounterById(_activeEncounterId);
    }

    private DungeonEncounterRuntimeData GetEncounterById(string encounterId)
    {
        if (string.IsNullOrEmpty(encounterId))
        {
            return null;
        }

        for (int i = 0; i < _activeEncounters.Count; i++)
        {
            DungeonEncounterRuntimeData encounter = _activeEncounters[i];
            if (encounter != null && encounter.EncounterId == encounterId)
            {
                return encounter;
            }
        }

        return null;
    }

    private DungeonEncounterRuntimeData GetEliteEncounter()
    {
        return GetEncounterById(_eliteEncounterId);
    }

    private DungeonMonsterRuntimeData GetEliteMonster()
    {
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null && monster.IsElite)
            {
                return monster;
            }
        }

        return null;
    }

    private bool IsEliteBattleActive()
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        return encounter != null && encounter.IsEliteEncounter;
    }

    private DungeonMonsterRuntimeData GetMonsterById(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            return null;
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null && monster.MonsterId == monsterId)
            {
                return monster;
            }
        }

        return null;
    }

    private DungeonMonsterRuntimeData GetBattleMonsterAtDisplayIndex(int displayIndex)
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null || displayIndex < 0 || displayIndex >= encounter.MonsterIds.Length)
        {
            return null;
        }

        return GetMonsterById(encounter.MonsterIds[displayIndex]);
    }

    private DungeonMonsterRuntimeData GetFirstLivingBattleMonster()
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null)
        {
            return null;
        }

        for (int i = 0; i < encounter.MonsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
            if (monster != null && !monster.IsDefeated && monster.CurrentHp > 0)
            {
                return monster;
            }
        }

        return null;
    }

    private int GetLivingBattleMonsterCount()
    {
        int count = 0;
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null)
        {
            return 0;
        }

        for (int i = 0; i < encounter.MonsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
            if (monster != null && !monster.IsDefeated && monster.CurrentHp > 0)
            {
                count += 1;
            }
        }

        return count;
    }

    private int CalculateActiveEncounterLoot()
    {
        int total = 0;
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null)
        {
            return 0;
        }

        for (int i = 0; i < encounter.MonsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
            if (monster != null)
            {
                total += monster.RewardAmount;
            }
        }

        return total;
    }

    private bool IsEncounterTriggered(DungeonEncounterRuntimeData encounter)
    {
        if (encounter == null || encounter.MonsterIds == null)
        {
            return false;
        }

        for (int i = 0; i < encounter.MonsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
            if (monster == null || monster.IsDefeated)
            {
                continue;
            }

            int manhattanDistance = Mathf.Abs(monster.GridPosition.x - _playerGridPosition.x) + Mathf.Abs(monster.GridPosition.y - _playerGridPosition.y);
            if (manhattanDistance <= 1)
            {
                return true;
            }
        }

        return false;
    }

    private void SetActiveBattleMonster(DungeonMonsterRuntimeData monster)
    {
        _activeBattleMonster = monster;
        _activeBattleMonsterId = monster != null ? monster.MonsterId : string.Empty;
    }

    private BattleActionType ParseBattleActionType(string actionKey)
    {
        if (string.IsNullOrWhiteSpace(actionKey))
        {
            return BattleActionType.None;
        }

        string normalized = actionKey.Trim().ToLowerInvariant();
        if (normalized == "attack")
        {
            return BattleActionType.Attack;
        }

        if (normalized == "skill")
        {
            return BattleActionType.Skill;
        }

        if (normalized == "retreat")
        {
            return BattleActionType.Retreat;
        }

        return BattleActionType.None;
    }

    private bool IsBattleActionAvailable(BattleActionType action)
    {
        if (_dungeonRunState != DungeonRunState.Battle || IsBattleInputLocked())
        {
            return false;
        }

        if (action == BattleActionType.Attack || action == BattleActionType.Skill)
        {
            return _battleState == BattleState.PartyActionSelect && GetCurrentActorMember() != null;
        }

        if (action == BattleActionType.Retreat)
        {
            return _battleState == BattleState.PartyActionSelect || _battleState == BattleState.PartyTargetSelect;
        }

        return false;
    }

    private bool SetHoveredBattleAction(BattleActionType action)
    {
        BattleActionType nextAction = action == BattleActionType.None || !IsBattleActionAvailable(action)
            ? BattleActionType.None
            : action;
        if (_hoverBattleAction == nextAction)
        {
            return false;
        }

        _hoverBattleAction = nextAction;
        RefreshSelectionPrompt();
        return true;
    }

    private bool SetHoveredBattleMonster(DungeonMonsterRuntimeData monster)
    {
        string monsterId = monster != null && !monster.IsDefeated ? monster.MonsterId : string.Empty;
        if (_hoverBattleMonsterId == monsterId)
        {
            return false;
        }

        _hoverBattleMonsterId = monsterId;
        return true;
    }

    private void ClearBattleHoverState()
    {
        _hoverBattleAction = BattleActionType.None;
        _hoverBattleMonsterId = string.Empty;
    }

    private void UpdateBattleMouseInteraction(Mouse mouse, Camera worldCamera)
    {
        if (_dungeonRunState != DungeonRunState.Battle || mouse == null || worldCamera == null || IsBattleInputLocked())
        {
            if (SetHoveredBattleMonster(null))
            {
                RefreshSelectionPrompt();
                RefreshDungeonPresentation();
            }

            return;
        }

        if (_battleState != BattleState.PartyTargetSelect)
        {
            if (SetHoveredBattleMonster(null))
            {
                RefreshSelectionPrompt();
                RefreshDungeonPresentation();
            }

            return;
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            CancelTargetSelection();
            return;
        }

        Vector2 screenPoint = mouse.position.ReadValue();
        float depth = Mathf.Abs(worldCamera.transform.position.z);
        Vector3 worldPoint3 = worldCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
        Vector2 worldPoint = new Vector2(worldPoint3.x, worldPoint3.y);

        DungeonMonsterRuntimeData hoveredMonster = null;
        for (int i = 0; i < _battleMonsterPlateRenderers.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetBattleMonsterAtDisplayIndex(i);
            SpriteRenderer plateRenderer = _battleMonsterPlateRenderers[i];
            if (monster == null || monster.IsDefeated || plateRenderer == null || !plateRenderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            Bounds bounds = plateRenderer.bounds;
            const float hitPadding = 0.22f;
            if (worldPoint.x >= bounds.min.x - hitPadding &&
                worldPoint.x <= bounds.max.x + hitPadding &&
                worldPoint.y >= bounds.min.y - hitPadding &&
                worldPoint.y <= bounds.max.y + hitPadding)
            {
                hoveredMonster = monster;
                break;
            }
        }

        if (SetHoveredBattleMonster(hoveredMonster))
        {
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
        }

        if (hoveredMonster != null && mouse.leftButton.wasPressedThisFrame)
        {
            TryResolveTargetSelection(hoveredMonster);
        }
    }

    private string GetBattleActionDisplayName(BattleActionType action, DungeonPartyMemberRuntimeData member)
    {
        if (action == BattleActionType.Attack)
        {
            return "Attack";
        }

        if (action == BattleActionType.Skill)
        {
            return member != null ? member.SkillName : "Skill";
        }

        if (action == BattleActionType.Retreat)
        {
            return "Retreat";
        }

        return string.Empty;
    }

    private string GetHoveredBattleMonsterText()
    {
        if (string.IsNullOrEmpty(_hoverBattleMonsterId))
        {
            return string.Empty;
        }

        DungeonMonsterRuntimeData monster = GetMonsterById(_hoverBattleMonsterId);
        return monster != null && !monster.IsDefeated ? monster.DisplayName : string.Empty;
    }

    private bool IsEventAvailable()
    {
        return IsDungeonRunActive && !_eventResolved && IsCurrentRoomType(DungeonRoomType.Shrine);
    }

    private string GetEventStatusText()
    {
        if (!IsDungeonRunActive)
        {
            return "None";
        }

        if (_eventResolved)
        {
            return "Resolved";
        }

        return IsEventAvailable() ? "Available" : "Not Found";
    }

    private string GetSelectedEventChoiceDisplayText()
    {
        if (string.IsNullOrEmpty(_selectedEventChoiceId))
        {
            return "None";
        }

        if (_selectedEventChoiceId == "recover")
        {
            return _totalEventHealAmount > 0
                ? "Recover at " + GetCurrentEventTitleText() + " (+" + _totalEventHealAmount + " HP)"
                : "Recover";
        }

        if (_selectedEventChoiceId == "loot")
        {
            return "Bonus Loot from " + GetCurrentEventTitleText() + " (+" + _eventLootAmount + " " + DungeonRewardResourceId + ")";
        }

        return _selectedEventChoiceId;
    }

    private string GetEventPromptText()
    {
        if (_dungeonRunState != DungeonRunState.EventChoice)
        {
            return "None";
        }

        string hoverLabel = _hoverEventChoiceId == "recover"
            ? "Recover Party"
            : _hoverEventChoiceId == "loot"
                ? "Claim Bonus Loot"
                : string.Empty;
        string suffix = string.IsNullOrEmpty(hoverLabel) ? string.Empty : " | Hover: " + hoverLabel;
        return "Choose one blessing in " + GetCurrentEventTitleText() + ". [1] Recover  [2] Bonus Loot or click an option." + suffix;
    }

    private string BuildLootBreakdownSummary()
    {
        if (!IsDungeonRunActive && _runResultState == RunResultState.None)
        {
            return "None";
        }

        int totalLoot = _battleLootAmount + _chestLootAmount + _eventLootAmount;
        return "Battle " + _battleLootAmount + " / Chest " + _chestLootAmount + " / Event " + _eventLootAmount + " / Total " + totalLoot;
    }

    private string BuildLootAmountText(int amount)
    {
        return DungeonRewardResourceId + " x" + Mathf.Max(0, amount);
    }

    private string BuildRawHpAmountText(int amount)
    {
        return "HP +" + Mathf.Max(0, amount);
    }

    private string BuildSignedLootAmountText(int amount)
    {
        string sign = amount < 0 ? "-" : "+";
        return DungeonRewardResourceId + " x" + sign + Mathf.Abs(amount);
    }

    private string NormalizeEventChoiceId(string optionKey)
    {
        if (string.IsNullOrEmpty(optionKey))
        {
            return string.Empty;
        }

        string normalized = optionKey.Trim().ToLowerInvariant();
        if (normalized == "recover" || normalized == "loot")
        {
            return normalized;
        }

        return string.Empty;
    }

    private string NormalizePreEliteChoiceId(string optionKey)
    {
        if (string.IsNullOrEmpty(optionKey))
        {
            return string.Empty;
        }

        string normalized = optionKey.Trim().ToLowerInvariant();
        if (normalized == "recover" || normalized == "bonus")
        {
            return normalized;
        }

        return string.Empty;
    }

    private void RefreshSelectionPrompt()
    {
        if (!IsDungeonRunActive)
        {
            _currentSelectionPrompt = "None";
            return;
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            _currentSelectionPrompt = "[Enter] Return to World";
            return;
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            ClearBattleHoverState();
            _currentSelectionPrompt = GetRouteChoicePromptText();
            return;
        }

        if (_dungeonRunState == DungeonRunState.EventChoice)
        {
            _currentSelectionPrompt = GetEventPromptText();
            return;
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            _currentSelectionPrompt = GetPreElitePromptText();
            return;
        }

        if (_dungeonRunState == DungeonRunState.Explore)
        {
            if (_exitUnlocked)
            {
                _currentSelectionPrompt = "Exit unlocked. Reach the gold tile or press [Q] to retreat.";
                return;
            }

            DungeonRoomTemplateData currentRoom = GetCurrentPlannedRoomStep();
            if (currentRoom != null)
            {
                if (currentRoom.RoomType == DungeonRoomType.Shrine)
                {
                    _currentSelectionPrompt = "Enter " + currentRoom.DisplayName + " and choose one blessing.";
                    return;
                }

                if (currentRoom.RoomType == DungeonRoomType.Cache)
                {
                    _currentSelectionPrompt = "Enter " + currentRoom.DisplayName + " and open the cache.";
                    return;
                }

                if (currentRoom.RoomType == DungeonRoomType.Elite)
                {
                    _currentSelectionPrompt = "The final elite waits ahead. Defeat " + _eliteName + ".";
                    return;
                }

                _currentSelectionPrompt = "Enter " + currentRoom.DisplayName + " and clear the encounter.";
                return;
            }

            _currentSelectionPrompt = "Explore the rooms, clear encounters, and search for the next route marker.";
            return;
        }

        if (_battleState == BattleState.EnemyTurn)
        {
            _currentSelectionPrompt = !string.IsNullOrEmpty(_enemyIntentText) && _enemyIntentText != "None"
                ? _enemyIntentText
                : "Enemy turn. Wait for the monsters to act.";
            return;
        }

        if (IsBattleInputLocked())
        {
            _currentSelectionPrompt = "Resolving action...";
            return;
        }

        if (_battleState == BattleState.PartyActionSelect)
        {
            DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
            if (member == null)
            {
                _currentSelectionPrompt = "Waiting for next actor.";
            }
            else
            {
                string hoverActionName = GetBattleActionDisplayName(_hoverBattleAction, member);
                string skillHintText = string.IsNullOrEmpty(member.SkillShortText) ? string.Empty : " | " + member.SkillShortText;
                _currentSelectionPrompt = string.IsNullOrEmpty(hoverActionName)
                    ? "Select action for " + member.DisplayName + " (" + member.RoleLabel + "). [1] Attack  [2] " + member.SkillName + "  [3] Retreat" + skillHintText
                    : _hoverBattleAction == BattleActionType.Skill
                        ? "Select " + hoverActionName + " for " + member.DisplayName + ". " + (string.IsNullOrEmpty(member.SkillShortText) ? "Click or matching key." : member.SkillShortText + " Click or matching key.")
                        : "Select " + hoverActionName + " for " + member.DisplayName + " by click or matching key.";
            }

            return;
        }

        if (_battleState == BattleState.PartyTargetSelect)
        {
            _currentSelectionPrompt = BuildTargetSelectionPrompt();
            return;
        }

        _currentSelectionPrompt = "None";
    }

    private string BuildTargetSelectionPrompt()
    {
        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        string actionName = _queuedBattleAction == BattleActionType.Skill && member != null ? member.SkillName : "Attack";
        string hoveredMonsterText = GetHoveredBattleMonsterText();
        string focusText = !string.IsNullOrEmpty(hoveredMonsterText)
            ? " | Hover: " + hoveredMonsterText
            : _activeBattleMonster != null && !_activeBattleMonster.IsDefeated
                ? " | Target: " + _activeBattleMonster.DisplayName
                : string.Empty;
        string skillHintText = _queuedBattleAction == BattleActionType.Skill && member != null && !string.IsNullOrEmpty(member.SkillShortText)
            ? " | " + member.SkillShortText
            : string.Empty;
        return "Select target for " + actionName + ": [1] " + BuildTargetOptionText(0) + "  [2] " + BuildTargetOptionText(1) + " | Click enemy | [Esc]/Right Click Cancel" + focusText + skillHintText;
    }

    private string BuildTargetOptionText(int displayIndex)
    {
        DungeonMonsterRuntimeData monster = GetBattleMonsterAtDisplayIndex(displayIndex);
        if (monster == null)
        {
            return "None";
        }

        string typeText = " [" + monster.MonsterType + "]";
        return monster.DisplayName + typeText + (monster.IsDefeated ? " (Defeated)" : " (HP " + monster.CurrentHp + "/" + monster.MaxHp + ")");
    }

    private string GetBattlePhaseText()
    {
        if (_dungeonRunState == DungeonRunState.None)
        {
            return "None";
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            return "Dispatch Planner";
        }

        if (_dungeonRunState == DungeonRunState.Explore)
        {
            return "Explore";
        }

        if (_dungeonRunState == DungeonRunState.EventChoice)
        {
            return "Event Choice";
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return "Pre-Elite Choice";
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            return _runResultState == RunResultState.None ? "Result" : _runResultState.ToString();
        }

        if (IsBattleInputLocked())
        {
            return _battleState == BattleState.EnemyTurn ? "Enemy Turn" : "Resolving";
        }

        return _battleState == BattleState.PartyActionSelect
            ? "Player Turn"
            : _battleState == BattleState.PartyTargetSelect
                ? "Target Select"
                : _battleState == BattleState.EnemyTurn
                    ? "Enemy Turn"
                    : _battleState == BattleState.Victory
                        ? "Victory"
                        : _battleState == BattleState.Defeat
                            ? "Defeat"
                            : _battleState == BattleState.Retreat
                                ? "Retreat"
                                : "Battle";
    }

    private string GetBattleCancelHintText()
    {
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            return "None";
        }

        if (_battleState == BattleState.EnemyTurn)
        {
            return _enemyIntentTelegraphActive ? "Enemy intent is telegraphed. Input locked." : "Enemy actions are resolving.";
        }

        if (IsBattleInputLocked())
        {
            return "Wait for the action to finish.";
        }

        if (_battleState == BattleState.PartyTargetSelect)
        {
            return "[Esc] or Right Click cancels target select.";
        }

        if (_battleState == BattleState.PartyActionSelect)
        {
            return "[1][2][3] or click actions.";
        }

        return "None";
    }

    private bool IsBattleInputLocked()
    {
        return _dungeonRunState == DungeonRunState.Battle && Time.unscaledTime < _battleInputLockUntilTime;
    }

    private void LockBattleInput(float duration = BattleInputLockSeconds)
    {
        _battleInputLockUntilTime = Mathf.Max(_battleInputLockUntilTime, Time.unscaledTime + Mathf.Max(0.01f, duration));
        RefreshSelectionPrompt();
    }

    private void ClearBattleInputLock()
    {
        _battleInputLockUntilTime = 0f;
        _wasBattleInputLockedLastFrame = false;
    }

    private void SetBattleFeedbackText(string text)
    {
        _battleFeedbackText = string.IsNullOrEmpty(text) ? "None" : text;
    }

    private void ClearEnemyIntentState()
    {
        _enemyIntentText = "None";
        _enemyTurnMonsterCursor = -1;
        _pendingEnemyTargetIndex = -1;
        _pendingEnemyAttackPower = 0;
        _pendingEnemyActionLabel = string.Empty;
        _pendingEnemyUsedSpecialAttack = false;
        _enemyIntentResolveAtTime = 0f;
        _enemyIntentTelegraphActive = false;
    }

    private void CancelTargetSelection()
    {
        if (_dungeonRunState != DungeonRunState.Battle || _battleState != BattleState.PartyTargetSelect || IsBattleInputLocked())
        {
            return;
        }

        ClearBattleHoverState();
        _queuedBattleAction = BattleActionType.None;
        _battleState = BattleState.PartyActionSelect;
        SetActiveBattleMonster(GetFirstLivingBattleMonster());
        SetBattleFeedbackText("Target selection canceled.");
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private void UpdateBattleTransientState()
    {
        UpdateBattleFloatingPopups();

        if (_dungeonRunState == DungeonRunState.Battle &&
            _battleState == BattleState.EnemyTurn &&
            _enemyIntentTelegraphActive &&
            Time.unscaledTime >= _enemyIntentResolveAtTime)
        {
            ExecuteQueuedEnemyIntent();
        }

        bool isLocked = IsBattleInputLocked();
        if (_wasBattleInputLockedLastFrame != isLocked)
        {
            _wasBattleInputLockedLastFrame = isLocked;
            if (_dungeonRunState == DungeonRunState.Battle)
            {
                RefreshSelectionPrompt();
            }
        }

        if (_dungeonRunState == DungeonRunState.Battle)
        {
            RefreshBattleViewPresentation();
            return;
        }

        ClearBattleFloatingPopups();
    }

    private void UpdateBattleFloatingPopups()
    {
        float now = Time.unscaledTime;
        for (int i = 0; i < _battleFloatingPopups.Length; i++)
        {
            BattleFloatingPopup popup = _battleFloatingPopups[i];
            if (popup == null || popup.Root == null || popup.TextMesh == null)
            {
                continue;
            }

            if (_battlePopupEndTimes[i] <= now || _dungeonRunState != DungeonRunState.Battle)
            {
                if (popup.Root.activeSelf)
                {
                    popup.Root.SetActive(false);
                }

                continue;
            }

            float duration = Mathf.Max(0.01f, _battlePopupEndTimes[i] - _battlePopupStartTimes[i]);
            float progress = Mathf.Clamp01((now - _battlePopupStartTimes[i]) / duration);
            popup.Root.SetActive(true);
            popup.Root.transform.localPosition = _battlePopupBasePositions[i] + new Vector3(0f, progress * BattleFloatingPopupRiseDistance, 0f);
            Color popupColor = _battlePopupColors[i];
            popupColor.a = Mathf.Lerp(1f, 0f, progress);
            popup.TextMesh.color = popupColor;
        }
    }

    private void ClearBattleFloatingPopups()
    {
        for (int i = 0; i < _battleFloatingPopups.Length; i++)
        {
            BattleFloatingPopup popup = _battleFloatingPopups[i];
            if (popup == null || popup.Root == null)
            {
                continue;
            }

            popup.Root.SetActive(false);
            if (popup.TextMesh != null)
            {
                popup.TextMesh.text = string.Empty;
            }

            _battlePopupStartTimes[i] = 0f;
            _battlePopupEndTimes[i] = 0f;
            _battlePopupBasePositions[i] = Vector3.zero;
        }
    }

    private int GetNextBattlePopupIndex()
    {
        float now = Time.unscaledTime;
        int bestIndex = 0;
        float earliestEnd = float.MaxValue;
        for (int i = 0; i < _battleFloatingPopups.Length; i++)
        {
            if (_battleFloatingPopups[i] == null)
            {
                continue;
            }

            if (_battlePopupEndTimes[i] <= now)
            {
                return i;
            }

            if (_battlePopupEndTimes[i] < earliestEnd)
            {
                earliestEnd = _battlePopupEndTimes[i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void ShowBattlePopup(Vector3 localPosition, string text, Color color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        EnsureDungeonVisuals();
        int popupIndex = GetNextBattlePopupIndex();
        BattleFloatingPopup popup = _battleFloatingPopups[popupIndex];
        if (popup == null || popup.Root == null || popup.TextMesh == null)
        {
            return;
        }

        _battlePopupBasePositions[popupIndex] = localPosition;
        _battlePopupColors[popupIndex] = color;
        _battlePopupStartTimes[popupIndex] = Time.unscaledTime;
        _battlePopupEndTimes[popupIndex] = Time.unscaledTime + BattleFloatingPopupDuration;
        popup.TextMesh.text = text;
        popup.TextMesh.color = color;
        popup.Root.transform.localPosition = localPosition;
        popup.Root.SetActive(_dungeonRunState == DungeonRunState.Battle);
    }

    private void ShowBattlePopupForMonster(DungeonMonsterRuntimeData monster, string text, Color color, float yOffset = 0.95f)
    {
        int displayIndex = GetBattleDisplayIndexForMonster(monster);
        if (displayIndex < 0)
        {
            return;
        }

        ShowBattlePopup(BattleMonsterViewPositions[displayIndex] + new Vector3(0f, yOffset, 0f), text, color);
    }

    private void ShowBattlePopupForPartyMember(int memberIndex, string text, Color color, float yOffset = 0.75f)
    {
        if (memberIndex < 0 || memberIndex >= BattlePartyViewPositions.Length)
        {
            return;
        }

        ShowBattlePopup(BattlePartyViewPositions[memberIndex] + new Vector3(0.05f, yOffset, 0f), text, color);
    }

    private int GetBattleDisplayIndexForMonster(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return -1;
        }

        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null)
        {
            return -1;
        }

        for (int i = 0; i < encounter.MonsterIds.Length; i++)
        {
            if (encounter.MonsterIds[i] == monster.MonsterId)
            {
                return i;
            }
        }

        return -1;
    }

    private void FlashMonster(DungeonMonsterRuntimeData monster, Color color, float duration = BattleFeedbackFlashSeconds)
    {
        int displayIndex = GetBattleDisplayIndexForMonster(monster);
        if (displayIndex < 0 || displayIndex >= _monsterFeedbackUntilTimes.Length)
        {
            return;
        }

        _monsterFeedbackColors[displayIndex] = color;
        _monsterFeedbackUntilTimes[displayIndex] = Time.unscaledTime + duration;
    }

    private void FlashPartyMember(int memberIndex, Color color, float duration = BattleFeedbackFlashSeconds)
    {
        if (memberIndex < 0 || memberIndex >= _partyFeedbackUntilTimes.Length)
        {
            return;
        }

        _partyFeedbackColors[memberIndex] = color;
        _partyFeedbackUntilTimes[memberIndex] = Time.unscaledTime + duration;
    }

    private bool TryGetMonsterFeedback(int displayIndex, out Color color, out float intensity)
    {
        color = Color.white;
        intensity = 0f;
        if (displayIndex < 0 || displayIndex >= _monsterFeedbackUntilTimes.Length)
        {
            return false;
        }

        float remaining = _monsterFeedbackUntilTimes[displayIndex] - Time.unscaledTime;
        if (remaining <= 0f)
        {
            return false;
        }

        color = _monsterFeedbackColors[displayIndex];
        intensity = Mathf.Clamp01(remaining / BattleFeedbackFlashSeconds);
        return true;
    }

    private bool TryGetPartyFeedback(int memberIndex, out Color color, out float intensity)
    {
        color = Color.white;
        intensity = 0f;
        if (memberIndex < 0 || memberIndex >= _partyFeedbackUntilTimes.Length)
        {
            return false;
        }

        float remaining = _partyFeedbackUntilTimes[memberIndex] - Time.unscaledTime;
        if (remaining <= 0f)
        {
            return false;
        }

        color = _partyFeedbackColors[memberIndex];
        intensity = Mathf.Clamp01(remaining / BattleFeedbackFlashSeconds);
        return true;
    }

    private void ResolveCurrentEncounterVictory()
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null)
        {
            return;
        }

        bool eliteVictory = encounter.IsEliteEncounter;
        int encounterLoot = CalculateActiveEncounterLoot();
        if (!encounter.IsCleared)
        {
            encounter.IsCleared = true;
            _clearedEncounterCount += 1;
            _battleLootAmount += encounterLoot;
            _carriedLootAmount += encounterLoot;
        }

        if (eliteVictory)
        {
            _eliteEncounterActive = false;
            _eliteDefeated = true;
            _eliteRewardGranted = true;
            AppendBattleLog("Final elite defeated: " + _eliteName + ".");
            AppendBattleLog("Elite reward secured: " + _eliteRewardLabel + " (" + BuildLootAmountText(encounterLoot) + ").");
            if (_eliteBonusRewardPending > 0 && !_eliteBonusRewardGranted)
            {
                _eliteBonusRewardGranted = true;
                _eliteBonusRewardGrantedAmount = _eliteBonusRewardPending;
                _battleLootAmount += _eliteBonusRewardGrantedAmount;
                _carriedLootAmount += _eliteBonusRewardGrantedAmount;
                AppendBattleLog("Bonus reward granted: " + BuildLootAmountText(_eliteBonusRewardGrantedAmount) + ".");
                _eliteBonusRewardPending = 0;
            }
        }
        else
        {
            AppendBattleLog(encounter.DisplayName + " cleared. Loot +" + encounterLoot + " " + DungeonRewardResourceId + ".");
        }

        AppendBattleLog("Party Condition is now " + GetPartyConditionText() + ". Total Party HP: " + BuildTotalPartyHpSummary() + ".");

        if (_clearedEncounterCount >= TotalEncounterCount && !_exitUnlocked)
        {
            _exitUnlocked = true;
            AppendBattleLog(eliteVictory ? "Final elite defeated. The exit is now unlocked." : "All encounters cleared. The exit is now unlocked.");
        }

        _activeEncounterId = string.Empty;
        _dungeonRunState = DungeonRunState.Explore;
        _battleState = BattleState.None;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        _battleTurnIndex = 0;
        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        ClearBattleHoverState();
        ClearBattleInputLock();
        ClearEnemyIntentState();
        ResetRoundActions();
        SetActiveBattleMonster(null);
        string eliteFeedback = eliteVictory
            ? "Final elite defeated. " + _eliteRewardLabel + " secured." + (_eliteBonusRewardGrantedAmount > 0 ? " Bonus granted." : string.Empty) + " Reach the exit."
            : encounter.DisplayName + " defeated. Continue exploring.";
        SetBattleFeedbackText(eliteFeedback);
        RefreshRoomSequenceState(true);
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private void BeginEnemyTurn()
    {
        _battleState = BattleState.EnemyTurn;
        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        _hoverBattleAction = BattleActionType.None;
        _queuedBattleAction = BattleActionType.None;
        ClearBattleHoverState();
        if (!TryQueueEnemyIntent(0))
        {
            ResetRoundActions();
            _battleState = BattleState.PartyActionSelect;
            TrySelectNextPartyActor(0);
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            ClearEnemyIntentState();
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
        }
    }

    private bool TryQueueEnemyIntent(int startDisplayIndex)
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null)
        {
            return false;
        }

        int firstIndex = Mathf.Max(0, startDisplayIndex);
        if (firstIndex >= encounter.MonsterIds.Length)
        {
            return false;
        }

        for (int displayIndex = firstIndex; displayIndex < encounter.MonsterIds.Length; displayIndex++)
        {
            DungeonMonsterRuntimeData monster = GetBattleMonsterAtDisplayIndex(displayIndex);
            if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
            {
                continue;
            }

            int targetIndex = GetMonsterTargetPartyMemberIndex(monster);
            if (targetIndex < 0)
            {
                continue;
            }

            bool useSpecial = ShouldUseEliteSpecialAttack(monster);
            float telegraphDuration = useSpecial ? EnemyIntentTelegraphSeconds + 0.12f : EnemyIntentTelegraphSeconds;
            _enemyTurnMonsterCursor = displayIndex;
            _pendingEnemyTargetIndex = targetIndex;
            _pendingEnemyAttackPower = GetEnemyActionPower(monster, useSpecial);
            _pendingEnemyActionLabel = GetEnemyActionLabel(monster, useSpecial);
            _pendingEnemyUsedSpecialAttack = useSpecial;
            SetActiveBattleMonster(monster);
            _enemyIntentTelegraphActive = true;
            _enemyIntentResolveAtTime = Time.unscaledTime + telegraphDuration;
            _enemyIntentText = BuildEnemyIntentText(monster, targetIndex, useSpecial);
            AppendBattleLog(_enemyIntentText);
            SetBattleFeedbackText(_enemyIntentText);
            LockBattleInput(telegraphDuration);
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        return false;
    }

    private bool TryActivateCurrentBattleAction(BattleActionType action)
    {
        if (!IsBattleActionAvailable(action))
        {
            return false;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (action != BattleActionType.Retreat && member == null)
        {
            return false;
        }

        if (action == BattleActionType.Retreat)
        {
            AppendBattleLog("The party retreats from battle and abandons the run.");
            FinishDungeonRun(RunResultState.Retreat, BattleState.Retreat, false, 0, ActiveDungeonPartyText + " retreated from " + _currentDungeonName + " with no loot.");
            _pendingDungeonExit = true;
            return true;
        }

        ClearBattleHoverState();
        _hoverBattleAction = action;
        _queuedBattleAction = action;
        LockBattleInput();

        if (action == BattleActionType.Attack || (action == BattleActionType.Skill && DoesSkillRequireTarget(member)))
        {
            _battleState = BattleState.PartyTargetSelect;
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            ClearBattleInputLock();
            SetBattleFeedbackText(GetBattleActionDisplayName(action, member) + " selected.");
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        if (member.SkillType == PartySkillType.AllEnemies)
        {
            int hitCount = 0;
            for (int i = 0; i < 2; i++)
            {
                DungeonMonsterRuntimeData monster = GetBattleMonsterAtDisplayIndex(i);
                if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
                {
                    continue;
                }

                int damage = Mathf.Max(1, member.SkillPower);
                monster.CurrentHp = Mathf.Max(0, monster.CurrentHp - damage);
                FlashMonster(monster, new Color(0.46f, 0.75f, 1f, 1f));
                ShowBattlePopupForMonster(monster, "-" + damage, new Color(0.46f, 0.75f, 1f, 1f));
                hitCount += 1;
                if (monster.CurrentHp <= 0)
                {
                    monster.IsDefeated = true;
                    ShowBattlePopupForMonster(monster, "Defeated", new Color(1f, 0.82f, 0.24f, 1f), 1.18f);
                    AppendBattleLog(monster.DisplayName + " is defeated.");
                }
            }

            AppendBattleLog(member.DisplayName + " used " + member.SkillName + " on all enemies.");
            SetBattleFeedbackText(member.SkillName + " hit " + hitCount + " enemies.");
        }
        else if (member.SkillType == PartySkillType.PartyHeal)
        {
            int totalRecovered = 0;
            for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
            {
                DungeonPartyMemberRuntimeData ally = _activeDungeonParty.Members[i];
                if (ally == null || ally.IsDefeated || ally.CurrentHp <= 0)
                {
                    continue;
                }

                int previousHp = ally.CurrentHp;
                ally.CurrentHp = Mathf.Min(ally.MaxHp, ally.CurrentHp + Mathf.Max(1, member.SkillPower));
                int recovered = ally.CurrentHp - previousHp;
                if (recovered <= 0)
                {
                    continue;
                }

                totalRecovered += recovered;
                FlashPartyMember(i, new Color(0.56f, 1f, 0.68f, 1f));
                ShowBattlePopupForPartyMember(i, "+" + recovered, new Color(0.56f, 1f, 0.68f, 1f));
            }

            AppendBattleLog(member.DisplayName + " used " + member.SkillName + " and restored " + totalRecovered + " HP.");
            SetBattleFeedbackText(member.SkillName + " restored " + totalRecovered + " HP.");
        }

        _partyActedThisRound[_currentActorIndex] = true;
        _queuedBattleAction = BattleActionType.None;
        ClearBattleInputLock();

        if (GetLivingBattleMonsterCount() <= 0)
        {
            ResolveCurrentEncounterVictory();
            return true;
        }

        int nextIndex = Mathf.Max(0, _currentActorIndex + 1);
        if (TrySelectNextPartyActor(nextIndex))
        {
            _battleState = BattleState.PartyActionSelect;
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        BeginEnemyTurn();
        return true;
    }
    private void HandleDungeonBattleInput(Keyboard keyboard)
    {
        if (keyboard == null || _dungeonRunState != DungeonRunState.Battle || IsBattleInputLocked())
        {
            return;
        }

        if (_battleState == BattleState.PartyTargetSelect)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelTargetSelection();
                return;
            }

            if (keyboard.qKey.wasPressedThisFrame || keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                TryActivateCurrentBattleAction(BattleActionType.Retreat);
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                TryResolveTargetSelection(GetBattleMonsterAtDisplayIndex(0));
                return;
            }

            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                TryResolveTargetSelection(GetBattleMonsterAtDisplayIndex(1));
            }

            return;
        }

        if (_battleState != BattleState.PartyActionSelect)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            TryActivateCurrentBattleAction(BattleActionType.Attack);
            return;
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            TryActivateCurrentBattleAction(BattleActionType.Skill);
            return;
        }

        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
        {
            TryActivateCurrentBattleAction(BattleActionType.Retreat);
        }
    }

    private int ApplyShrineRecovery()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int totalRecovered = 0;
        int recoverAmount = GetCurrentShrineRecoverAmount();
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null || member.IsDefeated || member.CurrentHp <= 0)
            {
                continue;
            }

            int previousHp = member.CurrentHp;
            member.CurrentHp = Mathf.Min(member.MaxHp, member.CurrentHp + recoverAmount);
            int recovered = member.CurrentHp - previousHp;
            if (recovered <= 0)
            {
                continue;
            }

            totalRecovered += recovered;
            FlashPartyMember(i, new Color(0.56f, 1f, 0.68f, 1f));
            ShowBattlePopupForPartyMember(i, "+" + recovered, new Color(0.56f, 1f, 0.68f, 1f));
        }

        return totalRecovered;
    }

    private bool TryResolveTargetSelection(DungeonMonsterRuntimeData targetMonster)
    {
        if (_dungeonRunState != DungeonRunState.Battle || _battleState != BattleState.PartyTargetSelect || IsBattleInputLocked() || targetMonster == null || targetMonster.IsDefeated || targetMonster.CurrentHp <= 0)
        {
            return false;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return false;
        }

        SetActiveBattleMonster(targetMonster);
        int damage = _queuedBattleAction == BattleActionType.Skill
            ? member.SkillType == PartySkillType.Finisher && targetMonster.CurrentHp <= member.Attack
                ? member.SkillPower + 2
                : member.SkillPower
            : member.Attack;
        damage = Mathf.Max(1, damage);
        targetMonster.CurrentHp = Mathf.Max(0, targetMonster.CurrentHp - damage);
        FlashMonster(targetMonster, new Color(1f, 0.48f, 0.30f, 1f));
        ShowBattlePopupForMonster(targetMonster, "-" + damage, new Color(1f, 0.48f, 0.30f, 1f));

        string actionName = _queuedBattleAction == BattleActionType.Skill ? member.SkillName : "Attack";
        AppendBattleLog(member.DisplayName + " used " + actionName + " on " + targetMonster.DisplayName + " for " + damage + " damage.");
        SetBattleFeedbackText(actionName + " dealt " + damage + " to " + targetMonster.DisplayName + ".");

        if (targetMonster.CurrentHp <= 0)
        {
            targetMonster.IsDefeated = true;
            ShowBattlePopupForMonster(targetMonster, "Defeated", new Color(1f, 0.82f, 0.24f, 1f), 1.18f);
            AppendBattleLog(targetMonster.DisplayName + " is defeated.");
        }

        _partyActedThisRound[_currentActorIndex] = true;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        ClearBattleHoverState();

        if (GetLivingBattleMonsterCount() <= 0)
        {
            ResolveCurrentEncounterVictory();
            return true;
        }

        int nextIndex = Mathf.Max(0, _currentActorIndex + 1);
        if (TrySelectNextPartyActor(nextIndex))
        {
            _battleState = BattleState.PartyActionSelect;
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        BeginEnemyTurn();
        return true;
    }

    private void ExecuteQueuedEnemyIntent()
    {
        if (_dungeonRunState != DungeonRunState.Battle || _battleState != BattleState.EnemyTurn || !_enemyIntentTelegraphActive || _activeBattleMonster == null)
        {
            return;
        }

        _enemyIntentTelegraphActive = false;
        ClearBattleInputLock();
        int targetIndex = _pendingEnemyTargetIndex;
        bool usePartyWideEliteSpecial = IsPartyWideEliteSpecial(_activeBattleMonster, _pendingEnemyUsedSpecialAttack);
        if (!usePartyWideEliteSpecial && (_activeDungeonParty == null || _activeDungeonParty.Members == null || targetIndex < 0 || targetIndex >= _activeDungeonParty.Members.Length))
        {
            if (!TryQueueEnemyIntent(_enemyTurnMonsterCursor + 1))
            {
                ResetRoundActions();
                _battleState = BattleState.PartyActionSelect;
                TrySelectNextPartyActor(0);
                SetActiveBattleMonster(GetFirstLivingBattleMonster());
                ClearEnemyIntentState();
                RefreshSelectionPrompt();
                RefreshDungeonPresentation();
            }

            return;
        }

        DungeonPartyMemberRuntimeData targetMember = (!usePartyWideEliteSpecial && _activeDungeonParty != null && _activeDungeonParty.Members != null && targetIndex >= 0 && targetIndex < _activeDungeonParty.Members.Length)
            ? _activeDungeonParty.Members[targetIndex]
            : null;
        if (!usePartyWideEliteSpecial && (targetMember == null || targetMember.IsDefeated || targetMember.CurrentHp <= 0))
        {
            if (!TryQueueEnemyIntent(_enemyTurnMonsterCursor + 1))
            {
                ResetRoundActions();
                _battleState = BattleState.PartyActionSelect;
                TrySelectNextPartyActor(0);
                SetActiveBattleMonster(GetFirstLivingBattleMonster());
                ClearEnemyIntentState();
                RefreshSelectionPrompt();
                RefreshDungeonPresentation();
            }

            return;
        }

        int damage = Mathf.Max(1, _pendingEnemyAttackPower > 0 ? _pendingEnemyAttackPower : _activeBattleMonster.Attack);
        string actionLabel = string.IsNullOrEmpty(_pendingEnemyActionLabel) ? "Attack" : _pendingEnemyActionLabel;
        if (usePartyWideEliteSpecial)
        {
            int sweepDamage = Mathf.Max(1, damage - 4);
            int hitCount = 0;
            int totalDamage = 0;
            for (int memberIndex = 0; memberIndex < _activeDungeonParty.Members.Length; memberIndex++)
            {
                DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[memberIndex];
                if (member == null || member.IsDefeated || member.CurrentHp <= 0)
                {
                    continue;
                }

                member.CurrentHp = Mathf.Max(0, member.CurrentHp - sweepDamage);
                FlashPartyMember(memberIndex, new Color(1f, 0.34f, 0.42f, 1f));
                ShowBattlePopupForPartyMember(memberIndex, "-" + sweepDamage, new Color(1f, 0.34f, 0.42f, 1f));
                totalDamage += sweepDamage;
                hitCount += 1;
                if (member.CurrentHp <= 0)
                {
                    member.IsDefeated = true;
                    ShowBattlePopupForPartyMember(memberIndex, "KO", new Color(1f, 0.82f, 0.24f, 1f), 1.02f);
                    AppendBattleLog(member.DisplayName + " is KO.");
                }
            }

            AppendBattleLog(_activeBattleMonster.DisplayName + " used " + actionLabel + " on all living allies for " + totalDamage + " total damage.");
            SetBattleFeedbackText(_activeBattleMonster.DisplayName + " unleashed " + actionLabel + " on " + hitCount + " allies.");
            _activeBattleMonster.TurnsActed += 1;
        }
        else
        {
            targetMember.CurrentHp = Mathf.Max(0, targetMember.CurrentHp - damage);
            FlashPartyMember(targetIndex, new Color(1f, 0.40f, 0.35f, 1f));
            ShowBattlePopupForPartyMember(targetIndex, "-" + damage, new Color(1f, 0.40f, 0.35f, 1f));
            AppendBattleLog(_activeBattleMonster.DisplayName + " used " + actionLabel + " on " + targetMember.DisplayName + " for " + damage + " damage.");
            SetBattleFeedbackText(_activeBattleMonster.DisplayName + " used " + actionLabel + " on " + targetMember.DisplayName + ".");
            _activeBattleMonster.TurnsActed += 1;

            if (targetMember.CurrentHp <= 0)
            {
                targetMember.IsDefeated = true;
                ShowBattlePopupForPartyMember(targetIndex, "KO", new Color(1f, 0.82f, 0.24f, 1f), 1.02f);
                AppendBattleLog(targetMember.DisplayName + " is KO.");
            }
        }

        if (GetFirstLivingPartyMemberIndex() < 0)
        {
            FinishDungeonRun(RunResultState.Defeat, BattleState.Defeat, false, 0, ActiveDungeonPartyText + " was defeated in " + _currentDungeonName + ".");
            return;
        }

        if (TryQueueEnemyIntent(_enemyTurnMonsterCursor + 1))
        {
            return;
        }

        ResetRoundActions();
        _battleState = BattleState.PartyActionSelect;
        TrySelectNextPartyActor(0);
        SetActiveBattleMonster(GetFirstLivingBattleMonster());
        ClearEnemyIntentState();
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private string BuildSurvivingMembersSummary()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return "0 / 4";
        }

        int survivingCount = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && !member.IsDefeated && member.CurrentHp > 0)
            {
                survivingCount += 1;
            }
        }

        return survivingCount + " / " + _activeDungeonParty.Members.Length;
    }

    private string BuildClearedEncounterSummary()
    {
        return _clearedEncounterCount + " / " + TotalEncounterCount;
    }

    private bool IsReturnToWorldPressed(Keyboard keyboard)
    {
        return keyboard != null &&
               (keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame);
    }

    private string GetHomeCityDisplayName()
    {
        WorldEntityData city = FindEntity(_currentHomeCityId);
        return city != null ? city.DisplayName : "Home City";
    }
    private DungeonMonsterRuntimeData CreateMonster(string monsterId, string encounterId, int roomIndex, string displayName, string monsterType, int hp, int attack, Vector2Int gridPosition, int rewardAmount, MonsterTargetPattern targetPattern, MonsterEncounterRole encounterRole, bool isElite = false, int specialAttack = 0, string specialActionName = "")
    {
        return new DungeonMonsterRuntimeData(monsterId, encounterId, roomIndex, displayName, monsterType, hp, attack, gridPosition, DungeonRewardResourceId, rewardAmount, targetPattern, encounterRole, isElite, specialAttack, specialActionName);
    }

    private TestDungeonPartyData CreatePlaceholderDungeonParty(string cityId, string partyId)
    {
        PrototypeRpgPartyDefinition partyDefinition = PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
        return new TestDungeonPartyData(partyId, cityId, partyDefinition, CreateDungeonRuntimeMembers(partyDefinition));
    }

    private DungeonPartyMemberRuntimeData[] CreateDungeonRuntimeMembers(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null || partyDefinition.Members == null || partyDefinition.Members.Length == 0)
        {
            return System.Array.Empty<DungeonPartyMemberRuntimeData>();
        }

        DungeonPartyMemberRuntimeData[] members = new DungeonPartyMemberRuntimeData[partyDefinition.Members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            members[i] = CreateDungeonRuntimeMember(partyDefinition.Members[i], i);
        }

        return members;
    }

    private DungeonPartyMemberRuntimeData CreateDungeonRuntimeMember(PrototypeRpgPartyMemberDefinition memberDefinition, int fallbackPartySlotIndex)
    {
        PrototypeRpgStatBlock baseStats = memberDefinition != null && memberDefinition.BaseStats != null
            ? memberDefinition.BaseStats
            : new PrototypeRpgStatBlock(1, 1, 0, 0);
        string memberId = memberDefinition != null ? memberDefinition.MemberId : string.Empty;
        string displayName = memberDefinition != null && !string.IsNullOrEmpty(memberDefinition.DisplayName) ? memberDefinition.DisplayName : "Adventurer";
        string roleTag = memberDefinition != null ? memberDefinition.RoleTag : string.Empty;
        string roleLabel = memberDefinition != null && !string.IsNullOrEmpty(memberDefinition.RoleLabel) ? memberDefinition.RoleLabel : "Adventurer";
        string defaultSkillId = memberDefinition != null ? memberDefinition.DefaultSkillId : string.Empty;
        string skillName = memberDefinition != null && !string.IsNullOrEmpty(memberDefinition.DefaultSkillName) ? memberDefinition.DefaultSkillName : "Skill";
        string skillShortText = memberDefinition != null ? memberDefinition.DefaultSkillShortText : string.Empty;
        int partySlotIndex = memberDefinition != null ? memberDefinition.PartySlotIndex : fallbackPartySlotIndex;

        PartySkillType skillType = PartySkillType.SingleHeavy;
        int skillPower = Mathf.Max(1, baseStats.Attack + 1);
        Color viewColor = Color.white;

        switch (defaultSkillId)
        {
            case "skill_power_strike":
                roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Warrior" : roleLabel;
                skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Power Strike" : skillName;
                skillShortText = string.IsNullOrEmpty(skillShortText) ? "Heavy single-target strike." : skillShortText;
                skillType = PartySkillType.SingleHeavy;
                skillPower = 10;
                viewColor = new Color(0.78f, 0.47f, 0.29f, 1f);
                break;
            case "skill_weak_point":
                roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Rogue" : roleLabel;
                skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Weak Point" : skillName;
                skillShortText = string.IsNullOrEmpty(skillShortText) ? "Finisher that hits harder on weak targets." : skillShortText;
                skillType = PartySkillType.Finisher;
                skillPower = 7;
                viewColor = new Color(0.28f, 0.75f, 0.58f, 1f);
                break;
            case "skill_arcane_burst":
                roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Mage" : roleLabel;
                skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Arcane Burst" : skillName;
                skillShortText = string.IsNullOrEmpty(skillShortText) ? "Arcane blast that hits all enemies." : skillShortText;
                skillType = PartySkillType.AllEnemies;
                skillPower = 6;
                viewColor = new Color(0.34f, 0.68f, 0.95f, 1f);
                break;
            case "skill_radiant_hymn":
                roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Cleric" : roleLabel;
                skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Radiant Hymn" : skillName;
                skillShortText = string.IsNullOrEmpty(skillShortText) ? "Party heal that restores all allies." : skillShortText;
                skillType = PartySkillType.PartyHeal;
                skillPower = 5;
                viewColor = new Color(0.94f, 0.84f, 0.46f, 1f);
                break;
            default:
                switch (roleTag)
                {
                    case "warrior":
                        roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Warrior" : roleLabel;
                        defaultSkillId = string.IsNullOrEmpty(defaultSkillId) ? "skill_power_strike" : defaultSkillId;
                        skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Power Strike" : skillName;
                        skillShortText = string.IsNullOrEmpty(skillShortText) ? "Heavy single-target strike." : skillShortText;
                        skillType = PartySkillType.SingleHeavy;
                        skillPower = 10;
                        viewColor = new Color(0.78f, 0.47f, 0.29f, 1f);
                        break;
                    case "rogue":
                        roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Rogue" : roleLabel;
                        defaultSkillId = string.IsNullOrEmpty(defaultSkillId) ? "skill_weak_point" : defaultSkillId;
                        skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Weak Point" : skillName;
                        skillShortText = string.IsNullOrEmpty(skillShortText) ? "Finisher that hits harder on weak targets." : skillShortText;
                        skillType = PartySkillType.Finisher;
                        skillPower = 7;
                        viewColor = new Color(0.28f, 0.75f, 0.58f, 1f);
                        break;
                    case "mage":
                        roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Mage" : roleLabel;
                        defaultSkillId = string.IsNullOrEmpty(defaultSkillId) ? "skill_arcane_burst" : defaultSkillId;
                        skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Arcane Burst" : skillName;
                        skillShortText = string.IsNullOrEmpty(skillShortText) ? "Arcane blast that hits all enemies." : skillShortText;
                        skillType = PartySkillType.AllEnemies;
                        skillPower = 6;
                        viewColor = new Color(0.34f, 0.68f, 0.95f, 1f);
                        break;
                    case "cleric":
                        roleLabel = string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer" ? "Cleric" : roleLabel;
                        defaultSkillId = string.IsNullOrEmpty(defaultSkillId) ? "skill_radiant_hymn" : defaultSkillId;
                        skillName = string.IsNullOrEmpty(skillName) || skillName == "Skill" ? "Radiant Hymn" : skillName;
                        skillShortText = string.IsNullOrEmpty(skillShortText) ? "Party heal that restores all allies." : skillShortText;
                        skillType = PartySkillType.PartyHeal;
                        skillPower = 5;
                        viewColor = new Color(0.94f, 0.84f, 0.46f, 1f);
                        break;
                    default:
                        roleLabel = string.IsNullOrEmpty(roleLabel) ? (string.IsNullOrEmpty(roleTag) ? "Adventurer" : roleTag) : roleLabel;
                        break;
                }
                break;
        }

        return new DungeonPartyMemberRuntimeData(memberId, displayName, roleLabel, roleTag, defaultSkillId, skillName, skillShortText, skillType, partySlotIndex, baseStats.MaxHp, baseStats.Attack, baseStats.Defense, baseStats.Speed, skillPower, viewColor);
    }

    private TestDungeonPartyData GetOrCreateDungeonParty(string cityId, string partyId)
    {
        if (string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(partyId))
        {
            return null;
        }

        if (!_dungeonPartyByCityId.TryGetValue(cityId, out TestDungeonPartyData party) || party == null || party.PartyId != partyId)
        {
            party = CreatePlaceholderDungeonParty(cityId, partyId);
            _dungeonPartyByCityId[cityId] = party;
        }
        else
        {
            party.PartyId = partyId;
            party.HomeCityId = cityId;
            party.PartyDefinition = party.PartyDefinition ?? PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
            party.DisplayName = party.PartyDefinition != null && !string.IsNullOrEmpty(party.PartyDefinition.DisplayName)
                ? party.PartyDefinition.DisplayName
                : partyId;
            if (party.Members == null || party.Members.Length == 0)
            {
                party.Members = CreateDungeonRuntimeMembers(party.PartyDefinition);
            }
        }

        return party;
    }

    private void AppendBattleLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (_recentBattleLogs.Count >= RecentBattleLogLimit)
        {
            _recentBattleLogs.RemoveAt(0);
        }

        _recentBattleLogs.Add(message.Trim());
    }

    private bool DoesSkillRequireTarget(DungeonPartyMemberRuntimeData member)
    {
        return member != null && (member.SkillType == PartySkillType.SingleHeavy || member.SkillType == PartySkillType.Finisher);
    }

    private void HandleDungeonRouteChoiceInput(Keyboard keyboard)
    {
        if (keyboard == null || _dungeonRunState != DungeonRunState.RouteChoice)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelRouteChoice();
            return;
        }

        if (keyboard.qKey.wasPressedThisFrame)
        {
            TryCycleCurrentDispatchPolicy();
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            TryTriggerRouteChoice(SafeRouteId);
            return;
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            TryTriggerRouteChoice(RiskyRouteId);
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            TryConfirmRouteChoice();
        }
    }

    private void CancelRouteChoice()
    {
        if (_dungeonRunState != DungeonRunState.RouteChoice)
        {
            return;
        }

        _recentBattleLogs.Clear();
        ResetDungeonRunPresentationState();
        if (_dungeonRoot != null)
        {
            _dungeonRoot.SetActive(false);
        }

        _pendingDungeonExit = true;
    }

    private void HandleDungeonEventChoiceInput(Keyboard keyboard)
    {
        if (keyboard == null || _dungeonRunState != DungeonRunState.EventChoice)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            TryTriggerEventChoice("recover");
            return;
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            TryTriggerEventChoice("loot");
        }
    }

    private void HandleDungeonPreEliteChoiceInput(Keyboard keyboard)
    {
        if (keyboard == null || _dungeonRunState != DungeonRunState.PreEliteChoice)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            TryTriggerPreEliteChoice("recover");
            return;
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            TryTriggerPreEliteChoice("bonus");
        }
    }

    private void HandleDungeonExploreInput(Keyboard keyboard)
    {
        if (keyboard == null || _dungeonRunState != DungeonRunState.Explore)
        {
            return;
        }

        if (keyboard.qKey.wasPressedThisFrame)
        {
            AppendBattleLog("The party abandons the run before reaching the exit.");
            FinishDungeonRun(RunResultState.Retreat, BattleState.Retreat, false, 0, ActiveDungeonPartyText + " retreated from " + _currentDungeonName + " with no loot.");
            _pendingDungeonExit = true;
            return;
        }

        Vector2Int delta = Vector2Int.zero;
        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) delta = Vector2Int.up;
        else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) delta = Vector2Int.down;
        else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame) delta = Vector2Int.left;
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) delta = Vector2Int.right;

        bool acted = false;
        if (delta != Vector2Int.zero)
        {
            Vector2Int nextPosition = _playerGridPosition + delta;
            if (!IsWithinDungeonGrid(nextPosition))
            {
                return;
            }

            _playerGridPosition = nextPosition;
            int roomIndex = GetRoomIndexForPosition(nextPosition);
            if (roomIndex > 0)
            {
                _currentRoomIndex = roomIndex;
            }

            acted = true;
        }
        else if (keyboard.spaceKey.wasPressedThisFrame)
        {
            acted = true;
        }

        if (!acted)
        {
            return;
        }

        _runTurnCount += 1;
        ProcessExploreStep();
    }

    private void ProcessExploreStep()
    {
        DungeonRoomTemplateData currentRoom = GetCurrentPlannedRoomStep();
        if (currentRoom != null)
        {
            _currentRoomIndex = Mathf.Max(1, GetCurrentPlannedRoomIndex() + 1);
        }

        if (_exitUnlocked && _eliteDefeated && _playerGridPosition == _exitGridPosition)
        {
            AppendRoomPathLabel("Exit Route");
            AppendBattleLog("The party reached the exit and escaped the dungeon.");
            FinishDungeonRun(RunResultState.Clear, BattleState.Victory, true, _carriedLootAmount, ActiveDungeonPartyText + " cleared " + _currentDungeonName + " and returned with " + BuildLootAmountText(_carriedLootAmount) + ".");
            return;
        }

        if (currentRoom == null)
        {
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return;
        }

        if (currentRoom.RoomType == DungeonRoomType.Cache)
        {
            if (_activeChest != null)
            {
                _activeChest.GridPosition = currentRoom.MarkerPosition;
            }

            if (_activeChest != null && !_activeChest.IsOpened && _playerGridPosition == currentRoom.MarkerPosition)
            {
                _activeChest.IsOpened = true;
                _chestOpenedCount += 1;
                _chestLootAmount += _activeChest.RewardAmount;
                _carriedLootAmount += _activeChest.RewardAmount;
                AppendBattleLog("Opened " + currentRoom.DisplayName + " for +" + _activeChest.RewardAmount + " " + DungeonRewardResourceId + ".");
                AppendBattleLog("Party Condition is now " + GetPartyConditionText() + ". Total Party HP: " + BuildTotalPartyHpSummary() + ".");
                SetBattleFeedbackText(currentRoom.DisplayName + " yielded " + _activeChest.RewardAmount + " " + DungeonRewardResourceId + ".");
                RefreshRoomSequenceState(true);
                RefreshSelectionPrompt();
                RefreshDungeonPresentation();
                return;
            }
        }
        else if (currentRoom.RoomType == DungeonRoomType.Shrine)
        {
            if (TryTriggerEventChoiceFromTile())
            {
                return;
            }
        }
        else if (currentRoom.RoomType == DungeonRoomType.Preparation)
        {
            if (TryTriggerPreEliteChoiceFromTile())
            {
                return;
            }
        }
        else if (currentRoom.RoomType == DungeonRoomType.Skirmish || currentRoom.RoomType == DungeonRoomType.Elite)
        {
            if (TryStartEncounter())
            {
                return;
            }
        }

        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private bool TryTriggerEventChoiceFromTile()
    {
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        if (room == null || room.RoomType != DungeonRoomType.Shrine || _eventResolved || _playerGridPosition != room.MarkerPosition)
        {
            return false;
        }

        OpenEventChoicePanel();
        return true;
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

    private void OpenEventChoicePanel()
    {
        _dungeonRunState = DungeonRunState.EventChoice;
        _hoverEventChoiceId = string.Empty;
        _selectedEventChoiceId = string.Empty;
        AppendBattleLog("Entered " + GetCurrentEventTitleText() + ".");
        SetBattleFeedbackText(GetCurrentEventTitleText() + " offers a choice.");
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private void OpenPreEliteChoicePanel()
    {
        _dungeonRunState = DungeonRunState.PreEliteChoice;
        _hoverPreEliteChoiceId = string.Empty;
        _selectedPreEliteChoiceId = string.Empty;
        AppendBattleLog("Entered " + GetCurrentPreEliteTitleText() + ".");
        SetBattleFeedbackText(GetCurrentPreEliteTitleText() + " offers one final choice before the elite.");
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private bool TryStartEncounter()
    {
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        if (room == null || (room.RoomType != DungeonRoomType.Skirmish && room.RoomType != DungeonRoomType.Elite))
        {
            return false;
        }

        if (room.RoomType == DungeonRoomType.Elite && !_preEliteDecisionResolved)
        {
            return false;
        }

        DungeonEncounterRuntimeData encounter = room.RoomType == DungeonRoomType.Elite ? GetEliteEncounter() : GetEncounterById(room.EncounterId);
        if (encounter == null || encounter.IsCleared || !string.IsNullOrEmpty(_activeEncounterId))
        {
            return false;
        }

        if (_playerGridPosition != room.MarkerPosition && !IsEncounterTriggered(encounter))
        {
            return false;
        }

        _activeEncounterId = encounter.EncounterId;
        _dungeonRunState = DungeonRunState.Battle;
        _battleState = BattleState.PartyActionSelect;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        _battleTurnIndex += 1;
        _selectedPartyMemberIndex = -1;
        _eliteEncounterActive = encounter.IsEliteEncounter;
        ClearBattleHoverState();
        ClearBattleInputLock();
        ClearEnemyIntentState();
        ClearBattleFloatingPopups();
        ResetRoundActions();
        TrySelectNextPartyActor(0);
        SetActiveBattleMonster(GetFirstLivingBattleMonster());

        if (encounter.IsEliteEncounter)
        {
            AppendBattleLog("Entered Elite Chamber.");
            AppendBattleLog("Final elite encounter started: " + encounter.DisplayName + ".");
            SetBattleFeedbackText("Final elite encounter: " + encounter.DisplayName + ".");
        }
        else
        {
            AppendBattleLog("Encounter! " + encounter.DisplayName + " pulls the party into battle view.");
            SetBattleFeedbackText(encounter.DisplayName + " engages the party.");
        }

        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }
    private void BuildFixedDungeonRoom()
    {
        _activeMonsters.Clear();
        _activeEncounters.Clear();
        _roomPathHistory.Clear();
        _currentRoomStepId = string.Empty;
        _eliteEncounterId = "encounter-room-elite";
        _eliteEncounterActive = false;
        _eliteDefeated = false;
        _resultEliteDefeated = false;
        _eliteName = "None";
        _eliteType = "None";
        _eliteRewardLabel = "None";
        _eliteRewardAmount = 0;
        _eliteRewardGranted = false;
        _resultEliteName = "None";
        _resultEliteRewardLabel = "None";
        _resultEliteRewardAmount = 0;
        _pendingEnemyUsedSpecialAttack = false;
        _resultRoomPathSummaryText = "None";
        _resultPartyHpSummaryText = "None";
        _resultPartyConditionText = "None";
        _playerGridPosition = new Vector2Int(2, 4);
        _exitGridPosition = new Vector2Int(14, 6);
        _eventGridPosition = ShrineEventGridPosition;
        _currentRoomIndex = 1;

        DungeonRouteTemplate template = GetSelectedRouteTemplate();
        bool isBeta = _currentDungeonId == "dungeon-beta";
        bool isRisky = _selectedRouteId == RiskyRouteId;
        int chestReward = template != null ? template.ChestRewardAmount : ChestRewardAmount;
        _activeChest = new DungeonChestRuntimeData("room-cache", 2, Room2ChestGridPosition, DungeonRewardResourceId, chestReward);

        if (isBeta)
        {
            if (isRisky)
            {
                _activeMonsters.Add(CreateMonster("beta-risky-room1-goblin-skirmisher", "encounter-room-1", 1, "Goblin Scout A", "Goblin", 12, 4, new Vector2Int(4, 5), 3, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Skirmisher));
                _activeMonsters.Add(CreateMonster("beta-risky-room1-goblin-striker", "encounter-room-1", 1, "Goblin Striker A", "Goblin", 14, 3, new Vector2Int(5, 4), 4, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, false, 7, "Heavy Strike"));
                _activeMonsters.Add(CreateMonster("beta-risky-room2-goblin-bulwark", "encounter-room-2", 2, "Goblin Guard", "Goblin", 21, 3, new Vector2Int(12, 5), 3, MonsterTargetPattern.FirstLiving, MonsterEncounterRole.Bulwark));
                _activeMonsters.Add(CreateMonster("beta-risky-room2-goblin-striker", "encounter-room-2", 2, "Goblin Striker B", "Goblin", 15, 4, new Vector2Int(13, 4), 4, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, false, 8, "Crushing Blow"));
                _eliteName = "Raider Chief";
                _eliteType = "Goblin Elite | Focused Execution";
                _eliteRewardLabel = "Raider War Spoils";
                _eliteRewardAmount = 11;
                _activeMonsters.Add(CreateMonster("beta-risky-elite", _eliteEncounterId, 3, _eliteName, "Goblin", 36, 6, new Vector2Int(13, 6), _eliteRewardAmount, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, true, 11, "Execution Strike"));
            }
            else
            {
                _activeMonsters.Add(CreateMonster("beta-safe-room1-slime-bulwark", "encounter-room-1", 1, "Slime Scout", "Slime", 18, 2, new Vector2Int(4, 5), 2, MonsterTargetPattern.RandomLiving, MonsterEncounterRole.Bulwark));
                _activeMonsters.Add(CreateMonster("beta-safe-room1-goblin-skirmisher", "encounter-room-1", 1, "Goblin Watch", "Goblin", 12, 4, new Vector2Int(5, 4), 3, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Skirmisher));
                _activeMonsters.Add(CreateMonster("beta-safe-room2-goblin-bulwark", "encounter-room-2", 2, "Goblin Guard", "Goblin", 20, 3, new Vector2Int(12, 5), 3, MonsterTargetPattern.FirstLiving, MonsterEncounterRole.Bulwark));
                _activeMonsters.Add(CreateMonster("beta-safe-room2-goblin-skirmisher", "encounter-room-2", 2, "Goblin Raider", "Goblin", 13, 4, new Vector2Int(13, 4), 3, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Skirmisher));
                _eliteName = "Goblin Captain";
                _eliteType = "Goblin Elite | Focused Command";
                _eliteRewardLabel = "Captain's Stash";
                _eliteRewardAmount = 9;
                _activeMonsters.Add(CreateMonster("beta-safe-elite", _eliteEncounterId, 3, _eliteName, "Goblin", 32, 5, new Vector2Int(13, 6), _eliteRewardAmount, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Bulwark, true, 9, "Command Strike"));
            }
        }
        else
        {
            if (isRisky)
            {
                _activeMonsters.Add(CreateMonster("alpha-risky-room1-slime-bulwark", "encounter-room-1", 1, "Slime A", "Slime", 18, 2, new Vector2Int(4, 5), 2, MonsterTargetPattern.RandomLiving, MonsterEncounterRole.Bulwark));
                _activeMonsters.Add(CreateMonster("alpha-risky-room1-goblin-striker", "encounter-room-1", 1, "Goblin A", "Goblin", 14, 3, new Vector2Int(5, 4), 3, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, false, 7, "Heavy Strike"));
                _activeMonsters.Add(CreateMonster("alpha-risky-room2-goblin-skirmisher", "encounter-room-2", 2, "Goblin B", "Goblin", 12, 4, new Vector2Int(12, 5), 2, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Skirmisher));
                _activeMonsters.Add(CreateMonster("alpha-risky-room2-goblin-striker", "encounter-room-2", 2, "Goblin C", "Goblin", 14, 4, new Vector2Int(13, 4), 2, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, false, 8, "Rending Blow"));
                _eliteName = "Gel Core";
                _eliteType = "Volatile Slime Elite | Surging Pressure";
                _eliteRewardLabel = "Volatile Core Cache";
                _eliteRewardAmount = 8;
                _activeMonsters.Add(CreateMonster("alpha-risky-elite", _eliteEncounterId, 3, _eliteName, "Slime", 30, 5, new Vector2Int(13, 6), _eliteRewardAmount, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, true, 10, "Core Rupture"));
            }
            else
            {
                _activeMonsters.Add(CreateMonster("alpha-safe-room1-slime-bulwark-a", "encounter-room-1", 1, "Slime A", "Slime", 19, 2, new Vector2Int(4, 5), 2, MonsterTargetPattern.RandomLiving, MonsterEncounterRole.Bulwark));
                _activeMonsters.Add(CreateMonster("alpha-safe-room1-slime-bulwark-b", "encounter-room-1", 1, "Slime B", "Slime", 18, 2, new Vector2Int(5, 4), 2, MonsterTargetPattern.RandomLiving, MonsterEncounterRole.Bulwark));
                _activeMonsters.Add(CreateMonster("alpha-safe-room2-slime-bulwark", "encounter-room-2", 2, "Slime C", "Slime", 18, 2, new Vector2Int(12, 5), 2, MonsterTargetPattern.RandomLiving, MonsterEncounterRole.Bulwark));
                _activeMonsters.Add(CreateMonster("alpha-safe-room2-slime-skirmisher", "encounter-room-2", 2, "Slime D", "Slime", 12, 3, new Vector2Int(13, 4), 2, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Skirmisher));
                _eliteName = "Slime Monarch";
                _eliteType = "Royal Slime Elite | Surging Pressure";
                _eliteRewardLabel = "Royal Gel Cache";
                _eliteRewardAmount = 6;
                _activeMonsters.Add(CreateMonster("alpha-safe-elite", _eliteEncounterId, 3, _eliteName, "Slime", 34, 5, new Vector2Int(13, 6), _eliteRewardAmount, MonsterTargetPattern.RandomLiving, MonsterEncounterRole.Bulwark, true, 9, "Royal Wave"));
            }
        }

        _activeEncounters.Add(new DungeonEncounterRuntimeData("encounter-room-1", 1, _currentDungeonId == "dungeon-beta" ? (isRisky ? "Raider Gate" : "Scout Gate") : (isRisky ? "Mixed Front" : "Slime Front"), new[] { _activeMonsters[0].MonsterId, _activeMonsters[1].MonsterId }));
        _activeEncounters.Add(new DungeonEncounterRuntimeData("encounter-room-2", 2, _currentDungeonId == "dungeon-beta" ? (isRisky ? "Ambush Hall" : "Guarded Vault") : (isRisky ? "Goblin Pair Hall" : "Watch Hall"), new[] { _activeMonsters[2].MonsterId, _activeMonsters[3].MonsterId }));
        _activeEncounters.Add(new DungeonEncounterRuntimeData(_eliteEncounterId, 3, _eliteName, new[] { _activeMonsters[_activeMonsters.Count - 1].MonsterId }, true));

        BuildPlannedRoomSequence();
        EnsureDungeonVisuals();
        SyncMonsterVisuals();
    }
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
        _selectedRouteDescription = template.Description;
        _selectedRouteRiskLabel = template.RiskLabel;
        _followedRecommendation = template.RouteId == _recommendedRouteId;
        _hoverRouteChoiceId = string.Empty;
        _hoverEventChoiceId = string.Empty;
        _selectedEventChoiceId = string.Empty;
        _resultEventChoiceText = "None";
        _hoverPreEliteChoiceId = string.Empty;
        _selectedPreEliteChoiceId = string.Empty;
        _resultPreEliteChoiceText = "Pending";
        _resultSurvivingMembersText = "None";
        _resultPartyHpSummaryText = "None";
        _resultPartyConditionText = "None";
        _resultRoomPathSummaryText = "None";
        _battleFeedbackText = "None";
        _runResultState = RunResultState.Playing;
        _dungeonRunState = DungeonRunState.Explore;
        _battleState = BattleState.None;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        _battleTurnIndex = 0;
        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        _runTurnCount = 0;
        _carriedLootAmount = 0;
        _battleLootAmount = 0;
        _chestLootAmount = 0;
        _eventLootAmount = 0;
        _totalEventHealAmount = 0;
        _preEliteHealAmount = 0;
        _resultPreEliteHealAmount = 0;
        _eliteBonusRewardPending = 0;
        _eliteBonusRewardGrantedAmount = 0;
        _resultEliteBonusRewardAmount = 0;
        _eliteRewardGranted = false;
        _pendingEnemyUsedSpecialAttack = false;
        _currentRoomIndex = 1;
        _clearedEncounterCount = 0;
        _chestOpenedCount = 0;
        _resultTurnsTaken = 0;
        _resultLootGained = 0;
        _resultBattleLootGained = 0;
        _resultChestLootGained = 0;
        _resultEventLootGained = 0;
        _resultClearedEncounters = 0;
        _resultOpenedChests = 0;
        _resultEliteRewardLabel = "None";
        _preRunManaShardStock = 0;
        _resultStockBefore = 0;
        _resultStockAfter = 0;
        _resultStockDelta = 0;
        _preRunConsecutiveDispatchCount = 0;
        _resultConsecutiveDispatchBefore = 0;
        _resultConsecutiveDispatchAfter = 0;
        _resultRecoveryEtaDays = 0;
        _resultNeedPressureBeforeText = "None";
        _resultNeedPressureAfterText = "None";
        _resultDispatchReadinessBeforeText = "None";
        _resultDispatchReadinessAfterText = "None";
        _enemyTurnMonsterCursor = -1;
        _pendingEnemyTargetIndex = -1;
        _exitUnlocked = false;
        _pendingDungeonExit = false;
        _eventResolved = false;
        _preEliteDecisionResolved = false;
        _eliteBonusRewardGranted = false;
        _preRunManaShardStock = GetCityManaShardStock(_currentHomeCityId);
        _preRunNeedPressureText = GetNeedPressureTextForStock(_preRunManaShardStock);
        _preRunDispatchReadinessText = GetDispatchReadinessText(_currentHomeCityId);
        _preRunConsecutiveDispatchCount = GetConsecutiveDispatchCount(_currentHomeCityId);
        ApplyDispatchReadinessOnDispatch(_currentHomeCityId);
        _playerGridPosition = new Vector2Int(2, 4);
        _exitGridPosition = new Vector2Int(14, 6);
        _eventGridPosition = ShrineEventGridPosition;
        ClearBattleHoverState();
        ClearBattleInputLock();
        ClearEnemyIntentState();
        ClearBattleFloatingPopups();
        ResetRoundActions();
        BuildFixedDungeonRoom();
        _roomPathHistory.Clear();
        AppendRoomPathLabel("Start Room");
        RefreshRoomSequenceState(true);
        RefreshExpectedNeedImpact();
        _recentBattleLogs.Clear();
        AppendBattleLog("Dispatched from " + GetHomeCityDisplayName() + " to " + _currentDungeonName + ".");
        AppendBattleLog("Selected " + _selectedRouteLabel + ". Room plan: " + BuildRoomPathPreviewText(_currentDungeonId, _selectedRouteId) + ".");
        AppendBattleLog("Need Pressure: " + _preRunNeedPressureText + " | Stock: " + BuildLootAmountText(_preRunManaShardStock) + " | Readiness: " + _preRunDispatchReadinessText + " -> " + GetDispatchReadinessText(_currentHomeCityId) + ".");
        SetBattleFeedbackText(GetHomeCityDisplayName() + " -> " + _currentDungeonName + " via " + _selectedRouteLabel + ".");
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private void FinishDungeonRun(RunResultState resultState, BattleState battleState, bool success, int lootReturned, string resultSummary)
    {
        if (!success && _eliteBonusRewardPending > 0 && !_eliteBonusRewardGranted)
        {
            AppendBattleLog("Pending elite bonus reward was lost.");
            _eliteBonusRewardPending = 0;
        }

        int safeReturnedLoot = success ? Mathf.Max(0, lootReturned) : 0;
        _resultTurnsTaken = _runTurnCount;
        _resultLootGained = safeReturnedLoot;
        _resultBattleLootGained = success ? _battleLootAmount : 0;
        _resultChestLootGained = success ? _chestLootAmount : 0;
        _resultEventLootGained = success ? _eventLootAmount : 0;
        _resultSurvivingMembersText = BuildSurvivingMembersSummary();
        _resultPartyHpSummaryText = BuildTotalPartyHpSummary();
        _resultPartyConditionText = GetPartyConditionText();
        _resultEventChoiceText = GetSelectedEventChoiceDisplayText();
        _resultPreEliteChoiceText = GetSelectedPreEliteChoiceDisplayText();
        _resultPreEliteHealAmount = _preEliteHealAmount;
        _resultClearedEncounters = _clearedEncounterCount;
        _resultOpenedChests = _chestOpenedCount;
        _resultEliteDefeated = _eliteDefeated;
        _resultEliteName = _eliteName;
        _resultEliteRewardLabel = _eliteRewardLabel;
        _resultEliteRewardAmount = _eliteRewardGranted ? _eliteRewardAmount : 0;
        _resultEliteBonusRewardAmount = _eliteBonusRewardGranted ? _eliteBonusRewardGrantedAmount : 0;
        _resultRoomPathSummaryText = BuildCurrentRoomPathSummary();
        _runResultState = resultState;
        _battleState = battleState;
        _dungeonRunState = DungeonRunState.ResultPanel;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        _carriedLootAmount = safeReturnedLoot;
        ClearBattleHoverState();
        ClearBattleInputLock();
        ClearEnemyIntentState();
        ClearBattleFloatingPopups();
        AppendBattleLog(string.IsNullOrEmpty(resultSummary) ? "The run ended." : resultSummary);
        AppendBattleLog("Party Condition at end: " + _resultPartyConditionText + " | HP: " + _resultPartyHpSummaryText + ".");
        if (_eliteRewardGranted && _resultEliteRewardAmount > 0)
        {
            AppendBattleLog("Elite reward granted: " + _resultEliteRewardLabel + " (" + BuildLootAmountText(_resultEliteRewardAmount) + ").");
        }
        SetBattleFeedbackText(resultState == RunResultState.Clear ? "Run clear." : resultState == RunResultState.Defeat ? "The party was defeated." : resultState == RunResultState.Retreat ? "The party retreated." : "Run complete.");

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
        _resultConsecutiveDispatchBefore = _preRunConsecutiveDispatchCount;
        _resultConsecutiveDispatchAfter = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetConsecutiveDispatchCount(_currentHomeCityId);
        _resultRecoveryEtaDays = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetRecoveryDaysToReady(_currentHomeCityId);

        if (!string.IsNullOrEmpty(_currentHomeCityId))
        {
            _lastDispatchStockDeltaByCityId[_currentHomeCityId] = BuildSignedLootAmountText(_resultStockDelta);
            _lastNeedPressureChangeByCityId[_currentHomeCityId] = BuildNeedPressureChangeSummary(_resultNeedPressureBeforeText, _resultNeedPressureAfterText);
            _lastDispatchReadinessChangeByCityId[_currentHomeCityId] = BuildDispatchReadinessChangeSummary(_resultDispatchReadinessBeforeText, _resultDispatchReadinessAfterText) + " | Streak " + _resultConsecutiveDispatchBefore + " -> " + _resultConsecutiveDispatchAfter;
            _lastDispatchImpactByCityId[_currentHomeCityId] = BuildLastDispatchImpactSummary(_resultStockBefore, _resultStockAfter, _resultStockDelta, _resultNeedPressureBeforeText, _resultNeedPressureAfterText);
        }

        AppendBattleLog("Returned " + BuildLootAmountText(safeReturnedLoot) + ".");
        AppendBattleLog("Need Pressure " + _resultNeedPressureBeforeText + " -> " + _resultNeedPressureAfterText + ".");
        AppendBattleLog("Dispatch Readiness " + _resultDispatchReadinessBeforeText + " -> " + _resultDispatchReadinessAfterText + " | ETA: " + BuildRecoveryEtaText(_resultRecoveryEtaDays) + ".");
        if (_eliteBonusRewardGranted && _eliteBonusRewardGrantedAmount > 0)
        {
            AppendBattleLog("Elite bonus reward earned: " + BuildLootAmountText(_eliteBonusRewardGrantedAmount) + ".");
        }
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private void ResetDungeonRunPresentationState()
    {
        _activeDungeonParty = null;
        _activeChest = null;
        _activeBattleMonster = null;
        _currentDungeonId = string.Empty;
        _currentDungeonName = "None";
        _currentHomeCityId = string.Empty;
        _activeEncounterId = string.Empty;
        _activeBattleMonsterId = string.Empty;
        _hoverBattleMonsterId = string.Empty;
        _battleFeedbackText = "None";
        _enemyIntentText = "None";
        _playerGridPosition = new Vector2Int(2, 4);
        _exitGridPosition = new Vector2Int(14, 4);
        _eventGridPosition = ShrineEventGridPosition;
        _currentSelectionPrompt = "None";
        _resultSurvivingMembersText = "None";
        _resultPartyHpSummaryText = "None";
        _resultPartyConditionText = "None";
        _hoverRouteChoiceId = string.Empty;
        _selectedRouteChoiceId = string.Empty;
        _selectedRouteId = string.Empty;
        _selectedRouteLabel = "None";
        _selectedRouteDescription = "None";
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
        _currentRoomStepId = string.Empty;
        _resultRoomPathSummaryText = "None";
        _eliteEncounterId = string.Empty;
        _eliteName = "None";
        _eliteType = "None";
        _eliteRewardLabel = "None";
        _resultEliteName = "None";
        _resultEliteRewardLabel = "None";
        _dungeonRunState = DungeonRunState.None;
        _battleState = BattleState.None;
        _runResultState = RunResultState.None;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        _battleTurnIndex = 0;
        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        _runTurnCount = 0;
        _carriedLootAmount = 0;
        _battleLootAmount = 0;
        _chestLootAmount = 0;
        _eventLootAmount = 0;
        _totalEventHealAmount = 0;
        _preEliteHealAmount = 0;
        _resultPreEliteHealAmount = 0;
        _eliteBonusRewardPending = 0;
        _eliteBonusRewardGrantedAmount = 0;
        _resultEliteBonusRewardAmount = 0;
        _currentRoomIndex = 1;
        _clearedEncounterCount = 0;
        _chestOpenedCount = 0;
        _resultTurnsTaken = 0;
        _resultLootGained = 0;
        _resultBattleLootGained = 0;
        _resultChestLootGained = 0;
        _resultEventLootGained = 0;
        _resultClearedEncounters = 0;
        _resultOpenedChests = 0;
        _preRunManaShardStock = 0;
        _resultStockBefore = 0;
        _resultStockAfter = 0;
        _resultStockDelta = 0;
        _preRunConsecutiveDispatchCount = 0;
        _resultConsecutiveDispatchBefore = 0;
        _resultConsecutiveDispatchAfter = 0;
        _resultRecoveryEtaDays = 0;
        _enemyTurnMonsterCursor = -1;
        _pendingEnemyTargetIndex = -1;
        _eliteRewardAmount = 0;
        _resultEliteRewardAmount = 0;
        _pendingEnemyUsedSpecialAttack = false;
        _eliteRewardGranted = false;
        _pendingEnemyAttackPower = 0;
        _exitUnlocked = false;
        _pendingDungeonExit = false;
        _eventResolved = false;
        _preEliteDecisionResolved = false;
        _eliteEncounterActive = false;
        _eliteDefeated = false;
        _resultEliteDefeated = false;
        _eliteBonusRewardGranted = false;
        _enemyIntentResolveAtTime = 0f;
        _enemyIntentTelegraphActive = false;
        _followedRecommendation = false;
        _plannedRooms.Clear();
        _roomPathHistory.Clear();
        _activeMonsters.Clear();
        _activeEncounters.Clear();
        ClearMonsterVisuals();
        ClearBattleInputLock();
        ClearBattleFloatingPopups();
        for (int i = 0; i < _partyFeedbackUntilTimes.Length; i++)
        {
            _partyFeedbackUntilTimes[i] = 0f;
            _partyFeedbackColors[i] = Color.white;
        }

        for (int i = 0; i < _monsterFeedbackUntilTimes.Length; i++)
        {
            _monsterFeedbackUntilTimes[i] = 0f;
            _monsterFeedbackColors[i] = Color.white;
        }

        if (_battleViewRoot != null)
        {
            _battleViewRoot.SetActive(false);
        }

        ResetRoundActions();
    }

    private bool TrySelectNextPartyActor(int startIndex)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            _currentActorIndex = -1;
            return false;
        }

        int firstIndex = Mathf.Max(0, startIndex);
        for (int i = firstIndex; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null || member.IsDefeated || member.CurrentHp <= 0 || _partyActedThisRound[i])
            {
                continue;
            }

            _currentActorIndex = i;
            _selectedPartyMemberIndex = i;
            return true;
        }

        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        return false;
    }

    private void ResetRoundActions()
    {
        for (int i = 0; i < _partyActedThisRound.Length; i++)
        {
            _partyActedThisRound[i] = false;
        }
    }
    private void RefreshDungeonPresentation()
    {
        EnsureDungeonVisuals();
        if (_dungeonRoot == null)
        {
            return;
        }

        _dungeonRoot.SetActive(IsDungeonRunActive);
        if (!IsDungeonRunActive)
        {
            RefreshBattleViewPresentation();
            return;
        }

        DungeonRoomTemplateData currentRoom = GetCurrentPlannedRoomStep();
        _currentRoomIndex = currentRoom != null ? Mathf.Max(1, GetCurrentPlannedRoomIndex() + 1) : Mathf.Max(1, _currentRoomIndex);

        for (int x = 0; x < DungeonGridWidth; x++)
        {
            for (int y = 0; y < DungeonGridHeight; y++)
            {
                SpriteRenderer tile = _dungeonTileRenderers[x, y];
                if (tile == null)
                {
                    continue;
                }

                Vector2Int grid = new Vector2Int(x, y);
                bool walkable = IsWithinDungeonGrid(grid);
                Color color = walkable ? new Color(0.15f, 0.20f, 0.29f, 1f) : new Color(0.08f, 0.11f, 0.16f, 1f);

                bool inRoom1 = x >= 1 && x <= 6 && y >= 1 && y <= 7;
                bool inCorridor = x >= 7 && x <= 10 && y >= 3 && y <= 5;
                bool inRoom2 = x >= 11 && x <= 15 && y >= 1 && y <= 7;
                if (inRoom1) color = new Color(0.17f, 0.22f, 0.31f, 1f);
                else if (inCorridor) color = new Color(0.20f, 0.24f, 0.31f, 1f);
                else if (inRoom2) color = new Color(0.16f, 0.21f, 0.29f, 1f);

                if (currentRoom != null && grid == currentRoom.MarkerPosition)
                {
                    color = currentRoom.RoomType == DungeonRoomType.Cache
                        ? new Color(0.36f, 0.29f, 0.12f, 1f)
                        : currentRoom.RoomType == DungeonRoomType.Shrine
                            ? new Color(0.20f, 0.31f, 0.22f, 1f)
                            : currentRoom.RoomType == DungeonRoomType.Preparation
                                ? new Color(0.31f, 0.22f, 0.38f, 1f)
                                : currentRoom.RoomType == DungeonRoomType.Elite
                                    ? new Color(0.35f, 0.16f, 0.16f, 1f)
                                    : new Color(0.21f, 0.25f, 0.38f, 1f);
                }

                if (_exitUnlocked && grid == _exitGridPosition)
                {
                    color = new Color(0.47f, 0.38f, 0.12f, 1f);
                }

                tile.color = color;
            }
        }

        if (_playerToken != null)
        {
            _playerToken.SetActive(_dungeonRunState != DungeonRunState.Battle);
            _playerToken.transform.localPosition = GetGridLocalPosition(_playerGridPosition);
        }
        if (_playerRenderer != null) _playerRenderer.color = new Color(0.74f, 0.50f, 0.29f, 1f);

        if (_exitToken != null)
        {
            _exitToken.SetActive(_dungeonRunState != DungeonRunState.Battle);
            _exitToken.transform.localPosition = GetGridLocalPosition(_exitGridPosition);
        }
        if (_exitRenderer != null) _exitRenderer.color = _exitUnlocked ? new Color(0.86f, 0.76f, 0.35f, 1f) : new Color(0.30f, 0.28f, 0.21f, 1f);

        if (_activeChest != null && _chestToken != null)
        {
            bool chestVisible = _dungeonRunState != DungeonRunState.Battle && currentRoom != null && currentRoom.RoomType == DungeonRoomType.Cache && !_activeChest.IsOpened;
            _activeChest.GridPosition = currentRoom != null && currentRoom.RoomType == DungeonRoomType.Cache ? currentRoom.MarkerPosition : _activeChest.GridPosition;
            _chestToken.SetActive(chestVisible);
            _chestToken.transform.localPosition = GetGridLocalPosition(_activeChest.GridPosition);
            if (_chestRenderer != null)
            {
                _chestRenderer.color = chestVisible ? new Color(0.86f, 0.74f, 0.28f, 1f) : new Color(0.34f, 0.31f, 0.24f, 0.4f);
            }
        }

        if (_eventToken != null)
        {
            bool shrineVisible = _dungeonRunState != DungeonRunState.Battle && currentRoom != null && currentRoom.RoomType == DungeonRoomType.Shrine && !_eventResolved;
            bool preparationVisible = _dungeonRunState != DungeonRunState.Battle && currentRoom != null && currentRoom.RoomType == DungeonRoomType.Preparation && !_preEliteDecisionResolved;
            bool eventVisible = shrineVisible || preparationVisible;
            if (eventVisible)
            {
                _eventGridPosition = currentRoom.MarkerPosition;
            }

            _eventToken.SetActive(eventVisible);
            _eventToken.transform.localPosition = GetGridLocalPosition(_eventGridPosition);
        }
        if (_eventRenderer != null)
        {
            _eventRenderer.color = currentRoom != null && currentRoom.RoomType == DungeonRoomType.Preparation
                ? (_preEliteDecisionResolved ? new Color(0.39f, 0.30f, 0.44f, 0.42f) : new Color(0.74f, 0.58f, 0.88f, 1f))
                : _eventResolved
                    ? new Color(0.25f, 0.32f, 0.24f, 0.4f)
                    : new Color(0.46f, 0.86f, 0.58f, 1f);
        }

        SyncMonsterVisuals();
        RefreshBattleViewPresentation();
    }
    private void RefreshBattleViewPresentation()
    {
        bool battleActive = _dungeonRunState == DungeonRunState.Battle;
        if (_battleViewRoot != null)
        {
            _battleViewRoot.SetActive(battleActive);
        }

        if (!battleActive)
        {
            return;
        }

        bool eliteActive = _eliteEncounterActive || (_activeBattleMonster != null && _activeBattleMonster.IsElite);
        Color backdropColor = eliteActive ? new Color(0.10f, 0.06f, 0.08f, 0.96f) : new Color(0.10f, 0.13f, 0.18f, 0.94f);
        Color headerColor = eliteActive ? new Color(0.34f, 0.18f, 0.18f, 0.96f) : new Color(0.17f, 0.26f, 0.35f, 0.96f);
        Color stageColor = eliteActive ? new Color(0.24f, 0.12f, 0.13f, 0.94f) : new Color(0.15f, 0.21f, 0.28f, 0.92f);
        if (_battleBackdropRenderer != null) _battleBackdropRenderer.color = backdropColor;
        if (_battleHeaderRenderer != null) _battleHeaderRenderer.color = headerColor;
        if (_battleStageRenderer != null) _battleStageRenderer.color = stageColor;
        if (_battleCommandPanelRenderer != null) _battleCommandPanelRenderer.color = new Color(0.08f, 0.10f, 0.13f, 0.95f);
        if (_battlePartyPanelRenderer != null) _battlePartyPanelRenderer.color = new Color(0.08f, 0.10f, 0.13f, 0.95f);
        if (_battleLogPanelRenderer != null) _battleLogPanelRenderer.color = new Color(0.08f, 0.10f, 0.13f, 0.95f);

        for (int i = 0; i < _battlePartyViewRenderers.Length; i++)
        {
            SpriteRenderer plateRenderer = _battlePartyPlateRenderers[i];
            SpriteRenderer viewRenderer = _battlePartyViewRenderers[i];
            DungeonPartyMemberRuntimeData member = _activeDungeonParty != null && i < _activeDungeonParty.Members.Length ? _activeDungeonParty.Members[i] : null;
            bool visible = member != null;
            if (plateRenderer != null)
            {
                plateRenderer.gameObject.SetActive(visible);
            }
            if (viewRenderer != null)
            {
                viewRenderer.gameObject.SetActive(visible);
            }
            if (!visible)
            {
                continue;
            }

            Vector3 position = BattlePartyViewPositions[i];
            if (plateRenderer != null)
            {
                plateRenderer.transform.localPosition = position + new Vector3(0f, -0.05f, 0f);
                plateRenderer.transform.localScale = new Vector3(1.95f, 1.55f, 1f);
                bool isCurrentActor = _battleState != BattleState.EnemyTurn && _currentActorIndex == i && !member.IsDefeated;
                Color plateColor = isCurrentActor ? new Color(0.33f, 0.46f, 0.61f, 0.94f) : new Color(0.18f, 0.22f, 0.30f, 0.80f);
                if (member.IsDefeated)
                {
                    plateColor = new Color(0.14f, 0.14f, 0.14f, 0.52f);
                }
                plateRenderer.color = plateColor;
            }

            if (viewRenderer != null)
            {
                viewRenderer.transform.localPosition = position;
                bool isCurrentActor = _battleState != BattleState.EnemyTurn && _currentActorIndex == i && !member.IsDefeated;
                float scale = member.IsDefeated ? 1.12f : isCurrentActor ? 1.30f : 1.18f;
                viewRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                Color viewColor = i == 0 ? new Color(0.73f, 0.50f, 0.29f, 1f)
                    : i == 1 ? new Color(0.36f, 0.69f, 0.60f, 1f)
                    : i == 2 ? new Color(0.42f, 0.68f, 0.84f, 1f)
                    : new Color(0.84f, 0.79f, 0.47f, 1f);
                if (_partyFeedbackUntilTimes[i] > Time.unscaledTime)
                {
                    viewColor = _partyFeedbackColors[i];
                }
                if (member.IsDefeated)
                {
                    viewColor = new Color(0.36f, 0.36f, 0.36f, 0.38f);
                }
                viewRenderer.color = viewColor;
            }
        }

        for (int i = 0; i < _battleMonsterViewRenderers.Length; i++)
        {
            SpriteRenderer plateRenderer = _battleMonsterPlateRenderers[i];
            SpriteRenderer viewRenderer = _battleMonsterViewRenderers[i];
            DungeonMonsterRuntimeData monster = GetBattleMonsterAtDisplayIndex(i);
            bool visible = monster != null;
            if (plateRenderer != null)
            {
                plateRenderer.gameObject.SetActive(visible);
            }
            if (viewRenderer != null)
            {
                viewRenderer.gameObject.SetActive(visible);
            }
            if (!visible)
            {
                continue;
            }

            Vector3 position = BattleMonsterViewPositions[i];
            if (plateRenderer != null)
            {
                plateRenderer.transform.localPosition = position + new Vector3(0f, -0.08f, 0f);
                plateRenderer.transform.localScale = monster.IsElite ? new Vector3(2.45f, 1.90f, 1f) : new Vector3(2.10f, 1.60f, 1f);
                Color plateColor = monster.IsElite ? new Color(0.46f, 0.18f, 0.18f, 0.92f) : new Color(0.22f, 0.25f, 0.31f, 0.82f);
                if (monster.IsDefeated)
                {
                    plateColor = new Color(0.14f, 0.14f, 0.14f, 0.48f);
                }
                else if (_battleState == BattleState.PartyTargetSelect && _activeBattleMonsterId == monster.MonsterId)
                {
                    plateColor = new Color(0.64f, 0.32f, 0.22f, 0.94f);
                }
                else if (_hoverBattleMonsterId == monster.MonsterId)
                {
                    plateColor = new Color(0.46f, 0.33f, 0.24f, 0.90f);
                }
                else if (_battleState == BattleState.EnemyTurn && _activeBattleMonsterId == monster.MonsterId)
                {
                    plateColor = new Color(0.50f, 0.24f, 0.24f, 0.94f);
                }
                plateRenderer.color = plateColor;
            }

            if (viewRenderer != null)
            {
                viewRenderer.transform.localPosition = position;
                bool isSelected = _battleState == BattleState.PartyTargetSelect && _activeBattleMonsterId == monster.MonsterId && !monster.IsDefeated;
                bool isHovered = _hoverBattleMonsterId == monster.MonsterId && !monster.IsDefeated;
                bool isActing = _battleState == BattleState.EnemyTurn && _activeBattleMonsterId == monster.MonsterId && !monster.IsDefeated;
                float scale = monster.IsElite ? 1.55f : 1.28f;
                if (isSelected) scale += 0.16f;
                else if (isHovered || isActing) scale += 0.10f;
                if (monster.IsDefeated) scale = monster.IsElite ? 1.30f : 1.12f;
                viewRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                viewRenderer.color = GetBattleMonsterViewColor(monster, i);
            }
        }
    }

    private void ClearMonsterVisuals()
    {
        foreach (KeyValuePair<string, SpriteRenderer> pair in _monsterRendererById)
        {
            if (pair.Value != null)
            {
                Object.Destroy(pair.Value.gameObject);
            }
        }

        _monsterRendererById.Clear();
    }

    private void SyncMonsterVisuals()
    {
        EnsureDungeonVisuals();
        DungeonRoomTemplateData currentRoom = GetCurrentPlannedRoomStep();
        bool isBattle = _dungeonRunState == DungeonRunState.Battle;

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster == null)
            {
                continue;
            }

            if (!_monsterRendererById.TryGetValue(monster.MonsterId, out SpriteRenderer renderer) || renderer == null)
            {
                GameObject token = CreateDungeonToken(_dungeonRoot.transform, "Monster_" + monster.MonsterId, GetGridLocalPosition(monster.GridPosition), new Vector2(0.9f, 0.9f), 38);
                renderer = token != null ? token.GetComponent<SpriteRenderer>() : null;
                _monsterRendererById[monster.MonsterId] = renderer;
            }

            if (renderer == null)
            {
                continue;
            }

            bool visible = false;
            if (!isBattle && currentRoom != null)
            {
                if (currentRoom.RoomType == DungeonRoomType.Skirmish && currentRoom.EncounterId == monster.EncounterId)
                {
                    visible = !monster.IsDefeated;
                }
                else if (currentRoom.RoomType == DungeonRoomType.Elite && monster.EncounterId == _eliteEncounterId)
                {
                    visible = !monster.IsDefeated;
                }
            }

            renderer.gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            renderer.transform.localPosition = GetGridLocalPosition(monster.GridPosition);
            renderer.transform.localScale = monster.IsElite ? new Vector3(1.18f, 1.18f, 1f) : new Vector3(0.92f, 0.92f, 1f);
            renderer.color = GetMonsterBaseColor(monster);
        }
    }

    private void EnsureDungeonVisuals()
    {
        if (_dungeonRoot != null)
        {
            return;
        }

        _dungeonTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _dungeonTexture.name = "DungeonRunTexture";
        _dungeonTexture.filterMode = FilterMode.Point;
        _dungeonTexture.wrapMode = TextureWrapMode.Clamp;
        _dungeonTexture.SetPixel(0, 0, Color.white);
        _dungeonTexture.Apply();

        _dungeonSprite = Sprite.Create(_dungeonTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        _dungeonSprite.name = "DungeonRunSprite";

        _dungeonRoot = new GameObject("DungeonRunRoot");
        _dungeonTileRenderers = new SpriteRenderer[DungeonGridWidth, DungeonGridHeight];
        for (int x = 0; x < DungeonGridWidth; x++)
        {
            for (int y = 0; y < DungeonGridHeight; y++)
            {
                GameObject tile = CreateDungeonToken(_dungeonRoot.transform, "Tile_" + x + "_" + y, GetGridLocalPosition(new Vector2Int(x, y)), new Vector2(0.94f, 0.94f), 0);
                _dungeonTileRenderers[x, y] = tile != null ? tile.GetComponent<SpriteRenderer>() : null;
            }
        }

        _playerToken = CreateDungeonToken(_dungeonRoot.transform, "PlayerToken", GetGridLocalPosition(_playerGridPosition), new Vector2(0.92f, 0.92f), 40);
        _playerRenderer = _playerToken != null ? _playerToken.GetComponent<SpriteRenderer>() : null;
        _exitToken = CreateDungeonToken(_dungeonRoot.transform, "ExitToken", GetGridLocalPosition(_exitGridPosition), new Vector2(0.95f, 0.95f), 39);
        _exitRenderer = _exitToken != null ? _exitToken.GetComponent<SpriteRenderer>() : null;
        _chestToken = CreateDungeonToken(_dungeonRoot.transform, "ChestToken", GetGridLocalPosition(Room2ChestGridPosition), new Vector2(0.88f, 0.88f), 39);
        _chestRenderer = _chestToken != null ? _chestToken.GetComponent<SpriteRenderer>() : null;
        _eventToken = CreateDungeonToken(_dungeonRoot.transform, "EventToken", GetGridLocalPosition(_eventGridPosition), new Vector2(0.88f, 0.88f), 39);
        _eventRenderer = _eventToken != null ? _eventToken.GetComponent<SpriteRenderer>() : null;

        _battleViewRoot = new GameObject("BattleViewRoot");
        _battleViewRoot.transform.SetParent(_dungeonRoot.transform, false);
        _battleBackdropRenderer = CreateBattleVisual(_battleViewRoot.transform, "Backdrop", new Vector3(0f, 0f, 8f), new Vector2(17.2f, 10.4f), 100).GetComponent<SpriteRenderer>();
        _battleHeaderRenderer = CreateBattleVisual(_battleViewRoot.transform, "Header", new Vector3(0f, 3.95f, 0f), new Vector2(17.2f, 1.05f), 101).GetComponent<SpriteRenderer>();
        _battleStageRenderer = CreateBattleVisual(_battleViewRoot.transform, "Stage", new Vector3(0f, 0.95f, 0f), new Vector2(15.8f, 5.9f), 102).GetComponent<SpriteRenderer>();
        _battleCommandPanelRenderer = CreateBattleVisual(_battleViewRoot.transform, "CommandPanel", new Vector3(-5.55f, -3.55f, 0f), new Vector2(5.0f, 2.15f), 103).GetComponent<SpriteRenderer>();
        _battlePartyPanelRenderer = CreateBattleVisual(_battleViewRoot.transform, "PartyPanel", new Vector3(0f, -3.55f, 0f), new Vector2(5.5f, 2.15f), 103).GetComponent<SpriteRenderer>();
        _battleLogPanelRenderer = CreateBattleVisual(_battleViewRoot.transform, "LogPanel", new Vector3(5.6f, -3.55f, 0f), new Vector2(5.2f, 2.15f), 103).GetComponent<SpriteRenderer>();

        for (int i = 0; i < _battleMonsterPlateRenderers.Length; i++)
        {
            GameObject plate = CreateBattleVisual(_battleViewRoot.transform, "MonsterPlate_" + i, BattleMonsterViewPositions[i], new Vector2(2.05f, 1.55f), 104);
            _battleMonsterPlateRenderers[i] = plate.GetComponent<SpriteRenderer>();
            GameObject view = CreateBattleVisual(_battleViewRoot.transform, "MonsterView_" + i, BattleMonsterViewPositions[i], new Vector2(1.15f, 1.15f), 105);
            _battleMonsterViewRenderers[i] = view.GetComponent<SpriteRenderer>();
        }

        for (int i = 0; i < _battlePartyPlateRenderers.Length; i++)
        {
            GameObject plate = CreateBattleVisual(_battleViewRoot.transform, "PartyPlate_" + i, BattlePartyViewPositions[i], new Vector2(1.85f, 1.45f), 104);
            _battlePartyPlateRenderers[i] = plate.GetComponent<SpriteRenderer>();
            GameObject view = CreateBattleVisual(_battleViewRoot.transform, "PartyView_" + i, BattlePartyViewPositions[i], new Vector2(1.05f, 1.05f), 105);
            _battlePartyViewRenderers[i] = view.GetComponent<SpriteRenderer>();
        }

        for (int i = 0; i < _battleFloatingPopups.Length; i++)
        {
            GameObject popupRoot = new GameObject("BattlePopup_" + i);
            popupRoot.transform.SetParent(_battleViewRoot.transform, false);
            popupRoot.SetActive(false);
            TextMesh textMesh = popupRoot.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.18f;
            textMesh.fontSize = 48;
            textMesh.color = Color.white;
            MeshRenderer meshRenderer = popupRoot.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 110;
            }
            _battleFloatingPopups[i] = new BattleFloatingPopup { Root = popupRoot, TextMesh = textMesh };
        }

        _battleViewRoot.SetActive(false);
    }

    private Color GetBattleMonsterViewColor(DungeonMonsterRuntimeData monster, int displayIndex)
    {
        if (monster == null)
        {
            return new Color(0.30f, 0.30f, 0.30f, 0f);
        }

        if (monster.IsDefeated)
        {
            return new Color(0.34f, 0.34f, 0.34f, 0.34f);
        }

        Color color = GetMonsterBaseColor(monster);
        if (displayIndex >= 0 && displayIndex < _monsterFeedbackUntilTimes.Length && _monsterFeedbackUntilTimes[displayIndex] > Time.unscaledTime)
        {
            color = _monsterFeedbackColors[displayIndex];
        }
        else if (_battleState == BattleState.EnemyTurn && _activeBattleMonsterId == monster.MonsterId)
        {
            color = Color.Lerp(color, Color.white, 0.28f);
        }
        else if (_battleState == BattleState.PartyTargetSelect && _activeBattleMonsterId == monster.MonsterId)
        {
            color = Color.Lerp(color, new Color(1f, 0.70f, 0.46f, 1f), 0.40f);
        }
        else if (_hoverBattleMonsterId == monster.MonsterId)
        {
            color = Color.Lerp(color, Color.white, 0.18f);
        }

        return color;
    }

    private Color GetMonsterBaseColor(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return Color.white;
        }

        if (monster.IsElite)
        {
            return monster.MonsterType == "Goblin"
                ? new Color(0.76f, 0.34f, 0.28f, 1f)
                : new Color(0.58f, 0.34f, 0.76f, 1f);
        }

        if (monster.EncounterRole == MonsterEncounterRole.Bulwark)
        {
            return monster.MonsterType == "Goblin"
                ? new Color(0.38f, 0.61f, 0.29f, 1f)
                : new Color(0.40f, 0.76f, 0.52f, 1f);
        }

        if (monster.EncounterRole == MonsterEncounterRole.Striker)
        {
            return monster.MonsterType == "Goblin"
                ? new Color(0.76f, 0.44f, 0.30f, 1f)
                : new Color(0.34f, 0.67f, 0.88f, 1f);
        }

        return monster.MonsterType == "Goblin"
            ? new Color(0.44f, 0.74f, 0.36f, 1f)
            : new Color(0.31f, 0.82f, 0.67f, 1f);
    }

    private Vector3 GetGridLocalPosition(Vector2Int gridPosition)
    {
        return new Vector3(-6.4f + (gridPosition.x * 0.8f), -3.2f + (gridPosition.y * 0.8f), 0f);
    }

    private GameObject CreateDungeonToken(Transform parent, string name, Vector3 localPosition, Vector2 size, int sortingOrder)
    {
        GameObject token = new GameObject(name);
        token.transform.SetParent(parent, false);
        token.transform.localPosition = localPosition;
        token.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = token.AddComponent<SpriteRenderer>();
        renderer.sprite = _dungeonSprite;
        renderer.sortingOrder = sortingOrder;
        return token;
    }

    private GameObject CreateBattleVisual(Transform parent, string name, Vector3 localPosition, Vector2 size, int sortingOrder)
    {
        return CreateDungeonToken(parent, name, localPosition, size, sortingOrder);
    }

    private bool IsWithinDungeonGrid(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= DungeonGridWidth || gridPosition.y < 0 || gridPosition.y >= DungeonGridHeight)
        {
            return false;
        }

        bool inRoom1 = gridPosition.x >= 1 && gridPosition.x <= 6 && gridPosition.y >= 1 && gridPosition.y <= 7;
        bool inCorridor = gridPosition.x >= 7 && gridPosition.x <= 10 && gridPosition.y >= 3 && gridPosition.y <= 5;
        bool inRoom2 = gridPosition.x >= 11 && gridPosition.x <= 15 && gridPosition.y >= 1 && gridPosition.y <= 7;
        return inRoom1 || inCorridor || inRoom2;
    }

    private int GetRoomIndexForPosition(Vector2Int gridPosition)
    {
        if (gridPosition.x >= 11)
        {
            return 2;
        }

        return 1;
    }

}
