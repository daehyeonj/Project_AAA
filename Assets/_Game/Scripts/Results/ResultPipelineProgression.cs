using System;
using System.Collections.Generic;

public sealed class PostRunProgressionInput
{
    public PrototypeDungeonRunResultContext CompatibilityRunResultContext = new PrototypeDungeonRunResultContext();
    public BattleResult BattleResult = new BattleResult();
    public string DungeonDangerLabel = string.Empty;
    public string RouteRiskLabel = string.Empty;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalHealingDone;
    public int[] MemberDamageDealt = Array.Empty<int>();
    public int[] MemberDamageTaken = Array.Empty<int>();
    public int[] MemberHealingDone = Array.Empty<int>();
    public int[] MemberActionCount = Array.Empty<int>();
    public int[] MemberKillCount = Array.Empty<int>();
    public PrototypeBattleEventRecord[] RecentEvents = Array.Empty<PrototypeBattleEventRecord>();
}

public sealed class PostRunProgressionOutput
{
    public PrototypeRpgProgressionSeedSnapshot ProgressionSeed = new PrototypeRpgProgressionSeedSnapshot();
    public PrototypeRpgCombatContributionSnapshot CombatContribution = new PrototypeRpgCombatContributionSnapshot();
    public PrototypeRpgProgressionPreviewSnapshot ProgressionPreview = new PrototypeRpgProgressionPreviewSnapshot();
}

public static class ResultPipelineProgression
{
    private const string RiskyRouteId = "risky";

    public static PostRunProgressionOutput Build(PostRunResolutionInput handoffInput)
    {
        PostRunResolutionInput safeHandoff = handoffInput ?? new PostRunResolutionInput();
        PostRunProgressionInput input = safeHandoff.ProgressionInput ?? new PostRunProgressionInput();
        input.CompatibilityRunResultContext = input.CompatibilityRunResultContext ?? safeHandoff.CompatibilityRunResultContext ?? new PrototypeDungeonRunResultContext();
        input.BattleResult = input.BattleResult ?? safeHandoff.BattleResult ?? new BattleResult();
        return Build(input);
    }

    public static PostRunProgressionOutput Build(PostRunProgressionInput input)
    {
        PostRunProgressionInput safeInput = input ?? new PostRunProgressionInput();
        PrototypeDungeonRunResultContext context = safeInput.CompatibilityRunResultContext ?? new PrototypeDungeonRunResultContext();
        PrototypeRpgRunResultSnapshot runResult = context.CanonicalRunResult ?? new PrototypeRpgRunResultSnapshot();
        BattleResult battleResult = safeInput.BattleResult ?? new BattleResult();

        PostRunProgressionOutput output = new PostRunProgressionOutput();
        output.ProgressionSeed = BuildProgressionSeed(runResult, context, battleResult, safeInput);
        output.CombatContribution = BuildCombatContribution(runResult, battleResult, safeInput);
        output.ProgressionPreview = BuildProgressionPreview(runResult, output.ProgressionSeed, output.CombatContribution);
        return output;
    }

