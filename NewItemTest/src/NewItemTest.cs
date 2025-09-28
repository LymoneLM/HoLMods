using BepInEx;
using BepInEx.Configuration;

namespace NewItemTest
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    public class NewItemTest
    {
        public const string MODNAME = "NewItemTest";
        public const string MODGUID = "cc.lymone.HoL." + MODNAME;
        public const string VERSION = "1.0.0";

        private static void Start()
        {

        }
    }
}
