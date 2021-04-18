//掛載在餌食物件上，餌食物件誕生時所自動表現出的基本行為

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 掛載在餌食物件上，餌食物件誕生時所自動表現出的基本行為
/// </summary>
public class FodderBehavior : MonoBehaviour
{
    [Header("參考物件")]
    public GameObject testSign; //測試用標記

    [SerializeField]
    private FodderAttributeData attribute; //屬性數據
    public FodderAttributeData GetAttribute { get { return attribute; } } //取得屬性數據

    private float originX; //原X軸位置
    private float timer; //計時器
    private float rotateTimer; //旋轉計時器
    private Rigidbody2D rb; //物件剛體
    private Image img; //圖像(Image)
    private BoxCollider2D collider; //碰撞體

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
        img = this.GetComponent<Image>();
        collider = this.GetComponent<BoxCollider2D>();

        rb.simulated = false; //Rigidbody暫不作用
    }

    //畫面更新時的物理移動
    void Update()
    {
        if (attribute == null) return; //若不存在屬性數據則不進行任何動作

        if (GameController.Instance.GetPlayingState)
        {
            Fall(); //若遊戲為進行狀態時, 餌食發生墜落
            if (attribute.rotationCurve.length > 1) Rotate(); //有設定旋轉曲線時, 產生旋轉
        }

    }

    //碰撞發生時
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor")) CollisionEvent(collision, FodderCollision.地面); //碰撞地面
        if (collision.gameObject.CompareTag("Cat")) CollisionEvent(collision, FodderCollision.貓); //接觸貓咪
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //設定屬性數據
    public void SetAttribute(FodderAttributeData att)
    {
        attribute = att;

        if (att == null) //若設定屬性為null
        {
            rb.simulated = false; //使RigidBody失效
            img.sprite = null;
        }
        else
        {
            rb.simulated = true;
            img.sprite = att.imageSprite; //設定圖像
            img.SetNativeSize(); //設定圖像尺寸
            collider.size = att.colliderSize; //設定碰撞體尺寸
            this.transform.rotation = Quaternion.identity; //初始化旋轉值
            originX = this.transform.position.x; //紀錄原X軸位置

            timer = 0; //計時器歸零
            rotateTimer = 0; //旋轉計時器歸零
        }
    }

    //墜落效果
    private void Fall()
    {
        //計時器推進(若超過曲線最終時間則設為最終時間)
        timer = Mathf.Clamp(timer + Time.deltaTime, 0, attribute.fallCurve.keys[attribute.fallCurve.keys.Length - 1].time);

        float _height = Mathf.Lerp(FodderController.Instance.Anchor_Bottom.position.y, FodderController.Instance.Anchor_Vertex.position.y, attribute.fallCurve.Evaluate(timer)); //此刻墜落高度(Lerp 0=地面位置, 1=生成位置(天上)) 曲線(fallCurve) time=0時為1, Value往0遞減(這樣曲線看起來比較直觀)
        float _shake = Mathf.Lerp(UIManager.Instance.EdgePos_x.x, UIManager.Instance.EdgePos_x.y, attribute.shakeCurve.Evaluate(timer)); //此刻震盪幅度(Lerp 0=畫面左端, 1=畫面右端, 依據目前X軸位置加算X軸位移幅度)

        rb.MovePosition(new Vector2(originX + _shake, _height));
    }

    //旋轉效果
    private void Rotate()
    {
        float v = attribute.rotationCurve.Evaluate(rotateTimer);
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, v);

        rotateTimer += Time.deltaTime;
        if (rotateTimer >= attribute.rotationCurve.keys[attribute.rotationCurve.keys.Length - 1].time)
        {
            float _t = rotateTimer - attribute.rotationCurve.keys[attribute.rotationCurve.keys.Length - 1].time;
            rotateTimer = _t;
        }
    }

    //物件碰撞效果
    private void CollisionEvent(Collision2D collision, FodderCollision type) //碰撞效果
    {
        switch (type) //碰撞
        {
            case FodderCollision.地面: //與地面碰撞的情況

                if (!this.gameObject.activeSelf) return; //若物件已經被關閉則直接結束程序

                switch (attribute.fodderCollisionType)
                {
                    case FodderCollisionType.消失:
                        FodderDestroy();
                        break;

                    case FodderCollisionType.滯留:
                        break;
                }

                break;

            case FodderCollision.貓: //與貓咪接觸的情況

                if (!this.gameObject.activeSelf) return; //若物件已經被關閉則直接結束程序

                CatBehavior cat = collision.gameObject.GetComponent<CatBehavior>();

                bool _bonus = false; //是否有跳躍加成

                if (cat.StateExistenceTest(CatStateNames.跳躍, out _) && cat == GameController.Instance.cat) //跳躍狀態 且 吃餌食的是主人公貓 會出現特別特效
                {
                    ParticleEffectController.Instance.ShowParticleEffect(ParticleEffectType.吃_特殊, collision.GetContact(0).point, true);
                    _bonus = true;
                }
                else //一般吃餌食特效
                {
                    ParticleEffectController.Instance.ShowParticleEffect(ParticleEffectType.吃_一般, collision.GetContact(0).point, true);
                }

                if (!GameController.Instance.gameOver) cat.EatFodder(this, attribute.fodderEffectAction, _bonus); //吃餌食(餌食效果套用)

                FodderDestroy(); //餌食消失

                break;
        }
    }

    //餌食消除
    public void FodderDestroy()
    {
        FodderController.Instance.FallingFodderIncrease(this.transform, false); //從落下中餌食列表中消除
        //if (testSign.activeSelf)
        //{
        //    GameController.Instance.cat.SetFollowTarget(); //若消除者為最近餌食, 則立即找到下一個最近餌食
        //}
        //Destroy(this.gameObject);

        SetAttribute(null);
        this.gameObject.SetActive(false);
        this.testSign.SetActive(false);
    }

}
