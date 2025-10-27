# NewtonsoftJson

提供[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)库的netstandard2.0版本。Shared the netstandard2.0 version of the [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) library.



这个模组是一个程序库，可能被其他模组依赖。通常情况下您不需要主动安装这个模组，除非某个模组的开发者要求你安装。绝大多数情况下模组管理器会根据依赖情况自动安装这个模组。如果您不确定是否有需求就请不要卸载这个模组。

This mod is a library that may be required by other mods. Normally, you don’t need to install it manually unless another mod developer instructs you to. In most cases, the mod manager will automatically install it based on dependency requirements. If you’re unsure whether you need it, please don’t uninstall it.



如果您有任何疑问或建议，欢迎添加QQ群交流：1058593281

If you have any questions, suggestions, feel free to join our [Discord Server](https://discord.gg/5ubSTurmBe) for discussion.

## 📦 安装 Installation

**推荐使用模组管理器自动安装** **Use Mod Manager**

- [GaleModManager](https://thunderstore.io/c/house-of-legacy/p/Kesomannen/GaleModManager/)
- [ThunderstoreModManager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager)

**手动安装** **Manual**

- 使用本包中BepInEx文件夹覆盖游戏根目录BepInEx文件夹


- Overwrite the BepInEx folder in the game's root directory with the one from this package

## 🧑‍💻 开发者 Developer

如果您的模组使用了Newtonsoft.Json，推荐您通过依赖这个模组来确保其加载。

If your mod uses Newtonsoft.Json, it’s recommended to depend on this mod to ensure it loads correctly.

#### manifest.json

```
"dependencies": [
    "LymoneLM-NewtonsoftJson-13.0.4"
]
```

具体依赖字串详见Thunderstore本模组页面的`Dependency string`

For the exact dependency string, please refer to the `Dependency string` section on this mod’s Thunderstore page.

> [!IMPORTANT]
>
> 务必不要将本模组或Newtonsoft.Json.dll纳入您上传的模组文件！
>
> Never include this mod or Newtonsoft.Json.dll in the files you upload for your own mod!

## 🫡 致谢 Acknowledgements

- 感谢 JamesNK 和 Newtonsoft.Json 其他贡献者，提供了高性能的开源 Json 库

- Thanks to JamesNK and other contributors to Newtonsoft.Json for providing a high-performance open-source JSON library.