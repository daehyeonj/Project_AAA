using System.Collections.Generic;

public sealed class PrototypeRpgEncounterMemberDefinition
{
    public string EnemyId { get; }
    public int SlotIndex { get; }
    public bool IsEliteMember { get; }

    public PrototypeRpgEncounterMemberDefinition(string enemyId, int slotIndex, bool isEliteMember = false)
    {
        EnemyId = string.IsNullOrWhiteSpace(enemyId) ? string.Empty : enemyId.Trim();
        SlotIndex = slotIndex >= 0 ? slotIndex : 0;
        IsEliteMember = isEliteMember;
    }
}

public sealed class PrototypeRpgEncounterDefinition
{
    public string DefinitionId { get; }
    public string EncounterId { get; }
    public string DisplayName { get; }
    public string EncounterTypeLabel { get; }
    public string RoomTypeLabel { get; }
    public string EliteStyleLabel { get; }
    public string RouteRiskLabel { get; }
    public string DangerHintLabel { get; }
    public string RewardPreviewHint { get; }
    public string RewardLabel { get; }
    public int RewardAmountHint { get; }
    public bool IsEliteEncounter { get; }
    public PrototypeRpgEncounterMemberDefinition[] EnemyMembers { get; }

    public PrototypeRpgEncounterDefinition(string definitionId, string encounterId, string displayName, string encounterTypeLabel, string roomTypeLabel, string eliteStyleLabel, string routeRiskLabel, string dangerHintLabel, string rewardPreviewHint, string rewardLabel, int rewardAmountHint, bool isEliteEncounter, PrototypeRpgEncounterMemberDefinition[] enemyMembers)
    {
        DefinitionId = string.IsNullOrWhiteSpace(definitionId) ? string.Empty : definitionId.Trim();
        EncounterId = string.IsNullOrWhiteSpace(encounterId) ? string.Empty : encounterId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Encounter" : displayName.Trim();
        EncounterTypeLabel = string.IsNullOrWhiteSpace(encounterTypeLabel) ? "Skirmish" : encounterTypeLabel.Trim();
        RoomTypeLabel = string.IsNullOrWhiteSpace(roomTypeLabel) ? "Skirmish Room" : roomTypeLabel.Trim();
        EliteStyleLabel = string.IsNullOrWhiteSpace(eliteStyleLabel) ? string.Empty : eliteStyleLabel.Trim();
        RouteRiskLabel = string.IsNullOrWhiteSpace(routeRiskLabel) ? string.Empty : routeRiskLabel.Trim();
        DangerHintLabel = string.IsNullOrWhiteSpace(dangerHintLabel) ? string.Empty : dangerHintLabel.Trim();
        RewardPreviewHint = string.IsNullOrWhiteSpace(rewardPreviewHint) ? string.Empty : rewardPreviewHint.Trim();
        RewardLabel = string.IsNullOrWhiteSpace(rewardLabel) ? string.Empty : rewardLabel.Trim();
        RewardAmountHint = rewardAmountHint > 0 ? rewardAmountHint : 0;
        IsEliteEncounter = isEliteEncounter;
        EnemyMembers = enemyMembers != null && enemyMembers.Length > 0 ? (PrototypeRpgEncounterMemberDefinition[])enemyMembers.Clone() : System.Array.Empty<PrototypeRpgEncounterMemberDefinition>();
    }
}

