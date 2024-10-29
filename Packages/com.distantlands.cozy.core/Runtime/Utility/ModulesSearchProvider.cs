#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace DistantLands.Cozy
{
    public class ModulesSearchProvider : ScriptableObject, ISearchWindowProvider
    {

        public List<Type> modules;
        public CozyWeather weather;



        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Select a Module"), 0),
                new SearchTreeGroupEntry(new GUIContent("Atmosphere"), 1),
                new SearchTreeGroupEntry(new GUIContent("Time"), 1),
                new SearchTreeGroupEntry(new GUIContent("Ecosystem"), 1),
                new SearchTreeGroupEntry(new GUIContent("Integration"), 1),
                new SearchTreeGroupEntry(new GUIContent("Utility"), 1),
                new SearchTreeGroupEntry(new GUIContent("Other"), 1)
            };
            for (int index = modules.Count - 1; index >= 0; index--)
            {
                string level = "";
                Type i = modules[index];
                SearchTreeEntry entry = GetSearchTreeEntry(i.Name, i, out level);
                entries.Insert(entries.FindIndex(x => x.content.text == level) + 1, entry);

            }
            return entries;
        }

        public SearchTreeEntry GetSearchTreeEntry(string name, Type type, out string category)
        {

            GUIContent content = new GUIContent();
            category = "Other";

            switch (name)
            {
                case "CozyAmbienceModule":
                    content = new GUIContent(" Ambience Module", (Texture)Resources.Load("Ambience Profile"));
                    category = "Ecosystem";
                    break;
                case "CozyEventModule":
                    content = new GUIContent(" Events Module", (Texture)Resources.Load("Events"));
                    category = "Utility";
                    break;
                case "CozyInteractionsModule":
                    content = new GUIContent(" Interactions Module", (Texture)Resources.Load("InteractionsModule"));
                    category = "Ecosystem";
                    break;
                case "CozyMicrosplatModule":
                    content = new GUIContent(" Microsplat Integration", (Texture)Resources.Load("Integration"));
                    category = "Integration";
                    break;
                case "CozyReflectionsModule":
                    content = new GUIContent(" Reflections Module", (Texture)Resources.Load("Reflections"));
                    category = "Atmosphere";
                    break;
                case "CozyReportsModule":
                    content = new GUIContent(" Reports Module", (Texture)Resources.Load("Reports"));
                    category = "Utility";
                    break;
                case "CozyDebugModule":
                    content = new GUIContent(" Debug Module", (Texture)Resources.Load("Debug"));
                    category = "Utility";
                    break;
                case "CozySatelliteModule":
                    content = new GUIContent(" Satellite Module", (Texture)Resources.Load("CozyMoon"));
                    category = "Atmosphere";
                    break;
                case "CozySaveLoadModule":
                    content = new GUIContent(" Save/Load Module", (Texture)Resources.Load("Save"));
                    category = "Utility";
                    break;
                case "CozyTVEModule":
                    content = new GUIContent(" The Vegetation Engine Integration", (Texture)Resources.Load("Boxophobic"));
                    category = "Integration";
                    break;
                case "CozyButoModule":
                    content = new GUIContent(" Buto Integration", (Texture)Resources.Load("Occa"));
                    category = "Integration";
                    break;
                case "VFXModule":
                    content = new GUIContent(" Visual FX Module", (Texture)Resources.Load("FX Module"));
                    category = "Atmosphere";
                    break;
                case "BlocksModule":
                    content = new GUIContent(" BLOCKS Module", (Texture)Resources.Load("Blocks"));
                    category = "Atmosphere";
                    break;
                case "PlumeModule":
                    content = new GUIContent(" PLUME Module", (Texture)Resources.Load("Cloud"));
                    category = "Atmosphere";
                    break;
                case "CataclysmModule":
                    content = new GUIContent(" CATACLYSM Module", (Texture)Resources.Load("Tornado"));
                    category = "Ecosystem";
                    break;
                case "LinkFishnetModule":
                    content = new GUIContent(" LINK Module", (Texture)Resources.Load("Link"));
                    category = "Integration";
                    break;
                case "LinkNetcodeModule":
                    content = new GUIContent(" LINK Module", (Texture)Resources.Load("Link"));
                    category = "Integration";
                    break;
                case "LinkPhotonModule":
                    content = new GUIContent(" LINK Module", (Texture)Resources.Load("Link"));
                    category = "Integration";
                    break;
                case "LinkMirrorModule":
                    content = new GUIContent(" LINK Module", (Texture)Resources.Load("Link"));
                    category = "Integration";
                    break;
                case "CultivateModule":
                    content = new GUIContent(" CULTIVATE Module", (Texture)Resources.Load("Ecosystem"));
                    category = "Ecosystem";
                    break;
                case "CozyHabits":
                    content = new GUIContent(" HABITS Module", (Texture)Resources.Load("Habits"));
                    category = "Time";
                    break;
                case "CozyTransitModule":
                    content = new GUIContent(" Transit Module", (Texture)Resources.Load("Transit"));
                    category = "Time";
                    break;
                case "CozyClimateModule":
                    content = new GUIContent(" Climate Module", (Texture)Resources.Load("Climate"));
                    category = "Ecosystem";
                    break;
                case "CozyWeatherModule":
                    content = new GUIContent(" Weather Module", (Texture)Resources.Load("Weather Profile-01"));
                    category = "Ecosystem";
                    break;
                case "CozyTimeModule":
                    content = new GUIContent(" Time Module", (Texture)Resources.Load("CozyCalendar"));
                    category = "Time";
                    break;
                case "SystemTimeModule":
                    content = new GUIContent(" System Time Module", (Texture)Resources.Load("CozySystemTime"));
                    category = "Time";
                    break;
                case "CozyAtmosphereModule":
                    content = new GUIContent(" Atmosphere Module", (Texture)Resources.Load("Atmosphere"));
                    category = "Atmosphere";
                    break;
                case "RadarModule":
                    content = new GUIContent(" Radar Module", (Texture)Resources.Load("CozyRadar"));
                    category = "Ecosystem";
                    break;
                case "EclipseModule":
                    content = new GUIContent(" Eclipse Module", (Texture)Resources.Load("CozyEclipse"));
                    category = "Atmosphere";
                    break;
                case "CozyControlPanelModule":
                    content = new GUIContent(" Control Panel Module", (Texture)Resources.Load("Control Panel"));
                    category = "Utility";
                    break;
                case "CozyWindModule":
                    content = new GUIContent(" Wind Module", (Texture)Resources.Load("Wind Module"));
                    category = "Ecosystem";
                    break;
                case "CozyMLSModule":
                    content = new GUIContent(" Magic Lightmap Switcher Integration", (Texture)Resources.Load("MLS Icon"));
                    category = "Integration";
                    break;
                case "ReSoundModule":
                    content = new GUIContent(" ReSOUND Module", (Texture)Resources.Load("ReSound Icon"));
                    category = "Utility";
                    break;
                default:
                    content = new GUIContent(name);
                    break;
            }

            SearchTreeEntry entry = new SearchTreeEntry(content);
            entry.userData = type;
            entry.level = 2;
            return entry;

        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            weather?.InitializeModule((Type)SearchTreeEntry.userData);
            return true;
        }
    }

    public class BiomeModulesSearchProvider : ScriptableObject, ISearchWindowProvider
    {

        public List<Type> modules;
        public CozyBiome biome;



        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Select a Module"), 0));
            foreach (Type i in modules)
            {

                entries.Add(GetSearchTreeEntry(i.Name, i));

            }
            return entries;
        }

        public SearchTreeEntry GetSearchTreeEntry(string name, Type type)
        {

            GUIContent content = new GUIContent();

            switch (name)
            {
                case "BlocksModule":
                    content = new GUIContent(" BLOCKS Extension", (Texture)Resources.Load("Blocks"));
                    break;
                case "CozyAmbienceModule":
                    content = new GUIContent(" Ambience Extension", (Texture)Resources.Load("Ambience Profile"));
                    break;
                case "CozyWeatherModule":
                    content = new GUIContent(" Weather Extension", (Texture)Resources.Load("Weather Profile-01"));
                    break;
                case "CozyClimateModule":
                    content = new GUIContent(" Climate Extension", (Texture)Resources.Load("Climate"));
                    break;
                case "CozyAtmosphereModule":
                    content = new GUIContent(" Atmosphere Extension", (Texture)Resources.Load("Atmosphere"));
                    break;
                case "CozyTimeModule":
                    content = new GUIContent(" Time Extension", (Texture)Resources.Load("CozyCalendar"));
                    break;
                default:
                    content = new GUIContent(name);
                    break;
            }

            SearchTreeEntry entry = new SearchTreeEntry(content);
            entry.level = 1;
            entry.userData = type;
            return entry;

        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            biome?.InitializeModule((Type)SearchTreeEntry.userData);
            return true;
        }
    }

}
#endif