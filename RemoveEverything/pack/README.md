# 一键拆除 RemoveEverything

一键清空宅邸建筑！快速安全无副作用！One-click to clear mansion buildings! Fast and safe with no side effects!



模组替换了宅邸管理界面原版的一键清空装饰性建筑，完全按照类似玩家手动处理的逻辑删除其余建筑，安全无副作用，甚至比原版清空的更快！

配置里提供了可选的快速模式关闭选项，如果您觉得删除的太快，或者不愿意忍受游戏过度卡顿，可以在配置文件内关闭。

The mod replaces the vanilla one-click clear decorative buildings feature in the mansion management interface. It follows a logic similar to manual player processing to remove other buildings, ensuring safety with no side effects—it's even faster than the vanilla clear function!

An optional fast mode toggle is provided in the configuration. If you feel the deletion is too rapid or prefer to avoid excessive game lag, you can disable it in the configuration file.



如果您有任何疑问或建议，或者发现BUG，欢迎添加QQ群交流：1058593281

If you have any questions, suggestions, or encounter bugs, feel free to join our [Discord Server](https://discord.gg/5ubSTurmBe) for discussion.

## ❓问题解答 FAQ

### 1.为什么拆除完我的府邸环境、安全、便捷值为负了？

每个成员（族人），都会降低一点这三种值。受限于游戏规则，任何成员必须有一个居住的府邸，本模组只是拆除了建筑让建筑增益将为0，并没有删除或驱逐成员。从而导致这三个值将是成员数的负值。

### 2.我的仓储/马厩上限怎么比我的剩余物品数量还小？

考虑到一键拆除的作用，模组默认会无视存储上限进行拆除，这对于存档来说是安全的。如果您不想让存储空间跌破储量，可以选择将配置中的`安全移除库存 Safe Remove Storage`选项设为`true`。

### 3.我的族人借阅的书籍没有归还，但是我的仓库里多了一本书。

知识是世家的根本。受限于阅读机制，模组必须就借出的书籍做出取舍。我不想让在读书的小人进度收到损失，也不像让家族受到损失，我的选择是将正在读的书送个小人，然后复制一本存入库存（可能有一点点的作弊嫌疑）。如果你不想这么做（或者不想作弊），同样可以在配置里关闭这个功能，就叫做`是否强行保留书籍 Force keep the books`，更多介绍可以看配置文件。

### 4.我能用在我的原版旧存档上吗？

可以，当然可以。但是需要注意在使用前备份存档！虽然我已经尽我所能将模组做的完全安全，但是您仍然是唯一为您的存档负责的人！

### 1. Why did my mansion's environment, safety, and convenience values turn negative after demolition?

Each member (clan member) reduces these three values by one point. Due to game mechanics, every member must have a residence in the mansion. This mod only removes the buildings, reducing the building bonuses to zero, but does not delete or expel members. As a result, these three values will be negative, equal to the number of members.

### 2. Why is my storage/stable capacity lower than my remaining item count?

Considering the purpose of one-click demolition, the mod defaults to ignoring storage limits during removal, which is safe for your save file. If you prefer not to let storage capacity drop below your current item count, you can set the `Safe Remove Storage` option in the configuration to `true`.

### 3. My clan members didn’t return borrowed books, but an extra book appeared in my storage.

Knowledge is the foundation of a noble family. Due to the limitations of the reading mechanics, the mod must make a choice regarding borrowed books. To avoid penalizing characters' reading progress or causing losses to the family, the mod grants the borrowed book to the character and duplicates another copy for storage (which may seem slightly like cheating). If you prefer not to do this (or wish to avoid cheating), you can disable this feature in the configuration under the option `Force keep the books`. For more details, refer to the configuration file.

### 4. Can I use this mod with my vanilla old save file?

Yes, absolutely. However, make sure to back up your save file before using it! Although I’ve done my best to ensure the mod is completely safe, you are ultimately responsible for your save file!

## 📦 安装 Installation

**推荐使用模组管理器自动安装** **Use Mod Manager**

- [GaleModManager](https://thunderstore.io/c/house-of-legacy/p/Kesomannen/GaleModManager/)
- [ThunderstoreModManager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager)

**手动安装** **Manual**

- 首先需要安装[BepInExPack](https://thunderstore.io/c/house-of-legacy/p/BepInEx/BepInExPack/)
- 使用本包中BepInEx文件夹覆盖游戏根目录BepInEx文件夹




- First, install [BepInExPack](https://thunderstore.io/c/house-of-legacy/p/BepInEx/BepInExPack/)
- Overwrite the BepInEx folder in the game's root directory with the one from this package

## 🔧 配置 Configuration

运行一次游戏后生成配置，位于：

```shell
BepInEx\config\cc.lymone.HoL.RemoveEverything.cfg
```

修改完保存配置，需要重启游戏生效



The configuration file is generated after running the game once, located at:

```shell
BepInEx\config\cc.lymone.HoL.RemoveEverything.cfg
```

Save the changes after modification and restart the game for them to take effect.