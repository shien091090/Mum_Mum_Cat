//粒子特效管理腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectController : MonoBehaviour
{
    private static ParticleEffectController _instance; //單例物件
    public static ParticleEffectController Instance { get { return _instance; } } //取得單例物件

    [Header("參考物件")]
    public RectTransform parentRect; //父物件區域(Rect)

    [System.Serializable]
    public struct ParticleEffectObject
    {
        public ParticleEffectType effectName; //效果名稱
        public GameObject prefab; //帶有Particle System的預置體
    }
    [Header("可自訂參數")]
    public List<ParticleEffectObject> particleEffectList = new List<ParticleEffectObject>(); //粒子特效列表

    private List<List<GameObject>> particleEffectPool = new List<List<GameObject>>(); //物件池(不同的ParticleEffectType各有一串物件池List)

    public Dictionary<ParticleEffectType, GameObject> dic_effectPrefab = new Dictionary<ParticleEffectType, GameObject>(); //(字典)從ParticleEffectType取得Prefab(GameObject)
    public Dictionary<ParticleEffectType, int> dic_effectTypeIndex = new Dictionary<ParticleEffectType, int>(); //(字典)從ParticleEffectType取得物件池所在的陣列索引

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Start()
    {
        for (int i = 0; i < particleEffectList.Count; i++) //建立字典 : 從ParticleEffectType取得Prefab(GameObject)
        {
            dic_effectPrefab.Add(particleEffectList[i].effectName, particleEffectList[i].prefab);
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //粒子特效演出
    //[input] type = 特效種類 , pos = 位置, isWorld = true:世界座標/false:本地座標
    public void ShowParticleEffect(ParticleEffectType type, Vector2 pos, bool isWorld)
    {
        GameObject go = ExtractFromPool(type); //從物件池中取得物件

        if(isWorld) go.transform.position = pos; //指定特效發生位置
        else go.transform.localPosition = pos; //指定特效發生位置

        go.GetComponent<ParticleSystem>().Play(); //執行特效
    }

    //從物件池中抽取物件
    public GameObject ExtractFromPool(ParticleEffectType effectType)
    {
        if (!dic_effectTypeIndex.ContainsKey(effectType)) //若指定的粒子特效尚未存在物件池
        {
            int _index = particleEffectPool.Count; //註冊一個索引值給預備建立的特效種類
            dic_effectTypeIndex.Add(effectType, _index); //建立字典儲存 ParticleEffectType ⇔ 索引值

            particleEffectPool.Add(new List<GameObject>()); //拓寬物件池, 用以儲存新的特效種類物件池
        }

        GameObject go = null;

        UnityEngine.Events.UnityAction CreateNew = new UnityEngine.Events.UnityAction(() => //Lambda:創立新物件&加長物件池陣列
        {
            go = Instantiate(dic_effectPrefab[effectType], parentRect); //建立新物件
            go.name += "_" + ( particleEffectPool[dic_effectTypeIndex[effectType]].Count + 1 ).ToString();
            particleEffectPool[dic_effectTypeIndex[effectType]].Add(go);
        });

        if (particleEffectPool[dic_effectTypeIndex[effectType]].Count == 0) CreateNew(); //若物件池中沒有任何物件
        else //若物件中存在物件
        {
            int index = -1; //物件池陣列索引
            for (int i = 0; i < particleEffectPool[dic_effectTypeIndex[effectType]].Count; i++)
            {
                if (!particleEffectPool[dic_effectTypeIndex[effectType]][i].GetComponent<ParticleSystem>().isPlaying) //若物件池中有找到未激活(!isPlaying)物件, 則設定其物件所在索引
                {
                    index = i;
                    break;
                }
            }

            if (index < 0) CreateNew(); //若物件池中的物件都在激活狀態, 則創立新物件
            else go = particleEffectPool[dic_effectTypeIndex[effectType]][index]; //若物件池中有物件處於未激活(!isPlaying), 則設定回傳該物件
        }

        return go;
    }
}
