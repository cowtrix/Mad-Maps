#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
#pragma warning disable CS0612 // Type or member is obsolete
using MadMaps.Terrains;
using MadMaps.WorldStamps;
using MadMaps.Common;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MadMaps.Common.Collections;
using System.Linq;

namespace MadMaps
{

    [InitializeOnLoad]
    public static class Updater2018_3
    {
        public class Logger
        {
            public struct LogMsg
            {
                public string Header;
                public string Message;
                public Exception Exception;
                public UnityEngine.Object Context;
                public bool GUIExpanded;

                public bool CanExpand()
                {
                    return !string.IsNullOrEmpty(Message) || Exception != null || Context != null;
                }
            }
            public List<LogMsg> Messages = new List<LogMsg>();
            public void Log(string header, string message = null, UnityEngine.Object context = null, Exception exception = null)
            {
                Messages.Add(new LogMsg()
                {
                    Header = header,
                    Message = message,
                    Context = context,
                    Exception = exception,
                });
                if(exception != null)
                {
                    Debug.LogError(header + "\n" + message);
                    Debug.LogException(exception);
                }
                else
                {
                    Debug.Log(header + "\n" + message);
                }
            }
        }
        public static Logger Log = new Logger();

        [Serializable]
        public class LinkDictionary : Dictionary<string, string>
        { }

        public static LinkDictionary UpdateLinks
        {
            get;
            set;
        }
        public static EditorPersistantVal<bool> LiveUpgraderEnabled;

        public static void ResetHash()
        {
            var projectHash = Application.dataPath.GetHashCode();
            var projectKey = "MadMaps_2018_3_" + projectHash;
            if (EditorPrefs.HasKey(projectKey))
            {
                EditorPrefs.DeleteKey(projectKey);
            }
        }

        static Updater2018_3()
        {
            LiveUpgraderEnabled = new EditorPersistantVal<bool>("MadMaps_2018_3_Upgrader_LiveUpgradeEnabled", true);
            UpdateLinks = new LinkDictionary();// = new EditorPersistantVal<LinkDictionary>("MadMaps_2018_3_Upgrader_UpdateLinks", new LinkDictionary());
            var projectHash = Application.dataPath.GetHashCode();
            var projectKey = "MadMaps_2018_3_" + projectHash;
            if (EditorPrefs.HasKey(projectKey))
            {
                return;
            }
            EditorPrefs.SetBool(projectKey, true);            
            Updater2018_3Window.Open();
        }

        public static List<string> GetAssetsToUpgrade()
        {
            var allPrefabs = AssetDatabase.FindAssets("t:prefab");
            var assetsToUpgrade = new List<string>();
            foreach (var guid in allPrefabs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                prefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (prefab == null)
                {
                    Debug.LogWarning("Failed to load prefab at " + path);
                    continue;
                }
                
                var worldStamps = prefab.GetComponentsInChildren<WorldStamps.WorldStamp>();
                if(worldStamps.Length > 0)
                {
                    assetsToUpgrade.Add(path);
                    UnityEngine.Object.DestroyImmediate(prefab);
                    continue;
                }

                var wrappers = prefab.GetComponentsInChildren<TerrainWrapper>();
                if (wrappers.Length > 0)
                {
                    assetsToUpgrade.Add(path);
                    UnityEngine.Object.DestroyImmediate(prefab);
                    continue;
                }
                UnityEngine.Object.DestroyImmediate(prefab);
            }
            return assetsToUpgrade;
        }

