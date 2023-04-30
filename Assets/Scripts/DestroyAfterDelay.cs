using UnityEngine;
using System.Collections;
using TMPro;

public class DestroyAfterDelay : MonoBehaviour
{
    public float lifetime;
    public float maxSize = 2;

    private float m_time = 0;
    private bool m_isPlaying = false;
    private Vector3 m_startingScale;

    public void Commence()
    {
        m_isPlaying = true;
        m_time = 0f;
        m_startingScale = transform.localScale;
    }

    private void Update()
    {
        if(m_isPlaying)
        {
            m_time += Time.deltaTime;
            transform.localScale = m_startingScale * (1f + ((maxSize-1f) * (m_time / lifetime)));
            gameObject.GetComponent<TextMeshPro>().color = new Color(m_time / lifetime, m_time / lifetime, m_time / lifetime, m_time / lifetime);

            if (m_time > lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}