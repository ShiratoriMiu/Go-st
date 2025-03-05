using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHpImage : MonoBehaviour
{
    [SerializeField] GameObject[] hpImages;

    public void UpdateHp(int _hp)
    {
        for(int i = 0; i < hpImages.Length; ++i)
        {
            if(i < _hp)
            {
                hpImages[i].SetActive(true);
            }
            else
            {
                hpImages[i].SetActive(false);
            }
        }
    }
}
