using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;



public class VirtualSlider : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    private Image bgImg;
    private Image jsImg;

    private bool isDragged;
    
    [Serializable]//Indicates that a class or a struct can be serialized. (attributes in c# are complicated)
    public class DirectionChangeEvent : UnityEvent<float> { }

    public DirectionChangeEvent directionChageEvents;

    [SerializeField]
    float sensitivity = 5.0f;

    Camera cam;

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

            float pos_delta_x = bgImg.rectTransform.sizeDelta.x / 2 * 0.8f;
            float pivot_x = (0.5f-bgImg.rectTransform.pivot.x) * bgImg.rectTransform.sizeDelta.x;
            
            pos.x = Mathf.Clamp(pos.x, pivot_x-pos_delta_x, pivot_x+pos_delta_x);

            jsImg.rectTransform.localPosition = new Vector2(pos.x, jsImg.rectTransform.localPosition.y);

            float value = Mathf.Clamp(jsImg.rectTransform.anchoredPosition.x / pos_delta_x, -1, 1 ) * sensitivity;

            directionChageEvents.Invoke(value);
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("Called");
        cam = eventData.pressEventCamera;
        Vector2 pos;
        isDragged = RectTransformUtility.ScreenPointToLocalPointInRectangle(jsImg.rectTransform, eventData.position, GetComponentInParent<Canvas>().worldCamera, out pos);//TODO doesn't work
        //Debug.Log(isDragged);
        //Debug.Log("RecTransform of bg:");
        //Debug.Log("anchorMin: \n" + bgImg.rectTransform.anchorMin.ToString());
        //Debug.Log("anchorMax: \n" + bgImg.rectTransform.anchorMax.ToString());
        //Debug.Log("sizeDelta: \n" + bgImg.rectTransform.sizeDelta.ToString());
        //Debug.Log("pivot: \n" + bgImg.rectTransform.pivot.ToString());
        //Debug.Log("localPosition: \n" + bgImg.rectTransform.localPosition.ToString());
        //Debug.Log("anchoredPosition: \n" + bgImg.rectTransform.anchoredPosition.ToString());
        //Debug.Log("RecTransform of js:");
        //Debug.Log("anchorMin: \n" + jsImg.rectTransform.anchorMin.ToString());
        //Debug.Log("anchorMax: \n" + jsImg.rectTransform.anchorMax.ToString());
        //Debug.Log("sizeDelta: \n" + jsImg.rectTransform.sizeDelta.ToString());
        //Debug.Log("pivot: \n" + jsImg.rectTransform.pivot.ToString());
        //Debug.Log("localPosition: \n" + jsImg.rectTransform.localPosition.ToString());
        //Debug.Log("anchoredPosition: \n" + jsImg.rectTransform.anchoredPosition.ToString());
        //Debug.Log("click position: \n" + pos.ToString());
        if (isDragged)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, eventData.position, GetComponentInParent<Canvas>().worldCamera, out pos);
            //Debug.Log("click position: \n" + pos.ToString());
            jsImg.rectTransform.localPosition = new Vector2(pos.x, jsImg.rectTransform.localPosition.y);
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        jsImg.rectTransform.anchoredPosition = Vector3.zero;
        isDragged = false;
    }

}
