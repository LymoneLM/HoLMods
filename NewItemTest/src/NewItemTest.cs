using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace YuanAPI
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    public class NewItemTest : BaseUnityPlugin
    {
        public const string MODNAME = "NewItemTest";
        public const string MODGUID = "cc.lymone.HoL." + MODNAME;
        public const string VERSION = "1.0.0";

        internal static Harmony harmony;
        internal static IYuanLogger logger;
        internal static ResourceData resource;

        private void Awake()
        {
            logger = new LoggerWrapper(Logger);
            YuanLogger.SetLogger(logger);
            harmony = new Harmony(MODGUID);
        }

        private void Start()
        {

        }

        private void Patch()
        {

        } 
    }
}
