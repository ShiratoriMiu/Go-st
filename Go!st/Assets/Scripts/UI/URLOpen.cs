using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLOpen : MonoBehaviour
{
    // ”CˆÓ‚ÌURL‚ðŽw’è
    [SerializeField] private string url = "https://example.com";

    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
