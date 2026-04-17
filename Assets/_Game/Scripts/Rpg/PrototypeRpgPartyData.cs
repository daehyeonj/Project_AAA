public sealed class PrototypeRpgStatBlock
{
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int Speed { get; }

    public PrototypeRpgStatBlock(int maxHp, int attack, int defense, int speed)
    {
        MaxHp = maxHp > 0 ? maxHp : 1;
        Attack = attack > 0 ? attack : 1;
        Defense = defense >= 0 ? defense : 0;
        Speed = speed >= 0 ? speed : 0;
    }
}

public sealed class PrototypeRpgSkillDefinition
{
    public string SkillId { get; }
    public string DisplayName { get; }
    public string ShortText { get; }
    public string TargetKind { get; }
    public string EffectType { get; }
    public int PowerValue { get; }
    public string PowerHint { get; }
    public string EffectHint { get; }
    public string RoleHint { get; }

    public PrototypeRpgSkillDefinition(
        string skillId,
        string displayName,
        string shortText,
        string targetKind,
        string effectType,
        int powerValue,
        string powerHint,
        string effectHint,
        string roleHint)
    {
        SkillId = string.IsNullOrWhiteSpace(skillId) ? string.Empty : skillId.Trim().ToLowerInvariant();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Skill" : displayName.Trim();
        ShortText = string.IsNullOrWhiteSpace(shortText) ? string.Empty : shortText.Trim();
        TargetKind = string.IsNullOrWhiteSpace(targetKind) ? "single_enemy" : targetKind.Trim().ToLowerInvariant();
        EffectType = string.IsNullOrWhiteSpace(effectType) ? "damage" : effectType.Trim().ToLowerInvariant();
        PowerValue = powerValue > 0 ? powerValue : 1;
        PowerHint = string.IsNullOrWhiteSpace(powerHint) ? string.Empty : powerHint.Trim();
        EffectHint = string.IsNullOrWhiteSpace(effectHint) ? string.Empty : effectHint.Trim();
        RoleHint = string.IsNullOrWhiteSpace(roleHint) ? string.Empty : roleHint.Trim();
    }
}

public static class PrototypeRpgSkillCatalog
{
    private static readonly PrototypeRpgSkillDefinition[] SharedDefinitions =
    {
        new PrototypeRpgSkillDefinition(
            "skill_power_strike",
            "Power Strike",
            "Heavy single-target strike.",
            "single_enemy",
            "damage",
            10,
            "high",
            "front-loaded physical burst",
            "Warrior"),
        new PrototypeRpgSkillDefinition(
            "skill_weak_point",
            "Weak Point",
            "Finisher that hits harder on weak targets.",
            "single_enemy",
            "finisher_damage",
            7,
            "medium_high",
            "precision finisher",
            "Rogue"),
        new PrototypeRpgSkillDefinition(
            "skill_arcane_burst",
            "Arcane Burst",
            "Arcane blast that hits all enemies.",
            "all_enemies",
            "damage",
            6,
            "medium",
            "multi-target arcane burst",
            "Mage"),
        new PrototypeRpgSkillDefinition(
            "skill_radiant_hymn",
            "Radiant Hymn",
            "Party heal that restores all allies.",
            "all_allies",
            "heal",
            5,
            "support",
            "party healing pulse",
            "Cleric")
    };

    public static PrototypeRpgSkillDefinition GetDefinition(string skillId)
    {
        string normalizedSkillId = NormalizeKey(skillId);
        if (string.IsNullOrEmpty(normalizedSkillId))
        {
            return null;
        }

        for (int i = 0; i < SharedDefinitions.Length; i++)
        {
            PrototypeRpgSkillDefinition definition = SharedDefinitions[i];
            if (definition != null && definition.SkillId == normalizedSkillId)
            {
                return definition;
            }
        }

        return null;
    }

    public static PrototypeRpgSkillDefinition GetFallbackDefinitionForRoleTag(string roleTag)
    {
        switch (NormalizeKey(roleTag))
        {
            case "warrior":
                return GetDefinition("skill_power_strike");
            case "rogue":
                return GetDefinition("skill_weak_point");
            case "mage":
                return GetDefinition("skill_arcane_burst");
            case "cleric":
                return GetDefinition("skill_radiant_hymn");
            default:
                return null;
        }
    }

