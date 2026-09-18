# FixLilith

### 简介
修复桌宠软件《不/存在的莉莉丝》在 linux (proton-ge11 + wayland) 下的闪烁问题
目前仅在 GNOME 50 wayland 和 sway 下测试过

### 已知问题
1. 鼠标追踪不生效
2. 控制台会持续报错"Client area does not match virtual screen yet. ..."
3. 无法坐到其他窗口上

### 使用教程
#### 前置条件
- Proton-GE 11 x64 (理论上任意子版本均可，本项目测试时使用Proton-GE 11.7 和 11.1）
- 支持 Unity-IL2CPP 的 BepInEx windows x64

#### 一、 安装Proton-GE 11
1. 下载 Proton-GE 11。你可以在官方仓库的 [release](https://github.com/GloriousEggroll/proton-ge-custom/releases) 页下载最新的 x86_64 构建
2. 在 steam 根目录下找到 compatibilitytools.d 文件夹（若没有则新建）。若你不知道 Steam 根目录在哪里，可以在库中任意选择一款游戏->查看本地文件，此时会打开 `.../Steam/steamapps/common/xxx`，路径中的 Steam 就是 steam 根目录
3. 将下载的 proton-ge 11 压缩包解压到 compatibilitytools.d
4. 重启 steam，之后在本游戏的兼容性选项中选择刚才安装的 proton-ge 版本

更详细的 Proton-GE 安装教程可以参考 [官方仓库](https://github.com/GloriousEggroll/proton-ge-custom) 的 README

#### 二、 在游戏根目录下安装支持 Unity-IL2CPP 的 BepInEx
1. 下载 BepInEx Unity-IL2CPP-win-x64 的最新构建。目前只有 BepInEx 6.0 bleeding edge 可以使用，你可以在 [这里](https://builds.bepinex.dev/projects/bepinex_be) 找到
2. 将压缩包中的内容解压到游戏根目录（如果你不知道在哪，在 steam 库页面中，选中本游戏（不/存在的莉莉丝），选择管理->浏览本地文件，即可轻松找到）。注意这一步完成后， winhttp.dll（BepInEx 的注入 dll）应该与 Lilith.exe（游戏本体）处于同一级目录中
3. 在游戏启动项中输入
  ```
  WINEDLLOVERRIDES="winhttp=n,b" %command%
  ```
4. 启动一次游戏，此时你应该能看到一个命令提示符窗口先行启动，并卡住一段时间，这是 BepInEx 在初次启动时下载UnityBaseLibrary，等待至游戏本体启动即可。若用时过长或持续失败，请尝试修改网络配置，或自行下载对应版本的 UnityBaseLibrary，之后将压缩包放在 `BepInEx/unity-libs` 目录下。你可以在 `BepInEx/config/BepInEx.cfg` 中搜索 `UnityBaseLibrariesSource` 关键字来查看目标 url

更详细的 BepInEx Unity-IL2CPP 安装教程，可以参考 [官方文档](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html)

#### 三、安装本补丁
1. 在 [发布页面](https://gitee.com/slakr/fix_lilith/releases) 下载最新的 dll 文件
2. 在游戏根目录中找到 `BepInEx/plugins` ，创建 `FixLilith` 文件夹（你可以自定义文件夹的名称，这只是为了方便区分不同的补丁文件）
3. 将 dll 文件复制到 `FixLilith` 文件夹中

#### 四、其他必要启动项
你需要在游戏启动项中添加 `WINE_LAYERED_OVERLAY_ALPHA=1` ，以便桌宠可以正常渲染与交互。目前这是 GE 版本的 Proton 独有的，也因此你需要Proton-GE。最终的启动项是：
  ```
  WINEDLLOVERRIDES="winhttp=n,b" WINE_LAYERED_OVERLAY_ALPHA=1 %command%
  ```
值得一提的是，`%command%` 是 steam 生成的用于启动游戏本体的命令，因此在 linux 版本中添加启动项需要注意其他参数与 %command% 的相对位置

### 构建教程
- 需要 dotnet 10。可参考 [官方文档](https://learn.microsoft.com/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website)
- 需要 支持 Unity-IL2CPP 的 BepInEx
1. 克隆本仓库到本地
   ```bash
   git clone https://gitee.com/slakr/fix_lilith.git
   cd fix_lilith
   ```
2. 修改 fixlilith.csproj
   1. 找到 \<BepInEx>/home/slakr/.local/share/Steam/steamapps/common/The NOexistenceN of Lilith/BepInEx\</BepInEx> 行（第 11 行）
   2. 将其修改为你的实际 BepInEx 安装路径
3. 构建
   ```bash
   dotnet build
   ```

构建的输出路径定义于 fixlilith.csproj 中的 \<OutputPath> 项（第 13 行），默认输出到 游戏根目录下的 `BepInEx/plugins/FixLilith`
