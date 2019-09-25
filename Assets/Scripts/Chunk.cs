using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightsPos {
    public int x;
    public int y;
    public LightsPos(int _x, int _y) {
        x = _x;
        y = _y;
    }
}

public class Chunk : MonoBehaviour {

    private float lastIntensity = -1;
    public Tilemap tilemap;
    public Tilemap tilemapLight;
    public Tilemap tilemapWall;
    public Tilemap ObjectMap;
    public Tilemap tilemapShadow;
    public int[,] wallTilesMap;
    public int[,] tilesMap;
    public GameObject[,] tilesObjetMap;
    public float[,] tilesLightMap;
    public float[,] tilesShadowMap;
    public GameObject player;
    public CycleDay cycleDay;
    public int indexX;
    public int indexY;
    public int chunkSize;
    public Dictionary<int, TileBase> tilebaseDictionary;
    public TileMapScript tileMapScript; // a supprimer ???
    private bool playerOnChunk = false;
    private int chunkGapWithPlayer = 2; // gap between player and chunk befor unload it.
    private bool firstInitialisation = true;
    public LightService lightService;

    private void OnEnable() {
        if (!firstInitialisation) {
            RefreshTiles();
            StartCoroutine(CheckPlayerPos());
        }
    }
    private void Update() {
        var intensity = cycleDay.GetIntensity();
        if (intensity != lastIntensity) {
            lastIntensity = intensity;
            var startX = indexX * chunkSize;
            var startY = indexY * chunkSize;
            for (var x = startX; x < startX + chunkSize; x++) {
                for (var y = startY; y < startY + chunkSize; y++) {
                    var shadow = tilesShadowMap[x, y] + intensity;
                    var light = tilesLightMap[x, y];
                    var walltTile = wallTilesMap[x, y];
                    if ((walltTile == 0 && tilesMap[x % chunkSize, y % chunkSize] == 0) || (light == 0 && shadow == 0)) {
                        tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, 0));
                        tilemapLight.SetColor(new Vector3Int(x, y, 0), new Color(0, 0, 0, 0));
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
            }
        }
    }
    private void OnDisable() {
        DeleteShadowTile();
    }
    private void DeleteShadowTile() { // TODO change le nom de la méthode
        var startX = indexX * chunkSize;
        var startY = indexY * chunkSize;
        for (var x = startX; x < startX + chunkSize; x++) {
            for (var y = startY; y < startY + chunkSize; y++) {
                tilemapShadow.SetTile(new Vector3Int(x, y, 0), null);
                tilemapWall.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }
    private void RefreshTiles() {
        Vector3Int[] positions = new Vector3Int[chunkSize * chunkSize];
        TileBase[] tileArray = new TileBase[positions.Length];
        for (int index = 0; index < positions.Length; index++) {
            var a = index % chunkSize;
            var b = index / chunkSize;
            positions[index] = new Vector3Int(a, b, 0);
            var tileBaseIndex = tilesMap[a, b];
            if (tileBaseIndex > 0) {
                tileArray[index] = tilebaseDictionary[tileBaseIndex];
            } else {
                tileArray[index] = null;
            }
        }
        tilemap.SetTiles(positions, tileArray);
        SetShadow();
        SetLight();
    }
    private void SetLight() {
        // TODO refacto avec le shadow pour pas faire 50 boucles !!!!!!!!!!!!!!!!!!
        var startX = indexX * chunkSize;
        var startY = indexY * chunkSize;
        for (var x = startX; x < startX + chunkSize; x++) {
            for (var y = startY; y < startY + chunkSize; y++) {
                tilemapLight.SetTile(new Vector3Int(x, y, 0), tilebaseDictionary[-1]);
                tilemapLight.SetColor(new Vector3Int(x, y, 0), new Color(0f, 0f, 0f, 0));
                /*if (wallTilesMap[x, y] > 0 && tilesLightMap[x, y] < 1) {
                    tilemapLight.SetTile(new Vector3Int(x, y, 0), tilebaseDictionary[7]);
                }*/
            }
        }
    }
    private void SetShadow() {
        var startX = indexX * chunkSize;
        var startY = indexY * chunkSize;
        for (var x = startX; x < startX + chunkSize; x++) {
            for (var y = startY; y < startY + chunkSize; y++) {
                tilemapShadow.SetTile(new Vector3Int(x, y, 0), tilebaseDictionary[-1]);
                tilemapShadow.SetColor(new Vector3Int(x, y, 0), new Color(0f, 0f, 0f, tilesShadowMap[x, y]));
                if (wallTilesMap[x, y] > 0) {
                    tilemapWall.SetTile(new Vector3Int(x, y, 0), tilebaseDictionary[7]);
                }
            }
        }
    }
    void Start() {
        StartCoroutine(CheckPlayerPos());
        RefreshTiles();
        firstInitialisation = false;
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