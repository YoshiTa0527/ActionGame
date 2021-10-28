using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{


    [SerializeField] Image m_fadePanelImage = default;

    public void LoadTargetScene(string targetScene)
    {
        StartCoroutine(FadeCor(targetScene));
    }

    IEnumerator FadeCor(string targetScene)
    {
        m_fadePanelImage.gameObject.SetActive(true);
        Color c = m_fadePanelImage.color;
        c.a = 0f;
        m_fadePanelImage.color = c;

        while (true)
        {
            yield return null;
            c.a += 0.1f;
            m_fadePanelImage.color = c;

            if (c.a >= 1f)
            {
                c.a = 1f;
                m_fadePanelImage.color = c;
                break;
            }
        }
        SceneManager.LoadScene(targetScene);

    }
}