    public static PrototypeRpgSkillDefinition ResolveDefinition(string skillId, string roleTag)
    {
        PrototypeRpgSkillDefinition directMatch = GetDefinition(skillId);
        return directMatch ?? GetFallbackDefinitionForRoleTag(roleTag);
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgPartyMemberDefinition
{
    public string MemberId { get; }
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string RoleLabel { get; }
    public int PartySlotIndex { get; }
    public PrototypeRpgStatBlock BaseStats { get; }
    public string DefaultSkillId { get; }
    public string DefaultSkillName { get; }
    public string DefaultSkillShortText { get; }
    public string GrowthTrackId { get; }
    public string JobId { get; }
    public string EquipmentLoadoutId { get; }
    public string SkillLoadoutId { get; }

    public PrototypeRpgPartyMemberDefinition(
        string memberId,
        string displayName,
        string roleTag,
        string roleLabel,
        int partySlotIndex,
        PrototypeRpgStatBlock baseStats,
        string defaultSkillId,
        string defaultSkillName,
        string defaultSkillShortText,
        string growthTrackId,
        string jobId,
        string equipmentLoadoutId,
        string skillLoadoutId)
    {
        string normalizedRoleTag = string.IsNullOrWhiteSpace(roleTag) ? "adventurer" : roleTag.Trim().ToLowerInvariant();
        PrototypeRpgSkillDefinition sharedSkill = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, normalizedRoleTag);

        MemberId = string.IsNullOrWhiteSpace(memberId) ? string.Empty : memberId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Adventurer" : displayName.Trim();
        RoleTag = normalizedRoleTag;
        RoleLabel = string.IsNullOrWhiteSpace(roleLabel) ? "Adventurer" : roleLabel.Trim();
        PartySlotIndex = partySlotIndex >= 0 ? partySlotIndex : 0;
        BaseStats = baseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        DefaultSkillId = sharedSkill != null
            ? sharedSkill.SkillId
            : (string.IsNullOrWhiteSpace(defaultSkillId) ? string.Empty : defaultSkillId.Trim().ToLowerInvariant());
        DefaultSkillName = !string.IsNullOrWhiteSpace(defaultSkillName)
            ? defaultSkillName.Trim()
            : (sharedSkill != null ? sharedSkill.DisplayName : "Skill");
        DefaultSkillShortText = !string.IsNullOrWhiteSpace(defaultSkillShortText)
            ? defaultSkillShortText.Trim()
            : (sharedSkill != null ? sharedSkill.ShortText : string.Empty);
        GrowthTrackId = string.IsNullOrWhiteSpace(growthTrackId) ? string.Empty : growthTrackId.Trim();
        JobId = string.IsNullOrWhiteSpace(jobId) ? string.Empty : jobId.Trim();
        EquipmentLoadoutId = string.IsNullOrWhiteSpace(equipmentLoadoutId) ? string.Empty : equipmentLoadoutId.Trim();
        SkillLoadoutId = string.IsNullOrWhiteSpace(skillLoadoutId) ? string.Empty : skillLoadoutId.Trim();
    }
}

public sealed class PrototypeRpgPartyArchetypeDefinition
{
    public string ArchetypeId { get; }
    public string ArchetypeLabel { get; }
    public string StrengthSummaryText { get; }
    public string RouteFitSummaryText { get; }
    public string DoctrineSummaryText { get; }
    public int PowerBonus { get; }
    public int CarryBonus { get; }

    public PrototypeRpgPartyArchetypeDefinition(
        string archetypeId,
        string archetypeLabel,
        string strengthSummaryText,
        string routeFitSummaryText,
        string doctrineSummaryText,
        int powerBonus,
        int carryBonus)
    {
        ArchetypeId = string.IsNullOrWhiteSpace(archetypeId) ? "bulwark" : archetypeId.Trim().ToLowerInvariant();
        ArchetypeLabel = string.IsNullOrWhiteSpace(archetypeLabel) ? "Bulwark Crew" : archetypeLabel.Trim();
        StrengthSummaryText = string.IsNullOrWhiteSpace(strengthSummaryText) ? "Steady frontline sustain." : strengthSummaryText.Trim();
        RouteFitSummaryText = string.IsNullOrWhiteSpace(routeFitSummaryText) ? "Best on safe recovery lines." : routeFitSummaryText.Trim();
        DoctrineSummaryText = string.IsNullOrWhiteSpace(doctrineSummaryText) ? "Doctrine pending." : doctrineSummaryText.Trim();
        PowerBonus = powerBonus;
        CarryBonus = carryBonus;
    }
}

public sealed class PrototypeRpgPartyPromotionDefinition
{
    public string PromotionStateId { get; }
    public string PromotionStateLabel { get; }
    public string PromotionSummaryText { get; }
    public string EquipmentTierKey { get; }
    public int PowerBonus { get; }
    public int CarryBonus { get; }

