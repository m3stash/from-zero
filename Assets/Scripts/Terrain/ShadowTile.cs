using System;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
namespace UnityEngine.Tilemaps {
    [Serializable]
    [CreateAssetMenu(fileName = "Shadow_tile", menuName = "Tiles/Shadow tile")]
    public class ShadowTile : TileBase {
        [SerializeField]
        public Sprite[] m_Sprites;
        public Texture2D texture2d;

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            UpdateTile(location, tileMap, ref tileData);
        }

        private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
            tileData.flags = TileFlags.None;
            tileData.sprite = m_Sprites[0];
        }

        private bool TileValue(ITilemap tileMap, Vector3Int position) {
            TileBase tile = tileMap.GetTile(position);
            return (tile != null && tile == this);
        }
        
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ShadowTile))]
    public class TerrainTileEditor2 : Editor {
        private ShadowTile Tile { get { return (target as ShadowTile); } }
        private readonly String folderPath = "Sprites/Tiles/";
        private readonly int numberTiles = 1;

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