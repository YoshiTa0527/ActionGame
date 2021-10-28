using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SliderController : MonoBehaviour
{
    [SerializeField] Slider m_slider;

    public void SliderControl(float currentValue, float maxValue)
    {
        DOTween.To(() => m_slider.value, v =>
        {
            m_slider.value = v;
        }, (float)currentValue / maxValue, 1f).SetEase(Ease.OutCubic);
    }
}
