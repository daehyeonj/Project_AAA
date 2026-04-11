using UnityEngine;

public sealed partial class PrototypePresentationShell
{
    private GameplayBattleHudPresenter _gameplayBattleHudPresenter;

    private sealed class GameplayBattleHudPresenter
    {
        private readonly PrototypePresentationShell _owner;

        public GameplayBattleHudPresenter(PrototypePresentationShell owner)
        {
            _owner = owner;
        }

        public void Draw(Rect screenRect)
        {
            _owner.DrawGameplayBattleHud(screenRect);
        }
    }
}
