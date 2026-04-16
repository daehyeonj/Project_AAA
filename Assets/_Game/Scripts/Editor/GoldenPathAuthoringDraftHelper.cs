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
    private const string DraftFolderAssetPath = "Assets/_Game/AuthoringDrafts/GoldenPathChains";
    private const string BatchTemplateSourceAssetPath = "Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-rest-path.json";
    private const string BatchTemplateDraftAssetPath = DraftFolderAssetPath + "/template-city-a-dungeon-alpha-safe.json";

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
                " | ManualNext=" + SafeText(assessment.ManualNext));
        }

        summary.AppendLine("- PromotableDrafts=" + promotableCount);
        summary.AppendLine("- BlockedDrafts=" + blockedCount);
        summary.AppendLine("- CandidateManualReview=" + candidateManualReviewCount);
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
                summary.AppendLine("- BatchTemplateDraftResolverKey=" + SafeText(assessment.DraftResolverKey));
                summary.AppendLine("- BatchTemplateDraftState=" + SafeText(assessment.PromotionState));
                summary.AppendLine("- BatchTemplateDraftRailFit=" + SafeText(assessment.SupportedRailFit));
                summary.AppendLine("- BatchTemplateDraftManualNext=" + SafeText(assessment.ManualNext));
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
        summary.AppendLine("- SurfacedExpansionGate=" + GoldenPathAuthoringValidationRunner.BuildSurfacedOpportunityExpansionGateInlineSummary());
        summary.AppendLine("- ManualNext=" + SafeText(assessment.ManualNext));
        summary.AppendLine("- QuickOpenHint=Selection now includes the draft, the current canonical owner if any, and the linked shared definition assets.");
        return summary.ToString();
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
        int openSupportedRailSlotCount)
    {
        if (promotableCount > 0)
        {
            return "candidate-ready-on-current-supported-rail";
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
