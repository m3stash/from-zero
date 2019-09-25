//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class ChunkService : MonoBehaviour {

//    private int chunkSize;
//    private int[,][,] tilesMapChunks;
//    private int[,][,] objectsMapChunks;
//    private GameObject player;
//    private int[,] worldMap;
//    private Dictionary<int, TileBase> tilebaseDictionary;
//    private Transform worldMapTransform;
//    private Chunk[,] chunksPooling;
//    private float waitingTimeAfterCreateChunk = 0.01f;
//          O | O | O
//          O | # | O  => # = perso on int array
//          O | O | O

//    private int chunkXLength;
//    private int chunkYLength;

//    public int[,] GetTilesMapChunks(int x, int y) {
//        return tilesMapChunks[x, y];
//    }

//    public int[,] GetObjectsMapChunks(int x, int y) {
//        return objectsMapChunks[x, y];
//    }

//    public Chunk GetChunk(int posX, int posY) {
//        for (var x = 0; x < 3; x++) {
//            for (var y = 0; y < 3; y++) {
//                var chunk = chunksPooling[x, y];
//                if (chunk.indexX == posX && chunk.indexY == posY) {
//                    return chunk;
//                }
//            }
//        }
//        return null;
//    }

//    public void Init(int _chunkSize, Dictionary<int, TileBase> _tilebaseDictionary, int[,] _worldMap, GameObject _player) {
//        player = _player;
//        worldMapTransform = GameObject.FindGameObjectWithTag("WorldMap").gameObject.transform;
//        tilebaseDictionary = _tilebaseDictionary;
//        worldMap = _worldMap;
//        chunkSize = _chunkSize;
//        chunksPooling = new Chunk[3, 3];
//         CreatePoolChunk(10, 28);
//        CreatePoolChunk(5, 5);
//        CreatePoolChunk(10, 28); // to do a dynamiser selon le haut de la map en cherchant le premier point de sol dispo et calculer la motier pour générer le pooling
//         RenderPartialMapForTest(); // for debug map
//    }

//    public void CreateChunksFromMaps(int[,] tilesMap, int[,] objectsMap, int chunkSize) {
//        chunkXLength = (tilesMap.GetUpperBound(0) + 1) / chunkSize;
//        chunkYLength = (tilesMap.GetUpperBound(1) + 1) / chunkSize;
//        int[,][,] tilesMapChunksArray = new int[chunkXLength, chunkYLength][,];
//        int[,][,] objectsMapChunksArray = new int[chunkXLength, chunkYLength][,];
//        for (var chkX = 0; chkX < chunkXLength; chkX++) {
//            for (var chkY = 0; chkY < chunkYLength; chkY++) {
//                int[,] tileMap = new int[chunkSize, chunkSize];
//                int[,] objectMap = new int[chunkSize, chunkSize];
//                for (var x = 0; x < chunkSize; x++) {
//                    for (var y = 0; y < chunkSize; y++) {
//                        tileMap[x, y] = tilesMap[(chkX * chunkSize) + x, (chkY * chunkSize) + y];
//                        objectMap[x, y] = objectsMap[(chkX * chunkSize) + x, (chkY * chunkSize) + y];
//                    }
//                }
//                tilesMapChunksArray[chkX, chkY] = tileMap;
//                objectsMapChunksArray[chkX, chkY] = objectMap;
//            }
//        }
//        tilesMapChunks = tilesMapChunksArray;
//        objectsMapChunks = objectsMapChunksArray;
//    }

//     for debug;
//    public void RenderPartialMapForTest() {
//        /*GameObject chunk = Instantiate((GameObject)Resources.Load("Prefabs/Chunk"), new Vector3(0, 0, 0), transform.rotation);
//        chunk.transform.parent = worldMapTransform;
//        Tilemap tilemap = chunk.GetComponentInChildren<Tilemap>();
//        TileMapScript tileMapScript = tilemap.GetComponent<TileMapScript>();
//        tileMapScript.worldMap = worldMap;
//        for (var x = 0; x < 1024; x++) {
//            for (var y = 0; y < 1152; y++) {
//                int tileIndex = worldMap[x, y];
//                if (tileIndex > 0) {
//                    tilemap.SetTile(new Vector3Int(x, y, 0), tilebaseDictionary[tileIndex]);
//                }
//            }
//        }*/
//         toDo a revoir...
//    }