    private static PrototypeRpgProgressionSeedSnapshot BuildProgressionSeed(
        PrototypeRpgRunResultSnapshot runResult,
        PrototypeDungeonRunResultContext context,
        BattleResult battleResult,
        PostRunProgressionInput input)
    {
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = runResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = runResult.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = runResult.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome.Members ?? Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();

        PrototypeRpgProgressionSeedSnapshot seed = new PrototypeRpgProgressionSeedSnapshot();
        seed.ResultStateKey = Value(runResult.ResultStateKey);
        seed.DungeonId = Value(runResult.DungeonId);
        seed.DungeonLabel = Value(runResult.DungeonLabel);
        seed.DungeonDangerLabel = FirstMeaningful(input.DungeonDangerLabel, context.DungeonDangerText);
        seed.RouteId = Value(runResult.RouteId);
        seed.RouteLabel = Value(runResult.RouteLabel);
        seed.RouteRiskLabel = FirstMeaningful(input.RouteRiskLabel, context.RouteRiskText);
        seed.TotalTurnsTaken = Max(0, runResult.TotalTurnsTaken);
        seed.ClearedEncounterCount = Max(0, encounterOutcome.ClearedEncounterCount);
        seed.EliteDefeated = eliteOutcome.IsEliteDefeated;
        seed.EliteName = Value(eliteOutcome.EliteName);
        seed.EliteTypeLabel = Value(eliteOutcome.EliteTypeLabel);
        seed.PartyConditionText = Value(partyOutcome.PartyConditionText);
        seed.SurvivingMemberCount = Max(0, partyOutcome.SurvivingMemberCount);
        seed.KnockedOutMemberCount = Max(0, partyOutcome.KnockedOutMemberCount);
        seed.EliteClearBonusHint = eliteOutcome.EliteBonusRewardEarned ? "Elite bonus reward earned." : eliteOutcome.IsEliteDefeated ? "Elite clear completed." : "Elite clear not achieved.";
        seed.RouteRiskHint = string.IsNullOrEmpty(seed.RouteRiskLabel) ? "Route risk: none." : "Route risk: " + seed.RouteRiskLabel + ".";
        seed.DungeonDangerHint = string.IsNullOrEmpty(seed.DungeonDangerLabel) ? "Dungeon danger: none." : "Dungeon danger: " + seed.DungeonDangerLabel + ".";
        seed.BattleContextSummaryText = FirstMeaningful(runResult.BattleContextSummaryText, context.BattleContextSummaryText, battleResult.BattleContextSummaryText);
        seed.BattleRuntimeSummaryText = FirstMeaningful(runResult.BattleRuntimeSummaryText, context.BattleRuntimeSummaryText, battleResult.RuntimeSummaryText);
        seed.BattleRuleSummaryText = FirstMeaningful(runResult.BattleRuleSummaryText, context.BattleRuleSummaryText, battleResult.LaneRuleSummaryText);
        seed.BattleResultCoreSummaryText = FirstMeaningful(runResult.BattleResultCoreSummaryText, context.BattleResultCoreSummaryText, battleResult.ResultSummaryText);
        seed.GearRewardCandidateSummaryText = FirstMeaningful(runResult.GearRewardCandidateSummaryText, context.GearRewardCandidateSummaryText);
        seed.EquipSwapChoiceSummaryText = FirstMeaningful(runResult.EquipSwapChoiceSummaryText, context.EquipSwapChoiceSummaryText);
        seed.GearCarryContinuitySummaryText = FirstMeaningful(runResult.GearCarryContinuitySummaryText, context.GearCarryContinuitySummaryText);
        seed.Loot = BuildLootSeed(runResult.LootOutcome);
        seed.RewardTags = BuildRewardTags(runResult);
        seed.GrowthTags = BuildGrowthTags(runResult);

        PrototypeRpgMemberProgressionSeed[] memberSeeds = new PrototypeRpgMemberProgressionSeed[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot member = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            memberSeeds[i] = new PrototypeRpgMemberProgressionSeed
            {
                MemberId = Value(member.MemberId),
                DisplayName = Value(member.DisplayName),
                RoleTag = Value(member.RoleTag),
                RoleLabel = Value(member.RoleLabel),
                DefaultSkillId = Value(member.DefaultSkillId),
                GrowthTrackId = Value(member.GrowthTrackId),
                JobId = Value(member.JobId),
                EquipmentLoadoutId = Value(member.EquipmentLoadoutId),
                SkillLoadoutId = Value(member.SkillLoadoutId),
                ResolvedSkillName = Value(member.ResolvedSkillName),
                ResolvedSkillShortText = Value(member.ResolvedSkillShortText),
                EquipmentSummaryText = Value(member.EquipmentSummaryText),
                GearContributionSummaryText = Value(member.GearContributionSummaryText),
                AppliedProgressionSummaryText = Value(member.AppliedProgressionSummaryText),
                CurrentRunSummaryText = Value(member.CurrentRunSummaryText),
                NextRunPreviewSummaryText = Value(member.NextRunPreviewSummaryText),
                Survived = member.Survived,
                KnockedOut = member.KnockedOut,
                CurrentHp = Max(0, member.CurrentHp),
                MaxHp = Max(1, member.MaxHp),
                Combat = new PrototypeRpgCombatContributionSeed
                {
                    DamageDealt = Contribution(input.MemberDamageDealt, i),
                    DamageTaken = Contribution(input.MemberDamageTaken, i),
                    HealingDone = Contribution(input.MemberHealingDone, i),
                    ActionCount = Contribution(input.MemberActionCount, i)
                }
            };
        }

        seed.Members = memberSeeds;
        return seed;
    }