        public static void Upgrade()
        { 
            var allPrefabs = AssetDatabase.FindAssets("t:prefab");
            foreach (var guid in allPrefabs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                prefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (prefab == null)
                {
                    Log.Log("Failed to load prefab at " + path, "We tried to load the prefab at this path, but failed to do so.");
                    continue;
                }

                //Debug.Log("Loaded prefab at " + path, prefab);
                var componentFound = false;
                var worldStamps = prefab.GetComponentsInChildren<WorldStamps.WorldStamp>();
                foreach (var worldStamp in worldStamps)
                {
                    // Upgrade WS
                    UpgradeStamp(worldStamp, true);
                    componentFound = true;
                }

                var wrappers = prefab.GetComponentsInChildren<TerrainWrapper>();
                foreach (var wrapper in wrappers)
                {
                    // Upgrade wrappers
                    UpgradeWrapper(wrapper, true);
                    componentFound = true;
                }

                if (componentFound)
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, path, InteractionMode.AutomatedAction);
                    AssetDatabase.ImportAsset(path);
                }
                UnityEngine.Object.DestroyImmediate(prefab);
            }
            AssetDatabase.SaveAssets();
        }

        public static void UpgradeStamp(WorldStamp stamp, bool resolve)
        {
            foreach (var splatData in stamp.Data.SplatData)
            {
                if (splatData.LegacyWrapper != null)
                {
                    try
                    {
                        string legacyGuid;
                        long localID;
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(splatData.LegacyWrapper, out legacyGuid, out localID);
                        var newGuid = GetBestTerrainLayer(legacyGuid, resolve);
                        splatData.Layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(AssetDatabase.GUIDToAssetPath(newGuid));
                    }
                    catch(Exception e)
                    {
                        Log.Log(string.Format("Failed to upgrade World Stamp {0}. Please email the developer with the console output.", stamp.name), null, stamp, e);
                        return;
                    }
                }
            }
            Log.Log(string.Format("Upgraded World Stamp {0}", stamp.name), null, stamp);
            stamp.HasUpgraded_2018_3 = true;
            EditorUtility.SetDirty(stamp.gameObject);
           
        }

        public static void UpgradeWrapper(TerrainWrapper wrapper, bool resolve)
        {
            foreach (var layer in wrapper.Layers)
            {
                var bLayer = layer as MMTerrainLayer;
                if (bLayer == null)
                {
                    continue;
                }
                foreach (var splat in bLayer.SplatData)
                {
                    try
                    {
                        var legacyGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(splat.Key));
                        var newGuid = GetBestTerrainLayer(legacyGuid, resolve);
                        var terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(AssetDatabase.GUIDToAssetPath(newGuid));
                        bLayer.TerrainLayerSplatData[terrainLayer] = splat.Value;
                    }
                    catch (Exception e)
                    {
                        Log.Log(string.Format("Failed to upgrade Terrain Wrapper {0}. Please email the developer with the console output.", wrapper.name), null, wrapper, e);
                        return;
                    }
                }                
            }
            var layers = new List<TerrainLayer>();
            foreach (var splatWrapper in wrapper.SplatPrototypes)
            {
                try
                {
                    var legacyGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(splatWrapper));
                    var newGuid = UpdateLinks[legacyGuid];
                    var terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(AssetDatabase.GUIDToAssetPath(newGuid));
                    layers.Add(terrainLayer);
                }
                catch (Exception e)
                {
                    Log.Log(string.Format("Failed to upgrade wrapper {0}. Please email the developer with the console output.", wrapper.name), null, wrapper, e);
                    return;
                }
            }
            wrapper.TerrainLayerSplatPrototypes = layers;
            Log.Log(string.Format("Upgraded Terrain Wrapper {0}", wrapper.name), null, wrapper);
            wrapper.HasUpgraded_2018_3 = true;
            EditorUtility.SetDirty(wrapper.gameObject);
        }

        private static string GetBestTerrainLayer(string legacyWrapperGuid, bool resolve)
        {
            Func<SplatPrototypeWrapper, TerrainLayer, int> compare = (wrapper, layer) =>
            {
                int score = 0;
                if(wrapper.Texture == layer.diffuseTexture)
                {
                    score++;
                }
                if (wrapper.NormalMap == layer.normalMapTexture)
                {
                    score++;
                }
                return score;
            };
            string candidate = null;
            if (UpdateLinks.TryGetValue(legacyWrapperGuid, out candidate) && !string.IsNullOrEmpty(candidate))
            {
                return candidate;
            }
            var terrainLayerGuids = AssetDatabase.FindAssets("t: TerrainLayer");
            var terrainLayers = new List<TerrainLayer>();
            var legacyWrapperPath = AssetDatabase.GUIDToAssetPath(legacyWrapperGuid);
            var legacyWrapper = AssetDatabase.LoadAssetAtPath<SplatPrototypeWrapper>(legacyWrapperPath);
            if(string.IsNullOrEmpty(legacyWrapperPath))
            {
                Log.Log("Failed to load SplatPrototypeWrapper at " + legacyWrapperPath);
            }
            foreach (var guid in terrainLayerGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
                if (layer == null)
                {
                    Debug.LogError("Failed to load Terrain Layer at path " + path);
                    continue;
                }
                terrainLayers.Add(layer);
            }
            var bestLayer = terrainLayers.Where((x) => compare(legacyWrapper, x) > 0).OrderBy((x) => compare(legacyWrapper, x)).FirstOrDefault();
            if(bestLayer != null)
            {
                long instanceID;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(bestLayer, out candidate, out instanceID);
            }
            if(resolve && string.IsNullOrEmpty(candidate))
            {
                if(EditorUtility.DisplayDialog("Mad Maps 2018.3 Updater", string.Format("Couldn't find a good match for SplatPrototypeWrapper {0}.", legacyWrapper.name), "Manually Choose", "Create New"))
                {
                    UpdateLinks[legacyWrapperGuid] = ManuallyChooseTerrainLayer(legacyWrapper);
                }
                else
                {
                    UpdateLinks[legacyWrapperGuid] = CreateNewTerrainLayer(legacyWrapper);
                }
            }
            return candidate;
        }

        public static string CreateNewTerrainLayer(SplatPrototypeWrapper wrapper)
        {
            var upgradePath = "Mad Maps 2018.3 Upgrade";
            if (!AssetDatabase.IsValidFolder("Assets/" + upgradePath))
            {
                AssetDatabase.CreateFolder("Assets", upgradePath);
            }
            var fileName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + upgradePath + "/TerrainLayer_" + wrapper.name + ".asset");
            var terrainLayer = new TerrainLayer();
            terrainLayer.diffuseTexture = wrapper.Texture;
            terrainLayer.normalMapTexture = wrapper.NormalMap;
            terrainLayer.name = wrapper.name;
            terrainLayer.metallic = wrapper.Metallic;
            terrainLayer.smoothness = wrapper.Smoothness;
            terrainLayer.tileSize = wrapper.TileSize;
            terrainLayer.tileOffset = wrapper.TileOffset;
            terrainLayer.specular = wrapper.SpecularColor;
            AssetDatabase.CreateAsset(terrainLayer, fileName);
            return fileName;
        }

        private static string ManuallyChooseTerrainLayer(SplatPrototypeWrapper legacyWrapper)
        {
            var newTerrainLayerPath = EditorUtility.OpenFilePanel("Choose Matching Terrain Layer for " + legacyWrapper.name, "Assets\\", "asset");
            var terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(newTerrainLayerPath);
            if (terrainLayer == null)
            {
                throw new Exception(string.Format("Failed to load Terrain Layer at {0}.", newTerrainLayerPath));
            }
            string guid;
            long instanceID;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(terrainLayer, out guid, out instanceID);
            return guid;
        }

        public static void PopulateLinks()
        {
            var links = UpdateLinks;
            var allWrapperGuids = AssetDatabase.FindAssets("t:SplatPrototypeWrapper");
            foreach(var wrapperGuid in allWrapperGuids)
            {
                if(links.ContainsKey(wrapperGuid) && !string.IsNullOrEmpty(links[wrapperGuid]))
                {
                    continue;
                }
                var layerGuid = GetBestTerrainLayer(wrapperGuid, false);
                links[wrapperGuid] = layerGuid;
            }
            Debug.LogFormat("Found {0} wrappers", links.Count);
        }
    }


}
#endif
