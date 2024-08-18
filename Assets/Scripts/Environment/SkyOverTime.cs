using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyOverTime : MonoBehaviour
{
    private Material material;
    int dateMonth = 0;
    string season = null;
    int dateHour = 0;
    private float offsetX = 0.0f;

    public GameObject _pointLight;

    void Start()
    {
        if (material == null)
        {
            Renderer objectRenderer = GetComponent<Renderer>();
            material = objectRenderer.material;
        }
        Time();
        Season();
    }

    void Time()
    {
        // Calling the current hour(int)
        dateHour = DateTime.Now.Hour;

        // Spring, Fall 
        if (season == "springNfall")
        {
            if (dateHour == 6 || dateHour == 7)
            {
                offsetX = 0.7f; // Before dawn
            }
            else if (dateHour > 7 && dateHour < 18)
            {
                offsetX = 0.9f; // Day
            }
            else if (dateHour == 18)
            {
                offsetX = 0.1f; // Before sunset
            }
            else if (dateHour == 19)
            {
                offsetX = 0.2f; // Sunset
            }
            else
            {
                offsetX = 0.5f; // Night
            }
        }

        // Summer
        else if (season == "summer")
        {
            if (dateHour == 5 || dateHour == 6)
            {
                offsetX = 0.7f;
            }
            else if (dateHour > 6 && dateHour < 19)
            {
                offsetX = 0.9f;
            }
            else if (dateHour == 19)
            {
                offsetX = 0.1f;
            }
            else
            {
                offsetX = 0.5f;
            }
        }

        // Winter
        else
        {
            if (dateHour == 7 || dateHour == 8)
            {
                offsetX = 0.7f;
            }
            else if (dateHour > 8 && dateHour < 16)
            {
                offsetX = 0.9f;
            }
            else if (dateHour == 16)
            {
                offsetX = 0.1f;
            }
            else if (dateHour == 17)
            {
                offsetX = 0.3f; // Sunset
            }
            else
            {
                offsetX = 0.5f;
            }
        }

        material.SetTextureOffset("_MainTex", new Vector2(offsetX, 0.0f));

        if (offsetX == 0.9f) // If day, turn off _pointLight.
        {
            if (SceneManager.GetActiveScene().name == "TitleScene") return;

            _pointLight.SetActive(false);
        }
    }

    void Season()
    {
        // Calling the current month(int)
        dateMonth = DateTime.Now.Month;

        if (dateMonth > 1 && dateMonth < 5 || dateMonth > 7 && dateMonth < 11)
        {
            season = "springNfall";
        }
        else if (dateMonth > 4 && dateMonth < 8)
        {
            season = "summer";
        }
        else
        {
            season = "winter";
        }
    }
}
