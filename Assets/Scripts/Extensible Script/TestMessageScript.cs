using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 專門用來測試的腳本, 主要針對欲測試對象透過Debug.Log印出
/// </summary>
/// <typeparam name="T"></typeparam>
public static class TestMessageScript<T>
{
    /// <summary>
    /// 印出List中的所有元素
    /// </summary>
    /// <param name="listName">顯示於Debug.Log上的List標題</param>
    /// <param name="list">欲列印出所有元素的List集合</param>
    public static void PrintListElements(string listName , List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Debug.Log(string.Format("<{0}>[{1}] : {2}", listName, i, list[i]));
        }
    }
}
