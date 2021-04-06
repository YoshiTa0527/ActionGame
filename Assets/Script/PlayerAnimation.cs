using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public static PlayerMovingDirection PlayerDirState { get; set; }
    public void ChangePlayerDirState(PlayerMovingDirection state)
    {
        PlayerDirState = state;
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Debug.Log("test" + PlayerDirState.ToString());
        if (h == 0 && v == 0) { PlayerDirState = PlayerMovingDirection.Neutral; }

        if (PlayerController.IsSprint) { PlayerDirState = PlayerMovingDirection.Sprint; }
        else
        {
            if (h == 0)
            {
                if (v > 0 && v <= 1) { PlayerDirState = PlayerMovingDirection.Forward; }
                else if (v < 0 && v >= -1) { PlayerDirState = PlayerMovingDirection.Back; }
            }
            else if (v == 0 || v != 0)
            {
                if (h > 0 && h <= 1) { PlayerDirState = PlayerMovingDirection.Right; }
                else if (h < 0 && h >= -1) { PlayerDirState = PlayerMovingDirection.Left; }
            }
        }

    }
}

public enum PlayerMovingDirection
{
    Neutral,
    Forward,
    Right,
    Left,
    Back,
    Sprint,
}


