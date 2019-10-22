using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
public class LightService : MonoBehaviour {

    private int[,] tilesWorldMap;
    private float[,] tilesShadowMap;
    private int[,] wallTilesMap;
    private float[,] tilesLightMap;

    public void Init(int[,] tilesWorldMap, float[,] tilesLightMap, int[,] wallTilesMap, float[,] tilesShadowMap) {
        this.tilesWorldMap = tilesWorldMap;
        this.wallTilesMap = wallTilesMap;
        this.tilesShadowMap = tilesShadowMap;
        this.tilesLightMap = tilesLightMap;
    }
    public void RecursivAddNewLight(int x, int y, float lastLight) {
        if (IsOutOfBound(x, y))
            return;
        float newLight = GetAmountLight(tilesWorldMap[x, y], wallTilesMap[x, y], lastLight);
        if ((newLight >= tilesLightMap[x, y]) || newLight >= 1)
            return;
        tilesLightMap[x, y] = newLight;
        RecursivAddNewLight(x + 1, y, newLight);
        RecursivAddNewLight(x, y + 1, newLight);
        RecursivAddNewLight(x - 1, y, newLight);
        RecursivAddNewLight(x, y - 1, newLight);
    }
    public void RecursivDeleteShadow(int x, int y) {
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
        RecursivDeleteShadow(x + 1, y);
        RecursivDeleteShadow(x, y + 1);
        RecursivDeleteShadow(x - 1, y);
        RecursivDeleteShadow(x, y - 1);
    }
    public void RecursivDeleteLight(int x, int y, bool toDelete) {
        if (IsOutOfBound(x, y))
            return;
        var minLight = GetNeightboorMinOrMaxOpacity(tilesLightMap, x, y, false);
        float newLight = GetAmountLight(tilesWorldMap[x, y], wallTilesMap[x, y], minLight);
        if (newLight <= tilesLightMap[x, y] && !toDelete || !toDelete && tilesLightMap[x, y] == 0.15f || newLight > 1)
            return;
        tilesLightMap[x, y] = newLight;
        RecursivDeleteLight(x + 1, y, false);
        RecursivDeleteLight(x, y + 1, false);
        RecursivDeleteLight(x - 1, y, false);
        RecursivDeleteLight(x, y - 1, false);
    }
    public void RecursivAddShadow(int x, int y) {
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
        RecursivAddShadow(x + 1, y);
        RecursivAddShadow(x, y + 1);
        RecursivAddShadow(x - 1, y);
        RecursivAddShadow(x, y - 1);
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
