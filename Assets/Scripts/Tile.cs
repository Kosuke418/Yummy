using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{

    public int IndCols;
    public int IndRows;

    public int Number
    {
        get
        {
            return number;
        }
        set
        {
            number = value;
            if (number == 0)
            {
                SetEmpty();
                SetMaterial();
            }
            else
            {
                ApplyStyle(number);
                SetVisible();
            }
        }
    }

    private int number;

    private Text TileText;
    private Image TileImage;

    private void Awake()
    {
        TileImage = this.GetComponent<Image>();
    }

    void ApplyStyleFromHolder(int index)
    {
        TileImage.sprite = Library.Instance.Ingreds[index].IngredSprite;
    }

    void ApplyStyle(int num)
    {
        switch (num)
        {
            case 1:
                ApplyStyleFromHolder(1);
                break;
            case 2:
                ApplyStyleFromHolder(2);
                break;
            case 3:
                ApplyStyleFromHolder(3);
                break;
            case 4:
                ApplyStyleFromHolder(4);
                break;
            case 5:
                ApplyStyleFromHolder(5);
                break;
            case 6:
                ApplyStyleFromHolder(6);
                break;
            case 7:
                ApplyStyleFromHolder(7);
                break;
            case 8:
                ApplyStyleFromHolder(8);
                break;
            case 9:
                ApplyStyleFromHolder(9);
                break;
            case 10:
                ApplyStyleFromHolder(10);
                break;
            case 11:
                ApplyStyleFromHolder(11);
                break;
            case 12:
                ApplyStyleFromHolder(12);
                break;
            case 13:
                ApplyStyleFromHolder(13);
                break;
            case 14:
                ApplyStyleFromHolder(14);
                break;
            case 15:
                ApplyStyleFromHolder(15);
                break;
            case 16:
                ApplyStyleFromHolder(16);
                break;
            case 17:
                ApplyStyleFromHolder(17);
                break;
            case 18:
                ApplyStyleFromHolder(18);
                break;
            case 19:
                ApplyStyleFromHolder(19);
                break;
            case 20:
                ApplyStyleFromHolder(20);
                break;
            case 21:
                ApplyStyleFromHolder(21);
                break;
            case 22:
                ApplyStyleFromHolder(22);
                break;
            case 23:
                ApplyStyleFromHolder(23);
                break;
            case 24:
                ApplyStyleFromHolder(24);
                break;
            case 25:
                ApplyStyleFromHolder(25);
                break;
            default:
                Debug.LogError("アプリスタイルのnumberを確認して");
                break;
        }
    }

    private void SetVisible()
    {
        TileImage.enabled = true; 
    }

    private void SetEmpty()
    {
        TileImage.enabled = false;
    }

    void SetMaterial()
    {
        float level = Mathf.Lerp(0, 1, Time.time);
        TileImage.material.SetFloat("_Vector1_D08391B4", level);
    }
}
