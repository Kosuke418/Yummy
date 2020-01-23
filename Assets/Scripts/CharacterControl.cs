using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControl : MonoBehaviour
{
    [SerializeField]
    GameObject P1Sprite, P2Sprite;
    float R, G, B, kamenRider, charaTransform, charaCount;

    void Start()
    {
        charaCount = 1;
    }

    void Update()
    {
        if (charaCount > 0)
        {
            charaTransform += 0.1f;
            P1Sprite.transform.eulerAngles = new Vector3(0, 0, -charaTransform);
            P2Sprite.transform.eulerAngles = new Vector3(0, 0, charaTransform);
            if (charaTransform >= 5)
            {
                charaCount = -1;
            }
        }
        else if (charaCount < 0)
        {
            charaTransform -= 0.1f;
            P1Sprite.transform.eulerAngles = new Vector3(0, 0, -charaTransform);
            P2Sprite.transform.eulerAngles = new Vector3(0, 0, charaTransform);
            if (charaTransform <= -10)
            {
                charaCount = 1;
            }
        }
    }
}