    private static PrototypeRpgCombatContributionSnapshot BuildCombatContribution(
        PrototypeRpgRunResultSnapshot runResult,
        BattleResult battleResult,
        PostRunProgressionInput input)
    {
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = runResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome.Members ?? Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();

        PrototypeRpgCombatContributionSnapshot snapshot = new PrototypeRpgCombatContributionSnapshot();
        snapshot.ResultStateKey = Value(runResult.ResultStateKey);
        snapshot.DungeonId = Value(runResult.DungeonId);
        snapshot.DungeonLabel = Value(runResult.DungeonLabel);
        snapshot.RouteId = Value(runResult.RouteId);
        snapshot.RouteLabel = Value(runResult.RouteLabel);
        snapshot.TotalTurnsTaken = Max(0, runResult.TotalTurnsTaken);
        snapshot.SurvivingMemberCount = Max(0, partyOutcome.SurvivingMemberCount);
        snapshot.KnockedOutMemberCount = Max(0, partyOutcome.KnockedOutMemberCount);
        snapshot.TotalDamageDealt = Max(0, input.TotalDamageDealt > 0 ? input.TotalDamageDealt : battleResult.TotalDamageDealt);
        snapshot.TotalDamageTaken = Max(0, input.TotalDamageTaken > 0 ? input.TotalDamageTaken : battleResult.TotalDamageTaken);
        snapshot.TotalHealingDone = Max(0, input.TotalHealingDone > 0 ? input.TotalHealingDone : battleResult.TotalHealingDone);
        snapshot.RecentEvents = CopyBattleEventRecords(input.RecentEvents);

        PrototypeRpgMemberContributionSnapshot[] memberSnapshots = new PrototypeRpgMemberContributionSnapshot[members.Length];
        bool eliteDefeated = runResult.EliteOutcome != null && runResult.EliteOutcome.IsEliteDefeated;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot member = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            memberSnapshots[i] = new PrototypeRpgMemberContributionSnapshot
            {
                MemberId = Value(member.MemberId),
                DisplayName = Value(member.DisplayName),
                RoleTag = Value(member.RoleTag),
                RoleLabel = Value(member.RoleLabel),
                DefaultSkillId = Value(member.DefaultSkillId),
                DamageDealt = Contribution(input.MemberDamageDealt, i),
                DamageTaken = Contribution(input.MemberDamageTaken, i),
                HealingDone = Contribution(input.MemberHealingDone, i),
                ActionCount = Contribution(input.MemberActionCount, i),
                KillCount = Contribution(input.MemberKillCount, i),
                KnockedOut = member.KnockedOut,
                Survived = member.Survived,
                EliteVictor = eliteDefeated && member.Survived
            };
        }

