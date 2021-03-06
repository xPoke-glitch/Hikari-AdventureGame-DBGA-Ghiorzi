using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarketTile : Tile
{
    public Animator EmoteAnimator { get => emoteAnimator; set => emoteAnimator = value; }

    [SerializeField]
    private Animator emoteAnimator;

    public override void EnterTile()
    {
        GameController.Instance.Player.IsInBuilding = true;
        emoteAnimator.ResetTrigger("CloseEmote");
        GameController.Instance.Player.Controls.UI.Exit.performed += ExitTile;
        // Show Market UI
        GameController.Instance.Player.StopMoving();
        CinematicController.Instance.StartCinematic();
        UIController.Instance.ShowShop();
        emoteAnimator.SetTrigger("PlayEmote");
    }

    private void ExitTile(InputAction.CallbackContext obj)
    {
        GameController.Instance.Player.IsInBuilding = false;
        Debug.Log("Exit Called");
        CinematicController.Instance.EndCinematic();
        UIController.Instance.HideShop();
        emoteAnimator.SetTrigger("CloseEmote");
        GameController.Instance.Player.BeginMove(Vector2Int.down);
        GameController.Instance.Player.Controls.UI.Exit.performed -= ExitTile;
    }
}
