﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System;

public class WorldManager : MonoBehaviour {

    private ChunkService chunkService;
    private LightService lightService;
    private LevelGenerator levelGenerator;
    private GameObject tile_selector;
    private GameObject player;
    private CycleDay cycleDay;
    private float[,] tilesLightMap;
    private float[,] tilesShadowMap;
    private int[,] tilesWorldMap;
    private int[,] wallTilesMap; // toDo rename
    private GameObject[,] tilesObjetMap;
    private Dictionary<int, TileBase> tilebaseDictionary;
    private Dictionary<int, Item_cfg> ObjectbaseDictionary;
    private Sprite[] block_sprites;

    public int worldSizeX;
    public int worldSizeY;
    public TileBase_cfg tilebase_cfg;
    public Tilemap tilemapLight;
    public Tilemap tilemapShadow;
    public Tilemap wallMap;
    public int chunkSize;

    void Start() {
        InitResources();
        CreateWorldMap();
        CreateLightMap();
        CreatePlayer();
        chunkService.Init(chunkSize, tilebaseDictionary, tilesWorldMap, tilesLightMap, player, tilemapLight, wallMap, tilesObjetMap, cycleDay, lightService, tilesShadowMap, tilemapShadow);
        lightService.Init(tilemapLight, tilesWorldMap, tilesLightMap, wallTilesMap, tilemapShadow, tilesShadowMap, cycleDay);
    }
    private void InitResources() {
        cycleDay = gameObject.GetComponentInChildren<CycleDay>();
        chunkService = gameObject.GetComponent<ChunkService>();
        levelGenerator = gameObject.GetComponent<LevelGenerator>();
        lightService = gameObject.GetComponent<LightService>();
        block_sprites = Resources.LoadAll<Sprite>("Sprites/blocks");
        tilebaseDictionary = tilebase_cfg.GetDico();
        tile_selector = Instantiate(Resources.Load("Prefabs/tile_selector")) as GameObject;
    }
    private void CreateLightMap() {
        tilesLightMap = new float[worldSizeX, worldSizeY];
        for(var x = 0; x < worldSizeX; x++) {
            for (var y = 0; y < worldSizeY; y++) {
                tilesLightMap[x, y] = 1;
            }
        }
        tilesShadowMap = new float[worldSizeX, worldSizeY];
        levelGenerator.GenerateWorldLight(tilesLightMap, tilesShadowMap, tilesWorldMap, wallTilesMap);
    }
    private void CreateWorldMap() {
        tilesWorldMap = new int[worldSizeX, worldSizeY];
        wallTilesMap = new int[worldSizeX, worldSizeY];
        tilesObjetMap = new GameObject[worldSizeX, worldSizeY];
        levelGenerator.GenerateTilesWorldMap(tilesWorldMap, wallTilesMap);
        chunkService.SetWallMap(wallTilesMap);
        chunkService.CreateChunksFromMaps(tilesWorldMap, chunkSize);
    }
    private void CreatePlayer() {
        player = Instantiate((GameObject)Resources.Load("Prefabs/Characters/Player/Player"), new Vector3(0, 0, 0), transform.rotation);
        tile_selector.GetComponent<TileSelector>().Init(player, this, wallTilesMap, tilesWorldMap, tilesObjetMap);
    }
    public void AddItem(int posX, int posY, InventoryItem item) {
        var id = item.config.id;
        // toDo voir si le mettre directement dans les chunks ne serait pas mieux!
        var go = Instantiate((GameObject)Resources.Load("Prefabs/Items/item_" + id), new Vector3(posX + 0.5f, posY + 0.5f, 0), transform.rotation);
        tilesObjetMap[posX, posY] = go;
        if (id == 11) {
            lightService.RecursivAddNewLight(posX, posY, 0, tilemapLight, tilesLightMap);
        }
    }
    public void DeleteItem(int posX, int posY) {
        if (tilesObjetMap[posX, posY].name == "item_11(Clone)") { // toDo changer cette merde
            lightService.RecursivDeleteLight(posX, posY, tilemapLight, tilesLightMap, tilesObjetMap, true);
        }
        tilesObjetMap[posX, posY] = null;
        Destroy(tilesObjetMap[posX, posY]);
        ManageItems.CreateItemOnMap(posX, posY, 11);
    }
    public void DeleteTile(int x, int y) {
        var id = tilesWorldMap[x, y];
        Chunk currentChunk = ManageChunkTile(x, y, 0);
        currentChunk.SetTile(new Vector3Int(x % chunkSize, y % chunkSize, 0), null);
        lightService.RecursivDeleteShadow(x, y, tilemapShadow, tilesLightMap, tilemapLight);
        ManageItems.CreateItemOnMap(x, y, id);
        RefreshChunkNeightboorTiles(x, y, currentChunk.tilemap);
    }
    public void AddTile(int x, int y, int id) {
        Chunk currentChunk = ManageChunkTile(x, y, id);
        currentChunk.SetTile(new Vector3Int(x % chunkSize, y % chunkSize, 0), tilebaseDictionary[id]);
        lightService.RecursivAddShadow(x, y, tilesLightMap, tilemapLight);
        RefreshChunkNeightboorTiles(x, y, currentChunk.tilemap);
    }
    private Chunk ManageChunkTile(int x, int y, int id) {
        int chunkX = (int)x / chunkSize;
        int chunkY = (int)y / chunkSize;
        var tilemap = chunkService.GetTilesMapChunks(chunkX, chunkY);
        tilemap[x % chunkSize, y % chunkSize] = id;
        tilesWorldMap[x, y] = id;
        return chunkService.GetChunk(chunkX, chunkY);
    }
    public void RefreshChunkNeightboorTiles(int x, int y, Tilemap tilemap) {
        var topBoundMap = chunkSize - 1;
        var rightBoundMap = chunkSize - 1;
        var posYInMap = y % chunkSize;
        var posXInMap = x % chunkSize;
        // tile position on tilemap
        bool yTopInMap = posYInMap == chunkSize - 1;
        bool yBottomInMap = posYInMap == 0;
        bool xRightInMap = posXInMap == chunkSize - 1;
        bool xLeftInMap = posXInMap == 0;
        int chunkPosX = (int)x / chunkSize;
        int chunkPosY = (int)y / chunkSize;
        // top //
        if (yTopInMap) {
            Chunk topChunk = chunkService.GetChunk(chunkPosX, chunkPosY + 1);
            // if diagonal left top
            if (xLeftInMap) {
                Chunk leftChunk = chunkService.GetChunk(chunkPosX - 1, chunkPosY);
                Chunk diagLeftTopChunk = chunkService.GetChunk(chunkPosX - 1, chunkPosY + 1);
                if (leftChunk) {
                    leftChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, topBoundMap, 0));
                    leftChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, topBoundMap - 1, 0));
                }
                if (diagLeftTopChunk) {
                    diagLeftTopChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, 0, 0));
                }
                if (topChunk) {
                    topChunk.tilemap.RefreshTile(new Vector3Int(0, 0, 0));
                    topChunk.tilemap.RefreshTile(new Vector3Int(1, 0, 0));
                }
            } else if (xRightInMap) {
                // if diagonal right top
                Chunk rightChunk = chunkService.GetChunk(chunkPosX + 1, chunkPosY);
                Chunk diagRightTopChunk = chunkService.GetChunk(chunkPosX + 1, chunkPosY + 1);
                if (rightChunk) {
                    rightChunk.tilemap.RefreshTile(new Vector3Int(0, topBoundMap, 0));
                    rightChunk.tilemap.RefreshTile(new Vector3Int(0, topBoundMap - 1, 0));
                }
                if (diagRightTopChunk) {
                    diagRightTopChunk.tilemap.RefreshTile(new Vector3Int(0, 0, 0));
                }
                if (topChunk) {
                    topChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, 0, 0));
                    topChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap - 1, 0, 0));
                }
            } else {
                // if just top
                if (topChunk) {
                    topChunk.tilemap.RefreshTile(new Vector3Int(posXInMap, 0, 0));
                    topChunk.tilemap.RefreshTile(new Vector3Int(posXInMap - 1, 0, 0));
                    topChunk.tilemap.RefreshTile(new Vector3Int(posXInMap + 1, 0, 0));
                }
            }
        } else {
            // 3 top tiles in current chunk
            tilemap.RefreshTile(new Vector3Int(posXInMap, posYInMap + 1, 0));
            tilemap.RefreshTile(new Vector3Int(posXInMap + 1, posYInMap + 1, 0));
            tilemap.RefreshTile(new Vector3Int(posXInMap - 1, posYInMap + 1, 0));
        }
        if (yBottomInMap) {
            // bottom //
            Chunk bottomChunk = chunkService.GetChunk(chunkPosX, chunkPosY - 1);
            if (xLeftInMap) {
                // if diagonal left bottom
                Chunk leftBottomChunk = chunkService.GetChunk(chunkPosX - 1, chunkPosY);
                Chunk diagLeftBottomChunk = chunkService.GetChunk(chunkPosX - 1, chunkPosY - 1);
                if (leftBottomChunk) {
                    leftBottomChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, 0, 0));
                    leftBottomChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, 1, 0));
                }
                if (diagLeftBottomChunk) {
                    diagLeftBottomChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, topBoundMap, 0));
                }
                if (bottomChunk) {
                    bottomChunk.tilemap.RefreshTile(new Vector3Int(0, topBoundMap, 0));
                    bottomChunk.tilemap.RefreshTile(new Vector3Int(1, topBoundMap, 0));
                }
            } else if (xRightInMap) {
                // if diagonal right bottom
                Chunk rightBottomChunk = chunkService.GetChunk(chunkPosX + 1, chunkPosY);
                Chunk diagRightBottomChunk = chunkService.GetChunk(chunkPosX + 1, chunkPosY - 1);
                if (rightBottomChunk) {
                    rightBottomChunk.RefreshTile(new Vector3Int(0, 0, 0));
                    rightBottomChunk.RefreshTile(new Vector3Int(0, 1, 0));
                }
                if (diagRightBottomChunk) {
                    diagRightBottomChunk.RefreshTile(new Vector3Int(0, topBoundMap, 0));
                }
                if (bottomChunk) {
                    bottomChunk.RefreshTile(new Vector3Int(rightBoundMap, topBoundMap, 0));
                    bottomChunk.RefreshTile(new Vector3Int(rightBoundMap - 1, topBoundMap, 0));
                }
            } else {
                // just bottom //
                if (bottomChunk) {
                    bottomChunk.tilemap.RefreshTile(new Vector3Int(posXInMap, topBoundMap, 0));
                    bottomChunk.tilemap.RefreshTile(new Vector3Int(posXInMap - 1, topBoundMap, 0));
                    bottomChunk.tilemap.RefreshTile(new Vector3Int(posXInMap + 1, topBoundMap, 0));
                }
            }
        } else {
            // 3 bottom tiles in current chunk
            tilemap.RefreshTile(new Vector3Int(posXInMap, posYInMap - 1, 0));
            tilemap.RefreshTile(new Vector3Int(posXInMap - 1, posYInMap - 1, 0));
            tilemap.RefreshTile(new Vector3Int(posXInMap + 1, posYInMap - 1, 0));
        }
        if (xLeftInMap) {
            // just left //
            Chunk leftChunk = chunkService.GetChunk(chunkPosX - 1, chunkPosY);
            if (leftChunk) {
                leftChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, posYInMap, 0));
                leftChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, posYInMap - 1, 0));
                leftChunk.tilemap.RefreshTile(new Vector3Int(rightBoundMap, posYInMap + 1, 0));
            }
        } else {
            tilemap.RefreshTile(new Vector3Int(posXInMap - 1, posYInMap, 0));
        }
        if (xRightInMap) {
            // just right
            Chunk rightChunk = chunkService.GetChunk(chunkPosX + 1, chunkPosY);
            if (rightChunk) {
                rightChunk.tilemap.RefreshTile(new Vector3Int(0, posYInMap, 0));
                rightChunk.tilemap.RefreshTile(new Vector3Int(0, posYInMap - 1, 0));
                rightChunk.tilemap.RefreshTile(new Vector3Int(0, posYInMap + 1, 0));
            }
        } else {
            tilemap.RefreshTile(new Vector3Int(posXInMap + 1, posYInMap, 0));
        }
    }
}
