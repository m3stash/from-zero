/*using System;
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

        int xLength;
        int yLength;
        private TileMapScript tileMapScript;
        private int[,] worldMap;

        public override void RefreshTile(Vector3Int location, ITilemap tileMap) {
            for (int yd = -1; yd <= 1; yd++) {
                for (int xd = -1; xd <= 1; xd++) {
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                    if (TileValue(tileMap, position)) {
                        tileMap.RefreshTile(position);
                    }
                }
            }
        }

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            UpdateTile(location, tileMap, ref tileData);
        }

        private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            tileData.transform = Matrix4x4.identity;
            tileData.color = Color.white;

            if (!tileMapScript) {
                tileMapScript = tileMap.GetComponent<TileMapScript>();
                worldMap = tileMapScript.worldMap;
                xLength = worldMap.GetUpperBound(0);
                yLength = worldMap.GetUpperBound(1);
            }

            int currentId = worldMap[location.x, location.y];
            int xLess = location.x - 1;
            int xMore = location.x + 1;
            int yMore = location.y + 1;
            int yLess = location.y - 1;
            int mask = 0;

            if (yMore <= yLength) {
                int top = worldMap[location.x, yMore];
                top = top == currentId ? top : 0;
                mask += top > 0 ? 1 : 0;
            }

            if (xMore <= xLength) {
                int right = worldMap[xMore, location.y];
                right = right == currentId ? right : 0;
                mask += right > 0 ? 4 : 0;

                if (yMore <= yLength) {
                    int diaRightTop = worldMap[xMore, yMore];
                    diaRightTop = diaRightTop == currentId ? diaRightTop : 0;
                    mask += diaRightTop > 0 ? 2 : 0;
                }

                if (yLess > -1) {
                    int diagBottomRight = worldMap[xMore, yLess];
                    diagBottomRight = diagBottomRight == currentId ? diagBottomRight : 0;
                    mask += diagBottomRight > 0 ? 8 : 0;
                }
            }

            if (yLess > -1) {
                int bottom = worldMap[location.x, yLess];
                bottom = bottom == currentId ? bottom : 0;
                mask += bottom > 0 ? 16 : 0;
            }

            if (xLess > -1) {
                int left = worldMap[xLess, location.y];
                left = left == currentId ? left : 0;
                mask += left > 0 ? 64 : 0;

                if (yLess > -1) {
                    int diagBottomLeft = worldMap[xLess, yLess];
                    diagBottomLeft = diagBottomLeft == currentId ? diagBottomLeft : 0;
                    mask += diagBottomLeft > 0 ? 32 : 0;
                }

                if (yMore <= yLength) {
                    int diagTopLeft = worldMap[xLess, yMore];
                    diagTopLeft = diagTopLeft == currentId ? diagTopLeft : 0;
                    mask += diagTopLeft > 0 ? 128 : 0;
                }
            }

            byte original = (byte)mask;
            if ((original | 254) < 255) { mask = mask & 125; }
            if ((original | 251) < 255) { mask = mask & 245; }
            if ((original | 239) < 255) { mask = mask & 215; }
            if ((original | 191) < 255) { mask = mask & 95; }
            int index = GetIndex((byte)mask);
            if (index >= 0 && index < m_Sprites.Length && TileValue(tileMap, location)) {
                tileData.sprite = m_Sprites[index];
                tileData.transform = GetTransform((byte)mask);
                tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
                tileData.colliderType = Tile.ColliderType.Sprite;
            }
        }

        private bool TileValue(ITilemap tileMap, Vector3Int position) {
            TileBase tile = tileMap.GetTile(position);
            return (tile != null && tile == this);
        }

        private int GetIndex(byte mask) {
            switch (mask) {
                case 0: return 0;
                case 1:
                case 4:
                case 16:
                case 64: return 1;
                case 5:
                case 20:
                case 80:
                case 65: return 2;
                case 7:
                case 28:
                case 112:
                case 193: return 3;
                case 17:
                case 68: return 4;
                case 21:
                case 84:
                case 81:
                case 69: return 5;
                case 23:
                case 92:
                case 113:
                case 197: return 6;
                case 29:
                case 116:
                case 209:
                case 71: return 7;
                case 31:
                case 124:
                case 241:
                case 199: return 8;
                case 85: return 9;
                case 87:
                case 93:
                case 117:
                case 213: return 10;
                case 95:
                case 125:
                case 245:
                case 215: return 11;
                case 119:
                case 221: return 12;
                case 127:
                case 253:
                case 247:
                case 223: return 13;
                case 255: return 14;
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

        public void OnEnable() {
            if (Tile.m_Sprites == null || Tile.m_Sprites.Length != 15) {
                Tile.m_Sprites = new Sprite[15];
                EditorUtility.SetDirty(Tile);
            }
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.LabelField("Place sprites shown based on the contents of the sprite.");
            EditorGUILayout.Space();

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 210;

            EditorGUI.BeginChangeCheck();
            Tile.m_Sprites[0] = (Sprite)EditorGUILayout.ObjectField("Filled", Tile.m_Sprites[0], typeof(Sprite), false, null);
            Tile.m_Sprites[1] = (Sprite)EditorGUILayout.ObjectField("Three Sides", Tile.m_Sprites[1], typeof(Sprite), false, null);
            Tile.m_Sprites[2] = (Sprite)EditorGUILayout.ObjectField("Two Sides and One Corner", Tile.m_Sprites[2], typeof(Sprite), false, null);
            Tile.m_Sprites[3] = (Sprite)EditorGUILayout.ObjectField("Two Adjacent Sides", Tile.m_Sprites[3], typeof(Sprite), false, null);
            Tile.m_Sprites[4] = (Sprite)EditorGUILayout.ObjectField("Two Opposite Sides", Tile.m_Sprites[4], typeof(Sprite), false, null);
            Tile.m_Sprites[5] = (Sprite)EditorGUILayout.ObjectField("One Side and Two Corners", Tile.m_Sprites[5], typeof(Sprite), false, null);
            Tile.m_Sprites[6] = (Sprite)EditorGUILayout.ObjectField("One Side and One Lower Corner", Tile.m_Sprites[6], typeof(Sprite), false, null);
            Tile.m_Sprites[7] = (Sprite)EditorGUILayout.ObjectField("One Side and One Upper Corner", Tile.m_Sprites[7], typeof(Sprite), false, null);
            Tile.m_Sprites[8] = (Sprite)EditorGUILayout.ObjectField("One Side", Tile.m_Sprites[8], typeof(Sprite), false, null);
            Tile.m_Sprites[9] = (Sprite)EditorGUILayout.ObjectField("Four Corners", Tile.m_Sprites[9], typeof(Sprite), false, null);
            Tile.m_Sprites[10] = (Sprite)EditorGUILayout.ObjectField("Three Corners", Tile.m_Sprites[10], typeof(Sprite), false, null);
            Tile.m_Sprites[11] = (Sprite)EditorGUILayout.ObjectField("Two Adjacent Corners", Tile.m_Sprites[11], typeof(Sprite), false, null);
            Tile.m_Sprites[12] = (Sprite)EditorGUILayout.ObjectField("Two Opposite Corners", Tile.m_Sprites[12], typeof(Sprite), false, null);
            Tile.m_Sprites[13] = (Sprite)EditorGUILayout.ObjectField("One Corner", Tile.m_Sprites[13], typeof(Sprite), false, null);
            Tile.m_Sprites[14] = (Sprite)EditorGUILayout.ObjectField("Empty", Tile.m_Sprites[14], typeof(Sprite), false, null);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(Tile);

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
#endif
}
*/