    public PrototypeRpgPartyPromotionDefinition(
        string promotionStateId,
        string promotionStateLabel,
        string promotionSummaryText,
        string equipmentTierKey,
        int powerBonus,
        int carryBonus)
    {
        PromotionStateId = string.IsNullOrWhiteSpace(promotionStateId) ? "recruit" : promotionStateId.Trim().ToLowerInvariant();
        PromotionStateLabel = string.IsNullOrWhiteSpace(promotionStateLabel) ? "Recruit Frame" : promotionStateLabel.Trim();
        PromotionSummaryText = string.IsNullOrWhiteSpace(promotionSummaryText) ? "Recruit frame. First field promotion is still pending." : promotionSummaryText.Trim();
        EquipmentTierKey = string.IsNullOrWhiteSpace(equipmentTierKey) ? "placeholder" : equipmentTierKey.Trim().ToLowerInvariant();
        PowerBonus = powerBonus;
        CarryBonus = carryBonus;
    }
}

public sealed class PrototypeRpgPartyDefinition
{
    public string PartyId { get; }
    public string DisplayName { get; }
    public string ArchetypeId { get; }
    public string ArchetypeLabel { get; }
    public string StrengthSummaryText { get; }
    public string RouteFitSummaryText { get; }
    public string DoctrineSummaryText { get; }
    public string PromotionStateId { get; }
    public string PromotionStateLabel { get; }
    public string PromotionSummaryText { get; }
    public int DerivedPower { get; }
    public int DerivedCarryCapacity { get; }
    public PrototypeRpgPartyMemberDefinition[] Members { get; }

    public PrototypeRpgPartyDefinition(
        string partyId,
        string displayName,
        string archetypeId,
        string archetypeLabel,
        string strengthSummaryText,
        string routeFitSummaryText,
        string doctrineSummaryText,
        string promotionStateId,
        string promotionStateLabel,
        string promotionSummaryText,
        int derivedPower,
        int derivedCarryCapacity,
        PrototypeRpgPartyMemberDefinition[] members)
    {
        PartyId = string.IsNullOrWhiteSpace(partyId) ? string.Empty : partyId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Test Party" : displayName.Trim();
        ArchetypeId = string.IsNullOrWhiteSpace(archetypeId) ? "bulwark" : archetypeId.Trim().ToLowerInvariant();
        ArchetypeLabel = string.IsNullOrWhiteSpace(archetypeLabel) ? "Bulwark Crew" : archetypeLabel.Trim();
        StrengthSummaryText = string.IsNullOrWhiteSpace(strengthSummaryText) ? "Steady frontline sustain." : strengthSummaryText.Trim();
        RouteFitSummaryText = string.IsNullOrWhiteSpace(routeFitSummaryText) ? "Best on safe recovery lines." : routeFitSummaryText.Trim();
        DoctrineSummaryText = string.IsNullOrWhiteSpace(doctrineSummaryText) ? "Doctrine pending." : doctrineSummaryText.Trim();
        PromotionStateId = string.IsNullOrWhiteSpace(promotionStateId) ? "recruit" : promotionStateId.Trim().ToLowerInvariant();
        PromotionStateLabel = string.IsNullOrWhiteSpace(promotionStateLabel) ? "Recruit Frame" : promotionStateLabel.Trim();
        PromotionSummaryText = string.IsNullOrWhiteSpace(promotionSummaryText) ? "Recruit frame. First field promotion is still pending." : promotionSummaryText.Trim();
        DerivedPower = derivedPower > 0 ? derivedPower : 1;
        DerivedCarryCapacity = derivedCarryCapacity > 0 ? derivedCarryCapacity : 1;
        Members = members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
    }
}

public static class PrototypeRpgPartyCatalog
{
    private static readonly PrototypeRpgPartyArchetypeDefinition[] Archetypes =
    {
        new PrototypeRpgPartyArchetypeDefinition(
            "bulwark",
            "Bulwark Crew",
            "Steady frontline sustain that protects restart stability.",
            "Best on safe or recover-first lanes where stability matters more than spike payout.",
            "Bulwark doctrine anchors the warrior and cleric so the party can absorb safer attrition.",
            0,
            0),
        new PrototypeRpgPartyArchetypeDefinition(
            "outrider",
            "Outrider Cell",
            "Burst tempo party that converts pressure into faster shard spikes.",
            "Best on risky pressure lanes where burst damage can end fights before attrition sticks.",
            "Outrider doctrine shifts weight to rogue and mage tempo so the run can cash in on pressure.",
            1,
            0),
        new PrototypeRpgPartyArchetypeDefinition(
            "salvager",
            "Salvager Wing",
            "Long-haul sustain party that banks safer payout and stronger carry.",
            "Best on balanced or safe lanes where haul quality and recovery both matter.",
            "Salvager doctrine trims volatility and leans on cleaner recover loops plus better carry.",
            0,
            1)
    };

