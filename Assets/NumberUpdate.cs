using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberUpdate : MonoBehaviour
{
    private float elapse;
    private float time = 0;
    private float floatNum;
    private int intNum;
    private Text text;
    private bool IsStart;
    private bool IsFloat;
    private void Awake()
    {
        IsStart = false;
        time = 0;
        elapse = 0;
        text = this.GetComponent<Text>();
    }
    private void Update()
    {
        StartNumberLerpByTime();
    }
    public void StartUpdateNum(Text text,float elapse = 1)
    {
        if (text.text.Contains('.'))
        {
            floatNum = float.Parse(text.text);
            IsFloat = true;
        }
        else
        {
            intNum = int.Parse(text.text);
            IsFloat = false;
        }
        IsStart = true;
    }
    private void StartNumberLerpByTime() 
    {
        if (IsStart) {
            if (IsFloat) {
                if (time < elapse)
                {
                    float temp = time / elapse;
                    float currentNum = Mathf.Lerp(0, floatNum, temp);
                    text.text = currentNum.ToString();
                    time += Time.deltaTime;
                }
                else
                {
                    text.text = floatNum.ToString();
                    IsStart = false;
                }
            }
            else
            {
                if (time < elapse)
                {
                    float temp = time / elapse;
                    float currentNum = Mathf.Lerp(0, floatNum, temp);
                    string tempStr = currentNum.ToString();
                    text.text = tempStr.Substring(tempStr.IndexOf('.') + remainBit);
                    time += Time.deltaTime;
                }
                else
                {
                    text.text = floatNum.ToString();
                    IsStart = false;
                }
            }
        }
    }
}
