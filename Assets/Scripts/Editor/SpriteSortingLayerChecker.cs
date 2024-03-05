#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteSortingLayerChecker : EditorWindow
    {
        private string sortingLayerToCheck = "";
        private string orderInLayerToCheck = "";
        private bool showInactive;
        private bool isComplex;
        private string newSceneLocation = "Scenes";

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
            isComplex = EditorGUILayout.Toggle("Is Complex", isComplex);
            if (isComplex)
            {
                GUILayout.Label("Enter the location of scenes in Assets folder to save the new scene:");
                newSceneLocation = EditorGUILayout.TextField("New Scene Location", newSceneLocation);
            }

            if (GUILayout.Button("Check Sprite Sorting Layers"))
            {
                if (isComplex)
                {
                    CheckSpriteSortingLayersComplex();
                }
                else
                {
                    CheckSpriteSortingLayers();
                }
            }
        }

        #region NormalCheck
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
        #endregion

        #region ComplexCheck
        private void CheckSpriteSortingLayersComplex()
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(currentScenePath))
            {
                Debug.LogWarning("Please save the current scene before cloning.");
                return;
            }
            string newSceneName = "Assets/"+newSceneLocation+"/CheckedScene_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), newSceneName);
            EditorSceneManager.OpenScene(newSceneName);
        }
        #endregion
    }
#endif
