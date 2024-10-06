using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    Slider _slider;
    void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(SliderValueUpdateSound);

        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        
        // Drag 이벤트 찾기
        EventTrigger.Entry dragEntry = eventTrigger.triggers.Find(entry => entry.eventID == EventTriggerType.Drag);

        dragEntry.callback.AddListener((data) => { SliderDragUpdateSound((PointerEventData)data); });
    }

    public void SliderValueUpdateSound(float value)
    {
        SoundManager._instance.UpdateMusicVolume();
    }

    public void SliderDragUpdateSound(PointerEventData data)
    {
        SoundManager._instance.UpdateMusicVolume();
    }
}