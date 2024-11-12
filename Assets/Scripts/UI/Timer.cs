using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Timer : MonoBehaviour
{
    public enum TimeFormat
	{
        MinutesSecondsMilliseconds,
        Seconds
	}
    public TimeFormat timeFormat;
    public bool running = false;
    
    public List<TMP_Text> timerTexts;
    [SerializeField] float time = 0;
    [SerializeField] float tickRate = 1;


    public UnityEvent OnTimeUp;

    public bool outputToLog = false;
    private void Start()
    {
        
    }
    private void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameState.Playing)
        {
            time += tickRate * Time.deltaTime; //time where 1 = 1 second

			foreach (TMP_Text item in timerTexts)
			{
                DisplayTime(item, time, timeFormat);
			}
        }

        //WHY AM I DOING THIS
        if (tickRate < 0 & time > 0)
        {
            time += tickRate * Time.unscaledDeltaTime; //time where 1 = 1 second

            foreach (TMP_Text item in timerTexts)
            {
                DisplayTime(item, time, timeFormat);
            }
            if (time <= 0)
            {
                OnTimeUp.Invoke();
            }
        }
    }

    private void DisplayTime(TMP_Text timerText, float time, TimeFormat format)
	{
        
		switch (format)
		{
			case TimeFormat.MinutesSecondsMilliseconds:
                timerText.text = FloatToMinSecMil(time);
                break;
			case TimeFormat.Seconds:
                timerText.text = Mathf.CeilToInt(time).ToString();
                break;
			default:
				break;
		}
	}

    private string FloatToMinSecMil(float num)
	{
        int minutes = (int)time / 60;
        int seconds = (int)time - 60 * minutes;
        int milliseconds = (int)(time * 100 - minutes * 6000 - seconds * 100); //milliseconds to two digits (factor of 100, not 1000)
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void ToggleTimer()
	{
        running = !running;
	}

    public void DumpTimeToFile()
	{
        string path = $"{Application.streamingAssetsPath}/finishTimes.txt";
        using (StreamWriter sw = File.AppendText(path))
        {
            sw.WriteLine(DateTime.Now.ToString() + ": " + FloatToMinSecMil(time));
        }
        // Open the file to read from.
        using (StreamReader sr = File.OpenText(path))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                Debug.Log(line);
            }
        }
    }
}