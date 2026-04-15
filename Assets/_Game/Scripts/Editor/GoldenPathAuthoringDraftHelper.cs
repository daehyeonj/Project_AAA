using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class GoldenPathAuthoringDraftHelper
{
    private const string CreateDraftMenuPath = "Tools/Project AAA/Create Draft From Selected Golden Path Chain";
    private const string DraftPromotionReadinessMenuPath = "Tools/Project AAA/Show Draft Promotion Readiness";
    private const string DraftPromotionPreflightMenuPath = "Tools/Project AAA/Show Draft Promotion Preflight";
    private const string QuickOpenDraftPromotionContextMenuPath = "Tools/Project AAA/Quick Open Draft Promotion Context";
    private const string DraftRetargetSummaryMenuPath = "Tools/Project AAA/Show Draft Retarget Candidate";
    private const string CreateRetargetedDraftMenuPath = "Tools/Project AAA/Create Retargeted Draft From Selected Draft";
    private const string HiddenCanonicalPromotionSummaryMenuPath = "Tools/Project AAA/Show Hidden Canonical Promotion Summary";
    private const string PromoteHiddenCanonicalMenuPath = "Tools/Project AAA/Promote Selected Draft As Hidden Canonical";
    private const string DraftFolderAssetPath = "Assets/_Game/AuthoringDrafts/GoldenPathChains";
    private const string CanonicalChainFolderAssetPath = "Assets/_Game/Resources/Content/GoldenPathChains";
    private const string BatchTemplateSourceAssetPath = "Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-rest-path.json";
    private const string BatchTemplateDraftAssetPath = DraftFolderAssetPath + "/template-city-a-dungeon-alpha-safe.json";
    private const string BatchTemplateRetargetDraftAssetPath = DraftFolderAssetPath + "/draft-city-a-dungeon-alpha-safe-off-rail-v1.json";

    private sealed class DraftPromotionAssessment
    {
        public string DraftResolverKey = string.Empty;
        public string CurrentCanonicalOwnerChainId = string.Empty;
        public string CurrentCanonicalOwnerAssetPath = string.Empty;
        public string PromotionState = string.Empty;
        public string ManualNext = string.Empty;
        public string SupportedRailFit = string.Empty;
        public int OpenSupportedRailSlotCount;
        public string OpenSupportedResolverKeys = string.Empty;
    }

    private sealed class DraftSupportedRailSlot
    {
        public string ResolverKey = string.Empty;
        public string CityId = string.Empty;
        public string DungeonId = string.Empty;
        public string RouteId = string.Empty;
        public bool IsOccupied;
        public string OwnerChainId = string.Empty;
        public string OwnerAssetPath = string.Empty;
    }

    private sealed class DraftSupportedRailSnapshot
    {
        public readonly List<DraftSupportedRailSlot> Slots = new List<DraftSupportedRailSlot>();
        public int OccupiedCount;
        public int OpenCount;
    }

    private sealed class DraftRetargetAssessment
    {
        public string SourceResolverKey = string.Empty;
        public string SourcePromotionState = string.Empty;
        public string SourceSupportedRailFit = string.Empty;
        public string SourceCanonicalOwnerChainId = string.Empty;
        public string SourceCanonicalOwnerAssetPath = string.Empty;
        public string RetargetRouteId = string.Empty;
        public string RetargetResolverKey = string.Empty;
        public string RetargetChainId = string.Empty;
        public string RetargetDraftAssetPath = string.Empty;
        public string RetargetState = string.Empty;
        public string RetargetSurfaceState = string.Empty;
        public string RetargetRailFit = string.Empty;
        public string RetargetCanonicalOwnerChainId = string.Empty;
        public string RetargetCanonicalOwnerAssetPath = string.Empty;
        public string RetargetDraftOwnerAssetPath = string.Empty;
        public string ManualNext = string.Empty;
    }

    private sealed class HiddenCanonicalPromotionAssessment
    {
        public string DraftResolverKey = string.Empty;
        public string PromotionState = string.Empty;
        public string CurrentCanonicalOwnerChainId = string.Empty;
        public string CurrentCanonicalOwnerAssetPath = string.Empty;
        public string TargetCanonicalChainId = string.Empty;
        public string TargetCanonicalAssetPath = string.Empty;
        public string TargetSurfaceState = string.Empty;
        public string CurrentRailImpact = string.Empty;
        public string ManualNext = string.Empty;
    }

    [MenuItem(CreateDraftMenuPath, true)]
    private static bool ValidateCreateDraftFromSelectedChainMenu()
    {
        return TryGetSelectedChainAsset(out _, out _, out _);
    }

    [MenuItem(CreateDraftMenuPath)]
    private static void CreateDraftFromSelectedChainMenu()
    {
        if (!TryGetSelectedChainAsset(out TextAsset sourceAsset, out string sourceAssetPath, out GoldenPathChainDefinition sourceDefinition))
        {
            Debug.LogWarning("[AuthoringTooling] Select one golden-path chain JSON asset under Resources/Content/GoldenPathChains before creating a draft.");
            return;
        }

        string suggestedAssetPath = AssetDatabase.GenerateUniqueAssetPath(
            DraftFolderAssetPath + "/" + BuildDraftFileName(sourceDefinition) + ".json");
        CreateDraftFromSourceDefinition(sourceAsset, sourceAssetPath, sourceDefinition, suggestedAssetPath, false);
    }

    [MenuItem(DraftPromotionReadinessMenuPath)]
    private static void ShowDraftPromotionReadinessMenu()
    {
        LogDraftPromotionReadinessSummary(false);
    }

    [MenuItem(DraftPromotionPreflightMenuPath)]
    private static void ShowDraftPromotionPreflightMenu()
    {
        LogDraftPromotionPreflightSummary(false);
    }

    [MenuItem(DraftRetargetSummaryMenuPath, true)]
    private static bool ValidateDraftRetargetSummaryMenu()
    {
        return TryGetSelectedDraftAsset(out _, out _, out _);
    }

    [MenuItem(DraftRetargetSummaryMenuPath)]
    private static void ShowDraftRetargetSummaryMenu()
    {
        if (!TryGetSelectedDraftAsset(out TextAsset draftAsset, out string draftAssetPath, out GoldenPathChainDefinition draftDefinition))
        {
            Debug.LogWarning("[AuthoringTooling] Select one draft JSON asset under AuthoringDrafts/GoldenPathChains before showing retarget candidates.");
            return;
        }

        Debug.Log(BuildDraftRetargetSummary(draftAsset, draftAssetPath, draftDefinition));
    }

    [MenuItem(QuickOpenDraftPromotionContextMenuPath, true)]
    private static bool ValidateQuickOpenDraftPromotionContextMenu()
    {
        return TryGetSelectedDraftAsset(out _, out _, out _);
    }

    [MenuItem(QuickOpenDraftPromotionContextMenuPath)]
    private static void QuickOpenDraftPromotionContextMenu()
    {
        if (!TryGetSelectedDraftAsset(out TextAsset draftAsset, out string draftAssetPath, out GoldenPathChainDefinition draftDefinition))
        {
            Debug.LogWarning("[AuthoringTooling] Select one draft JSON asset under AuthoringDrafts/GoldenPathChains before opening promotion context.");
            return;
        }

        QuickOpenDraftPromotionContext(draftAsset, draftAssetPath, draftDefinition);
    }

    [MenuItem(CreateRetargetedDraftMenuPath, true)]
    private static bool ValidateCreateRetargetedDraftMenu()
    {
        return TryGetSelectedDraftAsset(out _, out _, out _);
    }

    [MenuItem(CreateRetargetedDraftMenuPath)]
    private static void CreateRetargetedDraftMenu()
    {
        if (!TryGetSelectedDraftAsset(out TextAsset draftAsset, out string draftAssetPath, out GoldenPathChainDefinition draftDefinition))
        {
            Debug.LogWarning("[AuthoringTooling] Select one draft JSON asset under AuthoringDrafts/GoldenPathChains before creating a retargeted draft.");
            return;
        }

        CreateRetargetedDraftFromDraft(draftAsset, draftAssetPath, draftDefinition, false);
    }

    [MenuItem(HiddenCanonicalPromotionSummaryMenuPath, true)]
    private static bool ValidateHiddenCanonicalPromotionSummaryMenu()
    {
        return TryGetSelectedDraftAsset(out _, out _, out _);
    }

    [MenuItem(HiddenCanonicalPromotionSummaryMenuPath)]
    private static void ShowHiddenCanonicalPromotionSummaryMenu()
    {
        if (!TryGetSelectedDraftAsset(out TextAsset draftAsset, out string draftAssetPath, out GoldenPathChainDefinition draftDefinition))
        {
            Debug.LogWarning("[AuthoringTooling] Select one draft JSON asset under AuthoringDrafts/GoldenPathChains before showing hidden-canonical promotion summary.");
            return;
        }

        Debug.Log(BuildHiddenCanonicalPromotionSummary(draftAsset, draftAssetPath, draftDefinition));
    }

    [MenuItem(PromoteHiddenCanonicalMenuPath, true)]
    private static bool ValidatePromoteHiddenCanonicalMenu()
    {
        return TryGetSelectedDraftAsset(out _, out _, out _);
    }

    [MenuItem(PromoteHiddenCanonicalMenuPath)]
    private static void PromoteHiddenCanonicalMenu()
    {
        if (!TryGetSelectedDraftAsset(out TextAsset draftAsset, out string draftAssetPath, out GoldenPathChainDefinition draftDefinition))
        {
            Debug.LogWarning("[AuthoringTooling] Select one draft JSON asset under AuthoringDrafts/GoldenPathChains before promoting it as hidden canonical content.");
            return;
        }

        PromoteDraftAsHiddenCanonical(draftAsset, draftAssetPath, draftDefinition, false);
    }

    public static void RunCreateRepresentativeDraftTemplate()
    {
        TextAsset sourceAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BatchTemplateSourceAssetPath);
        if (sourceAsset == null)
        {
            Debug.LogError("[AuthoringTooling] Batch 39 draft template source asset is missing: " + BatchTemplateSourceAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        GoldenPathChainDefinition sourceDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(sourceAsset.text);
        if (sourceDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Batch 39 draft template source JSON is invalid: " + BatchTemplateSourceAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        CreateDraftFromSourceDefinition(sourceAsset, BatchTemplateSourceAssetPath, sourceDefinition, BatchTemplateDraftAssetPath, true);

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    public static void RunDraftPromotionReadinessSummary()
    {
        LogDraftPromotionReadinessSummary(true);
    }

    public static void RunDraftPromotionPreflightSummary()
    {
        LogDraftPromotionPreflightSummary(true);
    }

    public static void RunBatchTemplateDraftPromotionContextSummary()
    {
        TextAsset draftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BatchTemplateDraftAssetPath);
        if (draftAsset == null)
        {
            Debug.LogError("[AuthoringTooling] Draft promotion context source is missing: " + BatchTemplateDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
        if (draftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Draft promotion context JSON is invalid: " + BatchTemplateDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        Debug.Log(BuildDraftPromotionContextSummary(draftAsset, BatchTemplateDraftAssetPath, draftDefinition));

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    public static void RunBatchTemplateDraftRetargetSummary()
    {
        TextAsset draftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BatchTemplateDraftAssetPath);
        if (draftAsset == null)
        {
            Debug.LogError("[AuthoringTooling] Draft retarget summary source is missing: " + BatchTemplateDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
        if (draftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Draft retarget summary JSON is invalid: " + BatchTemplateDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        Debug.Log(BuildDraftRetargetSummary(draftAsset, BatchTemplateDraftAssetPath, draftDefinition));

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    public static void RunCreateBatchTemplateRetargetedDraft()
    {
        TextAsset draftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BatchTemplateDraftAssetPath);
        if (draftAsset == null)
        {
            Debug.LogError("[AuthoringTooling] Draft retarget source is missing: " + BatchTemplateDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
        if (draftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Draft retarget source JSON is invalid: " + BatchTemplateDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        CreateRetargetedDraftFromDraft(draftAsset, BatchTemplateDraftAssetPath, draftDefinition, true);

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    public static void RunBatchTemplateRetargetPromotionSummary()
    {
        TextAsset draftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BatchTemplateRetargetDraftAssetPath);
        if (draftAsset == null)
        {
            Debug.LogError("[AuthoringTooling] Hidden-canonical promotion summary source is missing: " + BatchTemplateRetargetDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
        if (draftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Hidden-canonical promotion summary JSON is invalid: " + BatchTemplateRetargetDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        Debug.Log(BuildHiddenCanonicalPromotionSummary(draftAsset, BatchTemplateRetargetDraftAssetPath, draftDefinition));

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    public static void RunPromoteBatchTemplateRetargetedDraft()
    {
        TextAsset draftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BatchTemplateRetargetDraftAssetPath);
        if (draftAsset == null)
        {
            Debug.LogError("[AuthoringTooling] Hidden-canonical promotion source is missing: " + BatchTemplateRetargetDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
        if (draftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Hidden-canonical promotion source JSON is invalid: " + BatchTemplateRetargetDraftAssetPath);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            return;
        }

        PromoteDraftAsHiddenCanonical(draftAsset, BatchTemplateRetargetDraftAssetPath, draftDefinition, true);

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void CreateDraftFromSourceDefinition(
        TextAsset sourceAsset,
        string sourceAssetPath,
        GoldenPathChainDefinition sourceDefinition,
        string draftAssetPath,
        bool overwriteExisting)
    {
        if (sourceAsset == null || sourceDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Cannot create draft because the source chain asset is missing.");
            return;
        }

        EnsureDraftFolderExists();

        GoldenPathChainDefinition draftDefinition = CloneDefinition(sourceDefinition);
        if (draftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Failed to clone the selected golden-path chain definition.");
            return;
        }

        draftDefinition.ChainId = BuildDraftChainId(sourceDefinition);
        draftDefinition.PrimaryCityDungeonDefinition = false;
        draftDefinition.SurfaceAsOpportunityVariant = false;

        if (draftDefinition.CanonicalRoute == null)
        {
            draftDefinition.CanonicalRoute = new GoldenPathRouteDefinition();
        }

        if (!HasText(draftDefinition.CanonicalRoute.RouteLabel) && HasText(sourceDefinition.CanonicalRoute != null ? sourceDefinition.CanonicalRoute.RouteId : string.Empty))
        {
            draftDefinition.CanonicalRoute.RouteLabel = sourceDefinition.CanonicalRoute.RouteId;
        }

        string resolvedDraftAssetPath = overwriteExisting
            ? draftAssetPath
            : AssetDatabase.GenerateUniqueAssetPath(draftAssetPath);

        string json = JsonUtility.ToJson(draftDefinition, true);
        string absoluteDraftAssetPath = ToAbsoluteAssetPath(resolvedDraftAssetPath);
        string draftDirectory = Path.GetDirectoryName(absoluteDraftAssetPath) ?? string.Empty;
        if (!string.IsNullOrEmpty(draftDirectory))
        {
            Directory.CreateDirectory(draftDirectory);
        }

        File.WriteAllText(absoluteDraftAssetPath, json, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(resolvedDraftAssetPath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh();

        TextAsset draftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(resolvedDraftAssetPath);
        if (draftAsset != null)
        {
            Selection.activeObject = draftAsset;
            EditorGUIUtility.PingObject(draftAsset);
        }

        Debug.Log(BuildDraftCreationSummary(sourceDefinition, sourceAssetPath, draftDefinition, resolvedDraftAssetPath));
    }

    private static bool TryGetSelectedChainAsset(out TextAsset sourceAsset, out string sourceAssetPath, out GoldenPathChainDefinition sourceDefinition)
    {
        sourceAsset = null;
        sourceAssetPath = string.Empty;
        sourceDefinition = null;

        UnityEngine.Object[] selectedObjects = Selection.objects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            TextAsset textAsset = selectedObjects[i] as TextAsset;
            if (textAsset == null)
            {
                continue;
            }

            string assetPath = AssetDatabase.GetAssetPath(textAsset);
            if (!IsGoldenPathChainAsset(assetPath))
            {
                continue;
            }

            GoldenPathChainDefinition definition = JsonUtility.FromJson<GoldenPathChainDefinition>(textAsset.text);
            if (definition == null || !HasText(definition.CityId) || !HasText(definition.DungeonId))
            {
                continue;
            }

            sourceAsset = textAsset;
            sourceAssetPath = assetPath;
            sourceDefinition = definition;
            return true;
        }

        return false;
    }

    private static bool TryGetSelectedDraftAsset(out TextAsset draftAsset, out string draftAssetPath, out GoldenPathChainDefinition draftDefinition)
    {
        draftAsset = null;
        draftAssetPath = string.Empty;
        draftDefinition = null;

        UnityEngine.Object[] selectedObjects = Selection.objects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            TextAsset textAsset = selectedObjects[i] as TextAsset;
            if (textAsset == null)
            {
                continue;
            }

            string assetPath = AssetDatabase.GetAssetPath(textAsset);
            if (!IsDraftChainAsset(assetPath))
            {
                continue;
            }

            GoldenPathChainDefinition definition = JsonUtility.FromJson<GoldenPathChainDefinition>(textAsset.text);
            if (definition == null || !HasText(definition.CityId) || !HasText(definition.DungeonId))
            {
                continue;
            }

            draftAsset = textAsset;
            draftAssetPath = assetPath;
            draftDefinition = definition;
            return true;
        }

        return false;
    }

    private static bool IsGoldenPathChainAsset(string assetPath)
    {
        return HasText(assetPath) &&
               assetPath.StartsWith("Assets/_Game/Resources/Content/GoldenPathChains/", StringComparison.Ordinal) &&
               assetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDraftChainAsset(string assetPath)
    {
        return HasText(assetPath) &&
               assetPath.StartsWith(DraftFolderAssetPath + "/", StringComparison.Ordinal) &&
               assetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static GoldenPathChainDefinition CloneDefinition(GoldenPathChainDefinition sourceDefinition)
    {
        if (sourceDefinition == null)
        {
            return null;
        }

        return JsonUtility.FromJson<GoldenPathChainDefinition>(JsonUtility.ToJson(sourceDefinition));
    }

    private static string BuildDraftCreationSummary(
        GoldenPathChainDefinition sourceDefinition,
        string sourceAssetPath,
        GoldenPathChainDefinition draftDefinition,
        string draftAssetPath)
    {
        GoldenPathRouteDefinition routeDefinition = draftDefinition != null ? draftDefinition.CanonicalRoute : null;
        GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
            ? routeDefinition.Rooms[0]
            : null;
        DraftPromotionAssessment promotionAssessment = AssessDraftPromotion(draftDefinition);

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Golden-path draft scaffold created");
        summary.AppendLine("- SourceAsset=" + SafeText(sourceAssetPath));
        summary.AppendLine("- DraftAsset=" + SafeText(draftAssetPath));
        summary.AppendLine("- DraftChainId=" + SafeText(draftDefinition != null ? draftDefinition.ChainId : string.Empty));
        summary.AppendLine("- City=" + SafeText(draftDefinition != null ? draftDefinition.CityId : string.Empty));
        summary.AppendLine("- Dungeon=" + SafeText(draftDefinition != null ? draftDefinition.DungeonId : string.Empty));
        summary.AppendLine("- Route=" + SafeText(routeDefinition != null ? routeDefinition.RouteId : string.Empty));
        summary.AppendLine("- SurfaceState=hidden-draft");
        summary.AppendLine("- CanonicalState=not-promoted");
        summary.AppendLine("- DraftResolverKey=" + SafeText(promotionAssessment.DraftResolverKey));
        summary.AppendLine("- CurrentCanonicalOwner=" + BuildCanonicalOwnerSummary(promotionAssessment));
        summary.AppendLine("- PromotionState=" + SafeText(promotionAssessment.PromotionState));
        summary.AppendLine("- SharedRefs=" + BuildSharedReferenceSummary(draftDefinition, routeDefinition, representativeRoom));
        summary.AppendLine("- AutoFilled=city/dungeon ids, route structure, room sequence, and shared reference ids copied from the source chain; surfaced flags reset to hidden draft.");
        summary.AppendLine("- ManualNext=" + SafeText(promotionAssessment.ManualNext));
        summary.AppendLine("- ValidatorNote=Drafts stay outside the canonical Resources path, so validator/tooling summaries ignore them until the draft is deliberately promoted.");
        return summary.ToString();
    }

    private static void LogDraftPromotionReadinessSummary(bool exitWhenBatchCompletes)
    {
        Debug.Log(BuildDraftPromotionReadinessSummary());

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void LogDraftPromotionPreflightSummary(bool exitWhenBatchCompletes)
    {
        Debug.Log(BuildDraftPromotionPreflightSummary());

        if (exitWhenBatchCompletes && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static string BuildDraftPromotionReadinessSummary()
    {
        TextAsset[] draftAssets = LoadDraftAssets();
        DraftSupportedRailSnapshot supportedRail = BuildSupportedRailSnapshot();
        int promotableCount = 0;
        int blockedCount = 0;
        int blockedByCanonicalOwnerCount = 0;
        int blockedByRouteSurfaceExpansionCount = 0;
        int blockedByMissingIdentityCount = 0;
        int blockedByInvalidJsonCount = 0;
        int candidateManualReviewCount = 0;
        int retargetableOffRailCandidateCount = 0;
        int hiddenOffRailCanonicalDraftCount = 0;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Draft promotion readiness");
        summary.AppendLine("- DraftCount=" + draftAssets.Length);
        summary.AppendLine("- SupportedRailSlots=" + supportedRail.Slots.Count);
        summary.AppendLine("- OccupiedSupportedRailSlots=" + supportedRail.OccupiedCount);
        summary.AppendLine("- OpenSupportedRailSlots=" + FormatCountOrNone(supportedRail.OpenCount));
        summary.AppendLine("- OpenSupportedResolverKeys=" + BuildOpenSupportedResolverKeySummary(supportedRail));

        for (int i = 0; i < draftAssets.Length; i++)
        {
            TextAsset draftAsset = draftAssets[i];
            if (draftAsset == null || string.IsNullOrWhiteSpace(draftAsset.text))
            {
                continue;
            }

            GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
            if (draftDefinition == null)
            {
                blockedCount++;
                blockedByInvalidJsonCount++;
                summary.AppendLine(
                    "- DraftAsset=" + SafeText(AssetDatabase.GetAssetPath(draftAsset)) +
                    " | PromotionState=blocked:invalid-json" +
                    " | ManualNext=Fix the draft JSON before promotion.");
                continue;
            }

            DraftPromotionAssessment assessment = AssessDraftPromotion(draftDefinition);
            DraftRetargetAssessment retargetAssessment = AssessDraftRetarget(draftDefinition, AssetDatabase.GetAssetPath(draftAsset));
            bool promotable = string.Equals(assessment.PromotionState, "candidate:manual-promotion-review", StringComparison.Ordinal);
            if (promotable)
            {
                promotableCount++;
                candidateManualReviewCount++;
            }
            else
            {
                blockedCount++;

                switch (assessment.PromotionState)
                {
                    case "blocked:resolver-key-already-owned":
                        blockedByCanonicalOwnerCount++;
                        break;
                    case "blocked:requires-runtime-route-surface-expansion":
                        blockedByRouteSurfaceExpansionCount++;
                        break;
                    case "blocked:missing-city-dungeon-route":
                        blockedByMissingIdentityCount++;
                        break;
                }
            }

            if (IsHelperRetargetCandidate(retargetAssessment))
            {
                retargetableOffRailCandidateCount++;
            }
            else if (IsAlreadyHiddenCanonicalRetarget(retargetAssessment))
            {
                hiddenOffRailCanonicalDraftCount++;
            }

            GoldenPathRouteDefinition routeDefinition = draftDefinition.CanonicalRoute;
            GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
                ? routeDefinition.Rooms[0]
                : null;
            summary.AppendLine(
                "- DraftAsset=" + SafeText(AssetDatabase.GetAssetPath(draftAsset)) +
                " | DraftChainId=" + SafeText(draftDefinition.ChainId) +
                " | ResolverKey=" + SafeText(assessment.DraftResolverKey) +
                " | PromotionState=" + SafeText(assessment.PromotionState) +
                " | CurrentCanonicalOwner=" + BuildCanonicalOwnerSummary(assessment) +
                " | SurfaceState=" + BuildDraftSurfaceState(draftDefinition) +
                " | SharedRefs=" + BuildSharedReferenceSummary(draftDefinition, routeDefinition, representativeRoom) +
                " | RetargetState=" + SafeText(retargetAssessment.RetargetState) +
                " | RetargetResolverKey=" + SafeText(retargetAssessment.RetargetResolverKey) +
                " | RetargetDraftAsset=" + SafeText(retargetAssessment.RetargetDraftAssetPath) +
                " | ManualNext=" + SafeText(assessment.ManualNext));
        }

        summary.AppendLine("- PromotableDrafts=" + promotableCount);
        summary.AppendLine("- BlockedDrafts=" + blockedCount);
        summary.AppendLine("- CandidateManualReview=" + candidateManualReviewCount);
        summary.AppendLine("- RetargetableOffRailCandidates=" + retargetableOffRailCandidateCount);
        summary.AppendLine("- HiddenOffRailCanonicalDrafts=" + hiddenOffRailCanonicalDraftCount);
        summary.AppendLine("- BlockedByCanonicalOwner=" + blockedByCanonicalOwnerCount);
        summary.AppendLine("- BlockedByRouteSurfaceExpansion=" + blockedByRouteSurfaceExpansionCount);
        summary.AppendLine("- BlockedByMissingIdentity=" + blockedByMissingIdentityCount);
        summary.AppendLine("- BlockedByInvalidJson=" + blockedByInvalidJsonCount);
        summary.AppendLine("- SurfacedExpansionGate=" + GoldenPathAuthoringValidationRunner.BuildSurfacedOpportunityExpansionGateInlineSummary());
        summary.AppendLine("- PromotionRecommendation=" + BuildDraftPromotionRecommendation(
            promotableCount,
            blockedByCanonicalOwnerCount,
            blockedByRouteSurfaceExpansionCount,
            blockedByMissingIdentityCount,
            blockedByInvalidJsonCount,
            retargetableOffRailCandidateCount,
            supportedRail.OpenCount));
        summary.AppendLine("- SaturationHint=The current Alpha/Beta surfaced matrix already occupies the supported safe/risky slots. Use this summary before trying to promote a draft as if it were a new surfaced opportunity.");
        return summary.ToString();
    }

    private static string BuildDraftPromotionPreflightSummary()
    {
        DraftSupportedRailSnapshot supportedRail = BuildSupportedRailSnapshot();
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Draft promotion preflight");
        summary.AppendLine("- SupportedRailSlots=" + supportedRail.Slots.Count);
        summary.AppendLine("- OccupiedSupportedRailSlots=" + supportedRail.OccupiedCount);
        summary.AppendLine("- OpenSupportedRailSlots=" + FormatCountOrNone(supportedRail.OpenCount));
        summary.AppendLine("- OpenSupportedResolverKeys=" + BuildOpenSupportedResolverKeySummary(supportedRail));
        summary.AppendLine("- PreflightRecommendation=" + BuildDraftPreflightRecommendation(supportedRail));

        for (int i = 0; i < supportedRail.Slots.Count; i++)
        {
            DraftSupportedRailSlot slot = supportedRail.Slots[i];
            summary.AppendLine(
                "- " + SafeText(slot.ResolverKey) +
                " | State=" + (slot.IsOccupied ? "occupied" : "open") +
                " | Owner=" + (slot.IsOccupied ? SafeText(slot.OwnerChainId) : "None") +
                " | Asset=" + (slot.IsOccupied ? SafeText(slot.OwnerAssetPath) : "None"));
        }

        TextAsset draftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BatchTemplateDraftAssetPath);
        if (draftAsset != null && !string.IsNullOrWhiteSpace(draftAsset.text))
        {
            GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
            if (draftDefinition != null)
            {
                DraftPromotionAssessment assessment = AssessDraftPromotion(draftDefinition);
                DraftRetargetAssessment retargetAssessment = AssessDraftRetarget(draftDefinition, BatchTemplateDraftAssetPath);
                summary.AppendLine("- BatchTemplateDraftResolverKey=" + SafeText(assessment.DraftResolverKey));
                summary.AppendLine("- BatchTemplateDraftState=" + SafeText(assessment.PromotionState));
                summary.AppendLine("- BatchTemplateDraftRailFit=" + SafeText(assessment.SupportedRailFit));
                summary.AppendLine("- BatchTemplateDraftRetargetState=" + SafeText(retargetAssessment.RetargetState));
                summary.AppendLine("- BatchTemplateDraftRetargetResolverKey=" + SafeText(retargetAssessment.RetargetResolverKey));
                summary.AppendLine("- BatchTemplateDraftRetargetAsset=" + SafeText(retargetAssessment.RetargetDraftAssetPath));
                summary.AppendLine("- BatchTemplateDraftManualNext=" + SafeText(assessment.ManualNext));
                summary.AppendLine("- BatchTemplateDraftRetargetManualNext=" + SafeText(retargetAssessment.ManualNext));
            }
        }

        return summary.ToString();
    }

    private static void QuickOpenDraftPromotionContext(
        TextAsset draftAsset,
        string draftAssetPath,
        GoldenPathChainDefinition draftDefinition)
    {
        List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>();
        List<string> selectedPaths = new List<string>();
        HashSet<string> seenPaths = new HashSet<string>(StringComparer.Ordinal);
        AddAssetSelection(selectedObjects, selectedPaths, seenPaths, draftAssetPath);

        DraftPromotionAssessment assessment = AssessDraftPromotion(draftDefinition);
        if (HasText(assessment.CurrentCanonicalOwnerAssetPath))
        {
            AddAssetSelection(selectedObjects, selectedPaths, seenPaths, assessment.CurrentCanonicalOwnerAssetPath);
        }

        AddSharedReferenceSelections(selectedObjects, selectedPaths, seenPaths, draftDefinition);

        if (selectedObjects.Count < 1)
        {
            Debug.LogWarning("[AuthoringTooling] No draft promotion context assets were resolved.");
            return;
        }

        Selection.objects = selectedObjects.ToArray();
        EditorGUIUtility.PingObject(selectedObjects[0]);
        Debug.Log(BuildDraftPromotionContextSummary(draftAsset, draftAssetPath, draftDefinition));
    }

    private static string BuildDraftPromotionContextSummary(
        TextAsset draftAsset,
        string draftAssetPath,
        GoldenPathChainDefinition draftDefinition)
    {
        DraftPromotionAssessment assessment = AssessDraftPromotion(draftDefinition);
        GoldenPathRouteDefinition routeDefinition = draftDefinition != null ? draftDefinition.CanonicalRoute : null;
        GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
            ? routeDefinition.Rooms[0]
            : null;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Draft promotion context");
        summary.AppendLine("- DraftAsset=" + SafeText(draftAssetPath));
        summary.AppendLine("- DraftChainId=" + SafeText(draftDefinition != null ? draftDefinition.ChainId : string.Empty));
        summary.AppendLine("- ResolverKey=" + SafeText(assessment.DraftResolverKey));
        summary.AppendLine("- PromotionState=" + SafeText(assessment.PromotionState));
        summary.AppendLine("- CurrentCanonicalOwner=" + BuildCanonicalOwnerSummary(assessment));
        summary.AppendLine("- SurfaceState=" + BuildDraftSurfaceState(draftDefinition));
        summary.AppendLine("- SharedRefs=" + BuildSharedReferenceSummary(draftDefinition, routeDefinition, representativeRoom));
        summary.AppendLine("- SharedAssetPaths=" + BuildSharedAssetPathSummary(draftDefinition, representativeRoom));
        summary.AppendLine("- DraftPromotionFit=" + BuildDraftPromotionFitSummary(assessment));
        summary.AppendLine("- SupportedRailFit=" + SafeText(assessment.SupportedRailFit));
        summary.AppendLine("- OpenSupportedRailSlots=" + FormatCountOrNone(assessment.OpenSupportedRailSlotCount));
        summary.AppendLine("- OpenSupportedResolverKeys=" + SafeText(assessment.OpenSupportedResolverKeys));
        DraftRetargetAssessment retargetAssessment = AssessDraftRetarget(draftDefinition, draftAssetPath);
        summary.AppendLine("- RetargetState=" + SafeText(retargetAssessment.RetargetState));
        summary.AppendLine("- RetargetResolverKey=" + SafeText(retargetAssessment.RetargetResolverKey));
        summary.AppendLine("- RetargetDraftAsset=" + SafeText(retargetAssessment.RetargetDraftAssetPath));
        summary.AppendLine("- RetargetRailFit=" + SafeText(retargetAssessment.RetargetRailFit));
        summary.AppendLine("- SurfacedExpansionGate=" + GoldenPathAuthoringValidationRunner.BuildSurfacedOpportunityExpansionGateInlineSummary());
        summary.AppendLine("- ManualNext=" + SafeText(assessment.ManualNext));
        summary.AppendLine("- RetargetManualNext=" + SafeText(retargetAssessment.ManualNext));
        summary.AppendLine("- QuickOpenHint=Selection now includes the draft, the current canonical owner if any, and the linked shared definition assets.");
        return summary.ToString();
    }

    private static string BuildDraftRetargetSummary(
        TextAsset draftAsset,
        string draftAssetPath,
        GoldenPathChainDefinition draftDefinition)
    {
        DraftPromotionAssessment promotionAssessment = AssessDraftPromotion(draftDefinition);
        DraftRetargetAssessment retargetAssessment = AssessDraftRetarget(draftDefinition, draftAssetPath);
        GoldenPathRouteDefinition routeDefinition = draftDefinition != null ? draftDefinition.CanonicalRoute : null;
        GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
            ? routeDefinition.Rooms[0]
            : null;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Draft retarget candidate");
        summary.AppendLine("- DraftAsset=" + SafeText(draftAssetPath));
        summary.AppendLine("- DraftChainId=" + SafeText(draftDefinition != null ? draftDefinition.ChainId : string.Empty));
        summary.AppendLine("- CurrentResolverKey=" + SafeText(promotionAssessment.DraftResolverKey));
        summary.AppendLine("- CurrentPromotionState=" + SafeText(promotionAssessment.PromotionState));
        summary.AppendLine("- CurrentCanonicalOwner=" + BuildCanonicalOwnerSummary(promotionAssessment));
        summary.AppendLine("- CurrentSupportedRailFit=" + SafeText(promotionAssessment.SupportedRailFit));
        summary.AppendLine("- CurrentSurfaceState=" + BuildDraftSurfaceState(draftDefinition));
        summary.AppendLine("- SharedRefs=" + BuildSharedReferenceSummary(draftDefinition, routeDefinition, representativeRoom));
        summary.AppendLine("- SharedAssetPaths=" + BuildSharedAssetPathSummary(draftDefinition, representativeRoom));
        summary.AppendLine("- RetargetState=" + SafeText(retargetAssessment.RetargetState));
        summary.AppendLine("- RetargetRouteId=" + SafeText(retargetAssessment.RetargetRouteId));
        summary.AppendLine("- RetargetResolverKey=" + SafeText(retargetAssessment.RetargetResolverKey));
        summary.AppendLine("- RetargetChainId=" + SafeText(retargetAssessment.RetargetChainId));
        summary.AppendLine("- RetargetDraftAsset=" + SafeText(retargetAssessment.RetargetDraftAssetPath));
        summary.AppendLine("- RetargetSurfaceState=" + SafeText(retargetAssessment.RetargetSurfaceState));
        summary.AppendLine("- RetargetRailFit=" + SafeText(retargetAssessment.RetargetRailFit));
        summary.AppendLine("- RetargetCanonicalOwner=" + BuildRetargetCanonicalOwnerSummary(retargetAssessment));
        summary.AppendLine("- RetargetDraftOwner=" + SafeText(retargetAssessment.RetargetDraftOwnerAssetPath));
        summary.AppendLine("- ManualNext=" + SafeText(retargetAssessment.ManualNext));
        summary.AppendLine("- RetargetHint=Retarget candidates stay hidden/off-rail until a later batch deliberately promotes them into Resources or widens the surfaced seam.");
        return summary.ToString();
    }

    private static string BuildHiddenCanonicalPromotionSummary(
        TextAsset draftAsset,
        string draftAssetPath,
        GoldenPathChainDefinition draftDefinition)
    {
        HiddenCanonicalPromotionAssessment assessment = AssessHiddenCanonicalPromotion(draftDefinition, draftAssetPath);
        GoldenPathRouteDefinition routeDefinition = draftDefinition != null ? draftDefinition.CanonicalRoute : null;
        GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
            ? routeDefinition.Rooms[0]
            : null;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Hidden canonical promotion summary");
        summary.AppendLine("- DraftAsset=" + SafeText(draftAssetPath));
        summary.AppendLine("- DraftChainId=" + SafeText(draftDefinition != null ? draftDefinition.ChainId : string.Empty));
        summary.AppendLine("- DraftResolverKey=" + SafeText(assessment.DraftResolverKey));
        summary.AppendLine("- PromotionState=" + SafeText(assessment.PromotionState));
        summary.AppendLine("- CurrentCanonicalOwner=" + BuildHiddenCanonicalOwnerSummary(assessment));
        summary.AppendLine("- TargetCanonicalChainId=" + SafeText(assessment.TargetCanonicalChainId));
        summary.AppendLine("- TargetCanonicalAsset=" + SafeText(assessment.TargetCanonicalAssetPath));
        summary.AppendLine("- TargetSurfaceState=" + SafeText(assessment.TargetSurfaceState));
        summary.AppendLine("- CurrentRailImpact=" + SafeText(assessment.CurrentRailImpact));
        summary.AppendLine("- SharedRefs=" + BuildSharedReferenceSummary(draftDefinition, routeDefinition, representativeRoom));
        summary.AppendLine("- SharedAssetPaths=" + BuildSharedAssetPathSummary(draftDefinition, representativeRoom));
        summary.AppendLine("- ManualNext=" + SafeText(assessment.ManualNext));
        summary.AppendLine("- PromotionHint=This path proves owner-safe hidden-canonical promotion only. It does not widen the current surfaced safe/risky rail.");
        return summary.ToString();
    }

    private static void PromoteDraftAsHiddenCanonical(
        TextAsset draftAsset,
        string draftAssetPath,
        GoldenPathChainDefinition draftDefinition,
        bool overwriteExisting)
    {
        if (draftAsset == null || draftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Cannot promote draft because the source draft asset is missing.");
            return;
        }

        HiddenCanonicalPromotionAssessment assessment = AssessHiddenCanonicalPromotion(draftDefinition, draftAssetPath);
        if (!string.Equals(assessment.PromotionState, "candidate:promote-hidden-canonical-owner-safe", StringComparison.Ordinal))
        {
            Debug.LogWarning("[AuthoringTooling] Hidden-canonical promotion is not available for the selected draft. " + SafeText(assessment.ManualNext));
            return;
        }

        EnsureCanonicalChainFolderExists();

        GoldenPathChainDefinition promotedDefinition = CloneDefinition(draftDefinition);
        if (promotedDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Failed to clone the selected draft definition for hidden-canonical promotion.");
            return;
        }

        promotedDefinition.ChainId = assessment.TargetCanonicalChainId;
        promotedDefinition.PrimaryCityDungeonDefinition = false;
        promotedDefinition.SurfaceAsOpportunityVariant = false;

        string resolvedCanonicalAssetPath = overwriteExisting
            ? assessment.TargetCanonicalAssetPath
            : AssetDatabase.GenerateUniqueAssetPath(assessment.TargetCanonicalAssetPath);

        string json = JsonUtility.ToJson(promotedDefinition, true);
        File.WriteAllText(ToAbsoluteAssetPath(draftAssetPath), json, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(draftAssetPath);

        string moveError = AssetDatabase.MoveAsset(draftAssetPath, resolvedCanonicalAssetPath);
        if (!string.IsNullOrEmpty(moveError))
        {
            Debug.LogError("[AuthoringTooling] Failed to promote hidden-canonical draft: " + moveError);
            return;
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        Debug.Log(BuildHiddenCanonicalPromotionResultSummary(
            draftDefinition,
            draftAssetPath,
            promotedDefinition,
            resolvedCanonicalAssetPath,
            assessment));
    }

    private static TextAsset[] LoadDraftAssets()
    {
        string[] assetGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { DraftFolderAssetPath });
        List<TextAsset> assets = new List<TextAsset>();
        for (int i = 0; i < assetGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
            if (!HasText(assetPath) || !assetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }

        assets.Sort((left, right) => string.CompareOrdinal(
            AssetDatabase.GetAssetPath(left),
            AssetDatabase.GetAssetPath(right)));
        return assets.ToArray();
    }

    private static void AddSharedReferenceSelections(
        List<UnityEngine.Object> selectedObjects,
        List<string> selectedPaths,
        HashSet<string> seenPaths,
        GoldenPathChainDefinition draftDefinition)
    {
        GoldenPathRouteDefinition routeDefinition = draftDefinition != null ? draftDefinition.CanonicalRoute : null;
        GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
            ? routeDefinition.Rooms[0]
            : null;

        AddAssetSelection(selectedObjects, selectedPaths, seenPaths, ResolveDefinitionAssetPath<GoldenPathRouteMeaningDefinition>(
            "Content/GoldenPathRouteMeanings",
            routeDefinition != null ? routeDefinition.RouteMeaningId : string.Empty,
            definition => definition != null ? definition.RouteMeaningId : string.Empty));
        AddAssetSelection(selectedObjects, selectedPaths, seenPaths, ResolveDefinitionAssetPath<GoldenPathOutcomeMeaningDefinition>(
            "Content/GoldenPathOutcomeMeanings",
            draftDefinition != null ? draftDefinition.OutcomeMeaningId : string.Empty,
            definition => definition != null ? definition.OutcomeMeaningId : string.Empty));
        AddAssetSelection(selectedObjects, selectedPaths, seenPaths, ResolveDefinitionAssetPath<GoldenPathCityDecisionMeaningDefinition>(
            "Content/GoldenPathCityDecisionMeanings",
            draftDefinition != null ? draftDefinition.CityDecisionMeaningId : string.Empty,
            definition => definition != null ? definition.CityDecisionMeaningId : string.Empty));
        AddAssetSelection(selectedObjects, selectedPaths, seenPaths, ResolveDefinitionAssetPath<GoldenPathEncounterProfileDefinition>(
            "Content/GoldenPathEncounterProfiles",
            representativeRoom != null ? representativeRoom.EncounterProfileId : string.Empty,
            definition => definition != null ? definition.EncounterProfileId : string.Empty));
        AddAssetSelection(selectedObjects, selectedPaths, seenPaths, ResolveDefinitionAssetPath<GoldenPathBattleSetupDefinition>(
            "Content/GoldenPathBattleSetups",
            representativeRoom != null ? representativeRoom.BattleSetupId : string.Empty,
            definition => definition != null ? definition.BattleSetupId : string.Empty));
    }

    private static void CreateRetargetedDraftFromDraft(
        TextAsset sourceDraftAsset,
        string sourceDraftAssetPath,
        GoldenPathChainDefinition sourceDraftDefinition,
        bool overwriteExisting)
    {
        if (sourceDraftAsset == null || sourceDraftDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Cannot retarget a draft because the source draft asset is missing.");
            return;
        }

        DraftRetargetAssessment retargetAssessment = AssessDraftRetarget(sourceDraftDefinition, sourceDraftAssetPath);
        if (!IsHelperRetargetCandidate(retargetAssessment))
        {
            Debug.LogWarning("[AuthoringTooling] No non-colliding retarget candidate is available for the selected draft. " + SafeText(retargetAssessment.ManualNext));
            return;
        }

        EnsureDraftFolderExists();

        GoldenPathChainDefinition retargetedDefinition = CloneDefinition(sourceDraftDefinition);
        if (retargetedDefinition == null)
        {
            Debug.LogError("[AuthoringTooling] Failed to clone the selected draft definition for retargeting.");
            return;
        }

        if (retargetedDefinition.CanonicalRoute == null)
        {
            retargetedDefinition.CanonicalRoute = new GoldenPathRouteDefinition();
        }

        retargetedDefinition.ChainId = retargetAssessment.RetargetChainId;
        retargetedDefinition.PrimaryCityDungeonDefinition = false;
        retargetedDefinition.SurfaceAsOpportunityVariant = false;
        retargetedDefinition.CanonicalRoute.RouteId = retargetAssessment.RetargetRouteId;
        if (!HasText(retargetedDefinition.CanonicalRoute.RouteLabel))
        {
            retargetedDefinition.CanonicalRoute.RouteLabel = retargetAssessment.RetargetRouteId;
        }

        string resolvedDraftAssetPath = overwriteExisting
            ? retargetAssessment.RetargetDraftAssetPath
            : AssetDatabase.GenerateUniqueAssetPath(retargetAssessment.RetargetDraftAssetPath);

        string json = JsonUtility.ToJson(retargetedDefinition, true);
        string absoluteDraftAssetPath = ToAbsoluteAssetPath(resolvedDraftAssetPath);
        string draftDirectory = Path.GetDirectoryName(absoluteDraftAssetPath) ?? string.Empty;
        if (!string.IsNullOrEmpty(draftDirectory))
        {
            Directory.CreateDirectory(draftDirectory);
        }

        File.WriteAllText(absoluteDraftAssetPath, json, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(resolvedDraftAssetPath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh();

        TextAsset createdDraftAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(resolvedDraftAssetPath);
        if (createdDraftAsset != null)
        {
            Selection.activeObject = createdDraftAsset;
            EditorGUIUtility.PingObject(createdDraftAsset);
        }

        Debug.Log(BuildRetargetedDraftCreationSummary(
            sourceDraftDefinition,
            sourceDraftAssetPath,
            retargetedDefinition,
            resolvedDraftAssetPath,
            retargetAssessment));
    }

    private static DraftPromotionAssessment AssessDraftPromotion(GoldenPathChainDefinition draftDefinition)
    {
        DraftPromotionAssessment assessment = new DraftPromotionAssessment();
        assessment.DraftResolverKey = BuildResolverKey(
            draftDefinition != null ? draftDefinition.CityId : string.Empty,
            draftDefinition != null ? draftDefinition.DungeonId : string.Empty,
            draftDefinition != null && draftDefinition.CanonicalRoute != null ? draftDefinition.CanonicalRoute.RouteId : string.Empty);
        DraftSupportedRailSnapshot supportedRail = BuildSupportedRailSnapshot();
        assessment.OpenSupportedRailSlotCount = supportedRail.OpenCount;
        assessment.OpenSupportedResolverKeys = BuildOpenSupportedResolverKeySummary(supportedRail);
        assessment.SupportedRailFit = BuildSupportedRailFit(supportedRail, assessment.DraftResolverKey);

        if (draftDefinition == null)
        {
            assessment.PromotionState = "blocked:missing-definition";
            assessment.ManualNext = "Fix the draft JSON before promotion.";
            return assessment;
        }

        string routeId = draftDefinition.CanonicalRoute != null ? draftDefinition.CanonicalRoute.RouteId : string.Empty;
        if (!HasText(draftDefinition.CityId) || !HasText(draftDefinition.DungeonId) || !HasText(routeId))
        {
            assessment.PromotionState = "blocked:missing-city-dungeon-route";
            assessment.ManualNext = "Fill in CityId, DungeonId, and CanonicalRoute.RouteId before promotion.";
            return assessment;
        }

        if (TryFindCanonicalOwner(draftDefinition.CityId, draftDefinition.DungeonId, routeId, out string canonicalOwnerChainId, out string canonicalOwnerAssetPath))
        {
            assessment.CurrentCanonicalOwnerChainId = canonicalOwnerChainId;
            assessment.CurrentCanonicalOwnerAssetPath = canonicalOwnerAssetPath;
            assessment.PromotionState = "blocked:resolver-key-already-owned";
            assessment.ManualNext = supportedRail.OpenCount > 0
                ? "Change the city/dungeon/route identity to an open supported resolver key or keep the draft hidden; moving this JSON into Resources as-is would collide with the current canonical route owner."
                : "The current supported safe/risky rail has no open resolver key. Keep this draft hidden, retarget it beyond the current surfaced rail, or widen the rail before promotion.";
            return assessment;
        }

        if (!IsCurrentSurfacingRouteId(routeId))
        {
            assessment.PromotionState = "blocked:requires-runtime-route-surface-expansion";
            assessment.ManualNext = "Only safe/risky routes currently flow into the player-facing CityHub/ExpeditionPrep surface. Widen the route surface first or keep this draft non-surfaced.";
            return assessment;
        }

        assessment.PromotionState = "candidate:manual-promotion-review";
        assessment.ManualNext = "Review ChainId, city-side rationale, route preview text, and shared refs, then move the draft into Resources and explicitly choose PrimaryCityDungeonDefinition / SurfaceAsOpportunityVariant.";
        return assessment;
    }

    private static DraftRetargetAssessment AssessDraftRetarget(GoldenPathChainDefinition draftDefinition, string currentDraftAssetPath)
    {
        DraftRetargetAssessment assessment = new DraftRetargetAssessment();
        DraftPromotionAssessment currentPromotion = AssessDraftPromotion(draftDefinition);
        assessment.SourceResolverKey = currentPromotion.DraftResolverKey;
        assessment.SourcePromotionState = currentPromotion.PromotionState;
        assessment.SourceSupportedRailFit = currentPromotion.SupportedRailFit;
        assessment.SourceCanonicalOwnerChainId = currentPromotion.CurrentCanonicalOwnerChainId;
        assessment.SourceCanonicalOwnerAssetPath = currentPromotion.CurrentCanonicalOwnerAssetPath;

        if (draftDefinition == null)
        {
            assessment.RetargetState = "blocked:missing-definition";
            assessment.ManualNext = "Fix the draft JSON before asking the helper for a retarget candidate.";
            return assessment;
        }

        string cityId = draftDefinition.CityId;
        string dungeonId = draftDefinition.DungeonId;
        string routeId = draftDefinition.CanonicalRoute != null ? draftDefinition.CanonicalRoute.RouteId : string.Empty;
        if (!HasText(cityId) || !HasText(dungeonId) || !HasText(routeId))
        {
            assessment.RetargetState = "blocked:missing-city-dungeon-route";
            assessment.ManualNext = "Fill in CityId, DungeonId, and CanonicalRoute.RouteId before retargeting.";
            return assessment;
        }

        if (!IsCurrentSurfacingRouteId(routeId) &&
            !TryFindCanonicalOwner(cityId, dungeonId, routeId, out _, out _) &&
            !TryFindDraftOwner(cityId, dungeonId, routeId, currentDraftAssetPath, out _))
        {
            assessment.RetargetRouteId = routeId;
            assessment.RetargetResolverKey = BuildResolverKey(cityId, dungeonId, routeId);
            assessment.RetargetChainId = SafeText(draftDefinition.ChainId);
            assessment.RetargetDraftAssetPath = SafeText(currentDraftAssetPath);
            assessment.RetargetState = "candidate:already-hidden-canonical-retarget";
            assessment.RetargetSurfaceState = "hidden-canonical-if-promoted";
            assessment.RetargetRailFit = "outside-current-supported-linked-dungeon-rail";
            assessment.ManualNext = "This draft already uses a non-colliding off-rail resolver key. Keep it hidden, or move it into Resources later as hidden canonical content before widening the surfaced seam.";
            return assessment;
        }

        for (int candidateIndex = 1; candidateIndex <= 12; candidateIndex++)
        {
            string candidateRouteId = BuildRetargetCandidateRouteId(routeId, candidateIndex);
            if (!HasText(candidateRouteId))
            {
                continue;
            }

            string candidateResolverKey = BuildResolverKey(cityId, dungeonId, candidateRouteId);
            string candidateDraftAssetPath = BuildRetargetDraftAssetPath(draftDefinition, candidateRouteId);
            string candidateChainId = BuildRetargetChainId(draftDefinition, candidateRouteId);

            if (TryFindCanonicalOwner(cityId, dungeonId, candidateRouteId, out string canonicalOwnerChainId, out string canonicalOwnerAssetPath))
            {
                if (candidateIndex == 1)
                {
                    assessment.RetargetCanonicalOwnerChainId = canonicalOwnerChainId;
                    assessment.RetargetCanonicalOwnerAssetPath = canonicalOwnerAssetPath;
                }

                continue;
            }

            if (TryFindDraftOwner(cityId, dungeonId, candidateRouteId, currentDraftAssetPath, out string existingDraftOwnerAssetPath))
            {
                if (string.Equals(existingDraftOwnerAssetPath, candidateDraftAssetPath, StringComparison.Ordinal))
                {
                    assessment.RetargetRouteId = candidateRouteId;
                    assessment.RetargetResolverKey = candidateResolverKey;
                    assessment.RetargetChainId = candidateChainId;
                    assessment.RetargetDraftAssetPath = candidateDraftAssetPath;
                    assessment.RetargetState = "candidate:existing-hidden-canonical-retarget";
                    assessment.RetargetSurfaceState = "hidden-canonical-if-promoted";
                    assessment.RetargetRailFit = "outside-current-supported-linked-dungeon-rail";
                    assessment.RetargetDraftOwnerAssetPath = existingDraftOwnerAssetPath;
                    assessment.ManualNext = "A non-colliding off-rail retarget draft already exists. Review that draft, keep it hidden, or promote it as hidden canonical content before widening the surfaced seam.";
                    return assessment;
                }

                continue;
            }

            assessment.RetargetRouteId = candidateRouteId;
            assessment.RetargetResolverKey = candidateResolverKey;
            assessment.RetargetChainId = candidateChainId;
            assessment.RetargetDraftAssetPath = candidateDraftAssetPath;
            assessment.RetargetState = "candidate:create-hidden-canonical-retarget";
            assessment.RetargetSurfaceState = "hidden-canonical-if-promoted";
            assessment.RetargetRailFit = "outside-current-supported-linked-dungeon-rail";
            assessment.ManualNext = "Use the retarget helper to create a non-colliding off-rail draft, then review city rationale, route preview text, and shared refs before any later promotion or surfaced-seam widening.";
            return assessment;
        }

        assessment.RetargetState = "blocked:no-non-colliding-retarget-candidate";
        assessment.ManualNext = "Every checked off-rail retarget candidate was already occupied. Rename the route by hand or widen the current surfaced seam intentionally.";
        return assessment;
    }

    private static HiddenCanonicalPromotionAssessment AssessHiddenCanonicalPromotion(
        GoldenPathChainDefinition draftDefinition,
        string draftAssetPath)
    {
        HiddenCanonicalPromotionAssessment assessment = new HiddenCanonicalPromotionAssessment();
        assessment.DraftResolverKey = BuildResolverKey(
            draftDefinition != null ? draftDefinition.CityId : string.Empty,
            draftDefinition != null ? draftDefinition.DungeonId : string.Empty,
            draftDefinition != null && draftDefinition.CanonicalRoute != null ? draftDefinition.CanonicalRoute.RouteId : string.Empty);
        assessment.TargetSurfaceState = "hidden-canonical";

        if (draftDefinition == null)
        {
            assessment.PromotionState = "blocked:missing-definition";
            assessment.ManualNext = "Fix the draft JSON before hidden-canonical promotion.";
            return assessment;
        }

        string routeId = draftDefinition.CanonicalRoute != null ? draftDefinition.CanonicalRoute.RouteId : string.Empty;
        if (!HasText(draftDefinition.CityId) || !HasText(draftDefinition.DungeonId) || !HasText(routeId))
        {
            assessment.PromotionState = "blocked:missing-city-dungeon-route";
            assessment.ManualNext = "Fill in CityId, DungeonId, and CanonicalRoute.RouteId before hidden-canonical promotion.";
            return assessment;
        }

        assessment.TargetCanonicalChainId = BuildPromotedChainId(draftDefinition);
        assessment.TargetCanonicalAssetPath = BuildCanonicalPromotionAssetPath(draftDefinition);
        assessment.CurrentRailImpact = IsCurrentSurfacingRouteId(routeId)
            ? "inside-current-safe-risky-surfaced-rail"
            : "outside-current-safe-risky-surfaced-rail";

        if (TryFindCanonicalOwner(draftDefinition.CityId, draftDefinition.DungeonId, routeId, out string canonicalOwnerChainId, out string canonicalOwnerAssetPath))
        {
            assessment.CurrentCanonicalOwnerChainId = canonicalOwnerChainId;
            assessment.CurrentCanonicalOwnerAssetPath = canonicalOwnerAssetPath;
            assessment.PromotionState = "blocked:resolver-key-already-owned";
            assessment.ManualNext = "This resolver key already belongs to canonical content. Retarget again or review the existing canonical owner instead of promoting another copy.";
            return assessment;
        }

        if (!IsDraftChainAsset(draftAssetPath))
        {
            assessment.PromotionState = "blocked:not-a-draft-asset";
            assessment.ManualNext = "Use hidden-canonical promotion only from AuthoringDrafts/GoldenPathChains.";
            return assessment;
        }

        string targetAbsoluteAssetPath = ToAbsoluteAssetPath(assessment.TargetCanonicalAssetPath);
        if (File.Exists(targetAbsoluteAssetPath))
        {
            assessment.PromotionState = "blocked:target-asset-already-exists";
            assessment.ManualNext = "The target canonical asset path already exists. Review or rename that canonical asset before promoting another hidden-canonical copy.";
            return assessment;
        }

        assessment.PromotionState = "candidate:promote-hidden-canonical-owner-safe";
        assessment.ManualNext = "Promote this draft into Resources as hidden canonical content. Keep PrimaryCityDungeonDefinition=false and SurfaceAsOpportunityVariant=false so the surfaced portfolio stays unchanged.";
        return assessment;
    }

    private static bool TryFindCanonicalOwner(string cityId, string dungeonId, string routeId, out string chainId, out string assetPath)
    {
        chainId = string.Empty;
        assetPath = string.Empty;

        TextAsset[] chainAssets = Resources.LoadAll<TextAsset>("Content/GoldenPathChains");
        for (int i = 0; i < chainAssets.Length; i++)
        {
            TextAsset chainAsset = chainAssets[i];
            if (chainAsset == null || string.IsNullOrWhiteSpace(chainAsset.text))
            {
                continue;
            }

            GoldenPathChainDefinition chainDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(chainAsset.text);
            if (chainDefinition == null ||
                !string.Equals(chainDefinition.CityId, cityId, StringComparison.Ordinal) ||
                !string.Equals(chainDefinition.DungeonId, dungeonId, StringComparison.Ordinal) ||
                chainDefinition.CanonicalRoute == null ||
                !string.Equals(chainDefinition.CanonicalRoute.RouteId, routeId, StringComparison.Ordinal))
            {
                continue;
            }

            chainId = chainDefinition.ChainId;
            assetPath = AssetDatabase.GetAssetPath(chainAsset);
            return true;
        }

        return false;
    }

    private static bool TryFindDraftOwner(
        string cityId,
        string dungeonId,
        string routeId,
        string excludedDraftAssetPath,
        out string assetPath)
    {
        assetPath = string.Empty;

        TextAsset[] draftAssets = LoadDraftAssets();
        for (int i = 0; i < draftAssets.Length; i++)
        {
            TextAsset draftAsset = draftAssets[i];
            if (draftAsset == null || string.IsNullOrWhiteSpace(draftAsset.text))
            {
                continue;
            }

            string candidateAssetPath = AssetDatabase.GetAssetPath(draftAsset);
            if (string.Equals(candidateAssetPath, excludedDraftAssetPath, StringComparison.Ordinal))
            {
                continue;
            }

            GoldenPathChainDefinition draftDefinition = JsonUtility.FromJson<GoldenPathChainDefinition>(draftAsset.text);
            if (draftDefinition == null ||
                !string.Equals(draftDefinition.CityId, cityId, StringComparison.Ordinal) ||
                !string.Equals(draftDefinition.DungeonId, dungeonId, StringComparison.Ordinal) ||
                draftDefinition.CanonicalRoute == null ||
                !string.Equals(draftDefinition.CanonicalRoute.RouteId, routeId, StringComparison.Ordinal))
            {
                continue;
            }

            assetPath = candidateAssetPath;
            return true;
        }

        return false;
    }

    private static void AddAssetSelection(
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

    private static string BuildCanonicalOwnerSummary(DraftPromotionAssessment assessment)
    {
        if (assessment == null || !HasText(assessment.CurrentCanonicalOwnerChainId))
        {
            return "None";
        }

        return assessment.CurrentCanonicalOwnerChainId + "@" + SafeText(assessment.CurrentCanonicalOwnerAssetPath);
    }

    private static string BuildDraftSurfaceState(GoldenPathChainDefinition draftDefinition)
    {
        if (draftDefinition == null)
        {
            return "None";
        }

        if (draftDefinition.PrimaryCityDungeonDefinition)
        {
            return "primary-if-promoted";
        }

        return draftDefinition.SurfaceAsOpportunityVariant
            ? "variant-if-promoted"
            : "hidden-draft";
    }

    private static string BuildDraftPromotionFitSummary(DraftPromotionAssessment assessment)
    {
        if (assessment == null || !HasText(assessment.PromotionState))
        {
            return "unknown";
        }

        switch (assessment.PromotionState)
        {
            case "candidate:manual-promotion-review":
                return "candidate-on-current-safe-risky-surface-rail";
            case "blocked:resolver-key-already-owned":
                return assessment.OpenSupportedRailSlotCount > 0
                    ? "collides-with-existing-canonical-surfaced-slot"
                    : "collides-with-existing-canonical-surfaced-slot-on-saturated-supported-rail";
            case "blocked:requires-runtime-route-surface-expansion":
                return "outside-current-safe-risky-surface-rail";
            case "blocked:missing-city-dungeon-route":
                return "identity-incomplete";
            case "blocked:invalid-json":
                return "draft-json-invalid";
            default:
                return "blocked:manual-review";
        }
    }

    private static string BuildDraftPromotionRecommendation(
        int promotableCount,
        int blockedByCanonicalOwnerCount,
        int blockedByRouteSurfaceExpansionCount,
        int blockedByMissingIdentityCount,
        int blockedByInvalidJsonCount,
        int retargetableOffRailCandidateCount,
        int openSupportedRailSlotCount)
    {
        if (promotableCount > 0)
        {
            return "candidate-ready-on-current-supported-rail";
        }

        if (retargetableOffRailCandidateCount > 0)
        {
            return "retarget-beyond-current-surface-rail-with-helper";
        }

        if (blockedByCanonicalOwnerCount > 0 &&
            blockedByRouteSurfaceExpansionCount == 0 &&
            blockedByMissingIdentityCount == 0 &&
            blockedByInvalidJsonCount == 0)
        {
            return openSupportedRailSlotCount > 0
                ? "retarget-the-draft-resolver-key-before-promotion"
                : "no-open-supported-resolver-key-on-current-rail";
        }

        if (blockedByRouteSurfaceExpansionCount > 0)
        {
            return "widen-runtime-route-surface-before-promotion";
        }

        if (blockedByMissingIdentityCount > 0 || blockedByInvalidJsonCount > 0)
        {
            return "repair-draft-definition-before-promotion";
        }

        return "no-promotable-draft-on-current-surfaced-rail";
    }

    private static string BuildDraftPreflightRecommendation(DraftSupportedRailSnapshot supportedRail)
    {
        if (supportedRail == null)
        {
            return "preflight-unavailable";
        }

        if (supportedRail.OpenCount > 0)
        {
            return "open-supported-slot-exists-review-retarget-before-promotion";
        }

        return "supported-safe-risky-rail-is-saturated-retarget-beyond-current-surface-or-widen-rail";
    }

    private static DraftSupportedRailSnapshot BuildSupportedRailSnapshot()
    {
        DraftSupportedRailSnapshot snapshot = new DraftSupportedRailSnapshot();
        StaticPlaceholderWorldView worldView = new StaticPlaceholderWorldView(PlaceholderResourceDataFactory.Create());
        WorldBoardReadModel board = worldView.BuildWorldBoardReadModel();
        CityStatusReadModel[] cities = board != null && board.Cities != null ? board.Cities : Array.Empty<CityStatusReadModel>();
        HashSet<string> seenResolverKeys = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < cities.Length; i++)
        {
            CityStatusReadModel city = cities[i];
            if (city == null || !HasText(city.CityId) || !HasText(city.LinkedDungeonId))
            {
                continue;
            }

            AddSupportedRailSlot(snapshot, seenResolverKeys, city.CityId, city.LinkedDungeonId, "safe");
            AddSupportedRailSlot(snapshot, seenResolverKeys, city.CityId, city.LinkedDungeonId, "risky");
        }

        snapshot.Slots.Sort((left, right) => string.CompareOrdinal(left.ResolverKey, right.ResolverKey));
        return snapshot;
    }

    private static void AddSupportedRailSlot(
        DraftSupportedRailSnapshot snapshot,
        HashSet<string> seenResolverKeys,
        string cityId,
        string dungeonId,
        string routeId)
    {
        if (snapshot == null ||
            seenResolverKeys == null ||
            !HasText(cityId) ||
            !HasText(dungeonId) ||
            !HasText(routeId))
        {
            return;
        }

        string resolverKey = BuildResolverKey(cityId, dungeonId, routeId);
        if (!seenResolverKeys.Add(resolverKey))
        {
            return;
        }

        DraftSupportedRailSlot slot = new DraftSupportedRailSlot
        {
            ResolverKey = resolverKey,
            CityId = cityId,
            DungeonId = dungeonId,
            RouteId = routeId
        };

        if (TryFindCanonicalOwner(cityId, dungeonId, routeId, out string ownerChainId, out string ownerAssetPath))
        {
            slot.IsOccupied = true;
            slot.OwnerChainId = ownerChainId;
            slot.OwnerAssetPath = ownerAssetPath;
            snapshot.OccupiedCount++;
        }
        else
        {
            snapshot.OpenCount++;
        }

        snapshot.Slots.Add(slot);
    }

    private static string BuildOpenSupportedResolverKeySummary(DraftSupportedRailSnapshot supportedRail)
    {
        if (supportedRail == null || supportedRail.OpenCount < 1)
        {
            return "None";
        }

        List<string> resolverKeys = new List<string>();
        for (int i = 0; i < supportedRail.Slots.Count; i++)
        {
            DraftSupportedRailSlot slot = supportedRail.Slots[i];
            if (slot != null && !slot.IsOccupied && HasText(slot.ResolverKey))
            {
                resolverKeys.Add(slot.ResolverKey);
            }
        }

        return resolverKeys.Count > 0 ? string.Join(", ", resolverKeys.ToArray()) : "None";
    }

    private static string BuildSupportedRailFit(DraftSupportedRailSnapshot supportedRail, string draftResolverKey)
    {
        if (supportedRail == null || !HasText(draftResolverKey))
        {
            return "unknown";
        }

        for (int i = 0; i < supportedRail.Slots.Count; i++)
        {
            DraftSupportedRailSlot slot = supportedRail.Slots[i];
            if (slot == null || !string.Equals(slot.ResolverKey, draftResolverKey, StringComparison.Ordinal))
            {
                continue;
            }

            return slot.IsOccupied
                ? supportedRail.OpenCount > 0
                    ? "supported-slot-owned-by-canonical-chain"
                    : "supported-slot-owned-and-current-rail-saturated"
                : "open-supported-slot";
        }

        return "outside-current-supported-linked-dungeon-rail";
    }

    private static string BuildRetargetCanonicalOwnerSummary(DraftRetargetAssessment assessment)
    {
        if (assessment == null || !HasText(assessment.RetargetCanonicalOwnerChainId))
        {
            return "None";
        }

        return assessment.RetargetCanonicalOwnerChainId + "@" + SafeText(assessment.RetargetCanonicalOwnerAssetPath);
    }

    private static string BuildRetargetCandidateRouteId(string sourceRouteId, int candidateIndex)
    {
        if (!HasText(sourceRouteId) || candidateIndex < 1)
        {
            return string.Empty;
        }

        if (sourceRouteId.IndexOf("-off-rail-", StringComparison.Ordinal) >= 0)
        {
            return sourceRouteId + "-alt-v" + candidateIndex;
        }

        return sourceRouteId + "-off-rail-v" + candidateIndex;
    }

    private static string BuildRetargetChainId(GoldenPathChainDefinition sourceDefinition, string retargetRouteId)
    {
        string cityId = HasText(sourceDefinition != null ? sourceDefinition.CityId : string.Empty)
            ? sourceDefinition.CityId
            : "city";
        string dungeonId = HasText(sourceDefinition != null ? sourceDefinition.DungeonId : string.Empty)
            ? sourceDefinition.DungeonId
            : "dungeon";
        return "draft-" + cityId + "-" + dungeonId + "-" + SafeText(retargetRouteId);
    }

    private static string BuildPromotedChainId(GoldenPathChainDefinition sourceDefinition)
    {
        string sourceChainId = SafeText(sourceDefinition != null ? sourceDefinition.ChainId : string.Empty);
        if (sourceChainId.StartsWith("draft-", StringComparison.Ordinal))
        {
            return sourceChainId.Substring("draft-".Length);
        }

        string cityId = HasText(sourceDefinition != null ? sourceDefinition.CityId : string.Empty)
            ? sourceDefinition.CityId
            : "city";
        string dungeonId = HasText(sourceDefinition != null ? sourceDefinition.DungeonId : string.Empty)
            ? sourceDefinition.DungeonId
            : "dungeon";
        string routeId = HasText(sourceDefinition != null && sourceDefinition.CanonicalRoute != null ? sourceDefinition.CanonicalRoute.RouteId : string.Empty)
            ? sourceDefinition.CanonicalRoute.RouteId
            : "route";
        return cityId + "-" + dungeonId + "-" + routeId;
    }

    private static string BuildCanonicalPromotionAssetPath(GoldenPathChainDefinition sourceDefinition)
    {
        return CanonicalChainFolderAssetPath + "/" + BuildPromotedChainId(sourceDefinition) + ".json";
    }

    private static string BuildRetargetDraftAssetPath(GoldenPathChainDefinition sourceDefinition, string retargetRouteId)
    {
        string cityId = HasText(sourceDefinition != null ? sourceDefinition.CityId : string.Empty)
            ? sourceDefinition.CityId
            : "city";
        string dungeonId = HasText(sourceDefinition != null ? sourceDefinition.DungeonId : string.Empty)
            ? sourceDefinition.DungeonId
            : "dungeon";
        return DraftFolderAssetPath + "/draft-" + cityId + "-" + dungeonId + "-" + SafeText(retargetRouteId) + ".json";
    }

    private static bool IsHelperRetargetCandidate(DraftRetargetAssessment assessment)
    {
        if (assessment == null)
        {
            return false;
        }

        return string.Equals(assessment.RetargetState, "candidate:create-hidden-canonical-retarget", StringComparison.Ordinal) ||
               string.Equals(assessment.RetargetState, "candidate:existing-hidden-canonical-retarget", StringComparison.Ordinal);
    }

    private static bool IsAlreadyHiddenCanonicalRetarget(DraftRetargetAssessment assessment)
    {
        return assessment != null &&
               string.Equals(assessment.RetargetState, "candidate:already-hidden-canonical-retarget", StringComparison.Ordinal);
    }

    private static string BuildRetargetedDraftCreationSummary(
        GoldenPathChainDefinition sourceDraftDefinition,
        string sourceDraftAssetPath,
        GoldenPathChainDefinition retargetedDefinition,
        string retargetedDraftAssetPath,
        DraftRetargetAssessment sourceRetargetAssessment)
    {
        GoldenPathRouteDefinition routeDefinition = retargetedDefinition != null ? retargetedDefinition.CanonicalRoute : null;
        GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
            ? routeDefinition.Rooms[0]
            : null;
        DraftPromotionAssessment createdPromotionAssessment = AssessDraftPromotion(retargetedDefinition);
        DraftRetargetAssessment createdRetargetAssessment = AssessDraftRetarget(retargetedDefinition, retargetedDraftAssetPath);

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Retargeted draft created");
        summary.AppendLine("- SourceDraftAsset=" + SafeText(sourceDraftAssetPath));
        summary.AppendLine("- SourceResolverKey=" + SafeText(sourceRetargetAssessment != null ? sourceRetargetAssessment.SourceResolverKey : string.Empty));
        summary.AppendLine("- SourcePromotionState=" + SafeText(sourceRetargetAssessment != null ? sourceRetargetAssessment.SourcePromotionState : string.Empty));
        summary.AppendLine("- SourceCanonicalOwner=" + BuildRetargetSourceOwnerSummary(sourceRetargetAssessment));
        summary.AppendLine("- RetargetDraftAsset=" + SafeText(retargetedDraftAssetPath));
        summary.AppendLine("- RetargetChainId=" + SafeText(retargetedDefinition != null ? retargetedDefinition.ChainId : string.Empty));
        summary.AppendLine("- RetargetRouteId=" + SafeText(routeDefinition != null ? routeDefinition.RouteId : string.Empty));
        summary.AppendLine("- RetargetResolverKey=" + SafeText(createdPromotionAssessment.DraftResolverKey));
        summary.AppendLine("- RetargetCurrentRailState=" + SafeText(createdPromotionAssessment.PromotionState));
        summary.AppendLine("- RetargetState=" + SafeText(createdRetargetAssessment.RetargetState));
        summary.AppendLine("- RetargetSurfaceState=" + SafeText(createdRetargetAssessment.RetargetSurfaceState));
        summary.AppendLine("- SharedRefs=" + BuildSharedReferenceSummary(retargetedDefinition, routeDefinition, representativeRoom));
        summary.AppendLine("- SharedAssetPaths=" + BuildSharedAssetPathSummary(retargetedDefinition, representativeRoom));
        summary.AppendLine("- ManualNext=" + SafeText(createdRetargetAssessment.ManualNext));
        summary.AppendLine("- RetargetNote=This retargeted draft stays off the current safe/risky surfaced seam. It is a hidden-canonical candidate, not a newly surfaced opportunity.");
        return summary.ToString();
    }

    private static string BuildHiddenCanonicalPromotionResultSummary(
        GoldenPathChainDefinition sourceDraftDefinition,
        string sourceDraftAssetPath,
        GoldenPathChainDefinition promotedDefinition,
        string promotedAssetPath,
        HiddenCanonicalPromotionAssessment promotionAssessment)
    {
        GoldenPathRouteDefinition routeDefinition = promotedDefinition != null ? promotedDefinition.CanonicalRoute : null;
        GoldenPathRoomDefinition representativeRoom = routeDefinition != null && routeDefinition.Rooms != null && routeDefinition.Rooms.Length > 0
            ? routeDefinition.Rooms[0]
            : null;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("[AuthoringTooling] Hidden canonical draft promoted");
        summary.AppendLine("- SourceDraftAsset=" + SafeText(sourceDraftAssetPath));
        summary.AppendLine("- SourceDraftChainId=" + SafeText(sourceDraftDefinition != null ? sourceDraftDefinition.ChainId : string.Empty));
        summary.AppendLine("- PromotedAsset=" + SafeText(promotedAssetPath));
        summary.AppendLine("- PromotedChainId=" + SafeText(promotedDefinition != null ? promotedDefinition.ChainId : string.Empty));
        summary.AppendLine("- PromotedResolverKey=" + SafeText(promotionAssessment != null ? promotionAssessment.DraftResolverKey : string.Empty));
        summary.AppendLine("- PromotionState=promoted:hidden-canonical-owner-safe");
        summary.AppendLine("- SurfaceState=hidden-canonical");
        summary.AppendLine("- RailImpact=unchanged-current-safe-risky-surfaced-rail");
        summary.AppendLine("- SharedRefs=" + BuildSharedReferenceSummary(promotedDefinition, routeDefinition, representativeRoom));
        summary.AppendLine("- SharedAssetPaths=" + BuildSharedAssetPathSummary(promotedDefinition, representativeRoom));
        summary.AppendLine("- ManualNext=Rerun validator plus surfaced/tooling summaries. The current surfaced portfolio should remain four routes while this promoted route appears as canonical but non-surfaced hidden content.");
        summary.AppendLine("- PromotionNote=This is owner-safe off-surface promotion proof. It does not widen the surfaced rail or add a fifth player-facing opportunity.");
        return summary.ToString();
    }

    private static string BuildRetargetSourceOwnerSummary(DraftRetargetAssessment assessment)
    {
        if (assessment == null || !HasText(assessment.SourceCanonicalOwnerChainId))
        {
            return "None";
        }

        return assessment.SourceCanonicalOwnerChainId + "@" + SafeText(assessment.SourceCanonicalOwnerAssetPath);
    }

    private static string BuildHiddenCanonicalOwnerSummary(HiddenCanonicalPromotionAssessment assessment)
    {
        if (assessment == null || !HasText(assessment.CurrentCanonicalOwnerChainId))
        {
            return "None";
        }

        return assessment.CurrentCanonicalOwnerChainId + "@" + SafeText(assessment.CurrentCanonicalOwnerAssetPath);
    }

    private static string FormatCountOrNone(int count)
    {
        return count > 0 ? count.ToString() : "None";
    }

    private static string BuildSharedAssetPathSummary(
        GoldenPathChainDefinition draftDefinition,
        GoldenPathRoomDefinition representativeRoom)
    {
        List<string> parts = new List<string>();
        GoldenPathRouteDefinition routeDefinition = draftDefinition != null ? draftDefinition.CanonicalRoute : null;
        AppendSharedAssetReference(parts, "route", routeDefinition != null ? routeDefinition.RouteMeaningId : string.Empty, ResolveDefinitionAssetPath<GoldenPathRouteMeaningDefinition>(
            "Content/GoldenPathRouteMeanings",
            routeDefinition != null ? routeDefinition.RouteMeaningId : string.Empty,
            definition => definition != null ? definition.RouteMeaningId : string.Empty));
        AppendSharedAssetReference(parts, "outcome", draftDefinition != null ? draftDefinition.OutcomeMeaningId : string.Empty, ResolveDefinitionAssetPath<GoldenPathOutcomeMeaningDefinition>(
            "Content/GoldenPathOutcomeMeanings",
            draftDefinition != null ? draftDefinition.OutcomeMeaningId : string.Empty,
            definition => definition != null ? definition.OutcomeMeaningId : string.Empty));
        AppendSharedAssetReference(parts, "city", draftDefinition != null ? draftDefinition.CityDecisionMeaningId : string.Empty, ResolveDefinitionAssetPath<GoldenPathCityDecisionMeaningDefinition>(
            "Content/GoldenPathCityDecisionMeanings",
            draftDefinition != null ? draftDefinition.CityDecisionMeaningId : string.Empty,
            definition => definition != null ? definition.CityDecisionMeaningId : string.Empty));
        AppendSharedAssetReference(parts, "encounter", representativeRoom != null ? representativeRoom.EncounterProfileId : string.Empty, ResolveDefinitionAssetPath<GoldenPathEncounterProfileDefinition>(
            "Content/GoldenPathEncounterProfiles",
            representativeRoom != null ? representativeRoom.EncounterProfileId : string.Empty,
            definition => definition != null ? definition.EncounterProfileId : string.Empty));
        AppendSharedAssetReference(parts, "battle", representativeRoom != null ? representativeRoom.BattleSetupId : string.Empty, ResolveDefinitionAssetPath<GoldenPathBattleSetupDefinition>(
            "Content/GoldenPathBattleSetups",
            representativeRoom != null ? representativeRoom.BattleSetupId : string.Empty,
            definition => definition != null ? definition.BattleSetupId : string.Empty));
        return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "None";
    }

    private static string BuildSharedReferenceSummary(
        GoldenPathChainDefinition chainDefinition,
        GoldenPathRouteDefinition routeDefinition,
        GoldenPathRoomDefinition representativeRoom)
    {
        StringBuilder summary = new StringBuilder();
        AppendSharedReference(summary, "route", routeDefinition != null ? routeDefinition.RouteMeaningId : string.Empty);
        AppendSharedReference(summary, "outcome", chainDefinition != null ? chainDefinition.OutcomeMeaningId : string.Empty);
        AppendSharedReference(summary, "city", chainDefinition != null ? chainDefinition.CityDecisionMeaningId : string.Empty);
        AppendSharedReference(summary, "encounter", representativeRoom != null ? representativeRoom.EncounterProfileId : string.Empty);
        AppendSharedReference(summary, "battle", representativeRoom != null ? representativeRoom.BattleSetupId : string.Empty);
        return summary.Length > 0 ? summary.ToString() : "None";
    }

    private static void AppendSharedReference(StringBuilder builder, string label, string value)
    {
        if (!HasText(value))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(", ");
        }

        builder.Append(label);
        builder.Append(":");
        builder.Append(value);
    }

    private static void AppendSharedAssetReference(List<string> parts, string label, string definitionId, string assetPath)
    {
        if (!HasText(definitionId))
        {
            return;
        }

        parts.Add(label + ":" + SafeText(definitionId) + "@" + SafeText(assetPath));
    }

    private static string BuildDraftChainId(GoldenPathChainDefinition sourceDefinition)
    {
        string sourceChainId = HasText(sourceDefinition != null ? sourceDefinition.ChainId : string.Empty)
            ? sourceDefinition.ChainId
            : "golden-path";
        return "draft-" + sourceChainId;
    }

    private static string BuildDraftFileName(GoldenPathChainDefinition sourceDefinition)
    {
        string cityId = HasText(sourceDefinition != null ? sourceDefinition.CityId : string.Empty) ? sourceDefinition.CityId : "city";
        string dungeonId = HasText(sourceDefinition != null ? sourceDefinition.DungeonId : string.Empty) ? sourceDefinition.DungeonId : "dungeon";
        string routeId = HasText(sourceDefinition != null && sourceDefinition.CanonicalRoute != null ? sourceDefinition.CanonicalRoute.RouteId : string.Empty)
            ? sourceDefinition.CanonicalRoute.RouteId
            : "route";
        return "draft-" + cityId + "-" + dungeonId + "-" + routeId;
    }

    private static string BuildResolverKey(string cityId, string dungeonId, string routeId)
    {
        return SafeText(cityId) + "::" + SafeText(dungeonId) + "::" + SafeText(routeId);
    }

    private static bool IsCurrentSurfacingRouteId(string routeId)
    {
        return string.Equals(routeId, "safe", StringComparison.Ordinal) ||
               string.Equals(routeId, "risky", StringComparison.Ordinal);
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

    private static void EnsureDraftFolderExists()
    {
        string absoluteDraftFolderPath = ToAbsoluteAssetPath(DraftFolderAssetPath);
        Directory.CreateDirectory(absoluteDraftFolderPath);
    }

    private static void EnsureCanonicalChainFolderExists()
    {
        string absoluteCanonicalFolderPath = ToAbsoluteAssetPath(CanonicalChainFolderAssetPath);
        Directory.CreateDirectory(absoluteCanonicalFolderPath);
    }

    private static string ToAbsoluteAssetPath(string assetPath)
    {
        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        string relativePath = assetPath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(projectRootPath, relativePath);
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "None" : value;
    }
}
