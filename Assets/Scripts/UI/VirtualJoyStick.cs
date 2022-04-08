using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;



public class VirtualJoyStick : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    private Image bgImg;
    private Image jsImg;

    private bool isDragged;
    private Vector2 dragInitPos;
    
    [Serializable]//Indicates that a class or a struct can be serialized. (attributes in c# are complicated)
    public class DirectionChangeEvent : UnityEvent<Vector2> { }

    public DirectionChangeEvent directionChageEvents;
    public float sensitivitiy = 0.005f;


    private void Awake()
    {
        bgImg = GetComponent<Image>();
        jsImg = transform.GetChild(0).GetComponent<Image>();
        isDragged = false;
    }


    public void Update()
    {
        if (isDragged)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out pos);

            Vector2 pos_limit = pos.normalized;
            pos_limit.x = Mathf.Abs(pos_limit.x) * bgImg.rectTransform.sizeDelta.x / 2 * 0.8f;
            pos_limit.y = Mathf.Abs(pos_limit.y) * bgImg.rectTransform.sizeDelta.y / 2 * 0.8f;

            pos_limit.x = pos_limit.x == 0 ? 10 : pos_limit.x;
            pos_limit.y = pos_limit.y == 0 ? 10 : pos_limit.y;

            pos.x = Mathf.Clamp(pos.x, -pos_limit.x, pos_limit.x);
            pos.y = Mathf.Clamp(pos.y, -pos_limit.y, pos_limit.y);

            jsImg.rectTransform.anchoredPosition = pos;

            pos = pos / pos_limit * sensitivitiy;

            Debug.Log("called with" + pos.ToString());
            directionChageEvents.Invoke(pos);
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        Vector2 pos;
        isDragged = RectTransformUtility.ScreenPointToLocalPointInRectangle(jsImg.rectTransform, eventData.position, eventData.pressEventCamera, out pos);
        if (isDragged)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, eventData.position, eventData.pressEventCamera, out pos);
            jsImg.rectTransform.anchoredPosition = pos;
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        jsImg.rectTransform.anchoredPosition = Vector3.zero;
        isDragged = false;
    }

}
