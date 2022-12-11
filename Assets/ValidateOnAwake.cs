using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ValidateOnAwake : MonoBehaviour
{
    private Slider _slider;
    // Start is called before the first frame update
    void Awake()
    {
        _slider = GetComponent<Slider>();
        StartCoroutine(InitSlider(2));
    }

    IEnumerator InitSlider(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        _slider.value = _slider.value;
    }
}
