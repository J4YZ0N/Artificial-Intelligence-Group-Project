using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class ChangeTheme : MonoBehaviour
{
    // Variables to store level themes
    public LevelThemes plain;
    public LevelThemes desert;
    public LevelThemes cave;
    public LevelThemes ice;
    public LevelThemes lava;
    public LevelThemes grave;
    public LevelThemes fort;

    private LevelThemes currentTheme;
    private int themeIndex = 0;
    private List<LevelThemes> themeList;

    private GameObject[] floor;
    private GameObject field;

    void Start()
    {
        themeList = new List<LevelThemes>();
        themeList.Add(plain);
        themeList.Add(desert);
        themeList.Add(cave);
        themeList.Add(ice);
        themeList.Add(lava);
        themeList.Add(grave);
        themeList.Add(fort);

        floor = GameObject.FindGameObjectsWithTag("Floor");
        field = GameObject.FindGameObjectWithTag("Field");
    }

    // Update is called once per frame
    public void Change()
    {
        if (themeIndex < 5)
        {
            themeIndex++;
            currentTheme = themeList[themeIndex];
        }
        else 
        {
            themeIndex = 0;
            currentTheme = themeList[themeIndex];
        }

        RenderSettings.skybox = currentTheme.sky;
        field.GetComponent<MeshFilter>().sharedMesh = currentTheme.field.GetComponent<MeshFilter>().sharedMesh;

        for (int i = 0; i < floor.Length; i++)
        {
            floor[i].GetComponent<MeshFilter>().sharedMesh = currentTheme.ground.GetComponent<MeshFilter>().sharedMesh;
        }
    }
}