    private static readonly PrototypeRpgPartyPromotionDefinition[] Promotions =
    {
        new PrototypeRpgPartyPromotionDefinition(
            "recruit",
            "Recruit Frame",
            "Recruit frame. First field promotion is still pending.",
            "placeholder",
            0,
            0),
        new PrototypeRpgPartyPromotionDefinition(
            "field",
            "Field Promotion",
            "Field-promoted after a clean return. Gear and pacing are sharper now.",
            "fieldkit",
            1,
            0),
        new PrototypeRpgPartyPromotionDefinition(
            "elite",
            "Elite Promotion",
            "Elite promotion. The party now carries veteran gear and a higher payout ceiling.",
            "elite",
            2,
            1)
    };

    public static PrototypeRpgPartyDefinition CreateDefaultPlaceholderParty(string partyId)
    {
        return CreateRuntimeParty(partyId, "bulwark", "recruit");
    }

    public static PrototypeRpgPartyDefinition CreateRuntimeParty(string partyId, string archetypeId, string promotionStateId, string displayName = null)
    {
        string safePartyId = string.IsNullOrWhiteSpace(partyId) ? "Test Party" : partyId.Trim();
        PrototypeRpgPartyArchetypeDefinition archetype = ResolveArchetype(archetypeId);
        PrototypeRpgPartyPromotionDefinition promotion = ResolvePromotion(promotionStateId);

        return new PrototypeRpgPartyDefinition(
            safePartyId,
            string.IsNullOrWhiteSpace(displayName) ? safePartyId : displayName.Trim(),
            archetype.ArchetypeId,
            archetype.ArchetypeLabel,
            archetype.StrengthSummaryText,
            archetype.RouteFitSummaryText,
            archetype.DoctrineSummaryText,
            promotion.PromotionStateId,
            promotion.PromotionStateLabel,
            promotion.PromotionSummaryText,
            ResolveDerivedPower(archetype.ArchetypeId, promotion.PromotionStateId),
            ResolveDerivedCarryCapacity(archetype.ArchetypeId, promotion.PromotionStateId),
            new[]
            {
                CreateRuntimeMember(safePartyId, archetype, promotion, 0, "alden", "Alden", "warrior", "Warrior", 28, 5, 2, 3, "skill_power_strike", "growth_frontline"),
                CreateRuntimeMember(safePartyId, archetype, promotion, 1, "mira", "Mira", "rogue", "Rogue", 19, 4, 1, 5, "skill_weak_point", "growth_precision"),
                CreateRuntimeMember(safePartyId, archetype, promotion, 2, "rune", "Rune", "mage", "Mage", 16, 3, 0, 4, "skill_arcane_burst", "growth_arcane"),
                CreateRuntimeMember(safePartyId, archetype, promotion, 3, "lia", "Lia", "cleric", "Cleric", 22, 3, 1, 2, "skill_radiant_hymn", "growth_support")
            });
    }

    public static string ResolveArchetypeIdForSeed(string homeCityId, int recruitSequence)
    {
        int safeSequence = recruitSequence > 0 ? recruitSequence : 1;
        string normalizedCityId = NormalizeKey(homeCityId);
        if (normalizedCityId == "city-a")
        {
            return safeSequence % 3 == 0 ? "salvager" : "bulwark";
        }

        if (normalizedCityId == "city-b")
        {
            return safeSequence % 2 == 0 ? "salvager" : "outrider";
        }

        return Archetypes[(safeSequence - 1) % Archetypes.Length].ArchetypeId;
    }

