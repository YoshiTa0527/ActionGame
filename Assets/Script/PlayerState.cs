using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの状態に応じてステートを変更する。プレイヤーにアタッチすること。
/// </summary>
public class PlayerState : MonoBehaviour
{
    public static MovingDirection m_PlayerDirState { get; set; }
    public static PlayerStates m_PlayerStates { get; set; }
    public static PlayerEquip m_PlayerEquip { get; set; }

    public static PlayerJumpAnimation m_PlayerJumpState { get; set; }
    [SerializeField] float m_rayMaxDis = 1f;

    public static void ChangePlayerDirState(MovingDirection state)
    {
        m_PlayerDirState = state;
    }
    public static void ChangePlayerStates(PlayerStates state)
    {
        m_PlayerStates = state;
    }
    public static void ChangePlayerEquip(PlayerEquip state)
    {
        m_PlayerEquip = state;
    }

    private void Start()
    {
        ChangePlayerStates(PlayerStates.InGame);
        ChangePlayerEquip(PlayerEquip.Grapplle);
    }
    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        /*プレイヤーの方向に合わせてステートを切り替える*/
        if (h == 0 && v == 0) { m_PlayerDirState = MovingDirection.Neutral; }
        else m_PlayerDirState = MovingDirection.Move;
        if (LockOnController.IsLock)
        {
            if (h == 0)
            {
                if (v > 0 && v <= 1) { m_PlayerDirState = MovingDirection.Forward; }
                else if (v < 0 && v >= -1) { m_PlayerDirState = MovingDirection.Back; }
            }
            else if (v == 0 || v != 0)
            {
                if (h > 0 && h <= 1) { m_PlayerDirState = MovingDirection.Right; }
                else if (h < 0 && h >= -1) { m_PlayerDirState = MovingDirection.Left; }
            }
        }



        /*プレイヤーが空中にいるとき、地面との距離によってステートを切り替える*/


    }
}

public enum PlayerEquip
{
    Grapplle,
}
public enum PlayerStates
{
    InGame,
    OpenUi,
}
public enum MovingDirection
{
    Neutral,
    Move,
    Forward,
    Right,
    Left,
    Back,
}

public enum PlayerJumpAnimation
{
    Start,
    InTheAir,
    End,
}

