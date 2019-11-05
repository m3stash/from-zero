using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
namespace UnityEngine.Tilemaps {
    [Serializable]
    [CreateAssetMenu(fileName = "New Terrain Tile", menuName = "Tiles/Terrain Tile")]

    public class TerrainTile : TileBase {

        [SerializeField]
        public Sprite[] m_Sprites;
        public Texture2D texture2d;
        private int xLength;
        private int yLength;
        private int[,] worldMap;
        private System.Random rand = new System.Random();

        public override void RefreshTile(Vector3Int location, ITilemap tileMap) {
            for (int yd = -1; yd <= 1; yd++) {
                for (int xd = -1; xd <= 1; xd++) {
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                    if (TileValue(tileMap, position)) {
                        tileMap.RefreshTile(position);
                    }
                }
            }
            // tileMap.RefreshTile(new Vector3Int(location.x, location.y, location.z));
        }

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            UpdateTile(location, tileMap, ref tileData);
        }

        /*private bool CheckIfSameId(int otherId, int currentId) {
            if (otherId > 0) {
                if((currentId == 1 && otherId == 2) || (currentId == 2 && otherId == 1)) {
                    return true;
                }
                return otherId == currentId;
            }
            return otherId == currentId;
        }*/

        private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            tileData.transform = Matrix4x4.identity;
            tileData.color = Color.white;
            // TODO refacto trop consommateur !
            var tileMapScript = tileMap.GetComponent<TileMapScript>();
            if (worldMap == null) {
                worldMap = tileMapScript.tilesWorldMap;
                xLength = worldMap.GetUpperBound(0);
                yLength = worldMap.GetUpperBound(1);
            }
            var currentTilePosX = tileMapScript.tilePosX + location.x;
            var currentTilePosY = tileMapScript.tilePosY + location.y;
            int currentId = worldMap[currentTilePosX, currentTilePosY];

            int xLess = currentTilePosX - 1;
            int xMore = currentTilePosX + 1;
            int yMore = currentTilePosY + 1;
            int yLess = currentTilePosY - 1;
            int mask = 0;

            if (yMore <= yLength) {
                int top = worldMap[currentTilePosX, yMore];
                // top = CheckIfSameId(top, currentId) ? top : 0;
                mask += top > 0 ? 1 : 0;
            }

            if (xMore <= xLength) {
                int right = worldMap[xMore, currentTilePosY];
                // right = CheckIfSameId(right, currentId) ? right : 0;
                mask += right > 0 ? 4 : 0;

                if (yMore <= yLength) {
                    int diaRightTop = worldMap[xMore, yMore];
                    // diaRightTop = CheckIfSameId(diaRightTop, currentId) ? diaRightTop : 0;
                    mask += diaRightTop > 0 ? 2 : 0;
                }

                if (yLess > -1) {
                    int diagBottomRight = worldMap[xMore, yLess];
                    // diagBottomRight = CheckIfSameId(diagBottomRight, currentId) ? diagBottomRight : 0;
                    diagBottomRight = currentId;
                    mask += diagBottomRight > 0 ? 8 : 0;
                }
            }

            if (yLess > -1) {
                int bottom = worldMap[currentTilePosX, yLess];
                // bottom = CheckIfSameId(bottom, currentId) ? bottom : 0;
                mask += bottom > 0 ? 16 : 0;
            }

            if (xLess > -1) {
                int left = worldMap[xLess, currentTilePosY];
                // left = CheckIfSameId(left, currentId) ? left : 0;
                mask += left > 0 ? 64 : 0;

                if (yLess > -1) {
                    int diagBottomLeft = worldMap[xLess, yLess];
                    // diagBottomLeft = CheckIfSameId(diagBottomLeft, currentId) ? diagBottomLeft : 0;
                    mask += diagBottomLeft > 0 ? 32 : 0;
                }

                if (yMore <= yLength) {
                    int diagTopLeft = worldMap[xLess, yMore];
                    // diagTopLeft = CheckIfSameId(diagTopLeft, currentId) ? diagTopLeft : 0;
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
                tileData.colliderType = Tile.ColliderType.Grid;
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
    [CustomEditor(typeof(TerrainTile))]
    public class TerrainTileEditor : Editor {
        private TerrainTile Tile { get { return (target as TerrainTile); } }
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
                AssetDatabase.SaveAssets(); // toDo tester si ça marche !
                SetTiles(Tile.texture2d);
            }
        }

        /*public override void OnInspectorGUI() {
            EditorGUIUtility.labelWidth = 210;
            if (Tile.texture2d) {
                var name = Tile.texture2d.name;
                var folderName = folderPath + "/" + name + "/";
                for (var i = 0; i < numberTiles; i++) {
                    var newName = name + "_" + i;
                    var sprite = AssetDatabase.LoadAssetAtPath(folderName + newName + ".asset", typeof(Sprite)) as Sprite;
                    Tile.m_Sprites[i] = (Sprite)EditorGUILayout.ObjectField(newName, sprite, typeof(Sprite), false, null);
                }
                EditorUtility.SetDirty(Tile);
            }
            EditorGUI.BeginChangeCheck();
            Tile.texture2d = (Texture2D)EditorGUILayout.ObjectField(Tile.texture2d, typeof(Texture2D), false);

            if (EditorGUI.EndChangeCheck()) {
                if (Tile.texture2d) {
                    var numberOfTileX = Tile.texture2d.width / tileWidth;
                    var numberOfTileY = Tile.texture2d.height / tileWidth;
                    var name = Tile.texture2d.name;
                    var newFolderName = folderPath + "/" + name;
                    if (!IsFolderExist(newFolderName)) {
                        CreateFolder(name);
                    }
                    var count = 0;
                    for (var x = 0; x < numberOfTileX; x++) {
                        for (var y = 0; y < numberOfTileY; y++) {
                            var newName = name + "_" + count;
                            // texture2D > rect > pivot > pixel per unit > extrude edges > mesh type, border
                            var sprite = Sprite.Create(Tile.texture2d, new Rect(x * tileWidth, y * tileWidth, tileWidth, tileWidth), new Vector2(0, 0), pixelPerUnit, 0, SpriteMeshType.Tight, Vector4.zero);
                            AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(sprite), newFolderName + "/" + newName + ".asset");
                            Tile.m_Sprites[count] = (Sprite)EditorGUILayout.ObjectField(newName, sprite, typeof(Sprite), false, null);
                            count++;
                        }
                    }
                    EditorUtility.SetDirty(Tile);
                }
            }
        }*/
    }
#endif
}