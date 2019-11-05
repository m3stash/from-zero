﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkService : MonoBehaviour {

    public int numberOfPool;
    private int chunkSize;
    private GameObject player;
    private int[,][,] tilesMapChunks;
    private float[,] tilesLightMap;
    private int[,] tilesWorldMap;
    private int[,] wallTilesMap;
    private CycleDay cycleDay;
    private Dictionary<int, TileBase> tilebaseDictionary;
    private Transform worldMapTransform;
    private float waitingTimeAfterCreateChunk = 0.3f;
    private List<Chunk> unUsedChunk = new List<Chunk>();
    private List<Chunk> usedChunk = new List<Chunk>();
    private int halfChunk;
    private int boundX;
    private int boundY;
    private LightService lightService;
    private float[,] tilesShadowMap;
    private int currentPlayerChunkX;
    private int currentPlayerChunkY;
    private readonly int chunkGapWithPlayer = 2; // gap between player and chunk befor unload it.

    //      O | O | O
    //      O | # | O  => # = perso on int array
    //      O | O | O
    private int chunkXLength;
    private int chunkYLength;

    public void FixedUpdate() {
        currentPlayerChunkX = (int)player.transform.position.x / chunkSize;
        currentPlayerChunkY = (int)player.transform.position.y / chunkSize;
        var chuncksToDesactivate = usedChunk.FindAll(chunk => Mathf.Abs(chunk.indexX - currentPlayerChunkX) >= chunkGapWithPlayer || Mathf.Abs(chunk.indexY - currentPlayerChunkY) >= chunkGapWithPlayer);
        chuncksToDesactivate.ForEach(chunk => {
            PlayerIsTooFar(chunk);
        });
        StartCoroutine(StartPool(player.transform.position));
    }
    public void SetWallMap(int[,] map) {
        wallTilesMap = map;
    }
    public int[,] GetTilesMapChunks(int x, int y) {
        return tilesMapChunks[x, y];
    }
    public Chunk GetChunk(int posX, int posY) {
        return usedChunk.Find(chunk => chunk.indexX == posX && chunk.indexY == posY);
    }
    public void Init(int _chunkSize, Dictionary<int, TileBase> _tilebaseDictionary, int[,] _tilesWorldMap, float[,] _tilesLightMap, GameObject _player, CycleDay _cycleDay, LightService _lightService, float[,] _tilesShadowMap) {
        boundX = tilesMapChunks.GetUpperBound(0);
        boundY = tilesMapChunks.GetUpperBound(1);
        halfChunk = _chunkSize / 2;
        player = _player;
        worldMapTransform = GameObject.FindGameObjectWithTag("WorldMap").gameObject.transform;
        tilebaseDictionary = _tilebaseDictionary;
        tilesLightMap = _tilesLightMap;
        tilesWorldMap = _tilesWorldMap;
        chunkSize = _chunkSize;
        cycleDay = _cycleDay;
        lightService = _lightService;
        tilesShadowMap = _tilesShadowMap;
        CreatePoolChunk(6, 26); // cas pour 32 tiles
        // CreatePoolChunk(3, 13); // cas pour 64 tiles
        // RenderPartialMapForTest(); // for debug map
    }
    public void CreateChunksFromMaps(int[,] tilesMap, int chunkSize) {
        chunkXLength = (tilesMap.GetUpperBound(0) + 1) / chunkSize;
        chunkYLength = (tilesMap.GetUpperBound(1) + 1) / chunkSize;
        int[,][,] tilesMapChunksArray = new int[chunkXLength, chunkYLength][,];
        for (var chkX = 0; chkX < chunkXLength; chkX++) {
            for (var chkY = 0; chkY < chunkYLength; chkY++) {
                int[,] tileMap = new int[chunkSize, chunkSize];
                for (var x = 0; x < chunkSize; x++) {
                    for (var y = 0; y < chunkSize; y++) {
                        tileMap[x, y] = tilesMap[(chkX * chunkSize) + x, (chkY * chunkSize) + y];
                    }
                }
                tilesMapChunksArray[chkX, chkY] = tileMap;
            }
        }
        tilesMapChunks = tilesMapChunksArray;
    }
    // for debug;
    public void RenderPartialMapForTest() {
        GameObject chunk = Instantiate((GameObject)Resources.Load("Prefabs/Chunk"), new Vector3(0, 0, 0), transform.rotation);
        chunk.transform.parent = worldMapTransform;
        Chunk ck = chunk.GetComponent<Chunk>();
        Tilemap[] tilemaps = chunk.GetComponentsInChildren<Tilemap>();

        ck.cycleDay = cycleDay;
        ck.tilesLightMap = tilesLightMap;
        ck.wallTilesMap = wallTilesMap;
        ck.lightService = lightService;
        ck.tilesShadowMap = tilesShadowMap;
        ck.chunkSize = chunkSize;
        ck.tilebaseDictionary = tilebaseDictionary;
        ck.indexX = 0;
        ck.indexY = 0;
    }
    public void InitialiseChunkPooling() {
        for (var i = 0; i < numberOfPool; i++) {
            GameObject chunk = Instantiate((GameObject)Resources.Load("Prefabs/Chunk"), new Vector3(0, 0, 0), transform.rotation);
            chunk.gameObject.SetActive(false);
            chunk.transform.parent = worldMapTransform;
            Chunk ck = chunk.GetComponent<Chunk>();
            ck.cycleDay = cycleDay;
            ck.tilesLightMap = tilesLightMap;
            ck.wallTilesMap = wallTilesMap;
            ck.chunkSize = chunkSize;
            ck.lightService = lightService;
            ck.tilebaseDictionary = tilebaseDictionary;
            ck.tilesMap = null;
            ck.tilesShadowMap = tilesShadowMap;
            ck.indexX = -1;
            ck.indexY = -1;
            unUsedChunk.Add(ck);
        }
    }
    public void CreatePoolChunk(int xStart, int yStart) {
        InitialiseChunkPooling();
        ManageChunkFromPool(xStart, yStart);
        // spawn player on center start chunk
        player.transform.position = new Vector3(xStart * chunkSize + (chunkSize / 2), yStart * chunkSize + (chunkSize / 6), 0);
    }
    private void ManageChunkFromPool(int chunkPosX, int chunkPosY) {
        if (unUsedChunk.Count == 0) {
            Debug.Log("ATTENTION pool vide !!!!!!!!!!!"); // ToDo => voir le pb de la pool de 20 !
        }
        usedChunk.Add(unUsedChunk[0]);
        unUsedChunk.RemoveAt(0);
        Chunk ck = usedChunk[usedChunk.Count - 1];
        GameObject chunkGo = ck.gameObject;
        chunkGo.transform.position = new Vector3(chunkPosX * chunkSize, chunkPosY * chunkSize, 0);
        ck.player = player;
        ck.tilesMap = tilesMapChunks[chunkPosX, chunkPosY]; // ToDo régler le pb de out of range !!!!!!!!!
        Tilemap[] tilemaps = chunkGo.GetComponentsInChildren<Tilemap>();
        // toDo refacto tout ça => just a POC !
        var tilemap = tilemaps[0].GetComponent<TileMapScript>();
        var wallmap = tilemaps[1].GetComponent<TileMapScript>();
        tilemap.tilePosX = chunkPosX * chunkSize;
        wallmap.tilePosX = chunkPosX * chunkSize;
        tilemap.tilePosY = chunkPosY * chunkSize;
        wallmap.tilePosY = chunkPosY * chunkSize;
        tilemap.tilesWorldMap = tilesWorldMap;
        wallmap.tilesWorldMap = tilesWorldMap;
        /*ck.tileMapScript.tilePosX = chunkPosX * chunkSize;
        ck.tileMapScript.tilePosY = chunkPosY * chunkSize;*/
        ck.indexX = chunkPosX;
        ck.indexY = chunkPosY;
        chunkGo.SetActive(true);
    }
    private void PlayerIsTooFar(Chunk ck) {
        var i = 0;
        int findIndex = -1;
        usedChunk.ForEach(chunk => {
            if (chunk.indexX == ck.indexX && chunk.indexY == ck.indexY) {
                findIndex = i;
                return;
            }
            i++;
        });
        if (findIndex != -1) {
            ck.gameObject.SetActive(false);
            unUsedChunk.Add(usedChunk[findIndex]);
            usedChunk.RemoveAt(findIndex);
        }
    }
    private bool ChunkAlreadyCreate(int x, int y) {
        return usedChunk.Find(chunk => chunk.indexX == x && chunk.indexY == y);
    }
    private IEnumerator StartPool(Vector3 playerPos) {
        var diff = 4;
        // ToDO voir a ne le faire que si top bottom left et right > 0 et < length
        var chunkIndexX = (int)playerPos.x / chunkSize;
        var chunkIndexY = (int)playerPos.y / chunkSize;
        var playerPosXMap = (int)playerPos.x % chunkSize;
        var playerPosYMap = (int)playerPos.y % chunkSize;
        var top = playerPosYMap > halfChunk + diff;
        var bottom = playerPosYMap < halfChunk - diff;
        var left = playerPosXMap < halfChunk - diff;
        var right = playerPosXMap > halfChunk + diff;
        if (right && chunkIndexX + 1 < boundX) {
            if (bottom && chunkIndexY - 1 >= 0) {
                // diag bottom right
                if (!ChunkAlreadyCreate(chunkIndexX, chunkIndexY - 1)) {
                    ManageChunkFromPool(chunkIndexX, chunkIndexY - 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
                if (!ChunkAlreadyCreate(chunkIndexX + 1, chunkIndexY - 1)) {
                    ManageChunkFromPool(chunkIndexX + 1, chunkIndexY - 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
            }
            if (top) {
                // diag top right 
                if (!ChunkAlreadyCreate(chunkIndexX, chunkIndexY + 1)) {
                    ManageChunkFromPool(chunkIndexX, chunkIndexY + 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
                if (!ChunkAlreadyCreate(chunkIndexX + 1, chunkIndexY + 1)) {
                    ManageChunkFromPool(chunkIndexX + 1, chunkIndexY + 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
            }
            // only right
            if (!ChunkAlreadyCreate(chunkIndexX + 1, chunkIndexY)) {
                ManageChunkFromPool(chunkIndexX + 1, chunkIndexY);
                yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
            }
        } else if (left && chunkIndexX - 1 >= 0) {
            if (bottom) {
                // diag bottom left
                if (!ChunkAlreadyCreate(chunkIndexX, chunkIndexY - 1)) {
                    ManageChunkFromPool(chunkIndexX, chunkIndexY - 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
                if (!ChunkAlreadyCreate(chunkIndexX - 1, chunkIndexY - 1)) {
                    ManageChunkFromPool(chunkIndexX - 1, chunkIndexY - 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
            }
            if (top) {
                // diag top left 
                if (!ChunkAlreadyCreate(chunkIndexX, chunkIndexY + 1)) {
                    ManageChunkFromPool(chunkIndexX, chunkIndexY + 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
                if (!ChunkAlreadyCreate(chunkIndexX - 1, chunkIndexY + 1)) {
                    ManageChunkFromPool(chunkIndexX - 1, chunkIndexY + 1);
                    yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
                }
            }
            // only left
            if (!ChunkAlreadyCreate(chunkIndexX - 1, chunkIndexY)) {
                ManageChunkFromPool(chunkIndexX - 1, chunkIndexY);
                yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
            }
        } else if (top && chunkIndexY + 1 <= boundY) {
            // only top
            if (!ChunkAlreadyCreate(chunkIndexX, chunkIndexY + 1)) {
                ManageChunkFromPool(chunkIndexX, chunkIndexY + 1);
                yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
            }
        } else if (bottom && chunkIndexY - 1 >= 0) {
            // only bottom
            if (!ChunkAlreadyCreate(chunkIndexX, chunkIndexY - 1)) {
                ManageChunkFromPool(chunkIndexX, chunkIndexY - 1);
                yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
            }
        }
    }
}