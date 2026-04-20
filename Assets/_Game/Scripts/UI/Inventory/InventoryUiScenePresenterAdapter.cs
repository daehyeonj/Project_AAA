public sealed class InventoryUiScenePresenterAdapter
{
    public InventoryUiPresenterModel BuildFromPreview(InventoryUiPreviewData previewData)
    {
        return InventoryUiPreviewAdapter.BuildPreviewModel(previewData);
    }

    public InventoryUiPresenterModel BuildPlaceholderRuntimeModel()
    {
        // Future runtime migration should adapt canonical inventory surfaces into this presenter model
        // without making the preview scene depend on BootEntry or world runtime mutation paths.
        return new InventoryUiPresenterModel();
    }
}