public static class PrototypeRpgEncounterCatalog
{
    private static readonly Dictionary<string, PrototypeRpgEncounterDefinition> Definitions = new Dictionary<string, PrototypeRpgEncounterDefinition>
    {
        ["alpha-safe-room1"] = new PrototypeRpgEncounterDefinition("alpha-safe-room1", "encounter-room-1", "Slime Front", "Skirmish", "Skirmish Room", string.Empty, "Safe", "Dungeon Alpha", "mana_shard x4 preview", string.Empty, 4, false, new[] { new PrototypeRpgEncounterMemberDefinition("alpha-safe-room1-slime-bulwark-a", 0), new PrototypeRpgEncounterMemberDefinition("alpha-safe-room1-slime-bulwark-b", 1) }),
        ["alpha-safe-room2"] = new PrototypeRpgEncounterDefinition("alpha-safe-room2", "encounter-room-2", "Watch Hall", "Skirmish", "Skirmish Room", string.Empty, "Safe", "Dungeon Alpha", "mana_shard x4 preview", string.Empty, 4, false, new[] { new PrototypeRpgEncounterMemberDefinition("alpha-safe-room2-slime-bulwark", 0), new PrototypeRpgEncounterMemberDefinition("alpha-safe-room2-slime-skirmisher", 1) }),
        ["alpha-safe-elite"] = new PrototypeRpgEncounterDefinition("alpha-safe-elite", "encounter-room-elite", "Slime Monarch", "Elite", "Elite Chamber", "Royal Slime Elite | Surging Pressure", "Safe", "Dungeon Alpha", "Royal Gel Cache preview", "Royal Gel Cache", 6, true, new[] { new PrototypeRpgEncounterMemberDefinition("alpha-safe-elite", 0, true) }),
        ["alpha-risky-room1"] = new PrototypeRpgEncounterDefinition("alpha-risky-room1", "encounter-room-1", "Mixed Front", "Skirmish", "Skirmish Room", string.Empty, "Risky", "Dungeon Alpha", "mana_shard x5 preview", string.Empty, 5, false, new[] { new PrototypeRpgEncounterMemberDefinition("alpha-risky-room1-slime-bulwark", 0), new PrototypeRpgEncounterMemberDefinition("alpha-risky-room1-goblin-striker", 1) }),
        ["alpha-risky-room2"] = new PrototypeRpgEncounterDefinition("alpha-risky-room2", "encounter-room-2", "Goblin Pair Hall", "Skirmish", "Skirmish Room", string.Empty, "Risky", "Dungeon Alpha", "mana_shard x4 preview", string.Empty, 4, false, new[] { new PrototypeRpgEncounterMemberDefinition("alpha-risky-room2-goblin-skirmisher", 0), new PrototypeRpgEncounterMemberDefinition("alpha-risky-room2-goblin-striker", 1) }),
        ["alpha-risky-elite"] = new PrototypeRpgEncounterDefinition("alpha-risky-elite", "encounter-room-elite", "Gel Core", "Elite", "Elite Chamber", "Volatile Slime Elite | Surging Pressure", "Risky", "Dungeon Alpha", "Volatile Core Cache preview", "Volatile Core Cache", 8, true, new[] { new PrototypeRpgEncounterMemberDefinition("alpha-risky-elite", 0, true) }),
        ["beta-safe-room1"] = new PrototypeRpgEncounterDefinition("beta-safe-room1", "encounter-room-1", "Scout Gate", "Skirmish", "Skirmish Room", string.Empty, "Safe", "Dungeon Beta", "mana_shard x5 preview", string.Empty, 5, false, new[] { new PrototypeRpgEncounterMemberDefinition("beta-safe-room1-slime-bulwark", 0), new PrototypeRpgEncounterMemberDefinition("beta-safe-room1-goblin-skirmisher", 1) }),
        ["beta-safe-room2"] = new PrototypeRpgEncounterDefinition("beta-safe-room2", "encounter-room-2", "Guarded Vault", "Skirmish", "Skirmish Room", string.Empty, "Safe", "Dungeon Beta", "mana_shard x6 preview", string.Empty, 6, false, new[] { new PrototypeRpgEncounterMemberDefinition("beta-safe-room2-goblin-bulwark", 0), new PrototypeRpgEncounterMemberDefinition("beta-safe-room2-goblin-skirmisher", 1) }),
        ["beta-safe-elite"] = new PrototypeRpgEncounterDefinition("beta-safe-elite", "encounter-room-elite", "Goblin Captain", "Elite", "Elite Chamber", "Goblin Elite | Focused Command", "Safe", "Dungeon Beta", "Captain's Stash preview", "Captain's Stash", 9, true, new[] { new PrototypeRpgEncounterMemberDefinition("beta-safe-elite", 0, true) }),
        ["beta-risky-room1"] = new PrototypeRpgEncounterDefinition("beta-risky-room1", "encounter-room-1", "Raider Gate", "Skirmish", "Skirmish Room", string.Empty, "Risky", "Dungeon Beta", "mana_shard x7 preview", string.Empty, 7, false, new[] { new PrototypeRpgEncounterMemberDefinition("beta-risky-room1-goblin-skirmisher", 0), new PrototypeRpgEncounterMemberDefinition("beta-risky-room1-goblin-striker", 1) }),
        ["beta-risky-room2"] = new PrototypeRpgEncounterDefinition("beta-risky-room2", "encounter-room-2", "Ambush Hall", "Skirmish", "Skirmish Room", string.Empty, "Risky", "Dungeon Beta", "mana_shard x7 preview", string.Empty, 7, false, new[] { new PrototypeRpgEncounterMemberDefinition("beta-risky-room2-goblin-bulwark", 0), new PrototypeRpgEncounterMemberDefinition("beta-risky-room2-goblin-striker", 1) }),
        ["beta-risky-elite"] = new PrototypeRpgEncounterDefinition("beta-risky-elite", "encounter-room-elite", "Raider Chief", "Elite", "Elite Chamber", "Goblin Elite | Focused Execution", "Risky", "Dungeon Beta", "Raider War Spoils preview", "Raider War Spoils", 11, true, new[] { new PrototypeRpgEncounterMemberDefinition("beta-risky-elite", 0, true) })
    };

    public static PrototypeRpgEncounterDefinition ResolveDefinition(string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
        {
            return null;
        }

        Definitions.TryGetValue(definitionId.Trim(), out PrototypeRpgEncounterDefinition definition);
        return definition;
    }

    public static PrototypeRpgEncounterDefinition BuildFallbackDefinition(string definitionId, string encounterId, string displayName, string encounterTypeLabel, string roomTypeLabel, string eliteStyleLabel, string routeRiskLabel, string dangerHintLabel, string rewardPreviewHint, string rewardLabel, int rewardAmountHint, bool isEliteEncounter, PrototypeRpgEncounterMemberDefinition[] enemyMembers)
    {
        return new PrototypeRpgEncounterDefinition(definitionId, encounterId, displayName, encounterTypeLabel, roomTypeLabel, eliteStyleLabel, routeRiskLabel, dangerHintLabel, rewardPreviewHint, rewardLabel, rewardAmountHint, isEliteEncounter, enemyMembers);
    }
}