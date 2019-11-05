using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour {

    private float lastIntensity = -1;
    public Tilemap tilemap;
    public Tilemap tilemapWall;
    public Tilemap tilemapShadow;
    public int[,] wallTilesMap;
    public int[,] tilesMap;
    public GameObject[,] tilesObjetMap;
    public float[,] tilesLightMap;
    public float[,] tilesShadowMap;
    public GameObject player;
    public CycleDay cycleDay;
    public Dictionary<int, TileBase> tilebaseDictionary;
    public int indexX;
    public int indexY;
    public int indexXWorldPos;
    public int indexYWorldPos;
    public int chunkSize;
    public LightService lightService;
    private bool firstInitialisation = true;
    private bool isShapesCreated = false;
    private BoxCollider2D bc2d;

    private void OnEnable() {
        WorldManager.RefreshLight += RefreshShadowMap;
        indexXWorldPos = indexX * chunkSize;
        indexYWorldPos = indexY * chunkSize;
        if (!firstInitialisation) {
            RefreshTiles();
        }
    }
    private void Update() {
        var intensity = cycleDay.GetIntensity();
        if (intensity != lastIntensity) {
            lastIntensity = intensity;
            this.RefreshShadowMap();
        }
    }
    private void RefreshShadowMap() {
        for (var x = 0; x < chunkSize; x++) {
            for (var y = 0; y < chunkSize; y++) {
                var shadow = tilesShadowMap[indexXWorldPos + x, indexYWorldPos + y] + cycleDay.GetIntensity();
                var light = tilesLightMap[indexXWorldPos + x, indexYWorldPos + y];
                if ((wallTilesMap[indexXWorldPos + x, indexYWorldPos + y] == 0 && wallTilesMap[indexXWorldPos + x, indexYWorldPos + y] == 0) || (shadow == 0 && light == 0)) {
                    tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, 0));
                } else {
                    if (light <= shadow && light < 1) {
                        tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, light));
                    } else {
                        tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, shadow));
                    }
                }
            }
        }
    }
    private void OnDisable() {
        isShapesCreated = false;
        WorldManager.RefreshLight -= RefreshShadowMap;
        DeleteTileMapsTiles();
    }
    private void DeleteTileMapsTiles() {
        for (var x = indexXWorldPos; x < indexXWorldPos + chunkSize; x++) {
            for (var y = indexYWorldPos; y < indexYWorldPos + chunkSize; y++) {
                tilemapShadow.SetTile(new Vector3Int(x, y, 0), null);
                tilemapWall.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }
    private void RefreshTiles() {
        Vector3Int[] positions = new Vector3Int[chunkSize * chunkSize];
        TileBase[] tileArray = new TileBase[positions.Length];
        TileBase[] tileArrayShadow = new TileBase[positions.Length];
        TileBase[] tileArrayWall = new TileBase[positions.Length];
        for (int index = 0; index < positions.Length; index++) {
            var x = index % chunkSize;
            var y = index / chunkSize;
            positions[index] = new Vector3Int(x, y, 0);
            var tileBaseIndex = tilesMap[x, y];
            if (tileBaseIndex > 0) {
                tileArray[index] = tilebaseDictionary[tileBaseIndex];
            } else {
                tileArray[index] = null;
            }
            if(tilesShadowMap[x, y] > 0) {
                tileArrayShadow[index] = tilebaseDictionary[-1];
            } else {
                tileArrayShadow[index] = null;
            }
            if (wallTilesMap[x + indexXWorldPos, y + indexYWorldPos] > 0) {
                tileArrayWall[index] = tilebaseDictionary[7];
            } else {
                tileArrayWall[index] = null;
            }
        }
        tilemap.SetTiles(positions, tileArray);
        tilemapShadow.SetTiles(positions, tileArrayShadow);
        tilemapWall.SetTiles(positions, tileArrayWall);
        this.RefreshShadowMap();
    }
    void Start() {
        var bc2d = GetComponentInChildren<BoxCollider2D>();
        bc2d.offset = new Vector2(chunkSize / 2, chunkSize / 2);
        bc2d.size = new Vector2(chunkSize, chunkSize);
        GetComponentInChildren<TilemapCollider2D>().enabled = false;
        firstInitialisation = false;
        RefreshTiles();
    }
    public void SetTile(Vector3Int vector3, TileBase tilebase) {
        tilemap.SetTile(vector3, tilebase);
    }
    public void RefreshTile(Vector3Int vector3) {
        tilemap.RefreshTile(vector3);
    }
    public void ChunckVisible() {
        if (!isShapesCreated) {
            GetComponentInChildren<TilemapCollider2D>().enabled = true;
            Debug.Log("ooooooooooooooooooui");
            isShapesCreated = true;
        }
    }
}