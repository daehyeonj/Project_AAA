public sealed class BattleUiScenePresenterAdapter
{
    public BattleUiPresenterModel BuildFromPreview(BattleUiPreviewData previewData)
    {
        return BattleUiPreviewAdapter.BuildPreviewModel(previewData);
    }

    public BattleUiPresenterModel BuildPlaceholderRuntimeModel()
    {
        // Future scene migration should feed canonical battle surfaces into the same presenter model
        // instead of binding scene-local UI directly to BootEntry or SampleScene-only adapters.
        return new BattleUiPresenterModel();
    }
}