//    public void CreatePoolChunk(int xStart, int yStart) {
//        for (var x = 0; x < 3; x++) {
//            for (var y = 0; y < 3; y++) {
//                GameObject chunk = Instantiate((GameObject)Resources.Load("Prefabs/Chunk"), new Vector3(xStart * chunkSize + x * chunkSize, yStart * chunkSize + y * chunkSize, 0), transform.rotation);
//                chunk.transform.parent = worldMapTransform;
//                Tilemap[] tilemaps = chunk.GetComponentsInChildren<Tilemap>();
//                 Debug.Log(tilemaps[0].name);
//                 Debug.Log(tilemaps[1].name);
//                Tilemap tilemap = chunk.GetComponentInChildren<Tilemap>();
//                Chunk ck = chunk.GetComponent<Chunk>();
//                ck.tilemap = tilemap;
//                ck.chunkSize = chunkSize;
//                ck.tilebaseDictionary = tilebaseDictionary;
//                ck.tileMap = tilesMapChunks[xStart + x, yStart + y];
//                ck.objectMap = objectsMapChunks[xStart + x, yStart + y];
//                ck.indexX = xStart + x;
//                ck.indexY = yStart + y;
//                ck.tm2d = tilemap.gameObject.GetComponent<TilemapCollider2D>();
//                TileMapScript tileMapScript = tilemap.GetComponent<TileMapScript>();
//                ck.tileMapScript = tileMapScript;
//                ck.tileMapScript.tilePosX = (xStart + x) * chunkSize;
//                ck.tileMapScript.tilePosY = (yStart + y) * chunkSize;
//                ck.tileMapScript.worldMap = worldMap;
//                chunksPooling[x, y] = ck;
//            }
//        }
//         spawn player on center of chunksPooling[1, 1]
//        player.transform.position = new Vector3(xStart * chunkSize + 1 * chunkSize + (chunkSize / 2), yStart * chunkSize + 1 * chunkSize + (chunkSize / 2), 0);
//        player.transform.position = new Vector3(366.25f, 368.50f, 0);
//    }

//    private void ManageChunkFromPool(int chunkPosX, int chunkPosY, int xChunkPool, int yChunkPool) {
//        Chunk ck = chunksPooling[xChunkPool, yChunkPool];
//        GameObject chunkGo = ck.gameObject;
//        chunkGo.transform.position = new Vector3(chunkPosX * chunkSize, chunkPosY * chunkSize, 1);
//        ck.player = player;
//        ck.tileMap = tilesMapChunks[chunkPosX, chunkPosY]; // ATTENTION AU OUT OF RANGE !
//        ck.objectMap = objectsMapChunks[chunkPosX, chunkPosY]; // ATTENTION AU OUT OF RANGE !
//        ck.tileMapScript.tilePosX = chunkPosX * chunkSize;
//        ck.tileMapScript.tilePosY = chunkPosY * chunkSize;
//        ck.indexX = chunkPosX;
//        ck.indexY = chunkPosY;
//         disable => create tiles > create one time collider tilemap after Redraw coroutine (attetion used for performce!!!)
//        ck.tm2d.enabled = false;
//        chunkGo.gameObject.SetActive(false);
//        StartCoroutine(Redraw(ck));
//    }

//    private IEnumerator Redraw(Chunk ck) {
//        /*for (var x = 0; x < chunkSize; x++) {
//            for (var y = 0; y < chunkSize; y++) {
//                int tileIndex = ck.chunkMap[x, y];
//                if (tileIndex > 0) {
//                    ck.tilemap.SetTile(new Vector3Int(x, y, 0), tilebaseDictionary[tileIndex]);
//                } else {
//                    ck.tilemap.SetTile(new Vector3Int(x, y, 0), null);
//                }
//            }
//        }*/
//        Vector3Int[] positions = new Vector3Int[chunkSize * chunkSize];
//        TileBase[] tileArray = new TileBase[positions.Length];
//        for (int index = 0; index < positions.Length; index++) {
//            positions[index] = new Vector3Int(index % chunkSize, index / chunkSize, 0);
//            var tileBaseIndex = ck.tileMap[index % chunkSize, index / chunkSize];
//            if (tileBaseIndex != 0) {
//                tileArray[index] = tilebaseDictionary[tileBaseIndex];
//            } else {
//                tileArray[index] = null;
//            }
//        }
//        ck.tilemap.SetTiles(positions, tileArray);
//        yield return new WaitForSeconds(0.01f);
//        ck.tm2d.enabled = true;
//        ck.gameObject.SetActive(true);
//    }

//    private void ChunkCollisionWithPlayer(Vector3 playerPos) {
//         TODO voir a ne le faire que si top bottom left et right > 0 et < length
//        var x = (int)playerPos.x / chunkSize;
//        var y = (int)playerPos.y / chunkSize;
//        /*if (chunksPooling[1, 2].indexY == y) {
//            Debug.Log("top");
//            StartCoroutine(CreateTopChunks(x, y));
//        } else if (chunksPooling[1, 0].indexY == y) {
//            Debug.Log("bottom");
//            StartCoroutine(CreateBottomChunks(x, y));
//        } else if (chunksPooling[0, 1].indexX == x) {
//            Debug.Log("left");
//            StartCoroutine(CreateLeftChunks(x, y));
//        } else if (chunksPooling[2, 1].indexX == x) {
//            Debug.Log("right");
//            StartCoroutine(CreateRightChunks(x, y));
//        }
//    }
//    private IEnumerator CreateRightChunks(int x, int y) {
//        /*       
//         *   O | O | X  -> 1
//         *   O | O | X  -> 2
//         *   O | O | X  -> 3
//        */
//        ManageChunkFromPool(x + 1, y + 1, 0, 2);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x + 1, y, 0, 1);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x + 1, y - 1, 0, 0);
//        RealocatePooling("right");
//    }

