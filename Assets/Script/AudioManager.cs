using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip[] m_soundEffects;
    AudioSource m_audioSource;

    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    public void PlaySE(string name) => m_audioSource.PlayOneShot(m_soundEffects.Where(se => se.name == name).FirstOrDefault());

    public void PlaySE(AudioClip clip) => m_audioSource.PlayOneShot(clip);
}

