using UnityEngine;

public sealed class ExpeditionPrepGateway
{
    private readonly StaticPlaceholderWorldView _worldView;

    public ExpeditionPrepGateway(StaticPlaceholderWorldView worldView)
    {
        _worldView = worldView;
    }

    public ExpeditionPrepSurfaceData GetSurfaceData()
    {
        return _worldView != null ? _worldView.BuildSelectedExpeditionPrepSurfaceData() : new ExpeditionPrepSurfaceData();
    }

    public ExpeditionLaunchRequest GetLaunchRequest()
    {
        return _worldView != null ? _worldView.BuildSelectedExpeditionLaunchRequest() : new ExpeditionLaunchRequest();
    }

    public bool TryOpenBoard()
    {
        return _worldView != null && _worldView.TryOpenSelectedCityExpeditionPrepBoard();
    }

    public void CancelPreparation()
    {
        if (_worldView != null)
        {
            _worldView.CancelExpeditionPrepBoard();
        }
    }

    public bool TrySelectRoute(string optionKey)
    {
        return _worldView != null && _worldView.TrySelectExpeditionPrepRoute(optionKey);
    }

    public bool TryCycleDispatchPolicy()
    {
        return _worldView != null && _worldView.TryCycleExpeditionPrepDispatchPolicy();
    }

    public bool TryConfirmLaunch(Camera worldCamera)
    {
        return _worldView != null && _worldView.TryConfirmSelectedExpeditionLaunch(worldCamera);
    }
}