    public static string GetInitialPromotionStateId()
    {
        return Promotions[0].PromotionStateId;
    }

    public static string GetNextPromotionStateId(string currentPromotionStateId)
    {
        string normalizedStateId = NormalizeKey(currentPromotionStateId);
        for (int i = 0; i < Promotions.Length; i++)
        {
            PrototypeRpgPartyPromotionDefinition promotion = Promotions[i];
            if (promotion != null && promotion.PromotionStateId == normalizedStateId)
            {
                return i + 1 < Promotions.Length ? Promotions[i + 1].PromotionStateId : promotion.PromotionStateId;
            }
        }

        return Promotions[1].PromotionStateId;
    }

    public static string GetPromotionStateLabel(string promotionStateId)
    {
        return ResolvePromotion(promotionStateId).PromotionStateLabel;
    }

    public static string GetArchetypeLabel(string archetypeId)
    {
        return ResolveArchetype(archetypeId).ArchetypeLabel;
    }

    public static string GetStrengthSummaryText(string archetypeId)
    {
        return ResolveArchetype(archetypeId).StrengthSummaryText;
    }

    public static string GetRouteFitSummaryText(string archetypeId)
    {
        return ResolveArchetype(archetypeId).RouteFitSummaryText;
    }

    public static string GetDoctrineSummaryText(string archetypeId)
    {
        return ResolveArchetype(archetypeId).DoctrineSummaryText;
    }

    public static string GetPromotionSummaryText(string promotionStateId)
    {
        return ResolvePromotion(promotionStateId).PromotionSummaryText;
    }

    public static int ResolveDerivedPower(string archetypeId, string promotionStateId)
    {
        PrototypeRpgPartyArchetypeDefinition archetype = ResolveArchetype(archetypeId);
        PrototypeRpgPartyPromotionDefinition promotion = ResolvePromotion(promotionStateId);
        return 3 + archetype.PowerBonus + promotion.PowerBonus;
    }

    public static int ResolveDerivedCarryCapacity(string archetypeId, string promotionStateId)
    {
        PrototypeRpgPartyArchetypeDefinition archetype = ResolveArchetype(archetypeId);
        PrototypeRpgPartyPromotionDefinition promotion = ResolvePromotion(promotionStateId);
        return 2 + archetype.CarryBonus + promotion.CarryBonus;
    }

    private static PrototypeRpgPartyMemberDefinition CreateRuntimeMember(
        string partyId,
        PrototypeRpgPartyArchetypeDefinition archetype,
        PrototypeRpgPartyPromotionDefinition promotion,
        int partySlotIndex,
        string memberKey,
        string displayName,
        string roleTag,
        string roleLabel,
        int maxHp,
        int attack,
        int defense,
        int speed,
        string defaultSkillId,
        string growthTrackId)
    {
        return new PrototypeRpgPartyMemberDefinition(
            BuildMemberId(partyId, partySlotIndex, memberKey),
            displayName,
            roleTag,
            roleLabel,
            partySlotIndex,
            ApplyArchetypeStats(archetype != null ? archetype.ArchetypeId : string.Empty, roleTag, new PrototypeRpgStatBlock(maxHp, attack, defense, speed)),
            defaultSkillId,
            string.Empty,
            string.Empty,
            ResolveGrowthTrackId(growthTrackId, archetype, promotion),
            ResolveJobId(roleTag, archetype),
            ResolveEquipmentLoadoutId(roleTag, promotion),
            ResolveSkillLoadoutId(roleTag, archetype, promotion));
    }

