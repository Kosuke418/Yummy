using UnityEngine;

public class CharacterControl : MonoBehaviour
{
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
            transform.eulerAngles = new Vector3(0, 0, -charaTransform);
            if (charaTransform >= 5)
            {
                charaCount = -1;
            }
        }
        else if (charaCount < 0)
        {
            charaTransform -= 0.1f;
            transform.eulerAngles = new Vector3(0, 0, -charaTransform);
            if (charaTransform <= -10)
            {
                charaCount = 1;
            }
        }
    }
}
