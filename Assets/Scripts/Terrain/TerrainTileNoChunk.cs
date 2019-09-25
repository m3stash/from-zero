using System;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
namespace UnityEngine.Tilemaps {
    [Serializable]
    [CreateAssetMenu(fileName = "New Terrain Tile without chunk", menuName = "Tiles/Terrain Tile No Chunk")]
    public class TerrainTileNoChunk : TileBase {
        [SerializeField]
        public Sprite[] m_Sprites;
        public Texture2D texture2d;
        private int xLength;
        private int yLength;
        private int[,] worldMap;
        private System.Random rand = new System.Random();

        // TODO a remettre et a revoir pour utiliser les positons de world et non de tilemap!
        /*public override void RefreshTile(Vector3Int location, ITilemap tileMap) {
            for (int yd = -1; yd <= 1; yd++) {
                for (int xd = -1; xd <= 1; xd++) {
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                    if (TileValue(tileMap, position)) {
                        tileMap.RefreshTile(position);
                    }
                }
            }
        }*/

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            UpdateTile(location, tileMap, ref tileData);
        }

        private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            tileData.transform = Matrix4x4.identity;
            if (worldMap == null) {
                worldMap = tileMap.GetComponent<TileMapScript>().tilesWorldMap;
                xLength = worldMap.GetUpperBound(0);
                yLength = worldMap.GetUpperBound(1);
            }
            var currentTilePosX = location.x;
            var currentTilePosY = location.y;
            int currentId = worldMap[currentTilePosX, currentTilePosY];

            int xLess = currentTilePosX - 1;
            int xMore = currentTilePosX + 1;
            int yMore = currentTilePosY + 1;
            int yLess = currentTilePosY - 1;
            int mask = 0;

            if (yMore <= yLength) {
                int top = worldMap[currentTilePosX, yMore];
                mask += top > 0 ? 1 : 0;
            }

            if (xMore <= xLength) {
                int right = worldMap[xMore, currentTilePosY];
                mask += right > 0 ? 4 : 0;

                if (yMore <= yLength) {
                    int diaRightTop = worldMap[xMore, yMore];
                    mask += diaRightTop > 0 ? 2 : 0;
                }

                if (yLess > -1) {
                    int diagBottomRight = worldMap[xMore, yLess];
                    mask += diagBottomRight > 0 ? 8 : 0;
                }
            }

            if (yLess > -1) {
                int bottom = worldMap[currentTilePosX, yLess];
                mask += bottom > 0 ? 16 : 0;
            }

            if (xLess > -1) {
                int left = worldMap[xLess, currentTilePosY];
                mask += left > 0 ? 64 : 0;

                if (yLess > -1) {
                    int diagBottomLeft = worldMap[xLess, yLess];
                    mask += diagBottomLeft > 0 ? 32 : 0;
                }

                if (yMore <= yLength) {
                    int diagTopLeft = worldMap[xLess, yMore];
                    mask += diagTopLeft > 0 ? 128 : 0;
                }
            }

            byte original = (byte)mask;
            if ((original | 254) < 255) { mask = mask & 125; }
            if ((original | 251) < 255) { mask = mask & 245; }
            if ((original | 239) < 255) { mask = mask & 215; }
            if ((original | 191) < 255) { mask = mask & 95; }
            int index = GetIndex((byte)mask);
            if (index >= 0) {
                tileData.sprite = m_Sprites[index];
                tileData.transform = GetTransform((byte)mask);
                tileData.flags = TileFlags.LockTransform;
            }
        }

        private bool TileValue(ITilemap tileMap, Vector3Int position) {
            TileBase tile = tileMap.GetTile(position);
            return (tile != null && tile == this);
        }

        private int GetRand(int[] array) {
            return array[rand.Next(0, 3)];
        }
        private int GetIndex(byte mask) {
            switch (mask) {
                case 0:
                    return GetRand(new int[] { 0, 12, 24 });
                case 1:
                case 4:
                case 16:
                case 64:
                    return GetRand(new int[] { 1, 13, 25 });
                case 7:
                case 28:
                case 112:
                case 193:
                    return GetRand(new int[] { 2, 14, 26 });
                case 17:
                case 68:
                    return GetRand(new int[] { 3, 15, 27 });
                case 31:
                case 124:
                case 241:
                case 199:
                    return GetRand(new int[] { 4, 16, 28 });
                case 255:
                    return GetRand(new int[] { 5, 17, 29 });
                case 5:
                case 20:
                case 80:
                case 65:
                    return GetRand(new int[] { 6, 18, 30 });
                case 21:
                case 84:
                case 81:
                case 69:
                    return GetRand(new int[] { 7, 19, 31 });
                case 23:
                case 92:
                case 113:
                case 197:
                    return GetRand(new int[] { 8, 20, 32 });
                case 29:
                case 116:
                case 209:
                case 71:
                    return GetRand(new int[] { 9, 21, 33 });
                case 85:
                    return 10;
                case 87:
                case 93:
                case 117:
                case 213:
                    return 11;
                case 95:
                case 125:
                case 245:
                case 215:
                    return 22;
                case 119:
                case 221:
                    return 23;
                case 127:
                case 253:
                case 247:
                case 223:
                    return 34;
            }
            return -1;
        }

        private Matrix4x4 GetTransform(byte mask) {
            switch (mask) {
                case 4:
                case 20:
                case 28:
                case 68:
                case 84:
                case 92:
                case 116:
                case 124:
                case 93:
                case 125:
                case 221:
                case 253:
                    return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f), Vector3.one);
                case 16:
                case 80:
                case 112:
                case 81:
                case 113:
                case 209:
                case 241:
                case 117:
                case 245:
                case 247:
                    return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -180f), Vector3.one);
                case 64:
                case 65:
                case 193:
                case 69:
                case 197:
                case 71:
                case 199:
                case 213:
                case 215:
                case 223:
                    return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -270f), Vector3.one);
            }
            return Matrix4x4.identity;
        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(TerrainTileNoChunk))]
    public class TerrainTileNoChunkEditor : Editor {
        private TerrainTileNoChunk Tile { get { return (target as TerrainTileNoChunk); } }
        private readonly String folderPath = "Sprites/Tiles/";
        private readonly int numberTiles = 35;

        public void OnEnable() {
            if (Tile.m_Sprites == null || Tile.m_Sprites.Length != numberTiles) {
                Tile.m_Sprites = new Sprite[numberTiles];
                EditorUtility.SetDirty(Tile);
            }
        }

        private void SetTiles(Texture2D texture2d) {
            if (Tile.texture2d) {
                var name = Tile.texture2d.name;
                var tiles = Resources.LoadAll(folderPath + name, typeof(Sprite));
                for (var i = 0; i < numberTiles; i++) {
                    var newName = name + "_" + i;
                    Tile.m_Sprites[i] = (Sprite)EditorGUILayout.ObjectField(newName, tiles[i], typeof(Sprite), false, null);
                }
                EditorUtility.SetDirty(Tile);
            }
        }

        public override void OnInspectorGUI() {
            EditorGUIUtility.labelWidth = 210;
            SetTiles(Tile.texture2d);
            EditorGUI.BeginChangeCheck();
            Tile.texture2d = (Texture2D)EditorGUILayout.ObjectField(Tile.texture2d, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck()) {
                SetTiles(Tile.texture2d);
            }
        }
    }
#endif
}