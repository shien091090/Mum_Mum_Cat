//掛載在提示文字上, 主要用途為隨著目標(聚光燈物件)的尺寸、位置不同, 自動調整文字顯示的位置, 使文字不會超出畫面或是出現在不直觀的位置上

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintTextBehavior : MonoBehaviour
{
    [Header("參考組件")]
    public RectTransform parentRect; //父物件範圍(文字範圍不可超過此Canvas範圍)
    public RectTransform targetRect; //參考目標範圍(文字位置隨著此Canvas移動)
    [Header("文字顯示參數")]
    public bool isAutoPos = false; //自動定位與否
    public bool verticalReverse = false; //文字顯示位置(false=以顯示於下方為主; true=以顯示於上方為主)
    public float yAxisDistance; //文字與targetCanvas之間的Y軸間隔距離
    public float xAxisOffsetAddition; //文字的X軸偏移量加成
    public float reservedSpace_Y; //文字與Y軸邊界之間的預留空間, 使文字在即將接觸邊界時可以更早反轉位置
    public float reservedSpace_X; //文字與X軸邊界之間的預留空間, 使文字在即將接觸邊界時可以更早固定位置不再移動

    private RectTransform rectTransform; //此物件的RectTransform組件
    private Text text; //此物件的Text組件

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();
        text = this.GetComponent<Text>();
    }

    void Update()
    {
        //KeyListen();
        if (isAutoPos) AutoPos();
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //按鍵監聽
    private void KeyListen()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(string.Format("RectTransform : Position[{0}, {1}, {2}], LocalPosition[{3}, {4}, {5}]", rectTransform.position.x, rectTransform.position.y, rectTransform.position.z, rectTransform.localPosition.x, rectTransform.localPosition.y, rectTransform.localPosition.z));
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Vector2 size = parentRect.sizeDelta;
            Debug.Log("Canvas[" + size.x + ", " + size.y + "]");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isAutoPos = !isAutoPos;
        }
    }

    //設置文字內容
    //content = 文字內容
    //haveSpotlight = 是否存在聚光燈, 若無, 則文字顯示於畫面中間
    public void SetTextContent(string content, bool haveSpotlight)
    {
        if (!this.gameObject.activeSelf) this.gameObject.SetActive(true); //顯示物件

        text.text = content; //透用文字內容
        if (haveSpotlight) AutoPos(); //若存在聚光燈, 則按照參數設定位置
        else rectTransform.localPosition = Vector2.zero; //若不存在聚光燈, 則文字顯示於畫面中間
    }

    //自動設定位置
    private void AutoPos()
    {
        float offset_y; //Y軸位移量
        float offset_x; //X軸位移量

        offset_y = ( targetRect.sizeDelta.y / 2f ) + ( rectTransform.sizeDelta.y / 2f ) + yAxisDistance; //文字Y軸位移量為聚光燈範圍長/2 + 文字範圍長/2 + 可調整距離

        //Y軸位置處理
        if (verticalReverse) //文字優先顯示於上方
        {
            //若文字會超出上方邊界, 則改成顯示於下方
            if (( targetRect.localPosition.y + offset_y + ( rectTransform.sizeDelta.y / 2f ) + reservedSpace_Y ) > ( parentRect.sizeDelta.y / 2f )) offset_y *= -1;
        }
        else //文字優先顯示於下方
        {
            if (( targetRect.localPosition.y - offset_y - ( rectTransform.sizeDelta.y / 2f ) - reservedSpace_Y ) > -( parentRect.sizeDelta.y / 2f )) offset_y *= -1;
        }

        //X軸位置處理
        offset_x = targetRect.localPosition.x * xAxisOffsetAddition;

        if (( targetRect.localPosition.x * xAxisOffsetAddition ) + ( rectTransform.sizeDelta.x / 2f ) + reservedSpace_X > ( parentRect.sizeDelta.x / 2f )) //若右邊超出邊界時, 固定位置不再往右移動
        {
            offset_x = ( parentRect.sizeDelta.x / 2f ) - ( rectTransform.sizeDelta.x / 2f ) - reservedSpace_X;
        }

        if (( targetRect.localPosition.x * xAxisOffsetAddition ) - ( rectTransform.sizeDelta.x / 2f ) - reservedSpace_X < -( parentRect.sizeDelta.x / 2f )) //若左邊超出邊界時, 固定位置不再往左移動
        {
            offset_x = -( parentRect.sizeDelta.x / 2f ) + ( rectTransform.sizeDelta.x / 2f ) + reservedSpace_X;
        }

        //顯示最終位置
        rectTransform.localPosition = new Vector2(offset_x, targetRect.localPosition.y + offset_y);
    }
}
