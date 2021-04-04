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
}

public enum PlayerMovingDirection
{
    Forward,
    RightForward,
    LeftForward,
    Right,
    Left,
    Back,
    RightBack,
    LeftBack,
}
