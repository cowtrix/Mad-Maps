#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
#pragma warning disable CS0612 // Type or member is obsolete
using MadMaps.Terrains;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace MadMaps
{
    public class Updater2018_3Window : EditorWindow
    {
        enum EWindowState
        {
            INTRO,
            LINK,
            LOG,
        }
        EWindowState WindowState = EWindowState.INTRO;
        const string GuiContent = @"This tool will upgrade pre-2018.3 Mad Maps prefabs to support the new terrain system. ";
        bool _howDoIWorkExpanded;
        const string HowDoIWork = "1. The tool will find every prefab in the project that has a WorldStamp or TerrainWrapper component. \n" +
            "2. It will look in those components for Splat Layer information.\n" +
            "3. For each splat layer that is still inked to a SplatPrototypeWrapper, the tool will attempt to find a good candidate TerrainLayer object in your project.\n" +
            "4. The tool will ask you if you'd like to use the reccomended TerrainLayer, manually pick a TerrainLayer, or create a new TerrainLayer.\n" +
            "5. By default this will create a link between the SplatPrototypeWrapper and the TerrainLayer, and will replace any other SplatPrototypeWrapper references of that SplatPrototypeWrapper with that TerrainLayer.\n";
        const string Warning = @"It is strongly reccomended that you backup the project before running this tool.";
        bool _whatIsLiveUpgraderExpanded;
        const string WhatIsLiveUpgrader = "Live Upgrader will run in the background of your editor and upgrade assets as they are loaded in scenes. This is important for upgrading the assets that live in your Unity scenes. Disabling the Live Updater will risk losing splat information in components that are not in prefabs.";
        List<string> _assetsToUpgrade = null;        
        bool _hasRefreshedWrappers = false;

        [MenuItem("Tools/Mad Maps/Run 2018.3 Updater")]
        public static void RunFromMenu()
        {
            Updater2018_3.ResetHash();
            Open();
        }

        public static void Open()
        {
            var window = GetWindow<Updater2018_3Window>("Mad Maps 2018.3 Updater");
            window.minSize = new Vector2(700, 500);
        }

        private void OnEnable()
        {
            _assetsToUpgrade = Updater2018_3.GetAssetsToUpgrade();
        }

        private void OnGUI()
        {
            switch(WindowState)
            {
                case EWindowState.INTRO:
                    FirstWindowGUI();
                    break;
                case EWindowState.LINK:
                    SetupLinksGUI();
                    break;
                case EWindowState.LOG:
                    DisplayProgessGUI();
                    break;
            }
        }

        Vector2 _firstWindowScroll;
        void FirstWindowGUI()
        {
            EditorGUILayout.LabelField(GuiContent, EditorStyles.wordWrappedLabel);
            _firstWindowScroll = EditorGUILayout.BeginScrollView(_firstWindowScroll, EditorStyles.textArea, GUILayout.MaxHeight(250));
            EditorGUILayout.SelectableLabel(_assetsToUpgrade.Aggregate("", (current, next) => current + "\n" + next), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            _howDoIWorkExpanded = EditorGUILayout.Foldout(_howDoIWorkExpanded, "How Does This Tool Work?");
            if (_howDoIWorkExpanded)
            {
                EditorGUILayout.LabelField(HowDoIWork, EditorStyles.textArea);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Enable Live Upgrader (Recommended)"));
            Updater2018_3.LiveUpgraderEnabled.Value = EditorGUILayout.Toggle(Updater2018_3.LiveUpgraderEnabled.Value);
            EditorGUILayout.EndHorizontal();
            _whatIsLiveUpgraderExpanded = EditorGUILayout.Foldout(_whatIsLiveUpgraderExpanded, "What Is Live Upgrader?");
            if (_whatIsLiveUpgraderExpanded)
            {
                EditorGUILayout.LabelField(WhatIsLiveUpgrader, EditorStyles.textArea);
            }
            EditorGUILayout.HelpBox(Warning, MessageType.Warning);
            if(GUILayout.Button("I Made a Backup, Go Ahead!", GUILayout.Height(60)))
            {
                Updater2018_3.Log.Messages.Clear();
                WindowState = EWindowState.LINK;
            }
        }

        Vector2 _linksScroll;
        void SetupLinksGUI()
        {
            _linksScroll = EditorGUILayout.BeginScrollView(_linksScroll);
            bool layerFailedToLoad = false;
            var wrapperGuidsToClear = new List<string>();
            var wrapperGuids = Updater2018_3.UpdateLinks.Keys;
            var links = Updater2018_3.UpdateLinks;
            foreach (var wrapperGuid in wrapperGuids)
            {
                var layerGuid = links[wrapperGuid];
                var splatPrototypeWrapperAsset = AssetDatabase.LoadAssetAtPath<SplatPrototypeWrapper>(AssetDatabase.GUIDToAssetPath(wrapperGuid));
                TerrainLayer terrainLayerAsset = null;
                if(!string.IsNullOrEmpty(layerGuid))
                {
                    if(layerGuid.Contains("Assets/"))
                    {
                        var tryGuid = AssetDatabase.AssetPathToGUID(layerGuid);
                        if(!string.IsNullOrEmpty(tryGuid))
                        {
                            links[wrapperGuid] = tryGuid;
                            break;
                        }
                    }
                    try
                    {
                        var path = AssetDatabase.GUIDToAssetPath(layerGuid);
                        terrainLayerAsset = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
                        if (terrainLayerAsset == null)
                        {
                            throw new Exception(string.Format("Failed to load asset with guid ({0}), path{1}", wrapperGuid, path));
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                        wrapperGuidsToClear.Add(wrapperGuid);
                        layerFailedToLoad = true;
                    }
                }                
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.ObjectField("", splatPrototypeWrapperAsset, typeof(SplatPrototypeWrapper), true);
                var newLink = EditorGUILayout.ObjectField("", terrainLayerAsset, typeof(TerrainLayer), false);
                if(newLink != terrainLayerAsset)
                {
                    string newGuid = null;
                    long localID;
                    if (newLink != null)
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newLink, out newGuid, out localID);
                    }
                    links[wrapperGuid] = newGuid;
                    GUIUtility.ExitGUI();
                    break;
                }
                if(GUILayout.Button("Create New Wrapper"))
                {
                    links[wrapperGuid] = Updater2018_3.CreateNewTerrainLayer(splatPrototypeWrapperAsset);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            foreach (var toClear in wrapperGuidsToClear)
            {
                links[toClear] = null;
            }
            Updater2018_3.UpdateLinks = links;
            var canContinue = !layerFailedToLoad && Updater2018_3.UpdateLinks.Values.All((x) => x != null);
            if(!canContinue)
            {
                EditorGUILayout.HelpBox("All SplatPrototypeWrappers must be resolved to continue.", MessageType.Info);
            }
            GUI.enabled = canContinue;
            if(GUILayout.Button("Upgrade my assets with these matches"))
            {
                WindowState = EWindowState.LOG;
                Updater2018_3.Upgrade();
            }
            GUI.enabled = true;
            if (!_hasRefreshedWrappers || GUILayout.Button("Rescan for SplatPrototypeWrappers in my project"))
            {
                _hasRefreshedWrappers = true;
                Updater2018_3.PopulateLinks();
            }
        }

        Vector2 _progressScroll;
        void DisplayProgessGUI()
        {
            EditorGUILayout.LabelField("Upgrade Log:", EditorStyles.boldLabel);
            _progressScroll = EditorGUILayout.BeginScrollView(_progressScroll);
            for(var i = 0; i < Updater2018_3.Log.Messages.Count; ++i)
            {
                var log = Updater2018_3.Log.Messages[i];
                if(!log.CanExpand())
                {
                    EditorGUILayout.LabelField("> " + log.Header);
                }
                else
                {
                    log.GUIExpanded = EditorGUILayout.Foldout(log.GUIExpanded, log.Header);
                    if (log.GUIExpanded)
                    {
                        EditorGUILayout.BeginVertical("box");
                        if(!string.IsNullOrEmpty(log.Message))
                        {
                            EditorGUILayout.LabelField(log.Message, EditorStyles.wordWrappedLabel);
                        }                        
                        if (log.Exception != null)
                        {
                            EditorGUILayout.HelpBox(log.Exception.Message + "\n" + log.Exception.StackTrace, MessageType.Error);
                        }
                        if(log.Context != null)
                        {
                            EditorGUILayout.ObjectField("Context", log.Context, typeof(UnityEngine.Object), true);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                Updater2018_3.Log.Messages[i] = log;
            }
            EditorGUILayout.EndScrollView();
            if(GUILayout.Button("Close"))
            {
                Close();
            }
        }
    }


}
#endif
