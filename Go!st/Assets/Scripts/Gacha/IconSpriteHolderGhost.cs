using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconSpriteHolderGhost : MonoBehaviour
{
    [SerializeField] Image iconHolderGhost;

    public void ChangeCoinGhostImage()
    {
        iconHolderGhost.color = Color.yellow;
    }
}