//    private IEnumerator CreateLeftChunks(int x, int y) {
//        /*        
//         *   X | O | O  -> 1
//         *   X | O | O  -> 2
//         *   X | O | O  -> 3
//        */
//        ManageChunkFromPool(x - 1, y + 1, 2, 2);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x - 1, y, 2, 1);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x - 1, y - 1, 2, 0);
//        RealocatePooling("left");
//    }

//    private IEnumerator CreateTopChunks(int x, int y) {
//        /*     
//         *   1   2   3
//         *   ---------
//         *   X | X | X
//         *   O | O | O
//         *   O | O | O
//        */
//        ManageChunkFromPool(x - 1, y + 1, 0, 0);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x, y + 1, 1, 0);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x + 1, y + 1, 2, 0);
//        RealocatePooling("top");
//    }

//    private IEnumerator CreateBottomChunks(int x, int y) {
//        /*       
//         *   O | O | O
//         *   O | O | O
//         *   X | X | X
//         *   ---------
//         *   1   2   3
//        */
//        ManageChunkFromPool(x - 1, y - 1, 0, 2);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x, y - 1, 1, 2);
//        yield return new WaitForSeconds(waitingTimeAfterCreateChunk);
//        ManageChunkFromPool(x + 1, y - 1, 2, 2);
//        RealocatePooling("bottom");
//    }

//    private void CreateRow() {
//        var boundX = chunksPooling.GetUpperBound(0);
//        var boundY = chunksPooling.GetUpperBound(1);
//    }

//    private void RealocatePooling(string moveTo) {

//        var chunkCopy = new Chunk[3, 3];

//        if (moveTo == "top") {
//            /*        
//             *   + | + | +   -> row 2
//             *   O | O | O   -> row 1
//             *   O | O | O   -> row 0
//             *   - | - | -  
//            */

//             row 2
//            chunkCopy[0, 2] = chunksPooling[0, 0];
//            chunkCopy[1, 2] = chunksPooling[1, 0];
//            chunkCopy[2, 2] = chunksPooling[2, 0];
//             row 1
//            chunkCopy[0, 1] = chunksPooling[0, 2];
//            chunkCopy[1, 1] = chunksPooling[1, 2];
//            chunkCopy[2, 1] = chunksPooling[2, 2];
//             row 0
//            chunkCopy[0, 0] = chunksPooling[0, 1];
//            chunkCopy[1, 0] = chunksPooling[1, 1];
//            chunkCopy[2, 0] = chunksPooling[2, 1];
//        }

//        if (moveTo == "bottom") {
//             row 2
//            chunkCopy[0, 2] = chunksPooling[0, 1];
//            chunkCopy[1, 2] = chunksPooling[1, 1];
//            chunkCopy[2, 2] = chunksPooling[2, 1];
//             row 1
//            chunkCopy[0, 1] = chunksPooling[0, 0];
//            chunkCopy[1, 1] = chunksPooling[1, 0];
//            chunkCopy[2, 1] = chunksPooling[2, 0];
//             row 0
//            chunkCopy[0, 0] = chunksPooling[0, 2];
//            chunkCopy[1, 0] = chunksPooling[1, 2];
//            chunkCopy[2, 0] = chunksPooling[2, 2];
//        }
//        if (moveTo == "right") {
//             col 0
//            chunkCopy[0, 0] = chunksPooling[1, 0];
//            chunkCopy[0, 1] = chunksPooling[1, 1];
//            chunkCopy[0, 2] = chunksPooling[1, 2];
//             col 1
//            chunkCopy[1, 0] = chunksPooling[2, 0];
//            chunkCopy[1, 1] = chunksPooling[2, 1];
//            chunkCopy[1, 2] = chunksPooling[2, 2];
//             col 2
//            chunkCopy[2, 0] = chunksPooling[0, 0];
//            chunkCopy[2, 1] = chunksPooling[0, 1];
//            chunkCopy[2, 2] = chunksPooling[0, 2];
//        }

//        if (moveTo == "left") {
//             col 0
//            chunkCopy[0, 0] = chunksPooling[2, 0];
//            chunkCopy[0, 1] = chunksPooling[2, 1];
//            chunkCopy[0, 2] = chunksPooling[2, 2];
//             col 1
//            chunkCopy[1, 0] = chunksPooling[0, 0];
//            chunkCopy[1, 1] = chunksPooling[0, 1];
//            chunkCopy[1, 2] = chunksPooling[0, 2];
//             col 2
//            chunkCopy[2, 0] = chunksPooling[1, 0];
//            chunkCopy[2, 1] = chunksPooling[1, 1];
//            chunkCopy[2, 2] = chunksPooling[1, 2];
//        }

//        chunksPooling = chunkCopy;
//    }

//}