    private static PrototypeRpgStatBlock ApplyArchetypeStats(string archetypeId, string roleTag, PrototypeRpgStatBlock baseStats)
    {
        PrototypeRpgStatBlock safeStats = baseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        int hp = safeStats.MaxHp;
        int attack = safeStats.Attack;
        int defense = safeStats.Defense;
        int speed = safeStats.Speed;

        switch (NormalizeKey(archetypeId))
        {
            case "outrider":
                switch (NormalizeKey(roleTag))
                {
                    case "warrior":
                        attack += 1;
                        speed += 1;
                        break;
                    case "rogue":
                        attack += 1;
                        speed += 2;
                        break;
                    case "mage":
                        attack += 2;
                        speed += 1;
                        break;
                    case "cleric":
                        speed += 1;
                        break;
                }
                break;

            case "salvager":
                switch (NormalizeKey(roleTag))
                {
                    case "warrior":
                        hp += 2;
                        defense += 1;
                        break;
                    case "rogue":
                        speed += 1;
                        break;
                    case "mage":
                        attack += 1;
                        break;
                    case "cleric":
                        hp += 2;
                        attack += 1;
                        defense += 1;
                        break;
                }
                break;

            default:
                switch (NormalizeKey(roleTag))
                {
                    case "warrior":
                        hp += 4;
                        defense += 1;
                        break;
                    case "rogue":
                        hp += 1;
                        defense += 1;
                        break;
                    case "cleric":
                        hp += 2;
                        defense += 1;
                        break;
                }
                break;
        }

        return new PrototypeRpgStatBlock(hp, attack, defense, speed);
    }

    private static string ResolveGrowthTrackId(
        string baseGrowthTrackId,
        PrototypeRpgPartyArchetypeDefinition archetype,
        PrototypeRpgPartyPromotionDefinition promotion)
    {
        string growthTrackId = string.IsNullOrWhiteSpace(baseGrowthTrackId) ? "growth_party" : baseGrowthTrackId.Trim();
        string archetypeKey = archetype != null ? archetype.ArchetypeId : "bulwark";
        string promotionKey = promotion != null ? promotion.PromotionStateId : "recruit";
        return growthTrackId + "_" + archetypeKey + "_" + promotionKey;
    }

    private static string ResolveJobId(string roleTag, PrototypeRpgPartyArchetypeDefinition archetype)
    {
        string normalizedRoleTag = NormalizeKey(roleTag);
        string doctrineSuffix = archetype != null ? archetype.ArchetypeId : "bulwark";
        return "job_" + normalizedRoleTag + "_" + doctrineSuffix;
    }

    private static string ResolveEquipmentLoadoutId(string roleTag, PrototypeRpgPartyPromotionDefinition promotion)
    {
        string normalizedRoleTag = NormalizeKey(roleTag);
        string tierKey = promotion != null ? promotion.EquipmentTierKey : "placeholder";
        return "equip_" + normalizedRoleTag + "_" + tierKey;
    }

    private static string ResolveSkillLoadoutId(
        string roleTag,
        PrototypeRpgPartyArchetypeDefinition archetype,
        PrototypeRpgPartyPromotionDefinition promotion)
    {
        string normalizedRoleTag = NormalizeKey(roleTag);
        string archetypeKey = archetype != null ? archetype.ArchetypeId : "bulwark";
        string promotionKey = promotion != null ? promotion.PromotionStateId : "recruit";
        return "skillloadout_" + normalizedRoleTag + "_" + archetypeKey + "_" + promotionKey;
    }

    private static PrototypeRpgPartyArchetypeDefinition ResolveArchetype(string archetypeId)
    {
        string normalizedArchetypeId = NormalizeKey(archetypeId);
        for (int i = 0; i < Archetypes.Length; i++)
        {
            PrototypeRpgPartyArchetypeDefinition archetype = Archetypes[i];
            if (archetype != null && archetype.ArchetypeId == normalizedArchetypeId)
            {
                return archetype;
            }
        }

        return Archetypes[0];
    }

    private static PrototypeRpgPartyPromotionDefinition ResolvePromotion(string promotionStateId)
    {
        string normalizedPromotionStateId = NormalizeKey(promotionStateId);
        for (int i = 0; i < Promotions.Length; i++)
        {
            PrototypeRpgPartyPromotionDefinition promotion = Promotions[i];
            if (promotion != null && promotion.PromotionStateId == normalizedPromotionStateId)
            {
                return promotion;
            }
        }

        return Promotions[0];
    }

    private static string BuildMemberId(string partyId, int partySlotIndex, string memberKey)
    {
        string safePartyId = string.IsNullOrWhiteSpace(partyId)
            ? "party"
            : partyId.Trim().ToLowerInvariant().Replace(" ", "-");
        string safeMemberKey = string.IsNullOrWhiteSpace(memberKey)
            ? "member"
            : memberKey.Trim().ToLowerInvariant();
        return safePartyId + "-slot-" + (partySlotIndex + 1) + "-" + safeMemberKey;
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

