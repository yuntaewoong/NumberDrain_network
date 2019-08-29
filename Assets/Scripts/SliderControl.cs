using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SliderControl : MonoBehaviour
{
    private Color[] colors = { Color.blue, Color.green, Color.yellow, Color.red };

    private Slider slider;
    private Image sliderImage;
    private void Start()
    {
        slider = GetComponent<Slider>();
        sliderImage = GetComponentInChildren<Image>();
    }
    private void SliderColorSet()
    {
        if (slider.value == 0)
            sliderImage.color = colors[0];
        else if (slider.value < slider.maxValue / 2)
            sliderImage.color = colors[1];
        else if (slider.value >= slider.maxValue / 2)
            sliderImage.color = colors[2];
        else
            sliderImage.color = colors[3];
    }
    void Update()
    {
        SliderColorSet();
    }
}
