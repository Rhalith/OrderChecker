#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpriteSortingLayerChecker : EditorWindow
    {
        private string sortingLayerToCheck = "";
        private string orderInLayerToCheck = "";
        private bool showInactive = false;

        [MenuItem("Tools/Sprite Sorting Layer Checker")]
        public static void ShowWindow()
        {
            GetWindow<SpriteSortingLayerChecker>("Sprite Sorting Layer Checker");
        }

        void OnGUI()
        {
            GUILayout.Label("Enter a Sorting Layer name to check, or leave empty to check all:",
                EditorStyles.boldLabel);
            sortingLayerToCheck = EditorGUILayout.TextField("Sorting Layer Name", sortingLayerToCheck);
            GUILayout.Label("Enter an Order in Layer to check, or leave empty to check all:", EditorStyles.boldLabel);
            orderInLayerToCheck = EditorGUILayout.TextField("Order in Layer", orderInLayerToCheck);
            GUILayout.Label("Show inactive objects:", EditorStyles.boldLabel);
            showInactive = EditorGUILayout.Toggle("Show Inactive", showInactive);

            if (GUILayout.Button("Check Sprite Sorting Layers"))
            {
                CheckSpriteSortingLayers();
            }
        }

        private void CheckSpriteSortingLayers()
        {
            Dictionary<string, List<GameObject>> sortingGroups = new Dictionary<string, List<GameObject>>();
            bool checkAllSortingLayers = string.IsNullOrEmpty(sortingLayerToCheck);
            bool checkSpecificOrder = int.TryParse(orderInLayerToCheck, out int orderInLayer);

            foreach (GameObject obj in FindObjectsOfType(typeof(GameObject), showInactive))
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    bool layerMatch = checkAllSortingLayers || renderer.sortingLayerName == sortingLayerToCheck;
                    bool orderMatch = !checkSpecificOrder || renderer.sortingOrder == orderInLayer;
                    
                    if (renderer.sortingLayerName.Equals("Default", System.StringComparison.Ordinal))
                    {
                        Debug.LogWarning($"GameObject '{obj.name}' is using the 'Default' sorting layer.", obj);
                    }

                    if (layerMatch && orderMatch)
                    {
                        string key = renderer.sortingLayerName + "_" + renderer.sortingOrder;
                        if (!sortingGroups.ContainsKey(key))
                        {
                            sortingGroups[key] = new List<GameObject>();
                        }

                        sortingGroups[key].Add(obj);
                    }
                }
            }

            DisplayResults(sortingGroups);
        }

        private void DisplayResults(Dictionary<string, List<GameObject>> sortingGroups)
        {
            foreach (var group in sortingGroups)
            {
                if (group.Value.Count > 1)
                {
                    Debug.Log(
                        $"---------------- Group: {group.Key}, Objects Count: {group.Value.Count} ----------------");
                    foreach (var obj in group.Value)
                    {
                        Debug.Log(obj.name, obj);
                    }
                }
            }
        }
    }
#endif
