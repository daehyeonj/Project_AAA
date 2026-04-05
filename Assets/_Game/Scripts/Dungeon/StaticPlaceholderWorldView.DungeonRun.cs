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
        public PrototypeRpgPartyMemberRuntimeState RuntimeState { get; }
        public string MemberId => RuntimeState != null ? RuntimeState.MemberId : string.Empty;
        public string DisplayName => RuntimeState != null ? RuntimeState.DisplayName : "Adventurer";
        public string RoleLabel => RuntimeState != null ? RuntimeState.RoleLabel : "Adventurer";
        public string RoleTag => RuntimeState != null ? RuntimeState.RoleTag : "adventurer";
        public string DefaultSkillId => RuntimeState != null ? RuntimeState.DefaultSkillId : string.Empty;
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

        public DungeonPartyMemberRuntimeData(string memberId, string displayName, string roleLabel, string roleTag, string defaultSkillId, string skillName, string skillShortText, PartySkillType skillType, int partySlotIndex, int maxHp, int attack, int defense, int speed, int skillPower, Color viewColor)
        {
            RuntimeState = new PrototypeRpgPartyMemberRuntimeState(memberId, displayName, roleTag, roleLabel, defaultSkillId, maxHp, attack, defense, speed);
            SkillName = string.IsNullOrEmpty(skillName) ? "Skill" : skillName;
            SkillShortText = string.IsNullOrEmpty(skillShortText) ? string.Empty : skillShortText;
            SkillType = skillType;
            PartySlotIndex = partySlotIndex >= 0 ? partySlotIndex : 0;
            SkillPower = skillPower > 0 ? skillPower : Attack + 1;
            ViewColor = viewColor.a > 0f ? viewColor : Color.white;
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
        public PrototypeRpgEnemyDefinition Definition { get; }
        public PrototypeRpgEnemyRuntimeState RuntimeState { get; }
        public string MonsterId => RuntimeState != null ? RuntimeState.EnemyId : string.Empty;
        public string EncounterId => RuntimeState != null ? RuntimeState.EncounterId : string.Empty;
        public int RoomIndex;
        public string DisplayName => RuntimeState != null ? RuntimeState.DisplayName : (Definition != null && !string.IsNullOrEmpty(Definition.DisplayName) ? Definition.DisplayName : "Monster");
        public string MonsterType => RuntimeState != null ? RuntimeState.TypeLabel : (Definition != null && !string.IsNullOrEmpty(Definition.TypeLabel) ? Definition.TypeLabel : "Monster");
        public int MaxHp => RuntimeState != null ? RuntimeState.MaxHp : (Definition != null ? Definition.MaxHp : 1);
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
        public int Attack => RuntimeState != null ? RuntimeState.AttackPower : (Definition != null ? Definition.AttackPower : 1);
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
        public string DefaultIntentKey => Definition != null ? Definition.DefaultIntentKey : string.Empty;
        public string SpecialIntentKey => Definition != null ? Definition.SpecialIntentKey : string.Empty;
        public string RoleTag => Definition != null && !string.IsNullOrEmpty(Definition.RoleTag) ? Definition.RoleTag : ResolveMonsterRoleTag(EncounterRole, IsElite);
        public string RoleLabel => Definition != null && !string.IsNullOrEmpty(Definition.RoleLabel) ? Definition.RoleLabel : GetFallbackMonsterRoleLabel(EncounterRole, IsElite);
        public string TraitText => Definition != null && !string.IsNullOrEmpty(Definition.TraitText) ? Definition.TraitText : string.Empty;
        public string BehaviorHintText => Definition != null && Definition.BehaviorHint != null ? Definition.BehaviorHint.DisplayHintText : string.Empty;

        public DungeonMonsterRuntimeData(PrototypeRpgEnemyDefinition definition, string monsterId, string encounterId, int roomIndex, string displayName, string monsterType, int hp, int attack, Vector2Int gridPosition, string rewardResourceId, int rewardAmount, MonsterTargetPattern targetPattern, MonsterEncounterRole encounterRole = MonsterEncounterRole.Bulwark, bool isElite = false, int specialAttack = 0, string specialActionName = "")
        {
            string fallbackRoleTag = ResolveMonsterRoleTag(encounterRole, isElite);
            string fallbackRoleLabel = GetFallbackMonsterRoleLabel(encounterRole, isElite);
            string fallbackDefaultIntentKey = ResolveDefaultEnemyIntentKey(targetPattern);
            string fallbackSpecialIntentKey = ResolveSpecialEnemyIntentKey(isElite, encounterRole, specialActionName);
            string fallbackBehaviorHintKey = ResolveEnemyBehaviorHintKey(encounterRole, isElite, specialActionName);
            Definition = definition ?? PrototypeRpgEnemyCatalog.BuildFallbackDefinition(monsterId, displayName, monsterType, fallbackRoleTag, fallbackRoleLabel, hp, attack, isElite, fallbackDefaultIntentKey, fallbackSpecialIntentKey, fallbackBehaviorHintKey, string.Empty, rewardResourceId, rewardAmount, specialActionName, specialAttack, fallbackRoleLabel);
            RuntimeState = new PrototypeRpgEnemyRuntimeState(!string.IsNullOrEmpty(Definition.EnemyId) ? Definition.EnemyId : monsterId, encounterId, !string.IsNullOrEmpty(Definition.DisplayName) ? Definition.DisplayName : displayName, !string.IsNullOrEmpty(Definition.TypeLabel) ? Definition.TypeLabel : monsterType, Definition.MaxHp > 0 ? Definition.MaxHp : hp, Definition.AttackPower > 0 ? Definition.AttackPower : attack, Definition.IsElite || isElite);
            RoomIndex = roomIndex;
            GridPosition = gridPosition;
            RewardResourceId = !string.IsNullOrEmpty(Definition.RewardResourceId) ? Definition.RewardResourceId : string.IsNullOrEmpty(rewardResourceId) ? DungeonRewardResourceId : rewardResourceId;
            RewardAmount = rewardAmount > 0 ? rewardAmount : Mathf.Max(1, Definition.RewardAmountHint > 0 ? Definition.RewardAmountHint : 1);
            EncounterRole = encounterRole;
            TargetPattern = targetPattern;
            SpecialAttack = specialAttack > 0 ? specialAttack : Mathf.Max(Definition.SpecialPowerHint > 0 ? Definition.SpecialPowerHint : Attack + 2, Attack);
            SpecialActionName = !string.IsNullOrEmpty(specialActionName) ? specialActionName : !string.IsNullOrEmpty(Definition.SpecialActionLabel) ? Definition.SpecialActionLabel : "Heavy Strike";
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
        public PrototypeRpgEncounterDefinition Definition { get; }
        public PrototypeRpgEncounterRuntimeState RuntimeState { get; }
        public string EncounterId => RuntimeState != null ? RuntimeState.EncounterId : (Definition != null ? Definition.EncounterId : string.Empty);
        public int RoomIndex => RuntimeState != null ? RuntimeState.RoomIndex : 0;
        public string DisplayName => RuntimeState != null ? RuntimeState.DisplayName : (Definition != null ? Definition.DisplayName : string.Empty);
        public string[] MonsterIds => RuntimeState != null ? RuntimeState.EnemyIds : System.Array.Empty<string>();
        public bool IsEliteEncounter => RuntimeState != null ? RuntimeState.IsEliteEncounter : (Definition != null && Definition.IsEliteEncounter);
        public bool IsCleared
        {
            get => RuntimeState != null && RuntimeState.IsCleared;
            set
            {
                if (RuntimeState != null)
                {
                    RuntimeState.SetCleared(value);
                }
            }
        }

        public DungeonEncounterRuntimeData(PrototypeRpgEncounterDefinition definition, string encounterId, int roomIndex, string displayName, string[] monsterIds, bool isEliteEncounter = false)
        {
            Definition = definition ?? PrototypeRpgEncounterCatalog.BuildFallbackDefinition(string.Empty, encounterId, displayName, isEliteEncounter ? "Elite" : "Skirmish", isEliteEncounter ? "Elite Chamber" : "Skirmish Room", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, 0, isEliteEncounter, System.Array.Empty<PrototypeRpgEncounterMemberDefinition>());
            RuntimeState = new PrototypeRpgEncounterRuntimeState(
                Definition != null ? Definition.DefinitionId : string.Empty,
                encounterId,
                roomIndex,
                !string.IsNullOrEmpty(Definition.DisplayName) ? Definition.DisplayName : displayName,
                Definition != null ? Definition.EncounterTypeLabel : string.Empty,
                Definition != null ? Definition.RoomTypeLabel : string.Empty,
                Definition != null ? Definition.EliteStyleLabel : string.Empty,
                Definition != null ? Definition.RouteRiskLabel : string.Empty,
                Definition != null ? Definition.DangerHintLabel : string.Empty,
                Definition != null ? Definition.RewardPreviewHint : string.Empty,
                Definition != null ? Definition.RewardLabel : string.Empty,
                Definition != null ? Definition.RewardAmountHint : 0,
                monsterIds,
                Definition.IsEliteEncounter || isEliteEncounter);
        }

        public void ResetForRun()
        {
            RuntimeState?.ResetForRun();
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
    private readonly Dictionary<string, PrototypeRpgAppliedPartyProgressState> _appliedPartyProgressBySessionKey = new Dictionary<string, PrototypeRpgAppliedPartyProgressState>();
    private string _latestAppliedPartyProgressSessionKey = string.Empty;
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
    private int _resultLostPendingRewardAmount;
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
    public PrototypeRpgRunResultSnapshot LatestRpgRunResultSnapshot => CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
    public PrototypeRpgProgressionSeedSnapshot LatestRpgProgressionSeedSnapshot => CopyRpgProgressionSeedSnapshot(_latestRpgProgressionSeedSnapshot);
    public PrototypeRpgCombatContributionSnapshot LatestRpgCombatContributionSnapshot => CopyRpgCombatContributionSnapshot(_latestRpgCombatContributionSnapshot);
    public PrototypeRpgProgressionPreviewSnapshot LatestRpgProgressionPreviewSnapshot => CopyRpgProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot);
    public PrototypeRpgPostRunSummarySurfaceData LatestRpgPostRunSummarySurfaceData => BuildRpgPostRunSummarySurfaceData();
    public PrototypeRpgPendingRewardDeltaPack LatestRpgPendingRewardDeltaPack => BuildRpgPendingRewardDeltaPack();
    public PrototypeRpgPresentationGatewayData LatestRpgPresentationGatewayData => BuildLatestRpgPresentationGateway();
    public PrototypeBattleInteractionSurfaceData LatestBattleInteractionSurfaceData => BuildBattleInteractionSurfaceData();
    public PrototypeRpgBattleRuntimeState LatestRpgBattleRuntimeState => BuildBattleRuntimeState();
    public PrototypeRpgPostRunUpgradeOfferSurface LatestRpgPostRunUpgradeOfferSurface => BuildRpgPostRunUpgradeOfferSurface();
    public PrototypeRpgAppliedPartyProgressState LatestRpgAppliedPartyProgressState => CopyRpgAppliedPartyProgressState(GetLatestAppliedPartyProgressStateInternal());

    public PrototypeRpgAppliedPartyProgressState GetAppliedPartyProgressState()
    {
        return CopyRpgAppliedPartyProgressState(GetLatestAppliedPartyProgressStateInternal());
    }

    public PrototypeRpgPostRunSummarySurfaceData GetPostRunSummarySurfaceData()
    {
        return BuildRpgPostRunSummarySurfaceData();
    }

    public PrototypeRpgPendingRewardDeltaPack GetPendingRewardDeltaPack()
    {
        return BuildRpgPendingRewardDeltaPack();
    }

    public PrototypeRpgPostRunUpgradeOfferSurface GetPostRunUpgradeOfferSurface()
    {
        return BuildRpgPostRunUpgradeOfferSurface();
    }

    public PrototypeRpgPresentationGatewayData GetRpgPresentationGatewayData()
    {
        return BuildLatestRpgPresentationGateway();
    }

    public PrototypeBattleInteractionSurfaceData GetBattleInteractionSurfaceData()
    {
        return BuildBattleInteractionSurfaceData();
    }

    public PrototypeRpgBattleRuntimeState GetBattleRuntimeState()
    {
        return BuildBattleRuntimeState();
    }

    public PrototypeRpgDungeonSelectionPreviewData GetSelectedDungeonSelectionPreviewData()
    {
        return BuildSelectedDungeonSelectionPreviewData();
    }
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
        PrototypeRpgMemberContributionSnapshot snapshot = BuildLiveBattleMemberContributionSnapshot(memberIndex);
        return BuildPartyMemberContributionSummaryText(snapshot);
    }

    private string BuildPartyMemberContributionSummaryText(PrototypeRpgMemberContributionSnapshot snapshot)
    {
        PrototypeRpgMemberContributionSnapshot safeSnapshot = snapshot ?? new PrototypeRpgMemberContributionSnapshot();
        if (!string.IsNullOrEmpty(safeSnapshot.ContributionSummaryText))
        {
            return safeSnapshot.ContributionSummaryText;
        }

        return "D " + Mathf.Max(0, safeSnapshot.DamageDealt) +
               "  H " + Mathf.Max(0, safeSnapshot.HealingDone) +
               "  A " + Mathf.Max(0, safeSnapshot.ActionCount) +
               "  K " + Mathf.Max(0, safeSnapshot.KillCount);
    }

    private PrototypeRpgMemberContributionSnapshot BuildLiveBattleMemberContributionSnapshot(int memberIndex)
    {
        PrototypeRpgCombatContributionSnapshot snapshot = BuildCurrentBattleContributionSnapshotView();
        PrototypeRpgMemberContributionSnapshot[] members = snapshot != null ? (snapshot.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>()) : System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        if (memberIndex < 0 || memberIndex >= members.Length)
        {
            return new PrototypeRpgMemberContributionSnapshot();
        }

        return CopyRpgMemberContributionSnapshot(members[memberIndex]);
    }

    private PrototypeRpgCombatContributionSnapshot BuildCurrentBattleContributionSnapshotView()
    {
        if ((_dungeonRunState == DungeonRunState.ResultPanel || (_runResultState != RunResultState.None && _runResultState != RunResultState.Playing)) &&
            HasRpgCombatContributionSnapshotData(_latestRpgCombatContributionSnapshot))
        {
            return CopyRpgCombatContributionSnapshot(_latestRpgCombatContributionSnapshot);
        }

        return BuildLiveBattleContributionSnapshot();
    }

    private PrototypeRpgCombatContributionSnapshot BuildLiveBattleContributionSnapshot()
    {
        PrototypeRpgCombatContributionSnapshot snapshot = new PrototypeRpgCombatContributionSnapshot();
        snapshot.ResultStateKey = GetActiveBattleContributionResultStateKey();
        snapshot.DungeonId = string.IsNullOrEmpty(_currentDungeonId) ? string.Empty : _currentDungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(_currentDungeonName) ? string.Empty : _currentDungeonName;
        snapshot.RouteId = string.IsNullOrEmpty(_selectedRouteId) ? string.Empty : _selectedRouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? string.Empty : _selectedRouteLabel;
        snapshot.TotalTurnsTaken = Mathf.Max(_runTurnCount, _battleTurnIndex);
        snapshot.SurvivingMemberCount = GetLivingPartyMemberCount();
        snapshot.KnockedOutMemberCount = GetKnockedOutMemberCount();
        snapshot.TotalDamageDealt = Mathf.Max(0, _totalDamageDealt);
        snapshot.TotalDamageTaken = Mathf.Max(0, _totalDamageTaken);
        snapshot.TotalHealingDone = Mathf.Max(0, _totalHealingDone);
        snapshot.RecentEvents = BuildRecentBattleEventRecords();

        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || _activeDungeonParty.Members.Length <= 0)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
            return snapshot;
        }

        PrototypeRpgMemberContributionSnapshot[] members = new PrototypeRpgMemberContributionSnapshot[_activeDungeonParty.Members.Length];
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            PrototypeRpgMemberContributionSnapshot memberSnapshot = new PrototypeRpgMemberContributionSnapshot();
            if (member != null)
            {
                memberSnapshot.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
                memberSnapshot.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? string.Empty : member.DisplayName;
                memberSnapshot.RoleTag = string.IsNullOrEmpty(member.RoleTag) ? string.Empty : member.RoleTag;
                memberSnapshot.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? string.Empty : member.RoleLabel;
                memberSnapshot.DefaultSkillId = string.IsNullOrEmpty(member.DefaultSkillId) ? string.Empty : member.DefaultSkillId;
                memberSnapshot.DamageDealt = GetRunMemberContributionValue(_runMemberDamageDealt, i);
                memberSnapshot.DamageTaken = GetRunMemberContributionValue(_runMemberDamageTaken, i);
                memberSnapshot.HealingDone = GetRunMemberContributionValue(_runMemberHealingDone, i);
                memberSnapshot.ActionCount = GetRunMemberContributionValue(_runMemberActionCount, i);
                memberSnapshot.KillCount = GetRunMemberContributionValue(_runMemberKillCount, i);
                memberSnapshot.KnockedOut = member.IsDefeated || member.CurrentHp <= 0;
                memberSnapshot.Survived = !memberSnapshot.KnockedOut;
                memberSnapshot.EliteVictor = _eliteDefeated && memberSnapshot.Survived;
            }

            members[i] = memberSnapshot;
        }

        snapshot.Members = members;
        return snapshot;
    }

    private string GetActiveBattleContributionResultStateKey()
    {
        switch (_runResultState)
        {
            case RunResultState.Clear:
                return PrototypeBattleOutcomeKeys.RunClear;
            case RunResultState.Defeat:
                return PrototypeBattleOutcomeKeys.RunDefeat;
            case RunResultState.Retreat:
                return PrototypeBattleOutcomeKeys.RunRetreat;
        }

        return _battleState == BattleState.Victory
            ? PrototypeBattleOutcomeKeys.EncounterVictory
            : PrototypeBattleOutcomeKeys.None;
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
        PrototypeRpgBattleRuntimeState runtime = BuildBattleRuntimeState();
        PrototypeRpgEncounterRuntimeState encounterRuntime = runtime != null ? runtime.EncounterRuntime : null;

        PrototypeBattleUiSurfaceData surface = new PrototypeBattleUiSurfaceData();
        surface.IsBattleActive = runtime != null && runtime.IsBattleActive;
        surface.IsTargetSelectionActive = runtime != null && runtime.IsTargetSelectionActive;
        surface.BattleStateKey = runtime != null && !string.IsNullOrEmpty(runtime.BattleStateKey) ? runtime.BattleStateKey : GetBattleStateKey();
        surface.CurrentDungeonName = runtime != null && !string.IsNullOrEmpty(runtime.CurrentDungeonName) ? runtime.CurrentDungeonName : (string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName);
        surface.CurrentRouteLabel = runtime != null && !string.IsNullOrEmpty(runtime.CurrentRouteLabel) ? runtime.CurrentRouteLabel : (string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel);
        surface.EncounterName = encounterRuntime != null && !string.IsNullOrEmpty(encounterRuntime.DisplayName) ? encounterRuntime.DisplayName : GetCurrentEncounterNameText();
        surface.EncounterRoomType = encounterRuntime != null && !string.IsNullOrEmpty(encounterRuntime.RoomTypeLabel) ? encounterRuntime.RoomTypeLabel : GetEncounterRoomTypeText();
        surface.PartyCondition = runtime != null && !string.IsNullOrEmpty(runtime.PartyConditionText) ? runtime.PartyConditionText : GetPartyConditionText();
        surface.TotalPartyHp = runtime != null && !string.IsNullOrEmpty(runtime.TotalPartyHpText) ? runtime.TotalPartyHpText : BuildTotalPartyHpSummary();
        surface.EliteStatusText = runtime != null && !string.IsNullOrEmpty(runtime.EliteStatusText) ? runtime.EliteStatusText : GetEliteStatusText();
        surface.EliteEncounterName = runtime != null && !string.IsNullOrEmpty(runtime.EliteEncounterName) ? runtime.EliteEncounterName : GetEliteEncounterNameText();
        surface.EliteTypeText = runtime != null && !string.IsNullOrEmpty(runtime.EliteTypeText) ? runtime.EliteTypeText : (string.IsNullOrEmpty(_eliteType) ? "None" : _eliteType);
        surface.EliteRewardHintText = runtime != null && !string.IsNullOrEmpty(runtime.EliteRewardHintText) ? runtime.EliteRewardHintText : GetEliteRewardHintText();
        surface.CurrentActor = BuildBattleUiCurrentActorData();
        surface.Timeline = BuildBattleUiTimelineData();
        surface.PartyMembers = BuildBattleUiPartyMembers();
        surface.SelectedEnemy = BuildSelectedBattleUiEnemyData(runtime);
        surface.EnemyRoster = BuildBattleUiEnemyRoster(runtime);
        if ((surface.SelectedEnemy == null || string.IsNullOrEmpty(surface.SelectedEnemy.MonsterId)) && surface.EnemyRoster.Length > 0)
        {
            surface.SelectedEnemy = surface.EnemyRoster[0];
        }

        surface.ActionContext = BuildBattleUiActionContextData(runtime, surface.CurrentActor, surface.SelectedEnemy);
        surface.TargetContext = BuildBattleUiTargetContextData(runtime, surface.SelectedEnemy);
        surface.CommandSurface = BuildBattleUiCommandSurfaceData(surface.CurrentActor, surface.ActionContext);
        surface.MessageSurface = BuildBattleUiMessageSurfaceData(runtime);
        surface.TargetSelection = BuildBattleUiTargetSelectionData(runtime, surface.CurrentActor, surface.ActionContext, surface.TargetContext);
        surface.EnemyIntent = runtime != null ? runtime.EnemyIntent : BuildCurrentEnemyIntentSnapshotView();
        surface.RecentEvents = runtime != null ? runtime.RecentEvents : BuildRecentBattleEventRecords();
        surface.ResultSnapshot = runtime != null ? runtime.ResultSnapshot : BuildCurrentBattleResultSnapshotView();
        surface.PartyContribution = runtime != null ? runtime.PartyContribution : BuildCurrentBattleContributionSnapshotView();
        return surface;
    }

    private PrototypeBattleInteractionSurfaceData BuildBattleInteractionSurfaceData()
    {
        PrototypeRpgBattleRuntimeState runtime = BuildBattleRuntimeState();
        PrototypeBattleInteractionSurfaceData surface = new PrototypeBattleInteractionSurfaceData();
        PrototypeBattleUiActorData actor = BuildBattleUiCurrentActorData();
        PrototypeBattleUiEnemyData selectedEnemy = BuildSelectedBattleUiEnemyData(runtime);
        PrototypeBattleUiActionContextData actionContext = BuildBattleUiActionContextData(runtime, actor, selectedEnemy);
        PrototypeBattleUiTargetContextData targetContext = BuildBattleUiTargetContextData(runtime, selectedEnemy);
        PrototypeBattleUiTargetSelectionData targetSelection = BuildBattleUiTargetSelectionData(runtime, actor, actionContext, targetContext);

        surface.HasInteractionSurface = runtime != null && runtime.IsBattleActive;
        surface.BattlePhaseKey = runtime != null && !string.IsNullOrEmpty(runtime.BattleStateKey) ? runtime.BattleStateKey : GetBattleStateKey();
        surface.CurrentActorId = runtime != null && !string.IsNullOrEmpty(runtime.CurrentActorId) ? runtime.CurrentActorId : actor != null && !string.IsNullOrEmpty(actor.ActorId) ? actor.ActorId : string.Empty;
        surface.CurrentActorLabel = runtime != null && !string.IsNullOrEmpty(runtime.CurrentActorLabel) ? runtime.CurrentActorLabel : actor != null && !string.IsNullOrEmpty(actor.DisplayName) ? actor.DisplayName : "None";
        surface.CurrentActorRoleLabel = actor != null && !string.IsNullOrEmpty(actor.RoleLabel) ? actor.RoleLabel : string.Empty;
        surface.SelectedActionKey = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionKey) ? actionContext.SelectedActionKey : string.Empty;
        surface.SelectedActionLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionLabel) ? actionContext.SelectedActionLabel : "Action";
        surface.MenuModeKey = runtime != null && !string.IsNullOrEmpty(runtime.PhaseKey) ? runtime.PhaseKey : GetBattleInteractionMenuModeKey();
        surface.CommandEntries = BuildBattleInteractionCommandEntries(actor, actionContext);
        surface.TargetCandidates = BuildBattleInteractionTargetCandidates(runtime);
        surface.InputHints = BuildBattleInteractionInputHintData(targetSelection);
        surface.ConfirmContext = BuildBattleInteractionConfirmContextData(actionContext);
        return surface;
    }

    private string GetBattleInteractionMenuModeKey()
    {
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            return "inactive";
        }

        if (IsBattleInputLocked())
        {
            return _battleState == BattleState.EnemyTurn ? "enemy_locked" : "input_locked";
        }

        switch (_battleState)
        {
            case BattleState.PartyActionSelect:
                return "action_select";
            case BattleState.PartyTargetSelect:
                return "target_select";
            case BattleState.EnemyTurn:
                return "enemy_turn";
            case BattleState.Victory:
                return "victory";
            case BattleState.Defeat:
                return "defeat";
            case BattleState.Retreat:
                return "retreat";
            default:
                return "battle";
        }
    }

    private PrototypeBattleCommandEntryData[] BuildBattleInteractionCommandEntries(PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext)
    {
        string skillLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedSkillLabel)
            ? actionContext.ResolvedSkillLabel
            : actor != null && !string.IsNullOrEmpty(actor.SkillLabel) && actor.SkillLabel != "None"
                ? actor.SkillLabel
                : "Skill";
        PrototypeBattleCommandEntryData[] entries = new PrototypeBattleCommandEntryData[4];
        entries[0] = BuildBattleInteractionCommandEntryData(BattleActionType.Attack, "Attack", "[1]", "Reliable basic strike.", actor, actionContext);
        entries[1] = BuildBattleInteractionCommandEntryData(BattleActionType.Skill, skillLabel, "[2]", actor != null && !string.IsNullOrEmpty(actor.SkillShortText) ? actor.SkillShortText : "Use the active actor's shared skill definition.", actor, actionContext);
        entries[2] = BuildBattleInteractionCommandEntryData(BattleActionType.None, "Items", "[I]", "Reserved for a later inventory batch.", actor, actionContext);
        entries[3] = BuildBattleInteractionCommandEntryData(BattleActionType.Retreat, "Retreat", "[3]/[Q]", "Leave the run using the current resolution flow.", actor, actionContext);
        return entries;
    }

    private PrototypeBattleCommandEntryData BuildBattleInteractionCommandEntryData(BattleActionType action, string displayLabel, string keyboardShortcutHint, string fallbackHintText, PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext)
    {
        PrototypeBattleCommandEntryData entry = new PrototypeBattleCommandEntryData();
        string actionKey = action == BattleActionType.None ? "item" : GetBattleActionKey(action);
        bool isAvailable = action != BattleActionType.None && IsBattleActionAvailable(action);
        entry.ActionKey = string.IsNullOrEmpty(actionKey) ? string.Empty : actionKey;
        entry.DisplayLabel = string.IsNullOrEmpty(displayLabel) ? "Action" : displayLabel;
        entry.IsAvailable = isAvailable;
        entry.IsHovered = action != BattleActionType.None && _dungeonRunState == DungeonRunState.Battle && _hoverBattleAction == action;
        entry.IsSelected = action != BattleActionType.None && _dungeonRunState == DungeonRunState.Battle && _queuedBattleAction == action;
        entry.AvailabilityReasonKey = isAvailable ? string.Empty : GetBattleInteractionCommandAvailabilityReasonKey(action);
        entry.HintText = BuildBattleInteractionCommandHintText(action, fallbackHintText, actor, actionContext);
        entry.KeyboardShortcutHint = string.IsNullOrEmpty(keyboardShortcutHint) ? string.Empty : keyboardShortcutHint;
        return entry;
    }

    private string GetBattleInteractionCommandAvailabilityReasonKey(BattleActionType action)
    {
        if (action == BattleActionType.None)
        {
            return "unavailable_not_implemented";
        }

        if (_dungeonRunState != DungeonRunState.Battle)
        {
            return "unavailable_wrong_phase";
        }

        if (IsBattleInputLocked())
        {
            return "unavailable_confirm_only";
        }

        if (action == BattleActionType.Attack || action == BattleActionType.Skill)
        {
            if (GetCurrentActorMember() == null)
            {
                return "unavailable_no_actor";
            }

            if (_battleState == BattleState.PartyTargetSelect)
            {
                return "unavailable_target_required";
            }

            return "unavailable_wrong_phase";
        }

        if (action == BattleActionType.Retreat)
        {
            return _battleState == BattleState.PartyActionSelect || _battleState == BattleState.PartyTargetSelect
                ? string.Empty
                : "unavailable_wrong_phase";
        }

        return "unavailable_wrong_phase";
    }

    private string BuildBattleInteractionCommandHintText(BattleActionType action, string fallbackHintText, PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext)
    {
        if (action == BattleActionType.Attack)
        {
            return "Reliable basic strike.";
        }

        if (action == BattleActionType.Skill)
        {
            if (actor != null && !string.IsNullOrEmpty(actor.SkillShortText))
            {
                return actor.SkillShortText;
            }

            if (actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedEffectType))
            {
                return GetBattleUiEffectText(actionContext.ResolvedEffectType, actionContext.ResolvedPowerValue);
            }
        }

        if (action == BattleActionType.Retreat)
        {
            return "End the run and return through the current writeback path.";
        }

        return string.IsNullOrEmpty(fallbackHintText) ? string.Empty : fallbackHintText;
    }

    private PrototypeBattleTargetCandidateData[] BuildBattleInteractionTargetCandidates()
    {
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            return System.Array.Empty<PrototypeBattleTargetCandidateData>();
        }

        PrototypeBattleUiEnemyData[] enemyRoster = BuildBattleUiEnemyRoster();
        if (enemyRoster == null || enemyRoster.Length == 0)
        {
            return System.Array.Empty<PrototypeBattleTargetCandidateData>();
        }

        PrototypeBattleTargetCandidateData[] candidates = new PrototypeBattleTargetCandidateData[enemyRoster.Length];
        bool targetingPhase = _battleState == BattleState.PartyTargetSelect;
        for (int i = 0; i < enemyRoster.Length; i++)
        {
            PrototypeBattleUiEnemyData enemy = enemyRoster[i] ?? new PrototypeBattleUiEnemyData();
            PrototypeBattleTargetCandidateData candidate = new PrototypeBattleTargetCandidateData();
            candidate.TargetId = string.IsNullOrEmpty(enemy.MonsterId) ? string.Empty : enemy.MonsterId;
            candidate.DisplayLabel = string.IsNullOrEmpty(enemy.DisplayName) ? "Monster" : enemy.DisplayName;
            candidate.RoleLabel = string.IsNullOrEmpty(enemy.RoleLabel) ? string.Empty : enemy.RoleLabel;
            candidate.TypeLabel = string.IsNullOrEmpty(enemy.TypeLabel) ? string.Empty : enemy.TypeLabel;
            candidate.CurrentHp = Mathf.Max(0, enemy.CurrentHp);
            candidate.MaxHp = Mathf.Max(1, enemy.MaxHp);
            candidate.IsDefeated = enemy.IsDefeated;
            candidate.IsHovered = enemy.IsHovered;
            candidate.IsSelected = enemy.IsSelected;
            candidate.IsAvailable = targetingPhase && !enemy.IsDefeated && enemy.CurrentHp > 0;
            candidate.TargetIndex = GetBattleMonsterDisplayIndex(enemy.MonsterId);
            if (candidate.TargetIndex < 0)
            {
                candidate.TargetIndex = i;
            }

            candidates[i] = candidate;
        }

        return candidates;
    }

    private PrototypeBattleInputHintData BuildBattleInteractionInputHintData(PrototypeBattleUiTargetSelectionData targetSelection)
    {
        PrototypeBattleInputHintData hints = new PrototypeBattleInputHintData();
        hints.ActionHint = _battleState == BattleState.PartyActionSelect
            ? "[1] Attack  [2] Skill  [3]/[Q] Retreat"
            : _battleState == BattleState.EnemyTurn
                ? "Enemy action resolving"
                : string.Empty;
        hints.TargetHint = _battleState == BattleState.PartyTargetSelect
            ? "[1]/[2] or click an enemy to resolve the queued action."
            : targetSelection != null && !string.IsNullOrEmpty(targetSelection.TargetLabel) && targetSelection.TargetLabel != "Choose a target"
                ? targetSelection.TargetLabel
                : string.Empty;
        hints.CancelHint = GetBattleCancelHintText();
        return hints;
    }

    private PrototypeBattleConfirmContextData BuildBattleInteractionConfirmContextData(PrototypeBattleUiActionContextData actionContext)
    {
        PrototypeBattleConfirmContextData context = new PrototypeBattleConfirmContextData();
        context.ConfirmRequired = false;
        context.PendingActionKey = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionKey)
            ? actionContext.SelectedActionKey
            : string.Empty;
        context.PendingActionLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionLabel)
            ? actionContext.SelectedActionLabel
            : "Action";
        context.ConfirmLabel = "Confirm";
        context.CancelLabel = _battleState == BattleState.PartyTargetSelect ? "Cancel Target Select" : "Cancel";
        return context;
    }

    private PrototypeRpgBattleRuntimeState BuildBattleRuntimeState()
    {
        PrototypeRpgBattleRuntimeState runtime = new PrototypeRpgBattleRuntimeState();
        runtime.IsBattleActive = _dungeonRunState == DungeonRunState.Battle;
        runtime.IsTargetSelectionActive = _battleState == BattleState.PartyTargetSelect;
        runtime.BattleStateKey = GetBattleStateKey();
        runtime.PhaseKey = GetBattleInteractionMenuModeKey();
        runtime.TurnIndex = Mathf.Max(_runTurnCount, _battleTurnIndex);
        runtime.CurrentDungeonName = string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
        runtime.CurrentRouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel;
        runtime.EncounterRuntime = BuildActiveEncounterRuntimeState();
        runtime.CurrentEncounterId = runtime.EncounterRuntime != null ? runtime.EncounterRuntime.EncounterId : string.Empty;

        DungeonPartyMemberRuntimeData currentMember = GetCurrentActorMember();
        if (_dungeonRunState == DungeonRunState.Battle && _battleState == BattleState.EnemyTurn && _activeBattleMonster != null)
        {
            runtime.CurrentActorId = _activeBattleMonster.MonsterId;
            runtime.CurrentActorIndex = _enemyTurnMonsterCursor;
            runtime.CurrentActorLabel = _activeBattleMonster.DisplayName;
            runtime.CurrentActorIsEnemy = true;
            runtime.ActingEnemyId = _activeBattleMonster.MonsterId;
        }
        else if (currentMember != null)
        {
            runtime.CurrentActorId = currentMember.MemberId;
            runtime.CurrentActorIndex = _currentActorIndex;
            runtime.CurrentActorLabel = currentMember.DisplayName;
            runtime.CurrentActorIsEnemy = false;
        }

        runtime.QueuedActionKey = GetBattleUiSelectedActionKey();
        runtime.QueuedActionLabel = GetBattleRuntimeQueuedActionLabel(runtime.QueuedActionKey, currentMember);
        runtime.SelectedTargetId = string.IsNullOrEmpty(_activeBattleMonsterId) ? string.Empty : _activeBattleMonsterId;
        runtime.HoveredTargetId = string.IsNullOrEmpty(_hoverBattleMonsterId) ? string.Empty : _hoverBattleMonsterId;
        DungeonPartyMemberRuntimeData pendingEnemyTarget = GetPartyMemberAtIndex(_pendingEnemyTargetIndex);
        runtime.PendingEnemyTargetId = pendingEnemyTarget != null ? pendingEnemyTarget.MemberId : string.Empty;
        runtime.FeedbackText = string.IsNullOrEmpty(_battleFeedbackText) ? string.Empty : _battleFeedbackText;
        runtime.SelectionPromptText = string.IsNullOrEmpty(_currentSelectionPrompt) ? string.Empty : _currentSelectionPrompt;
        runtime.CancelHintText = GetBattleCancelHintText();
        runtime.PartyConditionText = GetPartyConditionText();
        runtime.TotalPartyHpText = BuildTotalPartyHpSummary();
        runtime.EliteStatusText = GetEliteStatusText();
        runtime.EliteEncounterName = GetEliteEncounterNameText();
        runtime.EliteTypeText = string.IsNullOrEmpty(_eliteType) ? "None" : _eliteType;
        runtime.EliteRewardHintText = GetEliteRewardHintText();
        runtime.EnemyIntentText = string.IsNullOrEmpty(_enemyIntentText) ? "None" : _enemyIntentText;
                runtime.RecentLogs = BuildRecentBattleLogArray();
        runtime.EnemyIntent = BuildCurrentEnemyIntentSnapshotView();
        runtime.RecentEvents = BuildRecentBattleEventRecords();
        runtime.ResultSnapshot = BuildCurrentBattleResultSnapshotView();
        runtime.PartyContribution = BuildCurrentBattleContributionSnapshotView();
        runtime.CurrentActionResolution = BuildCurrentPartyActionResolutionRecord(ParseBattleActionType(runtime.QueuedActionKey), currentMember);
        runtime.PendingEnemyResolution = _battleState == BattleState.EnemyTurn && _activeBattleMonster != null
            ? BuildPendingEnemyActionResolutionRecord(_activeBattleMonster, _pendingEnemyTargetIndex, _pendingEnemyUsedSpecialAttack)
            : new PrototypeCombatResolutionRecord();
        return runtime;
    }

    private PrototypeRpgEncounterRuntimeState BuildActiveEncounterRuntimeState()
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null)
        {
            return new PrototypeRpgEncounterRuntimeState();
        }

        PrototypeRpgEncounterRuntimeState runtime = encounter.RuntimeState ?? new PrototypeRpgEncounterRuntimeState(
            encounter.Definition != null ? encounter.Definition.DefinitionId : string.Empty,
            encounter.EncounterId,
            encounter.RoomIndex,
            encounter.DisplayName,
            encounter.Definition != null ? encounter.Definition.EncounterTypeLabel : string.Empty,
            encounter.Definition != null ? encounter.Definition.RoomTypeLabel : string.Empty,
            encounter.Definition != null ? encounter.Definition.EliteStyleLabel : string.Empty,
            encounter.Definition != null ? encounter.Definition.RouteRiskLabel : string.Empty,
            encounter.Definition != null ? encounter.Definition.DangerHintLabel : string.Empty,
            encounter.Definition != null ? encounter.Definition.RewardPreviewHint : string.Empty,
            encounter.Definition != null ? encounter.Definition.RewardLabel : string.Empty,
            encounter.Definition != null ? encounter.Definition.RewardAmountHint : 0,
            encounter.MonsterIds,
            encounter.IsEliteEncounter);

        int livingEnemyCount = 0;
        int clearedEnemyCount = 0;
        string[] enemyIds = encounter.MonsterIds ?? System.Array.Empty<string>();
        for (int i = 0; i < enemyIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(enemyIds[i]);
            if (monster == null)
            {
                continue;
            }

            if (!monster.IsDefeated && monster.CurrentHp > 0)
            {
                livingEnemyCount++;
            }
            else
            {
                clearedEnemyCount++;
            }
        }

        DungeonMonsterRuntimeData focusedMonster = GetBattleUiFocusedMonster();
        string selectedEnemyId = string.IsNullOrEmpty(_activeBattleMonsterId) ? string.Empty : _activeBattleMonsterId;
        string focusedEnemyId = focusedMonster != null ? focusedMonster.MonsterId : string.Empty;
        string actingEnemyId = _battleState == BattleState.EnemyTurn && _activeBattleMonster != null ? _activeBattleMonster.MonsterId : string.Empty;
        string intentSummaryText = !string.IsNullOrEmpty(_enemyIntentText) ? _enemyIntentText : focusedMonster != null ? BuildBattleUiEnemyIntentLabel(focusedMonster) : string.Empty;
        runtime.SetCleared(encounter.IsCleared || (enemyIds.Length > 0 && livingEnemyCount <= 0));
        runtime.UpdateCombatViewState(selectedEnemyId, focusedEnemyId, actingEnemyId, intentSummaryText, livingEnemyCount, clearedEnemyCount);
        return runtime;
    }

    private string[] BuildRecentBattleLogArray(int maxCount = RecentBattleLogLimit)
    {
        List<string> logs = new List<string>();
        int count = Mathf.Clamp(maxCount, 1, RecentBattleLogLimit);
        for (int i = 0; i < count; i++)
        {
            string logLine = GetRecentBattleLogText(i);
            if (!string.IsNullOrEmpty(logLine) && logLine != "None")
            {
                logs.Add(logLine);
            }
        }

        return logs.Count > 0 ? logs.ToArray() : System.Array.Empty<string>();
    }

    private string GetBattleRuntimeQueuedActionLabel(string actionKey, DungeonPartyMemberRuntimeData currentMember)
    {
        if (actionKey == "attack")
        {
            return "Attack";
        }

        if (actionKey == "skill")
        {
            return currentMember != null && !string.IsNullOrEmpty(currentMember.SkillName) ? currentMember.SkillName : "Skill";
        }

        if (actionKey == "retreat")
        {
            return "Retreat";
        }

        return "Action";
    }

    private PrototypeBattleTargetCandidateData[] BuildBattleInteractionTargetCandidates(PrototypeRpgBattleRuntimeState runtime)
    {
        PrototypeBattleTargetCandidateData[] candidates = BuildBattleInteractionTargetCandidates();
        if (candidates == null || candidates.Length <= 0)
        {
            return System.Array.Empty<PrototypeBattleTargetCandidateData>();
        }

        bool isTargetSelectionActive = runtime != null && runtime.IsTargetSelectionActive;
        string hoveredTargetId = runtime != null ? runtime.HoveredTargetId : string.Empty;
        string selectedTargetId = runtime != null ? runtime.SelectedTargetId : string.Empty;
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeBattleTargetCandidateData candidate = candidates[i] ?? new PrototypeBattleTargetCandidateData();
            candidate.IsAvailable = isTargetSelectionActive && !candidate.IsDefeated && candidate.CurrentHp > 0;
            candidate.IsHovered = !string.IsNullOrEmpty(hoveredTargetId) && candidate.TargetId == hoveredTargetId;
            candidate.IsSelected = !string.IsNullOrEmpty(selectedTargetId) && candidate.TargetId == selectedTargetId;
            candidates[i] = candidate;
        }

        return candidates;
    }

    private PrototypeBattleUiEnemyData BuildSelectedBattleUiEnemyData(PrototypeRpgBattleRuntimeState runtime)
    {
        string preferredTargetId = runtime != null && !string.IsNullOrEmpty(runtime.HoveredTargetId)
            ? runtime.HoveredTargetId
            : runtime != null && !string.IsNullOrEmpty(runtime.SelectedTargetId)
                ? runtime.SelectedTargetId
                : runtime != null && runtime.EncounterRuntime != null && !string.IsNullOrEmpty(runtime.EncounterRuntime.FocusedEnemyId)
                    ? runtime.EncounterRuntime.FocusedEnemyId
                    : string.Empty;
        if (!string.IsNullOrEmpty(preferredTargetId))
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(preferredTargetId);
            if (monster != null)
            {
                return BuildBattleUiEnemyData(monster);
            }
        }

        return BuildBattleUiEnemyData(GetBattleUiFocusedMonster());
    }

    private PrototypeBattleUiEnemyData[] BuildBattleUiEnemyRoster(PrototypeRpgBattleRuntimeState runtime)
    {
        return BuildBattleUiEnemyRoster();
    }

    private PrototypeBattleUiMessageSurfaceData BuildBattleUiMessageSurfaceData(PrototypeRpgBattleRuntimeState runtime)
    {
        if (runtime == null)
        {
            return BuildBattleUiMessageSurfaceData();
        }

        PrototypeBattleUiMessageSurfaceData message = new PrototypeBattleUiMessageSurfaceData();
        message.Prompt = !string.IsNullOrEmpty(runtime.SelectionPromptText) ? runtime.SelectionPromptText : !string.IsNullOrEmpty(runtime.FeedbackText) ? runtime.FeedbackText : "Select an action.";
        message.CancelHint = !string.IsNullOrEmpty(runtime.CancelHintText) ? runtime.CancelHintText : GetBattleCancelHintText();
        message.Feedback = string.IsNullOrEmpty(runtime.FeedbackText) ? string.Empty : runtime.FeedbackText;
        message.RecentLogs = runtime.RecentLogs != null && runtime.RecentLogs.Length > 0 ? (string[])runtime.RecentLogs.Clone() : System.Array.Empty<string>();
        return message;
    }

    private PrototypeBattleUiTargetSelectionData BuildBattleUiTargetSelectionData(PrototypeRpgBattleRuntimeState runtime, PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext, PrototypeBattleUiTargetContextData targetContext)
    {
        PrototypeBattleUiTargetSelectionData targetSelection = BuildBattleUiTargetSelectionData(actor, actionContext, targetContext);
        if (runtime != null)
        {
            targetSelection.IsActive = runtime.IsTargetSelectionActive;
            if (!string.IsNullOrEmpty(runtime.QueuedActionLabel))
            {
                targetSelection.QueuedActionLabel = runtime.QueuedActionLabel;
            }

            if (!string.IsNullOrEmpty(runtime.CancelHintText))
            {
                targetSelection.CancelHint = runtime.CancelHintText;
            }
        }

        return targetSelection;
    }

        private PrototypeBattleUiActionContextData BuildBattleUiActionContextData(PrototypeRpgBattleRuntimeState runtime, PrototypeBattleUiActorData actor, PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiActionContextData actionContext = BuildBattleUiActionContextData(actor, selectedEnemy);
        if (runtime != null)
        {
            if (!string.IsNullOrEmpty(runtime.CurrentActorId))
            {
                actionContext.ActorId = runtime.CurrentActorId;
            }

            actionContext.ActorIndex = runtime.CurrentActorIndex;
            if (!string.IsNullOrEmpty(runtime.QueuedActionKey))
            {
                actionContext.SelectedActionKey = runtime.QueuedActionKey;
            }

            if (!string.IsNullOrEmpty(runtime.QueuedActionLabel))
            {
                actionContext.SelectedActionLabel = runtime.QueuedActionLabel;
            }

            if (!string.IsNullOrEmpty(runtime.SelectedTargetId))
            {
                actionContext.SelectedTargetId = runtime.SelectedTargetId;
            }

            PrototypeCombatResolutionRecord resolution = runtime.CurrentActionResolution;
            if (resolution != null)
            {
                actionContext.IsSkillAction = resolution.IsSkillAction;
                actionContext.ResolvedSkillId = resolution.SkillId;
                actionContext.ResolvedSkillLabel = resolution.IsSkillAction ? resolution.ActionLabel : string.Empty;
                actionContext.ResolvedTargetKind = resolution.TargetKind;
                actionContext.ResolvedTargetPolicyKey = resolution.TargetPolicyKey;
                actionContext.ResolvedEffectType = resolution.EffectTypeKey;
                actionContext.ResolvedPowerValue = resolution.PowerValue;
                actionContext.RequiresTarget = resolution.RequiresTarget;
                if (!string.IsNullOrEmpty(resolution.ActionKey))
                {
                    actionContext.SelectedActionKey = resolution.ActionKey;
                }

                if (!string.IsNullOrEmpty(resolution.ActionLabel))
                {
                    actionContext.SelectedActionLabel = resolution.ActionLabel;
                }
            }
        }

        return actionContext;
    }

    private PrototypeBattleUiTargetContextData BuildBattleUiTargetContextData(PrototypeRpgBattleRuntimeState runtime, PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiTargetContextData targetContext = BuildBattleUiTargetContextData(selectedEnemy);
        if (runtime != null && targetContext != null)
        {
            string lockedTargetId = !string.IsNullOrEmpty(runtime.SelectedTargetId) ? runtime.SelectedTargetId : string.Empty;
            string hoveredTargetId = !string.IsNullOrEmpty(runtime.HoveredTargetId) ? runtime.HoveredTargetId : string.Empty;
            targetContext.IsLocked = !string.IsNullOrEmpty(lockedTargetId) && lockedTargetId == targetContext.TargetMonsterId;
            targetContext.IsHovered = !string.IsNullOrEmpty(hoveredTargetId) && hoveredTargetId == targetContext.TargetMonsterId;
        }

        return targetContext;
    }

    private PrototypeBattleUiActorData BuildBattleUiCurrentActorData()
    {
        PrototypeBattleUiActorData actor = new PrototypeBattleUiActorData();
        if (_dungeonRunState == DungeonRunState.Battle && _battleState == BattleState.EnemyTurn && _activeBattleMonster != null)
        {
            actor.ActorId = _activeBattleMonster.MonsterId;
            actor.DisplayName = _activeBattleMonster.DisplayName;
            actor.RoleLabel = GetMonsterRoleText(_activeBattleMonster);
            actor.SkillLabel = string.IsNullOrEmpty(_activeBattleMonster.SpecialActionName) ? "Attack" : _activeBattleMonster.SpecialActionName;
            actor.SkillShortText = BuildBattleUiEnemyIntentLabel(_activeBattleMonster);
            actor.CurrentHp = Mathf.Max(0, _activeBattleMonster.CurrentHp);
            actor.MaxHp = Mathf.Max(1, _activeBattleMonster.MaxHp);
            actor.IsEnemy = true;
            actor.StatusText = "Enemy turn";
            return actor;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            actor.StatusText = "Awaiting turn";
            return actor;
        }

        actor.ActorId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
        actor.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "None" : member.DisplayName;
        actor.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? "Adventurer" : member.RoleLabel;
        actor.SkillLabel = string.IsNullOrEmpty(member.SkillName) ? "Skill" : member.SkillName;
        actor.SkillShortText = string.IsNullOrEmpty(member.SkillShortText) ? string.Empty : member.SkillShortText;
        actor.CurrentHp = Mathf.Max(0, member.CurrentHp);
        actor.MaxHp = Mathf.Max(1, member.MaxHp);
        actor.IsEnemy = false;
        actor.StatusText = _battleState == BattleState.PartyTargetSelect ? "Selecting target" : _battleState == BattleState.PartyActionSelect ? "Awaiting command" : "Ready";
        return actor;
    }

    private PrototypeBattleUiTimelineData BuildBattleUiTimelineData()
    {
        PrototypeBattleUiTimelineData timeline = new PrototypeBattleUiTimelineData();
        timeline.PhaseLabel = GetBattlePhaseText();
        timeline.NextStepLabel = GetBattleUiNextStepText();

        List<PrototypeBattleUiTimelineSlotData> slots = new List<PrototypeBattleUiTimelineSlotData>();
        if (_battleState == BattleState.EnemyTurn && _activeBattleMonster != null)
        {
            slots.Add(CreateBattleUiTimelineSlot(_activeBattleMonster.DisplayName, GetMonsterRoleText(_activeBattleMonster), true, true, false, "Current"));
            DungeonPartyMemberRuntimeData targetMember = GetPartyMemberAtIndex(_pendingEnemyTargetIndex);
            if (targetMember != null && !targetMember.IsDefeated && targetMember.CurrentHp > 0)
            {
                slots.Add(CreateBattleUiTimelineSlot(targetMember.DisplayName, targetMember.RoleLabel, false, false, true, "Targeted"));
            }

            AppendBattleUiNextPartySlots(slots, _pendingEnemyTargetIndex, 3);
        }
        else
        {
            DungeonPartyMemberRuntimeData currentMember = GetCurrentActorMember();
            if (currentMember != null)
            {
                slots.Add(CreateBattleUiTimelineSlot(currentMember.DisplayName, currentMember.RoleLabel, true, false, false, "Current"));
            }

            AppendBattleUiNextPartySlots(slots, _currentActorIndex, 3);
            DungeonMonsterRuntimeData previewMonster = GetBattleUiPrimaryPreviewMonster();
            if (previewMonster != null)
            {
                slots.Add(CreateBattleUiTimelineSlot(previewMonster.DisplayName, GetMonsterRoleText(previewMonster), false, true, _battleState == BattleState.PartyTargetSelect, _battleState == BattleState.PartyTargetSelect ? "Preview" : "Queued"));
            }
        }

        if (slots.Count == 0)
        {
            slots.Add(CreateBattleUiTimelineSlot("Awaiting encounter", "Battle", true, false, false, "Current"));
        }

        timeline.Slots = slots.ToArray();
        return timeline;
    }

    private PrototypeBattleUiTimelineSlotData CreateBattleUiTimelineSlot(string label, string secondaryLabel, bool isCurrent, bool isEnemy, bool isPending, string statusLabel)
    {
        PrototypeBattleUiTimelineSlotData slot = new PrototypeBattleUiTimelineSlotData();
        slot.Label = string.IsNullOrEmpty(label) ? "None" : label;
        slot.SecondaryLabel = string.IsNullOrEmpty(secondaryLabel) ? string.Empty : secondaryLabel;
        slot.StatusLabel = string.IsNullOrEmpty(statusLabel)
            ? isCurrent
                ? "Current"
                : isPending
                    ? "Pending"
                    : "Queued"
            : statusLabel;
        slot.IsCurrent = isCurrent;
        slot.IsEnemy = isEnemy;
        slot.IsPending = isPending;
        return slot;
    }

    private void AppendBattleUiNextPartySlots(List<PrototypeBattleUiTimelineSlotData> slots, int anchorIndex, int maxCount)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || maxCount <= 0)
        {
            return;
        }

        int startIndex = anchorIndex >= 0 ? anchorIndex + 1 : 0;
        int addedCount = 0;
        for (int offset = 0; offset < _activeDungeonParty.Members.Length && addedCount < maxCount; offset++)
        {
            int index = (startIndex + offset) % _activeDungeonParty.Members.Length;
            if (index == anchorIndex)
            {
                continue;
            }

            DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(index);
            if (member == null || member.IsDefeated || member.CurrentHp <= 0)
            {
                continue;
            }

            slots.Add(CreateBattleUiTimelineSlot(member.DisplayName, member.RoleLabel, false, false, false, "Queued"));
            addedCount += 1;
        }
    }

    private DungeonPartyMemberRuntimeData GetPartyMemberAtIndex(int memberIndex)
    {
        return _activeDungeonParty != null && _activeDungeonParty.Members != null && memberIndex >= 0 && memberIndex < _activeDungeonParty.Members.Length
            ? _activeDungeonParty.Members[memberIndex]
            : null;
    }

    private PrototypeBattleUiPartyMemberData[] BuildBattleUiPartyMembers()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || _activeDungeonParty.Members.Length == 0)
        {
            return System.Array.Empty<PrototypeBattleUiPartyMemberData>();
        }

        PrototypeBattleUiPartyMemberData[] members = new PrototypeBattleUiPartyMemberData[_activeDungeonParty.Members.Length];
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            members[i] = BuildBattleUiPartyMemberData(_activeDungeonParty.Members[i], i);
        }

        return members;
    }

    private PrototypeBattleUiPartyMemberData BuildBattleUiPartyMemberData(DungeonPartyMemberRuntimeData member, int memberIndex)
    {
        PrototypeBattleUiPartyMemberData data = new PrototypeBattleUiPartyMemberData();
        data.SlotIndex = memberIndex;
        if (member == null)
        {
            data.DisplayName = "Empty";
            data.StatusText = "Unavailable";
            return data;
        }

        bool isTargeted = _dungeonRunState == DungeonRunState.Battle &&
                          _battleState == BattleState.EnemyTurn &&
                          _pendingEnemyTargetIndex == memberIndex &&
                          !member.IsDefeated &&
                          member.CurrentHp > 0;
        bool isActive = _dungeonRunState == DungeonRunState.Battle &&
                        _currentActorIndex == memberIndex &&
                        !member.IsDefeated &&
                        member.CurrentHp > 0;

        data.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
        data.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Member " + (memberIndex + 1) : member.DisplayName;
        data.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? "Adventurer" : member.RoleLabel;
        data.SkillLabel = string.IsNullOrEmpty(member.SkillName) ? "Basic Action" : member.SkillName;
        data.SkillShortText = string.IsNullOrEmpty(member.SkillShortText) ? string.Empty : member.SkillShortText;
        data.CurrentHp = Mathf.Max(0, member.CurrentHp);
        data.MaxHp = Mathf.Max(1, member.MaxHp);
        data.Attack = member.Attack;
        data.Defense = member.Defense;
        data.Speed = member.Speed;
        data.IsActive = isActive;
        data.IsTargeted = isTargeted;
        data.IsKnockedOut = member.IsDefeated || member.CurrentHp <= 0;
        data.StatusText = data.IsKnockedOut
            ? "KO"
            : data.IsActive
                ? "Acting"
                : data.IsTargeted
                    ? "Targeted"
                    : "Ready";
        return data;
    }

    private PrototypeBattleUiEnemyData BuildSelectedBattleUiEnemyData()
    {
        return BuildBattleUiEnemyData(GetBattleUiFocusedMonster());
    }

    private PrototypeBattleUiEnemyData[] BuildBattleUiEnemyRoster()
    {
        List<PrototypeBattleUiEnemyData> roster = new List<PrototypeBattleUiEnemyData>();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
                if (monster != null)
                {
                    roster.Add(BuildBattleUiEnemyData(monster));
                }
            }
        }

        if (roster.Count == 0)
        {
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                DungeonMonsterRuntimeData monster = _activeMonsters[i];
                if (monster != null)
                {
                    roster.Add(BuildBattleUiEnemyData(monster));
                }
            }
        }

        return roster.Count > 0 ? roster.ToArray() : System.Array.Empty<PrototypeBattleUiEnemyData>();
    }

    private PrototypeBattleUiEnemyData BuildBattleUiEnemyData(DungeonMonsterRuntimeData monster)
    {
        PrototypeBattleUiEnemyData data = new PrototypeBattleUiEnemyData();
        if (monster == null)
        {
            data.DisplayName = "No target selected";
            data.StateLabel = "None";
            data.IntentLabel = "Unknown";
            data.TraitText = "Traits pending";
            return data;
        }

        bool isSelected = !monster.IsDefeated && _battleState == BattleState.PartyTargetSelect && _activeBattleMonsterId == monster.MonsterId;
        bool isHovered = !monster.IsDefeated && _battleState == BattleState.PartyTargetSelect && _hoverBattleMonsterId == monster.MonsterId;
        bool isActing = !monster.IsDefeated && _battleState == BattleState.EnemyTurn && _activeBattleMonsterId == monster.MonsterId;
        data.MonsterId = string.IsNullOrEmpty(monster.MonsterId) ? string.Empty : monster.MonsterId;
        data.DisplayName = string.IsNullOrEmpty(monster.DisplayName) ? "Monster" : monster.DisplayName;
        data.TypeLabel = string.IsNullOrEmpty(monster.MonsterType) ? "Monster" : monster.MonsterType;
        data.RoleLabel = GetMonsterRoleText(monster);
        data.IntentLabel = BuildBattleUiEnemyIntentLabel(monster);
        data.TraitText = BuildBattleUiEnemyTraitText(monster);
        data.CurrentHp = Mathf.Max(0, monster.CurrentHp);
        data.MaxHp = Mathf.Max(1, monster.MaxHp);
        data.Attack = monster.Attack;
        data.IsElite = monster.IsElite;
        data.IsSelected = isSelected;
        data.IsHovered = isHovered;
        data.IsActing = isActing;
        data.IsDefeated = monster.IsDefeated;
        data.StateLabel = monster.IsDefeated ? "Defeated" : isActing ? "Acting" : isHovered ? "Hover" : isSelected ? "Targeted" : "Alive";
        return data;
    }

    private string BuildBattleUiEnemyIntentLabel(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return "Unknown";
        }

        if (!string.IsNullOrEmpty(monster.IntentLabel))
        {
            return monster.IntentLabel;
        }

        if (!string.IsNullOrEmpty(_enemyIntentText) && _activeBattleMonsterId == monster.MonsterId)
        {
            return _enemyIntentText;
        }

        PrototypeRpgEnemyIntentDefinition intentDefinition = ResolveEnemyIntentDefinition(monster, monster.IsElite && !string.IsNullOrEmpty(monster.SpecialActionName));
        if (intentDefinition != null && !string.IsNullOrEmpty(intentDefinition.ShortLabel))
        {
            return intentDefinition.ShortLabel;
        }

        return !string.IsNullOrEmpty(monster.BehaviorHintText) ? monster.BehaviorHintText : "Unknown";
    }

    private string BuildBattleUiEnemyTraitText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return "Traits pending";
        }

        string traitText = !string.IsNullOrEmpty(monster.TraitText) ? monster.TraitText : !string.IsNullOrEmpty(monster.BehaviorHintText) ? monster.BehaviorHintText : "Traits pending";
        if (monster.IsElite)
        {
            DungeonEncounterRuntimeData encounter = GetEncounterById(monster.EncounterId);
            if (encounter != null && encounter.Definition != null && !string.IsNullOrEmpty(encounter.Definition.EliteStyleLabel))
            {
                traitText = encounter.Definition.EliteStyleLabel;
            }

            if (!string.IsNullOrEmpty(monster.SpecialActionName) && (string.IsNullOrEmpty(traitText) || !traitText.Contains(monster.SpecialActionName)))
            {
                traitText = string.IsNullOrEmpty(traitText) ? monster.SpecialActionName : traitText + " | " + monster.SpecialActionName;
            }
        }

        return string.IsNullOrEmpty(traitText) ? "Traits pending" : traitText;
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
        PrototypeBattleUiCommandSurfaceData commandSurface = new PrototypeBattleUiCommandSurfaceData();
        string selectedActionKey = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionKey)
            ? actionContext.SelectedActionKey
            : GetBattleUiSelectedActionKey();
        commandSurface.SelectedActionKey = selectedActionKey;
        commandSurface.SelectedActionLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionLabel)
            ? actionContext.SelectedActionLabel
            : GetBattleUiActionDisplayLabel(selectedActionKey, actor);

        DungeonPartyMemberRuntimeData currentMember = actor != null && !actor.IsEnemy ? GetCurrentActorMember() : null;
        int basicAttackPower = currentMember != null ? Mathf.Max(1, currentMember.Attack) : 1;
        string skillLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedSkillLabel)
            ? actionContext.ResolvedSkillLabel
            : actor != null && !string.IsNullOrEmpty(actor.SkillLabel) && actor.SkillLabel != "None"
                ? actor.SkillLabel
                : "Skill";
        string skillDescription = actor != null && !string.IsNullOrEmpty(actor.SkillShortText)
            ? actor.SkillShortText
            : "Uses the active actor's shared skill definition.";
        string skillTargetText = actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedTargetKind)
            ? GetBattleUiTargetTypeLabel(actionContext.ResolvedTargetKind)
            : "Single enemy";
        string skillEffectText = actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedEffectType)
            ? GetBattleUiEffectText(actionContext.ResolvedEffectType, actionContext.ResolvedPowerValue)
            : skillDescription;

        List<PrototypeBattleUiCommandDetailData> details = new List<PrototypeBattleUiCommandDetailData>();
        details.Add(BuildBattleUiCommandDetailData("attack", "Attack", "Reliable basic strike.", "Single enemy", "None", "Deal " + basicAttackPower + " damage.", IsBattleActionAvailable(BattleActionType.Attack), selectedActionKey == "attack"));
        details.Add(BuildBattleUiCommandDetailData("skill", skillLabel, skillDescription, skillTargetText, "No cost", skillEffectText, IsBattleActionAvailable(BattleActionType.Skill), selectedActionKey == "skill"));
        details.Add(BuildBattleUiCommandDetailData("item", "Item", "Reserved for the later inventory batch.", "Self / ally", "Not available yet", "No consumables are wired in this batch.", false, false));
        details.Add(BuildBattleUiCommandDetailData("retreat", "Retreat", "Leave the run using the current resolution flow.", "Party", "Ends the current run", "Return to WorldSim with the current writeback path.", IsBattleActionAvailable(BattleActionType.Retreat), selectedActionKey == "retreat"));
        details.Add(BuildBattleUiCommandDetailData("retreat_confirm", "Confirm retreat", "Commit to the retreat action.", "Party", "Confirm exit", "Uses the existing retreat resolution.", IsBattleActionAvailable(BattleActionType.Retreat), false));
        details.Add(BuildBattleUiCommandDetailData("back", "Back", "Return to the previous command layer.", "Current menu", "None", "Keeps the battle flow intact.", true, false));
        details.Add(BuildBattleUiCommandDetailData("cancel", "Cancel", "Leave confirmation without committing.", "Current dialog", "None", "Returns to party commands.", true, false));
        commandSurface.Details = details.ToArray();
        return commandSurface;
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
        PrototypeBattleUiMessageSurfaceData message = new PrototypeBattleUiMessageSurfaceData();
        message.Prompt = string.IsNullOrEmpty(_currentSelectionPrompt)
            ? string.IsNullOrEmpty(_battleFeedbackText) ? "Select an action." : _battleFeedbackText
            : _currentSelectionPrompt;
        message.CancelHint = GetBattleCancelHintText();
        message.Feedback = string.IsNullOrEmpty(_battleFeedbackText) ? string.Empty : _battleFeedbackText;

        List<string> logs = new List<string>();
        for (int i = 0; i < RecentBattleLogLimit; i++)
        {
            string logLine = GetRecentBattleLogText(i);
            if (!string.IsNullOrEmpty(logLine) && logLine != "None")
            {
                logs.Add(logLine);
            }
        }

        message.RecentLogs = logs.Count > 0 ? logs.ToArray() : System.Array.Empty<string>();
        return message;
    }

    private PrototypeBattleUiTargetSelectionData BuildBattleUiTargetSelectionData(PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext, PrototypeBattleUiTargetContextData targetContext)
    {
        PrototypeBattleUiTargetSelectionData targetSelection = new PrototypeBattleUiTargetSelectionData();
        targetSelection.IsActive = _battleState == BattleState.PartyTargetSelect;
        targetSelection.Title = actionContext != null && actionContext.IsSkillAction ? "Skill Target" : "Target Selection";
        targetSelection.QueuedActionLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionLabel)
            ? actionContext.SelectedActionLabel
            : GetBattleUiActionDisplayLabel(GetBattleUiSelectedActionKey(), actor);
        targetSelection.HasFocusedTarget = targetContext != null && targetContext.HasTarget;
        targetSelection.TargetLabel = targetContext != null && targetContext.HasTarget ? targetContext.TargetLabel : "Choose a target";
        targetSelection.TargetRoleLabel = targetContext != null && targetContext.HasTarget ? targetContext.TargetRoleLabel : string.Empty;
        targetSelection.TargetIntentLabel = targetContext != null && targetContext.HasTarget ? targetContext.TargetIntentLabel : string.Empty;
        targetSelection.TargetTraitText = targetContext != null && targetContext.HasTarget
            ? BuildBattleUiEnemyTraitText(GetMonsterById(targetContext.TargetMonsterId))
            : string.Empty;
        targetSelection.TargetStateText = targetContext != null && targetContext.HasTarget
            ? targetContext.TargetStateText
            : "Hover an enemy or click a target.";
        targetSelection.TargetCurrentHp = targetContext != null && targetContext.HasTarget ? targetContext.TargetCurrentHp : 0;
        targetSelection.TargetMaxHp = targetContext != null && targetContext.HasTarget ? Mathf.Max(1, targetContext.TargetMaxHp) : 1;
        targetSelection.SkillHintText = actionContext != null && actionContext.IsSkillAction && actor != null && !string.IsNullOrEmpty(actor.SkillShortText)
            ? actor.SkillShortText
            : string.Empty;
        targetSelection.CancelHint = GetBattleCancelHintText();
        return targetSelection;
    }

    private string GetBattleUiSelectedActionKey()
    {
        return _queuedBattleAction == BattleActionType.Attack
            ? "attack"
            : _queuedBattleAction == BattleActionType.Skill
                ? "skill"
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
        PrototypeBattleUiActionContextData actionContext = new PrototypeBattleUiActionContextData();
        if (actor == null)
        {
            return actionContext;
        }

        actionContext.ActorId = string.IsNullOrEmpty(actor.ActorId) ? string.Empty : actor.ActorId;
        actionContext.ActorIndex = actor.IsEnemy ? _enemyTurnMonsterCursor : _currentActorIndex;
        string selectedActionKey = GetBattleUiSelectedActionKey();
        actionContext.SelectedActionKey = selectedActionKey;
        actionContext.SelectedActionLabel = GetBattleUiActionDisplayLabel(selectedActionKey, actor);
        actionContext.SelectedTargetId = selectedEnemy != null && !string.IsNullOrEmpty(selectedEnemy.MonsterId)
            ? selectedEnemy.MonsterId
            : string.Empty;

        if (actor.IsEnemy)
        {
            actionContext.RequiresTarget = !string.IsNullOrEmpty(actionContext.SelectedTargetId);
            return actionContext;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return actionContext;
        }

        PrototypeCombatResolutionRecord resolution = BuildCurrentPartyActionResolutionRecord(ParseBattleActionType(selectedActionKey), member);
        if (resolution == null)
        {
            return actionContext;
        }

        if (!string.IsNullOrEmpty(resolution.ActionKey))
        {
            actionContext.SelectedActionKey = resolution.ActionKey;
        }

        if (!string.IsNullOrEmpty(resolution.ActionLabel))
        {
            actionContext.SelectedActionLabel = resolution.ActionLabel;
        }

        actionContext.IsSkillAction = resolution.IsSkillAction;
        actionContext.ResolvedSkillId = resolution.SkillId;
        actionContext.ResolvedSkillLabel = resolution.IsSkillAction ? resolution.ActionLabel : string.Empty;
        actionContext.ResolvedTargetKind = resolution.TargetKind;
        actionContext.ResolvedTargetPolicyKey = resolution.TargetPolicyKey;
        actionContext.ResolvedEffectType = resolution.EffectTypeKey;
        actionContext.ResolvedPowerValue = resolution.PowerValue;
        actionContext.RequiresTarget = resolution.RequiresTarget;
        return actionContext;
    }

    private PrototypeBattleUiTargetContextData BuildBattleUiTargetContextData(PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiTargetContextData targetContext = new PrototypeBattleUiTargetContextData();
        if (selectedEnemy == null || string.IsNullOrEmpty(selectedEnemy.MonsterId))
        {
            return targetContext;
        }

        targetContext.HasTarget = true;
        targetContext.TargetMonsterId = selectedEnemy.MonsterId;
        targetContext.TargetDisplayIndex = GetBattleMonsterDisplayIndex(selectedEnemy.MonsterId);
        targetContext.TargetLabel = string.IsNullOrEmpty(selectedEnemy.DisplayName) ? "Monster" : selectedEnemy.DisplayName;
        targetContext.TargetRoleLabel = string.IsNullOrEmpty(selectedEnemy.RoleLabel) ? "Monster" : selectedEnemy.RoleLabel;
        targetContext.TargetIntentLabel = string.IsNullOrEmpty(selectedEnemy.IntentLabel) ? "Unknown" : selectedEnemy.IntentLabel;
        targetContext.TargetStateText = string.IsNullOrEmpty(selectedEnemy.StateLabel) ? "Alive" : selectedEnemy.StateLabel;
        targetContext.TargetCurrentHp = Mathf.Max(0, selectedEnemy.CurrentHp);
        targetContext.TargetMaxHp = Mathf.Max(1, selectedEnemy.MaxHp);
        targetContext.IsHovered = selectedEnemy.IsHovered;
        targetContext.IsLocked = !string.IsNullOrEmpty(_activeBattleMonsterId) && _activeBattleMonsterId == selectedEnemy.MonsterId;
        targetContext.IsDefeated = selectedEnemy.IsDefeated;
        return targetContext;
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
                return "Deal " + safePower + " damage. Gains +2 against weakened targets.";
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
        PrototypeBattleEventRecord copy = new PrototypeBattleEventRecord();
        if (source == null)
        {
            return copy;
        }

        copy.EventId = source.EventId;
        copy.Sequence = source.Sequence;
        copy.EventKey = source.EventKey;
        copy.EventType = source.EventType;
        copy.PhaseKey = source.PhaseKey;
        copy.EncounterId = source.EncounterId;
        copy.ActorId = source.ActorId;
        copy.ActorName = source.ActorName;
        copy.TargetId = source.TargetId;
        copy.TargetName = source.TargetName;
        copy.ActionKey = source.ActionKey;
        copy.SkillId = source.SkillId;
        copy.Amount = source.Amount;
        copy.Value = source.Value;
        copy.DeltaHp = source.DeltaHp;
        copy.StepIndex = source.StepIndex;
        copy.TurnIndex = source.TurnIndex;
        copy.IsHeal = source.IsHeal;
        copy.IsDamage = source.IsDamage;
        copy.DidDefeat = source.DidDefeat;
        copy.DidKnockOut = source.DidKnockOut;
        copy.ShortText = source.ShortText;
        copy.Summary = source.Summary;
        copy.DetailText = source.DetailText;
        return copy;
    }
    private PrototypeBattleResultSnapshot CopyBattleResultSnapshot(PrototypeBattleResultSnapshot source)
    {
        PrototypeBattleResultSnapshot copy = new PrototypeBattleResultSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.OutcomeKey = source.OutcomeKey;
        copy.ResultStateKey = source.ResultStateKey;
        copy.ResultSummaryText = source.ResultSummaryText;
        copy.EncounterId = source.EncounterId;
        copy.EncounterName = source.EncounterName;
        copy.RouteLabel = source.RouteLabel;
        copy.CurrentDungeonName = source.CurrentDungeonName;
        copy.PartyMembersAtEndSummary = source.PartyMembersAtEndSummary;
        copy.FinalLootSummary = source.FinalLootSummary;
        copy.EliteEncounterId = source.EliteEncounterId;
        copy.EliteEncounterName = source.EliteEncounterName;
        copy.EliteRewardLabel = source.EliteRewardLabel;
        copy.EliteTypeLabel = source.EliteTypeLabel;
        copy.PreEliteChoiceSummary = source.PreEliteChoiceSummary;
        copy.SurvivingMembersText = source.SurvivingMembersText;
        copy.PartyHpSummaryText = source.PartyHpSummaryText;
        copy.PartyConditionText = source.PartyConditionText;
        copy.RoomPathSummaryText = source.RoomPathSummaryText;
        copy.EventChoiceText = source.EventChoiceText;
        copy.PreEliteChoiceText = source.PreEliteChoiceText;
        copy.RewardAmount = source.RewardAmount;
        copy.EliteBonusRewardAmount = source.EliteBonusRewardAmount;
        copy.PendingRewardAmount = source.PendingRewardAmount;
        copy.ReturnedLootAmount = source.ReturnedLootAmount;
        copy.TurnsTaken = source.TurnsTaken;
        copy.SurvivingMemberCount = source.SurvivingMemberCount;
        copy.KnockedOutMemberCount = source.KnockedOutMemberCount;
        copy.DefeatedEnemyCount = source.DefeatedEnemyCount;
        copy.EliteDefeated = source.EliteDefeated;
        copy.TotalDamageDealt = source.TotalDamageDealt;
        copy.TotalDamageTaken = source.TotalDamageTaken;
        copy.TotalHealingDone = source.TotalHealingDone;
        return copy;
    }
    private PrototypeEnemyIntentSnapshot CopyEnemyIntentSnapshot(PrototypeEnemyIntentSnapshot source)
    {
        PrototypeEnemyIntentSnapshot copy = new PrototypeEnemyIntentSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.IntentKey = source.IntentKey;
        copy.DisplayLabel = source.DisplayLabel;
        copy.TargetPatternKey = source.TargetPatternKey;
        copy.TargetPolicyKey = source.TargetPolicyKey;
        copy.PreviewText = source.PreviewText;
        copy.PredictedValue = source.PredictedValue;
        copy.PowerValue = source.PowerValue;
        copy.TargetId = source.TargetId;
        copy.TargetName = source.TargetName;
        copy.TargetDisplayName = source.TargetDisplayName;
        copy.SourceEnemyId = source.SourceEnemyId;
        copy.SourceEnemyName = source.SourceEnemyName;
        copy.ActorMonsterId = source.ActorMonsterId;
        copy.ActorDisplayName = source.ActorDisplayName;
        copy.ActionKey = source.ActionKey;
        copy.EffectTypeKey = source.EffectTypeKey;
        copy.SpecialActionLabel = source.SpecialActionLabel;
        copy.IsTelegraphed = source.IsTelegraphed;
        return copy;
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
        if (string.IsNullOrEmpty(eventKey))
        {
            return;
        }

        int sequence = _battleEventStepIndex;
        string resolvedPhaseKey = string.IsNullOrEmpty(phaseKey) ? GetBattlePhaseKey() : phaseKey;
        string resolvedActionKey = string.IsNullOrEmpty(actionKey) ? InferBattleActionKeyForEvent(actorId) : actionKey;
        string resolvedSkillId = string.IsNullOrEmpty(skillId) ? InferBattleSkillIdForEvent(actorId, resolvedActionKey) : skillId;
        string resolvedActorName = string.IsNullOrEmpty(actorName) ? GetCombatEntityDisplayName(actorId) : actorName;
        string resolvedTargetName = string.IsNullOrEmpty(targetName) ? GetCombatEntityDisplayName(targetId) : targetName;
        string resolvedSummary = string.IsNullOrEmpty(summary) ? eventKey : summary;
        string resolvedShortText = string.IsNullOrEmpty(shortText) ? resolvedSummary : shortText;

        PrototypeBattleEventRecord record = new PrototypeBattleEventRecord();
        record.EventId = eventKey + "_" + sequence;
        record.Sequence = sequence;
        record.EventKey = eventKey;
        record.EventType = string.IsNullOrEmpty(eventType) ? eventKey : eventType;
        record.PhaseKey = resolvedPhaseKey;
        record.EncounterId = BuildBattleEventEncounterId();
        record.ActorId = string.IsNullOrEmpty(actorId) ? string.Empty : actorId;
        record.ActorName = string.IsNullOrEmpty(resolvedActorName) ? string.Empty : resolvedActorName;
        record.TargetId = string.IsNullOrEmpty(targetId) ? string.Empty : targetId;
        record.TargetName = string.IsNullOrEmpty(resolvedTargetName) ? string.Empty : resolvedTargetName;
        record.ActionKey = string.IsNullOrEmpty(resolvedActionKey) ? string.Empty : resolvedActionKey;
        record.SkillId = string.IsNullOrEmpty(resolvedSkillId) ? string.Empty : resolvedSkillId;
        record.Amount = amount;
        record.Value = amount;
        record.DeltaHp = amount;
        record.StepIndex = sequence;
        record.TurnIndex = Mathf.Max(0, _battleTurnIndex);
        record.IsHeal = eventKey == PrototypeBattleEventKeys.HealApplied;
        record.IsDamage = IsBattleDamageEvent(eventKey) && amount > 0;
        record.DidDefeat = DidBattleEventDefeatTarget(eventKey, targetId);
        record.DidKnockOut = DidBattleEventKnockOutTarget(eventKey, targetId);
        record.ShortText = resolvedShortText;
        record.Summary = resolvedSummary;
        record.DetailText = BuildBattleEventDetailText(eventKey, resolvedSummary, resolvedActorName, resolvedTargetName, resolvedActionKey, amount);
        _battleEventStepIndex += 1;
        _battleEventRecords.Add(record);
        if (_battleEventRecords.Count > 32)
        {
            _battleEventRecords.RemoveAt(0);
        }
    }

    private string BuildBattleEventEncounterId()
    {
        if (_currentBattleResultSnapshot != null && !string.IsNullOrEmpty(_currentBattleResultSnapshot.EncounterId))
        {
            return _currentBattleResultSnapshot.EncounterId;
        }

        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && !string.IsNullOrEmpty(encounter.EncounterId))
        {
            return encounter.EncounterId;
        }

        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        return room != null && !string.IsNullOrEmpty(room.EncounterId) ? room.EncounterId : string.Empty;
    }

    private string BuildBattleEventDetailText(string eventKey, string summary, string actorName, string targetName, string actionKey, int amount)
    {
        if (eventKey == PrototypeBattleEventKeys.HealApplied && !string.IsNullOrEmpty(targetName))
        {
            return (string.IsNullOrEmpty(actorName) ? "Healing" : actorName) + " restored " + Mathf.Max(0, amount) + " HP to " + targetName + ".";
        }

        if (IsBattleDamageEvent(eventKey) && !string.IsNullOrEmpty(targetName))
        {
            return (string.IsNullOrEmpty(actorName) ? "Action" : actorName) + " dealt " + Mathf.Max(0, amount) + " damage to " + targetName + ".";
        }

        if (eventKey == PrototypeBattleEventKeys.TargetSelected && !string.IsNullOrEmpty(targetName))
        {
            return "Target selected: " + targetName + ".";
        }

        if (eventKey == PrototypeBattleEventKeys.ActionSelected && !string.IsNullOrEmpty(actionKey))
        {
            return "Action selected: " + actionKey + ".";
        }

        return string.IsNullOrEmpty(summary) ? eventKey : summary;
    }

    private bool IsBattleDamageEvent(string eventKey)
    {
        return eventKey == PrototypeBattleEventKeys.DamageApplied ||
               eventKey == PrototypeBattleEventKeys.AttackResolved ||
               eventKey == PrototypeBattleEventKeys.SkillResolved;
    }

    private bool DidBattleEventDefeatTarget(string eventKey, string targetId)
    {
        if (eventKey == PrototypeBattleEventKeys.EnemyDefeated)
        {
            return true;
        }

        if (!IsBattleDamageEvent(eventKey) || string.IsNullOrEmpty(targetId))
        {
            return false;
        }

        DungeonMonsterRuntimeData monster = GetMonsterById(targetId);
        return monster != null && monster.IsDefeated;
    }

    private bool DidBattleEventKnockOutTarget(string eventKey, string targetId)
    {
        if (eventKey == PrototypeBattleEventKeys.KnockOut)
        {
            return true;
        }

        if (!IsBattleDamageEvent(eventKey) || string.IsNullOrEmpty(targetId))
        {
            return false;
        }

        DungeonPartyMemberRuntimeData member = GetPartyMemberById(targetId);
        return member != null && member.IsDefeated;
    }
    public PrototypeBattleEventRecord[] GetRecentBattleEventRecords()
    {
        return BuildRecentBattleEventRecords();
    }

    public PrototypeBattleEventRecord GetLatestBattleEventRecord()
    {
        if (_battleEventRecords.Count <= 0)
        {
            return new PrototypeBattleEventRecord();
        }

        return CopyBattleEventRecord(_battleEventRecords[_battleEventRecords.Count - 1]);
    }

    public PrototypeBattleResultSnapshot BuildBattleResultSnapshot()
    {
        return BuildCurrentBattleResultSnapshotView();
    }

    private PrototypeBattleEventRecord[] BuildRecentBattleEventRecords(int maxCount = 8)
    {
        if (_battleEventRecords.Count == 0)
        {
            return System.Array.Empty<PrototypeBattleEventRecord>();
        }

        int count = Mathf.Clamp(maxCount, 1, _battleEventRecords.Count);
        PrototypeBattleEventRecord[] records = new PrototypeBattleEventRecord[count];
        int startIndex = _battleEventRecords.Count - count;
        for (int i = 0; i < count; i++)
        {
            records[i] = CopyBattleEventRecord(_battleEventRecords[startIndex + i]);
        }

        return records;
    }

    private PrototypeBattleResultSnapshot BuildCurrentBattleResultSnapshotView()
    {
        if (_currentBattleResultSnapshot != null &&
            ((!string.IsNullOrEmpty(_currentBattleResultSnapshot.ResultStateKey) && _currentBattleResultSnapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None) ||
             !string.IsNullOrEmpty(_currentBattleResultSnapshot.EncounterName)))
        {
            return CopyBattleResultSnapshot(_currentBattleResultSnapshot);
        }

        string outcomeKey = _currentBattleResultSnapshot != null && !string.IsNullOrEmpty(_currentBattleResultSnapshot.OutcomeKey)
            ? _currentBattleResultSnapshot.OutcomeKey
            : PrototypeBattleOutcomeKeys.None;
        return BuildBattleResultSnapshot(outcomeKey);
    }

    private PrototypeEnemyIntentSnapshot BuildCurrentEnemyIntentSnapshotView()
    {
        return CopyEnemyIntentSnapshot(_currentEnemyIntentSnapshot);
    }

    private void ResetCombatRulePipelineState()
    {
        _battleEventRecords.Clear();
        _currentBattleResultSnapshot = new PrototypeBattleResultSnapshot();
        _currentEnemyIntentSnapshot = new PrototypeEnemyIntentSnapshot();
        _battleEventStepIndex = 0;
        _totalDamageDealt = 0;
        _totalDamageTaken = 0;
        _totalHealingDone = 0;
    }

    private void UpdateBattleResultSnapshot(string outcomeKey)
    {
        _currentBattleResultSnapshot = BuildBattleResultSnapshot(outcomeKey);
    }

    private PrototypeBattleResultSnapshot BuildBattleResultSnapshot(string outcomeKey)
    {
        PrototypeBattleResultSnapshot snapshot = new PrototypeBattleResultSnapshot();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        string encounterId = encounter != null ? encounter.EncounterId : room != null ? room.EncounterId : string.Empty;
        string encounterName = encounter != null ? encounter.DisplayName : room != null ? room.DisplayName : string.Empty;
        PrototypeRpgRunResultSnapshot runResultSnapshot = HasRpgRunResultSnapshotData(_latestRpgRunResultSnapshot)
            ? _latestRpgRunResultSnapshot
            : null;
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = runResultSnapshot != null && runResultSnapshot.PartyOutcome != null
            ? runResultSnapshot.PartyOutcome
            : new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = runResultSnapshot != null && runResultSnapshot.LootOutcome != null
            ? runResultSnapshot.LootOutcome
            : new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = runResultSnapshot != null && runResultSnapshot.EliteOutcome != null
            ? runResultSnapshot.EliteOutcome
            : new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = runResultSnapshot != null && runResultSnapshot.EncounterOutcome != null
            ? runResultSnapshot.EncounterOutcome
            : new PrototypeRpgEncounterOutcomeSnapshot();

        snapshot.OutcomeKey = string.IsNullOrEmpty(outcomeKey) ? PrototypeBattleOutcomeKeys.None : outcomeKey;
        snapshot.ResultStateKey = snapshot.OutcomeKey;
        snapshot.ResultSummaryText = BuildBattleResultSummaryText(snapshot.OutcomeKey, runResultSnapshot);
        snapshot.EncounterId = string.IsNullOrEmpty(encounterId) ? string.Empty : encounterId;
        snapshot.EncounterName = string.IsNullOrEmpty(encounterName)
            ? (!string.IsNullOrEmpty(_eliteName) ? _eliteName : string.Empty)
            : encounterName;
        snapshot.RouteLabel = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.RouteLabel)
            ? runResultSnapshot.RouteLabel
            : (string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel);
        snapshot.CurrentDungeonName = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.DungeonLabel)
            ? runResultSnapshot.DungeonLabel
            : (string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName);
        snapshot.PartyMembersAtEndSummary = !string.IsNullOrEmpty(partyOutcome.PartyMembersAtEndSummary)
            ? partyOutcome.PartyMembersAtEndSummary
            : BuildPartyMembersAtEndSummary();
        snapshot.FinalLootSummary = !string.IsNullOrEmpty(lootOutcome.FinalLootSummary)
            ? lootOutcome.FinalLootSummary
            : BuildLootBreakdownSummary();
        snapshot.EliteEncounterId = string.IsNullOrEmpty(_eliteEncounterId) ? string.Empty : _eliteEncounterId;
        snapshot.EliteEncounterName = !string.IsNullOrEmpty(eliteOutcome.EliteName)
            ? eliteOutcome.EliteName
            : (string.IsNullOrEmpty(_eliteName) ? "None" : _eliteName);
        snapshot.EliteRewardLabel = !string.IsNullOrEmpty(eliteOutcome.EliteRewardLabel)
            ? eliteOutcome.EliteRewardLabel
            : (string.IsNullOrEmpty(_eliteRewardLabel) ? "None" : _eliteRewardLabel);
        snapshot.EliteTypeLabel = !string.IsNullOrEmpty(eliteOutcome.EliteTypeLabel)
            ? eliteOutcome.EliteTypeLabel
            : (string.IsNullOrEmpty(_eliteType) ? "None" : _eliteType);
        snapshot.PreEliteChoiceSummary = !string.IsNullOrEmpty(encounterOutcome.SelectedPreEliteChoice)
            ? encounterOutcome.SelectedPreEliteChoice
            : GetSelectedPreEliteChoiceDisplayText();
        snapshot.SurvivingMembersText = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.SurvivingMembersSummary)
            ? runResultSnapshot.SurvivingMembersSummary
            : BuildSurvivingMembersSummary();
        snapshot.PartyHpSummaryText = !string.IsNullOrEmpty(partyOutcome.PartyHpSummaryText)
            ? partyOutcome.PartyHpSummaryText
            : (string.IsNullOrEmpty(_resultPartyHpSummaryText) ? BuildTotalPartyHpSummary() : _resultPartyHpSummaryText);
        snapshot.PartyConditionText = !string.IsNullOrEmpty(partyOutcome.PartyConditionText)
            ? partyOutcome.PartyConditionText
            : (string.IsNullOrEmpty(_resultPartyConditionText) ? GetPartyConditionText() : _resultPartyConditionText);
        snapshot.RoomPathSummaryText = !string.IsNullOrEmpty(encounterOutcome.RoomPathSummary)
            ? encounterOutcome.RoomPathSummary
            : (string.IsNullOrEmpty(_resultRoomPathSummaryText) ? BuildSelectedRouteSummary() : _resultRoomPathSummaryText);
        snapshot.EventChoiceText = !string.IsNullOrEmpty(encounterOutcome.SelectedEventChoice)
            ? encounterOutcome.SelectedEventChoice
            : GetSelectedEventChoiceDisplayText();
        snapshot.PreEliteChoiceText = !string.IsNullOrEmpty(encounterOutcome.SelectedPreEliteChoice)
            ? encounterOutcome.SelectedPreEliteChoice
            : GetSelectedPreEliteChoiceDisplayText();
        snapshot.RewardAmount = eliteOutcome.EliteRewardAmount > 0
            ? eliteOutcome.EliteRewardAmount
            : Mathf.Max(_resultEliteRewardAmount, _eliteRewardAmount);
        snapshot.EliteBonusRewardAmount = eliteOutcome.EliteBonusRewardAmount > 0
            ? eliteOutcome.EliteBonusRewardAmount
            : Mathf.Max(_resultEliteBonusRewardAmount, _eliteBonusRewardGrantedAmount);
        snapshot.PendingRewardAmount = Mathf.Max(0, _eliteBonusRewardPending);
        snapshot.ReturnedLootAmount = runResultSnapshot != null
            ? Mathf.Max(0, lootOutcome.TotalLootGained)
            : Mathf.Max(0, _carriedLootAmount);
        snapshot.TurnsTaken = runResultSnapshot != null
            ? Mathf.Max(0, runResultSnapshot.TotalTurnsTaken)
            : Mathf.Max(_runTurnCount, _battleTurnIndex);
        snapshot.SurvivingMemberCount = partyOutcome.SurvivingMemberCount > 0 || partyOutcome.KnockedOutMemberCount > 0
            ? Mathf.Max(0, partyOutcome.SurvivingMemberCount)
            : GetLivingPartyMemberCount();
        snapshot.KnockedOutMemberCount = partyOutcome.SurvivingMemberCount > 0 || partyOutcome.KnockedOutMemberCount > 0
            ? Mathf.Max(0, partyOutcome.KnockedOutMemberCount)
            : GetKnockedOutMemberCount();
        snapshot.DefeatedEnemyCount = GetDefeatedEnemyCount();
        snapshot.EliteDefeated = eliteOutcome.IsEliteDefeated || _eliteDefeated;
        snapshot.TotalDamageDealt = Mathf.Max(0, _totalDamageDealt);
        snapshot.TotalDamageTaken = Mathf.Max(0, _totalDamageTaken);
        snapshot.TotalHealingDone = Mathf.Max(0, _totalHealingDone);
        return snapshot;
    }

    private string BuildBattleResultSummaryText(string outcomeKey, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.ResultSummary))
        {
            return runResultSnapshot.ResultSummary;
        }

        switch (outcomeKey)
        {
            case PrototypeBattleOutcomeKeys.EncounterVictory:
                return "Encounter cleared.";
            case PrototypeBattleOutcomeKeys.RunClear:
                return "The run ended in victory.";
            case PrototypeBattleOutcomeKeys.RunDefeat:
                return "The party was defeated.";
            case PrototypeBattleOutcomeKeys.RunRetreat:
                return "The party retreated.";
            default:
                return string.IsNullOrEmpty(_battleFeedbackText) ? "Battle in progress." : _battleFeedbackText;
        }
    }
    private PrototypeRpgRunResultSnapshot CopyRpgRunResultSnapshot(PrototypeRpgRunResultSnapshot source)
    {
        PrototypeRpgRunResultSnapshot copy = new PrototypeRpgRunResultSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.RunIdentity = string.IsNullOrEmpty(source.RunIdentity) ? string.Empty : source.RunIdentity;
        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.TotalTurnsTaken = Mathf.Max(0, source.TotalTurnsTaken);
        copy.ResultSummary = string.IsNullOrEmpty(source.ResultSummary) ? string.Empty : source.ResultSummary;
        copy.SurvivingMembersSummary = string.IsNullOrEmpty(source.SurvivingMembersSummary) ? string.Empty : source.SurvivingMembersSummary;
        copy.AppliedProgressSummaryText = string.IsNullOrEmpty(source.AppliedProgressSummaryText) ? string.Empty : source.AppliedProgressSummaryText;
        copy.PartyOutcome = CopyRpgPartyOutcomeSnapshot(source.PartyOutcome);
        copy.LootOutcome = CopyRpgLootOutcomeSnapshot(source.LootOutcome);
        copy.EliteOutcome = CopyRpgEliteOutcomeSnapshot(source.EliteOutcome);
        copy.EncounterOutcome = CopyRpgEncounterOutcomeSnapshot(source.EncounterOutcome);
        return copy;
    }

    private PrototypeRpgPartyOutcomeSnapshot CopyRpgPartyOutcomeSnapshot(PrototypeRpgPartyOutcomeSnapshot source)
    {
        PrototypeRpgPartyOutcomeSnapshot copy = new PrototypeRpgPartyOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.PartyConditionText = string.IsNullOrEmpty(source.PartyConditionText) ? string.Empty : source.PartyConditionText;
        copy.PartyHpSummaryText = string.IsNullOrEmpty(source.PartyHpSummaryText) ? string.Empty : source.PartyHpSummaryText;
        copy.PartyMembersAtEndSummary = string.IsNullOrEmpty(source.PartyMembersAtEndSummary) ? string.Empty : source.PartyMembersAtEndSummary;
        copy.AppliedPartySummaryText = string.IsNullOrEmpty(source.AppliedPartySummaryText) ? string.Empty : source.AppliedPartySummaryText;
        copy.SurvivingMemberCount = Mathf.Max(0, source.SurvivingMemberCount);
        copy.KnockedOutMemberCount = Mathf.Max(0, source.KnockedOutMemberCount);

        if (source.Members == null || source.Members.Length == 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
            return copy;
        }

        PrototypeRpgPartyMemberOutcomeSnapshot[] members = new PrototypeRpgPartyMemberOutcomeSnapshot[source.Members.Length];
        for (int i = 0; i < source.Members.Length; i++)
        {
            members[i] = CopyRpgPartyMemberOutcomeSnapshot(source.Members[i]);
        }

        copy.Members = members;
        return copy;
    }

    private PrototypeRpgPartyMemberOutcomeSnapshot CopyRpgPartyMemberOutcomeSnapshot(PrototypeRpgPartyMemberOutcomeSnapshot source)
    {
        PrototypeRpgPartyMemberOutcomeSnapshot copy = new PrototypeRpgPartyMemberOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.RoleLabel = string.IsNullOrEmpty(source.RoleLabel) ? string.Empty : source.RoleLabel;
        copy.DefaultSkillId = string.IsNullOrEmpty(source.DefaultSkillId) ? string.Empty : source.DefaultSkillId;
        copy.AppliedRoleLabel = string.IsNullOrEmpty(source.AppliedRoleLabel) ? string.Empty : source.AppliedRoleLabel;
        copy.AppliedDefaultSkillId = string.IsNullOrEmpty(source.AppliedDefaultSkillId) ? string.Empty : source.AppliedDefaultSkillId;
        copy.AppliedGrowthTrackId = string.IsNullOrEmpty(source.AppliedGrowthTrackId) ? string.Empty : source.AppliedGrowthTrackId;
        copy.AppliedJobId = string.IsNullOrEmpty(source.AppliedJobId) ? string.Empty : source.AppliedJobId;
        copy.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(source.AppliedEquipmentLoadoutId) ? string.Empty : source.AppliedEquipmentLoadoutId;
        copy.AppliedSkillLoadoutId = string.IsNullOrEmpty(source.AppliedSkillLoadoutId) ? string.Empty : source.AppliedSkillLoadoutId;
        copy.AppliedProgressSummaryText = string.IsNullOrEmpty(source.AppliedProgressSummaryText) ? string.Empty : source.AppliedProgressSummaryText;
        copy.CurrentHp = Mathf.Max(0, source.CurrentHp);
        copy.MaxHp = Mathf.Max(1, source.MaxHp);
        copy.Survived = source.Survived;
        copy.KnockedOut = source.KnockedOut;
        return copy;
    }

    private PrototypeRpgLootOutcomeSnapshot CopyRpgLootOutcomeSnapshot(PrototypeRpgLootOutcomeSnapshot source)
    {
        PrototypeRpgLootOutcomeSnapshot copy = new PrototypeRpgLootOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.RewardResourceId = string.IsNullOrEmpty(source.RewardResourceId) ? string.Empty : source.RewardResourceId;
        copy.TotalLootGained = Mathf.Max(0, source.TotalLootGained);
        copy.TotalReturnedAmount = Mathf.Max(0, source.TotalReturnedAmount);
        copy.BattleLootGained = Mathf.Max(0, source.BattleLootGained);
        copy.ChestLootGained = Mathf.Max(0, source.ChestLootGained);
        copy.EventLootGained = Mathf.Max(0, source.EventLootGained);
        copy.EliteRewardAmount = Mathf.Max(0, source.EliteRewardAmount);
        copy.EliteBonusRewardAmount = Mathf.Max(0, source.EliteBonusRewardAmount);
        copy.PendingBonusRewardLostAmount = Mathf.Max(0, source.PendingBonusRewardLostAmount);
        copy.CarryoverHintText = string.IsNullOrEmpty(source.CarryoverHintText) ? string.Empty : source.CarryoverHintText;
        copy.FinalLootSummary = string.IsNullOrEmpty(source.FinalLootSummary) ? string.Empty : source.FinalLootSummary;
        return copy;
    }

    private PrototypeRpgEliteOutcomeSnapshot CopyRpgEliteOutcomeSnapshot(PrototypeRpgEliteOutcomeSnapshot source)
    {
        PrototypeRpgEliteOutcomeSnapshot copy = new PrototypeRpgEliteOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.IsEliteDefeated = source.IsEliteDefeated;
        copy.EliteName = string.IsNullOrEmpty(source.EliteName) ? string.Empty : source.EliteName;
        copy.EliteTypeLabel = string.IsNullOrEmpty(source.EliteTypeLabel) ? string.Empty : source.EliteTypeLabel;
        copy.EliteRewardLabel = string.IsNullOrEmpty(source.EliteRewardLabel) ? string.Empty : source.EliteRewardLabel;
        copy.EliteRewardAmount = Mathf.Max(0, source.EliteRewardAmount);
        copy.EliteBonusRewardEarned = source.EliteBonusRewardEarned;
        copy.EliteBonusRewardAmount = Mathf.Max(0, source.EliteBonusRewardAmount);
        return copy;
    }

    private PrototypeRpgEncounterOutcomeSnapshot CopyRpgEncounterOutcomeSnapshot(PrototypeRpgEncounterOutcomeSnapshot source)
    {
        PrototypeRpgEncounterOutcomeSnapshot copy = new PrototypeRpgEncounterOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ClearedEncounterCount = Mathf.Max(0, source.ClearedEncounterCount);
        copy.ClearedEncounterSummary = string.IsNullOrEmpty(source.ClearedEncounterSummary) ? string.Empty : source.ClearedEncounterSummary;
        copy.OpenedChestCount = Mathf.Max(0, source.OpenedChestCount);
        copy.RoomPathSummary = string.IsNullOrEmpty(source.RoomPathSummary) ? string.Empty : source.RoomPathSummary;
        copy.SelectedEventChoice = string.IsNullOrEmpty(source.SelectedEventChoice) ? string.Empty : source.SelectedEventChoice;
        copy.SelectedPreEliteChoice = string.IsNullOrEmpty(source.SelectedPreEliteChoice) ? string.Empty : source.SelectedPreEliteChoice;
        copy.PreEliteHealAmount = Mathf.Max(0, source.PreEliteHealAmount);
        return copy;
    }

    private string BuildRpgRunResultLootSummary(PrototypeRpgLootOutcomeSnapshot lootOutcome)
    {
        if (lootOutcome == null)
        {
            return "None";
        }

        int battleLoot = Mathf.Max(0, lootOutcome.BattleLootGained);
        int chestLoot = Mathf.Max(0, lootOutcome.ChestLootGained);
        int eventLoot = Mathf.Max(0, lootOutcome.EventLootGained);
        int eliteLoot = Mathf.Max(0, lootOutcome.EliteRewardAmount);
        int eliteBonusLoot = Mathf.Max(0, lootOutcome.EliteBonusRewardAmount);
        int totalLoot = Mathf.Max(Mathf.Max(0, lootOutcome.TotalLootGained), battleLoot + chestLoot + eventLoot + eliteLoot + eliteBonusLoot);
        return "Battle " + battleLoot + " / Chest " + chestLoot + " / Event " + eventLoot + " / Elite " + eliteLoot + " / Elite Bonus " + eliteBonusLoot + " / Total " + totalLoot;
    }

    private PrototypeRpgPartyOutcomeSnapshot BuildRpgPartyOutcomeSnapshot()
    {
        PrototypeRpgPartyOutcomeSnapshot snapshot = new PrototypeRpgPartyOutcomeSnapshot();
        snapshot.PartyConditionText = GetPartyConditionText();
        snapshot.PartyHpSummaryText = BuildTotalPartyHpSummary();
        snapshot.PartyMembersAtEndSummary = BuildPartyMembersAtEndSummary();
        snapshot.SurvivingMemberCount = GetLivingPartyMemberCount();
        snapshot.KnockedOutMemberCount = GetKnockedOutMemberCount();

        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
            return snapshot;
        }

        PrototypeRpgPartyMemberOutcomeSnapshot[] members = new PrototypeRpgPartyMemberOutcomeSnapshot[_activeDungeonParty.Members.Length];
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberSnapshot = new PrototypeRpgPartyMemberOutcomeSnapshot();
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null)
            {
                memberSnapshot.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
                memberSnapshot.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? string.Empty : member.DisplayName;
                memberSnapshot.RoleTag = string.IsNullOrEmpty(member.RoleTag) ? string.Empty : member.RoleTag;
                memberSnapshot.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? string.Empty : member.RoleLabel;
                memberSnapshot.DefaultSkillId = string.IsNullOrEmpty(member.DefaultSkillId) ? string.Empty : member.DefaultSkillId;
                memberSnapshot.CurrentHp = Mathf.Max(0, member.CurrentHp);
                memberSnapshot.MaxHp = Mathf.Max(1, member.MaxHp);
                memberSnapshot.Survived = !member.IsDefeated && member.CurrentHp > 0;
                memberSnapshot.KnockedOut = member.IsDefeated;
            }

            members[i] = memberSnapshot;
        }

        snapshot.Members = members;
        return snapshot;
    }

    private PrototypeRpgRunResultSnapshot BuildRpgRunResultSnapshot(string resultStateKey, int safeReturnedLoot, string safeResultSummary, int lostPendingRewardAmount)
    {
        PrototypeRpgRunResultSnapshot snapshot = new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = BuildRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = new PrototypeRpgEncounterOutcomeSnapshot();
        int battleLoot = safeReturnedLoot > 0 ? Mathf.Max(0, _battleLootAmount) : 0;
        int chestLoot = safeReturnedLoot > 0 ? Mathf.Max(0, _chestLootAmount) : 0;
        int eventLoot = safeReturnedLoot > 0 ? Mathf.Max(0, _eventLootAmount) : 0;
        int eliteLoot = _eliteRewardGranted ? Mathf.Max(0, _eliteRewardAmount) : 0;
        int eliteBonusLoot = _eliteBonusRewardGranted ? Mathf.Max(0, _eliteBonusRewardGrantedAmount) : 0;

        snapshot.ResultStateKey = string.IsNullOrEmpty(resultStateKey) ? PrototypeBattleOutcomeKeys.None : resultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(_currentDungeonId) ? string.Empty : _currentDungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(_currentDungeonName) ? string.Empty : _currentDungeonName;
        snapshot.RouteId = string.IsNullOrEmpty(_selectedRouteId) ? string.Empty : _selectedRouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? string.Empty : _selectedRouteLabel;
        snapshot.TotalTurnsTaken = Mathf.Max(0, _runTurnCount);
        snapshot.ResultSummary = string.IsNullOrEmpty(safeResultSummary) ? "The run ended." : safeResultSummary;
        snapshot.SurvivingMembersSummary = BuildSurvivingMembersSummary();
        snapshot.RunIdentity = (string.IsNullOrEmpty(snapshot.DungeonId) ? "dungeon" : snapshot.DungeonId) + "|" + (string.IsNullOrEmpty(snapshot.RouteId) ? "route" : snapshot.RouteId) + "|" + snapshot.TotalTurnsTaken + "|" + snapshot.ResultStateKey;

        lootOutcome.RewardResourceId = DungeonRewardResourceId;
        lootOutcome.BattleLootGained = battleLoot;
        lootOutcome.ChestLootGained = chestLoot;
        lootOutcome.EventLootGained = eventLoot;
        lootOutcome.EliteRewardAmount = eliteLoot;
        lootOutcome.EliteBonusRewardAmount = eliteBonusLoot;
        lootOutcome.PendingBonusRewardLostAmount = Mathf.Max(0, lostPendingRewardAmount);
        lootOutcome.TotalLootGained = Mathf.Max(Mathf.Max(0, safeReturnedLoot), battleLoot + chestLoot + eventLoot + eliteLoot + eliteBonusLoot);
        lootOutcome.TotalReturnedAmount = lootOutcome.TotalLootGained;
        lootOutcome.FinalLootSummary = BuildRpgRunResultLootSummary(lootOutcome);
        lootOutcome.CarryoverHintText = BuildRpgRewardCarryoverHintText(lootOutcome, snapshot.ResultStateKey);

        eliteOutcome.IsEliteDefeated = _eliteDefeated;
        eliteOutcome.EliteName = string.IsNullOrEmpty(_eliteName) ? string.Empty : _eliteName;
        eliteOutcome.EliteTypeLabel = string.IsNullOrEmpty(_eliteType) ? string.Empty : _eliteType;
        eliteOutcome.EliteRewardLabel = string.IsNullOrEmpty(_eliteRewardLabel) ? string.Empty : _eliteRewardLabel;
        eliteOutcome.EliteRewardAmount = eliteLoot;
        eliteOutcome.EliteBonusRewardEarned = _eliteBonusRewardGranted;
        eliteOutcome.EliteBonusRewardAmount = eliteBonusLoot;

        encounterOutcome.ClearedEncounterCount = Mathf.Max(0, _clearedEncounterCount);
        encounterOutcome.ClearedEncounterSummary = BuildClearedEncounterSummary();
        encounterOutcome.OpenedChestCount = Mathf.Max(0, _chestOpenedCount);
        encounterOutcome.RoomPathSummary = BuildCurrentRoomPathSummary();
        encounterOutcome.SelectedEventChoice = GetSelectedEventChoiceDisplayText();
        encounterOutcome.SelectedPreEliteChoice = GetSelectedPreEliteChoiceDisplayText();
        encounterOutcome.PreEliteHealAmount = Mathf.Max(0, _preEliteHealAmount);

        snapshot.PartyOutcome = partyOutcome;
        snapshot.LootOutcome = lootOutcome;
        snapshot.EliteOutcome = eliteOutcome;
        snapshot.EncounterOutcome = encounterOutcome;
        ApplyRpgAppliedProgressToRunResultSnapshot(snapshot, GetLatestAppliedPartyProgressStateInternal());
        return snapshot;
    }

    private void ApplyRpgRunResultSnapshotToLegacyFields(PrototypeRpgRunResultSnapshot snapshot)
    {
        PrototypeRpgRunResultSnapshot safeSnapshot = snapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = safeSnapshot.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = safeSnapshot.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeSnapshot.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = safeSnapshot.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();

        _resultTurnsTaken = Mathf.Max(0, safeSnapshot.TotalTurnsTaken);
        _resultLootGained = Mathf.Max(0, lootOutcome.TotalLootGained);
        _resultBattleLootGained = Mathf.Max(0, lootOutcome.BattleLootGained);
        _resultChestLootGained = Mathf.Max(0, lootOutcome.ChestLootGained);
        _resultEventLootGained = Mathf.Max(0, lootOutcome.EventLootGained);
        _resultSurvivingMembersText = string.IsNullOrEmpty(safeSnapshot.SurvivingMembersSummary) ? string.Empty : safeSnapshot.SurvivingMembersSummary;
        _resultPartyHpSummaryText = string.IsNullOrEmpty(partyOutcome.PartyHpSummaryText) ? string.Empty : partyOutcome.PartyHpSummaryText;
        _resultPartyConditionText = string.IsNullOrEmpty(partyOutcome.PartyConditionText) ? string.Empty : partyOutcome.PartyConditionText;
        _resultEventChoiceText = string.IsNullOrEmpty(encounterOutcome.SelectedEventChoice) ? string.Empty : encounterOutcome.SelectedEventChoice;
        _resultPreEliteChoiceText = string.IsNullOrEmpty(encounterOutcome.SelectedPreEliteChoice) ? string.Empty : encounterOutcome.SelectedPreEliteChoice;
        _resultPreEliteHealAmount = Mathf.Max(0, encounterOutcome.PreEliteHealAmount);
        _resultClearedEncounters = Mathf.Max(0, encounterOutcome.ClearedEncounterCount);
        _resultOpenedChests = Mathf.Max(0, encounterOutcome.OpenedChestCount);
        _resultEliteDefeated = eliteOutcome.IsEliteDefeated;
        _resultEliteName = string.IsNullOrEmpty(eliteOutcome.EliteName) ? string.Empty : eliteOutcome.EliteName;
        _resultEliteRewardLabel = string.IsNullOrEmpty(eliteOutcome.EliteRewardLabel) ? string.Empty : eliteOutcome.EliteRewardLabel;
        _resultEliteRewardAmount = Mathf.Max(0, eliteOutcome.EliteRewardAmount);
        _resultEliteBonusRewardAmount = Mathf.Max(0, eliteOutcome.EliteBonusRewardAmount);
        _resultRoomPathSummaryText = string.IsNullOrEmpty(encounterOutcome.RoomPathSummary) ? string.Empty : encounterOutcome.RoomPathSummary;
    }
    private PrototypeRpgProgressionSeedSnapshot CopyRpgProgressionSeedSnapshot(PrototypeRpgProgressionSeedSnapshot source)
    {
        PrototypeRpgProgressionSeedSnapshot copy = new PrototypeRpgProgressionSeedSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.DungeonDangerLabel = string.IsNullOrEmpty(source.DungeonDangerLabel) ? string.Empty : source.DungeonDangerLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.RouteRiskLabel = string.IsNullOrEmpty(source.RouteRiskLabel) ? string.Empty : source.RouteRiskLabel;
        copy.TotalTurnsTaken = Mathf.Max(0, source.TotalTurnsTaken);
        copy.ClearedEncounterCount = Mathf.Max(0, source.ClearedEncounterCount);
        copy.EliteDefeated = source.EliteDefeated;
        copy.EliteName = string.IsNullOrEmpty(source.EliteName) ? string.Empty : source.EliteName;
        copy.EliteTypeLabel = string.IsNullOrEmpty(source.EliteTypeLabel) ? string.Empty : source.EliteTypeLabel;
        copy.PartyConditionText = string.IsNullOrEmpty(source.PartyConditionText) ? string.Empty : source.PartyConditionText;
        copy.SurvivingMemberCount = Mathf.Max(0, source.SurvivingMemberCount);
        copy.KnockedOutMemberCount = Mathf.Max(0, source.KnockedOutMemberCount);
        copy.EliteClearBonusHint = string.IsNullOrEmpty(source.EliteClearBonusHint) ? string.Empty : source.EliteClearBonusHint;
        copy.RouteRiskHint = string.IsNullOrEmpty(source.RouteRiskHint) ? string.Empty : source.RouteRiskHint;
        copy.DungeonDangerHint = string.IsNullOrEmpty(source.DungeonDangerHint) ? string.Empty : source.DungeonDangerHint;
        copy.Loot = source.Loot != null
            ? new PrototypeRpgLootSeed
            {
                TotalLootGained = Mathf.Max(0, source.Loot.TotalLootGained),
                BattleLootGained = Mathf.Max(0, source.Loot.BattleLootGained),
                ChestLootGained = Mathf.Max(0, source.Loot.ChestLootGained),
                EventLootGained = Mathf.Max(0, source.Loot.EventLootGained),
                EliteRewardAmount = Mathf.Max(0, source.Loot.EliteRewardAmount),
                EliteBonusRewardAmount = Mathf.Max(0, source.Loot.EliteBonusRewardAmount),
                PendingBonusRewardLostAmount = Mathf.Max(0, source.Loot.PendingBonusRewardLostAmount),
                CarryoverHintText = string.IsNullOrEmpty(source.Loot.CarryoverHintText) ? string.Empty : source.Loot.CarryoverHintText,
                LootBreakdownSummary = string.IsNullOrEmpty(source.Loot.LootBreakdownSummary) ? string.Empty : source.Loot.LootBreakdownSummary
            }
            : new PrototypeRpgLootSeed();
        copy.RewardTags = source.RewardTags != null && source.RewardTags.Length > 0 ? (string[])source.RewardTags.Clone() : System.Array.Empty<string>();
        copy.GrowthTags = source.GrowthTags != null && source.GrowthTags.Length > 0 ? (string[])source.GrowthTags.Clone() : System.Array.Empty<string>();

        PrototypeRpgMemberProgressionSeed[] sourceMembers = source.Members ?? System.Array.Empty<PrototypeRpgMemberProgressionSeed>();
        if (sourceMembers.Length > 0)
        {
            PrototypeRpgMemberProgressionSeed[] memberCopies = new PrototypeRpgMemberProgressionSeed[sourceMembers.Length];
            for (int i = 0; i < sourceMembers.Length; i++)
            {
                PrototypeRpgMemberProgressionSeed member = sourceMembers[i] ?? new PrototypeRpgMemberProgressionSeed();
                memberCopies[i] = new PrototypeRpgMemberProgressionSeed
                {
                    MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId,
                    DisplayName = string.IsNullOrEmpty(member.DisplayName) ? string.Empty : member.DisplayName,
                    RoleTag = string.IsNullOrEmpty(member.RoleTag) ? string.Empty : member.RoleTag,
                    RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? string.Empty : member.RoleLabel,
                    DefaultSkillId = string.IsNullOrEmpty(member.DefaultSkillId) ? string.Empty : member.DefaultSkillId,
                    GrowthTrackId = string.IsNullOrEmpty(member.GrowthTrackId) ? string.Empty : member.GrowthTrackId,
                    JobId = string.IsNullOrEmpty(member.JobId) ? string.Empty : member.JobId,
                    EquipmentLoadoutId = string.IsNullOrEmpty(member.EquipmentLoadoutId) ? string.Empty : member.EquipmentLoadoutId,
                    SkillLoadoutId = string.IsNullOrEmpty(member.SkillLoadoutId) ? string.Empty : member.SkillLoadoutId,
                    Survived = member.Survived,
                    KnockedOut = member.KnockedOut,
                    CurrentHp = Mathf.Max(0, member.CurrentHp),
                    MaxHp = Mathf.Max(1, member.MaxHp),
                    Combat = new PrototypeRpgCombatContributionSeed
                    {
                        DamageDealt = member.Combat != null ? Mathf.Max(0, member.Combat.DamageDealt) : 0,
                        DamageTaken = member.Combat != null ? Mathf.Max(0, member.Combat.DamageTaken) : 0,
                        HealingDone = member.Combat != null ? Mathf.Max(0, member.Combat.HealingDone) : 0,
                        ActionCount = member.Combat != null ? Mathf.Max(0, member.Combat.ActionCount) : 0,
                        KillCount = member.Combat != null ? Mathf.Max(0, member.Combat.KillCount) : 0
                    }
                };
            }
            copy.Members = memberCopies;
        }
        else
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberProgressionSeed>();
        }

        PrototypeRpgUnlockSeedSnapshot[] sourceUnlockSeeds = source.UnlockSeeds ?? System.Array.Empty<PrototypeRpgUnlockSeedSnapshot>();
        if (sourceUnlockSeeds.Length > 0)
        {
            PrototypeRpgUnlockSeedSnapshot[] unlockCopies = new PrototypeRpgUnlockSeedSnapshot[sourceUnlockSeeds.Length];
            for (int i = 0; i < sourceUnlockSeeds.Length; i++)
            {
                PrototypeRpgUnlockSeedSnapshot unlock = sourceUnlockSeeds[i] ?? new PrototypeRpgUnlockSeedSnapshot();
                unlockCopies[i] = new PrototypeRpgUnlockSeedSnapshot
                {
                    SourceRunIdentity = string.IsNullOrEmpty(unlock.SourceRunIdentity) ? string.Empty : unlock.SourceRunIdentity,
                    SourceEncounterId = string.IsNullOrEmpty(unlock.SourceEncounterId) ? string.Empty : unlock.SourceEncounterId,
                    DungeonId = string.IsNullOrEmpty(unlock.DungeonId) ? string.Empty : unlock.DungeonId,
                    RouteId = string.IsNullOrEmpty(unlock.RouteId) ? string.Empty : unlock.RouteId,
                    EliteId = string.IsNullOrEmpty(unlock.EliteId) ? string.Empty : unlock.EliteId,
                    UnlockCategoryKey = string.IsNullOrEmpty(unlock.UnlockCategoryKey) ? string.Empty : unlock.UnlockCategoryKey,
                    UnlockTargetKey = string.IsNullOrEmpty(unlock.UnlockTargetKey) ? string.Empty : unlock.UnlockTargetKey,
                    UnlockReasonText = string.IsNullOrEmpty(unlock.UnlockReasonText) ? string.Empty : unlock.UnlockReasonText,
                    ProgressionDependencyHint = string.IsNullOrEmpty(unlock.ProgressionDependencyHint) ? string.Empty : unlock.ProgressionDependencyHint,
                    IsPreviewOnly = unlock.IsPreviewOnly
                };
            }
            copy.UnlockSeeds = unlockCopies;
        }
        else
        {
            copy.UnlockSeeds = System.Array.Empty<PrototypeRpgUnlockSeedSnapshot>();
        }

        return copy;
    }

    private void ResetRpgProgressionSeedState()
    {
        _latestRpgProgressionSeedSnapshot = new PrototypeRpgProgressionSeedSnapshot();
        _latestRpgCombatContributionSnapshot = new PrototypeRpgCombatContributionSnapshot();
        _latestRpgProgressionPreviewSnapshot = new PrototypeRpgProgressionPreviewSnapshot();
        for (int i = 0; i < _runMemberDamageDealt.Length; i++)
        {
            _runMemberDamageDealt[i] = 0;
            _runMemberDamageTaken[i] = 0;
            _runMemberHealingDone[i] = 0;
            _runMemberActionCount[i] = 0;
            _runMemberKillCount[i] = 0;
        }
    }

    private void AddRunMemberContributionValue(int[] values, int memberIndex, int amount)
    {
        if (values == null || memberIndex < 0 || memberIndex >= values.Length || amount <= 0)
        {
            return;
        }

        values[memberIndex] += amount;
    }

    private int GetRunMemberContributionValue(int[] values, int memberIndex)
    {
        if (values == null || memberIndex < 0 || memberIndex >= values.Length)
        {
            return 0;
        }

        return Mathf.Max(0, values[memberIndex]);
    }

    private void AddProgressionTag(List<string> tags, string tag)
    {
        if (tags == null || string.IsNullOrEmpty(tag) || tags.Contains(tag))
        {
            return;
        }

        tags.Add(tag);
    }

    private string[] BuildRpgProgressionRewardTags(PrototypeRpgRunResultSnapshot snapshot)
    {
        List<string> tags = new List<string>();
        if (snapshot != null && !string.IsNullOrEmpty(snapshot.ResultStateKey) && snapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None)
        {
            AddProgressionTag(tags, snapshot.ResultStateKey);
        }

        if (snapshot != null && snapshot.LootOutcome != null && snapshot.LootOutcome.TotalLootGained > 0)
        {
            AddProgressionTag(tags, "loot_returned");
        }

        if (snapshot != null && snapshot.EliteOutcome != null && snapshot.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(tags, "elite_clear");
        }

        if (snapshot != null && snapshot.EliteOutcome != null && snapshot.EliteOutcome.EliteBonusRewardEarned)
        {
            AddProgressionTag(tags, "elite_bonus");
        }

        if (_selectedRouteId == RiskyRouteId)
        {
            AddProgressionTag(tags, "risky_route");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildRpgProgressionGrowthTags(PrototypeRpgRunResultSnapshot snapshot)
    {
        List<string> tags = new List<string>();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = snapshot != null ? snapshot.PartyOutcome : null;
        if (partyOutcome != null && partyOutcome.KnockedOutMemberCount > 0)
        {
            AddProgressionTag(tags, "party_recovery");
        }

        if (partyOutcome != null && partyOutcome.SurvivingMemberCount > 0 && partyOutcome.KnockedOutMemberCount <= 0)
        {
            AddProgressionTag(tags, "clean_survival");
        }

        if (snapshot != null && snapshot.TotalTurnsTaken >= 6)
        {
            AddProgressionTag(tags, "endurance_run");
        }

        if (snapshot != null && snapshot.EliteOutcome != null && snapshot.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(tags, "elite_mastery");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private PrototypeRpgLootSeed BuildRpgProgressionLootSeed(PrototypeRpgRunResultSnapshot snapshot)
    {
        PrototypeRpgLootSeed seed = new PrototypeRpgLootSeed();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = snapshot != null ? snapshot.LootOutcome : null;
        if (lootOutcome == null)
        {
            return seed;
        }

        seed.TotalLootGained = Mathf.Max(0, lootOutcome.TotalLootGained);
        seed.BattleLootGained = Mathf.Max(0, lootOutcome.BattleLootGained);
        seed.ChestLootGained = Mathf.Max(0, lootOutcome.ChestLootGained);
        seed.EventLootGained = Mathf.Max(0, lootOutcome.EventLootGained);
        seed.EliteRewardAmount = Mathf.Max(0, lootOutcome.EliteRewardAmount);
        seed.EliteBonusRewardAmount = Mathf.Max(0, lootOutcome.EliteBonusRewardAmount);
        seed.PendingBonusRewardLostAmount = Mathf.Max(0, lootOutcome.PendingBonusRewardLostAmount);
        seed.CarryoverHintText = string.IsNullOrEmpty(lootOutcome.CarryoverHintText) ? string.Empty : lootOutcome.CarryoverHintText;
        seed.LootBreakdownSummary = !string.IsNullOrEmpty(lootOutcome.FinalLootSummary) ? lootOutcome.FinalLootSummary : BuildRpgLootBreakdownText(lootOutcome);
        return seed;
    }

    private PrototypeRpgPartyMemberDefinition FindActivePartyMemberDefinition(string memberId)
    {
        if (string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        TestDungeonPartyData party = _activeDungeonParty;
        PrototypeRpgPartyDefinition partyDefinition = party != null ? party.PartyDefinition : null;
        PrototypeRpgPartyMemberDefinition[] members = partyDefinition != null ? (partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>()) : System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition member = members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private PrototypeRpgProgressionSeedSnapshot BuildRpgProgressionSeedSnapshot(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgProgressionSeedSnapshot snapshot = new PrototypeRpgProgressionSeedSnapshot();
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = safeRunResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeRunResult.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = safeRunResult.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome.Members ?? System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();

        snapshot.ResultStateKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? string.Empty : safeRunResult.ResultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(safeRunResult.DungeonId) ? string.Empty : safeRunResult.DungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(safeRunResult.DungeonLabel) ? string.Empty : safeRunResult.DungeonLabel;
        snapshot.DungeonDangerLabel = GetCurrentDungeonDangerText();
        snapshot.RouteId = string.IsNullOrEmpty(safeRunResult.RouteId) ? string.Empty : safeRunResult.RouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(safeRunResult.RouteLabel) ? string.Empty : safeRunResult.RouteLabel;
        snapshot.RouteRiskLabel = string.IsNullOrEmpty(_selectedRouteRiskLabel) ? string.Empty : _selectedRouteRiskLabel;
        snapshot.TotalTurnsTaken = Mathf.Max(0, safeRunResult.TotalTurnsTaken);
        snapshot.ClearedEncounterCount = Mathf.Max(0, encounterOutcome.ClearedEncounterCount);
        snapshot.EliteDefeated = eliteOutcome.IsEliteDefeated;
        snapshot.EliteName = string.IsNullOrEmpty(eliteOutcome.EliteName) ? string.Empty : eliteOutcome.EliteName;
        snapshot.EliteTypeLabel = string.IsNullOrEmpty(eliteOutcome.EliteTypeLabel) ? string.Empty : eliteOutcome.EliteTypeLabel;
        snapshot.PartyConditionText = string.IsNullOrEmpty(partyOutcome.PartyConditionText) ? string.Empty : partyOutcome.PartyConditionText;
        snapshot.SurvivingMemberCount = Mathf.Max(0, partyOutcome.SurvivingMemberCount);
        snapshot.KnockedOutMemberCount = Mathf.Max(0, partyOutcome.KnockedOutMemberCount);
        snapshot.EliteClearBonusHint = eliteOutcome.EliteBonusRewardEarned ? "Elite bonus reward earned." : eliteOutcome.IsEliteDefeated ? "Elite clear completed." : "Elite clear not achieved.";
        snapshot.RouteRiskHint = string.IsNullOrEmpty(snapshot.RouteRiskLabel) ? "Route risk: none." : "Route risk: " + snapshot.RouteRiskLabel + ".";
        snapshot.DungeonDangerHint = string.IsNullOrEmpty(snapshot.DungeonDangerLabel) ? "Dungeon danger: none." : "Dungeon danger: " + snapshot.DungeonDangerLabel + ".";
        snapshot.Loot = BuildRpgProgressionLootSeed(safeRunResult);
        snapshot.RewardTags = BuildRpgProgressionRewardTags(safeRunResult);
        snapshot.GrowthTags = BuildRpgProgressionGrowthTags(safeRunResult);
        snapshot.UnlockSeeds = System.Array.Empty<PrototypeRpgUnlockSeedSnapshot>();

        if (members.Length <= 0)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgMemberProgressionSeed>();
            return snapshot;
        }

        PrototypeRpgMemberProgressionSeed[] memberSeeds = new PrototypeRpgMemberProgressionSeed[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberOutcome = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            PrototypeRpgPartyMemberDefinition memberDefinition = FindActivePartyMemberDefinition(memberOutcome.MemberId);
            PrototypeRpgMemberProgressionSeed memberSeed = new PrototypeRpgMemberProgressionSeed();
            memberSeed.MemberId = string.IsNullOrEmpty(memberOutcome.MemberId) ? string.Empty : memberOutcome.MemberId;
            memberSeed.DisplayName = string.IsNullOrEmpty(memberOutcome.DisplayName) ? string.Empty : memberOutcome.DisplayName;
            memberSeed.RoleTag = string.IsNullOrEmpty(memberOutcome.RoleTag) ? string.Empty : memberOutcome.RoleTag;
            memberSeed.RoleLabel = string.IsNullOrEmpty(memberOutcome.RoleLabel) ? string.Empty : memberOutcome.RoleLabel;
            memberSeed.DefaultSkillId = string.IsNullOrEmpty(memberOutcome.DefaultSkillId) ? string.Empty : memberOutcome.DefaultSkillId;
            memberSeed.GrowthTrackId = memberDefinition != null ? memberDefinition.GrowthTrackId : string.Empty;
            memberSeed.JobId = memberDefinition != null ? memberDefinition.JobId : string.Empty;
            memberSeed.EquipmentLoadoutId = memberDefinition != null ? memberDefinition.EquipmentLoadoutId : string.Empty;
            memberSeed.SkillLoadoutId = memberDefinition != null ? memberDefinition.SkillLoadoutId : string.Empty;
            memberSeed.Survived = memberOutcome.Survived;
            memberSeed.KnockedOut = memberOutcome.KnockedOut;
            memberSeed.CurrentHp = Mathf.Max(0, memberOutcome.CurrentHp);
            memberSeed.MaxHp = Mathf.Max(1, memberOutcome.MaxHp);
            memberSeed.Combat = new PrototypeRpgCombatContributionSeed
            {
                DamageDealt = GetRunMemberContributionValue(_runMemberDamageDealt, i),
                DamageTaken = GetRunMemberContributionValue(_runMemberDamageTaken, i),
                HealingDone = GetRunMemberContributionValue(_runMemberHealingDone, i),
                ActionCount = GetRunMemberContributionValue(_runMemberActionCount, i),
                KillCount = GetRunMemberContributionValue(_runMemberKillCount, i)
            };
            memberSeeds[i] = memberSeed;
        }

        snapshot.Members = memberSeeds;
        return snapshot;
    }
    private PrototypeRpgCombatContributionSnapshot CopyRpgCombatContributionSnapshot(PrototypeRpgCombatContributionSnapshot source)
    {
        PrototypeRpgCombatContributionSnapshot copy = new PrototypeRpgCombatContributionSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.TotalTurnsTaken = Mathf.Max(0, source.TotalTurnsTaken);
        copy.SurvivingMemberCount = Mathf.Max(0, source.SurvivingMemberCount);
        copy.KnockedOutMemberCount = Mathf.Max(0, source.KnockedOutMemberCount);
        copy.TotalDamageDealt = Mathf.Max(0, source.TotalDamageDealt);
        copy.TotalDamageTaken = Mathf.Max(0, source.TotalDamageTaken);
        copy.TotalHealingDone = Mathf.Max(0, source.TotalHealingDone);

        PrototypeRpgMemberContributionSnapshot[] sourceMembers = source.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        if (sourceMembers.Length <= 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        }
        else
        {
            PrototypeRpgMemberContributionSnapshot[] memberCopies = new PrototypeRpgMemberContributionSnapshot[sourceMembers.Length];
            for (int i = 0; i < sourceMembers.Length; i++)
            {
                memberCopies[i] = CopyRpgMemberContributionSnapshot(sourceMembers[i]);
            }

            copy.Members = memberCopies;
        }

        PrototypeBattleEventRecord[] sourceEvents = source.RecentEvents ?? System.Array.Empty<PrototypeBattleEventRecord>();
        if (sourceEvents.Length <= 0)
        {
            copy.RecentEvents = System.Array.Empty<PrototypeBattleEventRecord>();
        }
        else
        {
            PrototypeBattleEventRecord[] eventCopies = new PrototypeBattleEventRecord[sourceEvents.Length];
            for (int i = 0; i < sourceEvents.Length; i++)
            {
                eventCopies[i] = CopyBattleEventRecord(sourceEvents[i]);
            }

            copy.RecentEvents = eventCopies;
        }

        return copy;
    }

    private PrototypeRpgMemberContributionSnapshot CopyRpgMemberContributionSnapshot(PrototypeRpgMemberContributionSnapshot source)
    {
        PrototypeRpgMemberContributionSnapshot copy = new PrototypeRpgMemberContributionSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.RoleLabel = string.IsNullOrEmpty(source.RoleLabel) ? string.Empty : source.RoleLabel;
        copy.DefaultSkillId = string.IsNullOrEmpty(source.DefaultSkillId) ? string.Empty : source.DefaultSkillId;
        copy.DamageDealt = Mathf.Max(0, source.DamageDealt);
        copy.DamageTaken = Mathf.Max(0, source.DamageTaken);
        copy.HealingDone = Mathf.Max(0, source.HealingDone);
        copy.ActionCount = Mathf.Max(0, source.ActionCount);
        copy.KillCount = Mathf.Max(0, source.KillCount);
        copy.KnockedOut = source.KnockedOut;
        copy.Survived = source.Survived;
        copy.EliteVictor = source.EliteVictor;
        copy.ContributionSummaryText = string.IsNullOrEmpty(source.ContributionSummaryText) ? string.Empty : source.ContributionSummaryText;
        return copy;
    }

    private PrototypeRpgProgressionPreviewSnapshot CopyRpgProgressionPreviewSnapshot(PrototypeRpgProgressionPreviewSnapshot source)
    {
        PrototypeRpgProgressionPreviewSnapshot copy = new PrototypeRpgProgressionPreviewSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.DungeonDangerLabel = string.IsNullOrEmpty(source.DungeonDangerLabel) ? string.Empty : source.DungeonDangerLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.RouteRiskLabel = string.IsNullOrEmpty(source.RouteRiskLabel) ? string.Empty : source.RouteRiskLabel;
        copy.EliteDefeated = source.EliteDefeated;
        copy.TotalLootGained = Mathf.Max(0, source.TotalLootGained);
        copy.HasAppliedProgress = source.HasAppliedProgress;
        copy.AppliedProgressSummaryText = string.IsNullOrEmpty(source.AppliedProgressSummaryText) ? string.Empty : source.AppliedProgressSummaryText;
        copy.AppliedLastRunIdentity = string.IsNullOrEmpty(source.AppliedLastRunIdentity) ? string.Empty : source.AppliedLastRunIdentity;
        copy.ProgressionPreviewText = string.IsNullOrEmpty(source.ProgressionPreviewText) ? string.Empty : source.ProgressionPreviewText;
        copy.UnlockPreviewSummaryText = string.IsNullOrEmpty(source.UnlockPreviewSummaryText) ? string.Empty : source.UnlockPreviewSummaryText;
        copy.RewardCarryoverHintText = string.IsNullOrEmpty(source.RewardCarryoverHintText) ? string.Empty : source.RewardCarryoverHintText;
        copy.GrowthCandidateSummaryText = string.IsNullOrEmpty(source.GrowthCandidateSummaryText) ? string.Empty : source.GrowthCandidateSummaryText;
        copy.UpgradeCandidateSummaryText = string.IsNullOrEmpty(source.UpgradeCandidateSummaryText) ? string.Empty : source.UpgradeCandidateSummaryText;
        copy.UpgradeOfferSummaryText = string.IsNullOrEmpty(source.UpgradeOfferSummaryText) ? string.Empty : source.UpgradeOfferSummaryText;
        copy.ApplyReadySummaryText = string.IsNullOrEmpty(source.ApplyReadySummaryText) ? string.Empty : source.ApplyReadySummaryText;
        copy.RewardHintTags = source.RewardHintTags != null && source.RewardHintTags.Length > 0 ? (string[])source.RewardHintTags.Clone() : System.Array.Empty<string>();
        copy.GrowthHintTags = source.GrowthHintTags != null && source.GrowthHintTags.Length > 0 ? (string[])source.GrowthHintTags.Clone() : System.Array.Empty<string>();

        PrototypeRpgUnlockPreviewSnapshot[] sourceUnlockPreviews = source.UnlockPreviews ?? System.Array.Empty<PrototypeRpgUnlockPreviewSnapshot>();
        if (sourceUnlockPreviews.Length <= 0)
        {
            copy.UnlockPreviews = System.Array.Empty<PrototypeRpgUnlockPreviewSnapshot>();
        }
        else
        {
            PrototypeRpgUnlockPreviewSnapshot[] unlockCopies = new PrototypeRpgUnlockPreviewSnapshot[sourceUnlockPreviews.Length];
            for (int i = 0; i < sourceUnlockPreviews.Length; i++)
            {
                unlockCopies[i] = CopyRpgUnlockPreviewSnapshot(sourceUnlockPreviews[i]);
            }

            copy.UnlockPreviews = unlockCopies;
        }

        PrototypeRpgMemberProgressPreview[] sourceMembers = source.Members ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        if (sourceMembers.Length <= 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        }
        else
        {
            PrototypeRpgMemberProgressPreview[] memberCopies = new PrototypeRpgMemberProgressPreview[sourceMembers.Length];
            for (int i = 0; i < sourceMembers.Length; i++)
            {
                memberCopies[i] = CopyRpgMemberProgressPreview(sourceMembers[i]);
            }

            copy.Members = memberCopies;
        }

        return copy;
    }

    private PrototypeRpgUnlockPreviewSnapshot CopyRpgUnlockPreviewSnapshot(PrototypeRpgUnlockPreviewSnapshot source)
    {
        PrototypeRpgUnlockPreviewSnapshot copy = new PrototypeRpgUnlockPreviewSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.SourceRunIdentity = string.IsNullOrEmpty(source.SourceRunIdentity) ? string.Empty : source.SourceRunIdentity;
        copy.SourceEncounterId = string.IsNullOrEmpty(source.SourceEncounterId) ? string.Empty : source.SourceEncounterId;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.EliteId = string.IsNullOrEmpty(source.EliteId) ? string.Empty : source.EliteId;
        copy.UnlockCategoryKey = string.IsNullOrEmpty(source.UnlockCategoryKey) ? string.Empty : source.UnlockCategoryKey;
        copy.UnlockTargetKey = string.IsNullOrEmpty(source.UnlockTargetKey) ? string.Empty : source.UnlockTargetKey;
        copy.UnlockReasonText = string.IsNullOrEmpty(source.UnlockReasonText) ? string.Empty : source.UnlockReasonText;
        copy.ProgressionDependencyHint = string.IsNullOrEmpty(source.ProgressionDependencyHint) ? string.Empty : source.ProgressionDependencyHint;
        copy.DisplayLabel = string.IsNullOrEmpty(source.DisplayLabel) ? string.Empty : source.DisplayLabel;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        copy.IsPreviewOnly = source.IsPreviewOnly;
        return copy;
    }

    private PrototypeRpgGrowthPathCandidate CopyRpgGrowthPathCandidate(PrototypeRpgGrowthPathCandidate source)
    {
        PrototypeRpgGrowthPathCandidate copy = new PrototypeRpgGrowthPathCandidate();
        if (source == null)
        {
            return copy;
        }

        copy.CandidateKey = string.IsNullOrEmpty(source.CandidateKey) ? string.Empty : source.CandidateKey;
        copy.CandidateTypeKey = string.IsNullOrEmpty(source.CandidateTypeKey) ? string.Empty : source.CandidateTypeKey;
        copy.SourceHookId = string.IsNullOrEmpty(source.SourceHookId) ? string.Empty : source.SourceHookId;
        copy.CandidateTargetId = string.IsNullOrEmpty(source.CandidateTargetId) ? string.Empty : source.CandidateTargetId;
        copy.PreviewLabel = string.IsNullOrEmpty(source.PreviewLabel) ? string.Empty : source.PreviewLabel;
        copy.PreviewText = string.IsNullOrEmpty(source.PreviewText) ? string.Empty : source.PreviewText;
        copy.RecommendedBecauseText = string.IsNullOrEmpty(source.RecommendedBecauseText) ? string.Empty : source.RecommendedBecauseText;
        copy.Priority = Mathf.Max(0, source.Priority);
        copy.AvailableLater = source.AvailableLater;
        copy.BlockedReasonHint = string.IsNullOrEmpty(source.BlockedReasonHint) ? string.Empty : source.BlockedReasonHint;
        return copy;
    }

    private PrototypeRpgUpgradeCandidate CopyRpgUpgradeCandidate(PrototypeRpgUpgradeCandidate source)
    {
        PrototypeRpgUpgradeCandidate copy = new PrototypeRpgUpgradeCandidate();
        if (source == null)
        {
            return copy;
        }

        copy.CandidateKey = string.IsNullOrEmpty(source.CandidateKey) ? string.Empty : source.CandidateKey;
        copy.CandidateTypeKey = string.IsNullOrEmpty(source.CandidateTypeKey) ? string.Empty : source.CandidateTypeKey;
        copy.SourceHookId = string.IsNullOrEmpty(source.SourceHookId) ? string.Empty : source.SourceHookId;
        copy.CandidateTargetId = string.IsNullOrEmpty(source.CandidateTargetId) ? string.Empty : source.CandidateTargetId;
        copy.PreviewLabel = string.IsNullOrEmpty(source.PreviewLabel) ? string.Empty : source.PreviewLabel;
        copy.PreviewText = string.IsNullOrEmpty(source.PreviewText) ? string.Empty : source.PreviewText;
        copy.RecommendedBecauseText = string.IsNullOrEmpty(source.RecommendedBecauseText) ? string.Empty : source.RecommendedBecauseText;
        copy.Priority = Mathf.Max(0, source.Priority);
        copy.AvailableLater = source.AvailableLater;
        copy.BlockedReasonHint = string.IsNullOrEmpty(source.BlockedReasonHint) ? string.Empty : source.BlockedReasonHint;
        return copy;
    }

    private PrototypeRpgGrowthPathCandidate[] CopyRpgGrowthPathCandidates(PrototypeRpgGrowthPathCandidate[] source)
    {
        PrototypeRpgGrowthPathCandidate[] safeSource = source ?? System.Array.Empty<PrototypeRpgGrowthPathCandidate>();
        if (safeSource.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgGrowthPathCandidate>();
        }

        PrototypeRpgGrowthPathCandidate[] copies = new PrototypeRpgGrowthPathCandidate[safeSource.Length];
        for (int i = 0; i < safeSource.Length; i++)
        {
            copies[i] = CopyRpgGrowthPathCandidate(safeSource[i]);
        }

        return copies;
    }

    private PrototypeRpgUpgradeCandidate[] CopyRpgUpgradeCandidates(PrototypeRpgUpgradeCandidate[] source)
    {
        PrototypeRpgUpgradeCandidate[] safeSource = source ?? System.Array.Empty<PrototypeRpgUpgradeCandidate>();
        if (safeSource.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgUpgradeCandidate>();
        }

        PrototypeRpgUpgradeCandidate[] copies = new PrototypeRpgUpgradeCandidate[safeSource.Length];
        for (int i = 0; i < safeSource.Length; i++)
        {
            copies[i] = CopyRpgUpgradeCandidate(safeSource[i]);
        }

        return copies;
    }

    private PrototypeRpgMemberProgressPreview CopyRpgMemberProgressPreview(PrototypeRpgMemberProgressPreview source)
    {
        PrototypeRpgMemberProgressPreview copy = new PrototypeRpgMemberProgressPreview();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.RoleLabel = string.IsNullOrEmpty(source.RoleLabel) ? string.Empty : source.RoleLabel;
        copy.GrowthTrackId = string.IsNullOrEmpty(source.GrowthTrackId) ? string.Empty : source.GrowthTrackId;
        copy.JobId = string.IsNullOrEmpty(source.JobId) ? string.Empty : source.JobId;
        copy.EquipmentLoadoutId = string.IsNullOrEmpty(source.EquipmentLoadoutId) ? string.Empty : source.EquipmentLoadoutId;
        copy.SkillLoadoutId = string.IsNullOrEmpty(source.SkillLoadoutId) ? string.Empty : source.SkillLoadoutId;
        copy.Survived = source.Survived;
        copy.Contribution = CopyRpgMemberContributionSnapshot(source.Contribution);
        copy.HasAppliedProgress = source.HasAppliedProgress;
        copy.AppliedProgressSummaryText = string.IsNullOrEmpty(source.AppliedProgressSummaryText) ? string.Empty : source.AppliedProgressSummaryText;
        copy.AppliedRoleLabel = string.IsNullOrEmpty(source.AppliedRoleLabel) ? string.Empty : source.AppliedRoleLabel;
        copy.AppliedDefaultSkillId = string.IsNullOrEmpty(source.AppliedDefaultSkillId) ? string.Empty : source.AppliedDefaultSkillId;
        copy.SuggestedGrowthHintTags = source.SuggestedGrowthHintTags != null && source.SuggestedGrowthHintTags.Length > 0 ? (string[])source.SuggestedGrowthHintTags.Clone() : System.Array.Empty<string>();
        copy.SuggestedRewardHintTags = source.SuggestedRewardHintTags != null && source.SuggestedRewardHintTags.Length > 0 ? (string[])source.SuggestedRewardHintTags.Clone() : System.Array.Empty<string>();
        copy.NotableOutcomeKey = string.IsNullOrEmpty(source.NotableOutcomeKey) ? string.Empty : source.NotableOutcomeKey;
        copy.PreviewSummaryText = string.IsNullOrEmpty(source.PreviewSummaryText) ? string.Empty : source.PreviewSummaryText;
        copy.GrowthDirectionLabel = string.IsNullOrEmpty(source.GrowthDirectionLabel) ? string.Empty : source.GrowthDirectionLabel;
        copy.RewardCarryoverHintText = string.IsNullOrEmpty(source.RewardCarryoverHintText) ? string.Empty : source.RewardCarryoverHintText;
        copy.UpgradeCandidateSummaryText = string.IsNullOrEmpty(source.UpgradeCandidateSummaryText) ? string.Empty : source.UpgradeCandidateSummaryText;
        copy.UpgradeOfferSummaryText = string.IsNullOrEmpty(source.UpgradeOfferSummaryText) ? string.Empty : source.UpgradeOfferSummaryText;
        copy.ApplyReadySummaryText = string.IsNullOrEmpty(source.ApplyReadySummaryText) ? string.Empty : source.ApplyReadySummaryText;
        copy.NextGrowthTrackHint = string.IsNullOrEmpty(source.NextGrowthTrackHint) ? string.Empty : source.NextGrowthTrackHint;
        copy.NextJobHint = string.IsNullOrEmpty(source.NextJobHint) ? string.Empty : source.NextJobHint;
        copy.NextSkillLoadoutHint = string.IsNullOrEmpty(source.NextSkillLoadoutHint) ? string.Empty : source.NextSkillLoadoutHint;
        copy.NextEquipmentLoadoutHint = string.IsNullOrEmpty(source.NextEquipmentLoadoutHint) ? string.Empty : source.NextEquipmentLoadoutHint;

        PrototypeRpgGrowthPathCandidate[] growthCandidates = source.GrowthPathCandidates ?? System.Array.Empty<PrototypeRpgGrowthPathCandidate>();
        if (growthCandidates.Length <= 0)
        {
            copy.GrowthPathCandidates = System.Array.Empty<PrototypeRpgGrowthPathCandidate>();
        }
        else
        {
            PrototypeRpgGrowthPathCandidate[] candidateCopies = new PrototypeRpgGrowthPathCandidate[growthCandidates.Length];
            for (int i = 0; i < growthCandidates.Length; i++)
            {
                candidateCopies[i] = CopyRpgGrowthPathCandidate(growthCandidates[i]);
            }

            copy.GrowthPathCandidates = candidateCopies;
        }

        PrototypeRpgUpgradeCandidate[] upgradeCandidates = source.UpgradeCandidates ?? System.Array.Empty<PrototypeRpgUpgradeCandidate>();
        if (upgradeCandidates.Length <= 0)
        {
            copy.UpgradeCandidates = System.Array.Empty<PrototypeRpgUpgradeCandidate>();
        }
        else
        {
            PrototypeRpgUpgradeCandidate[] candidateCopies = new PrototypeRpgUpgradeCandidate[upgradeCandidates.Length];
            for (int i = 0; i < upgradeCandidates.Length; i++)
            {
                candidateCopies[i] = CopyRpgUpgradeCandidate(upgradeCandidates[i]);
            }

            copy.UpgradeCandidates = candidateCopies;
        }

        return copy;
    }

    private bool HasRpgRunResultSnapshotData(PrototypeRpgRunResultSnapshot snapshot)
    {
        return snapshot != null &&
               !string.IsNullOrEmpty(snapshot.ResultStateKey) &&
               snapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None;
    }

    private bool HasRpgCombatContributionSnapshotData(PrototypeRpgCombatContributionSnapshot snapshot)
    {
        return snapshot != null &&
               (!string.IsNullOrEmpty(snapshot.ResultStateKey) ||
                (snapshot.Members != null && snapshot.Members.Length > 0) ||
                snapshot.TotalDamageDealt > 0 ||
                snapshot.TotalDamageTaken > 0 ||
                snapshot.TotalHealingDone > 0);
    }

    private bool HasRpgProgressionPreviewSnapshotData(PrototypeRpgProgressionPreviewSnapshot snapshot)
    {
        return snapshot != null &&
               (!string.IsNullOrEmpty(snapshot.ResultStateKey) ||
                !string.IsNullOrEmpty(snapshot.ProgressionPreviewText) ||
                !string.IsNullOrEmpty(snapshot.UnlockPreviewSummaryText) ||
                snapshot.HasAppliedProgress ||
                !string.IsNullOrEmpty(snapshot.AppliedProgressSummaryText) ||
                !string.IsNullOrEmpty(snapshot.RewardCarryoverHintText) ||
                !string.IsNullOrEmpty(snapshot.GrowthCandidateSummaryText) ||
                !string.IsNullOrEmpty(snapshot.UpgradeCandidateSummaryText) ||
                (snapshot.UnlockPreviews != null && snapshot.UnlockPreviews.Length > 0) ||
                (snapshot.Members != null && snapshot.Members.Length > 0) ||
                (snapshot.RewardHintTags != null && snapshot.RewardHintTags.Length > 0) ||
                (snapshot.GrowthHintTags != null && snapshot.GrowthHintTags.Length > 0));
    }

    private PrototypeRpgPresentationPhase BuildRpgPresentationPhase()
    {
        if (_dungeonRunState == DungeonRunState.Battle)
        {
            return PrototypeRpgPresentationPhase.Battle;
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            return PrototypeRpgPresentationPhase.ResultPanel;
        }

        if (HasRpgRunResultSnapshotData(_latestRpgRunResultSnapshot))
        {
            return PrototypeRpgPresentationPhase.PostRunSummary;
        }

        return PrototypeRpgPresentationPhase.None;
    }

    private string GetRpgPresentationPhaseKey(PrototypeRpgPresentationPhase phase)
    {
        switch (phase)
        {
            case PrototypeRpgPresentationPhase.Battle:
                return "battle";
            case PrototypeRpgPresentationPhase.ResultPanel:
                return "result_panel";
            case PrototypeRpgPresentationPhase.PostRunSummary:
                return "post_run_summary";
            default:
                return "none";
        }
    }

    private string BuildRpgPresentationRunStateKey(PrototypeRpgPresentationPhase phase, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (HasRpgRunResultSnapshotData(runResultSnapshot))
        {
            return runResultSnapshot.ResultStateKey;
        }

        if (phase == PrototypeRpgPresentationPhase.Battle)
        {
            return GetBattleStateKey();
        }

        if (_runResultState == RunResultState.Playing)
        {
            return "playing";
        }

        return "none";
    }

    private string BuildRpgResultHeadline(string resultStateKey)
    {
        switch (resultStateKey)
        {
            case PrototypeBattleOutcomeKeys.RunClear:
                return "Run Clear";
            case PrototypeBattleOutcomeKeys.RunDefeat:
                return "Run Defeat";
            case PrototypeBattleOutcomeKeys.RunRetreat:
                return "Run Retreat";
            default:
                return "Post-Run Summary";
        }
    }

    private string BuildRpgPostRunSummarySubheadline(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        List<string> parts = new List<string>();
        if (runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.DungeonLabel))
        {
            parts.Add(runResultSnapshot.DungeonLabel);
        }

        if (runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.RouteLabel))
        {
            parts.Add(runResultSnapshot.RouteLabel);
        }

        if (runResultSnapshot != null && runResultSnapshot.TotalTurnsTaken > 0)
        {
            parts.Add("Turns " + runResultSnapshot.TotalTurnsTaken);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "Run summary";
    }

    private string[] BuildRpgPostRunSummaryHighlightKeys(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        List<string> keys = new List<string>();
        if (runResultSnapshot == null)
        {
            return System.Array.Empty<string>();
        }

        if (!string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) && runResultSnapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None)
        {
            AddProgressionTag(keys, runResultSnapshot.ResultStateKey);
        }

        if (runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(keys, "elite_defeated");
        }

        if (runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.EliteBonusRewardEarned)
        {
            AddProgressionTag(keys, "elite_bonus");
        }

        if (runResultSnapshot.EncounterOutcome != null && !string.IsNullOrEmpty(runResultSnapshot.EncounterOutcome.SelectedEventChoice))
        {
            AddProgressionTag(keys, "event_choice");
        }

        if (runResultSnapshot.EncounterOutcome != null && !string.IsNullOrEmpty(runResultSnapshot.EncounterOutcome.SelectedPreEliteChoice))
        {
            AddProgressionTag(keys, "pre_elite_choice");
        }

        return keys.Count > 0 ? keys.ToArray() : System.Array.Empty<string>();
    }

    private PrototypeRpgMemberContributionSnapshot FindRpgMemberContributionSnapshot(PrototypeRpgCombatContributionSnapshot snapshot, string memberId)
    {
        if (snapshot == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        PrototypeRpgMemberContributionSnapshot[] members = snapshot.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberContributionSnapshot member = members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private PrototypeRpgMemberProgressPreview FindRpgMemberProgressPreview(PrototypeRpgProgressionPreviewSnapshot snapshot, string memberId)
    {
        if (snapshot == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        PrototypeRpgMemberProgressPreview[] members = snapshot.Members ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberProgressPreview member = members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private PrototypeRpgMemberProgressionSeed FindRpgMemberProgressionSeed(PrototypeRpgProgressionSeedSnapshot snapshot, string memberId)
    {
        if (snapshot == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        PrototypeRpgMemberProgressionSeed[] members = snapshot.Members ?? System.Array.Empty<PrototypeRpgMemberProgressionSeed>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberProgressionSeed member = members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private string BuildRpgTagListText(string[] tags)
    {
        string[] safeTags = tags ?? System.Array.Empty<string>();
        return safeTags.Length > 0 ? string.Join(", ", safeTags) : "None";
    }

    private string BuildRpgPartyOutcomeSummaryText(PrototypeRpgPartyOutcomeSnapshot partyOutcome)
    {
        if (partyOutcome == null)
        {
            return "None";
        }

        int totalMembers = partyOutcome.Members != null ? partyOutcome.Members.Length : 0;
        List<string> parts = new List<string>();
        if (totalMembers > 0)
        {
            parts.Add(partyOutcome.SurvivingMemberCount + " / " + totalMembers + " survived");
        }

        if (partyOutcome.KnockedOutMemberCount > 0)
        {
            parts.Add("KO " + partyOutcome.KnockedOutMemberCount);
        }

        if (!string.IsNullOrEmpty(partyOutcome.PartyConditionText))
        {
            parts.Add(partyOutcome.PartyConditionText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgContributionSummaryText(PrototypeRpgCombatContributionSnapshot contributionSnapshot)
    {
        if (!HasRpgCombatContributionSnapshotData(contributionSnapshot))
        {
            return "None";
        }

        int totalActions = 0;
        int totalKills = 0;
        PrototypeRpgMemberContributionSnapshot[] members = contributionSnapshot.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberContributionSnapshot member = members[i];
            if (member == null)
            {
                continue;
            }

            totalActions += Mathf.Max(0, member.ActionCount);
            totalKills += Mathf.Max(0, member.KillCount);
        }

        List<string> parts = new List<string>();
        parts.Add("D " + Mathf.Max(0, contributionSnapshot.TotalDamageDealt));
        parts.Add("Taken " + Mathf.Max(0, contributionSnapshot.TotalDamageTaken));
        parts.Add("Heal " + Mathf.Max(0, contributionSnapshot.TotalHealingDone));
        parts.Add("A " + Mathf.Max(0, totalActions));
        parts.Add("K " + Mathf.Max(0, totalKills));
        return string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgProgressionPreviewSummaryText(PrototypeRpgProgressionPreviewSnapshot previewSnapshot)
    {
        if (!HasRpgProgressionPreviewSnapshotData(previewSnapshot))
        {
            return "None";
        }

        if (!string.IsNullOrEmpty(previewSnapshot.ProgressionPreviewText))
        {
            return previewSnapshot.ProgressionPreviewText;
        }

        List<string> parts = new List<string>();
        if (previewSnapshot.TotalLootGained > 0)
        {
            parts.Add(BuildLootAmountText(previewSnapshot.TotalLootGained));
        }

        if (!string.IsNullOrEmpty(previewSnapshot.GrowthCandidateSummaryText) && previewSnapshot.GrowthCandidateSummaryText != "None")
        {
            parts.Add(previewSnapshot.GrowthCandidateSummaryText);
        }

        if (!string.IsNullOrEmpty(previewSnapshot.UpgradeCandidateSummaryText) && previewSnapshot.UpgradeCandidateSummaryText != "None")
        {
            parts.Add(previewSnapshot.UpgradeCandidateSummaryText);
        }

        if (previewSnapshot.RewardHintTags != null && previewSnapshot.RewardHintTags.Length > 0)
        {
            parts.Add("Reward " + BuildRpgTagListText(previewSnapshot.RewardHintTags));
        }

        if (previewSnapshot.GrowthHintTags != null && previewSnapshot.GrowthHintTags.Length > 0)
        {
            parts.Add("Growth " + BuildRpgTagListText(previewSnapshot.GrowthHintTags));
        }

        if (previewSnapshot.EliteDefeated)
        {
            parts.Add("elite_defeated");
        }

        if (!string.IsNullOrEmpty(previewSnapshot.UnlockPreviewSummaryText) && previewSnapshot.UnlockPreviewSummaryText != "None")
        {
            parts.Add(previewSnapshot.UnlockPreviewSummaryText);
        }

        if (!string.IsNullOrEmpty(previewSnapshot.RewardCarryoverHintText) && previewSnapshot.RewardCarryoverHintText != "None")
        {
            parts.Add(previewSnapshot.RewardCarryoverHintText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgLootBreakdownText(PrototypeRpgLootOutcomeSnapshot lootOutcome)
    {
        if (lootOutcome == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        if (lootOutcome.BattleLootGained > 0)
        {
            parts.Add("Battle " + BuildLootAmountText(lootOutcome.BattleLootGained));
        }

        if (lootOutcome.ChestLootGained > 0)
        {
            parts.Add("Chest " + BuildLootAmountText(lootOutcome.ChestLootGained));
        }

        if (lootOutcome.EventLootGained > 0)
        {
            parts.Add("Event " + BuildLootAmountText(lootOutcome.EventLootGained));
        }

        if (lootOutcome.EliteRewardAmount > 0)
        {
            parts.Add("Elite " + BuildLootAmountText(lootOutcome.EliteRewardAmount));
        }

        if (lootOutcome.EliteBonusRewardAmount > 0)
        {
            parts.Add("Bonus " + BuildLootAmountText(lootOutcome.EliteBonusRewardAmount));
        }

        if (parts.Count <= 0 && lootOutcome.TotalLootGained > 0)
        {
            parts.Add(BuildLootAmountText(lootOutcome.TotalLootGained));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgEliteOutcomeSummaryText(PrototypeRpgEliteOutcomeSnapshot eliteOutcome)
    {
        if (eliteOutcome == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        if (eliteOutcome.IsEliteDefeated)
        {
            string eliteLabel = string.IsNullOrEmpty(eliteOutcome.EliteName) ? "Elite defeated" : eliteOutcome.EliteName + " defeated";
            parts.Add(eliteLabel);
        }

        if (!string.IsNullOrEmpty(eliteOutcome.EliteTypeLabel))
        {
            parts.Add(eliteOutcome.EliteTypeLabel);
        }

        if (eliteOutcome.EliteRewardAmount > 0)
        {
            string rewardLabel = string.IsNullOrEmpty(eliteOutcome.EliteRewardLabel) ? "Elite Reward" : eliteOutcome.EliteRewardLabel;
            parts.Add(rewardLabel + " " + BuildLootAmountText(eliteOutcome.EliteRewardAmount));
        }

        if (eliteOutcome.EliteBonusRewardEarned && eliteOutcome.EliteBonusRewardAmount > 0)
        {
            parts.Add("Bonus " + BuildLootAmountText(eliteOutcome.EliteBonusRewardAmount));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgEventSummaryText(PrototypeRpgEncounterOutcomeSnapshot encounterOutcome)
    {
        if (encounterOutcome == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(encounterOutcome.ClearedEncounterSummary))
        {
            parts.Add(encounterOutcome.ClearedEncounterSummary);
        }
        else if (encounterOutcome.ClearedEncounterCount > 0)
        {
            parts.Add("Cleared " + encounterOutcome.ClearedEncounterCount);
        }

        if (encounterOutcome.OpenedChestCount > 0)
        {
            parts.Add("Chests " + encounterOutcome.OpenedChestCount);
        }

        if (!string.IsNullOrEmpty(encounterOutcome.SelectedEventChoice))
        {
            parts.Add(encounterOutcome.SelectedEventChoice);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgPreEliteSummaryText(PrototypeRpgEncounterOutcomeSnapshot encounterOutcome)
    {
        if (encounterOutcome == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(encounterOutcome.SelectedPreEliteChoice))
        {
            parts.Add(encounterOutcome.SelectedPreEliteChoice);
        }

        if (encounterOutcome.PreEliteHealAmount > 0)
        {
            parts.Add("Heal " + BuildRawHpAmountText(encounterOutcome.PreEliteHealAmount));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgMemberContributionSummaryText(PrototypeRpgMemberContributionSnapshot contribution)
    {
        if (contribution == null)
        {
            return "D 0 | T 0 | H 0 | A 0 | K 0";
        }

        if (!string.IsNullOrEmpty(contribution.ContributionSummaryText))
        {
            return contribution.ContributionSummaryText;
        }

        return "D " + Mathf.Max(0, contribution.DamageDealt) +
               " | T " + Mathf.Max(0, contribution.DamageTaken) +
               " | H " + Mathf.Max(0, contribution.HealingDone) +
               " | A " + Mathf.Max(0, contribution.ActionCount) +
               " | K " + Mathf.Max(0, contribution.KillCount);
    }

    private string BuildRpgMemberProgressPreviewSummaryText(PrototypeRpgMemberProgressPreview preview)
    {
        if (preview == null)
        {
            return "None";
        }

        if (!string.IsNullOrEmpty(preview.PreviewSummaryText))
        {
            return preview.PreviewSummaryText;
        }

        return BuildMemberProgressionPreviewText(preview);
    }

    private PrototypeRpgPostRunMemberCardData[] BuildRpgPostRunMemberCards(
        PrototypeRpgPartyOutcomeSnapshot partyOutcome,
        PrototypeRpgCombatContributionSnapshot contributionSnapshot,
        PrototypeRpgProgressionPreviewSnapshot previewSnapshot)
    {
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome != null && partyOutcome.Members != null
            ? partyOutcome.Members
            : System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
        if (members.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgPostRunMemberCardData>();
        }

        PrototypeRpgPostRunUpgradeOfferSurface offerSurface = BuildRpgPostRunUpgradeOfferSurface();
        PrototypeRpgUpgradeOfferCandidate[] offerCandidates = offerSurface != null && offerSurface.Offers != null
            ? offerSurface.Offers
            : System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        PrototypeRpgApplyReadyUpgradeChoice[] applyReadyChoices = offerSurface != null && offerSurface.ApplyReadyChoices != null
            ? offerSurface.ApplyReadyChoices
            : System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
        PrototypeRpgAppliedPartyProgressState appliedProgressState = GetLatestAppliedPartyProgressStateInternal();

        PrototypeRpgPostRunMemberCardData[] cards = new PrototypeRpgPostRunMemberCardData[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberOutcome = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            PrototypeRpgMemberContributionSnapshot contribution = FindRpgMemberContributionSnapshot(contributionSnapshot, memberOutcome.MemberId);
            PrototypeRpgMemberProgressPreview preview = FindRpgMemberProgressPreview(previewSnapshot, memberOutcome.MemberId);
            PrototypeRpgPostRunMemberCardData card = new PrototypeRpgPostRunMemberCardData();
            card.MemberId = string.IsNullOrEmpty(memberOutcome.MemberId) ? string.Empty : memberOutcome.MemberId;
            card.DisplayName = string.IsNullOrEmpty(memberOutcome.DisplayName) ? "Member" : memberOutcome.DisplayName;
            card.RoleLabel = string.IsNullOrEmpty(memberOutcome.RoleLabel) ? "Adventurer" : memberOutcome.RoleLabel;
            card.CurrentHp = Mathf.Max(0, memberOutcome.CurrentHp);
            card.MaxHp = Mathf.Max(1, memberOutcome.MaxHp);
            card.HpText = card.CurrentHp + " / " + card.MaxHp;
            card.Survived = memberOutcome.Survived;
            card.KnockedOut = memberOutcome.KnockedOut;
            card.OutcomeLabel = card.KnockedOut ? "KO" : card.Survived ? "Survived" : "Lost";
            card.DamageDealt = contribution != null ? Mathf.Max(0, contribution.DamageDealt) : 0;
            card.DamageTaken = contribution != null ? Mathf.Max(0, contribution.DamageTaken) : 0;
            card.HealingDone = contribution != null ? Mathf.Max(0, contribution.HealingDone) : 0;
            card.ActionCount = contribution != null ? Mathf.Max(0, contribution.ActionCount) : 0;
            card.KillCount = contribution != null ? Mathf.Max(0, contribution.KillCount) : 0;
            card.GrowthTrackId = preview != null && !string.IsNullOrEmpty(preview.GrowthTrackId) ? preview.GrowthTrackId : string.Empty;
            card.JobId = preview != null && !string.IsNullOrEmpty(preview.JobId) ? preview.JobId : string.Empty;
            card.EquipmentLoadoutId = preview != null && !string.IsNullOrEmpty(preview.EquipmentLoadoutId) ? preview.EquipmentLoadoutId : string.Empty;
            card.SkillLoadoutId = preview != null && !string.IsNullOrEmpty(preview.SkillLoadoutId) ? preview.SkillLoadoutId : string.Empty;
            card.ContributionSummaryText = BuildRpgMemberContributionSummaryText(contribution);
            card.ProgressionSummaryText = BuildRpgMemberProgressPreviewSummaryText(preview);
            card.UnlockSummaryText = BuildRpgMemberUnlockSummaryText(previewSnapshot, card.MemberId);
            card.GrowthDirectionLabel = preview != null && !string.IsNullOrEmpty(preview.GrowthDirectionLabel) ? preview.GrowthDirectionLabel : string.Empty;
            card.RewardCarryoverHintText = preview != null && !string.IsNullOrEmpty(preview.RewardCarryoverHintText) ? preview.RewardCarryoverHintText : string.Empty;
            card.UpgradeCandidateSummaryText = preview != null && !string.IsNullOrEmpty(preview.UpgradeCandidateSummaryText) ? preview.UpgradeCandidateSummaryText : string.Empty;
            card.UpgradeOfferSummaryText = preview != null && !string.IsNullOrEmpty(preview.UpgradeOfferSummaryText)
                ? preview.UpgradeOfferSummaryText
                : BuildRpgMemberUpgradeOfferSummaryText(card.MemberId, offerCandidates);
            card.ApplyReadySummaryText = preview != null && !string.IsNullOrEmpty(preview.ApplyReadySummaryText)
                ? preview.ApplyReadySummaryText
                : BuildRpgMemberApplyReadySummaryText(card.MemberId, applyReadyChoices);
            card.NextGrowthTrackHint = preview != null ? preview.NextGrowthTrackHint : string.Empty;
            card.NextJobHint = preview != null ? preview.NextJobHint : string.Empty;
            card.NextSkillLoadoutHint = preview != null ? preview.NextSkillLoadoutHint : string.Empty;
            card.NextEquipmentLoadoutHint = preview != null ? preview.NextEquipmentLoadoutHint : string.Empty;
            card.NotableOutcomeKey = preview != null && !string.IsNullOrEmpty(preview.NotableOutcomeKey) ? preview.NotableOutcomeKey : string.Empty;
            card.GrowthHintTags = preview != null && preview.SuggestedGrowthHintTags != null && preview.SuggestedGrowthHintTags.Length > 0
                ? (string[])preview.SuggestedGrowthHintTags.Clone()
                : System.Array.Empty<string>();
            card.RewardHintTags = preview != null && preview.SuggestedRewardHintTags != null && preview.SuggestedRewardHintTags.Length > 0
                ? (string[])preview.SuggestedRewardHintTags.Clone()
                : System.Array.Empty<string>();
            card.GrowthPathCandidates = preview != null ? CopyRpgGrowthPathCandidates(preview.GrowthPathCandidates) : System.Array.Empty<PrototypeRpgGrowthPathCandidate>();
            card.UpgradeCandidates = preview != null ? CopyRpgUpgradeCandidates(preview.UpgradeCandidates) : System.Array.Empty<PrototypeRpgUpgradeCandidate>();
            ApplyRpgAppliedProgressToPostRunMemberCard(card, appliedProgressState);
            cards[i] = card;
        }

        return cards;
    }

    private PrototypeRpgPostRunSummarySurfaceData BuildRpgPostRunSummarySurfaceData()
    {
        PrototypeRpgPostRunSummarySurfaceData surface = new PrototypeRpgPostRunSummarySurfaceData();
        PrototypeRpgRunResultSnapshot runResultSnapshot = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
        PrototypeRpgCombatContributionSnapshot contributionSnapshot = CopyRpgCombatContributionSnapshot(_latestRpgCombatContributionSnapshot);
        PrototypeRpgProgressionPreviewSnapshot previewSnapshot = CopyRpgProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot);
        PrototypeRpgPostRunUpgradeOfferSurface offerSurface = BuildRpgPostRunUpgradeOfferSurface();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = runResultSnapshot.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = runResultSnapshot.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = runResultSnapshot.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = runResultSnapshot.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();

        surface.HasResult = HasRpgRunResultSnapshotData(runResultSnapshot);
        surface.ResultStateKey = string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? string.Empty : runResultSnapshot.ResultStateKey;
        surface.Headline = surface.HasResult ? BuildRpgResultHeadline(surface.ResultStateKey) : "None";
        surface.Subheadline = surface.HasResult ? BuildRpgPostRunSummarySubheadline(runResultSnapshot) : string.Empty;
        surface.ResultSummaryText = string.IsNullOrEmpty(runResultSnapshot.ResultSummary) ? string.Empty : runResultSnapshot.ResultSummary;
        surface.DungeonLabel = string.IsNullOrEmpty(runResultSnapshot.DungeonLabel) ? string.Empty : runResultSnapshot.DungeonLabel;
        surface.RouteLabel = string.IsNullOrEmpty(runResultSnapshot.RouteLabel) ? string.Empty : runResultSnapshot.RouteLabel;
        surface.PartyOutcomeSummaryText = BuildRpgPartyOutcomeSummaryText(partyOutcome);
        surface.PartyConditionText = string.IsNullOrEmpty(_resultPartyConditionText) ? partyOutcome.PartyConditionText : _resultPartyConditionText;
        surface.PartyHpSummaryText = string.IsNullOrEmpty(_resultPartyHpSummaryText) ? partyOutcome.PartyHpSummaryText : _resultPartyHpSummaryText;
        surface.SurvivingMembersText = string.IsNullOrEmpty(_resultSurvivingMembersText) ? runResultSnapshot.SurvivingMembersSummary : _resultSurvivingMembersText;
        surface.ContributionSummaryText = BuildRpgContributionSummaryText(contributionSnapshot);
        surface.ProgressionPreviewSummaryText = BuildRpgProgressionPreviewSummaryText(previewSnapshot);
        surface.UnlockPreviewSummaryText = !string.IsNullOrEmpty(previewSnapshot.UnlockPreviewSummaryText) ? previewSnapshot.UnlockPreviewSummaryText : "None";
        surface.RewardCarryoverSummaryText = !string.IsNullOrEmpty(previewSnapshot.RewardCarryoverHintText)
            ? previewSnapshot.RewardCarryoverHintText
            : BuildRpgRewardCarryoverHintText(lootOutcome, surface.ResultStateKey);
        surface.GrowthCandidateSummaryText = !string.IsNullOrEmpty(previewSnapshot.GrowthCandidateSummaryText) ? previewSnapshot.GrowthCandidateSummaryText : "None";
        surface.UpgradeCandidateSummaryText = !string.IsNullOrEmpty(previewSnapshot.UpgradeCandidateSummaryText) ? previewSnapshot.UpgradeCandidateSummaryText : "None";
        surface.UpgradeOfferSummaryText = !string.IsNullOrEmpty(previewSnapshot.UpgradeOfferSummaryText)
            ? previewSnapshot.UpgradeOfferSummaryText
            : offerSurface != null && !string.IsNullOrEmpty(offerSurface.SummaryText) ? offerSurface.SummaryText : "None";
        surface.ApplyReadySummaryText = !string.IsNullOrEmpty(previewSnapshot.ApplyReadySummaryText)
            ? previewSnapshot.ApplyReadySummaryText
            : offerSurface != null && !string.IsNullOrEmpty(offerSurface.ApplyReadySummaryText) ? offerSurface.ApplyReadySummaryText : "None";
        surface.LootSummaryText = !string.IsNullOrEmpty(lootOutcome.FinalLootSummary)
            ? lootOutcome.FinalLootSummary
            : lootOutcome.TotalReturnedAmount > 0 ? BuildLootAmountText(lootOutcome.TotalReturnedAmount) : lootOutcome.TotalLootGained > 0 ? BuildLootAmountText(lootOutcome.TotalLootGained) : "None";
        surface.LootBreakdownText = BuildRpgLootBreakdownText(lootOutcome);
        surface.EliteOutcomeSummaryText = BuildRpgEliteOutcomeSummaryText(eliteOutcome);
        surface.EliteRewardIdentityText = string.IsNullOrEmpty(_resultEliteRewardLabel) ? eliteOutcome.EliteRewardLabel : _resultEliteRewardLabel;
        surface.EliteRewardAmountText = _resultEliteRewardAmount > 0 ? BuildLootAmountText(_resultEliteRewardAmount) : eliteOutcome.EliteRewardAmount > 0 ? BuildLootAmountText(eliteOutcome.EliteRewardAmount) : "None";
        surface.EventSummaryText = BuildRpgEventSummaryText(encounterOutcome);
        surface.EventChoiceText = string.IsNullOrEmpty(_resultEventChoiceText) ? encounterOutcome.SelectedEventChoice : _resultEventChoiceText;
        surface.PreEliteSummaryText = BuildRpgPreEliteSummaryText(encounterOutcome);
        surface.PreEliteChoiceText = string.IsNullOrEmpty(_resultPreEliteChoiceText) ? encounterOutcome.SelectedPreEliteChoice : _resultPreEliteChoiceText;
        surface.RoomPathSummaryText = string.IsNullOrEmpty(_resultRoomPathSummaryText) ? encounterOutcome.RoomPathSummary : _resultRoomPathSummaryText;
        surface.TurnsTakenText = Mathf.Max(_resultTurnsTaken, runResultSnapshot.TotalTurnsTaken).ToString();
        surface.MemberCards = BuildRpgPostRunMemberCards(partyOutcome, contributionSnapshot, previewSnapshot);
        surface.TopHighlightKeys = BuildRpgPostRunSummaryHighlightKeys(runResultSnapshot);
        return surface;
        ApplyRpgAppliedProgressToPostRunSummarySurface(surface, GetLatestAppliedPartyProgressStateInternal());
    }

    private PrototypeRpgRewardGrantEntryData[] BuildRpgRewardGrantEntries(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPendingRewardDeltaPack pack)
    {
        if (pack == null)
        {
            return System.Array.Empty<PrototypeRpgRewardGrantEntryData>();
        }

        List<PrototypeRpgRewardGrantEntryData> entries = new List<PrototypeRpgRewardGrantEntryData>();
        string rewardLabel = string.IsNullOrEmpty(pack.RewardLabel) ? "Elite Reward" : pack.RewardLabel;
        string resourceId = string.IsNullOrEmpty(pack.RewardResourceId) ? DungeonRewardResourceId : pack.RewardResourceId;
        bool hasResult = HasRpgRunResultSnapshotData(runResultSnapshot);

        if (pack.BaseRewardAmount > 0)
        {
            PrototypeRpgRewardGrantEntryData baseEntry = new PrototypeRpgRewardGrantEntryData();
            baseEntry.GrantKey = "elite_reward";
            baseEntry.DisplayLabel = rewardLabel;
            baseEntry.ResourceId = resourceId;
            baseEntry.Amount = Mathf.Max(0, pack.BaseRewardAmount);
            baseEntry.StateKey = hasResult ? "granted" : "available";
            baseEntry.SummaryText = baseEntry.DisplayLabel + " " + BuildLootAmountText(baseEntry.Amount);
            entries.Add(baseEntry);
        }

        if (pack.PendingBonusAmount > 0)
        {
            PrototypeRpgRewardGrantEntryData pendingEntry = new PrototypeRpgRewardGrantEntryData();
            pendingEntry.GrantKey = "elite_bonus_pending";
            pendingEntry.DisplayLabel = rewardLabel + " Bonus";
            pendingEntry.ResourceId = resourceId;
            pendingEntry.Amount = Mathf.Max(0, pack.PendingBonusAmount);
            pendingEntry.StateKey = "pending";
            pendingEntry.SummaryText = "Pending bonus " + BuildLootAmountText(pendingEntry.Amount);
            entries.Add(pendingEntry);
        }

        if (pack.GrantedBonusAmount > 0)
        {
            PrototypeRpgRewardGrantEntryData grantedEntry = new PrototypeRpgRewardGrantEntryData();
            grantedEntry.GrantKey = "elite_bonus_granted";
            grantedEntry.DisplayLabel = rewardLabel + " Bonus";
            grantedEntry.ResourceId = resourceId;
            grantedEntry.Amount = Mathf.Max(0, pack.GrantedBonusAmount);
            grantedEntry.StateKey = "granted";
            grantedEntry.SummaryText = "Granted bonus " + BuildLootAmountText(grantedEntry.Amount);
            entries.Add(grantedEntry);
        }

        if (pack.LostPendingAmount > 0)
        {
            PrototypeRpgRewardGrantEntryData lostEntry = new PrototypeRpgRewardGrantEntryData();
            lostEntry.GrantKey = "elite_bonus_lost";
            lostEntry.DisplayLabel = rewardLabel + " Bonus";
            lostEntry.ResourceId = resourceId;
            lostEntry.Amount = Mathf.Max(0, pack.LostPendingAmount);
            lostEntry.StateKey = "lost";
            lostEntry.SummaryText = "Lost pending bonus " + BuildLootAmountText(lostEntry.Amount);
            entries.Add(lostEntry);
        }

        return entries.Count > 0 ? entries.ToArray() : System.Array.Empty<PrototypeRpgRewardGrantEntryData>();
    }

    private PrototypeRpgMemberPendingDeltaData[] BuildRpgMemberPendingDeltas(PrototypeRpgProgressionPreviewSnapshot previewSnapshot)
    {
        PrototypeRpgMemberProgressPreview[] members = previewSnapshot != null && previewSnapshot.Members != null
            ? previewSnapshot.Members
            : System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        if (members.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgMemberPendingDeltaData>();
        }

        PrototypeRpgPostRunUpgradeOfferSurface offerSurface = BuildRpgPostRunUpgradeOfferSurface();
        PrototypeRpgUpgradeOfferCandidate[] offerCandidates = offerSurface != null && offerSurface.Offers != null
            ? offerSurface.Offers
            : System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        PrototypeRpgApplyReadyUpgradeChoice[] applyReadyChoices = offerSurface != null && offerSurface.ApplyReadyChoices != null
            ? offerSurface.ApplyReadyChoices
            : System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
        PrototypeRpgAppliedPartyProgressState appliedProgressState = GetLatestAppliedPartyProgressStateInternal();

        PrototypeRpgMemberPendingDeltaData[] deltas = new PrototypeRpgMemberPendingDeltaData[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberProgressPreview preview = members[i] ?? new PrototypeRpgMemberProgressPreview();
            PrototypeRpgMemberPendingDeltaData delta = new PrototypeRpgMemberPendingDeltaData();
            delta.MemberId = string.IsNullOrEmpty(preview.MemberId) ? string.Empty : preview.MemberId;
            delta.DisplayName = string.IsNullOrEmpty(preview.DisplayName) ? "Member" : preview.DisplayName;
            delta.RoleLabel = string.IsNullOrEmpty(preview.RoleLabel) ? "Adventurer" : preview.RoleLabel;
            delta.Survived = preview.Survived;
            delta.KnockedOut = preview.Contribution != null && preview.Contribution.KnockedOut;
            delta.GrowthTrackId = string.IsNullOrEmpty(preview.GrowthTrackId) ? string.Empty : preview.GrowthTrackId;
            delta.JobId = string.IsNullOrEmpty(preview.JobId) ? string.Empty : preview.JobId;
            delta.EquipmentLoadoutId = string.IsNullOrEmpty(preview.EquipmentLoadoutId) ? string.Empty : preview.EquipmentLoadoutId;
            delta.SkillLoadoutId = string.IsNullOrEmpty(preview.SkillLoadoutId) ? string.Empty : preview.SkillLoadoutId;
            delta.ContributionSummaryText = BuildRpgMemberContributionSummaryText(preview.Contribution);
            delta.PendingDeltaSummaryText = BuildRpgMemberProgressPreviewSummaryText(preview);
            delta.GrowthDirectionLabel = string.IsNullOrEmpty(preview.GrowthDirectionLabel) ? string.Empty : preview.GrowthDirectionLabel;
            delta.RewardCarryoverHintText = string.IsNullOrEmpty(preview.RewardCarryoverHintText) ? string.Empty : preview.RewardCarryoverHintText;
            delta.UpgradeCandidateSummaryText = string.IsNullOrEmpty(preview.UpgradeCandidateSummaryText) ? string.Empty : preview.UpgradeCandidateSummaryText;
            delta.UpgradeOfferSummaryText = !string.IsNullOrEmpty(preview.UpgradeOfferSummaryText)
                ? preview.UpgradeOfferSummaryText
                : BuildRpgMemberUpgradeOfferSummaryText(preview.MemberId, offerCandidates);
            delta.ApplyReadySummaryText = !string.IsNullOrEmpty(preview.ApplyReadySummaryText)
                ? preview.ApplyReadySummaryText
                : BuildRpgMemberApplyReadySummaryText(preview.MemberId, applyReadyChoices);
            delta.NotableOutcomeKey = string.IsNullOrEmpty(preview.NotableOutcomeKey) ? string.Empty : preview.NotableOutcomeKey;
            delta.SuggestedGrowthHintTags = preview.SuggestedGrowthHintTags != null && preview.SuggestedGrowthHintTags.Length > 0
                ? (string[])preview.SuggestedGrowthHintTags.Clone()
                : System.Array.Empty<string>();
            delta.SuggestedRewardHintTags = preview.SuggestedRewardHintTags != null && preview.SuggestedRewardHintTags.Length > 0
                ? (string[])preview.SuggestedRewardHintTags.Clone()
                : System.Array.Empty<string>();
            delta.GrowthPathCandidates = CopyRpgGrowthPathCandidates(preview.GrowthPathCandidates);
            delta.UpgradeCandidates = CopyRpgUpgradeCandidates(preview.UpgradeCandidates);
            ApplyRpgAppliedProgressToMemberPendingDelta(delta, appliedProgressState);
            deltas[i] = delta;
        }

        return deltas;
    }

    private string[] BuildRpgPendingRewardHighlightKeys(
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgPendingRewardDeltaPack pack,
        PrototypeRpgProgressionPreviewSnapshot previewSnapshot)
    {
        List<string> keys = new List<string>();
        if (runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) && runResultSnapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None)
        {
            AddProgressionTag(keys, runResultSnapshot.ResultStateKey);
        }

        if (pack != null)
        {
            if (pack.BaseRewardAmount > 0)
            {
                AddProgressionTag(keys, "elite_reward");
            }

            if (pack.HasPendingReward)
            {
                AddProgressionTag(keys, "elite_bonus_pending");
            }

            if (pack.HasGrantedReward)
            {
                AddProgressionTag(keys, "elite_bonus_granted");
            }

            if (pack.HasCarryoverDelta)
            {
                AddProgressionTag(keys, "reward_carryover");
            }

            if (pack.LostPendingAmount > 0)
            {
                AddProgressionTag(keys, "pending_reward_lost");
            }
        }

        string[] rewardTags = previewSnapshot != null ? previewSnapshot.RewardHintTags : null;
        if (rewardTags != null)
        {
            for (int i = 0; i < rewardTags.Length; i++)
            {
                AddProgressionTag(keys, rewardTags[i]);
            }
        }

        string[] growthTags = previewSnapshot != null ? previewSnapshot.GrowthHintTags : null;
        if (growthTags != null)
        {
            for (int i = 0; i < growthTags.Length; i++)
            {
                AddProgressionTag(keys, growthTags[i]);
            }
        }

        return keys.Count > 0 ? keys.ToArray() : System.Array.Empty<string>();
    }

    private string BuildRpgPendingRewardDeltaSummaryText(PrototypeRpgPendingRewardDeltaPack pack)
    {
        if (pack == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(pack.RewardLabel))
        {
            parts.Add(pack.RewardLabel);
        }

        if (pack.BaseRewardAmount > 0)
        {
            parts.Add("Base " + BuildLootAmountText(pack.BaseRewardAmount));
        }

        if (pack.PendingBonusAmount > 0)
        {
            parts.Add("Pending " + BuildLootAmountText(pack.PendingBonusAmount));
        }

        if (pack.GrantedBonusAmount > 0)
        {
            parts.Add("Granted " + BuildLootAmountText(pack.GrantedBonusAmount));
        }

        if (pack.TotalReturnedAmount > 0)
        {
            parts.Add("Returned " + BuildLootAmountText(pack.TotalReturnedAmount));
        }

        if (pack.LostPendingAmount > 0)
        {
            parts.Add("Lost " + BuildLootAmountText(pack.LostPendingAmount));
        }

        if (!string.IsNullOrEmpty(pack.CarryoverHintText) && pack.CarryoverHintText != "None")
        {
            parts.Add(pack.CarryoverHintText);
        }

        if (!string.IsNullOrEmpty(pack.TotalLootSummaryText) && pack.TotalLootSummaryText != "None")
        {
            parts.Add(pack.TotalLootSummaryText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private PrototypeRpgPendingRewardDeltaPack BuildRpgPendingRewardDeltaPack()
    {
        PrototypeRpgPendingRewardDeltaPack pack = new PrototypeRpgPendingRewardDeltaPack();
        PrototypeRpgRunResultSnapshot runResultSnapshot = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
        PrototypeRpgProgressionPreviewSnapshot progressionPreviewSnapshot = CopyRpgProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot);
        PrototypeRpgLootOutcomeSnapshot lootOutcome = runResultSnapshot.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = runResultSnapshot.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        bool hasRewardLabel = !string.IsNullOrEmpty(_eliteRewardLabel) && _eliteRewardLabel != "None";
        if (!hasRewardLabel && !string.IsNullOrEmpty(eliteOutcome.EliteRewardLabel))
        {
            hasRewardLabel = true;
        }

        pack.ResultStateKey = string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? string.Empty : runResultSnapshot.ResultStateKey;
        pack.DungeonLabel = !string.IsNullOrEmpty(runResultSnapshot.DungeonLabel) ? runResultSnapshot.DungeonLabel : (string.IsNullOrEmpty(_currentDungeonName) ? string.Empty : _currentDungeonName);
        pack.RouteLabel = !string.IsNullOrEmpty(runResultSnapshot.RouteLabel) ? runResultSnapshot.RouteLabel : (string.IsNullOrEmpty(_selectedRouteLabel) ? string.Empty : _selectedRouteLabel);
        pack.HasPendingReward = _eliteBonusRewardPending > 0;
        pack.HasPendingRewards = pack.HasPendingReward;
        pack.HasGrantedReward = _eliteBonusRewardGrantedAmount > 0;
        pack.RewardLabel = hasRewardLabel
            ? (!string.IsNullOrEmpty(_eliteRewardLabel) && _eliteRewardLabel != "None" ? _eliteRewardLabel : eliteOutcome.EliteRewardLabel)
            : string.Empty;
        pack.RewardResourceId = !string.IsNullOrEmpty(lootOutcome.RewardResourceId) ? lootOutcome.RewardResourceId : DungeonRewardResourceId;
        pack.BaseRewardAmount = Mathf.Max(0, _eliteRewardAmount > 0 ? _eliteRewardAmount : eliteOutcome.EliteRewardAmount);
        pack.PendingBonusAmount = Mathf.Max(0, _eliteBonusRewardPending);
        pack.GrantedBonusAmount = Mathf.Max(0, _eliteBonusRewardGrantedAmount > 0 ? _eliteBonusRewardGrantedAmount : eliteOutcome.EliteBonusRewardAmount);
        pack.BattleLootAmount = Mathf.Max(0, lootOutcome.BattleLootGained);
        pack.ChestLootAmount = Mathf.Max(0, lootOutcome.ChestLootGained);
        pack.EventLootAmount = Mathf.Max(0, lootOutcome.EventLootGained);
        pack.EliteRewardAmount = Mathf.Max(0, lootOutcome.EliteRewardAmount > 0 ? lootOutcome.EliteRewardAmount : pack.BaseRewardAmount);
        pack.EliteBonusRewardAmount = Mathf.Max(0, lootOutcome.EliteBonusRewardAmount > 0 ? lootOutcome.EliteBonusRewardAmount : (pack.PendingBonusAmount > 0 ? pack.PendingBonusAmount : pack.GrantedBonusAmount));
        pack.TotalReturnedAmount = Mathf.Max(0, lootOutcome.TotalReturnedAmount > 0 ? lootOutcome.TotalReturnedAmount : lootOutcome.TotalLootGained);
        pack.LostPendingAmount = Mathf.Max(0, lootOutcome.PendingBonusRewardLostAmount > 0 ? lootOutcome.PendingBonusRewardLostAmount : _resultLostPendingRewardAmount);
        pack.CarryoverHintText = !string.IsNullOrEmpty(lootOutcome.CarryoverHintText)
            ? lootOutcome.CarryoverHintText
            : BuildRpgRewardCarryoverHintText(lootOutcome, pack.ResultStateKey);
        pack.HasCarryoverDelta = pack.LostPendingAmount > 0 || !string.IsNullOrEmpty(pack.CarryoverHintText);
        pack.TotalLootSummaryText = !string.IsNullOrEmpty(lootOutcome.FinalLootSummary)
            ? lootOutcome.FinalLootSummary
            : pack.TotalReturnedAmount > 0 ? BuildLootAmountText(pack.TotalReturnedAmount) : "None";
        pack.LootBreakdownText = BuildRpgLootBreakdownText(lootOutcome);
        pack.EliteRewardSummaryText = pack.BaseRewardAmount > 0
            ? (string.IsNullOrEmpty(pack.RewardLabel) ? "Elite Reward" : pack.RewardLabel) + " " + BuildLootAmountText(pack.BaseRewardAmount)
            : "None";
        if (pack.PendingBonusAmount > 0)
        {
            pack.BonusRewardSummaryText = "Pending bonus " + BuildLootAmountText(pack.PendingBonusAmount);
        }
        else if (pack.GrantedBonusAmount > 0)
        {
            pack.BonusRewardSummaryText = "Granted bonus " + BuildLootAmountText(pack.GrantedBonusAmount);
        }
        else if (pack.LostPendingAmount > 0)
        {
            pack.BonusRewardSummaryText = "Lost pending bonus " + BuildLootAmountText(pack.LostPendingAmount);
        }
        else
        {
            pack.BonusRewardSummaryText = "None";
        }

        pack.RewardHintTags = progressionPreviewSnapshot.RewardHintTags != null && progressionPreviewSnapshot.RewardHintTags.Length > 0
            ? (string[])progressionPreviewSnapshot.RewardHintTags.Clone()
            : System.Array.Empty<string>();
        pack.GrowthHintTags = progressionPreviewSnapshot.GrowthHintTags != null && progressionPreviewSnapshot.GrowthHintTags.Length > 0
            ? (string[])progressionPreviewSnapshot.GrowthHintTags.Clone()
            : System.Array.Empty<string>();
        pack.RewardGrantEntries = BuildRpgRewardGrantEntries(runResultSnapshot, pack);
        pack.MemberPendingDeltas = BuildRpgMemberPendingDeltas(progressionPreviewSnapshot);
        pack.HasAnyRewardDelta = pack.BaseRewardAmount > 0 ||
                                 pack.PendingBonusAmount > 0 ||
                                 pack.GrantedBonusAmount > 0 ||
                                 pack.TotalReturnedAmount > 0 ||
                                 pack.HasCarryoverDelta ||
                                 pack.RewardGrantEntries.Length > 0 ||
                                 pack.MemberPendingDeltas.Length > 0 ||
                                 pack.RewardHintTags.Length > 0 ||
                                 pack.GrowthHintTags.Length > 0;
        pack.TopHighlightKeys = BuildRpgPendingRewardHighlightKeys(runResultSnapshot, pack, progressionPreviewSnapshot);
        pack.SummaryText = BuildRpgPendingRewardDeltaSummaryText(pack);
        return pack;
    }

    private PrototypeRpgAvailableSurfaceFlags BuildRpgPresentationAvailableSurfaceFlags(
        PrototypeRpgPresentationPhase phase,
        PrototypeBattleUiSurfaceData battleSurface,
        PrototypeRpgPostRunSummarySurfaceData postRunSummarySurface,
        PrototypeRpgPendingRewardDeltaPack pendingRewardDeltaPack,
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgCombatContributionSnapshot contributionSnapshot,
        PrototypeRpgProgressionPreviewSnapshot progressionPreviewSnapshot)
    {
        PrototypeRpgAvailableSurfaceFlags flags = PrototypeRpgAvailableSurfaceFlags.None;

        if (phase == PrototypeRpgPresentationPhase.Battle || (battleSurface != null && (battleSurface.IsBattleActive || battleSurface.IsTargetSelectionActive)))
        {
            flags |= PrototypeRpgAvailableSurfaceFlags.BattleSurface;
        }

        if ((postRunSummarySurface != null && !string.IsNullOrEmpty(postRunSummarySurface.ResultStateKey)) || phase == PrototypeRpgPresentationPhase.ResultPanel || phase == PrototypeRpgPresentationPhase.PostRunSummary)
        {
            flags |= PrototypeRpgAvailableSurfaceFlags.PostRunSummarySurface;
        }

        if (pendingRewardDeltaPack != null && pendingRewardDeltaPack.HasAnyRewardDelta)
        {
            flags |= PrototypeRpgAvailableSurfaceFlags.PendingRewardDeltaPack;
        }

        if (HasRpgRunResultSnapshotData(runResultSnapshot))
        {
            flags |= PrototypeRpgAvailableSurfaceFlags.RunResultSnapshot;
        }

        if (HasRpgCombatContributionSnapshotData(contributionSnapshot))
        {
            flags |= PrototypeRpgAvailableSurfaceFlags.CombatContributionSnapshot;
        }

        if (HasRpgProgressionPreviewSnapshotData(progressionPreviewSnapshot))
        {
            flags |= PrototypeRpgAvailableSurfaceFlags.ProgressionPreviewSnapshot;
        }

        return flags;
    }

    private string BuildRpgPresentationGatewayHeadline(
        PrototypeRpgPresentationPhase phase,
        PrototypeBattleUiSurfaceData battleSurface,
        PrototypeRpgPostRunSummarySurfaceData postRunSummarySurface)
    {
        switch (phase)
        {
            case PrototypeRpgPresentationPhase.Battle:
                if (battleSurface != null && !string.IsNullOrEmpty(battleSurface.EncounterName) && battleSurface.EncounterName != "None")
                {
                    return battleSurface.EncounterName;
                }

                return string.IsNullOrEmpty(_currentDungeonName) ? "Battle" : _currentDungeonName;
            case PrototypeRpgPresentationPhase.ResultPanel:
            case PrototypeRpgPresentationPhase.PostRunSummary:
                if (postRunSummarySurface != null && !string.IsNullOrEmpty(postRunSummarySurface.Headline) && postRunSummarySurface.Headline != "None")
                {
                    return postRunSummarySurface.Headline;
                }

                return "Post-Run Summary";
            default:
                return "RPG Presentation";
        }
    }

    private string BuildRpgPresentationGatewaySubheadline(
        PrototypeRpgPresentationPhase phase,
        PrototypeBattleUiSurfaceData battleSurface,
        PrototypeRpgPostRunSummarySurfaceData postRunSummarySurface)
    {
        if (phase == PrototypeRpgPresentationPhase.Battle)
        {
            List<string> parts = new List<string>();
            if (battleSurface != null && !string.IsNullOrEmpty(battleSurface.CurrentDungeonName) && battleSurface.CurrentDungeonName != "None")
            {
                parts.Add(battleSurface.CurrentDungeonName);
            }

            if (battleSurface != null && !string.IsNullOrEmpty(battleSurface.CurrentRouteLabel) && battleSurface.CurrentRouteLabel != "None")
            {
                parts.Add(battleSurface.CurrentRouteLabel);
            }

            if (battleSurface != null && !string.IsNullOrEmpty(battleSurface.PartyCondition) && battleSurface.PartyCondition != "None")
            {
                parts.Add(battleSurface.PartyCondition);
            }

            return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "Battle in progress";
        }

        if (postRunSummarySurface != null && !string.IsNullOrEmpty(postRunSummarySurface.ResultSummaryText))
        {
            return postRunSummarySurface.ResultSummaryText;
        }

        if (postRunSummarySurface != null && !string.IsNullOrEmpty(postRunSummarySurface.Subheadline))
        {
            return postRunSummarySurface.Subheadline;
        }

        return string.Empty;
    }

    private string[] BuildRpgPresentationGatewayHighlightKeys(
        string phaseKey,
        PrototypeRpgPostRunSummarySurfaceData postRunSummarySurface,
        PrototypeRpgPendingRewardDeltaPack pendingRewardDeltaPack,
        PrototypeRpgProgressionPreviewSnapshot progressionPreviewSnapshot)
    {
        List<string> keys = new List<string>();
        if (!string.IsNullOrEmpty(phaseKey) && phaseKey != "none")
        {
            AddProgressionTag(keys, phaseKey);
        }

        string[] summaryKeys = postRunSummarySurface != null ? postRunSummarySurface.TopHighlightKeys : null;
        if (summaryKeys != null)
        {
            for (int i = 0; i < summaryKeys.Length; i++)
            {
                AddProgressionTag(keys, summaryKeys[i]);
            }
        }

        string[] pendingKeys = pendingRewardDeltaPack != null ? pendingRewardDeltaPack.TopHighlightKeys : null;
        if (pendingKeys != null)
        {
            for (int i = 0; i < pendingKeys.Length; i++)
            {
                AddProgressionTag(keys, pendingKeys[i]);
            }
        }

        if (pendingRewardDeltaPack != null)
        {
            if (pendingRewardDeltaPack.HasPendingReward)
            {
                AddProgressionTag(keys, "elite_bonus_pending");
            }

            if (pendingRewardDeltaPack.HasGrantedReward)
            {
                AddProgressionTag(keys, "elite_bonus_granted");
            }
        }

        string[] rewardHintTags = progressionPreviewSnapshot != null ? progressionPreviewSnapshot.RewardHintTags : null;
        if (rewardHintTags != null)
        {
            for (int i = 0; i < rewardHintTags.Length; i++)
            {
                AddProgressionTag(keys, rewardHintTags[i]);
            }
        }

        string[] growthHintTags = progressionPreviewSnapshot != null ? progressionPreviewSnapshot.GrowthHintTags : null;
        if (growthHintTags != null)
        {
            for (int i = 0; i < growthHintTags.Length; i++)
            {
                AddProgressionTag(keys, growthHintTags[i]);
            }
        }

        return keys.Count > 0 ? keys.ToArray() : System.Array.Empty<string>();
    }

    private string BuildRpgPresentationRunIdentity(string dungeonLabel, string routeLabel, string runStateKey)
    {
        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(dungeonLabel))
        {
            parts.Add(dungeonLabel);
        }

        if (!string.IsNullOrEmpty(routeLabel))
        {
            parts.Add(routeLabel);
        }

        if (!string.IsNullOrEmpty(runStateKey) && runStateKey != "none")
        {
            parts.Add(runStateKey);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "none";
    }

    private PrototypeRpgPresentationGatewayData BuildLatestRpgPresentationGateway()
    {
        PrototypeRpgPresentationGatewayData gateway = new PrototypeRpgPresentationGatewayData();
        PrototypeRpgPresentationPhase phase = BuildRpgPresentationPhase();
        PrototypeRpgRunResultSnapshot runResultSnapshot = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
        PrototypeRpgCombatContributionSnapshot contributionSnapshot = CopyRpgCombatContributionSnapshot(_latestRpgCombatContributionSnapshot);
        PrototypeRpgProgressionPreviewSnapshot progressionPreviewSnapshot = CopyRpgProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot);
        PrototypeBattleUiSurfaceData battleSurface = BuildBattleUiSurfaceData();
        PrototypeBattleInteractionSurfaceData battleInteractionSurface = BuildBattleInteractionSurfaceData();
        PrototypeRpgPostRunSummarySurfaceData postRunSummarySurface = BuildRpgPostRunSummarySurfaceData();
        PrototypeRpgPendingRewardDeltaPack pendingRewardDeltaPack = BuildRpgPendingRewardDeltaPack();
        string phaseKey = GetRpgPresentationPhaseKey(phase);

        gateway.Phase = phase;
        gateway.PhaseKey = phaseKey;
        gateway.DungeonLabel = !string.IsNullOrEmpty(runResultSnapshot.DungeonLabel)
            ? runResultSnapshot.DungeonLabel
            : battleSurface != null && !string.IsNullOrEmpty(battleSurface.CurrentDungeonName) && battleSurface.CurrentDungeonName != "None"
                ? battleSurface.CurrentDungeonName
                : string.IsNullOrEmpty(_currentDungeonName) ? string.Empty : _currentDungeonName;
        gateway.RouteLabel = !string.IsNullOrEmpty(runResultSnapshot.RouteLabel)
            ? runResultSnapshot.RouteLabel
            : battleSurface != null && !string.IsNullOrEmpty(battleSurface.CurrentRouteLabel) && battleSurface.CurrentRouteLabel != "None"
                ? battleSurface.CurrentRouteLabel
                : string.IsNullOrEmpty(_selectedRouteLabel) ? string.Empty : _selectedRouteLabel;
        gateway.RunStateKey = BuildRpgPresentationRunStateKey(phase, runResultSnapshot);
        gateway.CurrentRunIdentity = BuildRpgPresentationRunIdentity(gateway.DungeonLabel, gateway.RouteLabel, gateway.RunStateKey);
        gateway.BattleSurface = battleSurface ?? new PrototypeBattleUiSurfaceData();
        gateway.BattleInteractionSurface = battleInteractionSurface ?? new PrototypeBattleInteractionSurfaceData();
        gateway.PostRunSummarySurface = postRunSummarySurface ?? new PrototypeRpgPostRunSummarySurfaceData();
        gateway.PendingRewardDeltaPack = pendingRewardDeltaPack ?? new PrototypeRpgPendingRewardDeltaPack();
        gateway.RunResultSnapshot = runResultSnapshot;
        gateway.CombatContributionSnapshot = contributionSnapshot;
        gateway.ProgressionPreviewSnapshot = progressionPreviewSnapshot;
        gateway.AvailableSurfaces = BuildRpgPresentationAvailableSurfaceFlags(
            phase,
            gateway.BattleSurface,
            gateway.PostRunSummarySurface,
            gateway.PendingRewardDeltaPack,
            gateway.RunResultSnapshot,
            gateway.CombatContributionSnapshot,
            gateway.ProgressionPreviewSnapshot);
        if (gateway.BattleInteractionSurface != null && gateway.BattleInteractionSurface.HasInteractionSurface)
        {
            gateway.AvailableSurfaces |= PrototypeRpgAvailableSurfaceFlags.BattleInteractionSurface;
        }

        gateway.HasBattleSurface = (gateway.AvailableSurfaces & PrototypeRpgAvailableSurfaceFlags.BattleSurface) != 0;
        gateway.HasBattleInteractionSurface = (gateway.AvailableSurfaces & PrototypeRpgAvailableSurfaceFlags.BattleInteractionSurface) != 0;
        gateway.HasPostRunSummarySurface = (gateway.AvailableSurfaces & PrototypeRpgAvailableSurfaceFlags.PostRunSummarySurface) != 0;
        gateway.HasPendingRewardDeltaPack = (gateway.AvailableSurfaces & PrototypeRpgAvailableSurfaceFlags.PendingRewardDeltaPack) != 0;
        gateway.Headline = BuildRpgPresentationGatewayHeadline(phase, gateway.BattleSurface, gateway.PostRunSummarySurface);
        gateway.Subheadline = BuildRpgPresentationGatewaySubheadline(phase, gateway.BattleSurface, gateway.PostRunSummarySurface);
        gateway.TopHighlightKeys = BuildRpgPresentationGatewayHighlightKeys(
            phaseKey,
            gateway.PostRunSummarySurface,
            gateway.PendingRewardDeltaPack,
            gateway.ProgressionPreviewSnapshot);
        return gateway;
    }

    private PrototypeRpgCombatContributionSnapshot BuildRpgCombatContributionSnapshot(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgCombatContributionSnapshot snapshot = new PrototypeRpgCombatContributionSnapshot();
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = safeRunResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome.Members ?? System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();

        snapshot.ResultStateKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? string.Empty : safeRunResult.ResultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(safeRunResult.DungeonId) ? string.Empty : safeRunResult.DungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(safeRunResult.DungeonLabel) ? string.Empty : safeRunResult.DungeonLabel;
        snapshot.RouteId = string.IsNullOrEmpty(safeRunResult.RouteId) ? string.Empty : safeRunResult.RouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(safeRunResult.RouteLabel) ? string.Empty : safeRunResult.RouteLabel;
        snapshot.TotalTurnsTaken = Mathf.Max(0, safeRunResult.TotalTurnsTaken);
        snapshot.SurvivingMemberCount = Mathf.Max(0, partyOutcome.SurvivingMemberCount);
        snapshot.KnockedOutMemberCount = Mathf.Max(0, partyOutcome.KnockedOutMemberCount);
        snapshot.TotalDamageDealt = Mathf.Max(0, _totalDamageDealt);
        snapshot.TotalDamageTaken = Mathf.Max(0, _totalDamageTaken);
        snapshot.TotalHealingDone = Mathf.Max(0, _totalHealingDone);
        snapshot.RecentEvents = BuildRecentBattleEventRecords();

        if (members.Length <= 0)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
            return snapshot;
        }

        PrototypeRpgMemberContributionSnapshot[] memberSnapshots = new PrototypeRpgMemberContributionSnapshot[members.Length];
        bool eliteDefeated = safeRunResult.EliteOutcome != null && safeRunResult.EliteOutcome.IsEliteDefeated;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberOutcome = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            PrototypeRpgMemberContributionSnapshot memberSnapshot = new PrototypeRpgMemberContributionSnapshot();
            memberSnapshot.MemberId = string.IsNullOrEmpty(memberOutcome.MemberId) ? string.Empty : memberOutcome.MemberId;
            memberSnapshot.DisplayName = string.IsNullOrEmpty(memberOutcome.DisplayName) ? string.Empty : memberOutcome.DisplayName;
            memberSnapshot.RoleTag = string.IsNullOrEmpty(memberOutcome.RoleTag) ? string.Empty : memberOutcome.RoleTag;
            memberSnapshot.RoleLabel = string.IsNullOrEmpty(memberOutcome.RoleLabel) ? string.Empty : memberOutcome.RoleLabel;
            memberSnapshot.DefaultSkillId = string.IsNullOrEmpty(memberOutcome.DefaultSkillId) ? string.Empty : memberOutcome.DefaultSkillId;
            memberSnapshot.DamageDealt = GetRunMemberContributionValue(_runMemberDamageDealt, i);
            memberSnapshot.DamageTaken = GetRunMemberContributionValue(_runMemberDamageTaken, i);
            memberSnapshot.HealingDone = GetRunMemberContributionValue(_runMemberHealingDone, i);
            memberSnapshot.ActionCount = GetRunMemberContributionValue(_runMemberActionCount, i);
            memberSnapshot.KillCount = GetRunMemberContributionValue(_runMemberKillCount, i);
            memberSnapshot.KnockedOut = memberOutcome.KnockedOut;
            memberSnapshot.Survived = memberOutcome.Survived;
            memberSnapshot.EliteVictor = eliteDefeated && memberOutcome.Survived;
            memberSnapshot.ContributionSummaryText = BuildRpgMemberContributionSummaryText(memberSnapshot);
            memberSnapshots[i] = memberSnapshot;
        }

        snapshot.Members = memberSnapshots;
        return snapshot;
    }
    private string[] BuildRpgProgressionPreviewRewardHintTags(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgProgressionSeedSnapshot progressionSeedSnapshot)
    {
        List<string> tags = new List<string>();
        string[] seedTags = progressionSeedSnapshot != null ? (progressionSeedSnapshot.RewardTags ?? System.Array.Empty<string>()) : System.Array.Empty<string>();
        for (int i = 0; i < seedTags.Length; i++)
        {
            AddProgressionTag(tags, seedTags[i]);
        }

        if (runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(tags, "elite_victor");
        }

        if (runResultSnapshot != null && runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            AddProgressionTag(tags, "retreat_penalty_hint");
        }

        bool riskyRoute = (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId) || (progressionSeedSnapshot != null && progressionSeedSnapshot.RouteId == RiskyRouteId);
        if (riskyRoute)
        {
            AddProgressionTag(tags, "risky_route_hint");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildRpgProgressionPreviewGrowthHintTags(PrototypeRpgProgressionSeedSnapshot progressionSeedSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot)
    {
        List<string> tags = new List<string>();
        string[] seedTags = progressionSeedSnapshot != null ? (progressionSeedSnapshot.GrowthTags ?? System.Array.Empty<string>()) : System.Array.Empty<string>();
        for (int i = 0; i < seedTags.Length; i++)
        {
            AddProgressionTag(tags, seedTags[i]);
        }

        if (contributionSnapshot != null && contributionSnapshot.KnockedOutMemberCount > 0)
        {
            AddProgressionTag(tags, "knocked_out");
        }

        if (contributionSnapshot != null && contributionSnapshot.SurvivingMemberCount > 0)
        {
            AddProgressionTag(tags, "survivor");
        }

        if (contributionSnapshot != null && contributionSnapshot.TotalHealingDone > 0)
        {
            AddProgressionTag(tags, "party_support");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildMemberProgressionGrowthHintTags(PrototypeRpgMemberContributionSnapshot contribution)
    {
        List<string> tags = new List<string>();
        if (contribution == null)
        {
            return System.Array.Empty<string>();
        }

        if (contribution.RoleTag == "warrior" && contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt)
        {
            AddProgressionTag(tags, "frontline_pressure");
        }

        if (contribution.RoleTag == "mage" && contribution.DamageDealt >= 8)
        {
            AddProgressionTag(tags, "aoe_clear");
        }

        if (contribution.RoleTag == "cleric" && contribution.HealingDone > 0)
        {
            AddProgressionTag(tags, "party_support");
        }

        if (contribution.Survived)
        {
            AddProgressionTag(tags, "survivor");
        }

        if (contribution.KnockedOut)
        {
            AddProgressionTag(tags, "knocked_out");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildMemberProgressionRewardHintTags(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        List<string> tags = new List<string>();
        if (contribution == null)
        {
            return System.Array.Empty<string>();
        }

        if (contribution.RoleTag == "rogue" && contribution.KillCount > 0)
        {
            AddProgressionTag(tags, "finisher");
        }

        if (runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated && contribution.Survived)
        {
            AddProgressionTag(tags, "elite_victor");
        }

        if (runResultSnapshot != null && runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            AddProgressionTag(tags, "retreat_penalty_hint");
        }

        if (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId)
        {
            AddProgressionTag(tags, "risky_route_hint");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string BuildMemberNotableOutcomeKey(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (contribution == null)
        {
            return string.Empty;
        }

        if (contribution.KnockedOut)
        {
            return "knocked_out";
        }

        if (runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated && contribution.Survived)
        {
            return "elite_victor";
        }

        if (contribution.RoleTag == "cleric" && contribution.HealingDone > 0)
        {
            return "party_support";
        }

        if (contribution.RoleTag == "mage" && contribution.DamageDealt >= 8)
        {
            return "aoe_clear";
        }

        if (contribution.RoleTag == "rogue" && contribution.KillCount > 0)
        {
            return "finisher";
        }

        if (contribution.RoleTag == "warrior" && contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt)
        {
            return "frontline_pressure";
        }

        if (runResultSnapshot != null && runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            return "retreat_penalty_hint";
        }

        if (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId)
        {
            return "risky_route_hint";
        }

        if (contribution.Survived)
        {
            return "survivor";
        }

        return string.Empty;
    }

    private string BuildRpgNormalizedHookLabel(string hookId, string fallbackLabel)
    {
        if (string.IsNullOrEmpty(hookId))
        {
            return string.IsNullOrEmpty(fallbackLabel) ? string.Empty : fallbackLabel;
        }

        string normalized = hookId;
        string[] prefixes = { "growth_", "track_", "job_", "equipment_", "equip_", "loadout_", "skill_" };
        for (int i = 0; i < prefixes.Length; i++)
        {
            string prefix = prefixes[i];
            if (!string.IsNullOrEmpty(prefix) && normalized.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(prefix.Length);
                break;
            }
        }

        string[] tokens = normalized.Split(new[] { '_', '-', '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length <= 0)
        {
            return string.IsNullOrEmpty(fallbackLabel) ? hookId : fallbackLabel;
        }

        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            string lower = token.ToLowerInvariant();
            tokens[i] = char.ToUpperInvariant(lower[0]) + (lower.Length > 1 ? lower.Substring(1) : string.Empty);
        }

        string label = string.Join(" ", tokens);
        return string.IsNullOrEmpty(label) ? (string.IsNullOrEmpty(fallbackLabel) ? hookId : fallbackLabel) : label;
    }

    private string BuildRpgContributionReasonText(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (contribution == null)
        {
            return string.Empty;
        }

        if (contribution.HealingDone > 0 && contribution.HealingDone >= contribution.DamageDealt)
        {
            return "Healing output favored support growth.";
        }

        if (contribution.KillCount > 0)
        {
            return "Kill pressure favored offensive growth.";
        }

        if (contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt)
        {
            return "Frontline pressure favored durable growth.";
        }

        if (contribution.EliteVictor)
        {
            return "Elite clear flagged a higher tier upgrade route.";
        }

        if (runResultSnapshot != null && runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            return "Retreat outcome shifted priority toward safer follow-ups.";
        }

        if (!contribution.Survived || contribution.KnockedOut)
        {
            return "Recovery needs now bias the next development step.";
        }

        return "Stable run data kept the next growth step flexible.";
    }

    private string BuildRpgMemberGrowthDirectionLabel(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (contribution == null)
        {
            return string.Empty;
        }

        if (contribution.KnockedOut || !contribution.Survived)
        {
            return "Recovery Route";
        }

        if (contribution.HealingDone > 0 && contribution.HealingDone >= contribution.DamageDealt)
        {
            return "Support Route";
        }

        if (contribution.KillCount > 0 || contribution.DamageDealt >= 8)
        {
            return "Offense Route";
        }

        if (contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt)
        {
            return "Guard Route";
        }

        if (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId)
        {
            return "Risk Route";
        }

        return "Balanced Route";
    }

    private string BuildRpgMemberRewardCarryoverHintText(
        PrototypeRpgMemberContributionSnapshot contribution,
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgLootSeed lootSeed)
    {
        PrototypeRpgLootOutcomeSnapshot lootOutcome = runResultSnapshot != null ? runResultSnapshot.LootOutcome : null;
        string carryoverHint = lootSeed != null && !string.IsNullOrEmpty(lootSeed.CarryoverHintText)
            ? lootSeed.CarryoverHintText
            : lootOutcome != null ? lootOutcome.CarryoverHintText : string.Empty;
        int returnedAmount = lootSeed != null && lootSeed.TotalLootGained > 0
            ? Mathf.Max(0, lootSeed.TotalLootGained)
            : lootOutcome != null ? Mathf.Max(0, lootOutcome.TotalReturnedAmount > 0 ? lootOutcome.TotalReturnedAmount : lootOutcome.TotalLootGained) : 0;
        int lostAmount = lootSeed != null && lootSeed.PendingBonusRewardLostAmount > 0
            ? Mathf.Max(0, lootSeed.PendingBonusRewardLostAmount)
            : lootOutcome != null ? Mathf.Max(0, lootOutcome.PendingBonusRewardLostAmount) : 0;

        if (string.IsNullOrEmpty(carryoverHint) && returnedAmount <= 0 && lostAmount <= 0)
        {
            return string.Empty;
        }

        if (contribution != null)
        {
            if (contribution.KnockedOut)
            {
                return lostAmount > 0
                    ? "Recover before committing lost bonus recovery."
                    : (!string.IsNullOrEmpty(carryoverHint) ? carryoverHint : "Recover first before spending returned rewards.");
            }

            if (contribution.HealingDone > 0)
            {
                return !string.IsNullOrEmpty(carryoverHint) ? carryoverHint : "Carryover favors support recovery upgrades.";
            }

            if (contribution.KillCount > 0)
            {
                return !string.IsNullOrEmpty(carryoverHint) ? carryoverHint : "Carryover can fund an aggressive upgrade branch.";
            }

            if (contribution.DamageTaken > contribution.DamageDealt)
            {
                return !string.IsNullOrEmpty(carryoverHint) ? carryoverHint : "Carryover can reinforce frontline durability.";
            }
        }

        if (!string.IsNullOrEmpty(carryoverHint))
        {
            return carryoverHint;
        }

        if (lostAmount > 0)
        {
            return "Lost pending bonus " + BuildLootAmountText(lostAmount);
        }

        return returnedAmount > 0 ? "Carryover available " + BuildLootAmountText(returnedAmount) : string.Empty;
    }

    private void AddGrowthPathCandidate(
        List<PrototypeRpgGrowthPathCandidate> candidates,
        string candidateTypeKey,
        string sourceHookId,
        string candidateTargetId,
        string previewLabel,
        string previewText,
        string recommendedBecauseText,
        int priority,
        bool availableLater,
        string blockedReasonHint)
    {
        if (candidates == null)
        {
            return;
        }

        string targetKey = string.IsNullOrEmpty(candidateTargetId) ? (string.IsNullOrEmpty(sourceHookId) ? candidateTypeKey : sourceHookId) : candidateTargetId;
        string candidateKey = string.IsNullOrEmpty(candidateTypeKey) ? targetKey : candidateTypeKey + ":" + targetKey;
        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeRpgGrowthPathCandidate existing = candidates[i];
            if (existing != null && existing.CandidateKey == candidateKey)
            {
                return;
            }
        }

        PrototypeRpgGrowthPathCandidate candidate = new PrototypeRpgGrowthPathCandidate();
        candidate.CandidateKey = candidateKey;
        candidate.CandidateTypeKey = string.IsNullOrEmpty(candidateTypeKey) ? string.Empty : candidateTypeKey;
        candidate.SourceHookId = string.IsNullOrEmpty(sourceHookId) ? string.Empty : sourceHookId;
        candidate.CandidateTargetId = string.IsNullOrEmpty(candidateTargetId) ? string.Empty : candidateTargetId;
        candidate.PreviewLabel = string.IsNullOrEmpty(previewLabel) ? BuildRpgNormalizedHookLabel(candidateTargetId, candidateTypeKey) : previewLabel;
        candidate.PreviewText = string.IsNullOrEmpty(previewText) ? string.Empty : previewText;
        candidate.RecommendedBecauseText = string.IsNullOrEmpty(recommendedBecauseText) ? string.Empty : recommendedBecauseText;
        candidate.Priority = Mathf.Max(0, priority);
        candidate.AvailableLater = availableLater;
        candidate.BlockedReasonHint = string.IsNullOrEmpty(blockedReasonHint) ? string.Empty : blockedReasonHint;
        candidates.Add(candidate);
    }

    private void AddUpgradeCandidate(
        List<PrototypeRpgUpgradeCandidate> candidates,
        string candidateTypeKey,
        string sourceHookId,
        string candidateTargetId,
        string previewLabel,
        string previewText,
        string recommendedBecauseText,
        int priority,
        bool availableLater,
        string blockedReasonHint)
    {
        if (candidates == null)
        {
            return;
        }

        string targetKey = string.IsNullOrEmpty(candidateTargetId) ? (string.IsNullOrEmpty(sourceHookId) ? candidateTypeKey : sourceHookId) : candidateTargetId;
        string candidateKey = string.IsNullOrEmpty(candidateTypeKey) ? targetKey : candidateTypeKey + ":" + targetKey;
        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeRpgUpgradeCandidate existing = candidates[i];
            if (existing != null && existing.CandidateKey == candidateKey)
            {
                return;
            }
        }

        PrototypeRpgUpgradeCandidate candidate = new PrototypeRpgUpgradeCandidate();
        candidate.CandidateKey = candidateKey;
        candidate.CandidateTypeKey = string.IsNullOrEmpty(candidateTypeKey) ? string.Empty : candidateTypeKey;
        candidate.SourceHookId = string.IsNullOrEmpty(sourceHookId) ? string.Empty : sourceHookId;
        candidate.CandidateTargetId = string.IsNullOrEmpty(candidateTargetId) ? string.Empty : candidateTargetId;
        candidate.PreviewLabel = string.IsNullOrEmpty(previewLabel) ? BuildRpgNormalizedHookLabel(candidateTargetId, candidateTypeKey) : previewLabel;
        candidate.PreviewText = string.IsNullOrEmpty(previewText) ? string.Empty : previewText;
        candidate.RecommendedBecauseText = string.IsNullOrEmpty(recommendedBecauseText) ? string.Empty : recommendedBecauseText;
        candidate.Priority = Mathf.Max(0, priority);
        candidate.AvailableLater = availableLater;
        candidate.BlockedReasonHint = string.IsNullOrEmpty(blockedReasonHint) ? string.Empty : blockedReasonHint;
        candidates.Add(candidate);
    }

    private PrototypeRpgGrowthPathCandidate[] BuildRpgMemberGrowthPathCandidates(
        PrototypeRpgMemberContributionSnapshot contribution,
        PrototypeRpgMemberProgressionSeed memberSeed,
        PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        List<PrototypeRpgGrowthPathCandidate> candidates = new List<PrototypeRpgGrowthPathCandidate>();
        string directionLabel = BuildRpgMemberGrowthDirectionLabel(contribution, runResultSnapshot);
        string reasonText = BuildRpgContributionReasonText(contribution, runResultSnapshot);
        bool availableLater = contribution == null || (!contribution.KnockedOut && contribution.Survived);
        string blockedReason = contribution != null && contribution.KnockedOut ? "Recover from KO before locking this path." : string.Empty;

        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.GrowthTrackId))
        {
            string label = BuildRpgNormalizedHookLabel(memberSeed.GrowthTrackId, "Growth Track");
            AddGrowthPathCandidate(candidates, "growth_track", memberSeed.GrowthTrackId, memberSeed.GrowthTrackId, label, directionLabel + " through " + label + ".", reasonText, 300, availableLater, blockedReason);
        }

        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.JobId))
        {
            string label = BuildRpgNormalizedHookLabel(memberSeed.JobId, "Job Path");
            AddGrowthPathCandidate(candidates, "job", memberSeed.JobId, memberSeed.JobId, label, "Job lane preview: " + label + ".", reasonText, contribution != null && contribution.EliteVictor ? 280 : 220, availableLater, blockedReason);
        }

        if (contribution != null && contribution.EliteVictor)
        {
            AddGrowthPathCandidate(candidates, "elite_route", string.Empty, "elite_victor", "Elite Victor Route", "Elite clear unlocks a higher-tier growth branch.", "Elite victory raised this candidate.", 260, true, string.Empty);
        }

        if (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId)
        {
            AddGrowthPathCandidate(candidates, "risk_route", string.Empty, "risky_route", "Risk Route Bias", "Risky route data nudges this member toward bolder growth.", "Route pressure biased the growth preview.", 180, true, string.Empty);
        }

        return candidates.Count > 0 ? candidates.ToArray() : System.Array.Empty<PrototypeRpgGrowthPathCandidate>();
    }

    private PrototypeRpgUpgradeCandidate[] BuildRpgMemberUpgradeCandidates(
        PrototypeRpgMemberContributionSnapshot contribution,
        PrototypeRpgMemberProgressionSeed memberSeed,
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgLootSeed lootSeed)
    {
        List<PrototypeRpgUpgradeCandidate> candidates = new List<PrototypeRpgUpgradeCandidate>();
        string reasonText = BuildRpgContributionReasonText(contribution, runResultSnapshot);
        string carryoverHint = BuildRpgMemberRewardCarryoverHintText(contribution, runResultSnapshot, lootSeed);
        bool availableLater = contribution == null || (!contribution.KnockedOut && contribution.Survived);
        string blockedReason = contribution != null && contribution.KnockedOut ? "Recover from KO before finalizing upgrades." : string.Empty;

        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.SkillLoadoutId))
        {
            string label = BuildRpgNormalizedHookLabel(memberSeed.SkillLoadoutId, "Skill Loadout");
            AddUpgradeCandidate(candidates, "skill_loadout", memberSeed.SkillLoadoutId, memberSeed.SkillLoadoutId, label, "Skill package preview: " + label + ".", reasonText, 260, availableLater, blockedReason);
        }

        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.EquipmentLoadoutId))
        {
            string label = BuildRpgNormalizedHookLabel(memberSeed.EquipmentLoadoutId, "Equipment Loadout");
            AddUpgradeCandidate(candidates, "equipment_loadout", memberSeed.EquipmentLoadoutId, memberSeed.EquipmentLoadoutId, label, "Equipment package preview: " + label + ".", reasonText, contribution != null && contribution.DamageTaken > 0 ? 250 : 210, availableLater, blockedReason);
        }

        if (!string.IsNullOrEmpty(carryoverHint))
        {
            AddUpgradeCandidate(candidates, "reward_carryover", string.Empty, "carryover", "Carryover Bias", carryoverHint, "Reward carryover influenced the upgrade preview.", 190, true, string.Empty);
        }

        if (runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated)
        {
            AddUpgradeCandidate(candidates, "unlock_seed", string.Empty, "elite_unlock", "Elite Unlock Seed", "Elite result can unlock a later upgrade branch.", "Elite result generated an unlock preview.", 170, true, string.Empty);
        }

        return candidates.Count > 0 ? candidates.ToArray() : System.Array.Empty<PrototypeRpgUpgradeCandidate>();
    }

    private string BuildRpgGrowthPathCandidateSummaryText(PrototypeRpgGrowthPathCandidate[] candidates)
    {
        PrototypeRpgGrowthPathCandidate[] safeCandidates = candidates ?? System.Array.Empty<PrototypeRpgGrowthPathCandidate>();
        if (safeCandidates.Length <= 0)
        {
            return "None";
        }

        List<string> labels = new List<string>();
        int displayCount = Mathf.Min(2, safeCandidates.Length);
        for (int i = 0; i < displayCount; i++)
        {
            PrototypeRpgGrowthPathCandidate candidate = safeCandidates[i];
            if (candidate == null)
            {
                continue;
            }

            string label = !string.IsNullOrEmpty(candidate.PreviewLabel)
                ? candidate.PreviewLabel
                : BuildRpgNormalizedHookLabel(candidate.CandidateTargetId, "Growth Path");
            if (!string.IsNullOrEmpty(label))
            {
                labels.Add(label);
            }
        }

        if (safeCandidates.Length > displayCount)
        {
            labels.Add("+" + (safeCandidates.Length - displayCount) + " more");
        }

        return labels.Count > 0 ? string.Join(" / ", labels.ToArray()) : "None";
    }

    private string BuildRpgUpgradeCandidateSummaryText(PrototypeRpgUpgradeCandidate[] candidates)
    {
        PrototypeRpgUpgradeCandidate[] safeCandidates = candidates ?? System.Array.Empty<PrototypeRpgUpgradeCandidate>();
        if (safeCandidates.Length <= 0)
        {
            return "None";
        }

        List<string> labels = new List<string>();
        int displayCount = Mathf.Min(2, safeCandidates.Length);
        for (int i = 0; i < displayCount; i++)
        {
            PrototypeRpgUpgradeCandidate candidate = safeCandidates[i];
            if (candidate == null)
            {
                continue;
            }

            string label = !string.IsNullOrEmpty(candidate.PreviewLabel)
                ? candidate.PreviewLabel
                : BuildRpgNormalizedHookLabel(candidate.CandidateTargetId, "Upgrade");
            if (!string.IsNullOrEmpty(label))
            {
                labels.Add(label);
            }
        }

        if (safeCandidates.Length > displayCount)
        {
            labels.Add("+" + (safeCandidates.Length - displayCount) + " more");
        }

        return labels.Count > 0 ? string.Join(" / ", labels.ToArray()) : "None";
    }

    private string BuildRpgGrowthPathCandidateSummaryText(PrototypeRpgMemberProgressPreview[] previews)
    {
        PrototypeRpgMemberProgressPreview[] safePreviews = previews ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        List<string> parts = new List<string>();
        int hiddenCount = 0;
        for (int i = 0; i < safePreviews.Length; i++)
        {
            PrototypeRpgMemberProgressPreview preview = safePreviews[i];
            if (preview == null)
            {
                continue;
            }

            string summary = BuildRpgGrowthPathCandidateSummaryText(preview.GrowthPathCandidates);
            if (string.IsNullOrEmpty(summary) || summary == "None")
            {
                continue;
            }

            string display = (string.IsNullOrEmpty(preview.DisplayName) ? "Member" : preview.DisplayName) + " -> " + summary;
            if (parts.Count < 3)
            {
                parts.Add(display);
            }
            else
            {
                hiddenCount += 1;
            }
        }

        if (hiddenCount > 0)
        {
            parts.Add("+" + hiddenCount + " more");
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgUpgradeCandidateSummaryText(PrototypeRpgMemberProgressPreview[] previews)
    {
        PrototypeRpgMemberProgressPreview[] safePreviews = previews ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        List<string> parts = new List<string>();
        int hiddenCount = 0;
        for (int i = 0; i < safePreviews.Length; i++)
        {
            PrototypeRpgMemberProgressPreview preview = safePreviews[i];
            if (preview == null)
            {
                continue;
            }

            string summary = !string.IsNullOrEmpty(preview.UpgradeCandidateSummaryText)
                ? preview.UpgradeCandidateSummaryText
                : BuildRpgUpgradeCandidateSummaryText(preview.UpgradeCandidates);
            if (string.IsNullOrEmpty(summary) || summary == "None")
            {
                continue;
            }

            string display = (string.IsNullOrEmpty(preview.DisplayName) ? "Member" : preview.DisplayName) + " -> " + summary;
            if (parts.Count < 3)
            {
                parts.Add(display);
            }
            else
            {
                hiddenCount += 1;
            }
        }

        if (hiddenCount > 0)
        {
            parts.Add("+" + hiddenCount + " more");
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string GetActivePostRunPartyId()
    {
        if (_activeDungeonParty != null && !string.IsNullOrEmpty(_activeDungeonParty.PartyId))
        {
            return _activeDungeonParty.PartyId;
        }

        return string.Empty;
    }

    private PrototypeRpgGrowthChoiceContext BuildRpgGrowthChoiceContext(
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgMemberProgressPreview preview,
        string partyId)
    {
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgMemberProgressPreview safePreview = preview ?? new PrototypeRpgMemberProgressPreview();
        PrototypeRpgMemberContributionSnapshot contribution = safePreview.Contribution ?? new PrototypeRpgMemberContributionSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = safeRunResult.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeRunResult.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = safeRunResult.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();

        PrototypeRpgGrowthChoiceContext context = new PrototypeRpgGrowthChoiceContext();
        context.RunIdentity = string.IsNullOrEmpty(safeRunResult.RunIdentity) ? string.Empty : safeRunResult.RunIdentity;
        context.ResultStateKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? string.Empty : safeRunResult.ResultStateKey;
        context.PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
        context.MemberId = string.IsNullOrEmpty(safePreview.MemberId) ? string.Empty : safePreview.MemberId;
        context.DisplayName = string.IsNullOrEmpty(safePreview.DisplayName) ? "Member" : safePreview.DisplayName;
        context.RoleTag = string.IsNullOrEmpty(safePreview.RoleTag) ? string.Empty : safePreview.RoleTag;
        context.RoleLabel = string.IsNullOrEmpty(safePreview.RoleLabel) ? "Adventurer" : safePreview.RoleLabel;
        context.GrowthTrackId = string.IsNullOrEmpty(safePreview.GrowthTrackId) ? string.Empty : safePreview.GrowthTrackId;
        context.JobId = string.IsNullOrEmpty(safePreview.JobId) ? string.Empty : safePreview.JobId;
        context.EquipmentLoadoutId = string.IsNullOrEmpty(safePreview.EquipmentLoadoutId) ? string.Empty : safePreview.EquipmentLoadoutId;
        context.SkillLoadoutId = string.IsNullOrEmpty(safePreview.SkillLoadoutId) ? string.Empty : safePreview.SkillLoadoutId;
        context.HasAppliedProgress = safePreview.HasAppliedProgress;
        context.CurrentIdentitySummaryText = string.IsNullOrEmpty(safePreview.AppliedProgressSummaryText) ? string.Empty : safePreview.AppliedProgressSummaryText;
        context.RouteId = string.IsNullOrEmpty(safeRunResult.RouteId) ? string.Empty : safeRunResult.RouteId;
        context.RouteLabel = string.IsNullOrEmpty(safeRunResult.RouteLabel) ? string.Empty : safeRunResult.RouteLabel;
        context.EventChoice = string.IsNullOrEmpty(encounterOutcome.SelectedEventChoice) ? string.Empty : encounterOutcome.SelectedEventChoice;
        context.DamageDealt = Mathf.Max(0, contribution.DamageDealt);
        context.DamageTaken = Mathf.Max(0, contribution.DamageTaken);
        context.HealingDone = Mathf.Max(0, contribution.HealingDone);
        context.ActionCount = Mathf.Max(0, contribution.ActionCount);
        context.KillCount = Mathf.Max(0, contribution.KillCount);
        context.Survived = contribution.Survived;
        context.KnockedOut = contribution.KnockedOut;
        context.EliteDefeated = eliteOutcome.IsEliteDefeated;
        context.EliteVictor = contribution.EliteVictor;
        context.RiskyRoute = safeRunResult.RouteId == RiskyRouteId;
        context.HasCarryover = lootOutcome.TotalReturnedAmount > 0 || lootOutcome.TotalLootGained > 0 || !string.IsNullOrEmpty(lootOutcome.CarryoverHintText);
        context.LostPendingReward = lootOutcome.PendingBonusRewardLostAmount > 0;
        context.ReasonText = BuildRpgContributionReasonText(contribution, safeRunResult);

        PrototypeRpgAppliedPartyProgressState appliedState = GetLatestAppliedPartyProgressStateInternal();
        if (DoesRpgAppliedStateMatchParty(appliedState, partyId))
        {
            PrototypeRpgAppliedPartyMemberProgressState memberState = FindRpgAppliedPartyMemberProgressState(appliedState, context.MemberId);
            if (memberState != null && memberState.HasAnyOverride)
            {
                context.HasAppliedProgress = true;
                context.RecentAppliedOfferId = string.IsNullOrEmpty(memberState.RecentAppliedOfferId) ? string.Empty : memberState.RecentAppliedOfferId;
                context.RecentAppliedOfferType = string.IsNullOrEmpty(memberState.RecentAppliedOfferType) ? string.Empty : memberState.RecentAppliedOfferType;
                context.RecentAppliedSummaryText = string.IsNullOrEmpty(memberState.RecentAppliedSummaryText) ? string.Empty : memberState.RecentAppliedSummaryText;
                context.AppliedLastRunIdentity = !string.IsNullOrEmpty(memberState.LastAppliedRunIdentity) ? memberState.LastAppliedRunIdentity : appliedState.LastAppliedRunIdentity;
                context.CurrentIdentitySummaryText = !string.IsNullOrEmpty(memberState.RecentAppliedSummaryText) ? memberState.RecentAppliedSummaryText : context.CurrentIdentitySummaryText;
                if (!string.IsNullOrEmpty(memberState.AppliedRoleLabel)) { context.RoleLabel = memberState.AppliedRoleLabel; }
                if (!string.IsNullOrEmpty(memberState.AppliedGrowthTrackId)) { context.GrowthTrackId = memberState.AppliedGrowthTrackId; }
                if (!string.IsNullOrEmpty(memberState.AppliedJobId)) { context.JobId = memberState.AppliedJobId; }
                if (!string.IsNullOrEmpty(memberState.AppliedEquipmentLoadoutId)) { context.EquipmentLoadoutId = memberState.AppliedEquipmentLoadoutId; }
                if (!string.IsNullOrEmpty(memberState.AppliedSkillLoadoutId)) { context.SkillLoadoutId = memberState.AppliedSkillLoadoutId; }
            }
        }

        context.TriggerKind = ResolveRpgGrowthChoiceTriggerKind(context);
        return context;
    }

    private bool DoesRpgAppliedStateMatchParty(PrototypeRpgAppliedPartyProgressState appliedState, string partyId)
    {
        return appliedState != null && (string.IsNullOrEmpty(partyId) || string.IsNullOrEmpty(appliedState.PartyId) || appliedState.PartyId == partyId);
    }

    private string ResolveRpgCurrentOfferIdentity(string offerType, PrototypeRpgGrowthChoiceContext context)
    {
        PrototypeRpgGrowthChoiceContext safeContext = context ?? new PrototypeRpgGrowthChoiceContext();
        switch (offerType)
        {
            case PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack: return string.IsNullOrEmpty(safeContext.GrowthTrackId) ? string.Empty : safeContext.GrowthTrackId;
            case PrototypeRpgUpgradeOfferTypeKeys.Job: return string.IsNullOrEmpty(safeContext.JobId) ? string.Empty : safeContext.JobId;
            case PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout: return string.IsNullOrEmpty(safeContext.EquipmentLoadoutId) ? string.Empty : safeContext.EquipmentLoadoutId;
            case PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout: return string.IsNullOrEmpty(safeContext.SkillLoadoutId) ? string.Empty : safeContext.SkillLoadoutId;
            default: return string.Empty;
        }
    }

    private bool IsRpgOfferEquivalentToCurrentIdentity(string offerType, string targetId, PrototypeRpgGrowthChoiceContext context)
    {
        string normalizedTarget = NormalizeRpgProgressStateKey(targetId);
        string normalizedCurrent = NormalizeRpgProgressStateKey(ResolveRpgCurrentOfferIdentity(offerType, context));
        return !string.IsNullOrEmpty(normalizedTarget) && normalizedTarget == normalizedCurrent;
    }

    private string BuildRpgContinuityOfferTargetId(string offerType, PrototypeRpgGrowthChoiceContext context)
    {
        PrototypeRpgGrowthChoiceContext safeContext = context ?? new PrototypeRpgGrowthChoiceContext();
        string currentIdentity = NormalizeRpgProgressStateKey(ResolveRpgCurrentOfferIdentity(offerType, safeContext));
        string roleTag = NormalizeRpgProgressStateKey(safeContext.RoleTag);
        switch (offerType)
        {
            case PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack:
                if (currentIdentity.Contains("frontline") && currentIdentity.Contains("mastery")) { return "growth_frontline_vanguard_heroic"; }
                if (currentIdentity.Contains("frontline")) { return currentIdentity.Contains("vanguard") ? "growth_frontline_vanguard_mastery" : "growth_frontline_vanguard"; }
                if (currentIdentity.Contains("precision") && currentIdentity.Contains("mastery")) { return "growth_precision_execution_deadeye"; }
                if (currentIdentity.Contains("precision")) { return currentIdentity.Contains("execution") ? "growth_precision_execution_mastery" : "growth_precision_execution"; }
                if (currentIdentity.Contains("arcane") && currentIdentity.Contains("mastery")) { return "growth_arcane_overchannel_apex"; }
                if (currentIdentity.Contains("arcane")) { return currentIdentity.Contains("overchannel") ? "growth_arcane_overchannel_mastery" : "growth_arcane_overchannel"; }
                if (currentIdentity.Contains("support") && currentIdentity.Contains("mastery")) { return "growth_support_sanctuary_apex"; }
                if (currentIdentity.Contains("support")) { return currentIdentity.Contains("sanctuary") ? "growth_support_sanctuary_mastery" : "growth_support_sanctuary"; }
                switch (roleTag)
                {
                    case "warrior": return "growth_frontline_vanguard";
                    case "rogue": return "growth_precision_execution";
                    case "mage": return "growth_arcane_overchannel";
                    case "cleric": return "growth_support_sanctuary";
                    default: return "growth_generalist_field";
                }
            case PrototypeRpgUpgradeOfferTypeKeys.Job:
                if (currentIdentity.Contains("vanguard") && currentIdentity.Contains("captain")) { return "job_warrior_vanguard_warlord"; }
                if (currentIdentity.Contains("vanguard")) { return "job_warrior_vanguard_captain"; }
                if (currentIdentity.Contains("shadow") && currentIdentity.Contains("assassin")) { return "job_rogue_shadowblade_phantom"; }
                if (currentIdentity.Contains("shadow")) { return "job_rogue_shadowblade_assassin"; }
                if (currentIdentity.Contains("spellweaver") && currentIdentity.Contains("archon")) { return "job_mage_spellweaver_paragon"; }
                if (currentIdentity.Contains("spellweaver")) { return "job_mage_spellweaver_archon"; }
                if (currentIdentity.Contains("sanctifier") && currentIdentity.Contains("hierophant")) { return "job_cleric_sanctifier_paragon"; }
                if (currentIdentity.Contains("sanctifier")) { return "job_cleric_sanctifier_hierophant"; }
                switch (roleTag)
                {
                    case "warrior": return "job_warrior_vanguard";
                    case "rogue": return "job_rogue_shadowblade";
                    case "mage": return "job_mage_spellweaver";
                    case "cleric": return "job_cleric_sanctifier";
                    default: return "job_adventurer_specialist";
                }
            case PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout:
                if ((currentIdentity.Contains("vanguard") || currentIdentity.Contains("bulwark")) && currentIdentity.Contains("bastion")) { return "equip_warrior_bastion_plate_set"; }
                if (currentIdentity.Contains("vanguard") || currentIdentity.Contains("bulwark")) { return "equip_warrior_vanguard_bastion_set"; }
                if ((currentIdentity.Contains("execution") || currentIdentity.Contains("precision")) && currentIdentity.Contains("edge")) { return "equip_rogue_execution_edge_mk2"; }
                if (currentIdentity.Contains("execution") || currentIdentity.Contains("precision")) { return "equip_rogue_execution_edge_set"; }
                if ((currentIdentity.Contains("overchannel") || currentIdentity.Contains("focus")) && currentIdentity.Contains("surge")) { return "equip_mage_overchannel_surge_set"; }
                if (currentIdentity.Contains("overchannel") || currentIdentity.Contains("focus")) { return "equip_mage_overchannel_focus_set"; }
                if ((currentIdentity.Contains("sanctuary") || currentIdentity.Contains("ward")) && currentIdentity.Contains("bulwark")) { return "equip_cleric_sanctuary_bulwark_set"; }
                if (currentIdentity.Contains("sanctuary") || currentIdentity.Contains("ward")) { return "equip_cleric_sanctuary_ward_set"; }
                switch (roleTag)
                {
                    case "warrior": return "equip_warrior_vanguard_set";
                    case "rogue": return "equip_rogue_execution_set";
                    case "mage": return "equip_mage_overchannel_set";
                    case "cleric": return "equip_cleric_sanctuary_set";
                    default: return "equip_adventurer_field_set";
                }
            case PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout:
                if (currentIdentity.Contains("crushing") && currentIdentity.Contains("combo")) { return "skillloadout_warrior_crushing_mastery"; }
                if (currentIdentity.Contains("crushing")) { return "skillloadout_warrior_crushing_combo"; }
                if (currentIdentity.Contains("finish") && currentIdentity.Contains("chain")) { return "skillloadout_rogue_finish_mastery"; }
                if (currentIdentity.Contains("finish")) { return "skillloadout_rogue_finish_chain"; }
                if (currentIdentity.Contains("overchannel") && currentIdentity.Contains("surge")) { return "skillloadout_mage_overchannel_mastery"; }
                if (currentIdentity.Contains("overchannel")) { return "skillloadout_mage_overchannel_surge"; }
                if (currentIdentity.Contains("benediction") && currentIdentity.Contains("echo")) { return "skillloadout_cleric_benediction_mastery"; }
                if (currentIdentity.Contains("benediction")) { return "skillloadout_cleric_benediction_echo"; }
                switch (roleTag)
                {
                    case "warrior": return "skillloadout_warrior_crushing";
                    case "rogue": return "skillloadout_rogue_finish";
                    case "mage": return "skillloadout_mage_overchannel";
                    case "cleric": return "skillloadout_cleric_benediction";
                    default: return "skillloadout_adventurer_field";
                }
            default:
                return string.Empty;
        }
    }

    private string ResolveRpgContinuityOfferTargetId(string offerType, string targetId, PrototypeRpgGrowthChoiceContext context)
    {
        string resolvedTarget = string.IsNullOrEmpty(targetId) ? string.Empty : targetId;
        if (string.IsNullOrEmpty(offerType))
        {
            return resolvedTarget;
        }

        if (IsRpgOfferEquivalentToCurrentIdentity(offerType, resolvedTarget, context))
        {
            string continuityTarget = BuildRpgContinuityOfferTargetId(offerType, context);
            if (!string.IsNullOrEmpty(continuityTarget))
            {
                return continuityTarget;
            }
        }

        return resolvedTarget;
    }

    private string BuildRpgOfferContinuityReasonText(string offerType, string originalTargetId, string refreshedTargetId, PrototypeRpgGrowthChoiceContext context)
    {
        PrototypeRpgGrowthChoiceContext safeContext = context ?? new PrototypeRpgGrowthChoiceContext();
        string normalizedOriginal = NormalizeRpgProgressStateKey(originalTargetId);
        string normalizedRefreshed = NormalizeRpgProgressStateKey(refreshedTargetId);
        if (!string.IsNullOrEmpty(normalizedOriginal) && !string.IsNullOrEmpty(normalizedRefreshed) && normalizedOriginal != normalizedRefreshed)
        {
            string appliedSummary = !string.IsNullOrEmpty(safeContext.CurrentIdentitySummaryText)
                ? safeContext.CurrentIdentitySummaryText
                : safeContext.RecentAppliedSummaryText;
            string reason = "Refreshed from current applied state";
            if (!string.IsNullOrEmpty(appliedSummary))
            {
                reason += ": " + appliedSummary;
            }

            return reason + ".";
        }

        if (safeContext.HasAppliedProgress && IsRpgOfferEquivalentToCurrentIdentity(offerType, refreshedTargetId, safeContext))
        {
            return "No stronger alternative offer was available beyond the current applied identity.";
        }

        return string.Empty;
    }

    private string BuildRpgOfferGenerationBasisSummaryText(PrototypeRpgAppliedPartyProgressState appliedState, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        PrototypeRpgUpgradeOfferCandidate[] safeOffers = offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        string appliedSummary = appliedState != null && appliedState.HasAppliedProgress && !string.IsNullOrEmpty(appliedState.SummaryText)
            ? appliedState.SummaryText
            : string.Empty;
        if (!string.IsNullOrEmpty(appliedSummary) && safeOffers.Length > 0)
        {
            return "Offer refresh basis: " + appliedSummary;
        }

        if (!string.IsNullOrEmpty(appliedSummary))
        {
            return "Applied basis: " + appliedSummary;
        }

        return safeOffers.Length > 0 ? "Offer refresh basis: base party state." : string.Empty;
    }

    private string ResolveRpgGrowthChoiceTriggerKind(PrototypeRpgGrowthChoiceContext context)
    {
        PrototypeRpgGrowthChoiceContext safeContext = context ?? new PrototypeRpgGrowthChoiceContext();
        if (safeContext.KnockedOut || !safeContext.Survived || safeContext.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat)
        {
            return PrototypeRpgGrowthChoiceTriggerKeys.Recovery;
        }

        if (safeContext.HealingDone > 0 && safeContext.HealingDone >= safeContext.DamageDealt)
        {
            return PrototypeRpgGrowthChoiceTriggerKeys.Support;
        }

        if (safeContext.DamageTaken > 0 && safeContext.DamageTaken >= safeContext.DamageDealt)
        {
            return PrototypeRpgGrowthChoiceTriggerKeys.Sustain;
        }

        if (safeContext.EliteVictor || safeContext.EliteDefeated)
        {
            return PrototypeRpgGrowthChoiceTriggerKeys.Elite;
        }

        if (safeContext.LostPendingReward || safeContext.HasCarryover)
        {
            return PrototypeRpgGrowthChoiceTriggerKeys.Carryover;
        }

        if (safeContext.RiskyRoute)
        {
            return PrototypeRpgGrowthChoiceTriggerKeys.Risk;
        }

        if (safeContext.KillCount > 0 || safeContext.DamageDealt > 0)
        {
            return PrototypeRpgGrowthChoiceTriggerKeys.Offense;
        }

        return PrototypeRpgGrowthChoiceTriggerKeys.Always;
    }

    private bool DoesRpgGrowthChoiceRuleMatch(PrototypeRpgGrowthChoiceRule rule, PrototypeRpgGrowthChoiceContext context)
    {
        if (rule == null || context == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(rule.PartyRoleTag) && !string.Equals(rule.PartyRoleTag, context.RoleTag, System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrEmpty(rule.TriggerKind) || rule.TriggerKind == PrototypeRpgGrowthChoiceTriggerKeys.Always)
        {
            return true;
        }

        switch (rule.TriggerKind)
        {
            case PrototypeRpgGrowthChoiceTriggerKeys.Offense:
                return context.KillCount > 0 || context.DamageDealt > 0;
            case PrototypeRpgGrowthChoiceTriggerKeys.Sustain:
                return context.DamageTaken > 0 && context.DamageTaken >= context.DamageDealt;
            case PrototypeRpgGrowthChoiceTriggerKeys.Support:
                return context.HealingDone > 0 && context.HealingDone >= context.DamageDealt;
            case PrototypeRpgGrowthChoiceTriggerKeys.Recovery:
                return context.KnockedOut || !context.Survived || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat;
            case PrototypeRpgGrowthChoiceTriggerKeys.Elite:
                return context.EliteVictor || context.EliteDefeated;
            case PrototypeRpgGrowthChoiceTriggerKeys.Risk:
                return context.RiskyRoute;
            case PrototypeRpgGrowthChoiceTriggerKeys.Carryover:
                return context.HasCarryover || context.LostPendingReward;
            default:
                return string.Equals(rule.TriggerKind, context.TriggerKind, System.StringComparison.OrdinalIgnoreCase);
        }
    }

    private string ResolveRpgFallbackOfferType(PrototypeRpgGrowthChoiceContext context)
    {
        if (context == null)
        {
            return PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack;
        }

        if (context.KnockedOut || !context.Survived)
        {
            return PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout;
        }

        if (context.HealingDone > 0 && context.HealingDone >= context.DamageDealt)
        {
            return PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout;
        }

        if (context.DamageTaken > context.DamageDealt)
        {
            return PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout;
        }

        if (context.RoleTag == "rogue" || context.RoleTag == "mage")
        {
            return PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout;
        }

        if (context.EliteVictor || context.EliteDefeated)
        {
            return PrototypeRpgUpgradeOfferTypeKeys.Job;
        }

        return PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack;
    }

    private string[] BuildRpgOfferTypesForRule(PrototypeRpgGrowthChoiceRule rule, PrototypeRpgGrowthChoiceContext context)
    {
        List<string> offerTypes = new List<string>();
        if (rule != null)
        {
            if (!string.IsNullOrEmpty(rule.GrowthTrackId))
            {
                offerTypes.Add(PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack);
            }

            if (!string.IsNullOrEmpty(rule.JobId))
            {
                offerTypes.Add(PrototypeRpgUpgradeOfferTypeKeys.Job);
            }

            if (!string.IsNullOrEmpty(rule.EquipmentLoadoutId))
            {
                offerTypes.Add(PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout);
            }

            if (!string.IsNullOrEmpty(rule.SkillLoadoutId))
            {
                offerTypes.Add(PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout);
            }
        }

        if (offerTypes.Count <= 0)
        {
            offerTypes.Add(ResolveRpgFallbackOfferType(context));
        }

        return offerTypes.ToArray();
    }

    private string GetRpgUpgradeOfferTargetKey(string offerType, PrototypeRpgGrowthChoiceContext context)
    {
        PrototypeRpgGrowthChoiceContext safeContext = context ?? new PrototypeRpgGrowthChoiceContext();
        switch (offerType)
        {
            case PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack:
                if (!string.IsNullOrEmpty(safeContext.GrowthTrackId))
                {
                    return safeContext.GrowthTrackId;
                }

                switch (safeContext.RoleTag)
                {
                    case "warrior": return "growth_frontline";
                    case "rogue": return "growth_precision";
                    case "mage": return "growth_arcane";
                    case "cleric": return "growth_support";
                    default: return "growth_generalist";
                }
            case PrototypeRpgUpgradeOfferTypeKeys.Job:
                if (!string.IsNullOrEmpty(safeContext.JobId))
                {
                    return safeContext.JobId;
                }

                switch (safeContext.RoleTag)
                {
                    case "warrior": return "job_warrior_novice";
                    case "rogue": return "job_rogue_novice";
                    case "mage": return "job_mage_novice";
                    case "cleric": return "job_cleric_novice";
                    default: return "job_adventurer_novice";
                }
            case PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout:
                if (!string.IsNullOrEmpty(safeContext.EquipmentLoadoutId))
                {
                    return safeContext.EquipmentLoadoutId;
                }

                switch (safeContext.RoleTag)
                {
                    case "warrior": return "equip_warrior_placeholder";
                    case "rogue": return "equip_rogue_placeholder";
                    case "mage": return "equip_mage_placeholder";
                    case "cleric": return "equip_cleric_placeholder";
                    default: return "equip_adventurer_placeholder";
                }
            case PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout:
                if (!string.IsNullOrEmpty(safeContext.SkillLoadoutId))
                {
                    return safeContext.SkillLoadoutId;
                }

                switch (safeContext.RoleTag)
                {
                    case "warrior": return "skillloadout_warrior_placeholder";
                    case "rogue": return "skillloadout_rogue_placeholder";
                    case "mage": return "skillloadout_mage_placeholder";
                    case "cleric": return "skillloadout_cleric_placeholder";
                    default: return "skillloadout_adventurer_placeholder";
                }
            default:
                return string.Empty;
        }
    }

    private string ResolveRpgGrowthChoiceTargetId(PrototypeRpgGrowthChoiceRule rule, PrototypeRpgGrowthChoiceContext context, string offerType)
    {
        if (rule != null)
        {
            switch (offerType)
            {
                case PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack:
                    if (!string.IsNullOrEmpty(rule.GrowthTrackId))
                    {
                        return rule.GrowthTrackId;
                    }
                    break;
                case PrototypeRpgUpgradeOfferTypeKeys.Job:
                    if (!string.IsNullOrEmpty(rule.JobId))
                    {
                        return rule.JobId;
                    }
                    break;
                case PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout:
                    if (!string.IsNullOrEmpty(rule.EquipmentLoadoutId))
                    {
                        return rule.EquipmentLoadoutId;
                    }
                    break;
                case PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout:
                    if (!string.IsNullOrEmpty(rule.SkillLoadoutId))
                    {
                        return rule.SkillLoadoutId;
                    }
                    break;
            }
        }

        return GetRpgUpgradeOfferTargetKey(offerType, context);
    }

    private string BuildRpgUpgradeOfferLabel(string offerType, string targetId, PrototypeRpgGrowthChoiceContext context)
    {
        string baseLabel = BuildRpgNormalizedHookLabel(targetId, "Offer");
        switch (offerType)
        {
            case PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack:
                return "Growth Track: " + baseLabel;
            case PrototypeRpgUpgradeOfferTypeKeys.Job:
                return "Job Path: " + baseLabel;
            case PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout:
                return "Equipment Loadout: " + baseLabel;
            case PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout:
                return "Skill Loadout: " + baseLabel;
            default:
                return (context != null && !string.IsNullOrEmpty(context.DisplayName) ? context.DisplayName + " " : string.Empty) + baseLabel;
        }
    }

    private string BuildRpgGrowthChoiceReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgGrowthChoiceRule rule)
    {
        string contributionReason = context != null && !string.IsNullOrEmpty(context.ReasonText)
            ? context.ReasonText
            : "Run data left the next upgrade step open.";
        string displayHint = rule != null && !string.IsNullOrEmpty(rule.DisplayHint)
            ? rule.DisplayHint
            : string.Empty;
        if (string.IsNullOrEmpty(displayHint))
        {
            return contributionReason;
        }

        return contributionReason + " " + displayHint;
    }

    private string BuildRpgUpgradeOfferEffectPreviewText(string offerType, string targetId, PrototypeRpgGrowthChoiceContext context)
    {
        string label = BuildRpgNormalizedHookLabel(targetId, "Offer");
        string displayName = context != null && !string.IsNullOrEmpty(context.DisplayName) ? context.DisplayName : "Member";
        switch (offerType)
        {
            case PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack:
                return label + " would steer " + displayName + " into a clearer growth lane.";
            case PrototypeRpgUpgradeOfferTypeKeys.Job:
                return label + " would preview a stronger role path for " + displayName + ".";
            case PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout:
                return label + " would reinforce the next equipment package for " + displayName + ".";
            case PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout:
                return label + " would preview a sharper skill package for " + displayName + ".";
            default:
                return label + " would change the next progression choice for " + displayName + ".";
        }
    }

    private int ResolveRpgGrowthChoicePriority(PrototypeRpgGrowthChoiceRule rule, PrototypeRpgGrowthChoiceContext context, string offerType)
    {
        int priority = rule != null ? Mathf.Max(0, rule.Priority) : 100;
        if (context == null)
        {
            return priority;
        }

        if (context.EliteVictor || context.EliteDefeated)
        {
            priority += offerType == PrototypeRpgUpgradeOfferTypeKeys.Job ? 24 : 12;
        }

        if (context.KnockedOut || !context.Survived)
        {
            priority += offerType == PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout ? 28 : 10;
        }

        if (context.HealingDone > 0 && context.HealingDone >= context.DamageDealt && offerType == PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout)
        {
            priority += 18;
        }

        if ((context.KillCount > 0 || context.DamageDealt >= 8) && (offerType == PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout || offerType == PrototypeRpgUpgradeOfferTypeKeys.Job))
        {
            priority += 14;
        }

        if (context.DamageTaken > context.DamageDealt && offerType == PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout)
        {
            priority += 16;
        }

        if ((context.HasCarryover || context.LostPendingReward) && offerType == PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout)
        {
            priority += 8;
        }

        if (context.RiskyRoute && offerType == PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout)
        {
            priority += 6;
        }

        return priority;
    }

    private string BuildRpgUpgradeOfferLockReason(PrototypeRpgGrowthChoiceContext context, string offerType, string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            return "Missing resolved target for this upgrade offer.";
        }

        if (context == null)
        {
            return string.Empty;
        }

        if (context.KnockedOut)
        {
            return "Recover from KO before finalizing this upgrade.";
        }

        if (!context.Survived && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat)
        {
            return "Defeat result keeps this offer preview-only for now.";
        }

        return string.Empty;
    }

    private PrototypeRpgUpgradeOfferDefinition[] BuildRpgUpgradeOfferDefinitions(
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgMemberProgressPreview[] previews,
        string partyId)
    {
        PrototypeRpgMemberProgressPreview[] safePreviews = previews ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        if (safePreviews.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgUpgradeOfferDefinition>();
        }

        PrototypeRpgGrowthChoiceRule[] rules = PrototypeRpgGrowthChoiceRuleCatalog.GetRules();
        List<PrototypeRpgUpgradeOfferDefinition> definitions = new List<PrototypeRpgUpgradeOfferDefinition>();
        for (int i = 0; i < safePreviews.Length; i++)
        {
            PrototypeRpgMemberProgressPreview preview = safePreviews[i] ?? new PrototypeRpgMemberProgressPreview();
            if (string.IsNullOrEmpty(preview.MemberId))
            {
                continue;
            }

            PrototypeRpgGrowthChoiceContext context = BuildRpgGrowthChoiceContext(runResultSnapshot, preview, partyId);
            bool matchedAny = false;
            for (int ruleIndex = 0; ruleIndex < rules.Length; ruleIndex++)
            {
                PrototypeRpgGrowthChoiceRule rule = rules[ruleIndex];
                if (!DoesRpgGrowthChoiceRuleMatch(rule, context))
                {
                    continue;
                }

                string[] offerTypes = BuildRpgOfferTypesForRule(rule, context);
                int addedForRule = 0;
                for (int offerTypeIndex = 0; offerTypeIndex < offerTypes.Length; offerTypeIndex++)
                {
                    if (rule != null && rule.OfferCount > 0 && addedForRule >= rule.OfferCount)
                    {
                        break;
                    }

                    string offerType = offerTypes[offerTypeIndex];
                    string originalTargetId = ResolveRpgGrowthChoiceTargetId(rule, context, offerType);
                    string targetId = ResolveRpgContinuityOfferTargetId(offerType, originalTargetId, context);
                    if (string.IsNullOrEmpty(targetId) || IsRpgOfferEquivalentToCurrentIdentity(offerType, targetId, context))
                    {
                        continue;
                    }

                    string offerId = context.MemberId + ":" + (rule != null && !string.IsNullOrEmpty(rule.RuleId) ? rule.RuleId : "fallback") + ":" + offerType + ":" + targetId;
                    bool alreadyExists = false;
                    for (int existingIndex = 0; existingIndex < definitions.Count; existingIndex++)
                    {
                        PrototypeRpgUpgradeOfferDefinition existing = definitions[existingIndex];
                        if (existing != null && existing.OfferId == offerId)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (alreadyExists)
                    {
                        continue;
                    }

                    PrototypeRpgUpgradeOfferDefinition definition = new PrototypeRpgUpgradeOfferDefinition();
                    definition.OfferId = offerId;
                    definition.MemberId = context.MemberId;
                    definition.DisplayName = context.DisplayName;
                    definition.OfferLabel = BuildRpgUpgradeOfferLabel(offerType, targetId, context);
                    definition.OfferType = string.IsNullOrEmpty(offerType) ? string.Empty : offerType;
                    definition.SourceRuleId = rule != null && !string.IsNullOrEmpty(rule.RuleId) ? rule.RuleId : "fallback";
                    definition.GrowthTrackId = offerType == PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack ? targetId : string.Empty;
                    definition.JobId = offerType == PrototypeRpgUpgradeOfferTypeKeys.Job ? targetId : string.Empty;
                    definition.EquipmentLoadoutId = offerType == PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout ? targetId : string.Empty;
                    definition.SkillLoadoutId = offerType == PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout ? targetId : string.Empty;
                    definition.EffectPreviewText = BuildRpgUpgradeOfferEffectPreviewText(offerType, targetId, context);
                    definition.ReasonText = AppendRpgSummaryText(BuildRpgGrowthChoiceReasonText(context, rule), BuildRpgOfferContinuityReasonText(offerType, originalTargetId, targetId, context));
                    definition.GroupKey = rule != null && !string.IsNullOrEmpty(rule.OfferGroupKey) ? rule.OfferGroupKey : offerType;
                    definition.ExclusionGroupKey = rule != null && !string.IsNullOrEmpty(rule.ExclusionGroupKey) ? rule.ExclusionGroupKey : string.Empty;
                    definition.Priority = ResolveRpgGrowthChoicePriority(rule, context, offerType) + (NormalizeRpgProgressStateKey(originalTargetId) != NormalizeRpgProgressStateKey(targetId) ? 18 : 0);
                    definition.LockReason = BuildRpgUpgradeOfferLockReason(context, offerType, targetId);
                    definition.IsLocked = !string.IsNullOrEmpty(definition.LockReason);
                    definition.IsRecommended = false;
                    definitions.Add(definition);
                    addedForRule += 1;
                    matchedAny = true;
                }
            }

            if (!matchedAny)
            {
                string fallbackOfferType = ResolveRpgFallbackOfferType(context);
                string fallbackTargetId = GetRpgUpgradeOfferTargetKey(fallbackOfferType, context);
                if (!string.IsNullOrEmpty(fallbackTargetId))
                {
                    PrototypeRpgUpgradeOfferDefinition fallbackDefinition = new PrototypeRpgUpgradeOfferDefinition();
                    fallbackDefinition.OfferId = context.MemberId + ":fallback:" + fallbackOfferType + ":" + fallbackTargetId;
                    fallbackDefinition.MemberId = context.MemberId;
                    fallbackDefinition.DisplayName = context.DisplayName;
                    fallbackDefinition.OfferLabel = BuildRpgUpgradeOfferLabel(fallbackOfferType, fallbackTargetId, context);
                    fallbackDefinition.OfferType = fallbackOfferType;
                    fallbackDefinition.SourceRuleId = "fallback";
                    fallbackDefinition.GrowthTrackId = fallbackOfferType == PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack ? fallbackTargetId : string.Empty;
                    fallbackDefinition.JobId = fallbackOfferType == PrototypeRpgUpgradeOfferTypeKeys.Job ? fallbackTargetId : string.Empty;
                    fallbackDefinition.EquipmentLoadoutId = fallbackOfferType == PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout ? fallbackTargetId : string.Empty;
                    fallbackDefinition.SkillLoadoutId = fallbackOfferType == PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout ? fallbackTargetId : string.Empty;
                    fallbackDefinition.EffectPreviewText = BuildRpgUpgradeOfferEffectPreviewText(fallbackOfferType, fallbackTargetId, context);
                    fallbackDefinition.ReasonText = string.IsNullOrEmpty(context.ReasonText) ? "Fallback offer derived from run summary." : context.ReasonText;
                    fallbackDefinition.GroupKey = fallbackOfferType;
                    fallbackDefinition.ExclusionGroupKey = string.Empty;
                    fallbackDefinition.Priority = 120;
                    fallbackDefinition.LockReason = BuildRpgUpgradeOfferLockReason(context, fallbackOfferType, fallbackTargetId);
                    fallbackDefinition.IsLocked = !string.IsNullOrEmpty(fallbackDefinition.LockReason);
                    fallbackDefinition.IsRecommended = false;
                    definitions.Add(fallbackDefinition);
                }
            }
        }

        return definitions.Count > 0 ? definitions.ToArray() : System.Array.Empty<PrototypeRpgUpgradeOfferDefinition>();
    }

    private PrototypeRpgUpgradeOfferCandidate[] BuildRpgUpgradeOfferCandidates(PrototypeRpgUpgradeOfferDefinition[] definitions)
    {
        PrototypeRpgUpgradeOfferDefinition[] safeDefinitions = definitions ?? System.Array.Empty<PrototypeRpgUpgradeOfferDefinition>();
        if (safeDefinitions.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        }

        List<PrototypeRpgUpgradeOfferCandidate> candidates = new List<PrototypeRpgUpgradeOfferCandidate>();
        List<string> memberIds = new List<string>();
        for (int i = 0; i < safeDefinitions.Length; i++)
        {
            PrototypeRpgUpgradeOfferDefinition definition = safeDefinitions[i];
            if (definition == null || string.IsNullOrEmpty(definition.MemberId) || memberIds.Contains(definition.MemberId))
            {
                continue;
            }

            memberIds.Add(definition.MemberId);
        }

        for (int memberIndex = 0; memberIndex < memberIds.Count; memberIndex++)
        {
            string memberId = memberIds[memberIndex];
            List<PrototypeRpgUpgradeOfferDefinition> memberDefinitions = new List<PrototypeRpgUpgradeOfferDefinition>();
            for (int i = 0; i < safeDefinitions.Length; i++)
            {
                PrototypeRpgUpgradeOfferDefinition definition = safeDefinitions[i];
                if (definition != null && definition.MemberId == memberId)
                {
                    memberDefinitions.Add(definition);
                }
            }

            memberDefinitions.Sort(delegate(PrototypeRpgUpgradeOfferDefinition left, PrototypeRpgUpgradeOfferDefinition right)
            {
                int rightPriority = right != null ? right.Priority : 0;
                int leftPriority = left != null ? left.Priority : 0;
                return rightPriority.CompareTo(leftPriority);
            });

            List<string> usedGroups = new List<string>();
            int selectedCount = 0;
            for (int i = 0; i < memberDefinitions.Count; i++)
            {
                if (selectedCount >= 3)
                {
                    break;
                }

                PrototypeRpgUpgradeOfferDefinition definition = memberDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(definition.GroupKey) && usedGroups.Contains(definition.GroupKey))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(definition.ExclusionGroupKey) && usedGroups.Contains(definition.ExclusionGroupKey))
                {
                    continue;
                }

                PrototypeRpgUpgradeOfferCandidate candidate = new PrototypeRpgUpgradeOfferCandidate();
                candidate.OfferId = string.IsNullOrEmpty(definition.OfferId) ? string.Empty : definition.OfferId;
                candidate.MemberId = string.IsNullOrEmpty(definition.MemberId) ? string.Empty : definition.MemberId;
                candidate.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? "Member" : definition.DisplayName;
                candidate.OfferLabel = string.IsNullOrEmpty(definition.OfferLabel) ? "Upgrade Offer" : definition.OfferLabel;
                candidate.OfferType = string.IsNullOrEmpty(definition.OfferType) ? string.Empty : definition.OfferType;
                candidate.SourceRuleId = string.IsNullOrEmpty(definition.SourceRuleId) ? string.Empty : definition.SourceRuleId;
                candidate.GrowthTrackId = string.IsNullOrEmpty(definition.GrowthTrackId) ? string.Empty : definition.GrowthTrackId;
                candidate.JobId = string.IsNullOrEmpty(definition.JobId) ? string.Empty : definition.JobId;
                candidate.EquipmentLoadoutId = string.IsNullOrEmpty(definition.EquipmentLoadoutId) ? string.Empty : definition.EquipmentLoadoutId;
                candidate.SkillLoadoutId = string.IsNullOrEmpty(definition.SkillLoadoutId) ? string.Empty : definition.SkillLoadoutId;
                candidate.EffectPreviewText = string.IsNullOrEmpty(definition.EffectPreviewText) ? string.Empty : definition.EffectPreviewText;
                candidate.ReasonText = string.IsNullOrEmpty(definition.ReasonText) ? string.Empty : definition.ReasonText;
                candidate.IsRecommended = selectedCount == 0;
                candidate.IsLocked = definition.IsLocked;
                candidate.LockReason = string.IsNullOrEmpty(definition.LockReason) ? string.Empty : definition.LockReason;
                candidate.GroupKey = string.IsNullOrEmpty(definition.GroupKey) ? string.Empty : definition.GroupKey;
                candidate.Priority = Mathf.Max(0, definition.Priority);
                candidates.Add(candidate);
                selectedCount += 1;

                if (!string.IsNullOrEmpty(definition.GroupKey))
                {
                    usedGroups.Add(definition.GroupKey);
                }

                if (!string.IsNullOrEmpty(definition.ExclusionGroupKey))
                {
                    usedGroups.Add(definition.ExclusionGroupKey);
                }
            }
        }

        return candidates.Count > 0 ? candidates.ToArray() : System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
    }

    private PrototypeRpgApplyReadyUpgradeChoice BuildRpgApplyReadyUpgradeChoice(
        string runIdentity,
        string partyId,
        string memberId,
        string displayName,
        PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        PrototypeRpgUpgradeOfferCandidate[] safeOffers = offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        PrototypeRpgApplyReadyUpgradeChoice choice = new PrototypeRpgApplyReadyUpgradeChoice();
        choice.ChoiceKey = string.IsNullOrEmpty(memberId) ? string.Empty : memberId + ":apply_ready";
        choice.MemberId = string.IsNullOrEmpty(memberId) ? string.Empty : memberId;
        choice.DisplayName = string.IsNullOrEmpty(displayName) ? "Member" : displayName;
        if (safeOffers.Length <= 0)
        {
            choice.SummaryText = "No apply-ready upgrade choice.";
            return choice;
        }

        PrototypeRpgUpgradeOfferCandidate selectedOffer = null;
        for (int i = 0; i < safeOffers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = safeOffers[i];
            if (offer != null && offer.IsRecommended && !offer.IsLocked)
            {
                selectedOffer = offer;
                break;
            }
        }

        if (selectedOffer == null)
        {
            for (int i = 0; i < safeOffers.Length; i++)
            {
                PrototypeRpgUpgradeOfferCandidate offer = safeOffers[i];
                if (offer != null && !offer.IsLocked)
                {
                    selectedOffer = offer;
                    break;
                }
            }
        }

        if (selectedOffer == null)
        {
            selectedOffer = safeOffers[0];
        }

        choice.OfferLabel = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.OfferLabel) ? selectedOffer.OfferLabel : "Upgrade Offer";
        choice.SelectedOfferId = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.OfferId) ? selectedOffer.OfferId : string.Empty;
        choice.SelectedOfferType = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.OfferType) ? selectedOffer.OfferType : string.Empty;
        choice.PendingApplyKey = string.IsNullOrEmpty(choice.SelectedOfferId) ? string.Empty : "apply_ready:" + choice.SelectedOfferId;
        choice.LockReason = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.LockReason) ? selectedOffer.LockReason : string.Empty;
        choice.IsReady = selectedOffer != null && !selectedOffer.IsLocked;

        choice.Request = new PrototypeRpgUpgradeSelectionRequest();
        choice.Request.RunIdentity = string.IsNullOrEmpty(runIdentity) ? string.Empty : runIdentity;
        choice.Request.PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
        choice.Request.MemberId = choice.MemberId;
        choice.Request.SelectedOfferId = choice.SelectedOfferId;
        choice.Request.SelectedOfferType = choice.SelectedOfferType;
        choice.Request.SourceRuleId = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.SourceRuleId) ? selectedOffer.SourceRuleId : string.Empty;
        choice.Request.PreviewText = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.EffectPreviewText) ? selectedOffer.EffectPreviewText : string.Empty;
        choice.Request.PendingApplyKey = choice.PendingApplyKey;
        choice.Request.WouldAffectGrowthTrackId = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.GrowthTrackId) ? selectedOffer.GrowthTrackId : string.Empty;
        choice.Request.WouldAffectJobId = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.JobId) ? selectedOffer.JobId : string.Empty;
        choice.Request.WouldAffectEquipmentLoadoutId = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.EquipmentLoadoutId) ? selectedOffer.EquipmentLoadoutId : string.Empty;
        choice.Request.WouldAffectSkillLoadoutId = selectedOffer != null && !string.IsNullOrEmpty(selectedOffer.SkillLoadoutId) ? selectedOffer.SkillLoadoutId : string.Empty;

        choice.Preview = new PrototypeRpgUpgradeSelectionPreview();
        choice.Preview.RunIdentity = choice.Request.RunIdentity;
        choice.Preview.MemberId = choice.MemberId;
        choice.Preview.DisplayName = choice.DisplayName;
        choice.Preview.OfferLabel = choice.OfferLabel;
        choice.Preview.SelectedOfferId = choice.SelectedOfferId;
        choice.Preview.SelectedOfferType = choice.SelectedOfferType;
        choice.Preview.SourceRuleId = choice.Request.SourceRuleId;
        choice.Preview.PreviewText = choice.Request.PreviewText;
        choice.Preview.PendingApplyKey = choice.PendingApplyKey;
        choice.Preview.WouldAffectGrowthTrackId = choice.Request.WouldAffectGrowthTrackId;
        choice.Preview.WouldAffectJobId = choice.Request.WouldAffectJobId;
        choice.Preview.WouldAffectEquipmentLoadoutId = choice.Request.WouldAffectEquipmentLoadoutId;
        choice.Preview.WouldAffectSkillLoadoutId = choice.Request.WouldAffectSkillLoadoutId;
        choice.Preview.IsLocked = !choice.IsReady;
        choice.Preview.LockReason = choice.LockReason;

        choice.SummaryText = choice.IsReady
            ? choice.DisplayName + " ready -> " + choice.OfferLabel
            : choice.DisplayName + " preview-only -> " + choice.OfferLabel + (string.IsNullOrEmpty(choice.LockReason) ? string.Empty : " (" + choice.LockReason + ")");
        return choice;
    }

    private PrototypeRpgApplyReadyUpgradeChoice[] BuildRpgApplyReadyUpgradeChoices(
        string runIdentity,
        string partyId,
        PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        PrototypeRpgUpgradeOfferCandidate[] safeOffers = offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        if (safeOffers.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
        }

        List<string> memberIds = new List<string>();
        List<PrototypeRpgApplyReadyUpgradeChoice> choices = new List<PrototypeRpgApplyReadyUpgradeChoice>();
        for (int i = 0; i < safeOffers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = safeOffers[i];
            if (offer == null || string.IsNullOrEmpty(offer.MemberId) || memberIds.Contains(offer.MemberId))
            {
                continue;
            }

            memberIds.Add(offer.MemberId);
            List<PrototypeRpgUpgradeOfferCandidate> memberOffers = new List<PrototypeRpgUpgradeOfferCandidate>();
            string displayName = string.IsNullOrEmpty(offer.DisplayName) ? "Member" : offer.DisplayName;
            for (int offerIndex = 0; offerIndex < safeOffers.Length; offerIndex++)
            {
                PrototypeRpgUpgradeOfferCandidate memberOffer = safeOffers[offerIndex];
                if (memberOffer != null && memberOffer.MemberId == offer.MemberId)
                {
                    memberOffers.Add(memberOffer);
                }
            }

            PrototypeRpgApplyReadyUpgradeChoice choice = BuildRpgApplyReadyUpgradeChoice(runIdentity, partyId, offer.MemberId, displayName, memberOffers.ToArray());
            if (!string.IsNullOrEmpty(choice.MemberId))
            {
                choices.Add(choice);
            }
        }

        return choices.Count > 0 ? choices.ToArray() : System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
    }

    private string BuildRpgMemberUpgradeOfferSummaryText(string memberId, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        if (string.IsNullOrEmpty(memberId))
        {
            return string.Empty;
        }

        PrototypeRpgUpgradeOfferCandidate[] safeOffers = offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        List<string> labels = new List<string>();
        int hiddenCount = 0;
        for (int i = 0; i < safeOffers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = safeOffers[i];
            if (offer == null || offer.MemberId != memberId)
            {
                continue;
            }

            string label = !string.IsNullOrEmpty(offer.OfferLabel) ? offer.OfferLabel : "Upgrade Offer";
            if (labels.Count < 2)
            {
                labels.Add(label);
            }
            else
            {
                hiddenCount += 1;
            }
        }

        if (hiddenCount > 0)
        {
            labels.Add("+" + hiddenCount + " more");
        }

        return labels.Count > 0 ? string.Join(" / ", labels.ToArray()) : string.Empty;
    }

    private string BuildRpgUpgradeOfferSummaryText(PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        PrototypeRpgUpgradeOfferCandidate[] safeOffers = offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        if (safeOffers.Length <= 0)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        int hiddenCount = 0;
        for (int i = 0; i < safeOffers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = safeOffers[i];
            if (offer == null)
            {
                continue;
            }

            string display = (string.IsNullOrEmpty(offer.DisplayName) ? "Member" : offer.DisplayName) + " -> " + (string.IsNullOrEmpty(offer.OfferLabel) ? "Upgrade Offer" : offer.OfferLabel);
            if (parts.Count < 3)
            {
                parts.Add(display);
            }
            else
            {
                hiddenCount += 1;
            }
        }

        if (hiddenCount > 0)
        {
            parts.Add("+" + hiddenCount + " more");
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgMemberApplyReadySummaryText(string memberId, PrototypeRpgApplyReadyUpgradeChoice[] choices)
    {
        if (string.IsNullOrEmpty(memberId))
        {
            return string.Empty;
        }

        PrototypeRpgApplyReadyUpgradeChoice[] safeChoices = choices ?? System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
        for (int i = 0; i < safeChoices.Length; i++)
        {
            PrototypeRpgApplyReadyUpgradeChoice choice = safeChoices[i];
            if (choice == null || choice.MemberId != memberId)
            {
                continue;
            }

            if (choice.IsReady)
            {
                return "Apply-ready " + choice.OfferLabel;
            }

            if (!string.IsNullOrEmpty(choice.OfferLabel))
            {
                return "Apply-ready blocked " + choice.OfferLabel;
            }
        }

        return string.Empty;
    }

    private string BuildRpgApplyReadySummaryText(PrototypeRpgApplyReadyUpgradeChoice[] choices)
    {
        PrototypeRpgApplyReadyUpgradeChoice[] safeChoices = choices ?? System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
        if (safeChoices.Length <= 0)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        int hiddenCount = 0;
        for (int i = 0; i < safeChoices.Length; i++)
        {
            PrototypeRpgApplyReadyUpgradeChoice choice = safeChoices[i];
            if (choice == null || string.IsNullOrEmpty(choice.MemberId))
            {
                continue;
            }

            string display = (string.IsNullOrEmpty(choice.DisplayName) ? "Member" : choice.DisplayName) + " -> " + (string.IsNullOrEmpty(choice.OfferLabel) ? "No offer" : choice.OfferLabel);
            if (!choice.IsReady)
            {
                display += " (preview)";
            }

            if (parts.Count < 3)
            {
                parts.Add(display);
            }
            else
            {
                hiddenCount += 1;
            }
        }

        if (hiddenCount > 0)
        {
            parts.Add("+" + hiddenCount + " more");
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string[] BuildRpgPostRunUpgradeOfferHighlightKeys(
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgUpgradeOfferCandidate[] offers,
        PrototypeRpgApplyReadyUpgradeChoice[] choices)
    {
        List<string> keys = new List<string>();
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgUpgradeOfferCandidate[] safeOffers = offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        PrototypeRpgApplyReadyUpgradeChoice[] safeChoices = choices ?? System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();

        if (!string.IsNullOrEmpty(safeRunResult.ResultStateKey))
        {
            AddProgressionTag(keys, safeRunResult.ResultStateKey);
        }

        if (safeRunResult.EliteOutcome != null && safeRunResult.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(keys, "elite_offer");
        }

        if (safeRunResult.RouteId == RiskyRouteId)
        {
            AddProgressionTag(keys, "risky_route");
        }

        if (safeRunResult.LootOutcome != null && safeRunResult.LootOutcome.PendingBonusRewardLostAmount > 0)
        {
            AddProgressionTag(keys, "pending_reward_lost");
        }

        for (int i = 0; i < safeOffers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = safeOffers[i];
            if (offer == null)
            {
                continue;
            }

            if (offer.IsRecommended)
            {
                AddProgressionTag(keys, "recommended_offer");
            }

            if (!offer.IsLocked)
            {
                AddProgressionTag(keys, "offer_ready");
            }
        }

        for (int i = 0; i < safeChoices.Length; i++)
        {
            PrototypeRpgApplyReadyUpgradeChoice choice = safeChoices[i];
            if (choice == null)
            {
                continue;
            }

            if (choice.IsReady)
            {
                AddProgressionTag(keys, "apply_ready");
            }
            else
            {
                AddProgressionTag(keys, "apply_preview_only");
            }
        }

        return keys.Count > 0 ? keys.ToArray() : System.Array.Empty<string>();
    }

    private PrototypeRpgPostRunUpgradeOfferSurface BuildRpgPostRunUpgradeOfferSurface()
    {
        PrototypeRpgPostRunUpgradeOfferSurface surface = new PrototypeRpgPostRunUpgradeOfferSurface();
        PrototypeRpgRunResultSnapshot runResultSnapshot = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
        PrototypeRpgProgressionPreviewSnapshot previewSnapshot = CopyRpgProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot);
        if (!HasRpgRunResultSnapshotData(runResultSnapshot) || !HasRpgProgressionPreviewSnapshotData(previewSnapshot))
        {
            return surface;
        }

        string partyId = GetActivePostRunPartyId();
        PrototypeRpgUpgradeOfferDefinition[] definitions = BuildRpgUpgradeOfferDefinitions(runResultSnapshot, previewSnapshot.Members, partyId);
        PrototypeRpgUpgradeOfferCandidate[] offers = BuildRpgUpgradeOfferCandidates(definitions);
        PrototypeRpgApplyReadyUpgradeChoice[] choices = BuildRpgApplyReadyUpgradeChoices(runResultSnapshot.RunIdentity, partyId, offers);

        surface.HasOfferSurface = offers.Length > 0 || choices.Length > 0;
        surface.RunIdentity = string.IsNullOrEmpty(runResultSnapshot.RunIdentity) ? string.Empty : runResultSnapshot.RunIdentity;
        surface.ResultStateKey = string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? string.Empty : runResultSnapshot.ResultStateKey;
        surface.PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
        surface.DungeonId = string.IsNullOrEmpty(runResultSnapshot.DungeonId) ? string.Empty : runResultSnapshot.DungeonId;
        surface.DungeonLabel = string.IsNullOrEmpty(runResultSnapshot.DungeonLabel) ? string.Empty : runResultSnapshot.DungeonLabel;
        surface.RouteId = string.IsNullOrEmpty(runResultSnapshot.RouteId) ? string.Empty : runResultSnapshot.RouteId;
        surface.RouteLabel = string.IsNullOrEmpty(runResultSnapshot.RouteLabel) ? string.Empty : runResultSnapshot.RouteLabel;
        surface.SummaryText = BuildRpgUpgradeOfferSummaryText(offers);
        surface.ApplyReadySummaryText = BuildRpgApplyReadySummaryText(choices);
        surface.HighlightKeys = BuildRpgPostRunUpgradeOfferHighlightKeys(runResultSnapshot, offers, choices);
        surface.Offers = offers;
        surface.ApplyReadyChoices = choices;
        return surface;
    }

    private string BuildMemberNextGrowthTrackHint(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgMemberProgressionSeed memberSeed)
    {
        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.GrowthTrackId))
        {
            return "Next track: " + BuildRpgNormalizedHookLabel(memberSeed.GrowthTrackId, "Growth Track");
        }

        if (contribution == null)
        {
            return string.Empty;
        }

        switch (contribution.RoleTag)
        {
            case "warrior":
                if (contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt)
                {
                    return "Next track: Vanguard guard";
                }

                if (contribution.KillCount > 0)
                {
                    return "Next track: Breaker assault";
                }

                return "Next track: Frontline discipline";
            case "rogue":
                return contribution.KillCount > 0 ? "Next track: Execution route" : "Next track: Ambush route";
            case "mage":
                return contribution.DamageDealt >= 8 ? "Next track: Arcane burst route" : "Next track: Focus casting route";
            case "cleric":
                return contribution.HealingDone > 0 ? "Next track: Sanctuary route" : "Next track: Recovery route";
            default:
                return "Next track: Adaptive route";
        }
    }

    private string BuildMemberNextJobHint(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgMemberProgressionSeed memberSeed)
    {
        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.JobId))
        {
            return "Job hint: " + BuildRpgNormalizedHookLabel(memberSeed.JobId, "Job Path");
        }

        if (contribution == null)
        {
            return string.Empty;
        }

        switch (contribution.RoleTag)
        {
            case "warrior":
                return contribution.EliteVictor ? "Job hint: Vanguard" : "Job hint: Shieldbearer";
            case "rogue":
                return contribution.KillCount > 0 ? "Job hint: Duelist" : "Job hint: Scout";
            case "mage":
                return contribution.DamageDealt >= 8 ? "Job hint: Channeler" : "Job hint: Scholar";
            case "cleric":
                return contribution.HealingDone > 0 ? "Job hint: Warden Priest" : "Job hint: Acolyte";
            default:
                return "Job hint: Adventurer";
        }
    }

    private string BuildMemberNextSkillLoadoutHint(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgMemberProgressionSeed memberSeed)
    {
        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.SkillLoadoutId))
        {
            return "Skill hint: " + BuildRpgNormalizedHookLabel(memberSeed.SkillLoadoutId, "Skill Loadout");
        }

        if (contribution == null)
        {
            return string.Empty;
        }

        switch (contribution.DefaultSkillId)
        {
            case "power_strike":
                return "Skill hint: heavier single-target loadout";
            case "weak_point":
                return "Skill hint: finisher loadout";
            case "arcane_burst":
                return "Skill hint: burst spread loadout";
            case "radiant_hymn":
                return "Skill hint: recovery hymn loadout";
            default:
                return string.IsNullOrEmpty(contribution.RoleLabel) ? string.Empty : "Skill hint: " + contribution.RoleLabel + " kit";
        }
    }

    private string BuildMemberNextEquipmentLoadoutHint(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgMemberProgressionSeed memberSeed)
    {
        if (memberSeed != null && !string.IsNullOrEmpty(memberSeed.EquipmentLoadoutId))
        {
            return "Gear hint: " + BuildRpgNormalizedHookLabel(memberSeed.EquipmentLoadoutId, "Equipment Loadout");
        }

        if (contribution == null)
        {
            return string.Empty;
        }

        switch (contribution.RoleTag)
        {
            case "warrior":
                return contribution.DamageTaken > 0 ? "Gear hint: guard armor" : "Gear hint: breaker steel";
            case "rogue":
                return contribution.KillCount > 0 ? "Gear hint: precision blades" : "Gear hint: scout kit";
            case "mage":
                return "Gear hint: focus catalyst";
            case "cleric":
                return contribution.HealingDone > 0 ? "Gear hint: relief charms" : "Gear hint: warding robes";
            default:
                return "Gear hint: field kit";
        }
    }

    private string BuildMemberProgressionPreviewText(PrototypeRpgMemberProgressPreview preview)
    {
        if (preview == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(preview.GrowthDirectionLabel))
        {
            parts.Add(preview.GrowthDirectionLabel);
        }

        string growthSummary = BuildRpgGrowthPathCandidateSummaryText(preview.GrowthPathCandidates);
        if (!string.IsNullOrEmpty(growthSummary) && growthSummary != "None")
        {
            parts.Add("Growth " + growthSummary);
        }

        string upgradeSummary = !string.IsNullOrEmpty(preview.UpgradeCandidateSummaryText)
            ? preview.UpgradeCandidateSummaryText
            : BuildRpgUpgradeCandidateSummaryText(preview.UpgradeCandidates);
        if (!string.IsNullOrEmpty(upgradeSummary) && upgradeSummary != "None")
        {
            parts.Add("Upgrade " + upgradeSummary);
        }

        if (!string.IsNullOrEmpty(preview.UpgradeOfferSummaryText) && preview.UpgradeOfferSummaryText != "None")
        {
            parts.Add("Offer " + preview.UpgradeOfferSummaryText);
        }

        if (!string.IsNullOrEmpty(preview.ApplyReadySummaryText) && preview.ApplyReadySummaryText != "None")
        {
            parts.Add(preview.ApplyReadySummaryText);
        }

        if (!string.IsNullOrEmpty(preview.RewardCarryoverHintText))
        {
            parts.Add(preview.RewardCarryoverHintText);
        }

        if (!string.IsNullOrEmpty(preview.NotableOutcomeKey))
        {
            parts.Add(preview.NotableOutcomeKey);
        }

        if (!string.IsNullOrEmpty(preview.NextGrowthTrackHint))
        {
            parts.Add(preview.NextGrowthTrackHint);
        }

        if (!string.IsNullOrEmpty(preview.NextJobHint))
        {
            parts.Add(preview.NextJobHint);
        }

        if (!string.IsNullOrEmpty(preview.NextSkillLoadoutHint))
        {
            parts.Add(preview.NextSkillLoadoutHint);
        }

        if (!string.IsNullOrEmpty(preview.NextEquipmentLoadoutHint))
        {
            parts.Add(preview.NextEquipmentLoadoutHint);
        }

        if (preview.SuggestedRewardHintTags != null && preview.SuggestedRewardHintTags.Length > 0)
        {
            parts.Add("Reward " + BuildRpgTagListText(preview.SuggestedRewardHintTags));
        }

        if (preview.SuggestedGrowthHintTags != null && preview.SuggestedGrowthHintTags.Length > 0)
        {
            parts.Add("Growth " + BuildRpgTagListText(preview.SuggestedGrowthHintTags));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private void AddUnlockSeed(
        List<PrototypeRpgUnlockSeedSnapshot> seeds,
        string sourceRunIdentity,
        string sourceEncounterId,
        string dungeonId,
        string routeId,
        string eliteId,
        string unlockCategoryKey,
        string unlockTargetKey,
        string unlockReasonText,
        string progressionDependencyHint)
    {
        if (seeds == null || string.IsNullOrEmpty(unlockCategoryKey) || string.IsNullOrEmpty(unlockTargetKey))
        {
            return;
        }

        for (int i = 0; i < seeds.Count; i++)
        {
            PrototypeRpgUnlockSeedSnapshot existing = seeds[i];
            if (existing != null && existing.UnlockCategoryKey == unlockCategoryKey && existing.UnlockTargetKey == unlockTargetKey)
            {
                return;
            }
        }

        PrototypeRpgUnlockSeedSnapshot seed = new PrototypeRpgUnlockSeedSnapshot();
        seed.SourceRunIdentity = string.IsNullOrEmpty(sourceRunIdentity) ? string.Empty : sourceRunIdentity;
        seed.SourceEncounterId = string.IsNullOrEmpty(sourceEncounterId) ? string.Empty : sourceEncounterId;
        seed.DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
        seed.RouteId = string.IsNullOrEmpty(routeId) ? string.Empty : routeId;
        seed.EliteId = string.IsNullOrEmpty(eliteId) ? string.Empty : eliteId;
        seed.UnlockCategoryKey = unlockCategoryKey;
        seed.UnlockTargetKey = unlockTargetKey;
        seed.UnlockReasonText = string.IsNullOrEmpty(unlockReasonText) ? string.Empty : unlockReasonText;
        seed.ProgressionDependencyHint = string.IsNullOrEmpty(progressionDependencyHint) ? string.Empty : progressionDependencyHint;
        seed.IsPreviewOnly = true;
        seeds.Add(seed);
    }

    private PrototypeRpgUnlockSeedSnapshot[] BuildRpgUnlockSeedSnapshots(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot)
    {
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgCombatContributionSnapshot safeContribution = contributionSnapshot ?? new PrototypeRpgCombatContributionSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeRunResult.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgMemberContributionSnapshot[] members = safeContribution.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        List<PrototypeRpgUnlockSeedSnapshot> seeds = new List<PrototypeRpgUnlockSeedSnapshot>();

        string sourceRunIdentity = !string.IsNullOrEmpty(safeRunResult.RunIdentity)
            ? safeRunResult.RunIdentity
            : (string.IsNullOrEmpty(safeRunResult.DungeonId) ? "run" : safeRunResult.DungeonId) + ":" + (string.IsNullOrEmpty(safeRunResult.RouteId) ? "route" : safeRunResult.RouteId) + ":" + (string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? "result" : safeRunResult.ResultStateKey);
        string eliteId = !string.IsNullOrEmpty(eliteOutcome.EliteName) ? eliteOutcome.EliteName : eliteOutcome.EliteTypeLabel;

        if (eliteOutcome.IsEliteDefeated)
        {
            AddUnlockSeed(seeds, sourceRunIdentity, "elite_encounter", safeRunResult.DungeonId, safeRunResult.RouteId, eliteId, "elite_unlock", string.IsNullOrEmpty(eliteId) ? "elite" : eliteId, "Elite clear recorded.", "Apply elite unlock on later progression pass.");
        }

        if (safeRunResult.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear)
        {
            AddUnlockSeed(seeds, sourceRunIdentity, "run_clear", safeRunResult.DungeonId, safeRunResult.RouteId, eliteId, "route_unlock", string.IsNullOrEmpty(safeRunResult.RouteId) ? "route_clear" : safeRunResult.RouteId, "Run clear recorded.", "Apply route-clear unlock on later progression pass.");
        }

        if (safeRunResult.RouteId == RiskyRouteId)
        {
            AddUnlockSeed(seeds, sourceRunIdentity, "route_risky", safeRunResult.DungeonId, safeRunResult.RouteId, eliteId, "risk_unlock", "risky_route", "Risky route completion recorded.", "Apply risky-route unlock on later progression pass.");
        }

        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberContributionSnapshot member = members[i] ?? new PrototypeRpgMemberContributionSnapshot();
            if (string.IsNullOrEmpty(member.MemberId))
            {
                continue;
            }

            if (member.KillCount > 0)
            {
                AddUnlockSeed(seeds, sourceRunIdentity, "member_track", safeRunResult.DungeonId, safeRunResult.RouteId, eliteId, "member_track", member.MemberId, (string.IsNullOrEmpty(member.DisplayName) ? "Member" : member.DisplayName) + " secured kills.", "Apply member combat track unlock later.");
            }
            else if (member.HealingDone > 0)
            {
                AddUnlockSeed(seeds, sourceRunIdentity, "member_support", safeRunResult.DungeonId, safeRunResult.RouteId, eliteId, "member_support", member.MemberId, (string.IsNullOrEmpty(member.DisplayName) ? "Member" : member.DisplayName) + " supported the party.", "Apply member support track unlock later.");
            }
        }

        return seeds.Count > 0 ? seeds.ToArray() : System.Array.Empty<PrototypeRpgUnlockSeedSnapshot>();
    }

    private string BuildRpgUnlockDisplayLabel(PrototypeRpgUnlockSeedSnapshot seed)
    {
        if (seed == null)
        {
            return "Unlock Seed";
        }

        switch (seed.UnlockCategoryKey)
        {
            case "elite_unlock":
                return "Elite Unlock Seed";
            case "route_unlock":
                return "Route Unlock Seed";
            case "risk_unlock":
                return "Risk Unlock Seed";
            case "member_track":
                return "Member Track Seed";
            case "member_support":
                return "Support Track Seed";
            default:
                return "Unlock Seed";
        }
    }

    private PrototypeRpgUnlockPreviewSnapshot[] BuildRpgUnlockPreviewSnapshots(PrototypeRpgUnlockSeedSnapshot[] unlockSeeds)
    {
        PrototypeRpgUnlockSeedSnapshot[] seeds = unlockSeeds ?? System.Array.Empty<PrototypeRpgUnlockSeedSnapshot>();
        if (seeds.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgUnlockPreviewSnapshot>();
        }

        PrototypeRpgUnlockPreviewSnapshot[] previews = new PrototypeRpgUnlockPreviewSnapshot[seeds.Length];
        for (int i = 0; i < seeds.Length; i++)
        {
            PrototypeRpgUnlockSeedSnapshot seed = seeds[i] ?? new PrototypeRpgUnlockSeedSnapshot();
            PrototypeRpgUnlockPreviewSnapshot preview = new PrototypeRpgUnlockPreviewSnapshot();
            preview.SourceRunIdentity = string.IsNullOrEmpty(seed.SourceRunIdentity) ? string.Empty : seed.SourceRunIdentity;
            preview.SourceEncounterId = string.IsNullOrEmpty(seed.SourceEncounterId) ? string.Empty : seed.SourceEncounterId;
            preview.DungeonId = string.IsNullOrEmpty(seed.DungeonId) ? string.Empty : seed.DungeonId;
            preview.RouteId = string.IsNullOrEmpty(seed.RouteId) ? string.Empty : seed.RouteId;
            preview.EliteId = string.IsNullOrEmpty(seed.EliteId) ? string.Empty : seed.EliteId;
            preview.UnlockCategoryKey = string.IsNullOrEmpty(seed.UnlockCategoryKey) ? string.Empty : seed.UnlockCategoryKey;
            preview.UnlockTargetKey = string.IsNullOrEmpty(seed.UnlockTargetKey) ? string.Empty : seed.UnlockTargetKey;
            preview.UnlockReasonText = string.IsNullOrEmpty(seed.UnlockReasonText) ? string.Empty : seed.UnlockReasonText;
            preview.ProgressionDependencyHint = string.IsNullOrEmpty(seed.ProgressionDependencyHint) ? string.Empty : seed.ProgressionDependencyHint;
            preview.DisplayLabel = BuildRpgUnlockDisplayLabel(seed);
            preview.SummaryText = !string.IsNullOrEmpty(preview.ProgressionDependencyHint)
                ? preview.DisplayLabel + ": " + preview.UnlockReasonText + " | " + preview.ProgressionDependencyHint
                : preview.DisplayLabel + ": " + preview.UnlockReasonText;
            preview.IsPreviewOnly = true;
            previews[i] = preview;
        }

        return previews;
    }

    private string BuildRpgUnlockPreviewSummaryText(PrototypeRpgUnlockPreviewSnapshot[] previews)
    {
        PrototypeRpgUnlockPreviewSnapshot[] safePreviews = previews ?? System.Array.Empty<PrototypeRpgUnlockPreviewSnapshot>();
        if (safePreviews.Length <= 0)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        int displayCount = Mathf.Min(3, safePreviews.Length);
        for (int i = 0; i < displayCount; i++)
        {
            PrototypeRpgUnlockPreviewSnapshot preview = safePreviews[i];
            string label = preview != null && !string.IsNullOrEmpty(preview.DisplayLabel) ? preview.DisplayLabel : "Unlock Seed";
            parts.Add(label);
        }

        if (safePreviews.Length > displayCount)
        {
            parts.Add("+" + (safePreviews.Length - displayCount) + " more");
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildRpgMemberUnlockSummaryText(PrototypeRpgProgressionPreviewSnapshot previewSnapshot, string memberId)
    {
        if (previewSnapshot == null || string.IsNullOrEmpty(memberId))
        {
            return string.Empty;
        }

        PrototypeRpgUnlockPreviewSnapshot[] previews = previewSnapshot.UnlockPreviews ?? System.Array.Empty<PrototypeRpgUnlockPreviewSnapshot>();
        List<string> parts = new List<string>();
        for (int i = 0; i < previews.Length; i++)
        {
            PrototypeRpgUnlockPreviewSnapshot preview = previews[i];
            if (preview == null)
            {
                continue;
            }

            if (preview.UnlockTargetKey == memberId)
            {
                parts.Add(string.IsNullOrEmpty(preview.DisplayLabel) ? "Unlock Seed" : preview.DisplayLabel);
            }
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private string BuildRpgRewardCarryoverHintText(PrototypeRpgLootOutcomeSnapshot lootOutcome, string resultStateKey)
    {
        if (lootOutcome == null)
        {
            return string.Empty;
        }

        int returnedAmount = Mathf.Max(0, lootOutcome.TotalReturnedAmount > 0 ? lootOutcome.TotalReturnedAmount : lootOutcome.TotalLootGained);
        int lostPendingAmount = Mathf.Max(0, lootOutcome.PendingBonusRewardLostAmount);
        if (returnedAmount <= 0 && lostPendingAmount <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (returnedAmount > 0)
        {
            string prefix = resultStateKey == PrototypeBattleOutcomeKeys.RunClear ? "Returned " : "Recovered ";
            parts.Add(prefix + BuildLootAmountText(returnedAmount));
        }

        if (lostPendingAmount > 0)
        {
            parts.Add("Lost pending bonus " + BuildLootAmountText(lostPendingAmount));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private PrototypeRpgProgressionPreviewSnapshot BuildRpgProgressionPreviewSnapshot(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgProgressionSeedSnapshot progressionSeedSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot)
    {
        PrototypeRpgProgressionPreviewSnapshot snapshot = new PrototypeRpgProgressionPreviewSnapshot();
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgProgressionSeedSnapshot safeSeed = progressionSeedSnapshot ?? new PrototypeRpgProgressionSeedSnapshot();
        PrototypeRpgCombatContributionSnapshot safeContribution = contributionSnapshot ?? new PrototypeRpgCombatContributionSnapshot();
        PrototypeRpgMemberContributionSnapshot[] contributionMembers = safeContribution.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        PrototypeRpgUnlockSeedSnapshot[] unlockSeeds = safeSeed.UnlockSeeds != null && safeSeed.UnlockSeeds.Length > 0
            ? safeSeed.UnlockSeeds
            : BuildRpgUnlockSeedSnapshots(safeRunResult, safeContribution);

        snapshot.ResultStateKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? string.Empty : safeRunResult.ResultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(safeRunResult.DungeonId) ? string.Empty : safeRunResult.DungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(safeRunResult.DungeonLabel) ? string.Empty : safeRunResult.DungeonLabel;
        snapshot.DungeonDangerLabel = string.IsNullOrEmpty(safeSeed.DungeonDangerLabel) ? string.Empty : safeSeed.DungeonDangerLabel;
        snapshot.RouteId = string.IsNullOrEmpty(safeRunResult.RouteId) ? string.Empty : safeRunResult.RouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(safeRunResult.RouteLabel) ? string.Empty : safeRunResult.RouteLabel;
        snapshot.RouteRiskLabel = string.IsNullOrEmpty(safeSeed.RouteRiskLabel) ? string.Empty : safeSeed.RouteRiskLabel;
        snapshot.EliteDefeated = safeSeed.EliteDefeated;
        snapshot.TotalLootGained = safeSeed.Loot != null ? Mathf.Max(0, safeSeed.Loot.TotalLootGained) : 0;
        snapshot.RewardHintTags = BuildRpgProgressionPreviewRewardHintTags(safeRunResult, safeSeed);
        snapshot.GrowthHintTags = BuildRpgProgressionPreviewGrowthHintTags(safeSeed, safeContribution);
        snapshot.UnlockPreviews = BuildRpgUnlockPreviewSnapshots(unlockSeeds);
        snapshot.UnlockPreviewSummaryText = BuildRpgUnlockPreviewSummaryText(snapshot.UnlockPreviews);
        snapshot.RewardCarryoverHintText = BuildRpgRewardCarryoverHintText(safeRunResult.LootOutcome, safeRunResult.ResultStateKey);

        if (contributionMembers.Length <= 0)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        }
        else
        {
            PrototypeRpgMemberProgressPreview[] previews = new PrototypeRpgMemberProgressPreview[contributionMembers.Length];
            for (int i = 0; i < contributionMembers.Length; i++)
            {
                PrototypeRpgMemberContributionSnapshot contribution = CopyRpgMemberContributionSnapshot(contributionMembers[i]);
                PrototypeRpgMemberProgressionSeed memberSeed = FindRpgMemberProgressionSeed(safeSeed, contribution.MemberId) ?? new PrototypeRpgMemberProgressionSeed();
                PrototypeRpgMemberProgressPreview preview = new PrototypeRpgMemberProgressPreview();
                preview.MemberId = string.IsNullOrEmpty(contribution.MemberId) ? string.Empty : contribution.MemberId;
                preview.DisplayName = string.IsNullOrEmpty(contribution.DisplayName) ? string.Empty : contribution.DisplayName;
                preview.RoleTag = string.IsNullOrEmpty(contribution.RoleTag) ? string.Empty : contribution.RoleTag;
                preview.RoleLabel = string.IsNullOrEmpty(contribution.RoleLabel) ? string.Empty : contribution.RoleLabel;
                preview.GrowthTrackId = string.IsNullOrEmpty(memberSeed.GrowthTrackId) ? string.Empty : memberSeed.GrowthTrackId;
                preview.JobId = string.IsNullOrEmpty(memberSeed.JobId) ? string.Empty : memberSeed.JobId;
                preview.EquipmentLoadoutId = string.IsNullOrEmpty(memberSeed.EquipmentLoadoutId) ? string.Empty : memberSeed.EquipmentLoadoutId;
                preview.SkillLoadoutId = string.IsNullOrEmpty(memberSeed.SkillLoadoutId) ? string.Empty : memberSeed.SkillLoadoutId;
                preview.Survived = contribution.Survived;
                preview.Contribution = contribution;
                preview.SuggestedGrowthHintTags = BuildMemberProgressionGrowthHintTags(contribution);
                preview.SuggestedRewardHintTags = BuildMemberProgressionRewardHintTags(contribution, safeRunResult);
                preview.NotableOutcomeKey = BuildMemberNotableOutcomeKey(contribution, safeRunResult);
                preview.GrowthDirectionLabel = BuildRpgMemberGrowthDirectionLabel(contribution, safeRunResult);
                preview.RewardCarryoverHintText = BuildRpgMemberRewardCarryoverHintText(contribution, safeRunResult, safeSeed.Loot);
                preview.GrowthPathCandidates = BuildRpgMemberGrowthPathCandidates(contribution, memberSeed, safeRunResult);
                preview.UpgradeCandidates = BuildRpgMemberUpgradeCandidates(contribution, memberSeed, safeRunResult, safeSeed.Loot);
                preview.UpgradeCandidateSummaryText = BuildRpgUpgradeCandidateSummaryText(preview.UpgradeCandidates);
                preview.NextGrowthTrackHint = BuildMemberNextGrowthTrackHint(contribution, memberSeed);
                preview.NextJobHint = BuildMemberNextJobHint(contribution, memberSeed);
                preview.NextSkillLoadoutHint = BuildMemberNextSkillLoadoutHint(contribution, memberSeed);
                preview.NextEquipmentLoadoutHint = BuildMemberNextEquipmentLoadoutHint(contribution, memberSeed);
                previews[i] = preview;
            }

            snapshot.Members = previews;
        }

        string partyId = GetActivePostRunPartyId();
        PrototypeRpgUpgradeOfferDefinition[] offerDefinitions = BuildRpgUpgradeOfferDefinitions(safeRunResult, snapshot.Members, partyId);
        PrototypeRpgUpgradeOfferCandidate[] offerCandidates = BuildRpgUpgradeOfferCandidates(offerDefinitions);
        PrototypeRpgApplyReadyUpgradeChoice[] applyReadyChoices = BuildRpgApplyReadyUpgradeChoices(safeRunResult.RunIdentity, partyId, offerCandidates);
        PrototypeRpgMemberProgressPreview[] memberPreviews = snapshot.Members ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        for (int i = 0; i < memberPreviews.Length; i++)
        {
            PrototypeRpgMemberProgressPreview preview = memberPreviews[i];
            if (preview == null)
            {
                continue;
            }

            preview.UpgradeOfferSummaryText = BuildRpgMemberUpgradeOfferSummaryText(preview.MemberId, offerCandidates);
            preview.ApplyReadySummaryText = BuildRpgMemberApplyReadySummaryText(preview.MemberId, applyReadyChoices);
            preview.PreviewSummaryText = BuildMemberProgressionPreviewText(preview);
        }

        snapshot.GrowthCandidateSummaryText = BuildRpgGrowthPathCandidateSummaryText(memberPreviews);
        snapshot.UpgradeCandidateSummaryText = BuildRpgUpgradeCandidateSummaryText(memberPreviews);
        snapshot.UpgradeOfferSummaryText = BuildRpgUpgradeOfferSummaryText(offerCandidates);
        snapshot.ApplyReadySummaryText = BuildRpgApplyReadySummaryText(applyReadyChoices);

        List<string> previewParts = new List<string>();
        if (snapshot.TotalLootGained > 0)
        {
            previewParts.Add(BuildLootAmountText(snapshot.TotalLootGained));
        }

        if (!string.IsNullOrEmpty(snapshot.GrowthCandidateSummaryText) && snapshot.GrowthCandidateSummaryText != "None")
        {
            previewParts.Add(snapshot.GrowthCandidateSummaryText);
        }

        if (!string.IsNullOrEmpty(snapshot.UpgradeCandidateSummaryText) && snapshot.UpgradeCandidateSummaryText != "None")
        {
            previewParts.Add(snapshot.UpgradeCandidateSummaryText);
        }

        if (!string.IsNullOrEmpty(snapshot.UpgradeOfferSummaryText) && snapshot.UpgradeOfferSummaryText != "None")
        {
            previewParts.Add(snapshot.UpgradeOfferSummaryText);
        }

        if (!string.IsNullOrEmpty(snapshot.ApplyReadySummaryText) && snapshot.ApplyReadySummaryText != "None")
        {
            previewParts.Add(snapshot.ApplyReadySummaryText);
        }

        if (snapshot.RewardHintTags != null && snapshot.RewardHintTags.Length > 0)
        {
            previewParts.Add("Reward " + BuildRpgTagListText(snapshot.RewardHintTags));
        }

        if (snapshot.GrowthHintTags != null && snapshot.GrowthHintTags.Length > 0)
        {
            previewParts.Add("Growth " + BuildRpgTagListText(snapshot.GrowthHintTags));
        }

        if (snapshot.EliteDefeated)
        {
            previewParts.Add("elite_defeated");
        }

        if (!string.IsNullOrEmpty(snapshot.UnlockPreviewSummaryText) && snapshot.UnlockPreviewSummaryText != "None")
        {
            previewParts.Add(snapshot.UnlockPreviewSummaryText);
        }

        if (!string.IsNullOrEmpty(snapshot.RewardCarryoverHintText))
        {
            previewParts.Add(snapshot.RewardCarryoverHintText);
        }

        snapshot.ProgressionPreviewText = previewParts.Count > 0 ? string.Join(" | ", previewParts.ToArray()) : "None";
        ApplyRpgAppliedProgressToProgressionPreviewSnapshot(snapshot, GetLatestAppliedPartyProgressStateInternal());
        return snapshot;
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
        if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
        {
            return 0;
        }

        bool wasDefeated = monster.IsDefeated;
        int safeDamage = Mathf.Max(1, damage);
        int applied = monster.RuntimeState != null ? monster.RuntimeState.ApplyDamage(safeDamage) : 0;
        if (monster.RuntimeState == null)
        {
            int previousHp = monster.CurrentHp;
            monster.CurrentHp = Mathf.Max(0, previousHp - safeDamage);
            applied = previousHp - monster.CurrentHp;
        }

        if (applied <= 0)
        {
            return 0;
        }

        _totalDamageDealt += applied;
        if (actor != null)
        {
            AddRunMemberContributionValue(_runMemberDamageDealt, actor.PartySlotIndex, applied);
        }

        FlashMonster(monster, popupColor);
        ShowBattlePopupForMonster(monster, "-" + applied, popupColor);
        string actionKey = actor != null ? GetBattleActionKey(_queuedBattleAction) : (_pendingEnemyUsedSpecialAttack ? "skill" : "attack");
        string skillId = actor != null && actionKey == "skill" ? actor.DefaultSkillId : string.Empty;
        RecordBattleEvent(
            PrototypeBattleEventKeys.DamageApplied,
            actor != null ? actor.MemberId : string.Empty,
            monster.MonsterId,
            applied,
            monster.DisplayName + " took " + applied + " damage.",
            actionKey: actionKey,
            skillId: skillId,
            actorName: actor != null ? actor.DisplayName : string.Empty,
            targetName: monster.DisplayName,
            shortText: "-" + applied);
        ResolveMonsterDefeat(monster, wasDefeated);
        if (actor != null && !wasDefeated && monster.IsDefeated)
        {
            AddRunMemberContributionValue(_runMemberKillCount, actor.PartySlotIndex, 1);
        }

        return applied;
    }

    private int ApplyBattleDamageToPartyMember(DungeonMonsterRuntimeData monster, int targetIndex, int damage, Color popupColor)
    {
        DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(targetIndex);
        if (member == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return 0;
        }

        bool wasKnockedOut = member.IsDefeated;
        int safeDamage = Mathf.Max(1, damage);
        int applied = member.RuntimeState != null ? member.RuntimeState.ApplyDamage(safeDamage) : 0;
        if (member.RuntimeState == null)
        {
            int previousHp = member.CurrentHp;
            member.CurrentHp = Mathf.Max(0, previousHp - safeDamage);
            applied = previousHp - member.CurrentHp;
        }

        if (applied <= 0)
        {
            return 0;
        }

        _totalDamageTaken += applied;
        AddRunMemberContributionValue(_runMemberDamageTaken, targetIndex, applied);

        FlashPartyMember(targetIndex, popupColor);
        ShowBattlePopupForPartyMember(targetIndex, "-" + applied, popupColor);
        RecordBattleEvent(
            PrototypeBattleEventKeys.DamageApplied,
            monster != null ? monster.MonsterId : string.Empty,
            member.MemberId,
            applied,
            member.DisplayName + " took " + applied + " damage.",
            actionKey: _pendingEnemyUsedSpecialAttack ? "skill" : "attack",
            actorName: monster != null ? monster.DisplayName : string.Empty,
            targetName: member.DisplayName,
            shortText: "-" + applied);
        ResolvePartyMemberKnockOut(member, wasKnockedOut);
        return applied;
    }

    private int ApplyBattleHealToPartyMember(DungeonPartyMemberRuntimeData actor, DungeonPartyMemberRuntimeData member, int memberIndex, int recoverAmount, Color popupColor)
    {
        if (member == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return 0;
        }

        int safeRecover = Mathf.Max(1, recoverAmount);
        int recovered = member.RuntimeState != null ? member.RuntimeState.RecoverHp(safeRecover) : 0;
        if (member.RuntimeState == null)
        {
            int previousHp = member.CurrentHp;
            member.CurrentHp = Mathf.Min(member.MaxHp, previousHp + safeRecover);
            recovered = member.CurrentHp - previousHp;
        }

        if (recovered <= 0)
        {
            return 0;
        }

        _totalHealingDone += recovered;
        if (actor != null)
        {
            AddRunMemberContributionValue(_runMemberHealingDone, actor.PartySlotIndex, recovered);
        }

        FlashPartyMember(memberIndex, popupColor);
        ShowBattlePopupForPartyMember(memberIndex, "+" + recovered, popupColor);
        RecordBattleEvent(
            PrototypeBattleEventKeys.HealApplied,
            actor != null ? actor.MemberId : string.Empty,
            member.MemberId,
            recovered,
            member.DisplayName + " recovered " + recovered + " HP.",
            actionKey: "skill",
            skillId: actor != null ? actor.DefaultSkillId : string.Empty,
            actorName: actor != null ? actor.DisplayName : string.Empty,
            targetName: member.DisplayName,
            shortText: "+" + recovered);
        return recovered;
    }

    private void ResolveMonsterDefeat(DungeonMonsterRuntimeData monster, bool wasDefeated)
    {
        if (monster == null || wasDefeated || !monster.IsDefeated)
        {
            return;
        }

        monster.RuntimeState?.ClearIntent();
        ShowBattlePopupForMonster(monster, "Defeated", new Color(1f, 0.82f, 0.24f, 1f), 1.18f);
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
        AddRunMemberContributionValue(_runMemberActionCount, _currentActorIndex, 1);
        _partyActedThisRound[_currentActorIndex] = true;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        ClearBattleHoverState();
        ClearBattleInputLock();

        if (GetLivingBattleMonsterCount() <= 0)
        {
            ResolveCurrentEncounterVictory();
            return;
        }

        int nextIndex = Mathf.Max(0, _currentActorIndex + 1);
        if (TrySelectNextPartyActor(nextIndex))
        {
            _battleState = BattleState.PartyActionSelect;
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            RecordCurrentPartyTurnStartEvent();
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return;
        }

        BeginEnemyTurn();
    }

    private void ResumePlayerTurnFromStartIndex(int startIndex)
    {
        ResetRoundActions();
        _battleState = BattleState.PartyActionSelect;
        TrySelectNextPartyActor(Mathf.Max(0, startIndex));
        SetActiveBattleMonster(GetFirstLivingBattleMonster());
        ClearEnemyIntentState();
        RecordCurrentPartyTurnStartEvent();
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
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
        PrototypeEnemyIntentSnapshot snapshot = new PrototypeEnemyIntentSnapshot();
        if (monster == null)
        {
            return snapshot;
        }

        PrototypeRpgEnemyIntentDefinition intentDefinition = ResolveEnemyIntentDefinition(monster, useSpecial);
        PrototypeCombatResolutionRecord resolution = BuildPendingEnemyActionResolutionRecord(monster, targetIndex, useSpecial);
        DungeonPartyMemberRuntimeData targetMember = GetPartyMemberAtIndex(targetIndex);
        snapshot.IntentKey = intentDefinition != null && !string.IsNullOrEmpty(intentDefinition.IntentKey)
            ? intentDefinition.IntentKey
            : useSpecial ? "enemy_special_attack" : "enemy_attack";
        snapshot.DisplayLabel = !string.IsNullOrEmpty(resolution.SummaryText)
            ? resolution.SummaryText
            : PrototypeCombatIntentPolicy.ResolveDisplayLabel(intentDefinition, resolution.ActionLabel);
        snapshot.TargetPatternKey = resolution.TargetPolicyKey;
        snapshot.TargetPolicyKey = resolution.TargetPolicyKey;
        snapshot.PreviewText = !string.IsNullOrEmpty(resolution.FeedbackText)
            ? resolution.FeedbackText
            : BuildEnemyIntentText(monster, targetIndex, useSpecial);
        snapshot.PredictedValue = Mathf.Max(1, resolution.PowerValue);
        snapshot.PowerValue = snapshot.PredictedValue;
        snapshot.TargetId = targetMember != null ? targetMember.MemberId : string.Empty;
        snapshot.TargetName = targetMember != null ? targetMember.DisplayName : string.Empty;
        snapshot.TargetDisplayName = targetMember != null ? targetMember.DisplayName : string.Empty;
        snapshot.SourceEnemyId = monster.MonsterId;
        snapshot.SourceEnemyName = monster.DisplayName;
        snapshot.ActorMonsterId = monster.MonsterId;
        snapshot.ActorDisplayName = monster.DisplayName;
        snapshot.ActionKey = !string.IsNullOrEmpty(resolution.ActionKey) ? resolution.ActionKey : (useSpecial ? PrototypeCombatActionKeys.Skill : PrototypeCombatActionKeys.Attack);
        snapshot.EffectTypeKey = !string.IsNullOrEmpty(resolution.EffectTypeKey)
            ? resolution.EffectTypeKey
            : PrototypeCombatIntentPolicy.ResolveEffectTypeKey(intentDefinition, useSpecial);
        snapshot.SpecialActionLabel = useSpecial ? resolution.ActionLabel : string.Empty;
        snapshot.IsTelegraphed = true;
        return snapshot;
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
        _appliedPartyProgressBySessionKey.Clear();
        _latestAppliedPartyProgressSessionKey = string.Empty;
        _recentBattleLogs.Clear();
        InitializeDispatchRecoveryState();
        ResetDungeonRunPresentationState();
    }

    private void DisposeDungeonRunSystems()
    {
        ClearMonsterVisuals();
        _appliedPartyProgressBySessionKey.Clear();
        _latestAppliedPartyProgressSessionKey = string.Empty;

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
        _appliedPartyProgressBySessionKey.Clear();
        _latestAppliedPartyProgressSessionKey = string.Empty;
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

        ConfigureDungeonCamera(worldCamera);
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
        string dungeonId = GetSelectedPreviewDungeonId();
        if (string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        PrototypeRpgDungeonSelectionPreviewData preview = BuildSelectedDungeonSelectionPreviewData(dungeonId);
        return preview != null && preview.EncounterPreview != null && !string.IsNullOrEmpty(preview.EncounterPreview.SummaryText)
            ? preview.EncounterPreview.SummaryText
            : "None";
    }

    private string GetSelectedPreviewDungeonId()
    {
        if (_selectedMarker == null)
        {
            return string.Empty;
        }

        if (_selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon)
        {
            return _selectedMarker.EntityData.Id;
        }

        if (_selectedMarker.EntityData.Kind == WorldEntityKind.City)
        {
            return GetLinkedDungeonIdForCity(_selectedMarker.EntityData.Id);
        }

        return string.Empty;
    }

    private PrototypeRpgDungeonSelectionPreviewData BuildSelectedDungeonSelectionPreviewData()
    {
        return BuildSelectedDungeonSelectionPreviewData(GetSelectedPreviewDungeonId());
    }

    private PrototypeRpgDungeonSelectionPreviewData BuildSelectedDungeonSelectionPreviewData(string dungeonId)
    {
        PrototypeRpgDungeonSelectionPreviewData preview = new PrototypeRpgDungeonSelectionPreviewData();
        preview.DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;

        DungeonIdentityTemplate dungeonTemplate = GetDungeonTemplateById(dungeonId);
        PrototypeRpgAppliedPartyProgressState appliedState = GetLatestAppliedPartyProgressStateInternal();
        PrototypeRpgRunResultSnapshot latestRunResult = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
        preview.DungeonLabel = dungeonTemplate != null ? dungeonTemplate.DungeonLabel : string.Empty;
        preview.DangerLabel = dungeonTemplate != null ? dungeonTemplate.DangerLabel : string.Empty;
        preview.HasAppliedPartyProgress = appliedState != null && appliedState.HasAppliedProgress;
        preview.AppliedPartySummaryText = preview.HasAppliedPartyProgress && !string.IsNullOrEmpty(appliedState.SummaryText) ? appliedState.SummaryText : string.Empty;
        preview.AppliedLastRunIdentity = appliedState != null && !string.IsNullOrEmpty(appliedState.LastAppliedRunIdentity) ? appliedState.LastAppliedRunIdentity : string.Empty;
        preview.AppliedLastResultStateKey = appliedState != null && !string.IsNullOrEmpty(appliedState.LastResultStateKey) ? appliedState.LastResultStateKey : string.Empty;
        preview.SelectedLastRunSummaryText = HasRpgRunResultSnapshotData(latestRunResult) ? latestRunResult.ResultSummary : string.Empty;
        preview.EncounterPreview = BuildSelectedEncounterPreviewData(dungeonId);
        preview.SafeRoutePreview = BuildRouteEncounterPreviewData(dungeonId, SafeRouteId);
        preview.RiskyRoutePreview = BuildRouteEncounterPreviewData(dungeonId, RiskyRouteId);
        preview.SafeRewardPreview = BuildRouteRewardPreviewData(dungeonId, SafeRouteId);
        preview.RiskyRewardPreview = BuildRouteRewardPreviewData(dungeonId, RiskyRouteId);
        preview.SafeEventPreview = BuildRouteEventPreviewData(dungeonId, SafeRouteId);
        preview.RiskyEventPreview = BuildRouteEventPreviewData(dungeonId, RiskyRouteId);
        preview.RecommendedPowerPreview = BuildRecommendedPowerPreviewData(dungeonId);
        return preview;
    }

    private PrototypeRpgEncounterPreview BuildSelectedEncounterPreviewData(string dungeonId)
    {
        PrototypeRpgEncounterPreview preview = new PrototypeRpgEncounterPreview();
        preview.DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;

        if (string.IsNullOrEmpty(dungeonId))
        {
            preview.SummaryText = "None";
            return preview;
        }

        DungeonIdentityTemplate dungeonTemplate = GetDungeonTemplateById(dungeonId);
        PrototypeRpgRouteEncounterPreview safeRoutePreview = BuildRouteEncounterPreviewData(dungeonId, SafeRouteId);
        PrototypeRpgRouteEncounterPreview riskyRoutePreview = BuildRouteEncounterPreviewData(dungeonId, RiskyRouteId);
        PrototypeRpgRecommendedPowerPreview recommendedPowerPreview = BuildRecommendedPowerPreviewData(dungeonId);

        preview.EncounterId = "selected-dungeon-preview";
        preview.DisplayName = dungeonTemplate != null ? dungeonTemplate.DungeonLabel : "Dungeon";
        preview.EncounterTypeLabel = "Route Preview";
        preview.RoomTypeLabel = "Dungeon Preview";
        preview.EnemyCount = Mathf.Max(safeRoutePreview.TotalEnemyCount, riskyRoutePreview.TotalEnemyCount);
        preview.HasElitePresence = safeRoutePreview.EliteEncounterCount > 0 || riskyRoutePreview.EliteEncounterCount > 0;
        preview.EnemyTypeSummary = BuildJoinedPreviewSummary(safeRoutePreview.EnemyTypeSummary, riskyRoutePreview.EnemyTypeSummary);
        preview.ThreatLabel = dungeonTemplate != null ? dungeonTemplate.DangerLabel : string.Empty;
        preview.RecommendedPowerHint = recommendedPowerPreview != null ? recommendedPowerPreview.RecommendedPowerText : string.Empty;
        preview.IntentPreviewText = recommendedPowerPreview != null ? recommendedPowerPreview.EnemyPatternSummary : string.Empty;

        int encounterCount = Mathf.Max(safeRoutePreview.EncounterCount, riskyRoutePreview.EncounterCount);
        string summary = encounterCount > 0
            ? encounterCount + " per route | " + preview.EnemyCount + " foes incl. elite"
            : "None";
        if (!string.IsNullOrEmpty(preview.EnemyTypeSummary))
        {
            summary += " | " + preview.EnemyTypeSummary;
        }

        if (dungeonTemplate != null && !string.IsNullOrEmpty(dungeonTemplate.StyleLabel))
        {
            summary += " | " + dungeonTemplate.StyleLabel;
        }

        preview.SummaryText = summary;
        return preview;
    }

    private PrototypeRpgRouteEncounterPreview BuildRouteEncounterPreviewData(string dungeonId, string routeId)
    {
        PrototypeRpgRouteEncounterPreview preview = new PrototypeRpgRouteEncounterPreview();
        preview.DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
        preview.RouteId = NormalizeRouteChoiceId(routeId);

        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            preview.SummaryText = "None";
            return preview;
        }

        PrototypeRpgEncounterVariationDefinition variation = ResolveEncounterVariationDefinition(dungeonId, routeId);
        PrototypeRpgEncounterDefinition[] encounterDefinitions = BuildRouteEncounterDefinitions(dungeonId, routeId);
        PrototypeRpgRecommendedPowerPreview recommendedPowerPreview = BuildRecommendedPowerPreviewData(dungeonId);
        preview.RouteLabel = template.RouteLabel;
        preview.RouteDescriptionText = template.Description;
        preview.RouteRiskLabel = template.RiskLabel;
        preview.RoomPathSummaryText = BuildRoomPathPreviewText(dungeonId, routeId);
        preview.EncounterCount = encounterDefinitions.Length;
        preview.EventFocusText = template.EventFocus;
        preview.RewardFocusText = variation != null && !string.IsNullOrEmpty(variation.RewardPreviewLabel)
            ? variation.RewardPreviewLabel
            : template.RewardPreview;
        preview.RecommendedPowerText = recommendedPowerPreview != null ? recommendedPowerPreview.RecommendedPowerText : string.Empty;
        preview.ThreatLabel = variation != null && !string.IsNullOrEmpty(variation.DifficultyHintLabel)
            ? variation.DifficultyHintLabel
            : string.IsNullOrEmpty(template.RiskLabel)
                ? (recommendedPowerPreview != null ? recommendedPowerPreview.ThreatTierLabel : string.Empty)
                : template.RiskLabel + " Risk";

        int totalEnemyCount = 0;
        int eliteEncounterCount = 0;
        for (int i = 0; i < encounterDefinitions.Length; i++)
        {
            PrototypeRpgEncounterDefinition definition = encounterDefinitions[i];
            if (definition == null)
            {
                continue;
            }

            totalEnemyCount += definition.EnemyMembers != null ? definition.EnemyMembers.Length : 0;
            if (definition.IsEliteEncounter)
            {
                eliteEncounterCount++;
            }
        }

        preview.TotalEnemyCount = totalEnemyCount;
        preview.EliteEncounterCount = eliteEncounterCount;
        preview.EnemyTypeSummary = BuildEncounterEnemyTypeSummary(encounterDefinitions);
        preview.PatternSummaryText = BuildEncounterPatternSummary(encounterDefinitions);
        preview.EncounterSummaryText = totalEnemyCount > 0
            ? totalEnemyCount + " foes" + (eliteEncounterCount > 0 ? " incl. " + eliteEncounterCount + " elite" : string.Empty)
            : "No encounter data";
        if (!string.IsNullOrEmpty(preview.EnemyTypeSummary))
        {
            preview.EncounterSummaryText += " | " + preview.EnemyTypeSummary;
        }

        preview.SummaryText = preview.EncounterSummaryText;
        return preview;
    }

    private PrototypeRpgRouteRewardPreview BuildRouteRewardPreviewData(string dungeonId, string routeId)
    {
        PrototypeRpgRouteRewardPreview preview = new PrototypeRpgRouteRewardPreview();
        preview.DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
        preview.RouteId = NormalizeRouteChoiceId(routeId);

        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            preview.SummaryText = "None";
            preview.RewardBreakdownText = "None";
            return preview;
        }

        PrototypeRpgEncounterVariationDefinition variation = ResolveEncounterVariationDefinition(dungeonId, routeId);
        PrototypeRpgEncounterDefinition[] encounterDefinitions = BuildRouteEncounterDefinitions(dungeonId, routeId);
        PrototypeRpgEncounterDefinition eliteDefinition = null;
        for (int i = 0; i < encounterDefinitions.Length; i++)
        {
            PrototypeRpgEncounterDefinition definition = encounterDefinitions[i];
            if (definition != null && definition.IsEliteEncounter)
            {
                eliteDefinition = definition;
                break;
            }
        }

        preview.RouteLabel = template.RouteLabel;
        preview.RewardLabel = eliteDefinition != null && !string.IsNullOrEmpty(eliteDefinition.RewardLabel)
            ? eliteDefinition.RewardLabel
            : variation != null && !string.IsNullOrEmpty(variation.RewardPreviewLabel)
                ? variation.RewardPreviewLabel
                : template.RewardPreview;
        preview.RewardResourceId = DungeonRewardResourceId;
        preview.BattleRewardAmountHint = Mathf.Max(0, template.BattleLootAmount);
        preview.ChestRewardAmountHint = Mathf.Max(0, template.ChestRewardAmount);
        preview.EventRewardAmountHint = Mathf.Max(0, template.BonusLootAmount);
        preview.EliteRewardAmountHint = eliteDefinition != null ? Mathf.Max(0, eliteDefinition.RewardAmountHint) : 0;
        preview.PendingBonusRewardHint = GetPreviewPendingBonusRewardHint(dungeonId, routeId, preview.RewardLabel);
        preview.TotalRewardAmountHint = preview.BattleRewardAmountHint + preview.ChestRewardAmountHint + preview.EventRewardAmountHint + preview.EliteRewardAmountHint + preview.PendingBonusRewardHint;
        preview.RewardBreakdownText = "Battle " + preview.BattleRewardAmountHint + " / Chest " + preview.ChestRewardAmountHint + " / Event " + preview.EventRewardAmountHint + " / Elite " + preview.EliteRewardAmountHint;
        if (preview.PendingBonusRewardHint > 0)
        {
            preview.RewardBreakdownText += " / Bonus " + preview.PendingBonusRewardHint;
        }

        preview.RewardBreakdownText += " / Total " + preview.TotalRewardAmountHint;
        preview.SummaryText = preview.RouteLabel + ": " + preview.RewardBreakdownText;
        return preview;
    }

    private PrototypeRpgRouteEventPreview BuildRouteEventPreviewData(string dungeonId, string routeId)
    {
        PrototypeRpgRouteEventPreview preview = new PrototypeRpgRouteEventPreview();
        preview.DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
        preview.RouteId = NormalizeRouteChoiceId(routeId);

        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            preview.SummaryText = "None";
            return preview;
        }

        preview.RouteLabel = template.RouteLabel;
        preview.RecoverAmountHint = Mathf.Max(0, template.RecoverAmount);
        preview.BonusRewardAmountHint = Mathf.Max(0, template.BonusLootAmount);
        preview.BonusRewardResourceId = DungeonRewardResourceId;
        preview.EventFocusText = template.EventFocus;
        preview.SummaryText = preview.RouteLabel + ": Recover +" + preview.RecoverAmountHint + " HP or Bonus +" + preview.BonusRewardAmountHint + " " + preview.BonusRewardResourceId + " | " + preview.EventFocusText;
        return preview;
    }

    private PrototypeRpgRecommendedPowerPreview BuildRecommendedPowerPreviewData(string dungeonId)
    {
        PrototypeRpgRecommendedPowerPreview preview = new PrototypeRpgRecommendedPowerPreview();
        preview.DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;

        DungeonIdentityTemplate dungeonTemplate = GetDungeonTemplateById(dungeonId);
        PrototypeRpgEncounterVariationDefinition safeVariation = ResolveEncounterVariationDefinition(dungeonId, SafeRouteId);
        PrototypeRpgEncounterVariationDefinition riskyVariation = ResolveEncounterVariationDefinition(dungeonId, RiskyRouteId);
        preview.DangerLabel = dungeonTemplate != null ? dungeonTemplate.DangerLabel : string.Empty;
        preview.ThreatTierLabel = dungeonTemplate != null && !string.IsNullOrEmpty(dungeonTemplate.StyleLabel)
            ? dungeonTemplate.StyleLabel
            : BuildJoinedPreviewSummary(
                safeVariation != null ? safeVariation.DifficultyHintLabel : string.Empty,
                riskyVariation != null ? riskyVariation.DifficultyHintLabel : string.Empty);
        int runtimeRecommendedPower = _runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId)
            ? Mathf.Max(0, _runtimeEconomyState.GetRecommendedPower(dungeonId))
            : 0;
        int variationRecommendedPower = Mathf.Max(
            safeVariation != null ? safeVariation.RecommendedPowerHint : 0,
            riskyVariation != null ? riskyVariation.RecommendedPowerHint : 0);
        preview.RecommendedPowerValue = runtimeRecommendedPower > 0 ? runtimeRecommendedPower : variationRecommendedPower;
        preview.RecommendedPowerBand = BuildRecommendedPowerBand(preview.RecommendedPowerValue);
        preview.RecommendedPowerText = preview.RecommendedPowerValue > 0 ? preview.RecommendedPowerValue.ToString() : "None";
        preview.EnemyPatternSummary = BuildJoinedPreviewSummary(
            BuildEncounterPatternSummary(BuildRouteEncounterDefinitions(dungeonId, SafeRouteId)),
            BuildEncounterPatternSummary(BuildRouteEncounterDefinitions(dungeonId, RiskyRouteId)));
        preview.SustainRiskHint = BuildRecommendedPowerSustainRiskHint(dungeonId, preview.RecommendedPowerValue);
        preview.SummaryText = preview.RecommendedPowerValue > 0
            ? "RP " + preview.RecommendedPowerValue + " | " + preview.RecommendedPowerBand + " | " + preview.SustainRiskHint
            : "None";
        return preview;
    }

    private PrototypeRpgEncounterDefinition[] BuildRouteEncounterDefinitions(string dungeonId, string routeId)
    {
        PrototypeRpgEncounterVariationDefinition variation = ResolveEncounterVariationDefinition(dungeonId, routeId);
        if (variation != null)
        {
            List<PrototypeRpgEncounterDefinition> definitions = new List<PrototypeRpgEncounterDefinition>();
            if (variation.RoomTemplates != null)
            {
                for (int i = 0; i < variation.RoomTemplates.Length; i++)
                {
                    PrototypeRpgEncounterTemplateDefinition template = variation.RoomTemplates[i];
                    PrototypeRpgEncounterDefinition definition = template != null
                        ? PrototypeRpgEncounterCatalog.ResolveDefinition(template.EncounterDefinitionId)
                        : null;
                    if (definition != null)
                    {
                        definitions.Add(definition);
                    }
                }
            }

            if (variation.EliteTemplate != null)
            {
                PrototypeRpgEncounterDefinition eliteDefinition = PrototypeRpgEncounterCatalog.ResolveDefinition(variation.EliteTemplate.EncounterDefinitionId);
                if (eliteDefinition != null)
                {
                    definitions.Add(eliteDefinition);
                }
            }

            if (definitions.Count > 0)
            {
                return definitions.ToArray();
            }
        }

        string prefix = BuildPreviewEncounterDefinitionPrefix(dungeonId, routeId);
        if (string.IsNullOrEmpty(prefix))
        {
            return System.Array.Empty<PrototypeRpgEncounterDefinition>();
        }

        List<PrototypeRpgEncounterDefinition> fallbackDefinitions = new List<PrototypeRpgEncounterDefinition>();
        string[] suffixes = { "-room1", "-room2", "-elite" };
        for (int i = 0; i < suffixes.Length; i++)
        {
            PrototypeRpgEncounterDefinition definition = PrototypeRpgEncounterCatalog.ResolveDefinition(prefix + suffixes[i]);
            if (definition != null)
            {
                fallbackDefinitions.Add(definition);
            }
        }

        return fallbackDefinitions.Count > 0 ? fallbackDefinitions.ToArray() : System.Array.Empty<PrototypeRpgEncounterDefinition>();
    }

    private string BuildPreviewEncounterDefinitionPrefix(string dungeonId, string routeId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return string.Empty;
        }

        bool isBeta = dungeonId == "dungeon-beta";
        bool isRisky = NormalizeRouteChoiceId(routeId) == RiskyRouteId;
        return (isBeta ? "beta" : "alpha") + "-" + (isRisky ? "risky" : "safe");
    }

    private string BuildEncounterEnemyTypeSummary(PrototypeRpgEncounterDefinition[] encounterDefinitions)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        for (int i = 0; i < encounterDefinitions.Length; i++)
        {
            PrototypeRpgEncounterDefinition encounterDefinition = encounterDefinitions[i];
            if (encounterDefinition == null || encounterDefinition.EnemyMembers == null)
            {
                continue;
            }

            for (int memberIndex = 0; memberIndex < encounterDefinition.EnemyMembers.Length; memberIndex++)
            {
                PrototypeRpgEncounterMemberDefinition memberDefinition = encounterDefinition.EnemyMembers[memberIndex];
                PrototypeRpgEnemyDefinition enemyDefinition = memberDefinition != null ? PrototypeRpgEnemyCatalog.ResolveDefinition(memberDefinition.EnemyId) : null;
                string typeLabel = enemyDefinition != null ? enemyDefinition.TypeLabel : string.Empty;
                if (string.IsNullOrEmpty(typeLabel))
                {
                    continue;
                }

                if (!counts.ContainsKey(typeLabel))
                {
                    counts[typeLabel] = 0;
                }

                counts[typeLabel]++;
            }
        }

        if (counts.Count == 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in counts)
        {
            parts.Add(pair.Key + " x" + pair.Value);
        }

        return string.Join(" / ", parts.ToArray());
    }

    private string BuildEncounterPatternSummary(PrototypeRpgEncounterDefinition[] encounterDefinitions)
    {
        HashSet<string> patterns = new HashSet<string>();
        for (int i = 0; i < encounterDefinitions.Length; i++)
        {
            PrototypeRpgEncounterDefinition encounterDefinition = encounterDefinitions[i];
            if (encounterDefinition == null || encounterDefinition.EnemyMembers == null)
            {
                continue;
            }

            for (int memberIndex = 0; memberIndex < encounterDefinition.EnemyMembers.Length; memberIndex++)
            {
                PrototypeRpgEncounterMemberDefinition memberDefinition = encounterDefinition.EnemyMembers[memberIndex];
                PrototypeRpgEnemyDefinition enemyDefinition = memberDefinition != null ? PrototypeRpgEnemyCatalog.ResolveDefinition(memberDefinition.EnemyId) : null;
                if (enemyDefinition == null)
                {
                    continue;
                }

                string patternLabel = enemyDefinition.BehaviorHint != null && !string.IsNullOrEmpty(enemyDefinition.BehaviorHint.ShortLabel)
                    ? enemyDefinition.BehaviorHint.ShortLabel
                    : !string.IsNullOrEmpty(enemyDefinition.RoleLabel)
                        ? enemyDefinition.RoleLabel
                        : string.Empty;
                if (!string.IsNullOrEmpty(patternLabel))
                {
                    patterns.Add(patternLabel);
                }
            }
        }

        if (patterns.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(" / ", new List<string>(patterns).ToArray());
    }

    private string BuildJoinedPreviewSummary(string left, string right)
    {
        if (string.IsNullOrEmpty(left))
        {
            return string.IsNullOrEmpty(right) ? string.Empty : right;
        }

        if (string.IsNullOrEmpty(right) || right == left)
        {
            return left;
        }

        return left + " | " + right;
    }

    private string BuildRecommendedPowerBand(int recommendedPowerValue)
    {
        if (recommendedPowerValue <= 0)
        {
            return "Unknown";
        }

        if (recommendedPowerValue <= 8)
        {
            return "Low";
        }

        if (recommendedPowerValue <= 14)
        {
            return "Standard";
        }

        if (recommendedPowerValue <= 20)
        {
            return "High";
        }

        return "Severe";
    }

    private string BuildRecommendedPowerSustainRiskHint(string dungeonId, int recommendedPowerValue)
    {
        DungeonIdentityTemplate dungeonTemplate = GetDungeonTemplateById(dungeonId);
        if (dungeonTemplate == null)
        {
            return recommendedPowerValue > 0 ? "Plan around the recommended power." : "No recommended power data.";
        }

        if (dungeonId == "dungeon-beta")
        {
            return recommendedPowerValue > 0
                ? "Goblin-heavy pressure asks for sturdier sustain."
                : "Goblin-heavy pressure is expected.";
        }

        return recommendedPowerValue > 0
            ? "Recover-friendly path keeps sustain safer."
            : "Recover-friendly route is available.";
    }

    private int GetPreviewPendingBonusRewardHint(string dungeonId, string routeId, string rewardLabel)
    {
        if (_eliteBonusRewardPending <= 0)
        {
            return 0;
        }

        if (!string.Equals(_currentDungeonId, dungeonId, System.StringComparison.Ordinal) || !string.Equals(_selectedRouteId, NormalizeRouteChoiceId(routeId), System.StringComparison.Ordinal))
        {
            return 0;
        }

        if (!string.IsNullOrEmpty(rewardLabel) && !string.IsNullOrEmpty(_eliteRewardLabel) && _eliteRewardLabel != "None" && rewardLabel != _eliteRewardLabel)
        {
            return 0;
        }

        return _eliteBonusRewardPending;
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
        PrototypeRpgDungeonSelectionPreviewData preview = BuildSelectedDungeonSelectionPreviewData(dungeonId);
        return preview != null && preview.EncounterPreview != null && !string.IsNullOrEmpty(preview.EncounterPreview.SummaryText)
            ? preview.EncounterPreview.SummaryText
            : "None";
    }

    private string BuildRoutePreviewSummaryText(string routeId)
    {
        return BuildRoutePreviewSummaryText(_currentDungeonId, routeId);
    }

    private string BuildRoutePreviewSummaryText(string dungeonId, string routeId)
    {
        PrototypeRpgRouteEncounterPreview routePreview = BuildRouteEncounterPreviewData(dungeonId, routeId);
        PrototypeRpgRouteRewardPreview rewardPreview = BuildRouteRewardPreviewData(dungeonId, routeId);
        if (routePreview == null || string.IsNullOrEmpty(routePreview.RouteLabel))
        {
            return "None";
        }

        return routePreview.RouteLabel + " | " + routePreview.RouteRiskLabel + " Risk | Rooms: " + routePreview.RoomPathSummaryText + " | Encounters: " + routePreview.EncounterSummaryText + " | Event: " + routePreview.EventFocusText + " | Reward: " + rewardPreview.RewardBreakdownText;
    }

    private string BuildRouteRewardPreviewText()
    {
        return BuildRouteRewardPreviewText(_currentDungeonId);
    }

    private string BuildRouteRewardPreviewText(string dungeonId)
    {
        PrototypeRpgRouteRewardPreview route1 = BuildRouteRewardPreviewData(dungeonId, SafeRouteId);
        PrototypeRpgRouteRewardPreview route2 = BuildRouteRewardPreviewData(dungeonId, RiskyRouteId);
        if (route1 == null || route2 == null || string.IsNullOrEmpty(route1.RouteLabel) || string.IsNullOrEmpty(route2.RouteLabel))
        {
            return "None";
        }

        return route1.SummaryText + " | " + route2.SummaryText;
    }

    private string BuildRouteEventPreviewText()
    {
        return BuildRouteEventPreviewText(_currentDungeonId);
    }

    private string BuildRouteEventPreviewText(string dungeonId)
    {
        PrototypeRpgRouteEventPreview route1 = BuildRouteEventPreviewData(dungeonId, SafeRouteId);
        PrototypeRpgRouteEventPreview route2 = BuildRouteEventPreviewData(dungeonId, RiskyRouteId);
        if (route1 == null || route2 == null || string.IsNullOrEmpty(route1.RouteLabel) || string.IsNullOrEmpty(route2.RouteLabel))
        {
            return "None";
        }

        return route1.SummaryText + " | " + route2.SummaryText;
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
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.Definition != null && !string.IsNullOrEmpty(encounter.Definition.RoomTypeLabel))
        {
            return encounter.Definition.RoomTypeLabel;
        }

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
        string routeId = !string.IsNullOrEmpty(_hoverRouteChoiceId)
            ? _hoverRouteChoiceId
            : _selectedRouteChoiceId;
        DungeonRouteTemplate template = GetRouteTemplateById(_currentDungeonId, routeId);
        if (template == null)
        {
            DungeonIdentityTemplate dungeonTemplate = GetCurrentDungeonTemplate();
            return dungeonTemplate == null
                ? "Review the linked dungeon and select a route before dispatching."
                : GetHomeCityDisplayName() + " -> " + dungeonTemplate.DungeonLabel + " | " + dungeonTemplate.DangerLabel + " | " + dungeonTemplate.StyleLabel;
        }

        PrototypeRpgRouteEncounterPreview routePreview = BuildRouteEncounterPreviewData(_currentDungeonId, template.RouteId);
        PrototypeRpgRouteRewardPreview rewardPreview = BuildRouteRewardPreviewData(_currentDungeonId, template.RouteId);
        return routePreview.RouteLabel + " | " + routePreview.RouteDescriptionText + " | " + routePreview.EncounterSummaryText + " | Event: " + routePreview.EventFocusText + " | Reward: " + rewardPreview.RewardBreakdownText;
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
        if (encounter != null)
        {
            return encounter.Definition != null && !string.IsNullOrEmpty(encounter.Definition.DisplayName) ? encounter.Definition.DisplayName : encounter.DisplayName;
        }

        return _dungeonRunState == DungeonRunState.Battle ? _currentDungeonName : "None";
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
        DungeonEncounterRuntimeData eliteEncounter = GetEliteEncounter();
        if (eliteEncounter != null && eliteEncounter.Definition != null && !string.IsNullOrEmpty(eliteEncounter.Definition.DisplayName))
        {
            return eliteEncounter.Definition.DisplayName;
        }

        if (!string.IsNullOrEmpty(_eliteName) && _eliteName != "None")
        {
            return _eliteName;
        }

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

        return !string.IsNullOrEmpty(monster.RoleLabel) ? monster.RoleLabel : GetFallbackMonsterRoleLabel(monster.EncounterRole, monster.IsElite);
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

        return PrototypeCombatRuleResolver.ResolveSkillTargetKind(skillDefinition, GetFallbackResolvedSkillTargetKind(member));
    }

    private string GetResolvedSkillEffectType(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition = null)
    {
        if (skillDefinition == null)
        {
            skillDefinition = ResolveMemberSkillDefinition(member);
        }

        return PrototypeCombatRuleResolver.ResolveSkillEffectType(skillDefinition, GetFallbackResolvedSkillEffectType(member));
    }

    private int GetResolvedSkillPower(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition = null)
    {
        if (skillDefinition == null)
        {
            skillDefinition = ResolveMemberSkillDefinition(member);
        }

        return PrototypeCombatRuleResolver.ResolveSkillPower(skillDefinition, GetFallbackResolvedSkillPower(member));
    }

    private string GetFallbackResolvedSkillTargetKind(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        switch (member.SkillType)
        {
            case PartySkillType.AllEnemies:
                return PrototypeCombatTargetKindKeys.AllEnemies;
            case PartySkillType.PartyHeal:
                return PrototypeCombatTargetKindKeys.AllAllies;
            default:
                return PrototypeCombatTargetKindKeys.SingleEnemy;
        }
    }

    private string GetFallbackResolvedSkillEffectType(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        switch (member.SkillType)
        {
            case PartySkillType.PartyHeal:
                return PrototypeCombatEffectKeys.Heal;
            case PartySkillType.Finisher:
                return PrototypeCombatEffectKeys.FinisherDamage;
            default:
                return PrototypeCombatEffectKeys.Damage;
        }
    }

    private int GetFallbackResolvedSkillPower(DungeonPartyMemberRuntimeData member)
    {
        if (member != null && member.SkillPower > 0)
        {
            return member.SkillPower;
        }

        return member != null ? Mathf.Max(1, member.Attack + 1) : 1;
    }

    private PrototypeCombatResolutionRecord BuildCurrentPartyActionResolutionRecord(BattleActionType action, DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition = null)
    {
        if (action == BattleActionType.Retreat)
        {
            return PrototypeCombatRuleResolver.BuildRetreatAction(member != null ? member.MemberId : string.Empty, _currentActorIndex, member != null ? member.DisplayName : ActiveDungeonPartyText);
        }

        if (member == null)
        {
            return new PrototypeCombatResolutionRecord();
        }

        if (action == BattleActionType.Skill)
        {
            if (skillDefinition == null)
            {
                skillDefinition = ResolveMemberSkillDefinition(member);
            }

            return PrototypeCombatRuleResolver.BuildSkillAction(
                member.MemberId,
                member.PartySlotIndex,
                member.DisplayName,
                skillDefinition,
                member.DefaultSkillId,
                member.SkillName,
                GetFallbackResolvedSkillTargetKind(member),
                GetFallbackResolvedSkillEffectType(member),
                GetFallbackResolvedSkillPower(member));
        }

        if (action == BattleActionType.Attack)
        {
            return PrototypeCombatRuleResolver.BuildAttackAction(member.MemberId, member.PartySlotIndex, member.DisplayName, Mathf.Max(1, member.Attack));
        }

        return new PrototypeCombatResolutionRecord();
    }

    private PrototypeCombatResolutionRecord BuildPendingEnemyActionResolutionRecord(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        if (monster == null)
        {
            return new PrototypeCombatResolutionRecord();
        }

        PrototypeRpgEnemyIntentDefinition intentDefinition = ResolveEnemyIntentDefinition(monster, useSpecial);
        string targetPolicyKey = GetEnemyIntentTargetPolicyKey(monster, useSpecial);
        DungeonPartyMemberRuntimeData targetMember = targetIndex >= 0 ? GetPartyMemberAtIndex(targetIndex) : null;
        string actionLabel = PrototypeCombatIntentPolicy.ResolveActionLabel(monster.Definition, intentDefinition, useSpecial, useSpecial ? monster.SpecialActionName : "Attack");
        string patternKey = PrototypeCombatIntentPolicy.ResolvePatternKey(monster.Definition, targetPolicyKey, useSpecial);
        int powerValue = GetEnemyActionPower(monster, useSpecial);
        PrototypeCombatResolutionRecord record = PrototypeCombatRuleResolver.BuildEnemyAction(
            monster.MonsterId,
            _enemyTurnMonsterCursor,
            monster.DisplayName,
            actionLabel,
            targetPolicyKey,
            IsPartyWideEliteSpecial(monster, useSpecial) ? PrototypeCombatTargetKindKeys.AllAllies : PrototypeCombatTargetKindKeys.SingleEnemy,
            PrototypeCombatIntentPolicy.ResolveEffectTypeKey(intentDefinition, useSpecial),
            powerValue,
            targetMember != null ? targetMember.MemberId : string.Empty,
            targetMember != null ? targetMember.DisplayName : string.Empty,
            targetIndex,
            patternKey);
        string hintText = intentDefinition != null && !string.IsNullOrEmpty(intentDefinition.DisplayHintText)
            ? intentDefinition.DisplayHintText
            : !string.IsNullOrEmpty(monster.BehaviorHintText) ? monster.BehaviorHintText : string.Empty;
        record.FeedbackText = PrototypeCombatIntentPolicy.BuildPreviewText(monster.DisplayName, GetMonsterRoleText(monster), record.ActionLabel, record.TargetPolicyKey, targetMember != null ? targetMember.DisplayName : string.Empty, hintText, powerValue, patternKey);
        record.LogText = record.FeedbackText;
        record.SummaryText = PrototypeCombatIntentPolicy.ResolveDisplayLabel(intentDefinition, record.ActionLabel);
        return record;
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

    private PrototypeRpgEnemyIntentDefinition ResolveEnemyIntentDefinition(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        return PrototypeCombatIntentPolicy.ResolveIntentDefinition(monster != null ? monster.Definition : null, GetFallbackEnemyTargetPolicyKey(monster, useSpecial), useSpecial);
    }

    private string GetFallbackEnemyTargetPolicyKey(DungeonMonsterRuntimeData monster, bool useSpecial = false)
    {
        if (monster == null)
        {
            return PrototypeCombatTargetPolicyKeys.FrontmostLiving;
        }

        return PrototypeCombatIntentPolicy.ResolveFallbackTargetPolicyKey(monster.RoleTag, monster.IsElite, useSpecial);
    }
    private int[] BuildLivingPartyMemberIndices()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return System.Array.Empty<int>();
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && !member.IsDefeated && member.CurrentHp > 0)
            {
                indices.Add(i);
            }
        }

        return indices.Count > 0 ? indices.ToArray() : System.Array.Empty<int>();
    }

    private int[] BuildLivingPartyMemberHpValues(int[] livingIndices)
    {
        if (livingIndices == null || livingIndices.Length <= 0)
        {
            return System.Array.Empty<int>();
        }

        int[] values = new int[livingIndices.Length];
        for (int i = 0; i < livingIndices.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(livingIndices[i]);
            values[i] = member != null ? Mathf.Max(0, member.CurrentHp) : int.MaxValue;
        }

        return values;
    }

    private string GetEnemyIntentTargetPolicyKey(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        PrototypeRpgEnemyIntentDefinition definition = ResolveEnemyIntentDefinition(monster, useSpecial);
        return PrototypeCombatIntentPolicy.ResolveTargetPolicyKey(definition, GetFallbackEnemyTargetPolicyKey(monster, useSpecial), IsPartyWideEliteSpecial(monster, useSpecial));
    }

    private int GetMonsterTargetPartyMemberIndex(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        int[] livingIndices = BuildLivingPartyMemberIndices();
        int[] currentHpValues = BuildLivingPartyMemberHpValues(livingIndices);
        return PrototypeCombatTargetPolicy.ResolveLivingTargetIndex(GetEnemyIntentTargetPolicyKey(monster, useSpecial), livingIndices, currentHpValues, Random.Range(0, int.MaxValue));
    }

    private bool ShouldUseEliteSpecialAttack(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return false;
        }

        return PrototypeCombatIntentPolicy.ShouldUseSpecialAttack(monster.IsElite, monster.RoleTag, monster.TurnsActed);
    }

    private int GetEnemyActionPower(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return 1;
        }

        PrototypeRpgEnemyIntentDefinition definition = ResolveEnemyIntentDefinition(monster, useSpecial);
        return PrototypeCombatIntentPolicy.ResolveActionPower(monster.Definition, definition, useSpecial, monster.Attack, monster.SpecialAttack);
    }
    private string GetEnemyActionLabel(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return "Attack";
        }

        PrototypeRpgEnemyIntentDefinition definition = ResolveEnemyIntentDefinition(monster, useSpecial);
        return PrototypeCombatIntentPolicy.ResolveActionLabel(monster.Definition, definition, useSpecial, useSpecial ? monster.SpecialActionName : "Attack");
    }

    private string BuildEnemyIntentText(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        PrototypeCombatResolutionRecord resolution = BuildPendingEnemyActionResolutionRecord(monster, targetIndex, useSpecial);
        return !string.IsNullOrEmpty(resolution.FeedbackText) ? resolution.FeedbackText : "Enemy intent.";
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
            return GetResolvedSkillDisplayName(member);
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
        _currentEnemyIntentSnapshot = new PrototypeEnemyIntentSnapshot();

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            if (_activeMonsters[i] != null)
            {
                _activeMonsters[i].RuntimeState?.ClearIntent();
            }
        }
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
        int grantedLoot = encounterLoot;
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

            bool useSpecial = ShouldUseEliteSpecialAttack(monster);
            int targetIndex = GetMonsterTargetPartyMemberIndex(monster, useSpecial);
            if (targetIndex < 0)
            {
                continue;
            }

            PrototypeCombatResolutionRecord pendingResolution = BuildPendingEnemyActionResolutionRecord(monster, targetIndex, useSpecial);
            float telegraphDuration = useSpecial ? EnemyIntentTelegraphSeconds + 0.12f : EnemyIntentTelegraphSeconds;
            _enemyTurnMonsterCursor = displayIndex;
            _pendingEnemyTargetIndex = targetIndex;
            _pendingEnemyAttackPower = pendingResolution.PowerValue > 0 ? pendingResolution.PowerValue : GetEnemyActionPower(monster, useSpecial);
            _pendingEnemyActionLabel = !string.IsNullOrEmpty(pendingResolution.ActionLabel) ? pendingResolution.ActionLabel : GetEnemyActionLabel(monster, useSpecial);
            _pendingEnemyUsedSpecialAttack = useSpecial;
            SetActiveBattleMonster(monster);
            _enemyIntentTelegraphActive = true;
            _enemyIntentResolveAtTime = Time.unscaledTime + telegraphDuration;

            for (int monsterIndex = 0; monsterIndex < _activeMonsters.Count; monsterIndex++)
            {
                if (_activeMonsters[monsterIndex] != null)
                {
                    _activeMonsters[monsterIndex].RuntimeState?.ClearIntent();
                }
            }

            RecordEnemyTurnStartEvent(monster);
            _currentEnemyIntentSnapshot = BuildEnemyIntentSnapshot(monster, targetIndex, useSpecial);
            _enemyIntentText = !string.IsNullOrEmpty(pendingResolution.FeedbackText)
                ? pendingResolution.FeedbackText
                : string.IsNullOrEmpty(_currentEnemyIntentSnapshot.PreviewText)
                    ? BuildEnemyIntentText(monster, targetIndex, useSpecial)
                    : _currentEnemyIntentSnapshot.PreviewText;
            monster.RuntimeState?.SetIntent(
                _currentEnemyIntentSnapshot.IntentKey,
                _currentEnemyIntentSnapshot.TargetPatternKey,
                _enemyIntentText,
                _currentEnemyIntentSnapshot.PredictedValue,
                _currentEnemyIntentSnapshot.TargetId);
            RecordBattleEvent(
                PrototypeBattleEventKeys.EnemyIntentShown,
                monster.MonsterId,
                _currentEnemyIntentSnapshot.TargetId,
                _currentEnemyIntentSnapshot.PredictedValue,
                _enemyIntentText,
                actionKey: _currentEnemyIntentSnapshot.ActionKey,
                phaseKey: "enemy_turn",
                actorName: monster.DisplayName,
                targetName: _currentEnemyIntentSnapshot.TargetName,
                shortText: !string.IsNullOrEmpty(pendingResolution.SummaryText) ? pendingResolution.SummaryText : _pendingEnemyActionLabel);
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

        PrototypeRpgSkillDefinition resolvedSkillDefinition = action == BattleActionType.Skill ? ResolveCurrentActorSkillDefinition() : null;
        PrototypeCombatResolutionRecord resolution = BuildCurrentPartyActionResolutionRecord(action, member, resolvedSkillDefinition);
        string actionLabel = !string.IsNullOrEmpty(resolution.ActionLabel) ? resolution.ActionLabel : GetBattleActionDisplayName(action, member);
        string actionKey = !string.IsNullOrEmpty(resolution.ActionKey) ? resolution.ActionKey : GetBattleActionKey(action);

        RecordBattleEvent(
            PrototypeBattleEventKeys.ActionSelected,
            member != null ? member.MemberId : string.Empty,
            string.Empty,
            0,
            (member != null ? member.DisplayName : ActiveDungeonPartyText) + " selected " + actionLabel + ".",
            actionKey: actionKey,
            skillId: resolution.IsSkillAction ? resolution.SkillId : string.Empty,
            phaseKey: "party_turn",
            actorName: member != null ? member.DisplayName : ActiveDungeonPartyText,
            shortText: actionLabel);

        if (action == BattleActionType.Retreat)
        {
            RecordBattleEvent(
                PrototypeBattleEventKeys.RetreatConfirmed,
                member != null ? member.MemberId : string.Empty,
                _currentDungeonId,
                0,
                ActiveDungeonPartyText + " confirmed retreat.",
                actionKey: PrototypeCombatActionKeys.Retreat,
                phaseKey: "retreat",
                actorName: member != null ? member.DisplayName : ActiveDungeonPartyText,
                targetName: _currentDungeonName,
                shortText: "Retreat confirmed");
            AppendBattleLog("The party retreats from battle and abandons the run.");
            FinishDungeonRun(RunResultState.Retreat, BattleState.Retreat, false, 0, ActiveDungeonPartyText + " retreated from " + _currentDungeonName + " with no loot.");
            return true;
        }

        ClearBattleHoverState();
        _hoverBattleAction = action;
        _queuedBattleAction = action;
        LockBattleInput();

        if (action == BattleActionType.Attack || resolution.RequiresTarget)
        {
            _battleState = BattleState.PartyTargetSelect;
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            ClearBattleInputLock();
            SetBattleFeedbackText(!string.IsNullOrEmpty(resolution.FeedbackText) ? resolution.FeedbackText : actionLabel + " selected.");
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        if (resolution.TargetKind == PrototypeCombatTargetKindKeys.AllEnemies && resolution.EffectTypeKey == PrototypeCombatEffectKeys.Damage)
        {
            int hitCount = 0;
            int totalDamage = 0;
            List<DungeonMonsterRuntimeData> targetMonsters = GetTargetableBattleMonsters();
            for (int i = 0; i < targetMonsters.Count; i++)
            {
                int applied = ApplyBattleDamageToMonster(member, targetMonsters[i], resolution.PowerValue, new Color(0.46f, 0.75f, 1f, 1f));
                if (applied > 0)
                {
                    totalDamage += applied;
                    hitCount += 1;
                }
            }

            resolution.ResolvedAmount = totalDamage;
            resolution.TotalResolvedAmount = totalDamage;
            resolution.AffectedCount = hitCount;
            resolution.ConditionApplied = PrototypeCombatRuleResolver.IsConditionApplied(resolution.ConditionKey, member.Attack, 0, hitCount, totalDamage);
            resolution.DidResolve = true;
            string feedbackText = PrototypeCombatRuleResolver.BuildPartyResolutionFeedbackText(resolution, "All enemies", totalDamage, hitCount, resolution.ConditionApplied);
            string logText = PrototypeCombatRuleResolver.BuildPartyResolutionLogText(resolution, member.DisplayName, "All enemies", totalDamage, hitCount, resolution.ConditionApplied);
            AppendBattleLog(logText);
            SetBattleFeedbackText(feedbackText);
            RecordBattleEvent(
                PrototypeBattleEventKeys.SkillResolved,
                member.MemberId,
                string.Empty,
                totalDamage,
                member.DisplayName + " resolved " + actionLabel + " on all enemies.",
                actionKey: PrototypeCombatActionKeys.Skill,
                skillId: resolution.SkillId,
                phaseKey: "resolution",
                actorName: member.DisplayName,
                targetName: "All enemies",
                shortText: PrototypeCombatRuleResolver.BuildResolutionShortText(resolution, resolution.ConditionApplied));
        }
        else if (resolution.TargetKind == PrototypeCombatTargetKindKeys.AllAllies && resolution.EffectTypeKey == PrototypeCombatEffectKeys.Heal)
        {
            int totalRecovered = 0;
            int healedCount = 0;
            List<int> livingAllyIndices = GetLivingAllies();
            for (int i = 0; i < livingAllyIndices.Count; i++)
            {
                int memberIndex = livingAllyIndices[i];
                DungeonPartyMemberRuntimeData ally = GetPartyMemberAtIndex(memberIndex);
                int recovered = ApplyBattleHealToPartyMember(member, ally, memberIndex, resolution.PowerValue, new Color(0.56f, 1f, 0.68f, 1f));
                totalRecovered += recovered;
                if (recovered > 0)
                {
                    healedCount += 1;
                }
            }

            resolution.ResolvedAmount = totalRecovered;
            resolution.TotalResolvedAmount = totalRecovered;
            resolution.AffectedCount = healedCount;
            resolution.ConditionApplied = PrototypeCombatRuleResolver.IsConditionApplied(resolution.ConditionKey, member.Attack, 0, healedCount, totalRecovered);
            resolution.DidResolve = true;
            string feedbackText = PrototypeCombatRuleResolver.BuildPartyResolutionFeedbackText(resolution, "All allies", totalRecovered, healedCount, resolution.ConditionApplied);
            string logText = PrototypeCombatRuleResolver.BuildPartyResolutionLogText(resolution, member.DisplayName, "All allies", totalRecovered, healedCount, resolution.ConditionApplied);
            AppendBattleLog(logText);
            SetBattleFeedbackText(feedbackText);
            RecordBattleEvent(
                PrototypeBattleEventKeys.SkillResolved,
                member.MemberId,
                string.Empty,
                totalRecovered,
                member.DisplayName + " resolved " + actionLabel + " for the party.",
                actionKey: PrototypeCombatActionKeys.Skill,
                skillId: resolution.SkillId,
                phaseKey: "resolution",
                actorName: member.DisplayName,
                targetName: "All allies",
                shortText: PrototypeCombatRuleResolver.BuildResolutionShortText(resolution, resolution.ConditionApplied));
        }
        else
        {
            resolution.DidResolve = true;
            AppendBattleLog(member.DisplayName + " used " + actionLabel + ".");
            SetBattleFeedbackText(actionLabel + " resolved.");
            RecordBattleEvent(
                PrototypeBattleEventKeys.SkillResolved,
                member.MemberId,
                string.Empty,
                0,
                member.DisplayName + " resolved " + actionLabel + ".",
                actionKey: PrototypeCombatActionKeys.Skill,
                skillId: resolution.SkillId,
                phaseKey: "resolution",
                actorName: member.DisplayName,
                shortText: PrototypeCombatRuleResolver.BuildResolutionShortText(resolution, false));
        }

        AdvanceBattleAfterPartyAction();
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
        PrototypeRpgSkillDefinition resolvedSkillDefinition = _queuedBattleAction == BattleActionType.Skill ? ResolveMemberSkillDefinition(member) : null;
        PrototypeCombatResolutionRecord resolution = BuildCurrentPartyActionResolutionRecord(_queuedBattleAction, member, resolvedSkillDefinition);
        string actionKey = !string.IsNullOrEmpty(resolution.ActionKey) ? resolution.ActionKey : PrototypeCombatActionKeys.Attack;
        string actionName = !string.IsNullOrEmpty(resolution.ActionLabel) ? resolution.ActionLabel : "Attack";
        int targetHpBefore = targetMonster.CurrentHp;
        bool conditionApplied = PrototypeCombatRuleResolver.IsConditionApplied(resolution.ConditionKey, member.Attack, targetHpBefore, 1, 0);
        int damage = resolution.IsSkillAction
            ? PrototypeCombatRuleResolver.ResolveSingleTargetDamage(resolution.EffectTypeKey, resolution.ConditionKey, resolution.PowerValue, member.Attack, targetHpBefore)
            : Mathf.Max(1, resolution.PowerValue);

        RecordBattleEvent(
            PrototypeBattleEventKeys.TargetSelected,
            member.MemberId,
            targetMonster.MonsterId,
            GetBattleMonsterDisplayIndex(targetMonster.MonsterId),
            member.DisplayName + " targeted " + targetMonster.DisplayName + ".",
            actionKey: actionKey,
            skillId: resolution.SkillId,
            phaseKey: "target_select",
            actorName: member.DisplayName,
            targetName: targetMonster.DisplayName,
            shortText: "Target locked");
        int appliedDamage = ApplyBattleDamageToMonster(member, targetMonster, damage, new Color(1f, 0.48f, 0.30f, 1f));
        resolution.ConditionApplied = conditionApplied;
        resolution.ResolvedAmount = appliedDamage;
        resolution.TotalResolvedAmount = appliedDamage;
        resolution.AffectedCount = appliedDamage > 0 ? 1 : 0;
        resolution.DidResolve = true;
        string feedbackText = PrototypeCombatRuleResolver.BuildPartyResolutionFeedbackText(resolution, targetMonster.DisplayName, appliedDamage, resolution.AffectedCount, conditionApplied);
        string logText = PrototypeCombatRuleResolver.BuildPartyResolutionLogText(resolution, member.DisplayName, targetMonster.DisplayName, appliedDamage, resolution.AffectedCount, conditionApplied);
        AppendBattleLog(logText);
        SetBattleFeedbackText(feedbackText);
        RecordBattleEvent(
            resolution.IsSkillAction ? PrototypeBattleEventKeys.SkillResolved : PrototypeBattleEventKeys.AttackResolved,
            member.MemberId,
            targetMonster.MonsterId,
            appliedDamage,
            member.DisplayName + " resolved " + actionName + " on " + targetMonster.DisplayName + ".",
            actionKey: actionKey,
            skillId: resolution.SkillId,
            phaseKey: "resolution",
            actorName: member.DisplayName,
            targetName: targetMonster.DisplayName,
            shortText: PrototypeCombatRuleResolver.BuildResolutionShortText(resolution, conditionApplied));
        AdvanceBattleAfterPartyAction();
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
        PrototypeCombatResolutionRecord resolution = BuildPendingEnemyActionResolutionRecord(_activeBattleMonster, targetIndex, _pendingEnemyUsedSpecialAttack);
        bool usePartyWideEliteSpecial = resolution.TargetKind == PrototypeCombatTargetKindKeys.AllAllies;
        if (!usePartyWideEliteSpecial && (_activeDungeonParty == null || _activeDungeonParty.Members == null || targetIndex < 0 || targetIndex >= _activeDungeonParty.Members.Length))
        {
            if (!TryQueueEnemyIntent(_enemyTurnMonsterCursor + 1))
            {
                ResumePlayerTurnFromStartIndex(0);
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
                ResumePlayerTurnFromStartIndex(0);
            }

            return;
        }

        int damage = Mathf.Max(1, resolution.PowerValue > 0 ? resolution.PowerValue : _activeBattleMonster.Attack);
        string actionLabel = string.IsNullOrEmpty(resolution.ActionLabel) ? "Attack" : resolution.ActionLabel;
        if (usePartyWideEliteSpecial)
        {
            int sweepDamage = Mathf.Max(1, damage - 4);
            int hitCount = 0;
            int totalDamage = 0;
            List<int> livingAllyIndices = GetLivingAllies();
            for (int i = 0; i < livingAllyIndices.Count; i++)
            {
                int memberIndex = livingAllyIndices[i];
                int applied = ApplyBattleDamageToPartyMember(_activeBattleMonster, memberIndex, sweepDamage, new Color(1f, 0.34f, 0.42f, 1f));
                if (applied > 0)
                {
                    totalDamage += applied;
                    hitCount += 1;
                }
            }

            resolution.ResolvedAmount = totalDamage;
            resolution.TotalResolvedAmount = totalDamage;
            resolution.AffectedCount = hitCount;
            resolution.DidResolve = true;
            string feedbackText = PrototypeCombatRuleResolver.BuildEnemyResolutionFeedbackText(resolution, _activeBattleMonster.DisplayName, "All living allies", totalDamage, hitCount);
            string logText = PrototypeCombatRuleResolver.BuildEnemyResolutionLogText(resolution, _activeBattleMonster.DisplayName, "All living allies", totalDamage, hitCount);
            AppendBattleLog(logText);
            SetBattleFeedbackText(feedbackText);
            RecordBattleEvent(
                _pendingEnemyUsedSpecialAttack ? PrototypeBattleEventKeys.SkillResolved : PrototypeBattleEventKeys.AttackResolved,
                _activeBattleMonster.MonsterId,
                string.Empty,
                totalDamage,
                _activeBattleMonster.DisplayName + " resolved " + actionLabel + " on all allies.",
                actionKey: resolution.ActionKey,
                phaseKey: "enemy_turn",
                actorName: _activeBattleMonster.DisplayName,
                targetName: "All living allies",
                shortText: PrototypeCombatRuleResolver.BuildResolutionShortText(resolution, false));
            _activeBattleMonster.TurnsActed += 1;
        }
        else
        {
            int appliedDamage = ApplyBattleDamageToPartyMember(_activeBattleMonster, targetIndex, damage, new Color(1f, 0.40f, 0.35f, 1f));
            resolution.ResolvedAmount = appliedDamage;
            resolution.TotalResolvedAmount = appliedDamage;
            resolution.AffectedCount = appliedDamage > 0 ? 1 : 0;
            resolution.DidResolve = true;
            string feedbackText = PrototypeCombatRuleResolver.BuildEnemyResolutionFeedbackText(resolution, _activeBattleMonster.DisplayName, targetMember.DisplayName, appliedDamage, resolution.AffectedCount);
            string logText = PrototypeCombatRuleResolver.BuildEnemyResolutionLogText(resolution, _activeBattleMonster.DisplayName, targetMember.DisplayName, appliedDamage, resolution.AffectedCount);
            AppendBattleLog(logText);
            SetBattleFeedbackText(feedbackText);
            RecordBattleEvent(
                _pendingEnemyUsedSpecialAttack ? PrototypeBattleEventKeys.SkillResolved : PrototypeBattleEventKeys.AttackResolved,
                _activeBattleMonster.MonsterId,
                targetMember.MemberId,
                appliedDamage,
                _activeBattleMonster.DisplayName + " resolved " + actionLabel + " on " + targetMember.DisplayName + ".",
                actionKey: resolution.ActionKey,
                phaseKey: "enemy_turn",
                actorName: _activeBattleMonster.DisplayName,
                targetName: targetMember.DisplayName,
                shortText: PrototypeCombatRuleResolver.BuildResolutionShortText(resolution, false));
            _activeBattleMonster.TurnsActed += 1;
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

        ResumePlayerTurnFromStartIndex(0);
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
        PrototypeRpgEnemyDefinition definition = PrototypeRpgEnemyCatalog.ResolveDefinition(monsterId)
            ?? PrototypeRpgEnemyCatalog.BuildFallbackDefinition(
                monsterId,
                displayName,
                monsterType,
                ResolveMonsterRoleTag(encounterRole, isElite),
                GetFallbackMonsterRoleLabel(encounterRole, isElite),
                hp,
                attack,
                isElite,
                ResolveDefaultEnemyIntentKey(targetPattern),
                ResolveSpecialEnemyIntentKey(isElite, encounterRole, specialActionName),
                ResolveEnemyBehaviorHintKey(encounterRole, isElite, specialActionName),
                string.Empty,
                DungeonRewardResourceId,
                rewardAmount,
                specialActionName,
                specialAttack,
                isElite ? GetFallbackMonsterRoleLabel(encounterRole, true) : GetFallbackMonsterRoleLabel(encounterRole, false));
        return new DungeonMonsterRuntimeData(definition, monsterId, encounterId, roomIndex, displayName, monsterType, hp, attack, gridPosition, DungeonRewardResourceId, rewardAmount, targetPattern, encounterRole, isElite, specialAttack, specialActionName);
    }

    private DungeonEncounterRuntimeData CreateEncounterRuntime(string definitionId, string encounterId, int roomIndex, string[] monsterIds, bool isEliteEncounter = false)
    {
        PrototypeRpgEncounterDefinition definition = PrototypeRpgEncounterCatalog.ResolveDefinition(definitionId)
            ?? PrototypeRpgEncounterCatalog.BuildFallbackDefinition(
                definitionId,
                encounterId,
                encounterId,
                isEliteEncounter ? "Elite" : "Skirmish",
                isEliteEncounter ? "Elite Chamber" : "Skirmish Room",
                string.Empty,
                _selectedRouteId == RiskyRouteId ? "Risky" : "Safe",
                _currentDungeonId == "dungeon-beta" ? "Dungeon Beta" : "Dungeon Alpha",
                string.Empty,
                string.Empty,
                0,
                isEliteEncounter,
                System.Array.Empty<PrototypeRpgEncounterMemberDefinition>());
        return new DungeonEncounterRuntimeData(definition, encounterId, roomIndex, definition.DisplayName, monsterIds, isEliteEncounter);
    }

    private void SyncEliteDefinitionPresentation()
    {
        DungeonEncounterRuntimeData eliteEncounter = GetEliteEncounter();
        if (eliteEncounter != null && eliteEncounter.Definition != null)
        {
            if (!string.IsNullOrEmpty(eliteEncounter.Definition.DisplayName))
            {
                _eliteName = eliteEncounter.Definition.DisplayName;
            }

            if (!string.IsNullOrEmpty(eliteEncounter.Definition.EliteStyleLabel))
            {
                _eliteType = eliteEncounter.Definition.EliteStyleLabel;
            }

            if (!string.IsNullOrEmpty(eliteEncounter.Definition.RewardLabel))
            {
                _eliteRewardLabel = eliteEncounter.Definition.RewardLabel;
            }

            if (eliteEncounter.Definition.RewardAmountHint > 0)
            {
                _eliteRewardAmount = eliteEncounter.Definition.RewardAmountHint;
            }
        }

        DungeonMonsterRuntimeData eliteMonster = GetEliteMonster();
        if (eliteMonster != null && eliteMonster.Definition != null)
        {
            if (!string.IsNullOrEmpty(eliteMonster.Definition.DisplayName))
            {
                _eliteName = eliteMonster.Definition.DisplayName;
            }

            if (!string.IsNullOrEmpty(eliteMonster.Definition.TraitText))
            {
                _eliteType = eliteMonster.Definition.TraitText;
            }

            if (!string.IsNullOrEmpty(eliteMonster.Definition.RewardLabel))
            {
                _eliteRewardLabel = eliteMonster.Definition.RewardLabel;
            }

            if (eliteMonster.Definition.RewardAmountHint > 0)
            {
                _eliteRewardAmount = eliteMonster.Definition.RewardAmountHint;
            }
        }
    }

    private PrototypeRpgEncounterVariationDefinition ResolveEncounterVariationDefinition(string dungeonId, string routeId)
    {
        return PrototypeRpgEncounterVariationCatalog.ResolveVariation(dungeonId, NormalizeRouteChoiceId(routeId));
    }

    private void ApplyEncounterVariationPresentationHints(PrototypeRpgEncounterVariationDefinition variation)
    {
        if (variation == null || variation.EliteTemplate == null)
        {
            return;
        }

        PrototypeRpgEliteTemplateDefinition eliteTemplate = variation.EliteTemplate;
        if (!string.IsNullOrEmpty(eliteTemplate.EncounterId))
        {
            _eliteEncounterId = eliteTemplate.EncounterId;
        }

        if (!string.IsNullOrEmpty(eliteTemplate.DisplayName))
        {
            _eliteName = eliteTemplate.DisplayName;
        }

        if (!string.IsNullOrEmpty(eliteTemplate.EliteStyleLabel))
        {
            _eliteType = eliteTemplate.EliteStyleLabel;
        }

        if (!string.IsNullOrEmpty(eliteTemplate.RewardPreviewLabel))
        {
            _eliteRewardLabel = eliteTemplate.RewardPreviewLabel;
        }

        if (eliteTemplate.RewardAmountPreviewHint > 0)
        {
            _eliteRewardAmount = eliteTemplate.RewardAmountPreviewHint;
        }
    }

    private void ApplyEncounterDefinitionPresentationHints(PrototypeRpgEncounterDefinition definition)
    {
        if (definition == null || !definition.IsEliteEncounter)
        {
            return;
        }

        if (!string.IsNullOrEmpty(definition.EncounterId))
        {
            _eliteEncounterId = definition.EncounterId;
        }

        if (!string.IsNullOrEmpty(definition.DisplayName))
        {
            _eliteName = definition.DisplayName;
        }

        if (!string.IsNullOrEmpty(definition.EliteStyleLabel))
        {
            _eliteType = definition.EliteStyleLabel;
        }

        if (!string.IsNullOrEmpty(definition.RewardLabel))
        {
            _eliteRewardLabel = definition.RewardLabel;
        }

        if (definition.RewardAmountHint > 0)
        {
            _eliteRewardAmount = definition.RewardAmountHint;
        }
    }

    private void BuildFixedDungeonRoomFromVariation(PrototypeRpgEncounterVariationDefinition variation)
    {
        if (variation == null)
        {
            BuildFixedDungeonRoomFromDefinitions(_currentDungeonId, _selectedRouteId);
            return;
        }

        ApplyEncounterVariationPresentationHints(variation);
        if (variation.RoomTemplates != null)
        {
            for (int i = 0; i < variation.RoomTemplates.Length; i++)
            {
                AppendEncounterTemplateRuntime(variation.RoomTemplates[i]);
            }
        }

        if (variation.EliteTemplate != null)
        {
            AppendEliteTemplateRuntime(variation.EliteTemplate);
        }
    }

    private void BuildFixedDungeonRoomFromDefinitions(string dungeonId, string routeId)
    {
        PrototypeRpgEncounterDefinition[] encounterDefinitions = BuildRouteEncounterDefinitions(dungeonId, routeId);
        int nextSkirmishRoomIndex = 1;
        for (int i = 0; i < encounterDefinitions.Length; i++)
        {
            PrototypeRpgEncounterDefinition definition = encounterDefinitions[i];
            if (definition == null)
            {
                continue;
            }

            bool isEliteEncounter = definition.IsEliteEncounter;
            int roomIndex = isEliteEncounter ? 3 : nextSkirmishRoomIndex++;
            string encounterId = !string.IsNullOrEmpty(definition.EncounterId)
                ? definition.EncounterId
                : isEliteEncounter
                    ? _eliteEncounterId
                    : "encounter-room-" + roomIndex;
            AppendEncounterDefinitionRuntime(definition, encounterId, roomIndex, isEliteEncounter);
        }
    }

    private void AppendEncounterTemplateRuntime(PrototypeRpgEncounterTemplateDefinition template)
    {
        if (template == null)
        {
            return;
        }

        PrototypeRpgEncounterDefinition definition = PrototypeRpgEncounterCatalog.ResolveDefinition(template.EncounterDefinitionId)
            ?? PrototypeRpgEncounterCatalog.BuildFallbackDefinition(
                template.TemplateId,
                template.EncounterId,
                template.DisplayName,
                template.EncounterTypeLabel,
                template.RoomTypeLabel,
                string.Empty,
                _selectedRouteId == RiskyRouteId ? "Risky" : "Safe",
                _currentDungeonId == "dungeon-beta" ? "Dungeon Beta" : "Dungeon Alpha",
                string.Empty,
                string.Empty,
                0,
                false,
                System.Array.Empty<PrototypeRpgEncounterMemberDefinition>());
        string encounterId = !string.IsNullOrEmpty(template.EncounterId) ? template.EncounterId : definition.EncounterId;
        AppendEncounterDefinitionRuntime(definition, encounterId, template.RoomIndex, false);
    }

    private void AppendEliteTemplateRuntime(PrototypeRpgEliteTemplateDefinition eliteTemplate)
    {
        if (eliteTemplate == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(eliteTemplate.EncounterId))
        {
            _eliteEncounterId = eliteTemplate.EncounterId;
        }

        if (!string.IsNullOrEmpty(eliteTemplate.DisplayName))
        {
            _eliteName = eliteTemplate.DisplayName;
        }

        if (!string.IsNullOrEmpty(eliteTemplate.EliteStyleLabel))
        {
            _eliteType = eliteTemplate.EliteStyleLabel;
        }

        if (!string.IsNullOrEmpty(eliteTemplate.RewardPreviewLabel))
        {
            _eliteRewardLabel = eliteTemplate.RewardPreviewLabel;
        }

        if (eliteTemplate.RewardAmountPreviewHint > 0)
        {
            _eliteRewardAmount = eliteTemplate.RewardAmountPreviewHint;
        }

        PrototypeRpgEncounterDefinition definition = PrototypeRpgEncounterCatalog.ResolveDefinition(eliteTemplate.EncounterDefinitionId)
            ?? PrototypeRpgEncounterCatalog.BuildFallbackDefinition(
                eliteTemplate.TemplateId,
                _eliteEncounterId,
                eliteTemplate.DisplayName,
                "Elite",
                "Elite Chamber",
                eliteTemplate.EliteStyleLabel,
                _selectedRouteId == RiskyRouteId ? "Risky" : "Safe",
                _currentDungeonId == "dungeon-beta" ? "Dungeon Beta" : "Dungeon Alpha",
                eliteTemplate.RewardPreviewLabel,
                eliteTemplate.RewardPreviewLabel,
                eliteTemplate.RewardAmountPreviewHint,
                true,
                System.Array.Empty<PrototypeRpgEncounterMemberDefinition>());
        AppendEncounterDefinitionRuntime(definition, _eliteEncounterId, eliteTemplate.RoomIndex > 0 ? eliteTemplate.RoomIndex : 3, true);
    }

    private void AppendEncounterDefinitionRuntime(PrototypeRpgEncounterDefinition definition, string encounterId, int roomIndex, bool isEliteEncounter)
    {
        if (definition == null)
        {
            return;
        }

        string resolvedEncounterId = !string.IsNullOrEmpty(encounterId)
            ? encounterId
            : !string.IsNullOrEmpty(definition.EncounterId)
                ? definition.EncounterId
                : isEliteEncounter
                    ? _eliteEncounterId
                    : "encounter-room-" + roomIndex;
        if (isEliteEncounter)
        {
            _eliteEncounterId = resolvedEncounterId;
            ApplyEncounterDefinitionPresentationHints(definition);
        }

        List<string> monsterIds = new List<string>();
        if (definition.EnemyMembers != null)
        {
            for (int i = 0; i < definition.EnemyMembers.Length; i++)
            {
                PrototypeRpgEncounterMemberDefinition memberDefinition = definition.EnemyMembers[i];
                DungeonMonsterRuntimeData monster = CreateMonsterFromEncounterMemberDefinition(resolvedEncounterId, roomIndex, memberDefinition, isEliteEncounter || definition.IsEliteEncounter);
                if (monster == null)
                {
                    continue;
                }

                _activeMonsters.Add(monster);
                monsterIds.Add(monster.MonsterId);
            }
        }

        _activeEncounters.Add(CreateEncounterRuntime(definition.DefinitionId, resolvedEncounterId, roomIndex, monsterIds.ToArray(), isEliteEncounter || definition.IsEliteEncounter));
    }

    private DungeonMonsterRuntimeData CreateMonsterFromEncounterMemberDefinition(string encounterId, int roomIndex, PrototypeRpgEncounterMemberDefinition memberDefinition, bool isEliteEncounter)
    {
        if (memberDefinition == null || string.IsNullOrEmpty(memberDefinition.EnemyId))
        {
            return null;
        }

        PrototypeRpgEnemyDefinition enemyDefinition = PrototypeRpgEnemyCatalog.ResolveDefinition(memberDefinition.EnemyId);
        bool isEliteMember = isEliteEncounter || memberDefinition.IsEliteMember || (enemyDefinition != null && enemyDefinition.IsElite);
        MonsterEncounterRole encounterRole = ResolveMonsterEncounterRoleFromDefinition(enemyDefinition);
        MonsterTargetPattern targetPattern = ResolveMonsterTargetPatternFromDefinition(enemyDefinition);
        Vector2Int gridPosition = ResolveEncounterMemberGridPosition(roomIndex, memberDefinition.SlotIndex, isEliteMember);
        string displayName = enemyDefinition != null && !string.IsNullOrEmpty(enemyDefinition.DisplayName) ? enemyDefinition.DisplayName : memberDefinition.EnemyId;
        string monsterType = enemyDefinition != null && !string.IsNullOrEmpty(enemyDefinition.TypeLabel) ? enemyDefinition.TypeLabel : "Monster";
        int maxHp = enemyDefinition != null ? Mathf.Max(1, enemyDefinition.MaxHp) : 1;
        int attackPower = enemyDefinition != null ? Mathf.Max(1, enemyDefinition.AttackPower) : 1;
        int rewardAmount = enemyDefinition != null && enemyDefinition.RewardAmountHint > 0 ? enemyDefinition.RewardAmountHint : 1;
        int specialAttack = enemyDefinition != null ? enemyDefinition.SpecialPowerHint : 0;
        string specialActionName = enemyDefinition != null ? enemyDefinition.SpecialActionLabel : string.Empty;
        return CreateMonster(memberDefinition.EnemyId, encounterId, roomIndex, displayName, monsterType, maxHp, attackPower, gridPosition, rewardAmount, targetPattern, encounterRole, isEliteMember, specialAttack, specialActionName);
    }

    private static MonsterEncounterRole ResolveMonsterEncounterRoleFromDefinition(PrototypeRpgEnemyDefinition definition)
    {
        string roleTag = definition != null ? definition.RoleTag : string.Empty;
        if (!string.IsNullOrEmpty(roleTag))
        {
            string normalizedRoleTag = roleTag.Trim().ToLowerInvariant();
            if (normalizedRoleTag.Contains("skirmisher"))
            {
                return MonsterEncounterRole.Skirmisher;
            }

            if (normalizedRoleTag.Contains("striker"))
            {
                return MonsterEncounterRole.Striker;
            }
        }

        return MonsterEncounterRole.Bulwark;
    }

    private static MonsterTargetPattern ResolveMonsterTargetPatternFromDefinition(PrototypeRpgEnemyDefinition definition)
    {
        string targetPolicyKey = definition != null && definition.DefaultIntentDefinition != null
            ? definition.DefaultIntentDefinition.TargetPolicyKey
            : string.Empty;
        if (string.IsNullOrEmpty(targetPolicyKey))
        {
            return MonsterTargetPattern.FirstLiving;
        }

        string normalizedTargetPolicyKey = targetPolicyKey.Trim().ToLowerInvariant();
        if (normalizedTargetPolicyKey.Contains("lowest"))
        {
            return MonsterTargetPattern.LowestHpLiving;
        }

        if (normalizedTargetPolicyKey.Contains("random"))
        {
            return MonsterTargetPattern.RandomLiving;
        }

        return MonsterTargetPattern.FirstLiving;
    }

    private static Vector2Int ResolveEncounterMemberGridPosition(int roomIndex, int slotIndex, bool isEliteMember)
    {
        if (isEliteMember || roomIndex >= 3)
        {
            return EliteEncounterMarkerPosition;
        }

        if (roomIndex <= 1)
        {
            return slotIndex <= 0 ? new Vector2Int(4, 5) : new Vector2Int(5, 4);
        }

        return slotIndex <= 0 ? new Vector2Int(12, 5) : new Vector2Int(13, 4);
    }

    private static string ResolveMonsterRoleTag(MonsterEncounterRole encounterRole, bool isElite)
    {
        if (isElite)
        {
            return encounterRole == MonsterEncounterRole.Striker ? "elite_striker" : "elite_bulwark";
        }

        switch (encounterRole)
        {
            case MonsterEncounterRole.Skirmisher:
                return "skirmisher";
            case MonsterEncounterRole.Striker:
                return "striker";
            default:
                return "bulwark";
        }
    }

    private static string GetFallbackMonsterRoleLabel(MonsterEncounterRole encounterRole, bool isElite)
    {
        if (isElite)
        {
            return "Elite";
        }

        switch (encounterRole)
        {
            case MonsterEncounterRole.Skirmisher:
                return "Skirmisher";
            case MonsterEncounterRole.Striker:
                return "Striker";
            default:
                return "Bulwark";
        }
    }

    private static string ResolveDefaultEnemyIntentKey(MonsterTargetPattern targetPattern)
    {
        switch (targetPattern)
        {
            case MonsterTargetPattern.LowestHpLiving:
                return "intent_finish_weakest";
            case MonsterTargetPattern.RandomLiving:
                return "intent_unstable_focus";
            default:
                return "intent_frontline_pressure";
        }
    }

    private static string ResolveSpecialEnemyIntentKey(bool isElite, MonsterEncounterRole encounterRole, string specialActionName)
    {
        if (string.IsNullOrEmpty(specialActionName))
        {
            return isElite || encounterRole == MonsterEncounterRole.Striker ? "intent_heavy_strike" : string.Empty;
        }

        string normalized = specialActionName.Trim().ToLowerInvariant();
        if (normalized.Contains("royal"))
        {
            return "intent_royal_wave";
        }

        if (normalized.Contains("command"))
        {
            return "intent_command_strike";
        }

        if (normalized.Contains("core"))
        {
            return "intent_core_rupture";
        }

        if (normalized.Contains("execution"))
        {
            return "intent_execution_strike";
        }

        if (normalized.Contains("rending"))
        {
            return "intent_rending_blow";
        }

        if (normalized.Contains("crushing"))
        {
            return "intent_crushing_blow";
        }

        if (normalized.Contains("heavy"))
        {
            return "intent_heavy_strike";
        }

        return isElite || encounterRole == MonsterEncounterRole.Striker ? "intent_heavy_strike" : string.Empty;
    }

    private static string ResolveEnemyBehaviorHintKey(MonsterEncounterRole encounterRole, bool isElite, string specialActionName)
    {
        string normalized = string.IsNullOrEmpty(specialActionName) ? string.Empty : specialActionName.Trim().ToLowerInvariant();
        if (isElite)
        {
            if (normalized.Contains("royal"))
            {
                return "behavior_royal_elite";
            }

            if (normalized.Contains("command"))
            {
                return "behavior_command_elite";
            }

            if (normalized.Contains("core"))
            {
                return "behavior_volatility_elite";
            }

            return "behavior_execution_elite";
        }

        switch (encounterRole)
        {
            case MonsterEncounterRole.Skirmisher:
                return "behavior_flexible_flank";
            case MonsterEncounterRole.Striker:
                return "behavior_focused_execution";
            default:
                return "behavior_front_guard";
        }
    }

    private static string NormalizeRpgProgressStateKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }

    private string BuildRpgPartyProgressSessionKey(string cityId, string partyId)
    {
        string normalizedCityId = NormalizeRpgProgressStateKey(cityId);
        string normalizedPartyId = NormalizeRpgProgressStateKey(partyId);
        if (string.IsNullOrEmpty(normalizedCityId) || string.IsNullOrEmpty(normalizedPartyId))
        {
            return string.Empty;
        }

        return normalizedCityId + "::" + normalizedPartyId;
    }

    private PrototypeRpgAppliedPartyProgressState GetLatestAppliedPartyProgressStateInternal()
    {
        if (_activeDungeonParty != null && !string.IsNullOrEmpty(_currentHomeCityId))
        {
            string sessionKey = BuildRpgPartyProgressSessionKey(_currentHomeCityId, _activeDungeonParty.PartyId);
            if (!string.IsNullOrEmpty(sessionKey) && _appliedPartyProgressBySessionKey.TryGetValue(sessionKey, out PrototypeRpgAppliedPartyProgressState activeState) && activeState != null)
            {
                return activeState;
            }
        }

        if (!string.IsNullOrEmpty(_latestAppliedPartyProgressSessionKey) && _appliedPartyProgressBySessionKey.TryGetValue(_latestAppliedPartyProgressSessionKey, out PrototypeRpgAppliedPartyProgressState latestState) && latestState != null)
        {
            return latestState;
        }

        return null;
    }

    private PrototypeRpgAppliedPartyProgressState CopyRpgAppliedPartyProgressState(PrototypeRpgAppliedPartyProgressState source)
    {
        PrototypeRpgAppliedPartyProgressState copy = new PrototypeRpgAppliedPartyProgressState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = string.IsNullOrEmpty(source.SessionKey) ? string.Empty : source.SessionKey;
        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.HomeCityId = string.IsNullOrEmpty(source.HomeCityId) ? string.Empty : source.HomeCityId;
        copy.HasAppliedProgress = source.HasAppliedProgress;
        copy.LastAppliedRunIdentity = string.IsNullOrEmpty(source.LastAppliedRunIdentity) ? string.Empty : source.LastAppliedRunIdentity;
        copy.LastResultStateKey = string.IsNullOrEmpty(source.LastResultStateKey) ? string.Empty : source.LastResultStateKey;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        PrototypeRpgAppliedPartyMemberProgressState[] members = source.Members ?? System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        if (members.Length <= 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
            return copy;
        }

        PrototypeRpgAppliedPartyMemberProgressState[] memberCopies = new PrototypeRpgAppliedPartyMemberProgressState[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            memberCopies[i] = CopyRpgAppliedPartyMemberProgressState(members[i]);
        }

        copy.Members = memberCopies;
        return copy;
    }

    private PrototypeRpgAppliedPartyMemberProgressState CopyRpgAppliedPartyMemberProgressState(PrototypeRpgAppliedPartyMemberProgressState source)
    {
        PrototypeRpgAppliedPartyMemberProgressState copy = new PrototypeRpgAppliedPartyMemberProgressState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.AppliedGrowthTrackId = string.IsNullOrEmpty(source.AppliedGrowthTrackId) ? string.Empty : source.AppliedGrowthTrackId;
        copy.AppliedJobId = string.IsNullOrEmpty(source.AppliedJobId) ? string.Empty : source.AppliedJobId;
        copy.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(source.AppliedEquipmentLoadoutId) ? string.Empty : source.AppliedEquipmentLoadoutId;
        copy.AppliedSkillLoadoutId = string.IsNullOrEmpty(source.AppliedSkillLoadoutId) ? string.Empty : source.AppliedSkillLoadoutId;
        copy.AppliedRoleLabel = string.IsNullOrEmpty(source.AppliedRoleLabel) ? string.Empty : source.AppliedRoleLabel;
        copy.AppliedDefaultSkillId = string.IsNullOrEmpty(source.AppliedDefaultSkillId) ? string.Empty : source.AppliedDefaultSkillId;
        copy.AppliedSkillName = string.IsNullOrEmpty(source.AppliedSkillName) ? string.Empty : source.AppliedSkillName;
        copy.AppliedSkillShortText = string.IsNullOrEmpty(source.AppliedSkillShortText) ? string.Empty : source.AppliedSkillShortText;
        copy.MaxHpModifier = source.MaxHpModifier;
        copy.AttackModifier = source.AttackModifier;
        copy.DefenseModifier = source.DefenseModifier;
        copy.SpeedModifier = source.SpeedModifier;
        copy.RecentAppliedOfferId = string.IsNullOrEmpty(source.RecentAppliedOfferId) ? string.Empty : source.RecentAppliedOfferId;
        copy.RecentAppliedOfferType = string.IsNullOrEmpty(source.RecentAppliedOfferType) ? string.Empty : source.RecentAppliedOfferType;
        copy.PendingApplyKey = string.IsNullOrEmpty(source.PendingApplyKey) ? string.Empty : source.PendingApplyKey;
        copy.RecentAppliedSummaryText = string.IsNullOrEmpty(source.RecentAppliedSummaryText) ? string.Empty : source.RecentAppliedSummaryText;
        copy.LastAppliedRunIdentity = string.IsNullOrEmpty(source.LastAppliedRunIdentity) ? string.Empty : source.LastAppliedRunIdentity;
        copy.HasAnyOverride = source.HasAnyOverride;
        return copy;
    }
    private PrototypeRpgAppliedPartyProgressState GetOrCreateRpgAppliedPartyProgressState(string cityId, string partyId, PrototypeRpgPartyDefinition partyDefinition)
    {
        string sessionKey = BuildRpgPartyProgressSessionKey(cityId, partyId);
        if (string.IsNullOrEmpty(sessionKey))
        {
            return null;
        }

        if (!_appliedPartyProgressBySessionKey.TryGetValue(sessionKey, out PrototypeRpgAppliedPartyProgressState state) || state == null)
        {
            state = CreateRpgAppliedPartyProgressState(cityId, partyId, partyDefinition);
            _appliedPartyProgressBySessionKey[sessionKey] = state;
        }
        else
        {
            state.SessionKey = sessionKey;
            state.PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
            state.HomeCityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            SyncRpgAppliedPartyProgressState(state, partyDefinition);
        }

        if (state.HasAppliedProgress)
        {
            _latestAppliedPartyProgressSessionKey = sessionKey;
        }

        return state;
    }

    private PrototypeRpgAppliedPartyProgressState CreateRpgAppliedPartyProgressState(string cityId, string partyId, PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgAppliedPartyProgressState state = new PrototypeRpgAppliedPartyProgressState();
        state.SessionKey = BuildRpgPartyProgressSessionKey(cityId, partyId);
        state.PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
        state.HomeCityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
        state.Members = System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        SyncRpgAppliedPartyProgressState(state, partyDefinition);
        return state;
    }

    private void SyncRpgAppliedPartyProgressState(PrototypeRpgAppliedPartyProgressState state, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (state == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null && partyDefinition.Members != null
            ? partyDefinition.Members
            : System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        Dictionary<string, PrototypeRpgAppliedPartyMemberProgressState> existingById = new Dictionary<string, PrototypeRpgAppliedPartyMemberProgressState>();
        PrototypeRpgAppliedPartyMemberProgressState[] existingMembers = state.Members ?? System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        for (int i = 0; i < existingMembers.Length; i++)
        {
            PrototypeRpgAppliedPartyMemberProgressState existing = existingMembers[i];
            if (existing != null && !string.IsNullOrEmpty(existing.MemberId) && !existingById.ContainsKey(existing.MemberId))
            {
                existingById.Add(existing.MemberId, existing);
            }
        }

        PrototypeRpgAppliedPartyMemberProgressState[] members = new PrototypeRpgAppliedPartyMemberProgressState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgAppliedPartyMemberProgressState memberState = null;
            if (definition != null && !string.IsNullOrEmpty(definition.MemberId))
            {
                existingById.TryGetValue(definition.MemberId, out memberState);
            }

            if (memberState == null)
            {
                memberState = new PrototypeRpgAppliedPartyMemberProgressState();
            }

            memberState.MemberId = definition != null && !string.IsNullOrEmpty(definition.MemberId) ? definition.MemberId : string.Empty;
            memberState.DisplayName = definition != null && !string.IsNullOrEmpty(definition.DisplayName) ? definition.DisplayName : "Member";
            memberState.RoleTag = definition != null && !string.IsNullOrEmpty(definition.RoleTag) ? definition.RoleTag : string.Empty;
            RefreshRpgAppliedMemberProgressState(definition, memberState);
            members[i] = memberState;
        }

        state.Members = members;
        state.HasAppliedProgress = HasAnyRpgAppliedProgress(state);
        state.SummaryText = BuildRpgAppliedPartyProgressSummaryText(state);
    }

    private bool HasAnyRpgAppliedProgress(PrototypeRpgAppliedPartyProgressState state)
    {
        PrototypeRpgAppliedPartyMemberProgressState[] members = state != null && state.Members != null
            ? state.Members
            : System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        for (int i = 0; i < members.Length; i++)
        {
            if (HasRpgAppliedMemberOverrideCore(members[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasRpgAppliedMemberOverrideCore(PrototypeRpgAppliedPartyMemberProgressState memberState)
    {
        return memberState != null && (
            !string.IsNullOrEmpty(memberState.AppliedGrowthTrackId) ||
            !string.IsNullOrEmpty(memberState.AppliedJobId) ||
            !string.IsNullOrEmpty(memberState.AppliedEquipmentLoadoutId) ||
            !string.IsNullOrEmpty(memberState.AppliedSkillLoadoutId) ||
            !string.IsNullOrEmpty(memberState.RecentAppliedOfferId));
    }

    private PrototypeRpgAppliedPartyMemberProgressState FindRpgAppliedPartyMemberProgressState(PrototypeRpgAppliedPartyProgressState state, string memberId)
    {
        if (state == null || string.IsNullOrEmpty(memberId) || state.Members == null)
        {
            return null;
        }

        for (int i = 0; i < state.Members.Length; i++)
        {
            PrototypeRpgAppliedPartyMemberProgressState memberState = state.Members[i];
            if (memberState != null && memberState.MemberId == memberId)
            {
                return memberState;
            }
        }

        return null;
    }

    private PrototypeRpgPartyMemberDefinition FindRpgPartyMemberDefinition(PrototypeRpgPartyDefinition partyDefinition, string memberId)
    {
        if (partyDefinition == null || partyDefinition.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < partyDefinition.Members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = partyDefinition.Members[i];
            if (definition != null && definition.MemberId == memberId)
            {
                return definition;
            }
        }

        return null;
    }

    private void RefreshRpgAppliedMemberProgressState(PrototypeRpgPartyMemberDefinition baseDefinition, PrototypeRpgAppliedPartyMemberProgressState memberState)
    {
        if (memberState == null)
        {
            return;
        }

        if (baseDefinition == null || !HasRpgAppliedMemberOverrideCore(memberState))
        {
            memberState.AppliedRoleLabel = string.Empty;
            memberState.AppliedDefaultSkillId = string.Empty;
            memberState.AppliedSkillName = string.Empty;
            memberState.AppliedSkillShortText = string.Empty;
            memberState.MaxHpModifier = 0;
            memberState.AttackModifier = 0;
            memberState.DefenseModifier = 0;
            memberState.SpeedModifier = 0;
            memberState.RecentAppliedSummaryText = string.Empty;
            memberState.HasAnyOverride = false;
            return;
        }

        PrototypeRpgStatBlock baseStats = baseDefinition.BaseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        int maxHp = baseStats.MaxHp;
        int attack = baseStats.Attack;
        int defense = baseStats.Defense;
        int speed = baseStats.Speed;
        ApplyRpgAppliedStatModifiers(memberState.AppliedGrowthTrackId, memberState.AppliedJobId, memberState.AppliedEquipmentLoadoutId, memberState.AppliedSkillLoadoutId, ref maxHp, ref attack, ref defense, ref speed);
        memberState.MaxHpModifier = maxHp - baseStats.MaxHp;
        memberState.AttackModifier = attack - baseStats.Attack;
        memberState.DefenseModifier = defense - baseStats.Defense;
        memberState.SpeedModifier = speed - baseStats.Speed;
        memberState.AppliedRoleLabel = ResolveRpgAppliedRoleLabel(baseDefinition, memberState.AppliedGrowthTrackId, memberState.AppliedJobId, memberState.AppliedEquipmentLoadoutId, memberState.AppliedSkillLoadoutId);
        memberState.AppliedDefaultSkillId = ResolveRpgAppliedDefaultSkillId(baseDefinition, memberState.AppliedSkillLoadoutId);
        memberState.AppliedSkillName = ResolveRpgAppliedSkillName(baseDefinition, memberState.AppliedDefaultSkillId, memberState.AppliedSkillLoadoutId);
        memberState.AppliedSkillShortText = ResolveRpgAppliedSkillShortText(baseDefinition, memberState.AppliedDefaultSkillId, memberState.AppliedSkillLoadoutId);
        memberState.RecentAppliedSummaryText = BuildRpgAppliedMemberSummaryText(memberState);
        memberState.HasAnyOverride = true;
    }
    private void ApplyRpgAppliedStatModifiers(string growthTrackId, string jobId, string equipmentLoadoutId, string skillLoadoutId, ref int maxHp, ref int attack, ref int defense, ref int speed)
    {
        string growth = NormalizeRpgProgressStateKey(growthTrackId);
        string job = NormalizeRpgProgressStateKey(jobId);
        string equipment = NormalizeRpgProgressStateKey(equipmentLoadoutId);
        string skillLoadout = NormalizeRpgProgressStateKey(skillLoadoutId);

        if (growth.Contains("frontline"))
        {
            maxHp += 4;
            defense += 1;
        }
        else if (growth.Contains("precision"))
        {
            attack += 1;
            speed += 2;
        }
        else if (growth.Contains("arcane"))
        {
            attack += 2;
            speed += 1;
        }
        else if (growth.Contains("support"))
        {
            maxHp += 2;
            defense += 1;
        }
        else if (growth.Contains("generalist"))
        {
            maxHp += 1;
            attack += 1;
            defense += 1;
        }

        if (job.Contains("vanguard"))
        {
            maxHp += 2;
            attack += 1;
            defense += 1;
        }
        else if (job.Contains("shadow"))
        {
            attack += 1;
            speed += 1;
        }
        else if (job.Contains("spellweaver"))
        {
            attack += 1;
            speed += 1;
        }
        else if (job.Contains("sanctifier"))
        {
            maxHp += 1;
            defense += 1;
        }

        if (equipment.Contains("vanguard") || equipment.Contains("bulwark"))
        {
            maxHp += 2;
            defense += 1;
        }
        else if (equipment.Contains("execution") || equipment.Contains("precision"))
        {
            attack += 1;
            speed += 1;
        }
        else if (equipment.Contains("focus") || equipment.Contains("overchannel"))
        {
            attack += 1;
        }
        else if (equipment.Contains("sanctuary") || equipment.Contains("ward"))
        {
            maxHp += 2;
            defense += 1;
        }

        if (skillLoadout.Contains("crushing"))
        {
            attack += 1;
        }
        else if (skillLoadout.Contains("finish"))
        {
            attack += 1;
            speed += 1;
        }
        else if (skillLoadout.Contains("overchannel"))
        {
            attack += 1;
        }
        else if (skillLoadout.Contains("benediction"))
        {
            defense += 1;
        }
    }

    private string ResolveRpgAppliedGrowthTrackId(string requestedId, string roleTag)
    {
        string requested = NormalizeRpgProgressStateKey(requestedId);
        switch (requested)
        {
            case "growth_frontline": return "growth_frontline_vanguard";
            case "growth_precision": return "growth_precision_execution";
            case "growth_arcane": return "growth_arcane_overchannel";
            case "growth_support": return "growth_support_sanctuary";
            case "growth_generalist": return "growth_generalist_field";
        }

        if (!string.IsNullOrEmpty(requested))
        {
            return requested;
        }

        switch (NormalizeRpgProgressStateKey(roleTag))
        {
            case "warrior": return "growth_frontline_vanguard";
            case "rogue": return "growth_precision_execution";
            case "mage": return "growth_arcane_overchannel";
            case "cleric": return "growth_support_sanctuary";
            default: return "growth_generalist_field";
        }
    }

    private string ResolveRpgAppliedJobId(string requestedId, string roleTag)
    {
        string requested = NormalizeRpgProgressStateKey(requestedId);
        switch (requested)
        {
            case "job_warrior_novice": return "job_warrior_vanguard";
            case "job_rogue_novice": return "job_rogue_shadowblade";
            case "job_mage_novice": return "job_mage_spellweaver";
            case "job_cleric_novice": return "job_cleric_sanctifier";
        }

        if (!string.IsNullOrEmpty(requested))
        {
            return requested;
        }

        switch (NormalizeRpgProgressStateKey(roleTag))
        {
            case "warrior": return "job_warrior_vanguard";
            case "rogue": return "job_rogue_shadowblade";
            case "mage": return "job_mage_spellweaver";
            case "cleric": return "job_cleric_sanctifier";
            default: return "job_adventurer_specialist";
        }
    }

    private string ResolveRpgAppliedEquipmentLoadoutId(string requestedId, string roleTag)
    {
        string requested = NormalizeRpgProgressStateKey(requestedId);
        switch (requested)
        {
            case "equip_warrior_placeholder": return "equip_warrior_vanguard_set";
            case "equip_rogue_placeholder": return "equip_rogue_execution_set";
            case "equip_mage_placeholder": return "equip_mage_overchannel_set";
            case "equip_cleric_placeholder": return "equip_cleric_sanctuary_set";
        }

        if (!string.IsNullOrEmpty(requested))
        {
            return requested;
        }

        switch (NormalizeRpgProgressStateKey(roleTag))
        {
            case "warrior": return "equip_warrior_vanguard_set";
            case "rogue": return "equip_rogue_execution_set";
            case "mage": return "equip_mage_overchannel_set";
            case "cleric": return "equip_cleric_sanctuary_set";
            default: return "equip_adventurer_field_set";
        }
    }

    private string ResolveRpgAppliedSkillLoadoutId(string requestedId, string roleTag)
    {
        string requested = NormalizeRpgProgressStateKey(requestedId);
        switch (requested)
        {
            case "skillloadout_warrior_placeholder": return "skillloadout_warrior_crushing";
            case "skillloadout_rogue_placeholder": return "skillloadout_rogue_finish";
            case "skillloadout_mage_placeholder": return "skillloadout_mage_overchannel";
            case "skillloadout_cleric_placeholder": return "skillloadout_cleric_benediction";
        }

        if (!string.IsNullOrEmpty(requested))
        {
            return requested;
        }

        switch (NormalizeRpgProgressStateKey(roleTag))
        {
            case "warrior": return "skillloadout_warrior_crushing";
            case "rogue": return "skillloadout_rogue_finish";
            case "mage": return "skillloadout_mage_overchannel";
            case "cleric": return "skillloadout_cleric_benediction";
            default: return "skillloadout_adventurer_field";
        }
    }

    private string ResolveRpgAppliedRoleLabel(PrototypeRpgPartyMemberDefinition baseDefinition, string growthTrackId, string jobId, string equipmentLoadoutId, string skillLoadoutId)
    {
        string job = NormalizeRpgProgressStateKey(jobId);
        if (job.Contains("vanguard")) { return "Vanguard Warrior"; }
        if (job.Contains("shadow")) { return "Shadow Rogue"; }
        if (job.Contains("spellweaver")) { return "Spellweaver Mage"; }
        if (job.Contains("sanctifier")) { return "Sanctifier Cleric"; }

        string growth = NormalizeRpgProgressStateKey(growthTrackId);
        if (growth.Contains("frontline")) { return "Frontline Warrior"; }
        if (growth.Contains("precision")) { return "Precision Rogue"; }
        if (growth.Contains("arcane")) { return "Arcane Mage"; }
        if (growth.Contains("support")) { return "Support Cleric"; }

        return baseDefinition != null && !string.IsNullOrEmpty(baseDefinition.RoleLabel) ? baseDefinition.RoleLabel : "Adventurer";
    }

    private string ResolveRpgAppliedDefaultSkillId(PrototypeRpgPartyMemberDefinition baseDefinition, string skillLoadoutId)
    {
        string roleTag = baseDefinition != null ? baseDefinition.RoleTag : string.Empty;
        switch (NormalizeRpgProgressStateKey(roleTag))
        {
            case "warrior": return "skill_power_strike";
            case "rogue": return "skill_weak_point";
            case "mage": return "skill_arcane_burst";
            case "cleric": return "skill_radiant_hymn";
            default:
                return baseDefinition != null && !string.IsNullOrEmpty(baseDefinition.DefaultSkillId) ? baseDefinition.DefaultSkillId : string.Empty;
        }
    }

    private string ResolveRpgAppliedSkillName(PrototypeRpgPartyMemberDefinition baseDefinition, string defaultSkillId, string skillLoadoutId)
    {
        string skillLoadout = NormalizeRpgProgressStateKey(skillLoadoutId);
        if (skillLoadout.Contains("crushing")) { return "Crushing Strike"; }
        if (skillLoadout.Contains("finish")) { return "Execution Point"; }
        if (skillLoadout.Contains("overchannel")) { return "Overchannel Burst"; }
        if (skillLoadout.Contains("benediction")) { return "Benediction Hymn"; }

        PrototypeRpgSkillDefinition skillDefinition = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, baseDefinition != null ? baseDefinition.RoleTag : string.Empty);
        if (skillDefinition != null && !string.IsNullOrEmpty(skillDefinition.DisplayName))
        {
            return skillDefinition.DisplayName;
        }

        return baseDefinition != null && !string.IsNullOrEmpty(baseDefinition.DefaultSkillName) ? baseDefinition.DefaultSkillName : "Skill";
    }

    private string ResolveRpgAppliedSkillShortText(PrototypeRpgPartyMemberDefinition baseDefinition, string defaultSkillId, string skillLoadoutId)
    {
        string skillLoadout = NormalizeRpgProgressStateKey(skillLoadoutId);
        if (skillLoadout.Contains("crushing")) { return "Heavier single-target strike prepared for the next run."; }
        if (skillLoadout.Contains("finish")) { return "Sharper finisher package for weakened enemies."; }
        if (skillLoadout.Contains("overchannel")) { return "Arcane burst with stronger overchannel tuning."; }
        if (skillLoadout.Contains("benediction")) { return "Broader restorative hymn for the full party."; }

        PrototypeRpgSkillDefinition skillDefinition = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, baseDefinition != null ? baseDefinition.RoleTag : string.Empty);
        if (skillDefinition != null && !string.IsNullOrEmpty(skillDefinition.ShortText))
        {
            return skillDefinition.ShortText;
        }

        return baseDefinition != null && !string.IsNullOrEmpty(baseDefinition.DefaultSkillShortText) ? baseDefinition.DefaultSkillShortText : string.Empty;
    }

    private int ResolveRpgAppliedSkillPowerBonus(PrototypeRpgPartyMemberDefinition memberDefinition)
    {
        if (memberDefinition == null)
        {
            return 0;
        }

        string skillLoadout = NormalizeRpgProgressStateKey(memberDefinition.SkillLoadoutId);
        string equipment = NormalizeRpgProgressStateKey(memberDefinition.EquipmentLoadoutId);
        int bonus = 0;
        if (skillLoadout.Contains("crushing") || skillLoadout.Contains("finish") || skillLoadout.Contains("overchannel"))
        {
            bonus += 2;
        }
        else if (skillLoadout.Contains("benediction"))
        {
            bonus += 1;
        }

        if (equipment.Contains("execution") || equipment.Contains("focus") || equipment.Contains("sanctuary") || equipment.Contains("vanguard"))
        {
            bonus += 1;
        }

        return bonus;
    }

    private PrototypeRpgPartyMemberDefinition ResolveAppliedMemberDefinition(PrototypeRpgPartyMemberDefinition baseDefinition, PrototypeRpgAppliedPartyMemberProgressState appliedState)
    {
        if (baseDefinition == null)
        {
            return null;
        }

        if (appliedState == null || !HasRpgAppliedMemberOverrideCore(appliedState))
        {
            return baseDefinition;
        }

        RefreshRpgAppliedMemberProgressState(baseDefinition, appliedState);
        PrototypeRpgStatBlock baseStats = baseDefinition.BaseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        PrototypeRpgStatBlock resolvedStats = new PrototypeRpgStatBlock(
            Mathf.Max(1, baseStats.MaxHp + appliedState.MaxHpModifier),
            Mathf.Max(1, baseStats.Attack + appliedState.AttackModifier),
            Mathf.Max(0, baseStats.Defense + appliedState.DefenseModifier),
            Mathf.Max(0, baseStats.Speed + appliedState.SpeedModifier));
        return new PrototypeRpgPartyMemberDefinition(
            baseDefinition.MemberId,
            baseDefinition.DisplayName,
            baseDefinition.RoleTag,
            !string.IsNullOrEmpty(appliedState.AppliedRoleLabel) ? appliedState.AppliedRoleLabel : baseDefinition.RoleLabel,
            baseDefinition.PartySlotIndex,
            resolvedStats,
            !string.IsNullOrEmpty(appliedState.AppliedDefaultSkillId) ? appliedState.AppliedDefaultSkillId : baseDefinition.DefaultSkillId,
            !string.IsNullOrEmpty(appliedState.AppliedSkillName) ? appliedState.AppliedSkillName : baseDefinition.DefaultSkillName,
            !string.IsNullOrEmpty(appliedState.AppliedSkillShortText) ? appliedState.AppliedSkillShortText : baseDefinition.DefaultSkillShortText,
            !string.IsNullOrEmpty(appliedState.AppliedGrowthTrackId) ? appliedState.AppliedGrowthTrackId : baseDefinition.GrowthTrackId,
            !string.IsNullOrEmpty(appliedState.AppliedJobId) ? appliedState.AppliedJobId : baseDefinition.JobId,
            !string.IsNullOrEmpty(appliedState.AppliedEquipmentLoadoutId) ? appliedState.AppliedEquipmentLoadoutId : baseDefinition.EquipmentLoadoutId,
            !string.IsNullOrEmpty(appliedState.AppliedSkillLoadoutId) ? appliedState.AppliedSkillLoadoutId : baseDefinition.SkillLoadoutId);
    }
    private bool CommitRpgPostRunUpgradeChoice(PrototypeRpgAppliedPartyProgressState appliedState, PrototypeRpgApplyReadyUpgradeChoice choice, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (appliedState == null || choice == null || !choice.IsReady)
        {
            return false;
        }

        PrototypeRpgUpgradeSelectionRequest request = choice.Request ?? new PrototypeRpgUpgradeSelectionRequest();
        string memberId = !string.IsNullOrEmpty(request.MemberId) ? request.MemberId : choice.MemberId;
        PrototypeRpgAppliedPartyMemberProgressState memberState = FindRpgAppliedPartyMemberProgressState(appliedState, memberId);
        PrototypeRpgPartyMemberDefinition baseDefinition = FindRpgPartyMemberDefinition(partyDefinition, memberId);
        if (memberState == null || baseDefinition == null)
        {
            return false;
        }

        string selectedOfferType = !string.IsNullOrEmpty(choice.SelectedOfferType) ? choice.SelectedOfferType : request.SelectedOfferType;
        bool changed = false;
        switch (selectedOfferType)
        {
            case PrototypeRpgUpgradeOfferTypeKeys.GrowthTrack:
                string nextGrowthTrackId = ResolveRpgAppliedGrowthTrackId(!string.IsNullOrEmpty(request.WouldAffectGrowthTrackId) ? request.WouldAffectGrowthTrackId : baseDefinition.GrowthTrackId, memberState.RoleTag);
                if (memberState.AppliedGrowthTrackId != nextGrowthTrackId)
                {
                    memberState.AppliedGrowthTrackId = nextGrowthTrackId;
                    changed = true;
                }
                break;
            case PrototypeRpgUpgradeOfferTypeKeys.Job:
                string nextJobId = ResolveRpgAppliedJobId(!string.IsNullOrEmpty(request.WouldAffectJobId) ? request.WouldAffectJobId : baseDefinition.JobId, memberState.RoleTag);
                if (memberState.AppliedJobId != nextJobId)
                {
                    memberState.AppliedJobId = nextJobId;
                    changed = true;
                }
                break;
            case PrototypeRpgUpgradeOfferTypeKeys.EquipmentLoadout:
                string nextEquipmentId = ResolveRpgAppliedEquipmentLoadoutId(!string.IsNullOrEmpty(request.WouldAffectEquipmentLoadoutId) ? request.WouldAffectEquipmentLoadoutId : baseDefinition.EquipmentLoadoutId, memberState.RoleTag);
                if (memberState.AppliedEquipmentLoadoutId != nextEquipmentId)
                {
                    memberState.AppliedEquipmentLoadoutId = nextEquipmentId;
                    changed = true;
                }
                break;
            case PrototypeRpgUpgradeOfferTypeKeys.SkillLoadout:
                string nextSkillLoadoutId = ResolveRpgAppliedSkillLoadoutId(!string.IsNullOrEmpty(request.WouldAffectSkillLoadoutId) ? request.WouldAffectSkillLoadoutId : baseDefinition.SkillLoadoutId, memberState.RoleTag);
                if (memberState.AppliedSkillLoadoutId != nextSkillLoadoutId)
                {
                    memberState.AppliedSkillLoadoutId = nextSkillLoadoutId;
                    changed = true;
                }
                break;
        }

        memberState.RecentAppliedOfferId = !string.IsNullOrEmpty(choice.SelectedOfferId) ? choice.SelectedOfferId : request.SelectedOfferId;
        memberState.RecentAppliedOfferType = string.IsNullOrEmpty(selectedOfferType) ? string.Empty : selectedOfferType;
        memberState.PendingApplyKey = !string.IsNullOrEmpty(choice.PendingApplyKey) ? choice.PendingApplyKey : request.PendingApplyKey;
        memberState.LastAppliedRunIdentity = !string.IsNullOrEmpty(request.RunIdentity) ? request.RunIdentity : appliedState.LastAppliedRunIdentity;
        RefreshRpgAppliedMemberProgressState(baseDefinition, memberState);
        return changed || memberState.HasAnyOverride;
    }

    private void CommitRpgPostRunUpgradeChoices()
    {
        PrototypeRpgRunResultSnapshot runResultSnapshot = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
        string partyId = GetActivePostRunPartyId();
        if (!HasRpgRunResultSnapshotData(runResultSnapshot) || string.IsNullOrEmpty(partyId) || string.IsNullOrEmpty(_currentHomeCityId))
        {
            return;
        }

        PrototypeRpgPartyDefinition partyDefinition = _activeDungeonParty != null && _activeDungeonParty.PartyDefinition != null
            ? _activeDungeonParty.PartyDefinition
            : PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
        PrototypeRpgAppliedPartyProgressState appliedState = GetOrCreateRpgAppliedPartyProgressState(_currentHomeCityId, partyId, partyDefinition);
        if (appliedState == null)
        {
            return;
        }

        PrototypeRpgPostRunUpgradeOfferSurface offerSurface = BuildRpgPostRunUpgradeOfferSurface();
        PrototypeRpgApplyReadyUpgradeChoice[] choices = offerSurface != null && offerSurface.ApplyReadyChoices != null
            ? offerSurface.ApplyReadyChoices
            : System.Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
        bool anyChanged = false;
        for (int i = 0; i < choices.Length; i++)
        {
            anyChanged |= CommitRpgPostRunUpgradeChoice(appliedState, choices[i], partyDefinition);
        }

        SyncRpgAppliedPartyProgressState(appliedState, partyDefinition);
        appliedState.LastAppliedRunIdentity = string.IsNullOrEmpty(runResultSnapshot.RunIdentity) ? appliedState.LastAppliedRunIdentity : runResultSnapshot.RunIdentity;
        appliedState.LastResultStateKey = string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? appliedState.LastResultStateKey : runResultSnapshot.ResultStateKey;
        appliedState.HasAppliedProgress = HasAnyRpgAppliedProgress(appliedState);
        appliedState.SummaryText = BuildRpgAppliedPartyProgressSummaryText(appliedState);
        RefreshRpgAppliedProgressReadbackConsistency();
        if (appliedState.HasAppliedProgress && !string.IsNullOrEmpty(appliedState.SessionKey))
        {
            _latestAppliedPartyProgressSessionKey = appliedState.SessionKey;
        }

        if (anyChanged && !string.IsNullOrEmpty(appliedState.SummaryText) && appliedState.SummaryText != "None")
        {
            AppendBattleLog("Applied next-run party progress: " + appliedState.SummaryText + ".");
        }
    }

    private string BuildRpgAppliedMemberSummaryText(PrototypeRpgAppliedPartyMemberProgressState memberState)
    {
        if (memberState == null || !HasRpgAppliedMemberOverrideCore(memberState))
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(memberState.AppliedRoleLabel))
        {
            parts.Add(memberState.AppliedRoleLabel);
        }
        if (!string.IsNullOrEmpty(memberState.AppliedSkillName))
        {
            parts.Add(memberState.AppliedSkillName);
        }

        List<string> stats = new List<string>();
        if (memberState.MaxHpModifier != 0) { stats.Add("HP " + (memberState.MaxHpModifier > 0 ? "+" : string.Empty) + memberState.MaxHpModifier); }
        if (memberState.AttackModifier != 0) { stats.Add("ATK " + (memberState.AttackModifier > 0 ? "+" : string.Empty) + memberState.AttackModifier); }
        if (memberState.DefenseModifier != 0) { stats.Add("DEF " + (memberState.DefenseModifier > 0 ? "+" : string.Empty) + memberState.DefenseModifier); }
        if (memberState.SpeedModifier != 0) { stats.Add("SPD " + (memberState.SpeedModifier > 0 ? "+" : string.Empty) + memberState.SpeedModifier); }
        if (stats.Count > 0)
        {
            parts.Add(string.Join(" ", stats.ToArray()));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : BuildRpgNormalizedHookLabel(memberState.RecentAppliedOfferType, "Upgrade");
    }

    private string BuildRpgAppliedPartyProgressSummaryText(PrototypeRpgAppliedPartyProgressState appliedState)
    {
        PrototypeRpgAppliedPartyMemberProgressState[] members = appliedState != null && appliedState.Members != null
            ? appliedState.Members
            : System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        List<string> parts = new List<string>();
        int hiddenCount = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgAppliedPartyMemberProgressState memberState = members[i];
            if (memberState == null || !memberState.HasAnyOverride || string.IsNullOrEmpty(memberState.RecentAppliedSummaryText))
            {
                continue;
            }

            if (parts.Count < 3)
            {
                string displayName = string.IsNullOrEmpty(memberState.DisplayName) ? "Member" : memberState.DisplayName;
                parts.Add(displayName + " -> " + memberState.RecentAppliedSummaryText);
            }
            else
            {
                hiddenCount++;
            }
        }

        if (hiddenCount > 0)
        {
            parts.Add("+" + hiddenCount + " more");
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string AppendRpgSummaryText(string existingText, string appendedText)
    {
        if (string.IsNullOrEmpty(appendedText) || appendedText == "None")
        {
            return string.IsNullOrEmpty(existingText) ? "None" : existingText;
        }

        if (string.IsNullOrEmpty(existingText) || existingText == "None")
        {
            return appendedText;
        }

        return existingText.Contains(appendedText, System.StringComparison.Ordinal) ? existingText : existingText + " | " + appendedText;
    }

    private void ApplyRpgAppliedProgressToRunResultSnapshot(PrototypeRpgRunResultSnapshot snapshot, PrototypeRpgAppliedPartyProgressState appliedState)
    {
        if (snapshot == null || appliedState == null || !appliedState.HasAppliedProgress)
        {
            return;
        }

        string appliedSummary = BuildRpgAppliedPartyProgressSummaryText(appliedState);
        if (!string.IsNullOrEmpty(appliedSummary) && appliedSummary != "None")
        {
            snapshot.AppliedProgressSummaryText = appliedSummary;
            if (snapshot.PartyOutcome != null)
            {
                snapshot.PartyOutcome.AppliedPartySummaryText = appliedSummary;
            }
        }

        PrototypeRpgPartyMemberOutcomeSnapshot[] members = snapshot.PartyOutcome != null && snapshot.PartyOutcome.Members != null
            ? snapshot.PartyOutcome.Members
            : System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot member = members[i];
            if (member == null)
            {
                continue;
            }

            PrototypeRpgAppliedPartyMemberProgressState memberState = FindRpgAppliedPartyMemberProgressState(appliedState, member.MemberId);
            if (memberState == null || !memberState.HasAnyOverride)
            {
                continue;
            }

            member.AppliedRoleLabel = string.IsNullOrEmpty(memberState.AppliedRoleLabel) ? member.AppliedRoleLabel : memberState.AppliedRoleLabel;
            member.AppliedDefaultSkillId = string.IsNullOrEmpty(memberState.AppliedDefaultSkillId) ? member.AppliedDefaultSkillId : memberState.AppliedDefaultSkillId;
            member.AppliedGrowthTrackId = string.IsNullOrEmpty(memberState.AppliedGrowthTrackId) ? member.AppliedGrowthTrackId : memberState.AppliedGrowthTrackId;
            member.AppliedJobId = string.IsNullOrEmpty(memberState.AppliedJobId) ? member.AppliedJobId : memberState.AppliedJobId;
            member.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(memberState.AppliedEquipmentLoadoutId) ? member.AppliedEquipmentLoadoutId : memberState.AppliedEquipmentLoadoutId;
            member.AppliedSkillLoadoutId = string.IsNullOrEmpty(memberState.AppliedSkillLoadoutId) ? member.AppliedSkillLoadoutId : memberState.AppliedSkillLoadoutId;
            member.AppliedProgressSummaryText = string.IsNullOrEmpty(memberState.RecentAppliedSummaryText) ? member.AppliedProgressSummaryText : memberState.RecentAppliedSummaryText;
        }
    }

    private void ApplyRpgAppliedProgressToProgressionPreviewSnapshot(PrototypeRpgProgressionPreviewSnapshot snapshot, PrototypeRpgAppliedPartyProgressState appliedState)
    {
        if (snapshot == null || appliedState == null || !appliedState.HasAppliedProgress)
        {
            return;
        }

        string appliedSummary = BuildRpgAppliedPartyProgressSummaryText(appliedState);
        snapshot.HasAppliedProgress = true;
        snapshot.AppliedLastRunIdentity = !string.IsNullOrEmpty(appliedState.LastAppliedRunIdentity) ? appliedState.LastAppliedRunIdentity : snapshot.AppliedLastRunIdentity;
        if (!string.IsNullOrEmpty(appliedSummary) && appliedSummary != "None")
        {
            snapshot.AppliedProgressSummaryText = appliedSummary;
            snapshot.ProgressionPreviewText = AppendRpgSummaryText(snapshot.ProgressionPreviewText, "Applied next run: " + appliedSummary);
            snapshot.ApplyReadySummaryText = AppendRpgSummaryText(snapshot.ApplyReadySummaryText, "Committed for next run");
        }

        PrototypeRpgMemberProgressPreview[] members = snapshot.Members ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberProgressPreview preview = members[i];
            if (preview == null)
            {
                continue;
            }

            PrototypeRpgAppliedPartyMemberProgressState memberState = FindRpgAppliedPartyMemberProgressState(appliedState, preview.MemberId);
            if (memberState == null || !memberState.HasAnyOverride)
            {
                continue;
            }

            preview.HasAppliedProgress = true;
            preview.AppliedProgressSummaryText = string.IsNullOrEmpty(memberState.RecentAppliedSummaryText) ? preview.AppliedProgressSummaryText : memberState.RecentAppliedSummaryText;
            preview.AppliedRoleLabel = string.IsNullOrEmpty(memberState.AppliedRoleLabel) ? preview.AppliedRoleLabel : memberState.AppliedRoleLabel;
            preview.AppliedDefaultSkillId = string.IsNullOrEmpty(memberState.AppliedDefaultSkillId) ? preview.AppliedDefaultSkillId : memberState.AppliedDefaultSkillId;
            if (!string.IsNullOrEmpty(memberState.AppliedRoleLabel)) { preview.RoleLabel = memberState.AppliedRoleLabel; }
            if (!string.IsNullOrEmpty(memberState.AppliedGrowthTrackId)) { preview.GrowthTrackId = memberState.AppliedGrowthTrackId; preview.NextGrowthTrackHint = "Applied track: " + BuildRpgNormalizedHookLabel(memberState.AppliedGrowthTrackId, "Growth Track"); }
            if (!string.IsNullOrEmpty(memberState.AppliedJobId)) { preview.JobId = memberState.AppliedJobId; preview.NextJobHint = "Applied job: " + BuildRpgNormalizedHookLabel(memberState.AppliedJobId, "Job Path"); }
            if (!string.IsNullOrEmpty(memberState.AppliedSkillLoadoutId)) { preview.SkillLoadoutId = memberState.AppliedSkillLoadoutId; preview.NextSkillLoadoutHint = "Applied skill loadout: " + BuildRpgNormalizedHookLabel(memberState.AppliedSkillLoadoutId, "Skill Loadout"); }
            if (!string.IsNullOrEmpty(memberState.AppliedEquipmentLoadoutId)) { preview.EquipmentLoadoutId = memberState.AppliedEquipmentLoadoutId; preview.NextEquipmentLoadoutHint = "Applied gear: " + BuildRpgNormalizedHookLabel(memberState.AppliedEquipmentLoadoutId, "Equipment Loadout"); }
            if (!string.IsNullOrEmpty(memberState.RecentAppliedSummaryText))
            {
                preview.PreviewSummaryText = AppendRpgSummaryText(preview.PreviewSummaryText, "Applied next run: " + memberState.RecentAppliedSummaryText);
                preview.ApplyReadySummaryText = AppendRpgSummaryText(preview.ApplyReadySummaryText, "Committed for next run");
            }
        }
    }

    private void RefreshRpgAppliedProgressReadbackConsistency()
    {
        PrototypeRpgAppliedPartyProgressState appliedState = GetLatestAppliedPartyProgressStateInternal();
        if (appliedState == null || !appliedState.HasAppliedProgress)
        {
            return;
        }

        if (HasRpgRunResultSnapshotData(_latestRpgRunResultSnapshot))
        {
            _latestRpgRunResultSnapshot = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
            ApplyRpgAppliedProgressToRunResultSnapshot(_latestRpgRunResultSnapshot, appliedState);
        }

        if (HasRpgProgressionPreviewSnapshotData(_latestRpgProgressionPreviewSnapshot))
        {
            _latestRpgProgressionPreviewSnapshot = CopyRpgProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot);
            ApplyRpgAppliedProgressToProgressionPreviewSnapshot(_latestRpgProgressionPreviewSnapshot, appliedState);
        }
    }
    private void ApplyRpgAppliedProgressToPostRunMemberCard(PrototypeRpgPostRunMemberCardData card, PrototypeRpgAppliedPartyProgressState appliedState)
    {
        if (card == null)
        {
            return;
        }

        PrototypeRpgAppliedPartyMemberProgressState memberState = FindRpgAppliedPartyMemberProgressState(appliedState, card.MemberId);
        if (memberState == null || !memberState.HasAnyOverride || string.IsNullOrEmpty(memberState.RecentAppliedSummaryText))
        {
            return;
        }

        card.ProgressionSummaryText = AppendRpgSummaryText(card.ProgressionSummaryText, "Applied next run: " + memberState.RecentAppliedSummaryText);
        if (!string.IsNullOrEmpty(memberState.AppliedRoleLabel)) { card.RoleLabel = memberState.AppliedRoleLabel; }
        if (!string.IsNullOrEmpty(memberState.AppliedGrowthTrackId)) { card.NextGrowthTrackHint = "Applied track: " + BuildRpgNormalizedHookLabel(memberState.AppliedGrowthTrackId, "Growth Track"); }
        if (!string.IsNullOrEmpty(memberState.AppliedJobId)) { card.NextJobHint = "Applied job: " + BuildRpgNormalizedHookLabel(memberState.AppliedJobId, "Job Path"); }
        if (!string.IsNullOrEmpty(memberState.AppliedSkillLoadoutId)) { card.NextSkillLoadoutHint = "Applied skill loadout: " + BuildRpgNormalizedHookLabel(memberState.AppliedSkillLoadoutId, "Skill Loadout"); }
        if (!string.IsNullOrEmpty(memberState.AppliedEquipmentLoadoutId)) { card.NextEquipmentLoadoutHint = "Applied gear: " + BuildRpgNormalizedHookLabel(memberState.AppliedEquipmentLoadoutId, "Equipment Loadout"); }
    }

    private void ApplyRpgAppliedProgressToMemberPendingDelta(PrototypeRpgMemberPendingDeltaData delta, PrototypeRpgAppliedPartyProgressState appliedState)
    {
        if (delta == null)
        {
            return;
        }

        PrototypeRpgAppliedPartyMemberProgressState memberState = FindRpgAppliedPartyMemberProgressState(appliedState, delta.MemberId);
        if (memberState == null || !memberState.HasAnyOverride || string.IsNullOrEmpty(memberState.RecentAppliedSummaryText))
        {
            return;
        }

        delta.PendingDeltaSummaryText = AppendRpgSummaryText(delta.PendingDeltaSummaryText, "Applied next run: " + memberState.RecentAppliedSummaryText);
        if (!string.IsNullOrEmpty(memberState.AppliedRoleLabel)) { delta.RoleLabel = memberState.AppliedRoleLabel; }
        if (!string.IsNullOrEmpty(memberState.AppliedGrowthTrackId)) { delta.GrowthTrackId = memberState.AppliedGrowthTrackId; }
        if (!string.IsNullOrEmpty(memberState.AppliedJobId)) { delta.JobId = memberState.AppliedJobId; }
        if (!string.IsNullOrEmpty(memberState.AppliedEquipmentLoadoutId)) { delta.EquipmentLoadoutId = memberState.AppliedEquipmentLoadoutId; }
        if (!string.IsNullOrEmpty(memberState.AppliedSkillLoadoutId)) { delta.SkillLoadoutId = memberState.AppliedSkillLoadoutId; }
    }

    private void ApplyRpgAppliedProgressToPostRunSummarySurface(PrototypeRpgPostRunSummarySurfaceData surface, PrototypeRpgAppliedPartyProgressState appliedState)
    {
        if (surface == null)
        {
            return;
        }

        string appliedSummary = BuildRpgAppliedPartyProgressSummaryText(appliedState);
        if (string.IsNullOrEmpty(appliedSummary) || appliedSummary == "None")
        {
            return;
        }

        surface.ProgressionPreviewSummaryText = AppendRpgSummaryText(surface.ProgressionPreviewSummaryText, "Applied next run: " + appliedSummary);
        surface.ApplyReadySummaryText = AppendRpgSummaryText(surface.ApplyReadySummaryText, "Committed for next run");
    }
    private TestDungeonPartyData CreatePlaceholderDungeonParty(string cityId, string partyId)
    {
        PrototypeRpgPartyDefinition partyDefinition = PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
        return new TestDungeonPartyData(partyId, cityId, partyDefinition, CreateDungeonRuntimeMembers(partyDefinition));
    }

    private DungeonPartyMemberRuntimeData[] CreateDungeonRuntimeMembers(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgAppliedPartyProgressState appliedProgressState = null)
    {
        if (partyDefinition == null || partyDefinition.Members == null || partyDefinition.Members.Length == 0)
        {
            return System.Array.Empty<DungeonPartyMemberRuntimeData>();
        }

        DungeonPartyMemberRuntimeData[] members = new DungeonPartyMemberRuntimeData[partyDefinition.Members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition baseDefinition = partyDefinition.Members[i];
            PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = FindRpgAppliedPartyMemberProgressState(appliedProgressState, baseDefinition != null ? baseDefinition.MemberId : string.Empty);
            PrototypeRpgPartyMemberDefinition resolvedDefinition = ResolveAppliedMemberDefinition(baseDefinition, appliedMemberState);
            members[i] = CreateDungeonRuntimeMember(resolvedDefinition ?? baseDefinition, i);
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

        PrototypeRpgSkillDefinition sharedSkillDefinition = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, roleTag);
        ApplySharedSkillDefinition(sharedSkillDefinition, baseStats, ref roleLabel, ref defaultSkillId, ref skillName, ref skillShortText, ref skillType, ref skillPower, ref viewColor);

        if (string.IsNullOrEmpty(defaultSkillId) && !string.IsNullOrEmpty(roleTag))
        {
            PrototypeRpgSkillDefinition fallbackDefinition = PrototypeRpgSkillCatalog.GetFallbackDefinitionForRoleTag(roleTag);
            ApplySharedSkillDefinition(fallbackDefinition, baseStats, ref roleLabel, ref defaultSkillId, ref skillName, ref skillShortText, ref skillType, ref skillPower, ref viewColor);
        }

        if (string.IsNullOrEmpty(roleLabel))
        {
            roleLabel = string.IsNullOrEmpty(roleTag) ? "Adventurer" : roleTag;
        }

        if (string.IsNullOrEmpty(skillName))
        {
            skillName = "Skill";
        }

        skillPower += ResolveRpgAppliedSkillPowerBonus(memberDefinition);

        return new DungeonPartyMemberRuntimeData(memberId, displayName, roleLabel, roleTag, defaultSkillId, skillName, skillShortText, skillType, partySlotIndex, baseStats.MaxHp, baseStats.Attack, baseStats.Defense, baseStats.Speed, skillPower, viewColor);
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
            party.PartyDefinition = party.PartyDefinition ?? PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
            party.DisplayName = party.PartyDefinition != null && !string.IsNullOrEmpty(party.PartyDefinition.DisplayName)
                ? party.PartyDefinition.DisplayName
                : partyId;
        }

        PrototypeRpgAppliedPartyProgressState appliedProgressState = GetOrCreateRpgAppliedPartyProgressState(cityId, partyId, party.PartyDefinition);
        party.Members = CreateDungeonRuntimeMembers(party.PartyDefinition, appliedProgressState);
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
        if (member == null)
        {
            return false;
        }

        PrototypeRpgSkillDefinition resolvedSkillDefinition = ResolveMemberSkillDefinition(member);
        return GetResolvedSkillTargetKind(member, resolvedSkillDefinition) == "single_enemy";
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
        int chestReward = template != null ? template.ChestRewardAmount : ChestRewardAmount;
        _activeChest = new DungeonChestRuntimeData("room-cache", 2, Room2ChestGridPosition, DungeonRewardResourceId, chestReward);

        PrototypeRpgEncounterVariationDefinition variation = ResolveEncounterVariationDefinition(_currentDungeonId, _selectedRouteId);
        if (variation != null)
        {
            BuildFixedDungeonRoomFromVariation(variation);
        }
        else
        {
            BuildFixedDungeonRoomFromDefinitions(_currentDungeonId, _selectedRouteId);
        }

        SyncEliteDefinitionPresentation();

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
        _resultLostPendingRewardAmount = 0;
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
        int lostPendingRewardAmount = 0;
        if (!success && _eliteBonusRewardPending > 0 && !_eliteBonusRewardGranted)
        {
            lostPendingRewardAmount = _eliteBonusRewardPending;
            AppendBattleLog("Pending elite bonus reward was lost.");
            _eliteBonusRewardPending = 0;
        }

        _resultLostPendingRewardAmount = Mathf.Max(0, lostPendingRewardAmount);
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

        _latestRpgRunResultSnapshot = BuildRpgRunResultSnapshot(outcomeKey, safeReturnedLoot, safeResultSummary, _resultLostPendingRewardAmount);
        ApplyRpgRunResultSnapshotToLegacyFields(_latestRpgRunResultSnapshot);

        if (resultState == RunResultState.Clear)
        {
            RecordBattleEvent(PrototypeBattleEventKeys.BattleVictory, string.Empty, _currentDungeonId, safeReturnedLoot, safeResultSummary, phaseKey: "victory", actorName: ActiveDungeonPartyText, targetName: _currentDungeonName, shortText: "Run clear");
        }
        else if (resultState == RunResultState.Defeat)
        {
            RecordBattleEvent(PrototypeBattleEventKeys.BattleDefeat, string.Empty, _currentDungeonId, 0, safeResultSummary, phaseKey: "defeat", actorName: ActiveDungeonPartyText, targetName: _currentDungeonName, shortText: "Run defeat");
        }
        else if (resultState == RunResultState.Retreat)
        {
            RecordBattleEvent(PrototypeBattleEventKeys.RetreatConfirmed, string.Empty, _currentDungeonId, 0, safeResultSummary, actionKey: "retreat", phaseKey: "retreat", actorName: ActiveDungeonPartyText, targetName: _currentDungeonName, shortText: "Run retreat");
        }

        RecordBattleEvent(PrototypeBattleEventKeys.BattleEnd, string.Empty, _currentDungeonId, safeReturnedLoot, safeResultSummary, phaseKey: resultState == RunResultState.Clear ? "victory" : resultState == RunResultState.Defeat ? "defeat" : resultState == RunResultState.Retreat ? "retreat" : GetBattlePhaseKey(), actorName: ActiveDungeonPartyText, targetName: _currentDungeonName, shortText: "Battle end");
        _latestRpgProgressionSeedSnapshot = BuildRpgProgressionSeedSnapshot(_latestRpgRunResultSnapshot);
        _latestRpgCombatContributionSnapshot = BuildRpgCombatContributionSnapshot(_latestRpgRunResultSnapshot);
        _latestRpgProgressionSeedSnapshot.UnlockSeeds = BuildRpgUnlockSeedSnapshots(_latestRpgRunResultSnapshot, _latestRpgCombatContributionSnapshot);
        _latestRpgProgressionPreviewSnapshot = BuildRpgProgressionPreviewSnapshot(_latestRpgRunResultSnapshot, _latestRpgProgressionSeedSnapshot, _latestRpgCombatContributionSnapshot);
        CommitRpgPostRunUpgradeChoices();
        UpdateBattleResultSnapshot(outcomeKey);
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
        if (_resultLostPendingRewardAmount > 0)
        {
            AppendBattleLog("Lost pending bonus reward: " + BuildLootAmountText(_resultLostPendingRewardAmount) + ".");
        }
        SetBattleFeedbackText(resultState == RunResultState.Clear ? "Run clear." : resultState == RunResultState.Defeat ? "The party was defeated." : resultState == RunResultState.Retreat ? "The party retreated." : "Run complete.");

        if (_runtimeEconomyState != null && !string.IsNullOrEmpty(_currentHomeCityId) && !string.IsNullOrEmpty(_currentDungeonId))
        {
            _runtimeEconomyState.ResolveDungeonRun(_currentHomeCityId, _currentDungeonId, DungeonRewardResourceId, safeReturnedLoot, success, safeResultSummary, _latestRpgRunResultSnapshot.SurvivingMembersSummary, _latestRpgRunResultSnapshot.EncounterOutcome.ClearedEncounterSummary, _latestRpgRunResultSnapshot.EncounterOutcome.SelectedEventChoice, _latestRpgRunResultSnapshot.LootOutcome.FinalLootSummary, BuildSelectedRouteSummary());
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
        _resultLostPendingRewardAmount = 0;
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
        ResetCombatRulePipelineState();
        _latestRpgRunResultSnapshot = new PrototypeRpgRunResultSnapshot();
        ResetRpgProgressionSeedState();
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

