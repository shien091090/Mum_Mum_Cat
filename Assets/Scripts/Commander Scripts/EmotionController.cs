//表情特效管理腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionController : MonoBehaviour
{
    [Header("參考物件")]
    public Transform parent; //父物件(貓咪)
    public GameObject scaredEffect; //害怕
    public GameObject dizzy; //倒地暈眩
    public GameObject speedDownEffect; //緩速

    //表情特效設定
    //[input] emotion : 表情種類 / state : 開或關
    public void SetEmotion(EmotionType emotion, bool state)
    {
        switch (emotion)
        {
            case EmotionType.害怕:
                scaredEffect.SetActive(state);
                break;

            case EmotionType.倒地暈眩:
                if (!state) goto SetActive;

                float anchorMin_x = 0; //X軸錨點(最小值)
                float anchorMax_x = 0; //X軸錨點(最大值)
                float pos_x = 0; //X軸位置
                float rot = 0; //旋轉值

                if (parent.localPosition.x >= 0) //貓咪在畫面右邊
                {
                    //Debug.Log("貓咪在畫面右邊");
                    anchorMin_x = 0;
                    anchorMax_x = 0;
                    pos_x = -8.5f;
                    rot = 180;
                }
                else //貓咪在畫面左邊
                {
                    //Debug.Log("貓咪在畫面左邊");
                    anchorMin_x = 1;
                    anchorMax_x = 1;
                    pos_x = 8.5f;
                    rot = 0;
                }

                RectTransform rt = dizzy.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(anchorMin_x, dizzy.GetComponent<RectTransform>().anchorMin.y); //設定X軸錨點(最小值)
                rt.anchorMax = new Vector2(anchorMax_x, dizzy.GetComponent<RectTransform>().anchorMax.y); //設定X軸錨點(最大值)
                rt.anchoredPosition = new Vector2(pos_x, dizzy.transform.localPosition.y); //設定位置
                dizzy.transform.rotation = Quaternion.Euler(0, rot, 0); //圖案翻轉

                SetActive:

                dizzy.SetActive(state);
                break;

            case EmotionType.緩速:
                speedDownEffect.SetActive(state);
                break;
        }
        
    }

    //表情特效重置
    public void ResetEmotion()
    {
        scaredEffect.SetActive(false);
        dizzy.SetActive(false);
    }

}
