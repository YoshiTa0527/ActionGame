using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DepthofFieldController : MonoBehaviour
{
    PostProcessVolume m_postProcessVolume;
    PostProcessProfile m_postProcessProfile;
    float m_defaultFocalLength;
    [SerializeField] float m_maxFocalLength = 60f;
    DepthOfField m_depth;
    bool m_isWorking = false;

    private void Start()
    {
        m_postProcessVolume = GetComponent<PostProcessVolume>();

        m_postProcessProfile = m_postProcessVolume.profile;

        m_depth = m_postProcessProfile.GetSetting<DepthOfField>();

        m_defaultFocalLength = m_depth.focalLength.GetValue<float>();
    }

    private void Update()
    {


    }

    public void EnableFocalDepth()
    {
        Debug.Log("EnableFocalDepth()");
        StartCoroutine(EnableFocalDepthCor());
    }

    IEnumerator EnableFocalDepthCor()
    {
        m_depth.focalLength.Override(m_maxFocalLength);
        m_depth.focusDistance.Override(m_maxFocalLength);
        yield return new WaitForSecondsRealtime(TimeScaleManager.m_hitStopDuration);
        m_depth.focalLength.Override(m_defaultFocalLength);
    }
}
