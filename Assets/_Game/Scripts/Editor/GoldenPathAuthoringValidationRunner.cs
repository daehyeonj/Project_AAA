using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class GoldenPathAuthoringValidationRunner
{
    private const string MenuPath = "Tools/Project AAA/Validate Authoring Content";
    private const string SurfacedMatrixMenuPath = "Tools/Project AAA/Show Surfaced Opportunity Matrix";
    private const string SurfacedPortfolioMenuPath = "Tools/Project AAA/Show Surfaced Opportunity Portfolio";
    private const string ExpansionGateMenuPath = "Tools/Project AAA/Show Surfaced Opportunity Expansion Gate";
    private const string RepresentativeStatusMenuPath = "Tools/Project AAA/Show Representative Chain Status";
    private const string SurfacedTraceMenuPath = "Tools/Project AAA/Trace Surfaced Opportunity Resolution";
    private const string ReferenceTraceMenuPath = "Tools/Project AAA/Show Representative Chain Reference Trace";
    private const string QuickOpenMenuPath = "Tools/Project AAA/Quick Open Golden Path Chain Assets";
    private const string QuickOpenSurfacedAssetsMenuPath = "Tools/Project AAA/Quick Open Surfaced Opportunity Assets";

    private sealed class RepresentativeChainValidationProfile
    {
        public string Label;
        public string CityId;
        public string DungeonId;
        public string RouteId;
        public bool ShouldOwnPrimaryCityDungeonLookup;
        public bool ShouldDifferFromPrimaryLookup;
        public bool ShouldSurfaceAsOpportunityVariant;
        public string WhyItMatters;
    }

    private sealed class SurfacedMatrixEntry
    {
        public string Role;
        public string ResolverKey;
        public string CityId;
        public string DungeonId;
        public string RouteId;
        public string SurfaceState;
        public string SourceLabel;
        public string ConsumerSurfaceState;
        public string ConsumerSourceLabel;
        public string PrepLinkState;
        public string RecommendationState;
        public string ConsumerSummaryText;
        public string ConsumerWhyItMattersText;
        public string RecommendedActionSummaryText;
        public string RecommendedActionReasonText;
        public string WhyNowText;
        public string ExpectedUsefulnessText;
        public string RouteMeaningId;
        public string OutcomeMeaningId;
        public string CityDecisionMeaningId;
        public string EncounterProfileId;
        public string BattleSetupId;
        public string SharedSummary;
        public string MissingSummary;
        public string OverrideSummary;
        public string IssueSummary;
        public string StatusLabel;
        public string StatusReasonSummary;
        public string AssetPath;
        public string RouteMeaningAssetPath;
        public string OutcomeMeaningAssetPath;
        public string CityDecisionMeaningAssetPath;
        public string EncounterProfileAssetPath;
        public string BattleSetupAssetPath;
    }

    private sealed class SurfacedConsumerProjection
    {
        public string SurfaceState;
        public string RouteLabel;
        public string ContentSourceLabel;
        public string SummaryText;
        public string WhyItMattersText;
        public string PrepLinkState;
        public string RecommendationState;
        public string RecommendedActionSummaryText;
        public string RecommendedActionReasonText;
        public string WhyNowText;
        public string ExpectedUsefulnessText;
    }

    private sealed class SurfacedMatrixSnapshot
    {
        public List<SurfacedMatrixEntry> Entries = new List<SurfacedMatrixEntry>();
        public Dictionary<string, SurfacedConsumerProjection> ConsumerLookup =
            new Dictionary<string, SurfacedConsumerProjection>(StringComparer.Ordinal);
        public int SurfacedCount;
        public int HiddenCount;
        public int FallbackCount;
        public int ConsumerSurfacedCount;
        public int ConsumerFallbackCount;
        public int ConsumerMismatchCount;
        public int FullyCanonicalSurfacedCount;
        public int CanonicalButNotSurfacedCount;
        public int SurfacedUsingFallbackCount;
        public int SurfacedWithOverrideCount;
        public int SurfacedPrepLinkedCount;
        public int SurfacedRecommendationLinkedCount;
        public int MeaningfullyDistinctSurfacedCount;
    }

    [MenuItem(MenuPath)]
    public static void ValidateAuthoringContentMenu()
    {
        RunValidationAndExitIfNeeded();
    }

    public static void RunAuthoringContentValidation()
    {
        RunValidationAndExitIfNeeded();
    }

    [MenuItem(SurfacedMatrixMenuPath)]
    public static void ShowSurfacedOpportunityMatrixMenu()
    {
        LogSurfacedOpportunityMatrixSummary(false);
    }

    public static void RunSurfacedOpportunityMatrixSummary()
    {
        LogSurfacedOpportunityMatrixSummary(true);
    }

    [MenuItem(SurfacedPortfolioMenuPath)]
    public static void ShowSurfacedOpportunityPortfolioMenu()
    {
        LogSurfacedOpportunityPortfolioSummary(false);
    }

    public static void RunSurfacedOpportunityPortfolioSummary()
    {
        LogSurfacedOpportunityPortfolioSummary(true);
    }

    [MenuItem(ExpansionGateMenuPath)]
    public static void ShowSurfacedOpportunityExpansionGateMenu()
    {
        LogSurfacedOpportunityExpansionGateSummary(false);
    }

    public static void RunSurfacedOpportunityExpansionGateSummary()
    {
        LogSurfacedOpportunityExpansionGateSummary(true);
    }

    public static string BuildSurfacedOpportunityExpansionGateInlineSummary()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        SurfacedMatrixEntry targetEntry = FindEntry(snapshot.Entries, "city-b", "dungeon-beta", "safe");
        return "TargetSurfaceProof=" + BuildTargetEntryProof(targetEntry) +
               " | ExpansionGate=" + BuildExpansionGateStatus(snapshot, targetEntry) +
               " | ExpansionGateWhy=" + BuildExpansionGateReason(snapshot, targetEntry) +
               " | SurfacedPortfolioCount=" + snapshot.SurfacedCount +
               " | MeaningfullyDistinctSurfaced=" + snapshot.MeaningfullyDistinctSurfacedCount +
               " | CanonicalButNotSurfaced=" + FormatCountOrNone(snapshot.CanonicalButNotSurfacedCount) +
               " | SurfacedUsingFallback=" + FormatCountOrNone(snapshot.SurfacedUsingFallbackCount) +
               " | SurfacedConsumerMismatch=" + FormatCountOrNone(snapshot.ConsumerMismatchCount);
    }

    [MenuItem(RepresentativeStatusMenuPath)]
    public static void ShowRepresentativeChainStatusMenu()
    {
        LogRepresentativeChainStatusSummary(false);
    }

    public static void RunRepresentativeChainStatusSummary()
    {
        LogRepresentativeChainStatusSummary(true);
    }

    [MenuItem(SurfacedTraceMenuPath)]
    public static void ShowSurfacedOpportunityResolutionTraceMenu()
    {
        LogSurfacedOpportunityResolutionTraceSummary(false);
    }

    public static void RunSurfacedOpportunityResolutionTrace()
    {
        LogSurfacedOpportunityResolutionTraceSummary(true);
    }

    [MenuItem(ReferenceTraceMenuPath)]
    public static void ShowRepresentativeChainReferenceTraceMenu()
    {
        LogRepresentativeChainReferenceTraceSummary(false);
    }

    public static void RunRepresentativeChainReferenceTrace()
    {
        LogRepresentativeChainReferenceTraceSummary(true);
    }

    [MenuItem(QuickOpenMenuPath)]
    public static void QuickOpenGoldenPathChainAssetsMenu()
    {
        TextAsset[] chainAssets = Resources.LoadAll<TextAsset>("Content/GoldenPathChains");
        Array.Sort(chainAssets, CompareChainAssets);

        if (chainAssets == null || chainAssets.Length < 1)
        {
            Debug.LogWarning("[AuthoringTooling] No golden-path chain assets were found under Resources/Content/GoldenPathChains.");
            return;
        }

        UnityEngine.Object[] selectedObjects = new UnityEngine.Object[chainAssets.Length];
        string[] selectedPaths = new string[chainAssets.Length];
        for (int i = 0; i < chainAssets.Length; i++)
        {
            selectedObjects[i] = chainAssets[i];
            selectedPaths[i] = SafeText(AssetDatabase.GetAssetPath(chainAssets[i]));
        }

        Selection.objects = selectedObjects;
        EditorGUIUtility.PingObject(selectedObjects[0]);
        Debug.Log("[AuthoringTooling] Selected golden-path chain assets: " + string.Join(", ", selectedPaths));
    }

    [MenuItem(QuickOpenSurfacedAssetsMenuPath)]
    public static void QuickOpenSurfacedOpportunityAssetsMenu()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>();
        List<string> selectedPaths = new List<string>();
        HashSet<string> seenPaths = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < snapshot.Entries.Count; i++)
        {
            SurfacedMatrixEntry entry = snapshot.Entries[i];
            if (!IsTrackedRepresentativeOrSurfaced(entry))
            {
                continue;
            }

            AddAssetPathSelection(selectedObjects, selectedPaths, seenPaths, entry.AssetPath);
            AddAssetPathSelection(selectedObjects, selectedPaths, seenPaths, entry.RouteMeaningAssetPath);
            AddAssetPathSelection(selectedObjects, selectedPaths, seenPaths, entry.OutcomeMeaningAssetPath);
            AddAssetPathSelection(selectedObjects, selectedPaths, seenPaths, entry.CityDecisionMeaningAssetPath);
            AddAssetPathSelection(selectedObjects, selectedPaths, seenPaths, entry.EncounterProfileAssetPath);
            AddAssetPathSelection(selectedObjects, selectedPaths, seenPaths, entry.BattleSetupAssetPath);
        }

        if (selectedObjects.Count < 1)
        {
            Debug.LogWarning("[AuthoringTooling] No surfaced or representative opportunity assets were resolved for quick-open.");
            return;
        }

        Selection.objects = selectedObjects.ToArray();
        EditorGUIUtility.PingObject(selectedObjects[0]);
        Debug.Log("[AuthoringTooling] Selected surfaced opportunity assets: " + string.Join(", ", selectedPaths.ToArray()));
    }

    private static void RunValidationAndExitIfNeeded()
    {
        int exitCode = RunValidation();
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(exitCode);
        }
    }

    private static int RunValidation()
    {
        List<string> passMessages = new List<string>();
        List<string> warnMessages = new List<string>();
        List<string> failMessages = new List<string>();
        SortedSet<string> sharedDefinitionsUsed = new SortedSet<string>(StringComparer.Ordinal);

        RepresentativeChainValidationProfile[] profiles = BuildRepresentativeProfiles();
        for (int i = 0; i < profiles.Length; i++)
        {
            ValidateRepresentativeChain(profiles[i], passMessages, warnMessages, failMessages, sharedDefinitionsUsed);
        }

        ValidateSupportedSafeVariant(passMessages, failMessages, sharedDefinitionsUsed);

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringValidation] Summary");
        summary.AppendLine("- ProfilesChecked=" + profiles.Length);
        summary.AppendLine("- SupportedVariantsChecked=1");
        summary.AppendLine("- PassCount=" + passMessages.Count);
        summary.AppendLine("- WarnCount=" + warnMessages.Count);
        summary.AppendLine("- FailCount=" + failMessages.Count);
        summary.AppendLine("- SharedDefinitionsUsed=" + (sharedDefinitionsUsed.Count > 0 ? string.Join(", ", ToArray(sharedDefinitionsUsed)) : "None"));

        AppendMessages(summary, "PASS", passMessages);
        AppendMessages(summary, "WARN", warnMessages);
        AppendMessages(summary, "FAIL", failMessages);

        if (failMessages.Count > 0)
        {
            Debug.LogError(summary.ToString());
            return 1;
        }

        if (warnMessages.Count > 0)
        {
            Debug.LogWarning(summary.ToString());
        }
        else
        {
            Debug.Log(summary.ToString());
        }

        return 0;
    }

    private static void LogSurfacedOpportunityMatrixSummary(bool exitWhenBatchCompletes)
    {
        string summary = BuildSurfacedOpportunityMatrixSummary();
        Debug.Log(summary);

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void LogSurfacedOpportunityPortfolioSummary(bool exitWhenBatchCompletes)
    {
        string summary = BuildSurfacedOpportunityPortfolioSummary();
        Debug.Log(summary);

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void LogRepresentativeChainStatusSummary(bool exitWhenBatchCompletes)
    {
        string summary = BuildRepresentativeChainStatusSummary();
        Debug.Log(summary);

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void LogSurfacedOpportunityExpansionGateSummary(bool exitWhenBatchCompletes)
    {
        string summary = BuildSurfacedOpportunityExpansionGateSummary();
        Debug.Log(summary);

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void LogSurfacedOpportunityResolutionTraceSummary(bool exitWhenBatchCompletes)
    {
        string summary = BuildSurfacedOpportunityResolutionTraceSummary();
        Debug.Log(summary);

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void LogRepresentativeChainReferenceTraceSummary(bool exitWhenBatchCompletes)
    {
        string summary = BuildRepresentativeChainReferenceTraceSummary();
        Debug.Log(summary);

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static string BuildSurfacedOpportunityMatrixSummary()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        List<SurfacedMatrixEntry> entries = snapshot.Entries;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Surfaced opportunity matrix");
        summary.AppendLine("- AuthoredChains=" + entries.Count);
        summary.AppendLine("- SurfacedCount=" + snapshot.SurfacedCount);
        summary.AppendLine("- HiddenCanonicalCount=" + snapshot.HiddenCount);
        summary.AppendLine("- FallbackCount=" + snapshot.FallbackCount);
        summary.AppendLine("- ConsumerSurfacedCount=" + snapshot.ConsumerSurfacedCount);
        summary.AppendLine("- ConsumerFallbackCount=" + snapshot.ConsumerFallbackCount);
        summary.AppendLine("- SurfacedConsumerMismatch=" + (snapshot.ConsumerMismatchCount > 0 ? snapshot.ConsumerMismatchCount.ToString() : "None"));
        summary.AppendLine("- RepresentativeProfiles=" + BuildRepresentativeProfiles().Length);
        summary.AppendLine("- FullyCanonicalAndSurfaced=" + snapshot.FullyCanonicalSurfacedCount);
        summary.AppendLine("- CanonicalButNotSurfaced=" + (snapshot.CanonicalButNotSurfacedCount > 0 ? snapshot.CanonicalButNotSurfacedCount.ToString() : "None"));
        summary.AppendLine("- SurfacedUsingFallback=" + (snapshot.SurfacedUsingFallbackCount > 0 ? snapshot.SurfacedUsingFallbackCount.ToString() : "None"));
        summary.AppendLine("- SurfacedWithChainOverride=" + (snapshot.SurfacedWithOverrideCount > 0 ? snapshot.SurfacedWithOverrideCount.ToString() : "None"));
        summary.AppendLine("- SurfacedPrepLinked=" + snapshot.SurfacedPrepLinkedCount);
        summary.AppendLine("- SurfacedRecommendationLinked=" + snapshot.SurfacedRecommendationLinkedCount);
        summary.AppendLine("- MeaningfullyDistinctSurfaced=" + snapshot.MeaningfullyDistinctSurfacedCount);
        summary.AppendLine("- TargetSurfaceProof=" + BuildTargetSurfaceProof(snapshot.ConsumerLookup, "city-b", "dungeon-beta", "safe"));

        for (int i = 0; i < entries.Count; i++)
        {
            SurfacedMatrixEntry entry = entries[i];
            summary.AppendLine(
                "- " + SafeText(entry.Role) +
                " | " + SafeText(entry.CityId) + " -> " + SafeText(entry.DungeonId) + " -> " + SafeText(entry.RouteId) +
                " | Status=" + SafeText(entry.StatusLabel) +
                " | StatusWhy=" + SafeText(entry.StatusReasonSummary) +
                " | Surface=" + SafeText(entry.SurfaceState) +
                " | Source=" + SafeText(entry.SourceLabel) +
                " | Consumer=" + SafeText(entry.ConsumerSurfaceState) +
                " | ConsumerSource=" + SafeText(entry.ConsumerSourceLabel) +
                " | Prep=" + SafeText(entry.PrepLinkState) +
                " | Recommendation=" + SafeText(entry.RecommendationState) +
                " | Shared=" + SafeText(entry.SharedSummary) +
                " | Missing=" + SafeText(entry.MissingSummary) +
                " | Overrides=" + SafeText(entry.OverrideSummary) +
                " | Issues=" + SafeText(entry.IssueSummary) +
                " | Asset=" + SafeText(entry.AssetPath));
        }

        return summary.ToString();
    }

    private static string BuildSurfacedOpportunityPortfolioSummary()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        List<SurfacedMatrixEntry> surfacedEntries = new List<SurfacedMatrixEntry>();

        for (int i = 0; i < snapshot.Entries.Count; i++)
        {
            SurfacedMatrixEntry entry = snapshot.Entries[i];
            if (entry != null &&
                string.Equals(entry.ConsumerSurfaceState, "cityhub+prep(actual)", StringComparison.Ordinal))
            {
                surfacedEntries.Add(entry);
            }
        }

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Surfaced opportunity portfolio");
        summary.AppendLine("- SurfacedPortfolioCount=" + surfacedEntries.Count);
        summary.AppendLine("- OfficialSurfacedSet=" + ((snapshot.CanonicalButNotSurfacedCount == 0 &&
                                                      snapshot.SurfacedUsingFallbackCount == 0 &&
                                                      snapshot.ConsumerMismatchCount == 0)
            ? "Yes"
            : "No"));
        summary.AppendLine("- CanonicalButNotSurfaced=" + (snapshot.CanonicalButNotSurfacedCount > 0 ? snapshot.CanonicalButNotSurfacedCount.ToString() : "None"));
        summary.AppendLine("- SurfacedUsingFallback=" + (snapshot.SurfacedUsingFallbackCount > 0 ? snapshot.SurfacedUsingFallbackCount.ToString() : "None"));
        summary.AppendLine("- SurfacedConsumerMismatch=" + (snapshot.ConsumerMismatchCount > 0 ? snapshot.ConsumerMismatchCount.ToString() : "None"));
        summary.AppendLine("- SurfacedPrepLinked=" + snapshot.SurfacedPrepLinkedCount);
        summary.AppendLine("- SurfacedRecommendationLinked=" + snapshot.SurfacedRecommendationLinkedCount);
        summary.AppendLine("- MeaningfullyDistinctSurfaced=" + snapshot.MeaningfullyDistinctSurfacedCount);
        summary.AppendLine("- TargetSurfaceProof=" + BuildTargetEntryProof(FindEntry(snapshot.Entries, "city-b", "dungeon-beta", "safe")));
        summary.AppendLine("- ExpansionGate=" + BuildExpansionGateStatus(snapshot, FindEntry(snapshot.Entries, "city-b", "dungeon-beta", "safe")));
        summary.AppendLine("- ExpansionGateWhy=" + BuildExpansionGateReason(snapshot, FindEntry(snapshot.Entries, "city-b", "dungeon-beta", "safe")));

        for (int i = 0; i < surfacedEntries.Count; i++)
        {
            SurfacedMatrixEntry entry = surfacedEntries[i];
            summary.AppendLine(
                "- " + SafeText(entry.CityId) +
                " -> " + SafeText(entry.DungeonId) +
                " -> " + SafeText(entry.RouteId) +
                " | Status=" + SafeText(entry.StatusLabel) +
                " | Source=" + SafeText(entry.SourceLabel) +
                " | Prep=" + SafeText(entry.PrepLinkState) +
                " | Recommendation=" + SafeText(entry.RecommendationState) +
                " | Shared=" + SafeText(entry.SharedSummary) +
                " | Overrides=" + SafeText(entry.OverrideSummary) +
                " | Issues=" + SafeText(entry.IssueSummary) +
                " | Why=" + SafeText(entry.StatusReasonSummary));
        }

        return summary.ToString();
    }

    private static string BuildSurfacedOpportunityExpansionGateSummary()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        List<SurfacedMatrixEntry> surfacedEntries = new List<SurfacedMatrixEntry>();

        for (int i = 0; i < snapshot.Entries.Count; i++)
        {
            SurfacedMatrixEntry entry = snapshot.Entries[i];
            if (entry != null &&
                string.Equals(entry.ConsumerSurfaceState, "cityhub+prep(actual)", StringComparison.Ordinal))
            {
                surfacedEntries.Add(entry);
            }
        }

        SurfacedMatrixEntry targetEntry = FindEntry(snapshot.Entries, "city-b", "dungeon-beta", "safe");

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Surfaced opportunity expansion gate");
        summary.AppendLine("- TargetResolverKey=city-b::dungeon-beta::safe");
        summary.AppendLine("- TargetSurfaceProof=" + BuildTargetEntryProof(targetEntry));
        summary.AppendLine("- SurfacedPortfolioCount=" + surfacedEntries.Count);
        summary.AppendLine("- MeaningfullyDistinctSurfaced=" + snapshot.MeaningfullyDistinctSurfacedCount);
        summary.AppendLine("- CanonicalButNotSurfaced=" + FormatCountOrNone(snapshot.CanonicalButNotSurfacedCount));
        summary.AppendLine("- SurfacedUsingFallback=" + FormatCountOrNone(snapshot.SurfacedUsingFallbackCount));
        summary.AppendLine("- SurfacedConsumerMismatch=" + FormatCountOrNone(snapshot.ConsumerMismatchCount));
        summary.AppendLine("- SurfacedPrepLinked=" + snapshot.SurfacedPrepLinkedCount);
        summary.AppendLine("- SurfacedRecommendationLinked=" + snapshot.SurfacedRecommendationLinkedCount);
        summary.AppendLine("- ExpansionGate=" + BuildExpansionGateStatus(snapshot, targetEntry));
        summary.AppendLine("- ExpansionGateWhy=" + BuildExpansionGateReason(snapshot, targetEntry));

        for (int i = 0; i < surfacedEntries.Count; i++)
        {
            SurfacedMatrixEntry entry = surfacedEntries[i];
            summary.AppendLine(
                "- " + SafeText(entry.CityId) +
                " -> " + SafeText(entry.DungeonId) +
                " -> " + SafeText(entry.RouteId) +
                " | Status=" + SafeText(entry.StatusLabel) +
                " | Surface=" + SafeText(entry.SurfaceState) +
                " | Consumer=" + SafeText(entry.ConsumerSurfaceState) +
                " | Prep=" + SafeText(entry.PrepLinkState) +
                " | Recommendation=" + SafeText(entry.RecommendationState) +
                " | Issues=" + SafeText(entry.IssueSummary));
        }

        return summary.ToString();
    }

    private static string BuildRepresentativeChainStatusSummary()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        List<SurfacedMatrixEntry> trackedEntries = new List<SurfacedMatrixEntry>();

        for (int i = 0; i < snapshot.Entries.Count; i++)
        {
            SurfacedMatrixEntry entry = snapshot.Entries[i];
            bool isRepresentative = entry.Role.StartsWith("Representative chain", StringComparison.Ordinal);
            bool isSurfacedVariant = entry.Role == "Surfaced safe variant" ||
                                     entry.SurfaceState.StartsWith("cityhub+prep", StringComparison.Ordinal);
            if (isRepresentative || isSurfacedVariant)
            {
                trackedEntries.Add(entry);
            }
        }

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Representative chain status");
        summary.AppendLine("- TrackedEntries=" + trackedEntries.Count);
        summary.AppendLine("- FullyCanonicalAndSurfaced=" + snapshot.FullyCanonicalSurfacedCount);
        summary.AppendLine("- CanonicalButNotSurfaced=" + (snapshot.CanonicalButNotSurfacedCount > 0 ? snapshot.CanonicalButNotSurfacedCount.ToString() : "None"));
        summary.AppendLine("- SurfacedUsingFallback=" + (snapshot.SurfacedUsingFallbackCount > 0 ? snapshot.SurfacedUsingFallbackCount.ToString() : "None"));
        summary.AppendLine("- SurfacedWithChainOverride=" + (snapshot.SurfacedWithOverrideCount > 0 ? snapshot.SurfacedWithOverrideCount.ToString() : "None"));
        summary.AppendLine("- SurfacedConsumerMismatch=" + (snapshot.ConsumerMismatchCount > 0 ? snapshot.ConsumerMismatchCount.ToString() : "None"));
        summary.AppendLine("- MeaningfullyDistinctSurfaced=" + snapshot.MeaningfullyDistinctSurfacedCount);

        for (int i = 0; i < trackedEntries.Count; i++)
        {
            SurfacedMatrixEntry entry = trackedEntries[i];
            summary.AppendLine(
                "- " + SafeText(entry.Role) +
                " | ResolverKey=" + BuildResolverKey(entry.CityId, entry.DungeonId, entry.RouteId) +
                " | Status=" + SafeText(entry.StatusLabel) +
                " | Surface=" + SafeText(entry.SurfaceState) +
                " | Consumer=" + SafeText(entry.ConsumerSurfaceState) +
                " | Source=" + SafeText(entry.SourceLabel) +
                " | ConsumerSource=" + SafeText(entry.ConsumerSourceLabel) +
                " | Prep=" + SafeText(entry.PrepLinkState) +
                " | Recommendation=" + SafeText(entry.RecommendationState) +
                " | Shared=" + SafeText(entry.SharedSummary) +
                " | Overrides=" + SafeText(entry.OverrideSummary) +
                " | Issues=" + SafeText(entry.IssueSummary) +
                " | Why=" + SafeText(entry.StatusReasonSummary));
        }

        return summary.ToString();
    }

    private static string BuildSurfacedOpportunityResolutionTraceSummary()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        List<SurfacedMatrixEntry> surfacedEntries = new List<SurfacedMatrixEntry>();

        for (int i = 0; i < snapshot.Entries.Count; i++)
        {
            SurfacedMatrixEntry entry = snapshot.Entries[i];
            if (entry != null &&
                string.Equals(entry.ConsumerSurfaceState, "cityhub+prep(actual)", StringComparison.Ordinal))
            {
                surfacedEntries.Add(entry);
            }
        }

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Surfaced opportunity resolution trace");
        summary.AppendLine("- SurfacedPortfolioCount=" + surfacedEntries.Count);
        summary.AppendLine("- MeaningfullyDistinctSurfaced=" + snapshot.MeaningfullyDistinctSurfacedCount);
        summary.AppendLine("- CanonicalButNotSurfaced=" + (snapshot.CanonicalButNotSurfacedCount > 0 ? snapshot.CanonicalButNotSurfacedCount.ToString() : "None"));
        summary.AppendLine("- SurfacedUsingFallback=" + (snapshot.SurfacedUsingFallbackCount > 0 ? snapshot.SurfacedUsingFallbackCount.ToString() : "None"));
        summary.AppendLine("- SurfacedConsumerMismatch=" + (snapshot.ConsumerMismatchCount > 0 ? snapshot.ConsumerMismatchCount.ToString() : "None"));

        for (int i = 0; i < surfacedEntries.Count; i++)
        {
            SurfacedMatrixEntry entry = surfacedEntries[i];
            summary.AppendLine(
                "- " + SafeText(entry.Role) +
                " | ResolverKey=" + SafeText(entry.ResolverKey) +
                " | Status=" + SafeText(entry.StatusLabel) +
                " | Distinctness=" + BuildDistinctnessSummary(entry, surfacedEntries) +
                " | Issues=" + SafeText(entry.IssueSummary) +
                " | Source=" + SafeText(entry.SourceLabel) +
                " | Consumer=" + SafeText(entry.ConsumerSurfaceState) +
                " | Prep=" + SafeText(entry.PrepLinkState) +
                " | Recommendation=" + SafeText(entry.RecommendationState));
            summary.AppendLine("  CityHubSummary=" + SafeText(entry.ConsumerSummaryText));
            summary.AppendLine("  CityHubWhy=" + SafeText(entry.ConsumerWhyItMattersText));
            summary.AppendLine("  PrepRecommendation=" + SafeText(entry.RecommendedActionSummaryText));
            summary.AppendLine("  PrepReason=" + SafeText(entry.RecommendedActionReasonText));
            summary.AppendLine("  PrepWhyNow=" + SafeText(entry.WhyNowText));
            summary.AppendLine("  PrepUsefulness=" + SafeText(entry.ExpectedUsefulnessText));
            summary.AppendLine(
                "  SharedRefs=" +
                "route:" + SafeText(entry.RouteMeaningId) +
                ", outcome:" + SafeText(entry.OutcomeMeaningId) +
                ", city:" + SafeText(entry.CityDecisionMeaningId) +
                ", encounter:" + SafeText(entry.EncounterProfileId) +
                ", battle:" + SafeText(entry.BattleSetupId));
            summary.AppendLine("  SharedAssetPaths=" + BuildSharedAssetReferenceSummary(entry));
            summary.AppendLine("  Asset=" + SafeText(entry.AssetPath));
        }

        return summary.ToString();
    }

    private static string BuildRepresentativeChainReferenceTraceSummary()
    {
        SurfacedMatrixSnapshot snapshot = BuildSurfacedMatrixSnapshot();
        List<SurfacedMatrixEntry> trackedEntries = new List<SurfacedMatrixEntry>();
        List<SurfacedMatrixEntry> surfacedEntries = new List<SurfacedMatrixEntry>();

        for (int i = 0; i < snapshot.Entries.Count; i++)
        {
            SurfacedMatrixEntry entry = snapshot.Entries[i];
            if (IsTrackedRepresentativeOrSurfaced(entry))
            {
                trackedEntries.Add(entry);
            }

            if (entry != null &&
                string.Equals(entry.ConsumerSurfaceState, "cityhub+prep(actual)", StringComparison.Ordinal))
            {
                surfacedEntries.Add(entry);
            }
        }

        int issueCount = 0;
        for (int i = 0; i < trackedEntries.Count; i++)
        {
            if (!string.Equals(trackedEntries[i].IssueSummary, "clean", StringComparison.Ordinal))
            {
                issueCount++;
            }
        }

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Representative chain reference trace");
        summary.AppendLine("- TrackedEntries=" + trackedEntries.Count);
        summary.AppendLine("- SurfacedTrackedEntries=" + surfacedEntries.Count);
        summary.AppendLine("- EntriesWithIssues=" + (issueCount > 0 ? issueCount.ToString() : "None"));
        summary.AppendLine("- CanonicalButNotSurfaced=" + (snapshot.CanonicalButNotSurfacedCount > 0 ? snapshot.CanonicalButNotSurfacedCount.ToString() : "None"));
        summary.AppendLine("- SurfacedUsingFallback=" + (snapshot.SurfacedUsingFallbackCount > 0 ? snapshot.SurfacedUsingFallbackCount.ToString() : "None"));
        summary.AppendLine("- SurfacedWithChainOverride=" + (snapshot.SurfacedWithOverrideCount > 0 ? snapshot.SurfacedWithOverrideCount.ToString() : "None"));

        for (int i = 0; i < trackedEntries.Count; i++)
        {
            SurfacedMatrixEntry entry = trackedEntries[i];
            bool consumerSurfaced = string.Equals(entry.ConsumerSurfaceState, "cityhub+prep(actual)", StringComparison.Ordinal);
            summary.AppendLine(
                "- " + SafeText(entry.Role) +
                " | ResolverKey=" + SafeText(entry.ResolverKey) +
                " | Status=" + SafeText(entry.StatusLabel) +
                " | Surface=" + SafeText(entry.SurfaceState) +
                " | Consumer=" + SafeText(entry.ConsumerSurfaceState) +
                " | Distinctness=" + (consumerSurfaced ? BuildDistinctnessSummary(entry, surfacedEntries) : "not-surfaced") +
                " | Issues=" + SafeText(entry.IssueSummary));
            summary.AppendLine("  ChainAsset=" + SafeText(entry.AssetPath));
            summary.AppendLine("  SharedAssets=" + BuildSharedAssetReferenceSummary(entry));
        }

        return summary.ToString();
    }

    private static SurfacedMatrixSnapshot BuildSurfacedMatrixSnapshot()
    {
        SurfacedMatrixSnapshot snapshot = new SurfacedMatrixSnapshot();
        TextAsset[] chainAssets = Resources.LoadAll<TextAsset>("Content/GoldenPathChains");
        Array.Sort(chainAssets, CompareChainAssets);
        snapshot.ConsumerLookup = BuildSurfacedConsumerLookup();

        for (int i = 0; i < chainAssets.Length; i++)
        {
            TextAsset asset = chainAssets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            GoldenPathChainDefinition definition = JsonUtility.FromJson<GoldenPathChainDefinition>(asset.text);
            if (definition == null || !HasText(definition.CityId) || !HasText(definition.DungeonId))
            {
                continue;
            }

            GoldenPathRouteDefinition routeDefinition = definition.CanonicalRoute;
            string routeId = routeDefinition != null ? routeDefinition.RouteId : string.Empty;
            string sourceLabel = GoldenPathContentRegistry.BuildContentSourceLabel(definition.CityId, definition.DungeonId, routeId);
            bool isCanonical = sourceLabel.StartsWith("data:", StringComparison.Ordinal);
            bool isSurfacedPrimary = definition.PrimaryCityDungeonDefinition;
            bool isSurfacedVariant = definition.SurfaceAsOpportunityVariant;
            string surfaceState = BuildAuthoredSurfaceState(isSurfacedPrimary, isSurfacedVariant);
            string resolverKey = BuildResolverKey(definition.CityId, definition.DungeonId, routeId);
            snapshot.ConsumerLookup.TryGetValue(resolverKey, out SurfacedConsumerProjection consumerProjection);
            bool authoredSurfaced = surfaceState.StartsWith("cityhub+prep", StringComparison.Ordinal);
            bool consumerSurfaced = consumerProjection != null &&
                                    consumerProjection.SurfaceState.StartsWith("cityhub+prep", StringComparison.Ordinal);
            bool consumerUsesFallback = consumerProjection != null &&
                                        HasText(consumerProjection.ContentSourceLabel) &&
                                        !consumerProjection.ContentSourceLabel.StartsWith("data:", StringComparison.Ordinal);
            bool prepLinked = consumerProjection != null &&
                              string.Equals(consumerProjection.PrepLinkState, "linked(actual)", StringComparison.Ordinal);
            bool recommendationLinked = consumerProjection != null &&
                                        string.Equals(consumerProjection.RecommendationState, "connected(actual)", StringComparison.Ordinal);

            GoldenPathRouteMeaningDefinition routeMeaning = null;
            GoldenPathOutcomeMeaningDefinition outcomeMeaning = null;
            GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning = null;
            GoldenPathEncounterProfileDefinition encounterProfile = null;
            GoldenPathBattleSetupDefinition battleSetup = null;
            GoldenPathRoomDefinition representativeRoom = null;

            bool representativeRoomLoaded = HasText(routeId) &&
                                            GoldenPathContentRegistry.TryGetRepresentativeEncounterRoom(
                                                definition.CityId,
                                                definition.DungeonId,
                                                routeId,
                                                out GoldenPathChainDefinition _,
                                                out GoldenPathRouteDefinition _,
                                                out representativeRoom);
            bool routeMeaningLoaded = routeDefinition != null &&
                                      HasText(routeDefinition.RouteMeaningId) &&
                                      GoldenPathContentRegistry.TryGetRouteMeaning(routeDefinition.RouteMeaningId, out routeMeaning);
            bool outcomeMeaningLoaded = HasText(definition.OutcomeMeaningId) &&
                                        GoldenPathContentRegistry.TryGetOutcomeMeaning(definition.OutcomeMeaningId, out outcomeMeaning);
            bool cityDecisionMeaningLoaded = HasText(definition.CityDecisionMeaningId) &&
                                             GoldenPathContentRegistry.TryGetCityDecisionMeaning(definition.CityDecisionMeaningId, out cityDecisionMeaning);
            bool encounterProfileLoaded = representativeRoomLoaded &&
                                          representativeRoom != null &&
                                          HasText(representativeRoom.EncounterProfileId) &&
                                          GoldenPathContentRegistry.TryGetEncounterProfile(representativeRoom.EncounterProfileId, out encounterProfile);
            bool battleSetupLoaded = representativeRoomLoaded &&
                                     representativeRoom != null &&
                                     HasText(representativeRoom.BattleSetupId) &&
                                     GoldenPathContentRegistry.TryGetBattleSetup(representativeRoom.BattleSetupId, out battleSetup);
            string routeMeaningAssetPath = ResolveRouteMeaningAssetPath(routeDefinition != null ? routeDefinition.RouteMeaningId : string.Empty);
            string outcomeMeaningAssetPath = ResolveOutcomeMeaningAssetPath(definition.OutcomeMeaningId);
            string cityDecisionMeaningAssetPath = ResolveCityDecisionMeaningAssetPath(definition.CityDecisionMeaningId);
            string encounterProfileAssetPath = ResolveEncounterProfileAssetPath(representativeRoom != null ? representativeRoom.EncounterProfileId : string.Empty);
            string battleSetupAssetPath = ResolveBattleSetupAssetPath(representativeRoom != null ? representativeRoom.BattleSetupId : string.Empty);

            List<string> missing = new List<string>();
            if (!isCanonical)
            {
                missing.Add("fallback:hardcoded");
            }

            if (!routeMeaningLoaded)
            {
                missing.Add("shared-route-meaning");
            }

            if (!outcomeMeaningLoaded)
            {
                missing.Add("shared-outcome-meaning");
            }

            if (!cityDecisionMeaningLoaded)
            {
                missing.Add("shared-city-decision-meaning");
            }

            if (!representativeRoomLoaded || representativeRoom == null)
            {
                missing.Add("representative-room");
            }
            else
            {
                if (!encounterProfileLoaded)
                {
                    missing.Add("shared-encounter-profile");
                }

                if (!battleSetupLoaded)
                {
                    missing.Add("shared-battle-setup");
                }
            }

            if (authoredSurfaced)
            {
                snapshot.SurfacedCount++;
            }
            else
            {
                snapshot.HiddenCount++;
            }

            if (!isCanonical)
            {
                snapshot.FallbackCount++;
            }

            if (consumerSurfaced)
            {
                snapshot.ConsumerSurfacedCount++;
            }

            if (consumerUsesFallback)
            {
                snapshot.ConsumerFallbackCount++;
            }

            if (authoredSurfaced != consumerSurfaced)
            {
                snapshot.ConsumerMismatchCount++;
            }

            string overrideSummary = BuildOverrideSummary(definition);
            bool meaningfulOverride = HasMeaningfulOverride(overrideSummary);
            if (isCanonical && authoredSurfaced && consumerSurfaced && !consumerUsesFallback && !meaningfulOverride)
            {
                snapshot.FullyCanonicalSurfacedCount++;
            }

            if (isCanonical && !authoredSurfaced)
            {
                snapshot.CanonicalButNotSurfacedCount++;
            }

            if (authoredSurfaced && !isCanonical)
            {
                snapshot.SurfacedUsingFallbackCount++;
            }

            if (authoredSurfaced && meaningfulOverride)
            {
                snapshot.SurfacedWithOverrideCount++;
            }

            if (authoredSurfaced && prepLinked)
            {
                snapshot.SurfacedPrepLinkedCount++;
            }

            if (authoredSurfaced && recommendationLinked)
            {
                snapshot.SurfacedRecommendationLinkedCount++;
            }

            snapshot.Entries.Add(new SurfacedMatrixEntry
            {
                Role = ResolveChainRole(definition.CityId, definition.DungeonId, routeId),
                ResolverKey = resolverKey,
                CityId = definition.CityId,
                DungeonId = definition.DungeonId,
                RouteId = routeId,
                SurfaceState = surfaceState,
                SourceLabel = sourceLabel,
                ConsumerSurfaceState = consumerProjection != null ? consumerProjection.SurfaceState : "not-visible(actual)",
                ConsumerSourceLabel = consumerProjection != null ? consumerProjection.ContentSourceLabel : "None",
                PrepLinkState = consumerProjection != null ? consumerProjection.PrepLinkState : "missing(actual)",
                RecommendationState = consumerProjection != null ? consumerProjection.RecommendationState : "missing(actual)",
                ConsumerSummaryText = consumerProjection != null ? consumerProjection.SummaryText : "None",
                ConsumerWhyItMattersText = consumerProjection != null ? consumerProjection.WhyItMattersText : "None",
                RecommendedActionSummaryText = consumerProjection != null ? consumerProjection.RecommendedActionSummaryText : "None",
                RecommendedActionReasonText = consumerProjection != null ? consumerProjection.RecommendedActionReasonText : "None",
                WhyNowText = consumerProjection != null ? consumerProjection.WhyNowText : "None",
                ExpectedUsefulnessText = consumerProjection != null ? consumerProjection.ExpectedUsefulnessText : "None",
                RouteMeaningId = routeMeaning != null ? routeMeaning.RouteMeaningId : "None",
                OutcomeMeaningId = outcomeMeaning != null ? outcomeMeaning.OutcomeMeaningId : "None",
                CityDecisionMeaningId = cityDecisionMeaning != null ? cityDecisionMeaning.CityDecisionMeaningId : "None",
                EncounterProfileId = encounterProfile != null ? encounterProfile.EncounterProfileId : "None",
                BattleSetupId = battleSetup != null ? battleSetup.BattleSetupId : "None",
                RouteMeaningAssetPath = routeMeaningAssetPath,
                OutcomeMeaningAssetPath = outcomeMeaningAssetPath,
                CityDecisionMeaningAssetPath = cityDecisionMeaningAssetPath,
                EncounterProfileAssetPath = encounterProfileAssetPath,
                BattleSetupAssetPath = battleSetupAssetPath,
                SharedSummary = BuildSharedSummary(routeMeaning, outcomeMeaning, cityDecisionMeaning, encounterProfile, battleSetup),
                MissingSummary = missing.Count > 0 ? string.Join(", ", missing.ToArray()) : "None",
                OverrideSummary = overrideSummary,
                IssueSummary = BuildIssueSummary(
                    isCanonical,
                    authoredSurfaced,
                    consumerSurfaced,
                    consumerUsesFallback,
                    meaningfulOverride,
                    prepLinked,
                    recommendationLinked,
                    missing,
                    overrideSummary),
                StatusLabel = BuildSurfacedMatrixStatusLabel(isCanonical, authoredSurfaced, consumerSurfaced, consumerUsesFallback, meaningfulOverride),
                StatusReasonSummary = BuildSurfacedMatrixStatusReasonSummary(
                    isCanonical,
                    authoredSurfaced,
                    consumerSurfaced,
                    consumerUsesFallback,
                    meaningfulOverride,
                    prepLinked,
                    recommendationLinked,
                    missing),
                AssetPath = SafeText(AssetDatabase.GetAssetPath(asset))
            });
        }

        snapshot.MeaningfullyDistinctSurfacedCount = CountMeaningfullyDistinctSurfacedEntries(snapshot.Entries);

        return snapshot;
    }

    private static Dictionary<string, SurfacedConsumerProjection> BuildSurfacedConsumerLookup()
    {
        Dictionary<string, SurfacedConsumerProjection> lookup = new Dictionary<string, SurfacedConsumerProjection>(StringComparer.Ordinal);
        StaticPlaceholderWorldView worldView = new StaticPlaceholderWorldView(PlaceholderResourceDataFactory.Create());
        WorldBoardReadModel board = worldView.BuildWorldBoardReadModel();
        CityStatusReadModel[] cities = board != null && board.Cities != null ? board.Cities : Array.Empty<CityStatusReadModel>();
        Dictionary<string, ExpeditionPrepReadModel> prepLookup = BuildPrepReadModelLookup(board);

        for (int cityIndex = 0; cityIndex < cities.Length; cityIndex++)
        {
            CityStatusReadModel city = cities[cityIndex];
            if (city == null || !HasText(city.CityId))
            {
                continue;
            }

            CityDecisionReadModel decision = city.Decision ?? CityDecisionModelBuilder.Build(board ?? WorldBoardReadModel.Empty, city);
            CityOpportunitySignal[] opportunities = decision != null && decision.Opportunities != null
                ? decision.Opportunities
                : Array.Empty<CityOpportunitySignal>();

            for (int opportunityIndex = 0; opportunityIndex < opportunities.Length; opportunityIndex++)
            {
                CityOpportunitySignal opportunity = opportunities[opportunityIndex];
                if (opportunity == null || !HasText(opportunity.DungeonId) || !HasText(opportunity.RouteId))
                {
                    continue;
                }

                ExpeditionPrepReadModel prepReadModel = null;
                prepLookup.TryGetValue(BuildCityDungeonKey(city.CityId, opportunity.DungeonId), out prepReadModel);
                CityOpportunitySignal prepOpportunity = FindOpportunity(prepReadModel != null ? prepReadModel.LinkedOpportunities : null, opportunity.DungeonId, opportunity.RouteId);
                bool prepLinked = prepOpportunity != null &&
                                  HasText(prepOpportunity.ContentSourceLabel) &&
                                  prepOpportunity.ContentSourceLabel.StartsWith("data:", StringComparison.Ordinal);
                bool recommendationLinked = prepLinked &&
                                            HasText(prepReadModel != null ? prepReadModel.RecommendedActionSummaryText : string.Empty) &&
                                            HasText(prepReadModel != null ? prepReadModel.RecommendedActionReasonText : string.Empty) &&
                                            (HasText(prepReadModel != null ? prepReadModel.WhyNowText : string.Empty) ||
                                             HasText(prepReadModel != null ? prepReadModel.ExpectedUsefulnessText : string.Empty) ||
                                             HasText(prepOpportunity != null ? prepOpportunity.WhyItMattersText : string.Empty));

                lookup[BuildResolverKey(city.CityId, opportunity.DungeonId, opportunity.RouteId)] = new SurfacedConsumerProjection
                {
                    SurfaceState = BuildConsumerSurfaceState(opportunity, prepLinked),
                    RouteLabel = SafeText(opportunity.RouteLabel),
                    ContentSourceLabel = SafeText(prepOpportunity != null ? prepOpportunity.ContentSourceLabel : opportunity.ContentSourceLabel),
                    SummaryText = SafeText(prepOpportunity != null ? prepOpportunity.SummaryText : opportunity.SummaryText),
                    WhyItMattersText = SafeText(prepOpportunity != null ? prepOpportunity.WhyItMattersText : opportunity.WhyItMattersText),
                    PrepLinkState = prepLinked ? "linked(actual)" : "missing(actual)",
                    RecommendationState = recommendationLinked ? "connected(actual)" : "city-only(actual)",
                    RecommendedActionSummaryText = SafeText(prepReadModel != null ? prepReadModel.RecommendedActionSummaryText : string.Empty),
                    RecommendedActionReasonText = SafeText(prepReadModel != null ? prepReadModel.RecommendedActionReasonText : string.Empty),
                    WhyNowText = SafeText(prepReadModel != null ? prepReadModel.WhyNowText : string.Empty),
                    ExpectedUsefulnessText = SafeText(prepReadModel != null ? prepReadModel.ExpectedUsefulnessText : string.Empty)
                };
            }
        }

        return lookup;
    }

    private static Dictionary<string, ExpeditionPrepReadModel> BuildPrepReadModelLookup(WorldBoardReadModel board)
    {
        Dictionary<string, ExpeditionPrepReadModel> lookup = new Dictionary<string, ExpeditionPrepReadModel>(StringComparer.Ordinal);
        CityStatusReadModel[] cities = board != null && board.Cities != null ? board.Cities : Array.Empty<CityStatusReadModel>();

        for (int cityIndex = 0; cityIndex < cities.Length; cityIndex++)
        {
            CityStatusReadModel city = cities[cityIndex];
            if (city == null || !HasText(city.CityId))
            {
                continue;
            }

            CityDecisionReadModel decision = city.Decision != null ? city.Decision : CityDecisionModelBuilder.Build(board ?? WorldBoardReadModel.Empty, city);
            CityOpportunitySignal[] opportunities = decision != null && decision.Opportunities != null
                ? decision.Opportunities
                : Array.Empty<CityOpportunitySignal>();

            for (int opportunityIndex = 0; opportunityIndex < opportunities.Length; opportunityIndex++)
            {
                CityOpportunitySignal opportunity = opportunities[opportunityIndex];
                if (opportunity == null || !HasText(opportunity.DungeonId))
                {
                    continue;
                }

                string key = BuildCityDungeonKey(city.CityId, opportunity.DungeonId);
                if (lookup.ContainsKey(key))
                {
                    continue;
                }

                DungeonStatusReadModel dungeon = FindDungeon(board, opportunity.DungeonId);
                if (dungeon == null)
                {
                    continue;
                }

                lookup[key] = ExpeditionPrepModelBuilder.BuildReadModel(
                    board ?? WorldBoardReadModel.Empty,
                    city,
                    dungeon,
                    city.DisplayName,
                    dungeon.DisplayName,
                    string.Empty,
                    string.Empty,
                    city.DispatchPolicyStateId,
                    new ApproachChoice(),
                    Array.Empty<RouteChoice>());
            }
        }

        return lookup;
    }

    private static string BuildConsumerSurfaceState(CityOpportunitySignal opportunity, bool prepLinked)
    {
        if (opportunity == null)
        {
            return "not-visible(actual)";
        }

        bool visibleInCityHub = HasText(opportunity.SummaryText) &&
                                HasText(opportunity.WhyItMattersText) &&
                                HasText(opportunity.ContentSourceLabel);

        if (visibleInCityHub && prepLinked)
        {
            return "cityhub+prep(actual)";
        }

        if (visibleInCityHub)
        {
            return "cityhub-only(actual)";
        }

        return "not-visible(actual)";
    }

    private static string BuildTargetSurfaceProof(
        Dictionary<string, SurfacedConsumerProjection> lookup,
        string cityId,
        string dungeonId,
        string routeId)
    {
        string resolverKey = BuildResolverKey(cityId, dungeonId, routeId);
        if (lookup == null || !lookup.TryGetValue(resolverKey, out SurfacedConsumerProjection projection) || projection == null)
        {
            return resolverKey + " | Consumer=not-visible(actual)";
        }

        return resolverKey +
               " | Consumer=" + SafeText(projection.SurfaceState) +
               " | Route=" + SafeText(projection.RouteLabel) +
               " | Source=" + SafeText(projection.ContentSourceLabel) +
               " | Prep=" + SafeText(projection.PrepLinkState) +
               " | Recommendation=" + SafeText(projection.RecommendationState) +
               " | Summary=" + SafeText(projection.SummaryText) +
               " | Why=" + SafeText(projection.WhyItMattersText);
    }

    private static string BuildTargetEntryProof(SurfacedMatrixEntry entry)
    {
        if (entry == null)
        {
            return "city-b::dungeon-beta::safe | Missing=entry";
        }

        return SafeText(entry.ResolverKey) +
               " | Status=" + SafeText(entry.StatusLabel) +
               " | Surface=" + SafeText(entry.SurfaceState) +
               " | Consumer=" + SafeText(entry.ConsumerSurfaceState) +
               " | Source=" + SafeText(entry.SourceLabel) +
               " | ConsumerSource=" + SafeText(entry.ConsumerSourceLabel) +
               " | Prep=" + SafeText(entry.PrepLinkState) +
               " | Recommendation=" + SafeText(entry.RecommendationState) +
               " | Issues=" + SafeText(entry.IssueSummary);
    }

    private static SurfacedMatrixEntry FindEntry(List<SurfacedMatrixEntry> entries, string cityId, string dungeonId, string routeId)
    {
        if (entries == null)
        {
            return null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            SurfacedMatrixEntry entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            if (string.Equals(entry.CityId, cityId, StringComparison.Ordinal) &&
                string.Equals(entry.DungeonId, dungeonId, StringComparison.Ordinal) &&
                string.Equals(entry.RouteId, routeId, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    private static string BuildExpansionGateStatus(SurfacedMatrixSnapshot snapshot, SurfacedMatrixEntry targetEntry)
    {
        if (!IsTargetSurfaceStable(targetEntry))
        {
            return "A:target-close-out";
        }

        if (!IsSurfacedMatrixHealthy(snapshot))
        {
            return "B:matrix-align";
        }

        return "C:tooling-next";
    }

    private static string BuildExpansionGateReason(SurfacedMatrixSnapshot snapshot, SurfacedMatrixEntry targetEntry)
    {
        if (!IsTargetSurfaceStable(targetEntry))
        {
            return "batch-43 target is not yet fully canonical + cityhub+prep(actual) + prep-linked + recommendation-linked";
        }

        if (!IsSurfacedMatrixHealthy(snapshot))
        {
            return "target is stable, but surfaced matrix still shows hidden canonical, surfaced fallback, consumer mismatch, prep drift, or recommendation drift";
        }

        return "target is stable and the current surfaced portfolio is fully canonical, consumer-visible, prep-linked, recommendation-linked, and varied enough for tooling-first follow-up";
    }

    private static bool IsTargetSurfaceStable(SurfacedMatrixEntry entry)
    {
        return entry != null &&
               string.Equals(entry.StatusLabel, "PASS:fully-canonical-and-surfaced", StringComparison.Ordinal) &&
               string.Equals(entry.ConsumerSurfaceState, "cityhub+prep(actual)", StringComparison.Ordinal) &&
               string.Equals(entry.PrepLinkState, "linked(actual)", StringComparison.Ordinal) &&
               string.Equals(entry.RecommendationState, "connected(actual)", StringComparison.Ordinal) &&
               string.Equals(entry.IssueSummary, "clean", StringComparison.Ordinal);
    }

    private static bool IsSurfacedMatrixHealthy(SurfacedMatrixSnapshot snapshot)
    {
        return snapshot != null &&
               snapshot.CanonicalButNotSurfacedCount == 0 &&
               snapshot.SurfacedUsingFallbackCount == 0 &&
               snapshot.ConsumerMismatchCount == 0 &&
               snapshot.SurfacedPrepLinkedCount == snapshot.SurfacedCount &&
               snapshot.SurfacedRecommendationLinkedCount == snapshot.SurfacedCount &&
               snapshot.MeaningfullyDistinctSurfacedCount >= 3;
    }

    private static string BuildSurfacedMatrixStatusLabel(
        bool isCanonical,
        bool authoredSurfaced,
        bool consumerSurfaced,
        bool consumerUsesFallback,
        bool meaningfulOverride)
    {
        if (authoredSurfaced && !isCanonical)
        {
            return "FAIL:surfaced-using-fallback";
        }

        if (authoredSurfaced != consumerSurfaced)
        {
            return "FAIL:surfaced-consumer-mismatch";
        }

        if (consumerUsesFallback)
        {
            return "FAIL:consumer-using-fallback";
        }

        if (authoredSurfaced && meaningfulOverride)
        {
            return "WARN:surfaced-with-chain-override";
        }

        if (isCanonical && !authoredSurfaced)
        {
            return "WARN:canonical-but-not-surfaced";
        }

        if (isCanonical && consumerSurfaced)
        {
            return "PASS:fully-canonical-and-surfaced";
        }

        if (isCanonical)
        {
            return "PASS:canonical-hidden";
        }

        return "FAIL:fallback-hardcoded";
    }

    private static string BuildSurfacedMatrixStatusReasonSummary(
        bool isCanonical,
        bool authoredSurfaced,
        bool consumerSurfaced,
        bool consumerUsesFallback,
        bool meaningfulOverride,
        bool prepLinked,
        bool recommendationLinked,
        List<string> missing)
    {
        List<string> reasons = new List<string>();

        if (isCanonical)
        {
            reasons.Add("canonical");
        }
        else
        {
            reasons.Add("fallback");
        }

        reasons.Add(authoredSurfaced ? "authored-surfaced" : "hidden");
        reasons.Add(consumerSurfaced ? "consumer-visible" : "consumer-hidden");
        reasons.Add(prepLinked ? "prep-linked" : "prep-missing");
        reasons.Add(recommendationLinked ? "recommendation-linked" : "recommendation-city-only");

        if (consumerUsesFallback)
        {
            reasons.Add("consumer-fallback");
        }

        if (meaningfulOverride)
        {
            reasons.Add("chain-override");
        }

        if (missing != null && missing.Count > 0)
        {
            reasons.Add("missing:" + string.Join("/", missing.ToArray()));
        }

        return string.Join(", ", reasons.ToArray());
    }

    private static int CountMeaningfullyDistinctSurfacedEntries(List<SurfacedMatrixEntry> entries)
    {
        List<SurfacedMatrixEntry> surfacedEntries = new List<SurfacedMatrixEntry>();
        if (entries != null)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                SurfacedMatrixEntry entry = entries[i];
                if (entry != null &&
                    string.Equals(entry.ConsumerSurfaceState, "cityhub+prep(actual)", StringComparison.Ordinal))
                {
                    surfacedEntries.Add(entry);
                }
            }
        }

        int count = 0;
        for (int i = 0; i < surfacedEntries.Count; i++)
        {
            SurfacedMatrixEntry entry = surfacedEntries[i];
            bool meaningfullyDistinct = true;
            for (int j = 0; j < surfacedEntries.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                if (CountDistinctMeaningAxes(entry, surfacedEntries[j]) < 2)
                {
                    meaningfullyDistinct = false;
                    break;
                }
            }

            if (meaningfullyDistinct)
            {
                count++;
            }
        }

        return count;
    }

    private static string BuildDistinctnessSummary(SurfacedMatrixEntry entry, List<SurfacedMatrixEntry> surfacedEntries)
    {
        if (entry == null)
        {
            return "None";
        }

        int distinctFromCount = 0;
        List<string> nearDuplicates = new List<string>();
        List<SurfacedMatrixEntry> peers = surfacedEntries ?? new List<SurfacedMatrixEntry>();

        for (int i = 0; i < peers.Count; i++)
        {
            SurfacedMatrixEntry other = peers[i];
            if (other == null || ReferenceEquals(entry, other))
            {
                continue;
            }

            int differingAxes = CountDistinctMeaningAxes(entry, other);
            if (differingAxes >= 2)
            {
                distinctFromCount++;
            }
            else
            {
                nearDuplicates.Add(SafeText(other.ResolverKey) + "(" + differingAxes + "-axis)");
            }
        }

        if (nearDuplicates.Count > 0)
        {
            return "near-duplicate:" + string.Join(", ", nearDuplicates.ToArray());
        }

        return "distinct-from-" + distinctFromCount + "-others";
    }

    private static int CountDistinctMeaningAxes(SurfacedMatrixEntry left, SurfacedMatrixEntry right)
    {
        if (left == null || right == null)
        {
            return 0;
        }

        int differenceCount = 0;

        if (!string.Equals(BuildCityMeaningKey(left), BuildCityMeaningKey(right), StringComparison.Ordinal))
        {
            differenceCount++;
        }

        if (!string.Equals(BuildRouteMeaningKey(left), BuildRouteMeaningKey(right), StringComparison.Ordinal))
        {
            differenceCount++;
        }

        if (!string.Equals(BuildOutcomeMeaningKey(left), BuildOutcomeMeaningKey(right), StringComparison.Ordinal))
        {
            differenceCount++;
        }

        if (!string.Equals(BuildEncounterBattleMeaningKey(left), BuildEncounterBattleMeaningKey(right), StringComparison.Ordinal))
        {
            differenceCount++;
        }

        if (!string.Equals(BuildRecommendationMeaningKey(left), BuildRecommendationMeaningKey(right), StringComparison.Ordinal))
        {
            differenceCount++;
        }

        return differenceCount;
    }

    private static string BuildCityMeaningKey(SurfacedMatrixEntry entry)
    {
        return SafeText(entry != null ? entry.CityId : string.Empty) +
               "|" +
               SafeText(entry != null ? entry.CityDecisionMeaningId : string.Empty) +
               "|" +
               SafeText(entry != null ? entry.ConsumerWhyItMattersText : string.Empty);
    }

    private static string BuildRouteMeaningKey(SurfacedMatrixEntry entry)
    {
        return SafeText(entry != null ? entry.RouteId : string.Empty) +
               "|" +
               SafeText(entry != null ? entry.RouteMeaningId : string.Empty);
    }

    private static string BuildOutcomeMeaningKey(SurfacedMatrixEntry entry)
    {
        return SafeText(entry != null ? entry.OutcomeMeaningId : string.Empty) +
               "|" +
               SafeText(entry != null ? entry.ExpectedUsefulnessText : string.Empty);
    }

    private static string BuildEncounterBattleMeaningKey(SurfacedMatrixEntry entry)
    {
        return SafeText(entry != null ? entry.EncounterProfileId : string.Empty) +
               "|" +
               SafeText(entry != null ? entry.BattleSetupId : string.Empty);
    }

    private static string BuildRecommendationMeaningKey(SurfacedMatrixEntry entry)
    {
        return SafeText(entry != null ? entry.RecommendedActionReasonText : string.Empty) +
               "|" +
               SafeText(entry != null ? entry.RecommendationState : string.Empty);
    }

    private static CityOpportunitySignal FindOpportunity(CityOpportunitySignal[] opportunities, string dungeonId, string routeId)
    {
        if (opportunities == null || opportunities.Length < 1)
        {
            return null;
        }

        for (int i = 0; i < opportunities.Length; i++)
        {
            CityOpportunitySignal opportunity = opportunities[i];
            if (opportunity == null)
            {
                continue;
            }

            bool dungeonMatches = !HasText(dungeonId) || string.Equals(opportunity.DungeonId, dungeonId, StringComparison.Ordinal);
            bool routeMatches = !HasText(routeId) || string.Equals(opportunity.RouteId, routeId, StringComparison.Ordinal);
            if (dungeonMatches && routeMatches)
            {
                return opportunity;
            }
        }

        return null;
    }

    private static DungeonStatusReadModel FindDungeon(WorldBoardReadModel board, string dungeonId)
    {
        DungeonStatusReadModel[] dungeons = board != null && board.Dungeons != null ? board.Dungeons : Array.Empty<DungeonStatusReadModel>();
        for (int i = 0; i < dungeons.Length; i++)
        {
            DungeonStatusReadModel dungeon = dungeons[i];
            if (dungeon != null && string.Equals(dungeon.DungeonId, dungeonId, StringComparison.Ordinal))
            {
                return dungeon;
            }
        }

        return null;
    }

    private static void ValidateRepresentativeChain(
        RepresentativeChainValidationProfile profile,
        List<string> passMessages,
        List<string> warnMessages,
        List<string> failMessages,
        SortedSet<string> sharedDefinitionsUsed)
    {
        string canonicalSource = GoldenPathContentRegistry.BuildContentSourceLabel(profile.CityId, profile.DungeonId, profile.RouteId);
        bool chainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(profile.CityId, profile.DungeonId, profile.RouteId, out GoldenPathChainDefinition chainDefinition);
        bool primaryLoaded = GoldenPathContentRegistry.TryGetChain(profile.CityId, profile.DungeonId, out GoldenPathChainDefinition primaryDefinition);
        bool routeLoaded = GoldenPathContentRegistry.TryGetCanonicalRoute(
            profile.CityId,
            profile.DungeonId,
            profile.RouteId,
            out GoldenPathChainDefinition routeChainDefinition,
            out GoldenPathRouteDefinition routeDefinition);
        bool representativeRoomLoaded = GoldenPathContentRegistry.TryGetRepresentativeEncounterRoom(
            profile.CityId,
            profile.DungeonId,
            profile.RouteId,
            out GoldenPathChainDefinition _,
            out GoldenPathRouteDefinition _,
            out GoldenPathRoomDefinition representativeRoom);

        GoldenPathRouteMeaningDefinition routeMeaning = null;
        GoldenPathOutcomeMeaningDefinition outcomeMeaning = null;
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning = null;
        GoldenPathEncounterProfileDefinition encounterProfile = null;
        GoldenPathBattleSetupDefinition battleSetup = null;

        bool routeMeaningLoaded = routeDefinition != null &&
                                  HasText(routeDefinition.RouteMeaningId) &&
                                  GoldenPathContentRegistry.TryGetRouteMeaning(routeDefinition.RouteMeaningId, out routeMeaning);
        bool outcomeMeaningLoaded = chainDefinition != null &&
                                    HasText(chainDefinition.OutcomeMeaningId) &&
                                    GoldenPathContentRegistry.TryGetOutcomeMeaning(chainDefinition.OutcomeMeaningId, out outcomeMeaning);
        bool cityDecisionMeaningLoaded = chainDefinition != null &&
                                         HasText(chainDefinition.CityDecisionMeaningId) &&
                                         GoldenPathContentRegistry.TryGetCityDecisionMeaning(chainDefinition.CityDecisionMeaningId, out cityDecisionMeaning);
        bool encounterProfileLoaded = representativeRoom != null &&
                                      HasText(representativeRoom.EncounterProfileId) &&
                                      GoldenPathContentRegistry.TryGetEncounterProfile(representativeRoom.EncounterProfileId, out encounterProfile);
        bool battleSetupLoaded = representativeRoom != null &&
                                 HasText(representativeRoom.BattleSetupId) &&
                                 GoldenPathContentRegistry.TryGetBattleSetup(representativeRoom.BattleSetupId, out battleSetup);
        bool surfacedVariantFlag = chainDefinition != null && chainDefinition.SurfaceAsOpportunityVariant;

        bool usesCanonicalData = canonicalSource.StartsWith("data:", StringComparison.Ordinal);
        bool primaryMatchesExpectation = !profile.ShouldOwnPrimaryCityDungeonLookup ||
                                        (primaryLoaded &&
                                         primaryDefinition != null &&
                                         routeChainDefinition != null &&
                                         string.Equals(primaryDefinition.ChainId, routeChainDefinition.ChainId, StringComparison.Ordinal) &&
                                         chainDefinition != null &&
                                         chainDefinition.PrimaryCityDungeonDefinition);
        bool routeVariantMatchesExpectation = !profile.ShouldDifferFromPrimaryLookup ||
                                             (primaryLoaded &&
                                              primaryDefinition != null &&
                                              routeChainDefinition != null &&
                                              !string.Equals(primaryDefinition.ChainId, routeChainDefinition.ChainId, StringComparison.Ordinal) &&
                                              chainDefinition != null &&
                                              !chainDefinition.PrimaryCityDungeonDefinition);

        List<string> missing = new List<string>();
        if (!chainLoaded || chainDefinition == null)
        {
            missing.Add("chain-definition");
        }

        if (!routeLoaded || routeDefinition == null)
        {
            missing.Add("canonical-route");
        }

        if (!usesCanonicalData)
        {
            missing.Add("canonical-data-path");
        }

        if (!routeMeaningLoaded)
        {
            missing.Add("shared-route-meaning");
        }

        if (!outcomeMeaningLoaded)
        {
            missing.Add("shared-outcome-meaning");
        }

        if (!cityDecisionMeaningLoaded)
        {
            missing.Add("shared-city-decision-meaning");
        }

        if (!representativeRoomLoaded || representativeRoom == null)
        {
            missing.Add("representative-room");
        }

        if (!encounterProfileLoaded)
        {
            missing.Add("shared-encounter-profile");
        }

        if (!battleSetupLoaded)
        {
            missing.Add("shared-battle-setup");
        }

        if (!primaryMatchesExpectation)
        {
            missing.Add("primary-city-dungeon-owner");
        }

        if (!routeVariantMatchesExpectation)
        {
            missing.Add("route-variant-owner");
        }

        if (profile.ShouldSurfaceAsOpportunityVariant && !surfacedVariantFlag)
        {
            missing.Add("surface-opportunity-variant=false");
        }

        if (routeMeaningLoaded)
        {
            sharedDefinitionsUsed.Add("route:" + routeMeaning.RouteMeaningId);
        }

        if (outcomeMeaningLoaded)
        {
            sharedDefinitionsUsed.Add("outcome:" + outcomeMeaning.OutcomeMeaningId);
        }

        if (cityDecisionMeaningLoaded)
        {
            sharedDefinitionsUsed.Add("city:" + cityDecisionMeaning.CityDecisionMeaningId);
        }

        if (encounterProfileLoaded)
        {
            sharedDefinitionsUsed.Add("encounter:" + encounterProfile.EncounterProfileId);
        }

        if (battleSetupLoaded)
        {
            sharedDefinitionsUsed.Add("battle:" + battleSetup.BattleSetupId);
        }

        string overrideSummary = BuildOverrideSummary(chainDefinition);
        string detail =
            profile.Label +
            " | Source=" + SafeText(canonicalSource) +
            " | ChainId=" + SafeText(chainDefinition != null ? chainDefinition.ChainId : "None") +
            " | Route=" + SafeText(routeDefinition != null ? routeDefinition.RouteId : profile.RouteId) +
            " | Surface=" + BuildAuthoredSurfaceState(
                chainDefinition != null && chainDefinition.PrimaryCityDungeonDefinition,
                surfacedVariantFlag) +
            " | Shared=" + BuildSharedSummary(routeMeaning, outcomeMeaning, cityDecisionMeaning, encounterProfile, battleSetup) +
            " | Overrides=" + overrideSummary +
            " | Why=" + SafeText(profile.WhyItMatters);

        if (missing.Count > 0)
        {
            failMessages.Add(detail + " | Missing=" + string.Join(", ", missing.ToArray()));
            return;
        }

        passMessages.Add(detail);

        if (HasMeaningfulOverride(overrideSummary))
        {
            warnMessages.Add(
                profile.Label +
                " keeps chain-local overrides active. Overrides=" + overrideSummary +
                " | Use shared authoring where possible and document why the override stays chain-specific.");
        }
    }

    private static void ValidateSupportedSafeVariant(
        List<string> passMessages,
        List<string> failMessages,
        SortedSet<string> sharedDefinitionsUsed)
    {
        const string cityId = "city-b";
        const string dungeonId = "dungeon-beta";
        const string routeId = "safe";
        string resolverKey = cityId + "::" + dungeonId + "::" + routeId;
        string canonicalSource = GoldenPathContentRegistry.BuildContentSourceLabel(cityId, dungeonId, routeId);
        bool chainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(cityId, dungeonId, routeId, out GoldenPathChainDefinition chainDefinition);
        bool primaryLoaded = GoldenPathContentRegistry.TryGetChain(cityId, dungeonId, out GoldenPathChainDefinition primaryDefinition);
        bool routeLoaded = GoldenPathContentRegistry.TryGetCanonicalRoute(
            cityId,
            dungeonId,
            routeId,
            out GoldenPathChainDefinition routeChainDefinition,
            out GoldenPathRouteDefinition routeDefinition);
        bool representativeRoomLoaded = GoldenPathContentRegistry.TryGetRepresentativeEncounterRoom(
            cityId,
            dungeonId,
            routeId,
            out GoldenPathChainDefinition _,
            out GoldenPathRouteDefinition _,
            out GoldenPathRoomDefinition representativeRoom);

        GoldenPathRouteMeaningDefinition routeMeaning = null;
        GoldenPathOutcomeMeaningDefinition outcomeMeaning = null;
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning = null;
        GoldenPathEncounterProfileDefinition encounterProfile = null;
        GoldenPathBattleSetupDefinition battleSetup = null;

        bool routeMeaningLoaded = routeDefinition != null &&
                                  HasText(routeDefinition.RouteMeaningId) &&
                                  GoldenPathContentRegistry.TryGetRouteMeaning(routeDefinition.RouteMeaningId, out routeMeaning);
        bool outcomeMeaningLoaded = chainDefinition != null &&
                                    HasText(chainDefinition.OutcomeMeaningId) &&
                                    GoldenPathContentRegistry.TryGetOutcomeMeaning(chainDefinition.OutcomeMeaningId, out outcomeMeaning);
        bool cityDecisionMeaningLoaded = chainDefinition != null &&
                                         HasText(chainDefinition.CityDecisionMeaningId) &&
                                         GoldenPathContentRegistry.TryGetCityDecisionMeaning(chainDefinition.CityDecisionMeaningId, out cityDecisionMeaning);
        bool encounterProfileLoaded = representativeRoom != null &&
                                      HasText(representativeRoom.EncounterProfileId) &&
                                      GoldenPathContentRegistry.TryGetEncounterProfile(representativeRoom.EncounterProfileId, out encounterProfile);
        bool battleSetupLoaded = representativeRoom != null &&
                                 HasText(representativeRoom.BattleSetupId) &&
                                 GoldenPathContentRegistry.TryGetBattleSetup(representativeRoom.BattleSetupId, out battleSetup);
        bool usesCanonicalData = canonicalSource.StartsWith("data:", StringComparison.Ordinal);
        bool surfacedVariantFlag = chainDefinition != null && chainDefinition.SurfaceAsOpportunityVariant;
        bool routeVariantMatchesExpectation = primaryLoaded &&
                                             primaryDefinition != null &&
                                             routeChainDefinition != null &&
                                             !string.Equals(primaryDefinition.ChainId, routeChainDefinition.ChainId, StringComparison.Ordinal) &&
                                             chainDefinition != null &&
                                             !chainDefinition.PrimaryCityDungeonDefinition;

        List<string> missing = new List<string>();
        if (!chainLoaded || chainDefinition == null)
        {
            missing.Add("chain-definition@" + resolverKey);
        }

        if (!routeLoaded || routeDefinition == null)
        {
            missing.Add("canonical-route@" + resolverKey);
        }

        if (!usesCanonicalData)
        {
            missing.Add("canonical-data-path=" + canonicalSource);
        }

        if (!routeMeaningLoaded)
        {
            missing.Add("shared-route-meaning:" + SafeText(routeDefinition != null ? routeDefinition.RouteMeaningId : "None"));
        }

        if (!outcomeMeaningLoaded)
        {
            missing.Add("shared-outcome-meaning:" + SafeText(chainDefinition != null ? chainDefinition.OutcomeMeaningId : "None"));
        }

        if (!cityDecisionMeaningLoaded)
        {
            missing.Add("shared-city-decision-meaning:" + SafeText(chainDefinition != null ? chainDefinition.CityDecisionMeaningId : "None"));
        }

        if (!representativeRoomLoaded || representativeRoom == null)
        {
            missing.Add("representative-room:" + SafeText(routeDefinition != null ? routeDefinition.RepresentativeEncounterId : "None"));
        }

        if (!encounterProfileLoaded)
        {
            missing.Add("shared-encounter-profile:" + SafeText(representativeRoom != null ? representativeRoom.EncounterProfileId : "None"));
        }

        if (!battleSetupLoaded)
        {
            missing.Add("shared-battle-setup:" + SafeText(representativeRoom != null ? representativeRoom.BattleSetupId : "None"));
        }

        if (!routeVariantMatchesExpectation)
        {
            missing.Add(
                "route-variant-owner primary=" + SafeText(primaryDefinition != null ? primaryDefinition.ChainId : "None") +
                " route=" + SafeText(routeChainDefinition != null ? routeChainDefinition.ChainId : "None"));
        }

        if (!surfacedVariantFlag)
        {
            missing.Add("surface-opportunity-variant=false");
        }

        if (routeMeaningLoaded)
        {
            sharedDefinitionsUsed.Add("route:" + routeMeaning.RouteMeaningId);
        }

        if (outcomeMeaningLoaded)
        {
            sharedDefinitionsUsed.Add("outcome:" + outcomeMeaning.OutcomeMeaningId);
        }

        if (cityDecisionMeaningLoaded)
        {
            sharedDefinitionsUsed.Add("city:" + cityDecisionMeaning.CityDecisionMeaningId);
        }

        if (encounterProfileLoaded)
        {
            sharedDefinitionsUsed.Add("encounter:" + encounterProfile.EncounterProfileId);
        }

        if (battleSetupLoaded)
        {
            sharedDefinitionsUsed.Add("battle:" + battleSetup.BattleSetupId);
        }

        string detail =
            "Surfaced safe variant | ResolverKey=" + resolverKey +
            " | Source=" + SafeText(canonicalSource) +
            " | ChainId=" + SafeText(chainDefinition != null ? chainDefinition.ChainId : "None") +
            " | Route=" + SafeText(routeDefinition != null ? routeDefinition.RouteId : routeId) +
            " | Surface=" + BuildAuthoredSurfaceState(
                chainDefinition != null && chainDefinition.PrimaryCityDungeonDefinition,
                surfacedVariantFlag) +
            " | Shared=" + BuildSharedSummary(routeMeaning, outcomeMeaning, cityDecisionMeaning, encounterProfile, battleSetup) +
            " | Overrides=" + BuildOverrideSummary(chainDefinition) +
            " | Why=City B's surfaced safe route should resolve through the same authoring rail as the risky variant.";

        if (missing.Count > 0)
        {
            failMessages.Add(detail + " | Missing=" + string.Join(", ", missing.ToArray()));
            return;
        }

        passMessages.Add(detail);
    }

    private static RepresentativeChainValidationProfile[] BuildRepresentativeProfiles()
    {
        return new[]
        {
            new RepresentativeChainValidationProfile
            {
                Label = "Representative chain #1",
                CityId = "city-a",
                DungeonId = "dungeon-alpha",
                RouteId = "safe",
                ShouldOwnPrimaryCityDungeonLookup = true,
                ShouldDifferFromPrimaryLookup = false,
                ShouldSurfaceAsOpportunityVariant = false,
                WhyItMatters = "Baseline safe path for City A's shard pressure."
            },
            new RepresentativeChainValidationProfile
            {
                Label = "Representative chain #2",
                CityId = "city-b",
                DungeonId = "dungeon-beta",
                RouteId = "risky",
                ShouldOwnPrimaryCityDungeonLookup = true,
                ShouldDifferFromPrimaryLookup = false,
                ShouldSurfaceAsOpportunityVariant = false,
                WhyItMatters = "Alternative city+dungeon pair proving the authoring rail is reusable."
            },
            new RepresentativeChainValidationProfile
            {
                Label = "Representative chain #3",
                CityId = "city-a",
                DungeonId = "dungeon-alpha",
                RouteId = "risky",
                ShouldOwnPrimaryCityDungeonLookup = false,
                ShouldDifferFromPrimaryLookup = true,
                ShouldSurfaceAsOpportunityVariant = true,
                WhyItMatters = "Route-variant chain proving shared city-side meaning can coexist with route-specific runtime authoring and surfaced opportunity expansion."
            }
        };
    }

    private static void AppendMessages(StringBuilder builder, string level, List<string> messages)
    {
        for (int i = 0; i < messages.Count; i++)
        {
            builder.AppendLine("- " + level + " :: " + messages[i]);
        }
    }

    private static string BuildSharedSummary(
        GoldenPathRouteMeaningDefinition routeMeaning,
        GoldenPathOutcomeMeaningDefinition outcomeMeaning,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning,
        GoldenPathEncounterProfileDefinition encounterProfile,
        GoldenPathBattleSetupDefinition battleSetup)
    {
        List<string> parts = new List<string>();
        if (routeMeaning != null)
        {
            parts.Add("route:" + routeMeaning.RouteMeaningId);
        }

        if (outcomeMeaning != null)
        {
            parts.Add("outcome:" + outcomeMeaning.OutcomeMeaningId);
        }

        if (cityDecisionMeaning != null)
        {
            parts.Add("city:" + cityDecisionMeaning.CityDecisionMeaningId);
        }

        if (encounterProfile != null)
        {
            parts.Add("encounter:" + encounterProfile.EncounterProfileId);
        }

        if (battleSetup != null)
        {
            parts.Add("battle:" + battleSetup.BattleSetupId);
        }

        return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "None";
    }

    private static string BuildSharedAssetReferenceSummary(SurfacedMatrixEntry entry)
    {
        List<string> parts = new List<string>();
        AppendSharedAssetReference(parts, "route", entry != null ? entry.RouteMeaningId : string.Empty, entry != null ? entry.RouteMeaningAssetPath : string.Empty);
        AppendSharedAssetReference(parts, "outcome", entry != null ? entry.OutcomeMeaningId : string.Empty, entry != null ? entry.OutcomeMeaningAssetPath : string.Empty);
        AppendSharedAssetReference(parts, "city", entry != null ? entry.CityDecisionMeaningId : string.Empty, entry != null ? entry.CityDecisionMeaningAssetPath : string.Empty);
        AppendSharedAssetReference(parts, "encounter", entry != null ? entry.EncounterProfileId : string.Empty, entry != null ? entry.EncounterProfileAssetPath : string.Empty);
        AppendSharedAssetReference(parts, "battle", entry != null ? entry.BattleSetupId : string.Empty, entry != null ? entry.BattleSetupAssetPath : string.Empty);
        return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "None";
    }

    private static string BuildIssueSummary(
        bool isCanonical,
        bool authoredSurfaced,
        bool consumerSurfaced,
        bool consumerUsesFallback,
        bool meaningfulOverride,
        bool prepLinked,
        bool recommendationLinked,
        List<string> missing,
        string overrideSummary)
    {
        List<string> issues = new List<string>();
        if (!isCanonical)
        {
            issues.Add("source:fallback-hardcoded");
        }

        if (authoredSurfaced != consumerSurfaced)
        {
            issues.Add(authoredSurfaced ? "consumer:hidden" : "consumer:unexpected-visible");
        }

        if (authoredSurfaced && !prepLinked)
        {
            issues.Add("prep:missing-link");
        }

        if (authoredSurfaced && !recommendationLinked)
        {
            issues.Add("recommendation:city-only");
        }

        if (consumerUsesFallback)
        {
            issues.Add("consumer:fallback");
        }

        if (meaningfulOverride)
        {
            issues.Add("override:" + SafeCompactText(overrideSummary));
        }

        if (missing != null && missing.Count > 0)
        {
            issues.Add("missing:" + string.Join("/", missing.ToArray()));
        }

        return issues.Count > 0 ? string.Join(" | ", issues.ToArray()) : "clean";
    }

    private static string BuildOverrideSummary(GoldenPathChainDefinition definition)
    {
        if (definition == null)
        {
            return "None";
        }

        List<string> overrides = new List<string>();
        if (HasText(definition.MissionObjectiveText))
        {
            overrides.Add("mission-objective");
        }

        if (HasText(definition.BottleneckSummaryText))
        {
            overrides.Add("bottleneck-summary");
        }

        if (HasText(definition.RewardMeaningText))
        {
            overrides.Add("reward-meaning");
        }

        if (HasText(definition.ResultImpactMeaningText))
        {
            overrides.Add("city-impact-meaning");
        }

        return overrides.Count > 0 ? string.Join(", ", overrides.ToArray()) : "None";
    }

    private static string BuildAuthoredSurfaceState(bool isSurfacedPrimary, bool isSurfacedVariant)
    {
        if (isSurfacedPrimary)
        {
            return "cityhub+prep(primary)";
        }

        if (isSurfacedVariant)
        {
            return "cityhub+prep(variant)";
        }

        return "hidden-canonical";
    }

    private static bool HasMeaningfulOverride(string overrideSummary)
    {
        return HasText(overrideSummary) && overrideSummary != "mission-objective, bottleneck-summary";
    }

    private static bool IsTrackedRepresentativeOrSurfaced(SurfacedMatrixEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

        bool isRepresentative = entry.Role.StartsWith("Representative chain", StringComparison.Ordinal);
        bool isSurfacedVariant = string.Equals(entry.Role, "Surfaced safe variant", StringComparison.Ordinal) ||
                                 entry.SurfaceState.StartsWith("cityhub+prep", StringComparison.Ordinal);
        return isRepresentative || isSurfacedVariant;
    }

    private static void AddAssetPathSelection(
        List<UnityEngine.Object> selectedObjects,
        List<string> selectedPaths,
        HashSet<string> seenPaths,
        string assetPath)
    {
        if (!HasText(assetPath) ||
            string.Equals(assetPath, "None", StringComparison.Ordinal) ||
            !seenPaths.Add(assetPath))
        {
            return;
        }

        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        if (asset == null)
        {
            return;
        }

        selectedObjects.Add(asset);
        selectedPaths.Add(assetPath);
    }

    private static void AppendSharedAssetReference(List<string> parts, string label, string definitionId, string assetPath)
    {
        if (!HasText(definitionId))
        {
            return;
        }

        parts.Add(label + ":" + SafeText(definitionId) + "@" + SafeText(assetPath));
    }

    private static string ResolveRouteMeaningAssetPath(string routeMeaningId)
    {
        return ResolveDefinitionAssetPath<GoldenPathRouteMeaningDefinition>(
            "Content/GoldenPathRouteMeanings",
            routeMeaningId,
            definition => definition != null ? definition.RouteMeaningId : string.Empty);
    }

    private static string ResolveOutcomeMeaningAssetPath(string outcomeMeaningId)
    {
        return ResolveDefinitionAssetPath<GoldenPathOutcomeMeaningDefinition>(
            "Content/GoldenPathOutcomeMeanings",
            outcomeMeaningId,
            definition => definition != null ? definition.OutcomeMeaningId : string.Empty);
    }

    private static string ResolveCityDecisionMeaningAssetPath(string cityDecisionMeaningId)
    {
        return ResolveDefinitionAssetPath<GoldenPathCityDecisionMeaningDefinition>(
            "Content/GoldenPathCityDecisionMeanings",
            cityDecisionMeaningId,
            definition => definition != null ? definition.CityDecisionMeaningId : string.Empty);
    }

    private static string ResolveEncounterProfileAssetPath(string encounterProfileId)
    {
        return ResolveDefinitionAssetPath<GoldenPathEncounterProfileDefinition>(
            "Content/GoldenPathEncounterProfiles",
            encounterProfileId,
            definition => definition != null ? definition.EncounterProfileId : string.Empty);
    }

    private static string ResolveBattleSetupAssetPath(string battleSetupId)
    {
        return ResolveDefinitionAssetPath<GoldenPathBattleSetupDefinition>(
            "Content/GoldenPathBattleSetups",
            battleSetupId,
            definition => definition != null ? definition.BattleSetupId : string.Empty);
    }

    private static string ResolveDefinitionAssetPath<T>(string resourceFolder, string definitionId, Func<T, string> idSelector)
        where T : class
    {
        if (!HasText(definitionId))
        {
            return "None";
        }

        TextAsset[] assets = Resources.LoadAll<TextAsset>(resourceFolder);
        for (int i = 0; i < assets.Length; i++)
        {
            TextAsset asset = assets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            T definition = JsonUtility.FromJson<T>(asset.text);
            if (definition == null || !string.Equals(idSelector(definition), definitionId, StringComparison.Ordinal))
            {
                continue;
            }

            return SafeText(AssetDatabase.GetAssetPath(asset));
        }

        return "None";
    }

    private static string ResolveChainRole(string cityId, string dungeonId, string routeId)
    {
        RepresentativeChainValidationProfile[] profiles = BuildRepresentativeProfiles();
        for (int i = 0; i < profiles.Length; i++)
        {
            RepresentativeChainValidationProfile profile = profiles[i];
            if (string.Equals(profile.CityId, cityId, StringComparison.Ordinal) &&
                string.Equals(profile.DungeonId, dungeonId, StringComparison.Ordinal) &&
                string.Equals(profile.RouteId, routeId, StringComparison.Ordinal))
            {
                return profile.Label;
            }
        }

        if (string.Equals(cityId, "city-b", StringComparison.Ordinal) &&
            string.Equals(dungeonId, "dungeon-beta", StringComparison.Ordinal) &&
            string.Equals(routeId, "safe", StringComparison.Ordinal))
        {
            return "Surfaced safe variant";
        }

        return "Authored canonical chain";
    }

    private static string BuildResolverKey(string cityId, string dungeonId, string routeId)
    {
        return (cityId ?? string.Empty) + "::" + (dungeonId ?? string.Empty) + "::" + (routeId ?? string.Empty);
    }

    private static string BuildCityDungeonKey(string cityId, string dungeonId)
    {
        return (cityId ?? string.Empty) + "::" + (dungeonId ?? string.Empty);
    }

    private static int CompareChainAssets(TextAsset left, TextAsset right)
    {
        return string.CompareOrdinal(
            left != null ? left.name : string.Empty,
            right != null ? right.name : string.Empty);
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "None" : value;
    }

    private static string SafeCompactText(string value)
    {
        return SafeText(value).Replace(", ", "/");
    }

    private static string FormatCountOrNone(int count)
    {
        return count > 0 ? count.ToString() : "None";
    }

    private static string[] ToArray(SortedSet<string> values)
    {
        string[] result = new string[values.Count];
        values.CopyTo(result);
        return result;
    }
}
