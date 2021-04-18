//掛載在血條物件上的腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarBehavior : MonoBehaviour
{
    public CatBehavior followTarget; //跟隨目標
    public Vector2 offset; //位置偏移量
    public bool isColorChange; //血條顏色是否隨著血量而變化
    public Color color_full; //滿血時的血條顏色
    public Color color_die; //血量歸零時的血條顏色

    private Slider _slider; //自身Slider組件
    private delegate float ColorValue(float c1, float c2); //算式委派

    public float GetHp { get { return _slider.value; } } //取得目前HP

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        _slider = this.GetComponent<Slider>();
    }

    void Update()
    {
        AutoFollow(); //自動追蹤物件
        AutoColorChange(); //自動變色
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //血條數據初始化
    public void HpBarInitialize()
    {
        _slider.maxValue = followTarget.catMaxHp; //設定最大血量
        _slider.value = followTarget.catIniHp; //設定初始血量
    }

    //自動跟隨物件
    private void AutoFollow()
    {
        transform.localPosition = (Vector2)followTarget.transform.localPosition + offset;
    }


    //血量顏色自動改變
    private void AutoColorChange()
    {
        Image hpImg = _slider.fillRect.GetComponent<Image>();

        if (!isColorChange) //若血條顏色為固定顏色時
        {
            if (hpImg.color != color_full) hpImg.color = color_full;
            return;
        }

        ColorValue _colorValue = (c1, c2) => _slider.value * ( c2 - c1 ) / _slider.maxValue; //

        float _r = _colorValue(color_die.r, color_full.r);
        float _g = _colorValue(color_die.g, color_full.g);
        float _b = _colorValue(color_die.b, color_full.b);
        float _a = _colorValue(color_die.a, color_full.a);

        hpImg.color = new Color(color_die.r + _r, color_die.g + _g, color_die.b + _b, color_die.a + _a);
    }

    //血量傷害
    public void HpDamage(float v)
    {
        bool isDie = false;

        if (_slider.value < -v) isDie = true;
        else isDie = false;

        if (!isDie) _slider.value += v;
        else
        {
            _slider.value = 0;
            StartCoroutine(GameController.Instance.GameOver()); //GameOver程序
        }
    }
}
