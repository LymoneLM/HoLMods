using System;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;

namespace BepInLoadTest
{
    public static class Test {
        public const string GUID = "cc.lymone.HoL.BepInLoadTest";
        public const string NAME = "BepInLoadTest";
    }

    [BepInPlugin(Test.GUID, Test.NAME, "1.0.0")]
    public class Class : BaseUnityPlugin {
        public new static ManualLogSource Logger;

        public static string ClassName;
        public void Awake() {
            Logger = base.Logger;
            Harmony.CreateAndPatchAll(typeof(MainloadPatch));

            ClassName = this.GetType().Name;
            Logger.LogInfo($"{ClassName} Awake");
        }

        public void Start() {
            Logger.LogInfo($"{ClassName} Start");
        }
    }

    [BepInDependency(Test.GUID)]
    [BepInPlugin(Test.GUID + "_1", Test.NAME,"1.0.0")]
    public class Class1 : BaseUnityPlugin {


        public static string ClassName;
        public void Awake() {
            ClassName = this.GetType().Name;
            Logger.LogInfo($"{ClassName} Awake");
        }

        public void Start() {
            Logger.LogInfo($"{ClassName} Start");
        }
    }

    [BepInDependency(Test.GUID + "_1")]
    [BepInPlugin(Test.GUID + "_11", Test.NAME, "1.0.0")]
    public class Class11 : BaseUnityPlugin {
        public static string ClassName;
        public void Awake() {
            ClassName = this.GetType().Name;
            Logger.LogInfo($"{ClassName} Awake");
        }

        public void Start() {
            Logger.LogInfo($"{ClassName} Start");
        }
    }

    [BepInDependency(Test.GUID)]
    [BepInPlugin(Test.GUID + "_2", Test.NAME, "1.0.0")]
    public class Class2 : BaseUnityPlugin {
        public static string ClassName;
        public void Awake() {
            ClassName = this.GetType().Name;
            Logger.LogInfo($"{ClassName} Awake");
        }

        public void Start() {
            Logger.LogInfo($"{ClassName} Start");
        }
    }

    [BepInDependency(Test.GUID + "_2")]
    [BepInPlugin(Test.GUID + "_22", Test.NAME, "1.0.0")]
    public class Class22 : BaseUnityPlugin {
        public static string ClassName;
        public void Awake() {
            ClassName = this.GetType().Name;
            Logger.LogInfo($"{ClassName} Awake");
        }

        public void Start() {
            Logger.LogInfo($"{ClassName} Start");
        }
    }

    public class MainloadPatch {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mainload), "Start")]
        public static void Prefix() {
            Class.Logger.LogInfo("Mainload Start Start");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mainload), "Start")]
        public static void Postfix() {
            Class.Logger.LogInfo("Mainload Start Done");
        }
    }
}

