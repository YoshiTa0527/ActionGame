﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの状態に応じてステートを変更する。プレイヤーにアタッチすること。
/// </summary>
public class PlayerState : MonoBehaviour
{
    public static PlayerMovingDirection m_PlayerDirState { get; set; }
    public static PlayerStates m_PlayerStates { get; set; }
    public static PlayerEquip m_PlayerEquip { get; set; }

    public static PlayerJumpAnimation m_PlayerJumpState { get; set; }
    [SerializeField] float m_rayMaxDis = 1f;

    public static void ChangePlayerDirState(PlayerMovingDirection state)
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
        if (h == 0 && v == 0) { m_PlayerDirState = PlayerMovingDirection.Neutral; }
        if (PlayerController.IsSprint) { m_PlayerDirState = PlayerMovingDirection.Sprint; }
        else
        {
            if (h == 0)
            {
                if (v > 0 && v <= 1) { m_PlayerDirState = PlayerMovingDirection.Forward; }
                else if (v < 0 && v >= -1) { m_PlayerDirState = PlayerMovingDirection.Back; }
            }
            else if (v == 0 || v != 0)
            {
                if (h > 0 && h <= 1) { m_PlayerDirState = PlayerMovingDirection.Right; }
                else if (h < 0 && h >= -1) { m_PlayerDirState = PlayerMovingDirection.Left; }
            }
        }

        /*プレイヤーが空中にいるとき、地面との距離によってステートを切り替える*/
        RaycastHit hit;
        Physics.Raycast(this.transform.position, Vector3.down, out hit, Mathf.Infinity);
        Debug.DrawRay(this.transform.position, Vector3.down * hit.distance, Color.green);

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
public enum PlayerMovingDirection
{
    Neutral,
    Forward,
    Right,
    Left,
    Back,
    Sprint,
}

public enum PlayerJumpAnimation
{
    Start,
    InTheAir,
    End,
}

