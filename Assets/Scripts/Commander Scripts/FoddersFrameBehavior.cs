//主要處理投擲餌食時, 餌食投擲隊伍的圖像動畫

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoddersFrameBehavior : MonoBehaviour
{
    [Header("參考物件")]
    public Transform imagesParent; //餌食圖像父物件

    [Header("遊戲進行狀態")]
    public bool throwing = false; //投擲中(禁止下次投擲)
    public List<Image> imageList; //餌食圖像清單

    private Animation animation; //動畫器

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        animation = this.GetComponent<Animation>();
    }

    void Start()
    {
        //自動建立餌食圖像清單
        if (imagesParent.childCount <= 0) throw new System.Exception("[ERROR]餌食圖像父物件中不存在任何物件");

        for (int i = 0; i < imagesParent.childCount; i++)
        {
            if (imagesParent.GetChild(i).gameObject.GetComponent<Image>() == null) throw new System.Exception(string.Format("[ERROR]子物件{0}中沒有Image組件", imagesParent.GetChild(i).name));
            imageList.Add(imagesParent.GetChild(i).gameObject.GetComponent<Image>()); //加入至餌食圖像清單
        }

        if (imageList.Count != FodderController.Instance.fodderQueueLength) throw new System.Exception("[ERROR]餌食圖像數量與設定值(fodderQueueLength)不符");
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //餌食圖像初始化
    public void ImageInitialize()
    {
        for (int i = 0; i < imageList.Count; i++)
        {
            FodderType _type = FodderController.Instance.GetFodderQueue[i]; //取得餌食種類
            FodderAttributeData _attribute = FodderController.Instance.dic_attributeList[_type]; //取得該餌食屬性數據
            Sprite _sprite = _attribute.frameLineSprite; //從屬性數據中取得該餌食的圖像對列框圖像

            imageList[i].sprite = _sprite;
        }
    }

    //更換指定圖像
    //[input] imageIndex : 欲更換圖像的index / spriteIndex : 圖片編號index
    public void ImageAssign(int imageIndex, int spriteIndex)
    {
        FodderType _type = FodderController.Instance.GetFodderQueue[spriteIndex]; //取得最後一個餌食種類
        FodderAttributeData _attribute = FodderController.Instance.dic_attributeList[_type]; //取得該餌食屬性數據
        Sprite _sprite = _attribute.frameLineSprite; //從屬性數據中取得該餌食的圖像對列框圖像

        imageList[imageIndex].sprite = _sprite;
    }

    //撥放投擲餌食後的往前替補動畫
    public void PlayAnimation()
    {
        ImageInitialize(); //餌食圖像初始化
        throwing = true; //投擲中(禁止下次投擲)

        animation.Play(); //撥放動畫
    }

    //動畫結束
    public void AnimationEnd()
    {
        throwing = false;
    }
}