        snapshot.Members = memberSnapshots;
        return snapshot;
    }

    private static PrototypeRpgProgressionPreviewSnapshot BuildProgressionPreview(
        PrototypeRpgRunResultSnapshot runResult,
        PrototypeRpgProgressionSeedSnapshot seed,
        PrototypeRpgCombatContributionSnapshot contribution)
    {
        PrototypeRpgMemberContributionSnapshot[] contributions = contribution.Members ?? Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        PrototypeRpgMemberProgressionSeed[] seedMembers = seed.Members ?? Array.Empty<PrototypeRpgMemberProgressionSeed>();

        PrototypeRpgProgressionPreviewSnapshot preview = new PrototypeRpgProgressionPreviewSnapshot();
        preview.ResultStateKey = Value(runResult.ResultStateKey);
        preview.DungeonId = Value(runResult.DungeonId);
        preview.DungeonLabel = Value(runResult.DungeonLabel);
        preview.DungeonDangerLabel = Value(seed.DungeonDangerLabel);
        preview.RouteId = Value(runResult.RouteId);
        preview.RouteLabel = Value(runResult.RouteLabel);
        preview.RouteRiskLabel = Value(seed.RouteRiskLabel);
        preview.EliteDefeated = seed.EliteDefeated;
        preview.TotalLootGained = seed.Loot != null ? Max(0, seed.Loot.TotalLootGained) : 0;
        preview.BattleContextSummaryText = Value(seed.BattleContextSummaryText);
        preview.BattleRuntimeSummaryText = Value(seed.BattleRuntimeSummaryText);
        preview.BattleRuleSummaryText = Value(seed.BattleRuleSummaryText);
        preview.BattleResultCoreSummaryText = Value(seed.BattleResultCoreSummaryText);
        preview.GearRewardCandidateSummaryText = Value(seed.GearRewardCandidateSummaryText);
        preview.EquipSwapChoiceSummaryText = Value(seed.EquipSwapChoiceSummaryText);
        preview.GearCarryContinuitySummaryText = Value(seed.GearCarryContinuitySummaryText);
        preview.RewardHintTags = BuildPreviewRewardHintTags(runResult, seed);
        preview.GrowthHintTags = BuildPreviewGrowthHintTags(seed, contribution);

        PrototypeRpgMemberProgressPreview[] members = new PrototypeRpgMemberProgressPreview[contributions.Length];
        for (int i = 0; i < contributions.Length; i++)
        {
            PrototypeRpgMemberContributionSnapshot memberContribution = contributions[i] ?? new PrototypeRpgMemberContributionSnapshot();
            PrototypeRpgMemberProgressionSeed seedMember = i < seedMembers.Length ? seedMembers[i] ?? new PrototypeRpgMemberProgressionSeed() : new PrototypeRpgMemberProgressionSeed();
            members[i] = new PrototypeRpgMemberProgressPreview
            {
                MemberId = FirstValue(memberContribution.MemberId, seedMember.MemberId),
                DisplayName = FirstValue(memberContribution.DisplayName, seedMember.DisplayName),
                RoleTag = FirstValue(memberContribution.RoleTag, seedMember.RoleTag),
                RoleLabel = FirstValue(memberContribution.RoleLabel, seedMember.RoleLabel),
                GrowthTrackId = Value(seedMember.GrowthTrackId),
                JobId = Value(seedMember.JobId),
                EquipmentLoadoutId = Value(seedMember.EquipmentLoadoutId),
                SkillLoadoutId = Value(seedMember.SkillLoadoutId),
                ResolvedSkillName = Value(seedMember.ResolvedSkillName),
                ResolvedSkillShortText = Value(seedMember.ResolvedSkillShortText),
                EquipmentSummaryText = Value(seedMember.EquipmentSummaryText),
                GearContributionSummaryText = Value(seedMember.GearContributionSummaryText),
                AppliedProgressionSummaryText = Value(seedMember.AppliedProgressionSummaryText),
                CurrentRunSummaryText = Value(seedMember.CurrentRunSummaryText),
                NextRunPreviewSummaryText = Value(seedMember.NextRunPreviewSummaryText),
                Survived = memberContribution.Survived,
                Contribution = memberContribution,
                SuggestedGrowthHintTags = BuildMemberGrowthHintTags(memberContribution),
                SuggestedRewardHintTags = BuildMemberRewardHintTags(memberContribution, runResult),
                NotableOutcomeKey = BuildMemberNotableOutcomeKey(memberContribution, runResult)
            };
        }

        preview.Members = members;
        return preview;
    }

    private static PrototypeRpgLootSeed BuildLootSeed(PrototypeRpgLootOutcomeSnapshot lootOutcome)
    {
        PrototypeRpgLootSeed seed = new PrototypeRpgLootSeed();
        if (lootOutcome == null)
        {
            return seed;
        }

        seed.TotalLootGained = Max(0, lootOutcome.TotalLootGained);
        seed.BattleLootGained = Max(0, lootOutcome.BattleLootGained);
        seed.ChestLootGained = Max(0, lootOutcome.ChestLootGained);
        seed.EventLootGained = Max(0, lootOutcome.EventLootGained);
        seed.EliteRewardAmount = Max(0, lootOutcome.EliteRewardAmount);
        seed.EliteBonusRewardAmount = Max(0, lootOutcome.EliteBonusRewardAmount);
        seed.LootBreakdownSummary = Value(lootOutcome.FinalLootSummary);
        return seed;
    }

    private static string[] BuildRewardTags(PrototypeRpgRunResultSnapshot runResult)
    {
        List<string> tags = new List<string>();
        if (!string.IsNullOrEmpty(runResult.ResultStateKey) && runResult.ResultStateKey != PrototypeBattleOutcomeKeys.None)
        {
            AddTag(tags, runResult.ResultStateKey);
        }

        if (runResult.LootOutcome != null && runResult.LootOutcome.TotalLootGained > 0)
        {
            AddTag(tags, "loot_returned");
        }

        if (runResult.EliteOutcome != null && runResult.EliteOutcome.IsEliteDefeated)
        {
            AddTag(tags, "elite_clear");
        }

        if (runResult.EliteOutcome != null && runResult.EliteOutcome.EliteBonusRewardEarned)
        {
            AddTag(tags, "elite_bonus");
        }

        if (runResult.RouteId == RiskyRouteId)
        {
            AddTag(tags, "risky_route");
        }

        return tags.ToArray();
    }

    private static string[] BuildGrowthTags(PrototypeRpgRunResultSnapshot runResult)
    {
        List<string> tags = new List<string>();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = runResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        if (partyOutcome.KnockedOutMemberCount > 0)
        {
            AddTag(tags, "party_recovery");
        }

        if (partyOutcome.SurvivingMemberCount > 0 && partyOutcome.KnockedOutMemberCount <= 0)
        {
            AddTag(tags, "clean_survival");
        }

        if (runResult.TotalTurnsTaken >= 6)
        {
            AddTag(tags, "endurance_run");
        }

        if (runResult.EliteOutcome != null && runResult.EliteOutcome.IsEliteDefeated)
        {
            AddTag(tags, "elite_mastery");
        }

        return tags.ToArray();
    }

    private static string[] BuildPreviewRewardHintTags(PrototypeRpgRunResultSnapshot runResult, PrototypeRpgProgressionSeedSnapshot seed)
    {
        List<string> tags = new List<string>();
        AppendTags(tags, seed.RewardTags);
        if (runResult.EliteOutcome != null && runResult.EliteOutcome.IsEliteDefeated)
        {
            AddTag(tags, "elite_victor");
        }

        if (runResult.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            AddTag(tags, "retreat_penalty_hint");
        }

        if (runResult.RouteId == RiskyRouteId || seed.RouteId == RiskyRouteId)
        {
            AddTag(tags, "risky_route_hint");
        }

        return tags.ToArray();
    }

    private static string[] BuildPreviewGrowthHintTags(PrototypeRpgProgressionSeedSnapshot seed, PrototypeRpgCombatContributionSnapshot contribution)
    {
        List<string> tags = new List<string>();
        AppendTags(tags, seed.GrowthTags);
        if (contribution.KnockedOutMemberCount > 0)
        {
            AddTag(tags, "knocked_out");
        }

        if (contribution.SurvivingMemberCount > 0)
        {
            AddTag(tags, "survivor");
        }

        if (contribution.TotalHealingDone > 0)
        {
            AddTag(tags, "party_support");
        }

        return tags.ToArray();
    }

    private static string[] BuildMemberGrowthHintTags(PrototypeRpgMemberContributionSnapshot contribution)
    {
        List<string> tags = new List<string>();
        if (contribution.RoleTag == "warrior" && contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt) AddTag(tags, "frontline_pressure");
        if (contribution.RoleTag == "mage" && contribution.DamageDealt >= 8) AddTag(tags, "aoe_clear");
        if (contribution.RoleTag == "cleric" && contribution.HealingDone > 0) AddTag(tags, "party_support");
        if (contribution.Survived) AddTag(tags, "survivor");
        if (contribution.KnockedOut) AddTag(tags, "knocked_out");
        return tags.ToArray();
    }

    private static string[] BuildMemberRewardHintTags(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResult)
    {
        List<string> tags = new List<string>();
        if (contribution.RoleTag == "rogue" && contribution.KillCount > 0) AddTag(tags, "finisher");
        if (runResult.EliteOutcome != null && runResult.EliteOutcome.IsEliteDefeated && contribution.Survived) AddTag(tags, "elite_victor");
        if (runResult.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat) AddTag(tags, "retreat_penalty_hint");
        if (runResult.RouteId == RiskyRouteId) AddTag(tags, "risky_route_hint");
        return tags.ToArray();
    }

    private static string BuildMemberNotableOutcomeKey(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResult)
    {
        if (contribution.KnockedOut) return "knocked_out";
        if (runResult.EliteOutcome != null && runResult.EliteOutcome.IsEliteDefeated && contribution.Survived) return "elite_victor";
        if (contribution.RoleTag == "cleric" && contribution.HealingDone > 0) return "party_support";
        if (contribution.RoleTag == "mage" && contribution.DamageDealt >= 8) return "aoe_clear";
        if (contribution.RoleTag == "rogue" && contribution.KillCount > 0) return "finisher";
        if (contribution.RoleTag == "warrior" && contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt) return "frontline_pressure";
        if (runResult.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat) return "retreat_penalty_hint";
        if (runResult.RouteId == RiskyRouteId) return "risky_route_hint";
        return contribution.Survived ? "survivor" : string.Empty;
    }

    private static PrototypeBattleEventRecord[] CopyBattleEventRecords(PrototypeBattleEventRecord[] source)
    {
        PrototypeBattleEventRecord[] safeSource = source ?? Array.Empty<PrototypeBattleEventRecord>();
        PrototypeBattleEventRecord[] copies = new PrototypeBattleEventRecord[safeSource.Length];
        for (int i = 0; i < safeSource.Length; i++)
        {
            PrototypeBattleEventRecord item = safeSource[i] ?? new PrototypeBattleEventRecord();
            copies[i] = new PrototypeBattleEventRecord
            {
                EventId = Value(item.EventId),
                Sequence = Max(0, item.Sequence),
                EventKey = Value(item.EventKey),
                EventType = Value(item.EventType),
                PhaseKey = Value(item.PhaseKey),
                ActorId = Value(item.ActorId),
                ActorName = Value(item.ActorName),
                TargetId = Value(item.TargetId),
                TargetName = Value(item.TargetName),
                ActionKey = Value(item.ActionKey),
                SkillId = Value(item.SkillId),
                Amount = Max(0, item.Amount),
                Value = Max(0, item.Value),
                StepIndex = Max(0, item.StepIndex),
                TurnIndex = Max(0, item.TurnIndex),
                ShortText = Value(item.ShortText),
                Summary = Value(item.Summary)
            };
        }

        return copies;
    }

    private static void AppendTags(List<string> tags, string[] source)
    {
        string[] safeSource = source ?? Array.Empty<string>();
        for (int i = 0; i < safeSource.Length; i++)
        {
            AddTag(tags, safeSource[i]);
        }
    }

    private static void AddTag(List<string> tags, string tag)
    {
        if (tags == null || string.IsNullOrEmpty(tag) || tags.Contains(tag))
        {
            return;
        }

        tags.Add(tag);
    }

    private static int Contribution(int[] values, int index)
    {
        return values != null && index >= 0 && index < values.Length ? Max(0, values[index]) : 0;
    }

    private static int Max(int minimum, int value)
    {
        return value < minimum ? minimum : value;
    }

    private static string Value(string text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : text;
    }

    private static string FirstValue(string primary, string fallback)
    {
        return !string.IsNullOrEmpty(primary) ? primary : Value(fallback);
    }

    private static string FirstMeaningful(params string[] values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (!string.IsNullOrEmpty(values[i]) && values[i] != "None" && values[i] != "(missing)")
            {
                return values[i];
            }
        }

        return string.Empty;
    }
}
