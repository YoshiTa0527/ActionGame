using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    /// <summary>プレイヤーの体力</summary>
    [SerializeField] int m_health = 100;
    int m_maxHealth;
    [SerializeField] SliderController m_hpSlider;
    /// <summary> プレイヤーの命 </summary>
    public int m_life = 1;
    /// <summary>プレイヤーの魔力</summary>
    [SerializeField] float m_mana = 80;
    float m_maxMana;
    [SerializeField] SliderController m_mpSlider;
    public float Mana { get => m_mana; }

    /// <summary>スタミナの自動回復が始まる時間</summary>
    [SerializeField] float m_autoRestoreStaminaInterval = 0.5f;
    float m_staminaTimer;
    bool m_staminaRestoreFlag = false;

    private void Start()
    {
        m_maxHealth = m_health;
        m_maxMana = m_mana;
    }

    private void FixedUpdate()
    {

    }

    public void TakeDamage(int damage)
    {
        this.m_health -= damage;
        if (m_health <= 0) m_health = 0;

        m_hpSlider.SliderControl(m_health, m_maxHealth);
    }

    public void RestoreHealth(int health)
    {
        this.m_health += health;
        if (m_health >= m_maxHealth) m_health = m_maxHealth;
        m_hpSlider.SliderControl(m_health, m_maxHealth);
    }

    public void DecrementMana(float cost)
    {
        this.m_mana -= cost;
        if (m_mana <= 0) m_mana = 0;
        m_mpSlider.SliderControl(m_mana, m_maxMana);
    }

    public void RestoreMana(float mana)
    {
        this.m_mana += mana;
        if (m_mana >= m_maxMana) m_mana = m_maxMana;
        m_mpSlider.SliderControl(m_mana, m_maxMana);
    }
}
