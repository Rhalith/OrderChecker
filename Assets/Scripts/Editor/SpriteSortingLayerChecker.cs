#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Rhalith.Sorting_Layer_Checker.Scripts.Editor
{
    public class SpriteSortingLayerChecker : EditorWindow
    {
        private string sortingLayerToCheck = "";
        private string orderInLayerToCheck = "";
        private bool showInactive;
        private bool isSceneCreation;
        private string newSceneLocation = "Scenes";

        [MenuItem("Tools/Sprite Sorting Layer Checker")]
        public static void ShowWindow()
        {
            GetWindow<SpriteSortingLayerChecker>("Sprite Sorting Layer Checker");
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter a Sorting Layer name to check, or leave empty to check all:",
                EditorStyles.boldLabel);
            sortingLayerToCheck = EditorGUILayout.TextField("Sorting Layer Name", sortingLayerToCheck);
            GUILayout.Label("Enter an Order in Layer to check, or leave empty to check all:", EditorStyles.boldLabel);
            orderInLayerToCheck = EditorGUILayout.TextField("Order in Layer", orderInLayerToCheck);
            GUILayout.Label("Show inactive objects:", EditorStyles.boldLabel);
            showInactive = EditorGUILayout.Toggle("Show Inactive", showInactive);
            isSceneCreation = EditorGUILayout.Toggle("Create scene", isSceneCreation);
            if (isSceneCreation)
            {
                GUILayout.Label("Enter the location of scenes in Assets folder to save the new scene:");
                newSceneLocation = EditorGUILayout.TextField("New Scene Location", newSceneLocation);
            }

            if (GUILayout.Button("Check Sprite Sorting Layers"))
            {
                if (isSceneCreation)
                {
                    bool proceed = EditorUtility.DisplayDialog("Confirm Action", "Are you sure you want to create a new scene for checking?", "Yes", "No");
                    if (proceed)
                    {
                        CreateSceneForChecking();
                    }
                }
                else
                {
                    bool proceed = EditorUtility.DisplayDialog("Confirm Action", "Are you sure you want to check sprite sorting layers?", "Yes", "No");
                    if (proceed)
                    {
                        CheckSpriteSortingLayers();
                    }
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

        #region SceneCreation
        private void CreateSceneForChecking()
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(currentScenePath))
            {
                Debug.LogWarning("Please save the current scene before cloning.");
                return;
            }
            string newSceneName = "Assets/"+newSceneLocation+"/CheckedScene_"+ EditorSceneManager.GetActiveScene().name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), newSceneName);
            EditorSceneManager.OpenScene(newSceneName);
            
            Dictionary<Tuple<string, int>, List<GameObject>> sortingGroups = new ();
            
            bool checkAllSortingLayers = string.IsNullOrEmpty(sortingLayerToCheck);
            bool checkSpecificOrder = int.TryParse(orderInLayerToCheck, out int orderInLayer);

            foreach (GameObject obj in FindObjectsOfType(typeof(GameObject), showInactive))
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    bool layerMatch = checkAllSortingLayers || renderer.sortingLayerName == sortingLayerToCheck;
                    bool orderMatch = !checkSpecificOrder || renderer.sortingOrder == orderInLayer;
                    
                    if (layerMatch && orderMatch)
                    {
                        Tuple<string, int> key = new Tuple<string, int>(renderer.sortingLayerName, renderer.sortingOrder);
                        if (!sortingGroups.ContainsKey(key))
                        {
                            sortingGroups[key] = new List<GameObject>();
                        }

                        sortingGroups[key].Add(obj);
                        
                    }
                }
            }
            ChangeCreatedSceneHiearchy(sortingGroups);
        }
        
        private void ChangeCreatedSceneHiearchy(Dictionary<Tuple<string, int>, List<GameObject>> sortingGroups)
        {
            Dictionary<string, GameObject> layerDictionary = new Dictionary<string, GameObject>();

            foreach (var group in sortingGroups)
            {
                string layerName = group.Key.Item1;
                int orderInLayer = group.Key.Item2;

                if (!layerDictionary.ContainsKey(layerName))
                {
                    GameObject layerGameObject = new GameObject();
                    layerGameObject.name = layerName;
                    layerDictionary[layerName] = layerGameObject;
                }

                GameObject layerObject = layerDictionary[layerName];

                string orderName = orderInLayer.ToString();
                Transform orderTransform = layerObject.transform.Find(orderName);

                if (orderTransform == null)
                {
                    GameObject orderGameObject = new GameObject();
                    orderGameObject.name = orderName;
                    orderGameObject.transform.SetParent(layerObject.transform);
                    orderTransform = orderGameObject.transform;
                }

                foreach (var obj in group.Value)
                {
                    obj.transform.SetParent(orderTransform);
                }
            }
        }
        #endregion
    }
}
#endif
