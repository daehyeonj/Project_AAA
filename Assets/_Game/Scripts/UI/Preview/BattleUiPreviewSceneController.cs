using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class BattleUiPreviewSceneController : MonoBehaviour
{
    [SerializeField] private BattleUiSkinDefinition _skin;
    [SerializeField] private BattleUiLayoutProfile _layout;
    [SerializeField] private BattleUiPreviewData _previewData;
    [SerializeField] private BattleUiPreviewPresenter _presenter;

    private void Reset()
    {
        EnsurePresenter();
    }

    private void Awake()
    {
        EnsurePresenter();
    }

    private void OnEnable()
    {
        EnsurePresenter();
        _presenter.Configure(_skin, _layout, _previewData);
    }

    private void OnGUI()
    {
        EnsurePresenter();
        _presenter.Configure(_skin, _layout, _previewData);
        _presenter.Draw();
    }

    public void SetDependencies(BattleUiSkinDefinition skin, BattleUiLayoutProfile layout, BattleUiPreviewData previewData)
    {
        _skin = skin;
        _layout = layout;
        _previewData = previewData;
        EnsurePresenter();
        _presenter.Configure(_skin, _layout, _previewData);
    }

    private void EnsurePresenter()
    {
        if (_presenter == null)
        {
            _presenter = new BattleUiPreviewPresenter();
        }
    }
}
