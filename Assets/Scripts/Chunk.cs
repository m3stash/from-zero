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
    private int chunkGapWithPlayer = 2; // gap between player and chunk befor unload it.
    private bool firstInitialisation = true;

    private void OnEnable() {
        // tilesMapArray = ;
        indexXWorldPos = indexX * chunkSize;
        indexYWorldPos = indexY * chunkSize;
        if (!firstInitialisation) {
            RefreshTiles();
            StartCoroutine(CheckPlayerPos());
        }
    }

    public void RefreshChunck() {
        indexXWorldPos = indexX * chunkSize;
        indexYWorldPos = indexY * chunkSize;
        RefreshTiles();
        StartCoroutine(CheckPlayerPos());
    }

    private void Update() {
        var intensity = cycleDay.GetIntensity();
        if (intensity != lastIntensity) {
            lastIntensity = intensity;
            for (var x = indexXWorldPos; x < indexXWorldPos + chunkSize; x++) {
                for (var y = indexYWorldPos; y < indexYWorldPos + chunkSize; y++) {
                    lightService.SetTilemapOpacity(tilesShadowMap, tilesLightMap, x, y);
                }
            }
        }
    }
    private void OnDisable() {
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
            var a = index % chunkSize;
            var b = index / chunkSize;
            var xx = (index % chunkSize) + indexXWorldPos;
            var yy = (index / chunkSize) + indexYWorldPos;
            positions[index] = new Vector3Int(a, b, 0);
            var tileBaseIndex = tilesMap[a, b];
            if (tileBaseIndex > 0) {
                tileArray[index] = tilebaseDictionary[tileBaseIndex];
            }
            tileArrayShadow[index] = tilebaseDictionary[-1];
            if (wallTilesMap[xx, yy] > 0) {
                tileArrayWall[index] = tilebaseDictionary[7];
            }
        }
        tilemap.SetTiles(positions, tileArray);
        //tilemapShadow.SetTiles(positions, tileArrayShadow);
        //tilemapWall.SetTiles(positions, tileArrayWall);
        InitTilesMap();
    }
    private void InitTilesMap() {
        for (var x = 0; x < chunkSize; x++) {
            for (var y = 0; y < chunkSize; y++) {
                tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, tilesShadowMap[indexXWorldPos + x, indexYWorldPos + y]));
            }
        }
    }
    void Start() {
        /*StartCoroutine(CheckPlayerPos());
        RefreshTiles();
        firstInitialisation = false;*/
    }

    private IEnumerator CheckPlayerPos() {
        while (true) {
            var playerPos = player.gameObject.transform.position;
            var currentPlayerChunkX = (int)playerPos.x / chunkSize;
            var currentPlayerChunkY = (int)playerPos.y / chunkSize;
            if (currentPlayerChunkX == indexX && currentPlayerChunkY == indexY) {
                SendMessageUpwards("PlayerChunkEnter", playerPos);
            } else if (Mathf.Abs(currentPlayerChunkX - indexX) >= chunkGapWithPlayer || Mathf.Abs(currentPlayerChunkY - indexY) >= chunkGapWithPlayer) {
                SendMessageUpwards("PlayerIsTooFar", this);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void SetTile(Vector3Int vector3, TileBase tilebase) {
        tilemap.SetTile(vector3, tilebase);
    }


    public void RefreshTile(Vector3Int vector3) {
        tilemap.RefreshTile(vector3);
    }

}