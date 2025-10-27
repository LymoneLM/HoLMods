# NewtonsoftJson

提供[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)库的netstandard2.0版本，利用BepInEx加载依赖链确保库正常载入。Provides the netstandard2.0 version of the [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) library, ensuring proper loading through the BepInEx dependency chain.



这个模组是一个程序库，可能被其他模组依赖。通常情况下您不需要主动安装这个模组，除非某个模组的开发者要求你安装。绝大多数情况下模组管理器会根据依赖情况自动安装这个模组。如果您不确定是否有需求就请不要卸载这个模组。

This mod is a library that may be required by other mods. Normally, you don’t need to install it manually unless another mod developer instructs you to. In most cases, the mod manager will automatically install it based on dependency requirements. If you’re unsure whether you need it, please don’t uninstall it.



如果您有任何疑问或建议，或者发现BUG，欢迎添加QQ群交流：1058593281

If you have any questions, suggestions, or encounter bugs, feel free to join our [Discord Server](https://discord.gg/5ubSTurmBe) for discussion.

## 📦 安装 Installation

**推荐使用模组管理器自动安装** **Use Mod Manager**

- [GaleModManager](https://thunderstore.io/c/house-of-legacy/p/Kesomannen/GaleModManager/)
- [ThunderstoreModManager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager)

**手动安装** **Manual**

- 使用本包中BepInEx文件夹覆盖游戏根目录BepInEx文件夹


- Overwrite the BepInEx folder in the game's root directory with the one from this package

## 🧑‍💻 开发者 Developer

如果您的模组使用了Newtonsoft.Json，推荐您通过依赖这个模组来确保其加载。为了确保正确依赖，您需要在两个位置加入依赖信息：

If your mod uses Newtonsoft.Json, it’s recommended to depend on this mod to ensure it loads correctly. To properly declare this dependency, add dependency information in two places:

#### BepInEx插件类的依赖属性 | Dependency attribute in the BepInEx plugin class

```c#
[BepInDependency("cc.lymone.HoL.NewtonsoftJsonPlugin")]
```

#### manifest.json

```
"dependencies": [
    "LymoneLM-NewtonsoftJson-1.0.0"
]
```

具体依赖字串详见Thunderstore本模组页面的`Dependency string`

For the exact dependency string, please refer to the `Dependency string` section on this mod’s Thunderstore page.

> [!IMPORTANT]
>
> 务必不要将本模组以及Newtonsoft.Json.dll纳入您上传的模组文件！
>
> Never include this mod or Newtonsoft.Json.dll in the files you upload for your own mod!

## 🫡 致谢 Acknowledgements

- 感谢 [ValheimModding Team](https://thunderstore.io/c/valheim/p/ValheimModding/) ，这个插件的代码来自他们的[项目](https://github.com/Valheim-Modding/CommonPackages)

- Thanks to the [ValheimModding Team](https://thunderstore.io/c/valheim/p/ValheimModding/); this plugin’s code is derived from their [project](https://github.com/Valheim-Modding/CommonPackages).

- 感谢 JamesNK 和 Newtonsoft.Json 其他贡献者，提供了高性能的开源 Json 库

- Thanks to JamesNK and other contributors to Newtonsoft.Json for providing a high-performance open-source JSON library.