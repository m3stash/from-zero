using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


public class CycleDay : MonoBehaviour {

    public int startHour = 8;
    public int durationDayOnMinute;
    [SerializeField]
    private int currentHour = 0;
    private readonly int secondInDay = 86400;
    private int timerDay = 0;
    private int convertedInRealSecond;
    private float intensity = 0f;
    private Tilemap tileLightMap;
    // private List<Material> materials = new List<Material>();

    void Start() {
        tileLightMap = GetComponentInChildren<Tilemap>();
        timerDay = startHour * 60 * 60;
        // sky = GameObject.FindGameObjectWithTag("Sky").GetComponent<SpriteRenderer>();
        ConvertTime();
        StartCoroutine(DayNightCicle());
    }
    public float GetIntensity() {
        // return intensity;
        return 0;
    }
    public void ConvertTime() {
        int durationOnSecond = durationDayOnMinute * 60;
        convertedInRealSecond = secondInDay / durationOnSecond;
    }
    private IEnumerator DayNightCicle() {
        while (true) {
            currentHour = timerDay / 60 / 60;
            SetIntensity(currentHour);
            timerDay = currentHour == 24 ? 0 : timerDay += convertedInRealSecond;
            yield return new WaitForSeconds(1);
        }
    }
    private void SetIntensity(int currentHour) {
        // On divise 1 / 24 afin d'avoir l'intensité par heure = 0.01
        switch (currentHour) {
            case 0:
                intensity = 0.8f;
                break;
            case 1:
                intensity = 0.8f;
                break;
            case 2:
                intensity = 0.8f;
                break;
            case 3:
                intensity = 0.7f;
                break;
            case 4:
                intensity = 0.6f;
                break;
            case 5:
                intensity = 0.5f;
                break;
            case 6:
                intensity = 0.4f;
                break;
            case 7:
                intensity = 0.3f;
                break;
            case 8:
                intensity = 0.2f;
                break;
            case 9:
                intensity = 0.1f;
                break;
            case 17:
                intensity = 0.1f;
                break;
            case 18:
                intensity = 0.2f;
                break;
            case 19:
                intensity = 0.3f;
                break;
            case 20:
                intensity = 0.4f;
                break;
            case 21:
                intensity = 0.5f;
                break;
            case 22:
                intensity = 0.6f;
                break;
            case 23:
                intensity = 0.7f;
                break;
            case 24:
                intensity = 0.8f;
                break;
            default:
                intensity = 0;
                break;
        }
    }

}

