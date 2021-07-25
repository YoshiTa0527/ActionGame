using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    /// <summary>プレイヤーの体力</summary>
    [SerializeField] int m_health = 100;
    public int Health { get => m_health; }
    /// <summary> プレイヤーの命 </summary>
    public int m_life = 1;
    /// <summary>プレイヤーの魔力</summary>
    [SerializeField] int m_mana = 80;
    public int Mana { get => m_mana; }
    /// <summary>プレイヤーのスタミナ</summary>
    [SerializeField] int m_stamina = 100;
    public int Stamina { get => m_stamina; }


    public void TakeDamage(int damage)
    {
        this.m_health -= damage;
        if (m_health <= 0)
        {
            /*処理*/
            Debug.Log("ダウン！");
        }
    }

    public void RestoreHealth(int health)
    {
        this.m_health += health;
    }

    public void DecrementMana(int cost)
    {
        this.m_health -= cost;
    }

    public void RestoreMana(int mana)
    {
        this.m_health += mana;
    }

    public void DecrementStamina(int cost)
    {
        this.m_health -= cost;
    }

    public void RestoreStamina(int stamina)
    {
        this.m_health += stamina;
    }
}
