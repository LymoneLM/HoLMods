# 数据工具 MainloadTool

开发工具，提供部分游戏数据的导出，存档自动加载和自动删档保护。Dev tool, providing export of partial game data, automatic save loading, and automatic save deletion protection.



可以实时导出最新的物品、建筑等游戏数据，支持的数据详见更新日志。

提供启动游戏自动加载某存档功能，便于模组开发调试，需要在配置中开启。 

提供删档保护，防止对存档进行修改时出现失误导致存档丢失，或者因为游戏错误导致的存档丢失。

Real-time export of the latest game data such as items, buildings, etc., is supported. For details on the data types available, please refer to the update log.

The feature of automatically loading a specific save when starting the game is provided, which is convenient for mod development and debugging. This feature needs to be enabled in the configuration.

Save deletion protection is offered to prevent accidental loss of saves due to mistakes made during modifications or game errors.



如果您有任何疑问或建议，或者发现BUG，欢迎添加QQ群交流：1058593281

If you have any questions, suggestions, or encounter bugs, feel free to join our [Discord Server](https://discord.gg/5ubSTurmBe) for discussion.

## 📦 安装 Installation

**推荐使用模组管理器自动安装** **Use Mod Manager**

- [GaleModManager](https://thunderstore.io/c/house-of-legacy/p/Kesomannen/GaleModManager/)
- [ThunderstoreModManager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager)

**手动安装** **Manual**

- 首先需要安装[BepInExPack](https://thunderstore.io/c/house-of-legacy/p/BepInEx/BepInExPack/) 和 [YuanAPI](https://thunderstore.io/c/house-of-legacy/p/LymoneLM/YuanAPI/)
- 使用本包中BepInEx文件夹覆盖游戏根目录BepInEx文件夹




- First, install [BepInExPack](https://thunderstore.io/c/house-of-legacy/p/BepInEx/BepInExPack/) and [YuanAPI](https://thunderstore.io/c/house-of-legacy/p/LymoneLM/YuanAPI/)
- Overwrite the BepInEx folder in the game's root directory with the one from this package

## 🔧 配置 Configuration

运行一次游戏后生成配置，位于：

```shell
BepInEx\config\cc.lymone.HoL.MainloadTool.cfg
```

修改完保存配置，需要重启游戏生效



The configuration file is generated after running the game once, located at:

```shell
BepInEx\config\cc.lymone.HoL.MainloadTool.cfg
```

Save the changes after modification and restart the game for them to take effect.