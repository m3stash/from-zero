using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class LightService : MonoBehaviour {

    // private Tilemap tilemapLight;
    private Tilemap tilemapShadow;
    private int[,] tilesWorldMap;
    // private float[,] tilesLightMap;
    private float[,] tilesShadowMap;
    private int[,] wallTilesMap;
    private CycleDay cycleDay;

    public void Init(Tilemap _tilemapLight, int[,] _tilesWorldMap, float[,] _tilesLightMap, int[,] _wallTilesMap, Tilemap _tilemapShadow, float[,] _tilesShadowMap, CycleDay _cycleDay) {
        // tilemapLight = _tilemapLight;
        tilesWorldMap = _tilesWorldMap;
        // tilesLightMap = _tilesLightMap;
        wallTilesMap = _wallTilesMap;
        tilemapShadow = _tilemapShadow;
        tilesShadowMap = _tilesShadowMap;
        cycleDay = _cycleDay;
    }
    public void RecursivAddNewLight(int x, int y, float lastLight, Tilemap tilemapLight, float[,] tilesLightMap) {
        if (IsOutOfBound(x, y))
            return;
        float newLight = GetAmountLight(tilesWorldMap[x, y], wallTilesMap[x, y], lastLight);
        if ((newLight >= tilesLightMap[x, y]) || newLight >= 1)
            return;
        tilesLightMap[x, y] = newLight;
        SetTilemapOpacity(tilemapLight, tilemapShadow, tilesShadowMap, tilesLightMap, x, y);
        RecursivAddNewLight(x + 1, y, newLight, tilemapLight, tilesLightMap);
        RecursivAddNewLight(x, y + 1, newLight, tilemapLight, tilesLightMap);
        RecursivAddNewLight(x - 1, y, newLight, tilemapLight, tilesLightMap);
        RecursivAddNewLight(x, y - 1, newLight, tilemapLight, tilesLightMap);
    }
    public void RecursivDeleteShadow(int x, int y, Tilemap tilemapShadow, float[,] tilesLightMap, Tilemap tilemapLight) {
        if (IsOutOfBound(x, y))
            return;
        var wallTileMap = wallTilesMap[x, y];
        var tileWorldMap = tilesWorldMap[x, y];
        var tileLightMap = tilesLightMap[x, y];
        var tileShadowMap = tilesShadowMap[x, y];
        var shadowOpacity = GetNeightboorMinOrMaxOpacity(tilesShadowMap, x, y, false);
        float newShadow = GetAmountLight(tileWorldMap, wallTileMap, shadowOpacity);
        var lightOpacity = GetNeightboorMinOrMaxOpacity(tilesLightMap, x, y, false);
        float newLight = GetAmountLight(tileWorldMap, wallTileMap, lightOpacity);
        if (newShadow >= tileShadowMap && newLight >= tileLightMap)
            return;
        if (newShadow < tileShadowMap) {
            tilesShadowMap[x, y] = newShadow;
        }
        if (newLight < tileLightMap) {
            tilesLightMap[x, y] = newLight;
        }
        SetTilemapOpacity(tilemapLight, tilemapShadow, tilesShadowMap, tilesLightMap, x, y);
        RecursivDeleteShadow(x + 1, y, tilemapShadow, tilesLightMap, tilemapLight);
        RecursivDeleteShadow(x, y + 1, tilemapShadow, tilesLightMap, tilemapLight);
        RecursivDeleteShadow(x - 1, y, tilemapShadow, tilesLightMap, tilemapLight);
        RecursivDeleteShadow(x, y - 1, tilemapShadow, tilesLightMap, tilemapLight);
    }
    public void RecursivDeleteLight(int x, int y, Tilemap tilemapLight, float[,] tilesLightMap, GameObject[,] tilesObjetMap, bool toDelete) {
        if (IsOutOfBound(x, y))
            return;
        var minLight = GetNeightboorMinOrMaxOpacity(tilesLightMap, x, y, false);
        float newLight = GetAmountLight(tilesWorldMap[x, y], wallTilesMap[x, y], minLight);
        if (newLight <= tilesLightMap[x, y] && !toDelete || !toDelete && tilesLightMap[x, y] == 0.15f) // toDo detecter 
            return;
        tilesLightMap[x, y] = newLight;
        SetTilemapOpacity(tilemapLight, tilemapShadow, tilesShadowMap, tilesLightMap, x, y);
        RecursivDeleteLight(x + 1, y, tilemapLight, tilesLightMap, tilesObjetMap, false);
        RecursivDeleteLight(x, y + 1, tilemapLight, tilesLightMap, tilesObjetMap, false);
        RecursivDeleteLight(x - 1, y, tilemapLight, tilesLightMap, tilesObjetMap, false);
        RecursivDeleteLight(x, y - 1, tilemapLight, tilesLightMap, tilesObjetMap, false);
    }
    public void RecursivAddShadow(int x, int y, float[,] tilesLightMap, Tilemap tilemapLight) {
        var tileWorldMap = tilesWorldMap[x, y];
        var wallTileMap = wallTilesMap[x, y];
        var tileLightMap = tilesLightMap[x, y];
        if (IsOutOfBound(x, y) || (tileWorldMap == 0 && wallTileMap == 0 || tileLightMap == 0.15f)) // toDo voir à faire ça autrement ? => lightOpacity == 0.15f
            return;
        var tileShadowMap = tilesShadowMap[x, y];
        var shadowOpacity = GetNeightboorMinOrMaxOpacity(tilesShadowMap, x, y, false);
        float newShadow = GetAmountLight(tileWorldMap, wallTileMap, shadowOpacity);
        var lightOpacity = GetNeightboorMinOrMaxOpacity(tilesLightMap, x, y, false);
        float newLight = GetAmountLight(tileWorldMap, wallTileMap, lightOpacity);
        if (newLight <= tileLightMap && newShadow <= tileShadowMap)
            return;
        if (newShadow > tileShadowMap) {
            tilesShadowMap[x, y] = newShadow;
        }
        if (newLight > tileLightMap) {
            tilesLightMap[x, y] = newLight;
        }
        SetTilemapOpacity(tilemapLight, tilemapShadow, tilesShadowMap, tilesLightMap, x, y);
        RecursivAddShadow(x + 1, y, tilesLightMap, tilemapLight);
        RecursivAddShadow(x, y + 1, tilesLightMap, tilemapLight);
        RecursivAddShadow(x - 1, y, tilesLightMap, tilemapLight);
        RecursivAddShadow(x, y - 1, tilesLightMap, tilemapLight);
    }
    private void SetTilemapOpacity(Tilemap tilemapLight, Tilemap tilemapShadow, float[,] tilesShadowMap, float[,] tilesLightMap, int x, int y) {
        var shadow = tilesShadowMap[x, y] + cycleDay.GetIntensity();
        var light = tilesLightMap[x, y];
        if ((wallTilesMap[x, y] == 0 && tilesWorldMap[x, y] == 0) || (shadow == 0 && light == 0)) {
            tilemapLight.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, 0));
            tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, 0));
        } else {
            if (light <= shadow) {
                tilemapLight.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, light));
                tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, 0));
            } else {
                tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, shadow));
                tilemapLight.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, 0));
            }
        }
    }
    private float GetNeightboorMinOrMaxOpacity(float[,] map, int x, int y, bool isMax) {
        var t = IsOutOfBoundMap(x, y + 1, map) ? 1 : map[x, y + 1];
        var b = IsOutOfBoundMap(x, y - 1, map) ? 1 : map[x, y - 1];
        var l = IsOutOfBoundMap(x - 1, y, map) ? 1 : map[x - 1, y];
        var r = IsOutOfBoundMap(x + 1, y, map) ? 1 : map[x + 1, y];
        if (isMax) {
            return Mathf.Max(t, b, l, r);
        }
        return Mathf.Min(t, b, l, r);
    }
    private bool IsOutOfBoundMap(int x, int y, float[,] map) {
        return (x < 0 || x > map.GetUpperBound(0)) || (y < 0 || y > map.GetUpperBound(1));
    }
    private bool IsOutOfBound(int x, int y) {
        return (x < 0 || x > tilesWorldMap.GetUpperBound(0)) || (y < 0 || y > tilesWorldMap.GetUpperBound(1));
    }
    private float GetAmountLight(int tile, int wallTile, float lastLight) {
        if (tile == 0 && wallTile == 0) {
            return lastLight + 0.04f;
        }
        float newLight = 0;
        if (tile > 0) {
            newLight = lastLight + 0.15f;
        } else {
            if (wallTile > 0) {
                newLight = lastLight + 0.05f;
            }
        }
        return newLight > 1 ? 1 : newLight;
    }
}
