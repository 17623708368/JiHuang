using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.EventSystems;
using System;

public static class UITool
{
    /// <summary>
    /// 绑定鼠标效果
    /// </summary>
    public static void BindMouseEffect(this Component component)
    {
        // 鼠标进入后要放大
        component.OnMouseEnter(MouseEffect, component, false, component.transform.localScale);
        // 鼠标离开时恢复正常
        component.OnMouseExit(MouseEffect, component, true, component.transform.localScale);
    }

    public static void RemoveMouseEffect(this Component component)
    {
        // 手动触发一次退出
        JKEventListener jkl = component.GetComponent<JKEventListener>();
        if (jkl!=null) jkl.OnPointerExit(null);

        component.RemoveMouseEnter(MouseEffect);
        component.RemoveMouseExit(MouseEffect);
        GameManager.Instance.SetCursorState(CursorState.Normal);
    }

    /// <summary>
    /// 鼠标效果
    /// </summary>
    private static void MouseEffect(PointerEventData arg1, object[] args)
    {
        Component component = (Component)args[0];
        bool targetIsNoraml = (bool)args[1];
        Vector3 normalScale = (Vector3)args[2];

        // 设置鼠标指针的外观
        if (targetIsNoraml)
        {
            GameManager.Instance.SetCursorState(CursorState.Normal);
        }
        else
        {
            GameManager.Instance.SetCursorState(CursorState.Handle);
        }
        component.StartCoroutine(DoMouseEffect(component, targetIsNoraml, normalScale));
    }

    private static IEnumerator DoMouseEffect(Component component, bool targetIsNoraml, Vector3 normalScale)
    {
        Transform transform = component.transform;
        Vector3 currScale = transform.localScale;
        // 缩小
        if (targetIsNoraml)
        {
            Vector3 targetScale = normalScale;
            while (transform.localScale.x>targetScale.x)
            {
                yield return null;
                if (transform == null) yield break;
                currScale -= Time.deltaTime * 2 * Vector3.one;
                transform.localScale = currScale;
            }
            transform.localScale = targetScale;
        }
        // 放大
        else
        {
            Vector3 targetScale = normalScale * 1.1f;
            while (transform.localScale.x < targetScale.x)
            {
                yield return null;
                if (transform == null) yield break;
                currScale += Time.deltaTime * 2 * Vector3.one;
                transform.localScale = currScale;
            }
            transform.localScale = targetScale;
        }
    }
}
