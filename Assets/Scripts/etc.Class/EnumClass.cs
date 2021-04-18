//餌食掉到地上時的事件
public enum FodderCollisionType
{
    消失, 滯留
}

//貓咪狀態列表
public enum CatStateNames
{
    行走, 跳躍, 跌倒, 助跑起跳, 害怕, 速度變化, 追蹤
}

//拮抗方式
public enum AntagonismType
{
    無, 銷毀, 凍結, 隱藏
}

//碰撞目標種類
public enum FodderCollision
{
    地面, 貓
}

//餌食種類
public enum FodderType
{
    無, 普通魚, 好的魚, 高級魚, 雞腿, 毒香菇, 泥巴, 毒藥, 美味罐罐, 貓菇, 烏龜罐罐, 運動飲料
}

//特殊餌食標籤
public enum FodderSpecialTag
{
    無, 香味
}

//關卡事件
public enum StageEventType
{
    watatu君現身, 雲霄飛車, 下雨天, 小狗搶食物
}

//事件觸發條件種類
public enum EventConditionType
{
    每隔定量遊戲時間, 總遊戲時間, 此關卡經過時間, 總累計分數, 此關卡分數進度百分比, 此關卡累計分數
}

//表情種類
public enum EmotionType
{
    害怕, 倒地暈眩, 緩速
}

//移動方向
public enum MoveDirection
{
    往左, 往右, 不移動
}

//旋轉方式
public enum RotationType
{
    旋轉, 搖擺
}

//粒子特效種類
public enum ParticleEffectType
{
    吃_特殊, 吃_一般, 跳躍
}