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
    private static readonly Vector2Int ShrineEventGridPosition = new Vector2Int(8, 5);
    private static readonly Vector2Int Room2EventGridPosition = new Vector2Int(9, 3);
    private static readonly Vector2Int Room2ChestGridPosition = new Vector2Int(10, 3);
    private static readonly Vector2Int Room1EncounterMarkerPosition = new Vector2Int(4, 4);
    private static readonly Vector2Int Room2EncounterMarkerPosition = new Vector2Int(13, 4);
    private static readonly Vector2Int PreparationRoomGridPosition = new Vector2Int(12, 6);
    private static readonly Vector2Int EliteEncounterMarkerPosition = new Vector2Int(13, 2);
    private static readonly Vector3[] BattleMonsterViewPositions =
    {
        new Vector3(4.55f, 0.72f, 0f),
        new Vector3(5.95f, 0.18f, 0f)
    };

    private static readonly Vector3[] BattlePartyViewPositions =
    {
        new Vector3(-6.15f, -0.88f, 0f),
        new Vector3(-4.55f, -0.88f, 0f),
        new Vector3(-2.95f, -0.88f, 0f),
        new Vector3(-1.35f, -0.88f, 0f)
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
        Move,
        EndTurn,
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
        "A damp restway with layered slime pressure and the cleanest recovery window in Alpha.",
        "Low",
        "Mud Slimes -> Watch Slimes -> Slime Monarch",
        "Rest shrine heavily favors the recover line.",
        "Lowest shard ceiling, safest build into the monarch.",
        14,
        8,
        3,
        2);

    private static readonly DungeonRouteTemplate AlphaRiskyRouteTemplate = new DungeonRouteTemplate(
        RiskyRouteId,
        "Standard Path",
        "A broken central push where slimes and goblins overlap before the unstable core.",
        "Medium",
        "Slime Scouts -> Goblin Pair -> Gel Core",
        "Unstable shrine keeps recover and loot nearly even.",
        "Sharper shard ceiling if the party can absorb mixed spikes.",
        17,
        6,
        5,
        3);

    private static readonly DungeonRouteTemplate BetaSafeRouteTemplate = new DungeonRouteTemplate(
        SafeRouteId,
        "Guarded Path",
        "A patrol-heavy watch route with steadier goblin pressure and a controlled captain setup.",
        "Medium",
        "Scout Pair -> Guarded Vault -> Goblin Captain",
        "Watchfire offers real recovery, but the cache line stays respectable.",
        "Stable haul with enough reserve left for the captain.",
        18,
        4,
        6,
        4);

    private static readonly DungeonRouteTemplate BetaRiskyRouteTemplate = new DungeonRouteTemplate(
        RiskyRouteId,
        "Greedy Path",
        "A raider vault line built around ambush bursts, stolen caches, and the chief at the exit.",
        "High",
        "Goblin Scouts -> Vault Ambush -> Raider Chief",
        "War-banner loot line is strongest; the recovery line is deliberately thin.",
        "Best shard haul in Beta if the party survives the burst damage.",
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
        public PrototypeRpgPartyMemberRuntimeState RuntimeState { get; }
        public string MemberId => RuntimeState != null ? RuntimeState.MemberId : string.Empty;
        public string DisplayName => RuntimeState != null ? RuntimeState.DisplayName : "Adventurer";
        public string RoleLabel => RuntimeState != null ? RuntimeState.RoleLabel : "Adventurer";
        public string RoleTag => RuntimeState != null ? RuntimeState.RoleTag : "adventurer";
        public string DefaultSkillId => RuntimeState != null ? RuntimeState.DefaultSkillId : string.Empty;
        public string EquipmentSummaryText => RuntimeState != null ? RuntimeState.EquipmentSummaryText : "No gear";
        public string AppliedProgressionSummaryText => RuntimeState != null ? RuntimeState.AppliedProgressionSummaryText : "No applied progression.";
        public string CurrentRunSummaryText => RuntimeState != null ? RuntimeState.CurrentRunSummaryText : "No current-run summary.";
        public string SkillName;
        public string SkillShortText;
        public PartySkillType SkillType;
        public int PartySlotIndex;
        public int MaxHp => RuntimeState != null ? RuntimeState.MaxHp : 1;
        public int CurrentHp
        {
            get => RuntimeState != null ? RuntimeState.CurrentHp : 0;
            set
            {
                if (RuntimeState != null)
                {
                    RuntimeState.SetCurrentHp(value);
                }
            }
        }

        public int Attack => RuntimeState != null ? RuntimeState.Attack : 1;
        public int Defense => RuntimeState != null ? RuntimeState.Defense : 0;
        public int Speed => RuntimeState != null ? RuntimeState.Speed : 0;
        public int SkillPower;
        public bool IsDefeated
        {
            get => RuntimeState != null && RuntimeState.IsKnockedOut;
            set
            {
                if (RuntimeState != null)
                {
                    RuntimeState.SetKnockedOut(value);
                }
            }
        }

        public Color ViewColor;

        public DungeonPartyMemberRuntimeData(PrototypeRpgPartyMemberRuntimeState runtimeState, string skillName, string skillShortText, PartySkillType skillType, int partySlotIndex, int skillPower, Color viewColor)
        {
            RuntimeState = runtimeState ?? new PrototypeRpgPartyMemberRuntimeState(string.Empty, "Adventurer", "adventurer", "Adventurer", string.Empty, 1, 1, 0, 0);
            SkillName = string.IsNullOrEmpty(skillName) ? "Skill" : skillName;
            SkillShortText = string.IsNullOrEmpty(skillShortText) ? string.Empty : skillShortText;
            SkillType = skillType;
            PartySlotIndex = partySlotIndex >= 0 ? partySlotIndex : 0;
            SkillPower = skillPower > 0 ? skillPower : Attack + 1;
            ViewColor = viewColor.a > 0f ? viewColor : Color.white;
        }

        public DungeonPartyMemberRuntimeData(string memberId, string displayName, string roleLabel, string roleTag, string defaultSkillId, string skillName, string skillShortText, PartySkillType skillType, int partySlotIndex, int maxHp, int attack, int defense, int speed, int skillPower, Color viewColor)
            : this(new PrototypeRpgPartyMemberRuntimeState(memberId, displayName, roleTag, roleLabel, defaultSkillId, maxHp, attack, defense, speed), skillName, skillShortText, skillType, partySlotIndex, skillPower, viewColor)
        {
        }

        public void ResetForRun()
        {
            RuntimeState?.ResetForRun();
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
        public PrototypeRpgEnemyRuntimeState RuntimeState { get; }
        public string MonsterId => RuntimeState != null ? RuntimeState.EnemyId : string.Empty;
        public string EncounterId => RuntimeState != null ? RuntimeState.EncounterId : string.Empty;
        public int RoomIndex;
        public string DisplayName => RuntimeState != null ? RuntimeState.DisplayName : "Monster";
        public string MonsterType => RuntimeState != null ? RuntimeState.TypeLabel : "Monster";
        public int MaxHp => RuntimeState != null ? RuntimeState.MaxHp : 1;
        public int CurrentHp
        {
            get => RuntimeState != null ? RuntimeState.CurrentHp : 0;
            set
            {
                if (RuntimeState != null)
                {
                    RuntimeState.SetCurrentHp(value);
                }
            }
        }
        public int Attack => RuntimeState != null ? RuntimeState.AttackPower : 1;
        public Vector2Int GridPosition;
        public bool IsDefeated
        {
            get => RuntimeState != null && RuntimeState.IsDefeated;
            set
            {
                if (RuntimeState != null)
                {
                    RuntimeState.SetDefeated(value);
                }
            }
        }
        public string IntentLabel
        {
            get => RuntimeState != null ? RuntimeState.IntentLabel : string.Empty;
            set
            {
                if (RuntimeState != null)
                {
                    RuntimeState.SetIntentLabel(value);
                }
            }
        }
        public string RewardResourceId;
        public int RewardAmount;
        public MonsterEncounterRole EncounterRole;
        public MonsterTargetPattern TargetPattern;
        public bool IsElite => RuntimeState != null && RuntimeState.IsElite;
        public int SpecialAttack;
        public string SpecialActionName;
        public int TurnsActed;

        public DungeonMonsterRuntimeData(string monsterId, string encounterId, int roomIndex, string displayName, string monsterType, int hp, int attack, Vector2Int gridPosition, string rewardResourceId, int rewardAmount, MonsterTargetPattern targetPattern, MonsterEncounterRole encounterRole = MonsterEncounterRole.Bulwark, bool isElite = false, int specialAttack = 0, string specialActionName = "")
        {
            RuntimeState = new PrototypeRpgEnemyRuntimeState(monsterId, encounterId, displayName, monsterType, hp, attack, isElite);
            RoomIndex = roomIndex;
            GridPosition = gridPosition;
            RewardResourceId = string.IsNullOrEmpty(rewardResourceId) ? DungeonRewardResourceId : rewardResourceId;
            RewardAmount = rewardAmount > 0 ? rewardAmount : 1;
            EncounterRole = encounterRole;
            TargetPattern = targetPattern;
            SpecialAttack = specialAttack > 0 ? specialAttack : Mathf.Max(Attack + 2, Attack);
            SpecialActionName = string.IsNullOrEmpty(specialActionName) ? "Heavy Strike" : specialActionName;
            TurnsActed = 0;
        }

        public void ResetForEncounter()
        {
            if (RuntimeState != null)
            {
                RuntimeState.ResetForEncounter();
            }

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
    private readonly List<PrototypeBattleEventRecord> _battleEventRecords = new List<PrototypeBattleEventRecord>(32);
    private PrototypeBattleResultSnapshot _currentBattleResultSnapshot = new PrototypeBattleResultSnapshot();
    private PrototypeEnemyIntentSnapshot _currentEnemyIntentSnapshot = new PrototypeEnemyIntentSnapshot();
    private PrototypeRpgRunResultSnapshot _latestRpgRunResultSnapshot = new PrototypeRpgRunResultSnapshot();
    private PrototypeRpgProgressionSeedSnapshot _latestRpgProgressionSeedSnapshot = new PrototypeRpgProgressionSeedSnapshot();
    private PrototypeRpgCombatContributionSnapshot _latestRpgCombatContributionSnapshot = new PrototypeRpgCombatContributionSnapshot();
    private PrototypeRpgProgressionPreviewSnapshot _latestRpgProgressionPreviewSnapshot = new PrototypeRpgProgressionPreviewSnapshot();
    private int _battleEventStepIndex = 0;
    private int _totalDamageDealt = 0;
    private int _totalDamageTaken = 0;
    private int _totalHealingDone = 0;
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
    private readonly int[] _runMemberDamageDealt = new int[4];
    private readonly int[] _runMemberDamageTaken = new int[4];
    private readonly int[] _runMemberHealingDone = new int[4];
    private readonly int[] _runMemberActionCount = new int[4];
    private readonly int[] _runMemberKillCount = new int[4];
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
    private int _battlePresentationStateStamp = int.MinValue;
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
    private int _battlesFoughtCount;
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
    public string RouteChoiceTitleText => "Legacy Dispatch Fallback";
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
    public PrototypeRpgRunResultSnapshot LatestRpgRunResultSnapshot => CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
    public PrototypeRpgProgressionSeedSnapshot LatestRpgProgressionSeedSnapshot => CopyRpgProgressionSeedSnapshot(_latestRpgProgressionSeedSnapshot);
    public PrototypeRpgCombatContributionSnapshot LatestRpgCombatContributionSnapshot => CopyRpgCombatContributionSnapshot(_latestRpgCombatContributionSnapshot);
    public PrototypeRpgProgressionPreviewSnapshot LatestRpgProgressionPreviewSnapshot => CopyRpgProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot);
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

    public string GetPartyMemberContributionText(int memberIndex)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || memberIndex < 0 || memberIndex >= _activeDungeonParty.Members.Length)
        {
            return "D 0  H 0  A 0  K 0";
        }

        return "D " + GetRunMemberContributionValue(_runMemberDamageDealt, memberIndex) +
               "  H " + GetRunMemberContributionValue(_runMemberHealingDone, memberIndex) +
               "  A " + GetRunMemberContributionValue(_runMemberActionCount, memberIndex) +
               "  K " + GetRunMemberContributionValue(_runMemberKillCount, memberIndex);
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

    public PrototypeBattleUiSurfaceData BuildBattleUiSurfaceData()
    {
        return BuildRpgOwnedBattleUiSurfaceData();
    }

    private PrototypeBattleUiActorData BuildBattleUiCurrentActorData()
    {
        return BuildRpgOwnedBattleUiCurrentActorData();
    }

    private PrototypeBattleUiTimelineData BuildBattleUiTimelineData()
    {
        return BuildRpgOwnedBattleUiTimelineData();
    }

    private PrototypeBattleUiTimelineSlotData CreateBattleUiTimelineSlot(string label, string secondaryLabel, bool isCurrent, bool isEnemy, bool isPending, string statusLabel)
    {
        return CreateRpgOwnedBattleUiTimelineSlot(label, secondaryLabel, isCurrent, isEnemy, isPending, statusLabel);
    }

    private void AppendBattleUiNextPartySlots(List<PrototypeBattleUiTimelineSlotData> slots, int anchorIndex, int maxCount)
    {
        AppendRpgOwnedBattleUiNextPartySlots(slots, anchorIndex, maxCount);
    }

    private DungeonPartyMemberRuntimeData GetPartyMemberAtIndex(int memberIndex)
    {
        return _activeDungeonParty != null && _activeDungeonParty.Members != null && memberIndex >= 0 && memberIndex < _activeDungeonParty.Members.Length
            ? _activeDungeonParty.Members[memberIndex]
            : null;
    }

    private PrototypeBattleUiPartyMemberData[] BuildBattleUiPartyMembers()
    {
        return BuildRpgOwnedBattleUiPartyMembers();
    }

    private PrototypeBattleUiPartyMemberData BuildBattleUiPartyMemberData(DungeonPartyMemberRuntimeData member, int memberIndex)
    {
        return BuildRpgOwnedBattleUiPartyMemberData(member, memberIndex);
    }

    private PrototypeBattleUiEnemyData BuildSelectedBattleUiEnemyData()
    {
        return BuildRpgOwnedSelectedBattleUiEnemyData();
    }

    private PrototypeBattleUiEnemyData[] BuildBattleUiEnemyRoster()
    {
        return BuildRpgOwnedBattleUiEnemyRoster();
    }

    private PrototypeBattleUiEnemyData BuildBattleUiEnemyData(DungeonMonsterRuntimeData monster)
    {
        return BuildRpgOwnedBattleUiEnemyData(monster);
    }

    private string BuildBattleUiEnemyIntentLabel(DungeonMonsterRuntimeData monster)
    {
        return BuildRpgOwnedBattleUiEnemyIntentLabel(monster);
    }

    private string BuildBattleUiEnemyTraitText(DungeonMonsterRuntimeData monster)
    {
        return BuildRpgOwnedBattleUiEnemyTraitText(monster);
    }

    private DungeonMonsterRuntimeData GetBattleUiFocusedMonster()
    {
        if (!string.IsNullOrEmpty(_hoverBattleMonsterId))
        {
            DungeonMonsterRuntimeData hoveredMonster = GetMonsterById(_hoverBattleMonsterId);
            if (hoveredMonster != null && !hoveredMonster.IsDefeated)
            {
                return hoveredMonster;
            }
        }

        if (!string.IsNullOrEmpty(_activeBattleMonsterId))
        {
            DungeonMonsterRuntimeData activeMonster = GetMonsterById(_activeBattleMonsterId);
            if (activeMonster != null && !activeMonster.IsDefeated)
            {
                return activeMonster;
            }
        }

        if (_activeBattleMonster != null && !_activeBattleMonster.IsDefeated)
        {
            return _activeBattleMonster;
        }

        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData encounterMonster = GetMonsterById(encounter.MonsterIds[i]);
                if (encounterMonster != null && !encounterMonster.IsDefeated)
                {
                    return encounterMonster;
                }
            }
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null && !monster.IsDefeated)
            {
                return monster;
            }
        }

        return _activeMonsters.Count > 0 ? _activeMonsters[0] : null;
    }

    private DungeonMonsterRuntimeData GetBattleUiPrimaryPreviewMonster()
    {
        DungeonMonsterRuntimeData focusedMonster = GetBattleUiFocusedMonster();
        if (focusedMonster != null)
        {
            return focusedMonster;
        }

        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData encounterMonster = GetMonsterById(encounter.MonsterIds[i]);
                if (encounterMonster != null)
                {
                    return encounterMonster;
                }
            }
        }

        return _activeMonsters.Count > 0 ? _activeMonsters[0] : null;
    }

    private PrototypeBattleUiCommandSurfaceData BuildBattleUiCommandSurfaceData(PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext)
    {
        return BuildRpgOwnedBattleUiCommandSurfaceData(actor, actionContext);
    }

    private PrototypeBattleUiCommandDetailData BuildBattleUiCommandDetailData(string key, string label, string description, string targetText, string costText, string effectText, bool isAvailable, bool isSelected)
    {
        PrototypeBattleUiCommandDetailData detail = new PrototypeBattleUiCommandDetailData();
        detail.Key = string.IsNullOrEmpty(key) ? string.Empty : key;
        detail.Label = string.IsNullOrEmpty(label) ? "None" : label;
        detail.Description = string.IsNullOrEmpty(description) ? string.Empty : description;
        detail.TargetText = string.IsNullOrEmpty(targetText) ? string.Empty : targetText;
        detail.CostText = string.IsNullOrEmpty(costText) ? string.Empty : costText;
        detail.EffectText = string.IsNullOrEmpty(effectText) ? string.Empty : effectText;
        detail.IsAvailable = isAvailable;
        detail.IsSelected = isSelected;
        return detail;
    }

    private PrototypeBattleUiMessageSurfaceData BuildBattleUiMessageSurfaceData()
    {
        return BuildRpgOwnedBattleUiMessageSurfaceData();
    }

    private PrototypeBattleUiTargetSelectionData BuildBattleUiTargetSelectionData(PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext, PrototypeBattleUiTargetContextData targetContext)
    {
        return BuildRpgOwnedBattleUiTargetSelectionData(actor, actionContext, targetContext);
    }

    private string GetBattleUiSelectedActionKey()
    {
        return _queuedBattleAction == BattleActionType.Attack
            ? "attack"
            : _queuedBattleAction == BattleActionType.Skill
                ? "skill"
                : _queuedBattleAction == BattleActionType.Move
                    ? "move"
                : _queuedBattleAction == BattleActionType.EndTurn
                    ? "end_turn"
                : _queuedBattleAction == BattleActionType.Retreat
                    ? "retreat"
                    : string.Empty;
    }

    private string GetBattleUiActionDisplayLabel(string actionKey, PrototypeBattleUiActorData actor)
    {
        if (actionKey == "attack")
        {
            return "Attack";
        }

        if (actionKey == "skill")
        {
            return actor != null && !string.IsNullOrEmpty(actor.SkillLabel) && actor.SkillLabel != "None"
                ? actor.SkillLabel
                : "Skill";
        }

        if (actionKey == "retreat")
        {
            return "Retreat";
        }

        if (actionKey == "move")
        {
            return "Move";
        }

        if (actionKey == "move_front")
        {
            return "Move to Front Row";
        }

        if (actionKey == "move_middle")
        {
            return "Move to Middle Row";
        }

        if (actionKey == "move_back")
        {
            return "Move to Back Row";
        }

        if (actionKey == "end_turn")
        {
            return "End Turn";
        }

        return "Action";
    }

    private string GetBattleStateKey()
    {
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            return "none";
        }

        switch (_battleState)
        {
            case BattleState.PartyActionSelect:
                return "party_action_select";
            case BattleState.PartyTargetSelect:
                return "party_target_select";
            case BattleState.EnemyTurn:
                return "enemy_turn";
            case BattleState.Victory:
                return "victory";
            case BattleState.Defeat:
                return "defeat";
            case BattleState.Retreat:
                return "retreat";
            default:
                return "none";
        }
    }

    private PrototypeBattleUiActionContextData BuildBattleUiActionContextData(PrototypeBattleUiActorData actor, PrototypeBattleUiEnemyData selectedEnemy)
    {
        return BuildRpgOwnedBattleUiActionContextData(actor, selectedEnemy);
    }

    private PrototypeBattleUiTargetContextData BuildBattleUiTargetContextData(PrototypeBattleUiEnemyData selectedEnemy)
    {
        return BuildRpgOwnedBattleUiTargetContextData(selectedEnemy);
    }

    private int GetBattleMonsterDisplayIndex(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            return -1;
        }

        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                if (encounter.MonsterIds[i] == monsterId)
                {
                    return i;
                }
            }
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null && monster.MonsterId == monsterId)
            {
                return i;
            }
        }

        return -1;
    }

    private string GetBattleUiTargetTypeLabel(string targetKind)
    {
        switch (targetKind)
        {
            case "all_allies":
                return "All allies";
            case "all_enemies":
                return "All enemies";
            case "party":
                return "Party";
            case "single_enemy":
                return "Single enemy";
            default:
                return "Current menu";
        }
    }

    private string GetBattleUiEffectText(string effectType, int powerValue)
    {
        int safePower = powerValue > 0 ? powerValue : 1;
        switch (effectType)
        {
            case "heal":
                return "Restore " + safePower + " HP to all allies.";
            case "finisher_damage":
                return "Deal " + safePower + " damage. Gains a bonus on exposed or weakened targets.";
            case "move":
                return "Reposition to a different lane.";
            case "end_turn":
                return "Pass the turn without using an action.";
            case "retreat":
                return "Exit the run using the current resolution flow.";
            case "damage":
            default:
                return "Deal " + safePower + " damage.";
        }
    }

    private string GetBattleActionKey(BattleActionType action)
    {
        switch (action)
        {
            case BattleActionType.Attack:
                return "attack";
            case BattleActionType.Skill:
                return "skill";
            case BattleActionType.Move:
                return "move";
            case BattleActionType.EndTurn:
                return "end_turn";
            case BattleActionType.Retreat:
                return "retreat";
            default:
                return string.Empty;
        }
    }

    private DungeonPartyMemberRuntimeData GetPartyMemberById(string memberId)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private string GetBattlePhaseKey()
    {
        switch (_battleState)
        {
            case BattleState.PartyActionSelect:
                return "party_action_select";
            case BattleState.PartyTargetSelect:
                return "party_target_select";
            case BattleState.EnemyTurn:
                return "enemy_turn";
            case BattleState.Victory:
                return "victory";
            case BattleState.Defeat:
                return "defeat";
            case BattleState.Retreat:
                return "retreat";
            default:
                return "none";
        }
    }

    private string GetCombatEntityDisplayName(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return string.Empty;
        }

        DungeonPartyMemberRuntimeData member = GetPartyMemberById(entityId);
        if (member != null)
        {
            return member.DisplayName;
        }

        DungeonMonsterRuntimeData monster = GetMonsterById(entityId);
        if (monster != null)
        {
            return monster.DisplayName;
        }

        if (entityId == _currentDungeonId && !string.IsNullOrEmpty(_currentDungeonName))
        {
            return _currentDungeonName;
        }

        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.EncounterId == entityId)
        {
            return encounter.DisplayName;
        }

        if (!string.IsNullOrEmpty(_eliteEncounterId) && entityId == _eliteEncounterId)
        {
            return string.IsNullOrEmpty(_eliteName) ? entityId : _eliteName;
        }

        return entityId;
    }

    private string InferBattleActionKeyForEvent(string actorId)
    {
        if (string.IsNullOrEmpty(actorId))
        {
            return string.Empty;
        }

        if (GetPartyMemberById(actorId) != null)
        {
            string actionKey = GetBattleActionKey(_queuedBattleAction);
            return string.IsNullOrEmpty(actionKey) ? "attack" : actionKey;
        }

        if (GetMonsterById(actorId) != null)
        {
            return _pendingEnemyUsedSpecialAttack ? "skill" : "attack";
        }

        return string.Empty;
    }

    private string InferBattleSkillIdForEvent(string actorId, string actionKey)
    {
        if (!string.Equals(actionKey, "skill"))
        {
            return string.Empty;
        }

        DungeonPartyMemberRuntimeData member = GetPartyMemberById(actorId);
        return member != null ? member.DefaultSkillId : string.Empty;
    }

    private string BuildPartyMembersAtEndSummary()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || _activeDungeonParty.Members.Length == 0)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null)
            {
                continue;
            }

            string statusSuffix = member.IsDefeated ? " KO" : string.Empty;
            parts.Add(member.DisplayName + " " + member.CurrentHp + "/" + member.MaxHp + statusSuffix);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private PrototypeBattleEventRecord CopyBattleEventRecord(PrototypeBattleEventRecord source)
    {
        return CopyRpgOwnedBattleEventRecord(source);
    }

    private PrototypeBattleResultSnapshot CopyBattleResultSnapshot(PrototypeBattleResultSnapshot source)
    {
        return CopyRpgOwnedBattleResultSnapshot(source);
    }

    private PrototypeEnemyIntentSnapshot CopyEnemyIntentSnapshot(PrototypeEnemyIntentSnapshot source)
    {
        return CopyRpgOwnedEnemyIntentSnapshot(source);
    }

    private void RecordBattleEvent(
        string eventKey,
        string actorId,
        string targetId,
        int amount,
        string summary,
        string actionKey = "",
        string skillId = "",
        string phaseKey = "",
        string actorName = "",
        string targetName = "",
        string shortText = "",
        string eventType = "")
    {
        RecordRpgOwnedBattleEvent(eventKey, actorId, targetId, amount, summary, actionKey, skillId, phaseKey, actorName, targetName, shortText, eventType);
    }

    public PrototypeBattleEventRecord[] GetRecentBattleEventRecords()
    {
        return BuildRecentBattleEventRecords();
    }

    public PrototypeBattleEventRecord GetLatestBattleEventRecord()
    {
        return GetRpgOwnedLatestBattleEventRecord();
    }

    private PrototypeBattleEventRecord[] BuildRecentBattleEventRecords(int maxCount = 8)
    {
        return BuildRpgOwnedRecentBattleEventRecords(maxCount);
    }

    private PrototypeEnemyIntentSnapshot BuildCurrentEnemyIntentSnapshotView()
    {
        return BuildRpgOwnedCurrentEnemyIntentSnapshotView();
    }

    private void ResetCombatRulePipelineState()
    {
        ResetRpgOwnedCombatRulePipelineState();
    }




    private int GetDefeatedEnemyCount()
    {
        int count = 0;
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
                if (monster != null && (monster.IsDefeated || monster.CurrentHp <= 0))
                {
                    count += 1;
                }
            }

            return count;
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null && (monster.IsDefeated || monster.CurrentHp <= 0))
            {
                count += 1;
            }
        }

        return count;
    }

    private void RecordCurrentPartyTurnStartEvent()
    {
        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return;
        }

        RecordBattleEvent(
            PrototypeBattleEventKeys.TurnStart,
            member.MemberId,
            string.Empty,
            Mathf.Max(0, _currentActorIndex),
            member.DisplayName + " turn started.",
            phaseKey: "party_turn",
            actorName: member.DisplayName,
            shortText: "Turn started");
    }

    private void RecordEnemyTurnStartEvent(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return;
        }

        RecordBattleEvent(
            PrototypeBattleEventKeys.TurnStart,
            monster.MonsterId,
            string.Empty,
            Mathf.Max(0, _enemyTurnMonsterCursor),
            monster.DisplayName + " turn started.",
            phaseKey: "enemy_turn",
            actorName: monster.DisplayName,
            shortText: "Turn started");
    }

    private int ApplyBattleDamageToMonster(DungeonPartyMemberRuntimeData actor, DungeonMonsterRuntimeData monster, int damage, Color popupColor)
    {
        return ApplyRpgOwnedBattleDamageToMonster(actor, monster, damage, popupColor);
    }

    private int ApplyBattleDamageToPartyMember(DungeonMonsterRuntimeData monster, int targetIndex, int damage, Color popupColor)
    {
        return ApplyRpgOwnedBattleDamageToPartyMember(monster, targetIndex, damage, popupColor);
    }

    private int ApplyBattleHealToPartyMember(DungeonPartyMemberRuntimeData actor, DungeonPartyMemberRuntimeData member, int memberIndex, int recoverAmount, Color popupColor)
    {
        return ApplyRpgOwnedBattleHealToPartyMember(actor, member, memberIndex, recoverAmount, popupColor);
    }

    private void ResolveMonsterDefeat(DungeonMonsterRuntimeData monster, bool wasDefeated)
    {
        if (monster == null || wasDefeated || !monster.IsDefeated)
        {
            return;
        }

        monster.RuntimeState?.ClearIntent();
        ShowBattlePopupForMonster(monster, "Defeated", new Color(1f, 0.82f, 0.24f, 1f), 1.18f);
        ShowBattleRewardPopupForMonster(monster);
        AppendBattleLog(monster.DisplayName + " is defeated.");
        RecordBattleEvent(
            PrototypeBattleEventKeys.EnemyDefeated,
            string.Empty,
            monster.MonsterId,
            0,
            monster.DisplayName + " is defeated.",
            phaseKey: "resolution",
            targetName: monster.DisplayName,
            shortText: "Enemy defeated");
    }

    private void ShowBattleRewardPopupForMonster(DungeonMonsterRuntimeData monster)
    {
        string rewardPopupText = BuildBattleRewardPopupText(monster);
        if (string.IsNullOrEmpty(rewardPopupText))
        {
            return;
        }

        ShowBattlePopupForMonster(monster, rewardPopupText, new Color(0.98f, 0.84f, 0.38f, 1f), 1.42f);
    }

    private string BuildBattleRewardPopupText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null || monster.RewardAmount <= 0)
        {
            return string.Empty;
        }

        string resourceId = string.IsNullOrEmpty(monster.RewardResourceId)
            ? DungeonRewardResourceId
            : monster.RewardResourceId;
        return resourceId == DungeonRewardResourceId
            ? "Loot +" + monster.RewardAmount
            : resourceId + " +" + monster.RewardAmount;
    }

    private void ResolvePartyMemberKnockOut(DungeonPartyMemberRuntimeData member, bool wasKnockedOut)
    {
        if (member == null || wasKnockedOut || !member.IsDefeated)
        {
            return;
        }

        ShowBattlePopupForPartyMember(member.PartySlotIndex, "KO", new Color(1f, 0.82f, 0.24f, 1f), 1.02f);
        AppendBattleLog(member.DisplayName + " is KO.");
        RecordBattleEvent(
            PrototypeBattleEventKeys.KnockOut,
            string.Empty,
            member.MemberId,
            0,
            member.DisplayName + " is KO.",
            phaseKey: "resolution",
            targetName: member.DisplayName,
            shortText: "Party KO");
    }
    private void AdvanceBattleAfterPartyAction()
    {
        AdvanceRpgOwnedBattleAfterPartyAction();
    }

    private void ResumePlayerTurnFromStartIndex(int startIndex)
    {
        ResumeRpgOwnedPlayerTurnFromStartIndex(startIndex);
    }

    private List<int> GetLivingAllies()
    {
        List<int> livingIndices = new List<int>();
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return livingIndices;
        }

        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && !member.IsDefeated && member.CurrentHp > 0)
            {
                livingIndices.Add(i);
            }
        }

        return livingIndices;
    }

    private List<DungeonMonsterRuntimeData> GetTargetableBattleMonsters()
    {
        List<DungeonMonsterRuntimeData> monsters = new List<DungeonMonsterRuntimeData>();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
                if (monster != null && !monster.IsDefeated && monster.CurrentHp > 0)
                {
                    monsters.Add(monster);
                }
            }

            return monsters;
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null && !monster.IsDefeated && monster.CurrentHp > 0)
            {
                monsters.Add(monster);
            }
        }

        return monsters;
    }

    private PrototypeEnemyIntentSnapshot BuildEnemyIntentSnapshot(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        return BuildRpgOwnedEnemyIntentSnapshot(monster, targetIndex, useSpecial);
    }
    private string GetBattleUiNextStepText()
    {
        if (_battleState == BattleState.PartyTargetSelect)
        {
            return "Choose target -> resolve action";
        }

        if (_battleState == BattleState.EnemyTurn)
        {
            return "Enemy action resolving";
        }

        if (_battleState == BattleState.Victory)
        {
            return "Battle won -> return to summary";
        }

        if (_battleState == BattleState.Defeat || _battleState == BattleState.Retreat)
        {
            return "Resolve battle result";
        }

        return _battleState == BattleState.PartyActionSelect
            ? "Choose command -> choose target"
            : "Awaiting battle state";
    }

    public bool IsBattleActionAvailable(string actionKey)
    {
        if (IsRpgOwnedBattleMoveActionKey(actionKey))
        {
            return CanRpgOwnedCurrentActorShiftToBattleLane(GetCurrentActorMember(), GetRpgOwnedBattleMoveLaneKey(actionKey));
        }

        return IsBattleActionAvailable(ParseBattleActionType(actionKey));
    }

    public bool IsBattleActionHovered(string actionKey)
    {
        if (IsRpgOwnedBattleMoveActionKey(actionKey))
        {
            return _dungeonRunState == DungeonRunState.Battle && _hoverBattleAction == BattleActionType.Move;
        }

        BattleActionType action = ParseBattleActionType(actionKey);
        return _dungeonRunState == DungeonRunState.Battle && action != BattleActionType.None && _hoverBattleAction == action;
    }

    public bool IsBattleActionSelected(string actionKey)
    {
        if (IsRpgOwnedBattleMoveActionKey(actionKey))
        {
            return _dungeonRunState == DungeonRunState.Battle &&
                   _battleState == BattleState.PartyActionSelect &&
                   _queuedBattleAction == BattleActionType.Move;
        }

        BattleActionType action = ParseBattleActionType(actionKey);
        return _dungeonRunState == DungeonRunState.Battle &&
               action != BattleActionType.None &&
               _queuedBattleAction == action &&
               (_battleState == BattleState.PartyTargetSelect ||
                (action == BattleActionType.Move && _battleState == BattleState.PartyActionSelect));
    }

    public void SetBattleActionHover(string actionKey)
    {
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            _hoverBattleAction = BattleActionType.None;
            return;
        }

        if (IsRpgOwnedBattleMoveActionKey(actionKey))
        {
            SetHoveredBattleAction(BattleActionType.Move);
            return;
        }

        SetHoveredBattleAction(ParseBattleActionType(actionKey));
    }

    public bool TryTriggerBattleAction(string actionKey)
    {
        if (TryResolveRpgOwnedBattleMoveAction(actionKey))
        {
            return true;
        }

        return TryActivateCurrentBattleAction(ParseBattleActionType(actionKey));
    }

    public bool IsRouteChoiceAvailable(string optionKey)
    {
        string routeId = NormalizeRouteChoiceId(optionKey);
        return IsExpeditionPrepRouteSelectionActive() && !string.IsNullOrEmpty(routeId);
    }

    public bool IsRouteChoiceHovered(string optionKey)
    {
        string routeId = NormalizeRouteChoiceId(optionKey);
        return IsExpeditionPrepRouteSelectionActive() && !string.IsNullOrEmpty(routeId) && _hoverRouteChoiceId == routeId;
    }

    public bool IsRouteChoiceSelected(string optionKey)
    {
        string routeId = NormalizeRouteChoiceId(optionKey);
        return IsExpeditionPrepRouteSelectionActive() && !string.IsNullOrEmpty(routeId) && _selectedRouteChoiceId == routeId;
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
        if (_isExpeditionPrepBoardOpen)
        {
            SetExpeditionPrepFeedbackText("Route set to " + template.RouteLabel + ".");
        }
        else
        {
            SetBattleFeedbackText("Selected " + template.RouteLabel + ".");
        }
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }

    public bool CanConfirmRouteChoice()
    {
        if (!IsExpeditionPrepRouteSelectionActive() ||
            _runtimeEconomyState == null ||
            string.IsNullOrEmpty(_currentHomeCityId) ||
            string.IsNullOrEmpty(_currentDungeonId) ||
            string.IsNullOrEmpty(_selectedRouteChoiceId))
        {
            return false;
        }

        LaunchReadiness readiness = BuildPlannerLaunchReadiness();
        return readiness.GateResult != null && readiness.GateResult.CanLaunch;
    }

    public bool TryConfirmRouteChoice()
    {
        if (!CanConfirmRouteChoice())
        {
            if (_isExpeditionPrepBoardOpen)
            {
                LaunchReadiness readiness = BuildPlannerLaunchReadiness();
                string feedback = readiness.GateResult != null && !string.IsNullOrEmpty(readiness.GateResult.SummaryText)
                    ? readiness.GateResult.SummaryText
                    : "Resolve the current launch blocker before committing the expedition.";
                SetExpeditionPrepFeedbackText(feedback);
            }

            return false;
        }

        string routeId = NormalizeRouteChoiceId(_selectedRouteChoiceId);
        ExpeditionStartContext launchContext;
        if (!TryConsumeProjectedLaunchContext(routeId, out launchContext))
        {
            if (_isExpeditionPrepBoardOpen)
            {
                string feedback = launchContext != null && !string.IsNullOrEmpty(launchContext.LaunchLockSummaryText)
                    ? launchContext.LaunchLockSummaryText
                    : "Projected launch context is blocked.";
                SetExpeditionPrepFeedbackText(feedback);
            }

            return false;
        }

        ExpeditionPrepHandoff handoff = BuildSelectedExpeditionPrepHandoff();
        string launchCityId = handoff != null && !string.IsNullOrEmpty(handoff.CityId)
            ? handoff.CityId
            : _currentHomeCityId;
        string launchDungeonId = handoff != null && !string.IsNullOrEmpty(handoff.DungeonId)
            ? handoff.DungeonId
            : _currentDungeonId;
        string launchRouteId = handoff != null && !string.IsNullOrEmpty(handoff.SelectedRouteId)
            ? NormalizeRouteChoiceId(handoff.SelectedRouteId)
            : routeId;
        DungeonRouteTemplate template = GetRouteTemplateById(launchDungeonId, launchRouteId);
        if (template == null)
        {
            if (_isExpeditionPrepBoardOpen)
            {
                SetExpeditionPrepFeedbackText("Selected route is unavailable.");
            }

            return false;
        }

        string partyId = _runtimeEconomyState.BeginDungeonRun(launchCityId, launchDungeonId, launchRouteId);
        if (string.IsNullOrEmpty(partyId))
        {
            string failureSummary = handoff != null && !string.IsNullOrEmpty(handoff.BlockedReasonText)
                ? handoff.BlockedReasonText
                : "Could not reserve the ready party for the selected expedition.";
            RecordFailedProjectedLaunchContext(launchRouteId, failureSummary);
            if (_isExpeditionPrepBoardOpen)
            {
                SetExpeditionPrepFeedbackText(failureSummary);
            }

            return false;
        }

        StartDungeonRunForRoute(template, partyId, launchContext);
        CloseExpeditionPrepBoardShell();
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
        worldCamera.orthographicSize = _dungeonRunState == DungeonRunState.Battle ? 6.6f : 5.6f;
    }

        public bool TryEnterSelectedCityDungeon(Camera worldCamera)
    {
        if (_runtimeEconomyState == null || _selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City || IsDungeonRunActive || _isExpeditionPrepBoardOpen)
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

        ConfigureDungeonCamera(worldCamera);
        UpdateBattleTransientState();
        if (HandleBattleResultPopoverInput(keyboard))
        {
            return;
        }

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

    private bool HandleBattleResultPopoverInput(Keyboard keyboard)
    {
        if (!IsBattleResultPopoverVisible)
        {
            return false;
        }

        ClearBattleHoverState();
        _hoverPreEliteChoiceId = string.Empty;
        if (keyboard != null &&
            (keyboard.enterKey.wasPressedThisFrame ||
             keyboard.numpadEnterKey.wasPressedThisFrame ||
             keyboard.spaceKey.wasPressedThisFrame ||
             keyboard.escapeKey.wasPressedThisFrame))
        {
            ClearBattleResultPopover();
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
        }

        return true;
    }

    public bool ConsumeDungeonRunExitRequest()
    {
        if (!_pendingDungeonExit)
        {
            return false;
        }

        PreparePendingExpeditionPostRunRevealForWorldReturn();
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
        return BuildRoutePreviewSummaryText(ResolveScenarioContextCityId(), _currentDungeonId, routeId);
    }

    private string BuildRoutePreviewSummaryText(string dungeonId, string routeId)
    {
        return BuildRoutePreviewSummaryText(ResolveScenarioContextCityId(), dungeonId, routeId);
    }

    private string BuildRoutePreviewSummaryText(string cityId, string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            return "None";
        }

        string scenarioLabel = TryBuildRouteScenarioLabelFromContent(cityId, dungeonId, routeId);
        string chooseWhenText = TryBuildRouteChooseWhenTextFromContent(cityId, dungeonId, routeId);
        string watchOutText = TryBuildRouteWatchOutTextFromContent(cityId, dungeonId, routeId);
        string followUpHintText = TryBuildRouteFollowUpHintTextFromContent(cityId, dungeonId, routeId);
        string strategicPreview = BuildScenarioPipeText(
            HasText(scenarioLabel)
                ? template.RouteLabel + " | " + scenarioLabel
                : template.RouteLabel + " | " + template.RiskLabel + " Risk",
            BuildLabeledScenarioClause("Choose when", chooseWhenText),
            BuildLabeledScenarioClause("Watch", watchOutText),
            BuildLabeledScenarioClause("Follow-up", followUpHintText));
        if (strategicPreview != "None")
        {
            return strategicPreview;
        }

        return template.RouteLabel + " | " + template.RiskLabel + " Risk | " + template.Description + " | Rooms: " + BuildRoomPathPreviewText(dungeonId, routeId) + " | Threats: " + template.EncounterPreview + " | Shrine Read: " + template.EventFocus;
    }

    private string BuildRouteRewardPreviewText()
    {
        return BuildRouteRewardPreviewText(_currentDungeonId);
    }

    private string BuildRouteRewardPreviewText(string dungeonId)
    {
        string cityId = ResolveScenarioContextCityId();
        DungeonRouteTemplate route1 = GetRouteTemplateById(dungeonId, SafeRouteId);
        DungeonRouteTemplate route2 = GetRouteTemplateById(dungeonId, RiskyRouteId);
        if (route1 == null || route2 == null)
        {
            return "None";
        }

        string route1Preview = BuildScenarioPipeText(
            route1.RouteLabel,
            BuildLabeledScenarioClause("Reward", TryBuildRouteRewardFocusTextFromContent(cityId, dungeonId, SafeRouteId)),
            BuildLabeledScenarioClause("Party fit", TryBuildRoutePartyFitTextFromContent(cityId, dungeonId, SafeRouteId)));
        string route2Preview = BuildScenarioPipeText(
            route2.RouteLabel,
            BuildLabeledScenarioClause("Reward", TryBuildRouteRewardFocusTextFromContent(cityId, dungeonId, RiskyRouteId)),
            BuildLabeledScenarioClause("Party fit", TryBuildRoutePartyFitTextFromContent(cityId, dungeonId, RiskyRouteId)));
        if (route1Preview != "None" && route2Preview != "None")
        {
            return route1Preview + " || " + route2Preview;
        }

        if (route1Preview != "None")
        {
            return route1Preview;
        }

        if (route2Preview != "None")
        {
            return route2Preview;
        }

        int route1Total = route1.BattleLootAmount + route1.ChestRewardAmount + route1.BonusLootAmount;
        int route2Total = route2.BattleLootAmount + route2.ChestRewardAmount + route2.BonusLootAmount;
        return route1.RouteLabel + ": " + route1.RewardPreview + " | Recover +" + route1.RecoverAmount + " HP | Total " + BuildLootAmountText(route1Total) +
            " | " + route2.RouteLabel + ": " + route2.RewardPreview + " | Recover +" + route2.RecoverAmount + " HP | Total " + BuildLootAmountText(route2Total);
    }

    private string BuildRouteEventPreviewText()
    {
        return BuildRouteEventPreviewText(_currentDungeonId);
    }

    private string BuildRouteEventPreviewText(string dungeonId)
    {
        string cityId = ResolveScenarioContextCityId();
        DungeonRouteTemplate route1 = GetRouteTemplateById(dungeonId, SafeRouteId);
        DungeonRouteTemplate route2 = GetRouteTemplateById(dungeonId, RiskyRouteId);
        if (route1 == null || route2 == null)
        {
            return "None";
        }

        string route1Preview = BuildScenarioPipeText(
            route1.RouteLabel,
            BuildLabeledScenarioClause("Combat", TryBuildRouteCombatPlanTextFromContent(cityId, dungeonId, SafeRouteId)),
            BuildLabeledScenarioClause("Event", route1.EventFocus));
        string route2Preview = BuildScenarioPipeText(
            route2.RouteLabel,
            BuildLabeledScenarioClause("Combat", TryBuildRouteCombatPlanTextFromContent(cityId, dungeonId, RiskyRouteId)),
            BuildLabeledScenarioClause("Event", route2.EventFocus));
        if (route1Preview != "None" && route2Preview != "None")
        {
            return route1Preview + " || " + route2Preview;
        }

        if (route1Preview != "None")
        {
            return route1Preview;
        }

        if (route2Preview != "None")
        {
            return route2Preview;
        }

        return route1.RouteLabel + ": " + route1.EventFocus + " | Recover +" + route1.RecoverAmount + " HP each or " + BuildLootAmountText(route1.BonusLootAmount) +
            " | " + route2.RouteLabel + ": " + route2.EventFocus + " | Recover +" + route2.RecoverAmount + " HP each or " + BuildLootAmountText(route2.BonusLootAmount);
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

        AddPlannedRoomStep(dungeonId + "-elite", "Elite Chamber", "Elite Chamber", DungeonRoomType.Elite, EliteEncounterMarkerPosition, _eliteEncounterId);
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
                ? "The war table is a raider's last bargain: patch the party only lightly, or walk into the chief fight worn and claim the bigger bounty if you finish it."
                : "The guard muster can steady the party before the captain, but taking that order shaves the reward line.";
        }

        return _selectedRouteId == RiskyRouteId
            ? "The core threshold hums like a warning: recover now and dull the final spike, or stay bruised and squeeze the richer payout out of the elite."
            : "The quiet antechamber offers one calm reset before the monarch, but accepting it means settling for the smaller reward line.";
    }

    private string GetCurrentPreEliteOptionAText()
    {
        string actionLabel;
        if (_currentDungeonId == "dungeon-beta")
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Patch Up at the War Table" : "Take the Guard Muster Rations";
        }
        else
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Stabilize at the Core Threshold" : "Reset in the Quiet Antechamber";
        }

        return actionLabel + " (+" + GetCurrentPreEliteRecoverAmount() + " HP to each living ally)";
    }

    private string GetCurrentPreEliteOptionBText()
    {
        string actionLabel;
        if (_currentDungeonId == "dungeon-beta")
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Mark the Raider Bounty" : "Hold the Line for Captain Spoils";
        }
        else
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Hold for the Core Bounty" : "Press Through for Monarch Spoils";
        }

        return actionLabel + " (+" + GetCurrentEliteBonusRewardAmount() + " " + DungeonRewardResourceId + " on elite victory)";
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
        string actionLabel;
        if (_currentDungeonId == "dungeon-beta")
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Patch Up Beneath the War Banner" : "Rest at the Watchfire";
        }
        else
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Stabilize at the Unstable Shrine" : "Take the Rest Shrine Blessing";
        }

        return actionLabel + " (+" + GetCurrentShrineRecoverAmount() + " HP to each living ally)";
    }

    private string GetCurrentEventOptionBText()
    {
        string actionLabel;
        if (_currentDungeonId == "dungeon-beta")
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Rip Down the War Banner Cache" : "Take the Watchfire Cache";
        }
        else
        {
            actionLabel = _selectedRouteId == RiskyRouteId ? "Strip the Shard Reservoir" : "Break the Rest Shrine Cache";
        }

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

    private DungeonRoomTemplateData GetPlannedRoomByMarkerPosition(Vector2Int markerPosition)
    {
        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            DungeonRoomTemplateData room = _plannedRooms[i];
            if (room != null && room.MarkerPosition == markerPosition)
            {
                return room;
            }
        }

        return null;
    }

    private Color GetRoomMarkerColor(DungeonRoomType roomType)
    {
        if (roomType == DungeonRoomType.Cache)
        {
            return new Color(0.46f, 0.34f, 0.16f, 1f);
        }

        if (roomType == DungeonRoomType.Shrine)
        {
            return new Color(0.24f, 0.38f, 0.28f, 1f);
        }

        if (roomType == DungeonRoomType.Preparation)
        {
            return new Color(0.39f, 0.28f, 0.46f, 1f);
        }

        if (roomType == DungeonRoomType.Elite)
        {
            return new Color(0.48f, 0.20f, 0.20f, 1f);
        }

        return new Color(0.25f, 0.32f, 0.46f, 1f);
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
        string routeId = GetRecommendedRouteId(cityId, dungeonId);
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        string scenarioLabel = TryBuildRouteScenarioLabelFromContent(cityId, dungeonId, routeId);
        return template == null
            ? "None"
            : HasText(scenarioLabel)
                ? template.RouteLabel + " | " + scenarioLabel + " | " + template.RiskLabel + " Risk"
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
                ? "Mana Shard stock is low, so the board leans toward the richer line."
                : readiness == DispatchReadinessState.Recovering
                    ? "Mana Shard stock is low, but the city is still recovering. The board prefers the middle ground."
                    : "Mana Shard stock is low, but the city is strained. The board avoids the hardest greed line.";
        }

        if (needPressure == "Watch")
        {
            return readiness == DispatchReadinessState.Strained
                ? "Stock needs watching and the city looks strained. The board prefers a steadier line."
                : "Stock needs watching. The board prefers a balanced line.";
        }

        return readiness == DispatchReadinessState.Ready
            ? "Stock is stable. The board can afford the steadier line."
            : readiness == DispatchReadinessState.Recovering
                ? "Stock is stable and the city is recovering. The board favors the steadier line."
                : "Stock is stable, but the city is strained. The board favors a safer push or a short delay.";
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
                ? "City policy leans safer, but urgent demand still keeps the richer recommendation alive."
                : "City policy leans toward safer dispatches.";
        }

        if (policy == DispatchPolicyState.Profit)
        {
            return readiness == DispatchReadinessState.Ready
                ? "City policy leans toward richer dispatches."
                : readiness == DispatchReadinessState.Recovering
                    ? "City policy leans richer, but recovery trims that bias."
                    : "City policy leans richer, but city strain trims that bias.";
        }

        return "City policy stays balanced.";
    }

    private string BuildRecommendationReasonText(string cityId, string dungeonId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        string baseRecommendedRouteId = GetBaseRecommendedRouteId(cityId, dungeonId);
        string finalRecommendedRouteId = ApplyPolicyBiasToRecommendedRoute(cityId, dungeonId, baseRecommendedRouteId);
        string summary = BuildScenarioSentenceText(
            BuildBaseRecommendationReasonText(cityId, dungeonId),
            BuildPolicyRecommendationReasonText(cityId, dungeonId, baseRecommendedRouteId, finalRecommendedRouteId),
            TryBuildRouteChooseWhenTextFromContent(cityId, dungeonId, finalRecommendedRouteId),
            TryBuildRouteFollowUpHintTextFromContent(cityId, dungeonId, finalRecommendedRouteId));
        string partyFitText = BuildRuntimePartyRouteFitText(ResolveDispatchReadyPartyId(cityId), dungeonId, finalRecommendedRouteId);
        string finalText = BuildScenarioSentenceText(summary, partyFitText);
        return HasText(finalText) ? finalText : "None";
    }

    private string BuildFallbackExpectedNeedImpactText(string cityId, string dungeonId, string routeId)
    {
        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        string balancedRouteId = GetBalancedRouteId(dungeonId);
        if (needPressure == "Urgent")
        {
            if (readiness != DispatchReadinessState.Ready)
            {
                return routeId == balancedRouteId
                    ? "Keeps shard income moving without breaking the recovering party."
                    : "Protects the party, but stock recovers slowly.";
            }

            return routeId == RiskyRouteId
                ? "Fastest line for refilling shard stock."
                : "Steadier run, smaller refill.";
        }

        if (needPressure == "Watch")
        {
            if (routeId == balancedRouteId)
            {
                return readiness == DispatchReadinessState.Strained
                    ? "Balances shard income while easing city strain."
                    : "Keeps both sustain and income in view.";
            }

            return routeId == RiskyRouteId
                ? "More shards now, but the city eats the strain."
                : "Safer line, lighter return.";
        }

        return readiness == DispatchReadinessState.Ready
            ? routeId == SafeRouteId
                ? "Low-pressure run that protects the party."
                : "More reward than the city currently needs."
            : routeId == SafeRouteId
                ? "Best line for letting the city and party recover together."
                : "Adds reward, but drags recovery out.";
    }

    private string BuildExpectedNeedImpactText(string cityId, string dungeonId, string routeId)
    {
        if (string.IsNullOrEmpty(dungeonId) || string.IsNullOrEmpty(routeId))
        {
            return "None";
        }

        string impactText = TryBuildExpectedNeedImpactFromContent(cityId, dungeonId, routeId);
        if (!HasText(impactText))
        {
            impactText = BuildFallbackExpectedNeedImpactText(cityId, dungeonId, routeId);
        }

        string followUpHintText = TryBuildRouteFollowUpHintTextFromContent(cityId, dungeonId, routeId);
        string partyFitText = BuildRuntimePartyRouteFitText(ResolveDispatchReadyPartyId(cityId), dungeonId, routeId);
        string finalText = BuildScenarioSentenceText(impactText, followUpHintText, partyFitText);
        return HasText(finalText) ? finalText : "None";
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
        if (IsExpeditionPrepRouteSelectionActive() && _currentHomeCityId == cityId)
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

            string feedback = "Dispatch policy set to " + BuildDispatchPolicyText(GetDispatchPolicyState(cityId)) + ".";
            if (_isExpeditionPrepBoardOpen)
            {
                SetExpeditionPrepFeedbackText(feedback);
            }
            else
            {
                SetBattleFeedbackText(feedback);
            }
        }

        return true;
    }

    public bool TryCycleCurrentDispatchPolicy()
    {
        return IsExpeditionPrepRouteSelectionActive() && !string.IsNullOrEmpty(_currentHomeCityId)
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
            ? "Arrive steadier"
            : _hoverPreEliteChoiceId == "bonus"
                ? "Hold for the kill bonus"
                : string.Empty;
        string suffix = string.IsNullOrEmpty(hoverLabel) ? string.Empty : " | Hover: " + hoverLabel;
        return "Set the entry into " + _eliteName + ": [1] Arrive steadier  [2] Hold for the kill bonus, or click an option." + suffix;
    }

    private string GetEventDescriptionText()
    {
        if (_currentDungeonId == "dungeon-beta")
        {
            return _selectedRouteId == RiskyRouteId
                ? "The war-banner shrine is a raider altar. It offers only a thin patch-up; the real temptation is tearing down the banner rack and leaving with stolen bounty before the chief."
                : "The watchfire shrine sits behind the patrol line. Resting here keeps the captain approach orderly, while breaking the fire cache trades that control for more shards.";
        }

        return _selectedRouteId == RiskyRouteId
            ? "The unstable shrine flickers under mixed pressure. One choice steadies the party after the goblin crossfire; the other strips the shard reservoir and dares the route to keep hurting you."
            : "The rest shrine still has warmth in it. You can let the mire drain off the party, or crack the cache stones and leave the next rooms harsher but richer.";
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
                ? "ExpeditionPrep owns the normal launch seam. This legacy fallback only appears when DungeonRun must recover a missing route handoff."
                : GetHomeCityDisplayName() + " -> " + dungeonTemplate.DungeonLabel + " | ExpeditionPrep owns the normal launch seam. Use this legacy fallback only when DungeonRun must recover a missing route handoff.";
        }

        return GetHomeCityDisplayName() + " -> " + template.RouteLabel + " | Legacy fallback only | " + template.RiskLabel + " Risk | " + template.Description + " | Threats: " + template.EncounterPreview + " | Elite Read: " + template.RewardPreview;
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
        string dispatchHint = CanConfirmRouteChoice() ? "[Enter] Continue Fallback" : "Select a route, then [Enter] Continue Fallback";
        string policyText = BuildDispatchPolicyText(GetDispatchPolicyState(_currentHomeCityId));
        return "Legacy fallback only. ExpeditionPrep owns the normal launch seam. " + dispatchHint + " | [Q] Policy: " + policyText + " | [1] " + option1 + "  [2] " + option2 + "  [Esc] Return | Recommended: " + _recommendedRouteLabel + " | Selected: " + selectedRouteLabel + hoverSuffix;
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
            return "Legacy Dispatch Fallback";
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
            return "Legacy Dispatch Fallback";
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
        return IsRpgOwnedPartyWideEliteSpecial(monster, useSpecial);
    }

    private PrototypeRpgPartyMemberRuntimeState GetMemberRuntimeState(DungeonPartyMemberRuntimeData member)
    {
        return member != null ? member.RuntimeState : null;
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
            PrototypeRpgPartyMemberRuntimeState runtimeState = GetMemberRuntimeState(_activeDungeonParty.Members[i]);
            if (runtimeState != null)
            {
                total += Mathf.Max(0, runtimeState.CurrentHp);
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
            PrototypeRpgPartyMemberRuntimeState runtimeState = GetMemberRuntimeState(_activeDungeonParty.Members[i]);
            if (runtimeState != null)
            {
                total += Mathf.Max(1, runtimeState.MaxHp);
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
            PrototypeRpgPartyMemberRuntimeState runtimeState = GetMemberRuntimeState(_activeDungeonParty.Members[i]);
            if (runtimeState != null && runtimeState.CurrentHp < runtimeState.MaxHp)
            {
                count += 1;
            }
        }

        return count;
    }

    private int GetLivingPartyMemberCount()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            PrototypeRpgPartyMemberRuntimeState runtimeState = GetMemberRuntimeState(_activeDungeonParty.Members[i]);
            if (runtimeState != null && runtimeState.IsAvailable)
            {
                count += 1;
            }
        }

        return count;
    }

    private int GetKnockedOutMemberCount()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            PrototypeRpgPartyMemberRuntimeState runtimeState = GetMemberRuntimeState(_activeDungeonParty.Members[i]);
            if (runtimeState != null && runtimeState.IsKnockedOut)
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

        if (GetLivingPartyMemberCount() <= 0)
        {
            return "Collapsed";
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

    private PrototypeRpgSkillDefinition ResolveCurrentActorSkillDefinition()
    {
        return ResolveMemberSkillDefinition(GetCurrentActorMember());
    }

    private PrototypeRpgSkillDefinition ResolveMemberSkillDefinition(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return null;
        }

        return PrototypeRpgSkillCatalog.ResolveDefinition(member.DefaultSkillId, member.RoleTag);
    }

    private string GetResolvedSkillDisplayName(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition = null)
    {
        if (skillDefinition == null)
        {
            skillDefinition = ResolveMemberSkillDefinition(member);
        }

        if (skillDefinition != null && !string.IsNullOrEmpty(skillDefinition.DisplayName))
        {
            return skillDefinition.DisplayName;
        }

        return member != null && !string.IsNullOrEmpty(member.SkillName) ? member.SkillName : "Skill";
    }

    private string GetResolvedSkillTargetKind(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition = null)
    {
        if (skillDefinition == null)
        {
            skillDefinition = ResolveMemberSkillDefinition(member);
        }

        if (skillDefinition != null && !string.IsNullOrEmpty(skillDefinition.TargetKind))
        {
            return skillDefinition.TargetKind;
        }

        if (member == null)
        {
            return string.Empty;
        }

        switch (member.SkillType)
        {
            case PartySkillType.AllEnemies:
                return "all_enemies";
            case PartySkillType.PartyHeal:
                return "all_allies";
            default:
                return "single_enemy";
        }
    }

    private string GetResolvedSkillEffectType(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition = null)
    {
        if (skillDefinition == null)
        {
            skillDefinition = ResolveMemberSkillDefinition(member);
        }

        if (skillDefinition != null && !string.IsNullOrEmpty(skillDefinition.EffectType))
        {
            return skillDefinition.EffectType;
        }

        if (member == null)
        {
            return string.Empty;
        }

        switch (member.SkillType)
        {
            case PartySkillType.PartyHeal:
                return "heal";
            case PartySkillType.Finisher:
                return "finisher_damage";
            default:
                return "damage";
        }
    }

    private int GetResolvedSkillPower(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition = null)
    {
        if (skillDefinition == null)
        {
            skillDefinition = ResolveMemberSkillDefinition(member);
        }

        if (skillDefinition != null && skillDefinition.PowerValue > 0)
        {
            return skillDefinition.PowerValue;
        }

        if (member != null && member.SkillPower > 0)
        {
            return member.SkillPower;
        }

        return member != null ? Mathf.Max(1, member.Attack + 1) : 1;
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
        return GetRpgOwnedMonsterTargetPartyMemberIndex(monster);
    }

        private bool ShouldUseEliteSpecialAttack(DungeonMonsterRuntimeData monster)
    {
        return ShouldRpgOwnedUseEliteSpecialAttack(monster);
    }

    private int GetEnemyActionPower(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        return GetRpgOwnedEnemyActionPower(monster, useSpecial);
    }

    private string GetEnemyActionLabel(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        return GetRpgOwnedEnemyActionLabel(monster, useSpecial);
    }

        private string BuildEnemyIntentText(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        return BuildRpgOwnedEnemyIntentText(monster, targetIndex, useSpecial);
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

    private Vector3 GetBattlePartyViewPosition(int memberIndex)
    {
        if (memberIndex < 0 || memberIndex >= BattlePartyViewPositions.Length)
        {
            return Vector3.zero;
        }

        int worldSlotIndex = ResolveBattlePartyWorldSlotIndex(memberIndex);
        worldSlotIndex = Mathf.Clamp(worldSlotIndex, 0, BattlePartyViewPositions.Length - 1);
        return BattlePartyViewPositions[worldSlotIndex];
    }

    private int ResolveBattlePartyWorldSlotIndex(int memberIndex)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return memberIndex;
        }

        List<int> orderedMemberIndices = new List<int>(_activeDungeonParty.Members.Length);
        for (int i = 0; i < _activeDungeonParty.Members.Length && i < BattlePartyViewPositions.Length; i++)
        {
            if (_activeDungeonParty.Members[i] != null)
            {
                orderedMemberIndices.Add(i);
            }
        }

        if (orderedMemberIndices.Count <= 0)
        {
            return memberIndex;
        }

        orderedMemberIndices.Sort((leftIndex, rightIndex) =>
        {
            DungeonPartyMemberRuntimeData leftMember = _activeDungeonParty.Members[leftIndex];
            DungeonPartyMemberRuntimeData rightMember = _activeDungeonParty.Members[rightIndex];
            int laneCompare = ResolveBattlePartyWorldLaneOrder(GetPartyMemberLaneKey(leftMember))
                .CompareTo(ResolveBattlePartyWorldLaneOrder(GetPartyMemberLaneKey(rightMember)));
            if (laneCompare != 0)
            {
                return laneCompare;
            }

            int roleCompare = ResolveBattlePartyWorldRoleOrder(leftMember)
                .CompareTo(ResolveBattlePartyWorldRoleOrder(rightMember));
            if (roleCompare != 0)
            {
                return roleCompare;
            }

            return leftIndex.CompareTo(rightIndex);
        });

        int resolvedIndex = orderedMemberIndices.IndexOf(memberIndex);
        return resolvedIndex >= 0 ? resolvedIndex : memberIndex;
    }

    private int ResolveBattlePartyWorldLaneOrder(string laneKey)
    {
        switch (laneKey)
        {
            case PrototypeBattleLaneKeys.Bottom:
                return 0;
            case PrototypeBattleLaneKeys.Top:
                return 2;
            default:
                return 1;
        }
    }

    private int ResolveBattlePartyWorldRoleOrder(DungeonPartyMemberRuntimeData member)
    {
        string roleTag = member != null && !string.IsNullOrEmpty(member.RoleTag)
            ? member.RoleTag.ToLowerInvariant()
            : string.Empty;
        switch (roleTag)
        {
            case "mage":
                return 0;
            case "cleric":
                return 1;
            case "rogue":
                return 2;
            case "warrior":
                return 3;
            default:
                return 4;
        }
    }

    private DungeonMonsterRuntimeData GetFirstLivingBattleMonster()
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null)
        {
            return null;
        }

        DungeonPartyMemberRuntimeData currentActor = _battleState == BattleState.PartyTargetSelect
            ? GetCurrentActorMember()
            : null;
        PrototypeRpgSkillDefinition skillDefinition = _queuedBattleAction == BattleActionType.Skill && currentActor != null
            ? ResolveMemberSkillDefinition(currentActor)
            : null;
        DungeonMonsterRuntimeData fallback = null;

        for (int i = 0; i < encounter.MonsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
            if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
            {
                continue;
            }

            fallback ??= monster;
            if (currentActor == null)
            {
                return monster;
            }

            PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(currentActor, monster, _queuedBattleAction, skillDefinition);
            if (laneResolution == null || laneResolution.ReachabilityStateKey != "blocked")
            {
                return monster;
            }
        }

        return fallback;
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
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        return CalculateEncounterLoot(encounter);
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

        if (normalized == "move")
        {
            return BattleActionType.Move;
        }

        if (normalized == "end_turn" || normalized == "endturn" || normalized == "end-turn")
        {
            return BattleActionType.EndTurn;
        }

        if (normalized == "retreat")
        {
            return BattleActionType.Retreat;
        }

        return BattleActionType.None;
    }

    private bool IsBattleActionAvailable(BattleActionType action)
    {
        return IsRpgOwnedBattleActionAvailable(action);
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
            return GetResolvedSkillDisplayName(member);
        }

        if (action == BattleActionType.Move)
        {
            return "Move";
        }

        if (action == BattleActionType.EndTurn)
        {
            return "End Turn";
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
            ? "Keep the party steadier"
            : _hoverEventChoiceId == "loot"
                ? "Cash out the shrine"
                : string.Empty;
        string suffix = string.IsNullOrEmpty(hoverLabel) ? string.Empty : " | Hover: " + hoverLabel;
        return "Settle " + GetCurrentEventTitleText() + ": [1] Keep the party steadier  [2] Cash out the shrine, or click an option." + suffix;
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

    private void OpenEncounterBattleResultPopover(DungeonEncounterRuntimeData encounter, int grantedLoot, bool eliteVictory)
    {
        string encounterName = encounter != null && !string.IsNullOrEmpty(encounter.DisplayName)
            ? encounter.DisplayName
            : GetCurrentEncounterNameText();
        string summaryText = eliteVictory
            ? "Final elite defeated. The exit route is now open."
            : _exitUnlocked
                ? "Encounter resolved. All encounters are cleared and the exit is unlocked."
                : "Encounter resolved. Continue the expedition.";
        SetBattleResultPopover(
            PrototypeBattleOutcomeKeys.EncounterVictory,
            eliteVictory ? "Elite Encounter Cleared" : "Encounter Cleared",
            encounterName,
            summaryText,
            grantedLoot > 0 ? BuildLootAmountText(grantedLoot) : "None",
            BuildEncounterBattleDropSummaryText(encounter),
            "Growth and equipment rewards resolve when the full run ends.",
            "[Enter]/[Space]/[Esc] Continue");
    }

    private void OpenRunBattleResultPopover(string outcomeKey, string safeResultSummary, int safeReturnedLoot)
    {
        PrototypeBattleResultSnapshot snapshot = BuildRpgOwnedCurrentBattleResultSnapshotView();
        PrototypeDungeonRunResultContext resultContext = LatestDungeonRunResultContext;
        string titleText = outcomeKey == PrototypeBattleOutcomeKeys.RunDefeat
            ? "Battle Defeat"
            : outcomeKey == PrototypeBattleOutcomeKeys.RunRetreat
                ? "Retreat Confirmed"
                : "Battle Result";
        string encounterName = !string.IsNullOrEmpty(snapshot.EncounterName)
            ? snapshot.EncounterName
            : !string.IsNullOrEmpty(_currentDungeonName)
                ? _currentDungeonName
                : "Encounter";
        SetBattleResultPopover(
            outcomeKey,
            titleText,
            encounterName,
            safeResultSummary,
            safeReturnedLoot > 0 ? BuildLootAmountText(safeReturnedLoot) : "None",
            BuildRunBattleResultDropSummaryText(resultContext),
            BuildRunBattleResultGrowthSummaryText(resultContext),
            "[Enter]/[Space]/[Esc] Review result");
    }

    private string BuildEncounterBattleDropSummaryText(DungeonEncounterRuntimeData encounter)
    {
        int encounterLoot = CalculateEncounterLoot(encounter);
        return encounterLoot > 0 ? BuildLootAmountText(encounterLoot) : "None";
    }

    private string BuildRunBattleResultDropSummaryText(PrototypeDungeonRunResultContext resultContext)
    {
        if (resultContext != null && !string.IsNullOrEmpty(resultContext.GearRewardCandidateSummaryText) && resultContext.GearRewardCandidateSummaryText != "None")
        {
            return resultContext.GearRewardCandidateSummaryText;
        }

        if (_latestRpgRunResultSnapshot != null &&
            !string.IsNullOrEmpty(_latestRpgRunResultSnapshot.PendingRewardSummaryText))
        {
            return _latestRpgRunResultSnapshot.PendingRewardSummaryText;
        }

        return "None";
    }

    private string BuildRunBattleResultGrowthSummaryText(PrototypeDungeonRunResultContext resultContext)
    {
        OutcomeReadback outcomeReadback = resultContext != null
            ? resultContext.WorldOutcomeReadbackPreview ?? new OutcomeReadback()
            : new OutcomeReadback();
        if (!string.IsNullOrEmpty(outcomeReadback.LatestGrowthHighlightText) && outcomeReadback.LatestGrowthHighlightText != "None")
        {
            return outcomeReadback.LatestGrowthHighlightText;
        }

        if (_latestRpgRunResultSnapshot != null &&
            !string.IsNullOrEmpty(_latestRpgRunResultSnapshot.PendingRewardSummaryText))
        {
            return "Pending reward stash " + _latestRpgRunResultSnapshot.PendingRewardSummaryText;
        }

        return "Growth reveal is available in the run result.";
    }

    private int CalculateEncounterLoot(DungeonEncounterRuntimeData encounter)
    {
        int total = 0;
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
                    ? "Select action for " + member.DisplayName + " (" + member.RoleLabel + "). [1] Attack  [2] " + member.SkillName + "  [3] Item  [4] Move  [5] End Turn  [Q] Retreat" + skillHintText
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
            return "Legacy Dispatch Fallback";
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
        ClearRpgOwnedEnemyIntentState();
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

        ShowBattlePopup(GetBattlePartyViewPosition(memberIndex) + new Vector3(0.05f, yOffset, 0f), text, color);
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
        int grantedLoot = encounterLoot;
        if (!encounter.IsCleared)
        {
            encounter.IsCleared = true;
            _clearedEncounterCount += 1;
            _battlesFoughtCount += 1;
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
                grantedLoot += _eliteBonusRewardGrantedAmount;
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

        UpdateBattleResultSnapshot(PrototypeBattleOutcomeKeys.EncounterVictory);
        RecordBattleEvent(
            PrototypeBattleEventKeys.BattleVictory,
            string.Empty,
            encounter.EncounterId,
            grantedLoot,
            eliteVictory ? "Elite encounter won." : encounter.DisplayName + " cleared.",
            phaseKey: "victory",
            actorName: ActiveDungeonPartyText,
            targetName: encounter.DisplayName,
            shortText: eliteVictory ? "Elite victory" : "Encounter victory");
        OpenEncounterBattleResultPopover(encounter, grantedLoot, eliteVictory);

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
        BeginRpgOwnedEnemyTurn();
    }

    private bool TryQueueEnemyIntent(int startDisplayIndex)
    {
        return TryQueueRpgOwnedEnemyIntent(startDisplayIndex);
    }
    private bool TryActivateCurrentBattleAction(BattleActionType action)
    {
        return TryResolveRpgOwnedBattleAction(action);
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

            if (keyboard.qKey.wasPressedThisFrame)
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

        if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
        {
            TryActivateCurrentBattleAction(BattleActionType.Move);
            return;
        }

        if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
        {
            TryActivateCurrentBattleAction(BattleActionType.EndTurn);
            return;
        }

        if (keyboard.qKey.wasPressedThisFrame)
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
        return TryResolveRpgOwnedTargetSelection(targetMonster);
    }
    private void ExecuteQueuedEnemyIntent()
    {
        ExecuteRpgOwnedQueuedEnemyIntent();
    }
    private string BuildSurvivingMembersSummary()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return "0 / 4";
        }

        int totalMembers = _activeDungeonParty.Members.Length;
        int survivingCount = GetLivingPartyMemberCount();
        int knockedOutCount = GetKnockedOutMemberCount();
        return knockedOutCount > 0
            ? survivingCount + " / " + totalMembers + " (" + knockedOutCount + " KO)"
            : survivingCount + " / " + totalMembers;
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
        PrototypeRpgPartyDefinition partyDefinition = BuildRuntimePartyDefinition(partyId) ?? PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
        return new TestDungeonPartyData(partyId, cityId, partyDefinition, CreateDungeonRuntimeMembers(partyDefinition));
    }

    private DungeonPartyMemberRuntimeData[] CreateDungeonRuntimeMembers(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return System.Array.Empty<DungeonPartyMemberRuntimeData>();
        }

        PrototypeRpgPartyRuntimeResolveSurface partySurface = BuildRuntimePartyResolveSurface(partyDefinition.PartyId);
        if (partySurface == null)
        {
            partySurface = PrototypeRpgRuntimeResolveBuilder.BuildPartySurface(
                partyDefinition,
                memberDefinition => memberDefinition != null ? memberDefinition.EquipmentLoadoutId : string.Empty);
        }

        return CreateDungeonRuntimeMembers(partySurface);
    }

    private DungeonPartyMemberRuntimeData[] CreateDungeonRuntimeMembers(PrototypeRpgPartyRuntimeResolveSurface partySurface)
    {
        if (partySurface == null || partySurface.Members == null || partySurface.Members.Length == 0)
        {
            return System.Array.Empty<DungeonPartyMemberRuntimeData>();
        }

        DungeonPartyMemberRuntimeData[] members = new DungeonPartyMemberRuntimeData[partySurface.Members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            members[i] = CreateDungeonRuntimeMember(partySurface.Members[i], i);
        }

        return members;
    }

    private DungeonPartyMemberRuntimeData CreateDungeonRuntimeMember(PrototypeRpgMemberRuntimeResolveSurface memberSurface, int fallbackPartySlotIndex)
    {
        PrototypeRpgPartyMemberRuntimeState runtimeState = new PrototypeRpgPartyMemberRuntimeState(memberSurface);
        string roleTag = memberSurface != null ? memberSurface.RoleTag : string.Empty;
        string defaultSkillId = memberSurface != null ? memberSurface.DefaultSkillId : string.Empty;
        string skillName = memberSurface != null && !string.IsNullOrEmpty(memberSurface.ResolvedSkillName) ? memberSurface.ResolvedSkillName : "Skill";
        string skillShortText = memberSurface != null && !string.IsNullOrEmpty(memberSurface.ResolvedSkillShortText) ? memberSurface.ResolvedSkillShortText : string.Empty;
        if (memberSurface != null && !string.IsNullOrEmpty(memberSurface.EquipmentSummaryText))
        {
            skillShortText = string.IsNullOrEmpty(skillShortText)
                ? memberSurface.EquipmentSummaryText
                : skillShortText + " | " + memberSurface.EquipmentSummaryText;
        }

        int partySlotIndex = memberSurface != null ? memberSurface.PartySlotIndex : fallbackPartySlotIndex;

        PartySkillType skillType = PartySkillType.SingleHeavy;
        int skillPower = memberSurface != null && memberSurface.SkillPower > 0 ? memberSurface.SkillPower : Mathf.Max(1, runtimeState.Attack + 1);
        Color viewColor = Color.white;

        PrototypeRpgSkillDefinition sharedSkillDefinition = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, roleTag);
        string roleLabel = runtimeState.RoleLabel;
        ApplySharedSkillDefinition(sharedSkillDefinition, new PrototypeRpgStatBlock(runtimeState.MaxHp, runtimeState.Attack, runtimeState.Defense, runtimeState.Speed), ref roleLabel, ref defaultSkillId, ref skillName, ref skillShortText, ref skillType, ref skillPower, ref viewColor);

        if (string.IsNullOrEmpty(defaultSkillId) && !string.IsNullOrEmpty(roleTag))
        {
            PrototypeRpgSkillDefinition fallbackDefinition = PrototypeRpgSkillCatalog.GetFallbackDefinitionForRoleTag(roleTag);
            ApplySharedSkillDefinition(fallbackDefinition, new PrototypeRpgStatBlock(runtimeState.MaxHp, runtimeState.Attack, runtimeState.Defense, runtimeState.Speed), ref roleLabel, ref defaultSkillId, ref skillName, ref skillShortText, ref skillType, ref skillPower, ref viewColor);
        }

        if (string.IsNullOrEmpty(roleLabel))
        {
            roleLabel = string.IsNullOrEmpty(roleTag) ? "Adventurer" : roleTag;
        }

        if (string.IsNullOrEmpty(skillName))
        {
            skillName = "Skill";
        }

        return new DungeonPartyMemberRuntimeData(runtimeState, skillName, skillShortText, skillType, partySlotIndex, skillPower, viewColor);
    }

    private void ApplySharedSkillDefinition(
        PrototypeRpgSkillDefinition sharedSkillDefinition,
        PrototypeRpgStatBlock baseStats,
        ref string roleLabel,
        ref string defaultSkillId,
        ref string skillName,
        ref string skillShortText,
        ref PartySkillType skillType,
        ref int skillPower,
        ref Color viewColor)
    {
        if (sharedSkillDefinition == null)
        {
            return;
        }

        defaultSkillId = sharedSkillDefinition.SkillId;
        if (string.IsNullOrEmpty(skillName) || skillName == "Skill")
        {
            skillName = sharedSkillDefinition.DisplayName;
        }

        if (string.IsNullOrEmpty(skillShortText))
        {
            skillShortText = sharedSkillDefinition.ShortText;
        }

        if (string.IsNullOrEmpty(roleLabel) || roleLabel == "Adventurer")
        {
            roleLabel = string.IsNullOrEmpty(sharedSkillDefinition.RoleHint) ? roleLabel : sharedSkillDefinition.RoleHint;
        }

        skillType = ResolveSharedSkillType(sharedSkillDefinition);
        skillPower = sharedSkillDefinition.PowerValue > 0 ? sharedSkillDefinition.PowerValue : Mathf.Max(1, baseStats.Attack + 1);
        viewColor = ResolveSharedSkillViewColor(sharedSkillDefinition);
    }

    private PartySkillType ResolveSharedSkillType(PrototypeRpgSkillDefinition sharedSkillDefinition)
    {
        if (sharedSkillDefinition == null)
        {
            return PartySkillType.SingleHeavy;
        }

        if (sharedSkillDefinition.TargetKind == "all_allies" && sharedSkillDefinition.EffectType == "heal")
        {
            return PartySkillType.PartyHeal;
        }

        if (sharedSkillDefinition.TargetKind == "all_enemies" && sharedSkillDefinition.EffectType == "damage")
        {
            return PartySkillType.AllEnemies;
        }

        if (sharedSkillDefinition.TargetKind == "single_enemy" && sharedSkillDefinition.EffectType == "finisher_damage")
        {
            return PartySkillType.Finisher;
        }

        return PartySkillType.SingleHeavy;
    }

    private Color ResolveSharedSkillViewColor(PrototypeRpgSkillDefinition sharedSkillDefinition)
    {
        if (sharedSkillDefinition == null)
        {
            return Color.white;
        }

        switch (sharedSkillDefinition.SkillId)
        {
            case "skill_power_strike":
                return new Color(0.78f, 0.47f, 0.29f, 1f);
            case "skill_weak_point":
                return new Color(0.28f, 0.75f, 0.58f, 1f);
            case "skill_arcane_burst":
                return new Color(0.34f, 0.68f, 0.95f, 1f);
            case "skill_radiant_hymn":
                return new Color(0.94f, 0.84f, 0.46f, 1f);
            default:
                return Color.white;
        }
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
            party.PartyDefinition = BuildRuntimePartyDefinition(partyId) ?? PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
            party.DisplayName = party.PartyDefinition != null && !string.IsNullOrEmpty(party.PartyDefinition.DisplayName)
                ? party.PartyDefinition.DisplayName
                : partyId;
            party.Members = CreateDungeonRuntimeMembers(party.PartyDefinition);
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
        return DoesRpgOwnedSkillRequireTarget(member);
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
        ResetCombatRulePipelineState();
        ResetRoundActions();
        TrySelectNextPartyActor(0);
        SetActiveBattleMonster(GetFirstLivingBattleMonster());
        RecordCurrentPartyTurnStartEvent();

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
                _activeMonsters.Add(CreateMonster("beta-risky-elite", _eliteEncounterId, 3, _eliteName, "Goblin", 36, 6, EliteEncounterMarkerPosition, _eliteRewardAmount, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, true, 11, "Execution Strike"));
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
                _activeMonsters.Add(CreateMonster("beta-safe-elite", _eliteEncounterId, 3, _eliteName, "Goblin", 32, 5, EliteEncounterMarkerPosition, _eliteRewardAmount, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Bulwark, true, 9, "Command Strike"));
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
                _activeMonsters.Add(CreateMonster("alpha-risky-elite", _eliteEncounterId, 3, _eliteName, "Slime", 30, 5, EliteEncounterMarkerPosition, _eliteRewardAmount, MonsterTargetPattern.LowestHpLiving, MonsterEncounterRole.Striker, true, 10, "Core Rupture"));
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
                _activeMonsters.Add(CreateMonster("alpha-safe-elite", _eliteEncounterId, 3, _eliteName, "Slime", 34, 5, EliteEncounterMarkerPosition, _eliteRewardAmount, MonsterTargetPattern.RandomLiving, MonsterEncounterRole.Bulwark, true, 9, "Royal Wave"));
            }
        }

        _activeEncounters.Add(new DungeonEncounterRuntimeData("encounter-room-1", 1, _currentDungeonId == "dungeon-beta" ? (isRisky ? "Raider Gate" : "Scout Gate") : (isRisky ? "Mixed Front" : "Slime Front"), new[] { _activeMonsters[0].MonsterId, _activeMonsters[1].MonsterId }));
        _activeEncounters.Add(new DungeonEncounterRuntimeData("encounter-room-2", 2, _currentDungeonId == "dungeon-beta" ? (isRisky ? "Ambush Hall" : "Guarded Vault") : (isRisky ? "Goblin Pair Hall" : "Watch Hall"), new[] { _activeMonsters[2].MonsterId, _activeMonsters[3].MonsterId }));
        _activeEncounters.Add(new DungeonEncounterRuntimeData(_eliteEncounterId, 3, _eliteName, new[] { _activeMonsters[_activeMonsters.Count - 1].MonsterId }, true));

        BuildPlannedRoomSequence();
        EnsureDungeonVisuals();
        SyncMonsterVisuals();
    }
    private void ApplyExpeditionStartContext(ExpeditionStartContext launchContext)
    {
        if (launchContext == null)
        {
            return;
        }

        _currentHomeCityId = string.IsNullOrEmpty(launchContext.StartCityId) ? _currentHomeCityId : launchContext.StartCityId;
        _currentDungeonId = string.IsNullOrEmpty(launchContext.DungeonId) ? _currentDungeonId : launchContext.DungeonId;
        if (!string.IsNullOrEmpty(launchContext.DungeonLabel) && launchContext.DungeonLabel != "None")
        {
            _currentDungeonName = launchContext.DungeonLabel;
        }
        else if (!string.IsNullOrEmpty(_currentDungeonId))
        {
            string dungeonLabel = ResolveDispatchEntityDisplayName(_currentDungeonId);
            _currentDungeonName = string.IsNullOrEmpty(dungeonLabel) ? _currentDungeonId : dungeonLabel;
        }

        _recommendedRouteId = NormalizeRouteChoiceId(launchContext.RecommendedRouteId);
        _recommendedRouteLabel = string.IsNullOrEmpty(launchContext.RecommendedRouteLabel) ? "None" : launchContext.RecommendedRouteLabel;
        _recommendedRouteReason = string.IsNullOrEmpty(launchContext.RecommendationReasonText) ? "None" : launchContext.RecommendationReasonText;
        _expectedNeedImpactText = string.IsNullOrEmpty(launchContext.ExpectedNeedImpactText) ? "None" : launchContext.ExpectedNeedImpactText;
        _preRunNeedPressureText = string.IsNullOrEmpty(launchContext.NeedPressureText) ? "None" : launchContext.NeedPressureText;
        _preRunDispatchReadinessText = string.IsNullOrEmpty(launchContext.DispatchReadinessText) ? "None" : launchContext.DispatchReadinessText;
        _selectedRouteChoiceId = NormalizeRouteChoiceId(launchContext.SelectedRouteId);
        _selectedRouteId = _selectedRouteChoiceId;
        _selectedRouteLabel = string.IsNullOrEmpty(launchContext.SelectedRouteLabel) ? "None" : launchContext.SelectedRouteLabel;
        _selectedRouteDescription = string.IsNullOrEmpty(launchContext.RoutePreviewSummaryText) ? "None" : launchContext.RoutePreviewSummaryText;
        _selectedRouteRiskLabel = string.IsNullOrEmpty(launchContext.RouteRiskLabel) ? "None" : launchContext.RouteRiskLabel;
        _followedRecommendation = launchContext.FollowedRecommendation ||
            (!string.IsNullOrEmpty(_selectedRouteId) && _selectedRouteId == _recommendedRouteId);
    }

    private void StartDungeonRunForRoute(DungeonRouteTemplate template, string partyId)
    {
        StartDungeonRunForRoute(template, partyId, BuildProjectedExpeditionStartContext());
    }

    private void StartDungeonRunForRoute(DungeonRouteTemplate template, string partyId, ExpeditionStartContext launchContext)
    {
        if (template == null || string.IsNullOrEmpty(partyId))
        {
            return;
        }

        ApplyExpeditionStartContext(launchContext);
        _activeDungeonParty = GetOrCreateDungeonParty(_currentHomeCityId, partyId);
        if (_activeDungeonParty == null)
        {
            return;
        }

        _activeDungeonParty.ResetForRun();
        ResetCombatRulePipelineState();
        _latestRpgRunResultSnapshot = new PrototypeRpgRunResultSnapshot();
        ResetRpgProgressionSeedState();
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
        _battlesFoughtCount = 0;
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
        _latestExpeditionStartContext = CopyExpeditionStartContext(launchContext);
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
        string outcomeKey = resultState == RunResultState.Clear
            ? PrototypeBattleOutcomeKeys.RunClear
            : resultState == RunResultState.Defeat
                ? PrototypeBattleOutcomeKeys.RunDefeat
                : resultState == RunResultState.Retreat
                    ? PrototypeBattleOutcomeKeys.RunRetreat
                    : PrototypeBattleOutcomeKeys.None;
        string safeResultSummary = !string.IsNullOrEmpty(resultSummary)
            ? resultSummary
            : resultState == RunResultState.Clear
                ? "The run ended in victory."
                : resultState == RunResultState.Defeat
                    ? "The party was defeated."
                    : resultState == RunResultState.Retreat
                        ? "The party retreated."
                        : "The run ended.";

        CaptureDungeonRunResultSnapshotState(outcomeKey, safeReturnedLoot, safeResultSummary);


        if (resultState == RunResultState.Clear)
        {
            RecordBattleEvent(
                PrototypeBattleEventKeys.BattleVictory,
                string.Empty,
                _currentDungeonId,
                safeReturnedLoot,
                safeResultSummary,
                phaseKey: "victory",
                actorName: ActiveDungeonPartyText,
                targetName: _currentDungeonName,
                shortText: "Run clear");
        }
        else if (resultState == RunResultState.Defeat)
        {
            RecordBattleEvent(
                PrototypeBattleEventKeys.BattleDefeat,
                string.Empty,
                _currentDungeonId,
                0,
                safeResultSummary,
                phaseKey: "defeat",
                actorName: ActiveDungeonPartyText,
                targetName: _currentDungeonName,
                shortText: "Run defeat");
        }
        else if (resultState == RunResultState.Retreat)
        {
            RecordBattleEvent(
                PrototypeBattleEventKeys.RetreatConfirmed,
                string.Empty,
                _currentDungeonId,
                0,
                safeResultSummary,
                actionKey: "retreat",
                phaseKey: "retreat",
                actorName: ActiveDungeonPartyText,
                targetName: _currentDungeonName,
                shortText: "Run retreat");
        }

        RecordBattleEvent(
            PrototypeBattleEventKeys.BattleEnd,
            string.Empty,
            _currentDungeonId,
            safeReturnedLoot,
            safeResultSummary,
            phaseKey: resultState == RunResultState.Clear ? "victory" : resultState == RunResultState.Defeat ? "defeat" : resultState == RunResultState.Retreat ? "retreat" : GetBattlePhaseKey(),
            actorName: ActiveDungeonPartyText,
            targetName: _currentDungeonName,
            shortText: "Battle end");
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
        AppendBattleLog(safeResultSummary);
        AppendBattleLog("Party Condition at end: " + _resultPartyConditionText + " | HP: " + _resultPartyHpSummaryText + ".");
        if (_eliteRewardGranted && _resultEliteRewardAmount > 0)
        {
            AppendBattleLog("Elite reward granted: " + _resultEliteRewardLabel + " (" + BuildLootAmountText(_resultEliteRewardAmount) + ").");
        }
        SetBattleFeedbackText(resultState == RunResultState.Clear ? "Run clear." : resultState == RunResultState.Defeat ? "The party was defeated." : resultState == RunResultState.Retreat ? "The party retreated." : "Run complete.");

        EmitDungeonRunPostRunHandoff(outcomeKey, success, safeReturnedLoot);
        if (resultState != RunResultState.Clear)
        {
            OpenRunBattleResultPopover(outcomeKey, safeResultSummary, safeReturnedLoot);
        }

        AppendBattleLog("Returned " + BuildLootAmountText(safeReturnedLoot) + ".");
        if (_eliteBonusRewardGranted && _resultEliteBonusRewardAmount > 0)
        {
            AppendBattleLog("Elite bonus reward earned: " + BuildLootAmountText(_resultEliteBonusRewardAmount) + ".");
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
        ClearBattleResultPopover();
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
        _battlesFoughtCount = 0;
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
        ResetCombatRulePipelineState();
        _latestRpgRunResultSnapshot = new PrototypeRpgRunResultSnapshot();
        ResetRpgProgressionSeedState();
        _plannedRooms.Clear();
        _roomPathHistory.Clear();
        _activeMonsters.Clear();
        _activeEncounters.Clear();
        ClearMonsterVisuals();
        ClearBattleInputLock();
        ClearBattleResultPopover();
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
        return TrySelectRpgOwnedNextPartyActor(startIndex);
    }

    private void ResetRoundActions()
    {
        ResetRpgOwnedRoundActions();
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
        bool battleActive = _dungeonRunState == DungeonRunState.Battle;

        for (int x = 0; x < DungeonGridWidth; x++)
        {
            for (int y = 0; y < DungeonGridHeight; y++)
            {
                SpriteRenderer tile = _dungeonTileRenderers[x, y];
                if (tile == null)
                {
                    continue;
                }

                tile.gameObject.SetActive(!battleActive);
                if (battleActive)
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

                DungeonRoomTemplateData markerRoom = GetPlannedRoomByMarkerPosition(grid);
                if (markerRoom != null)
                {
                    Color markerColor = GetRoomMarkerColor(markerRoom.RoomType);
                    bool isCurrentMarker = currentRoom != null && markerRoom.RoomId == currentRoom.RoomId;
                    bool isResolvedMarker = IsRoomStepResolved(markerRoom);
                    color = isCurrentMarker
                        ? markerColor
                        : isResolvedMarker
                            ? Color.Lerp(markerColor, new Color(0.12f, 0.15f, 0.21f, 1f), 0.70f)
                            : Color.Lerp(markerColor, new Color(0.15f, 0.18f, 0.24f, 1f), 0.36f);
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
        if (_battleViewRoot != null && _battleViewRoot.activeSelf != battleActive)
        {
            _battleViewRoot.SetActive(battleActive);
        }

        if (!battleActive)
        {
            _battlePresentationStateStamp = int.MinValue;
            return;
        }

        bool hasActiveFeedback = HasActiveBattlePresentationFeedback();
        int presentationStamp = hasActiveFeedback ? int.MinValue : ComputeBattlePresentationStateStamp();
        if (!hasActiveFeedback && _battlePresentationStateStamp == presentationStamp)
        {
            return;
        }

        _battlePresentationStateStamp = presentationStamp;

        bool eliteActive = _eliteEncounterActive || (_activeBattleMonster != null && _activeBattleMonster.IsElite);
        Color backdropColor = eliteActive ? new Color(0.10f, 0.06f, 0.08f, 0.96f) : new Color(0.10f, 0.13f, 0.18f, 0.94f);
        Color headerColor = eliteActive ? new Color(0.34f, 0.18f, 0.18f, 0.96f) : new Color(0.17f, 0.26f, 0.35f, 0.96f);
        Color stageColor = eliteActive ? new Color(0.32f, 0.19f, 0.20f, 0.60f) : new Color(0.24f, 0.32f, 0.42f, 0.56f);
        if (_battleBackdropRenderer != null) _battleBackdropRenderer.color = new Color(backdropColor.r, backdropColor.g, backdropColor.b, 0f);
        if (_battleHeaderRenderer != null) _battleHeaderRenderer.color = new Color(headerColor.r, headerColor.g, headerColor.b, 0f);
        if (_battleStageRenderer != null) _battleStageRenderer.color = new Color(stageColor.r, stageColor.g, stageColor.b, 0f);
        if (_battleCommandPanelRenderer != null) _battleCommandPanelRenderer.color = new Color(0.08f, 0.10f, 0.13f, 0f);
        if (_battlePartyPanelRenderer != null) _battlePartyPanelRenderer.color = new Color(0.08f, 0.10f, 0.13f, 0f);
        if (_battleLogPanelRenderer != null) _battleLogPanelRenderer.color = new Color(0.08f, 0.10f, 0.13f, 0f);

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

            Vector3 position = GetBattlePartyViewPosition(i);
            if (plateRenderer != null)
            {
                plateRenderer.transform.localPosition = position + new Vector3(0f, -0.05f, 0f);
                plateRenderer.transform.localScale = new Vector3(1.72f, 1.38f, 1f);
                bool isCurrentActor = _battleState != BattleState.EnemyTurn && _currentActorIndex == i && !member.IsDefeated;
                Color plateColor = isCurrentActor ? new Color(0.46f, 0.66f, 0.88f, 0.98f) : new Color(0.30f, 0.36f, 0.46f, 0.94f);
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
                float scale = member.IsDefeated ? 1.10f : isCurrentActor ? 1.24f : 1.12f;
                viewRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                Color viewColor = i == 0 ? new Color(0.83f, 0.60f, 0.38f, 1f)
                    : i == 1 ? new Color(0.48f, 0.79f, 0.70f, 1f)
                    : i == 2 ? new Color(0.53f, 0.78f, 0.93f, 1f)
                    : new Color(0.92f, 0.86f, 0.58f, 1f);
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
                plateRenderer.transform.localScale = monster.IsElite ? new Vector3(2.28f, 1.76f, 1f) : new Vector3(1.96f, 1.50f, 1f);
                Color plateColor = monster.IsElite ? new Color(0.58f, 0.29f, 0.29f, 0.96f) : new Color(0.34f, 0.38f, 0.44f, 0.92f);
                if (monster.IsDefeated)
                {
                    plateColor = new Color(0.14f, 0.14f, 0.14f, 0.48f);
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
                bool isHovered = _hoverBattleMonsterId == monster.MonsterId && !monster.IsDefeated;
                bool isActing = _battleState == BattleState.EnemyTurn && _activeBattleMonsterId == monster.MonsterId && !monster.IsDefeated;
                float scale = monster.IsElite ? 1.55f : 1.28f;
                if (isHovered || isActing) scale += 0.10f;
                if (monster.IsDefeated) scale = monster.IsElite ? 1.30f : 1.12f;
                viewRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                viewRenderer.color = GetBattleMonsterViewColor(monster, i);
            }
        }
    }

    private bool HasActiveBattlePresentationFeedback()
    {
        float now = Time.unscaledTime;
        for (int i = 0; i < _partyFeedbackUntilTimes.Length; i++)
        {
            if (_partyFeedbackUntilTimes[i] > now)
            {
                return true;
            }
        }

        for (int i = 0; i < _monsterFeedbackUntilTimes.Length; i++)
        {
            if (_monsterFeedbackUntilTimes[i] > now)
            {
                return true;
            }
        }

        return false;
    }

    private int ComputeBattlePresentationStateStamp()
    {
        unchecked
        {
            int hash = 17;
            hash = CombineBattleStamp(hash, _dungeonRunState.GetHashCode());
            hash = CombineBattleStamp(hash, _battleState.GetHashCode());
            hash = CombineBattleStamp(hash, _currentActorIndex);
            hash = CombineBattleStamp(hash, _activeBattleMonsterId);
            hash = CombineBattleStamp(hash, _hoverBattleMonsterId);
            hash = CombineBattleStamp(hash, _pendingEnemyTargetIndex);
            hash = CombineBattleStamp(hash, _queuedBattleAction.GetHashCode());
            hash = CombineBattleStamp(hash, _eliteEncounterActive);

            DungeonPartyMemberRuntimeData[] members = _activeDungeonParty != null ? _activeDungeonParty.Members : null;
            int memberCount = members != null ? members.Length : 0;
            hash = CombineBattleStamp(hash, memberCount);
            for (int i = 0; i < memberCount; i++)
            {
                DungeonPartyMemberRuntimeData member = members[i];
                hash = CombineBattleStamp(hash, member != null);
                if (member == null)
                {
                    continue;
                }

                hash = CombineBattleStamp(hash, member.MemberId);
                hash = CombineBattleStamp(hash, GetPartyMemberLaneKey(member));
                hash = CombineBattleStamp(hash, member.CurrentHp);
                hash = CombineBattleStamp(hash, member.MaxHp);
                hash = CombineBattleStamp(hash, member.IsDefeated);
            }

            hash = CombineBattleStamp(hash, _activeMonsters.Count);
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                DungeonMonsterRuntimeData monster = _activeMonsters[i];
                hash = CombineBattleStamp(hash, monster != null);
                if (monster == null)
                {
                    continue;
                }

                hash = CombineBattleStamp(hash, monster.MonsterId);
                hash = CombineBattleStamp(hash, GetMonsterLaneKey(monster));
                hash = CombineBattleStamp(hash, monster.CurrentHp);
                hash = CombineBattleStamp(hash, monster.MaxHp);
                hash = CombineBattleStamp(hash, monster.IsDefeated);
                hash = CombineBattleStamp(hash, monster.IsElite);
            }

            return hash;
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
                GameObject token = CreateDungeonToken(_dungeonRoot.transform, "Monster_" + monster.MonsterId, GetGridLocalPosition(monster.GridPosition), new Vector2(0.74f, 0.74f), 38);
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
            renderer.transform.localScale = monster.IsElite ? new Vector3(1.06f, 1.06f, 1f) : new Vector3(0.90f, 0.90f, 1f);
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
                GameObject tile = CreateDungeonToken(_dungeonRoot.transform, "Tile_" + x + "_" + y, GetGridLocalPosition(new Vector2Int(x, y)), new Vector2(0.72f, 0.72f), 0);
                _dungeonTileRenderers[x, y] = tile != null ? tile.GetComponent<SpriteRenderer>() : null;
            }
        }

        _playerToken = CreateDungeonToken(_dungeonRoot.transform, "PlayerToken", GetGridLocalPosition(_playerGridPosition), new Vector2(0.66f, 0.66f), 40);
        _playerRenderer = _playerToken != null ? _playerToken.GetComponent<SpriteRenderer>() : null;
        _exitToken = CreateDungeonToken(_dungeonRoot.transform, "ExitToken", GetGridLocalPosition(_exitGridPosition), new Vector2(0.70f, 0.70f), 39);
        _exitRenderer = _exitToken != null ? _exitToken.GetComponent<SpriteRenderer>() : null;
        _chestToken = CreateDungeonToken(_dungeonRoot.transform, "ChestToken", GetGridLocalPosition(Room2ChestGridPosition), new Vector2(0.64f, 0.64f), 39);
        _chestRenderer = _chestToken != null ? _chestToken.GetComponent<SpriteRenderer>() : null;
        _eventToken = CreateDungeonToken(_dungeonRoot.transform, "EventToken", GetGridLocalPosition(_eventGridPosition), new Vector2(0.64f, 0.64f), 39);
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
            GameObject plate = CreateBattleVisual(_battleViewRoot.transform, "PartyPlate_" + i, GetBattlePartyViewPosition(i), new Vector2(1.85f, 1.45f), 104);
            _battlePartyPlateRenderers[i] = plate.GetComponent<SpriteRenderer>();
            GameObject view = CreateBattleVisual(_battleViewRoot.transform, "PartyView_" + i, GetBattlePartyViewPosition(i), new Vector2(1.05f, 1.05f), 105);
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
