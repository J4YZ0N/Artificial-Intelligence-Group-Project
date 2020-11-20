using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class ChangeTheme : MonoBehaviour
{
    // Variables to store and change level themes
    public LevelThemes plain;
    public LevelThemes desert;
    public LevelThemes cave;
    public LevelThemes ice;
    public LevelThemes lava;
    public LevelThemes grave;
    public LevelThemes fort;

    public LevelThemes currentTheme;
    private List<LevelThemes> themeList;
    private int themeIndex = 0;

    private GameObject[] floor;
    private GameObject field;

    ObstacleSpawner mObstacleSpawner;

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

        currentTheme = plain;

        mObstacleSpawner = GetComponent<ObstacleSpawner>();

        floor = GameObject.FindGameObjectsWithTag("Floor");
        field = GameObject.FindGameObjectWithTag("Field");
    }

    // Changes background theme when called
    public void Change()
    {
        if (themeIndex < 6)
        {
            themeIndex++;
            currentTheme = themeList[themeIndex];
        }
        else 
        {
            themeIndex = 0;
            currentTheme = themeList[themeIndex];
        }

        mObstacleSpawner.ChangeObstacles();

        //RenderSettings.skybox = currentTheme.sky;
        field.GetComponent<MeshFilter>().sharedMesh = currentTheme.field.GetComponent<MeshFilter>().sharedMesh;

        for (int i = 0; i < floor.Length; i++)
        {
            floor[i].GetComponent<MeshFilter>().sharedMesh = currentTheme.ground.GetComponent<MeshFilter>().sharedMesh;
        }
    }
}
