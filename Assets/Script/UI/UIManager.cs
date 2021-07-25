using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayerStatus m_status;
    [SerializeField] Slider m_healthSlider;
    [SerializeField] Slider m_manaSlider;
    [SerializeField] Slider m_staminahSlider;

    private void OnDecreaseParam(Slider targetSlider, int currentValue, int maxValue)
    {
        DOTween.To(() => targetSlider.value, v =>
        {
            if (v <= 0)
            {
                Destroy(this.gameObject);
            }
            targetSlider.value = v;
        }, (float)currentValue / maxValue, 1f).SetEase(Ease.OutCubic);
    }

}
