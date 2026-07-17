# VR_iceball / VR_ball

基于 Unity、PICO、OpenXR 和 Photon Fusion 2 开发的 VR 冰球训练与真人 1v1 联机原型。项目支持单机训练、AI 守门员训练、限时挑战，以及两台 PICO 头显进入同一 Photon 房间进行实时冰球对战。

> Unity 项目名为 `VR_ball`，当前 GitHub 仓库名为 `VR_iceball`。

## 项目状态

- 已完成单机训练闭环：主菜单、模式选择、限时挑战、AI 训练、训练结束和返回主菜单。
- 已完成 Photon Fusion 2 真人 1v1 联机基础：两台 PICO 可进入同一房间并看到对方。
- 已完成网络玩家同步：头显、左手、右手和球杆实时同步。
- 已完成网络冰球同步：生成、击打、飞行、进球检测、比分、倒计时和重置。
- 已完成 VR UI 基础：World Space Canvas、头显跟随、手柄射线点击、比赛结束 UI。
- 已完成 PICO 真机双设备安装与联机调试流程。

## 核心玩法

### 单机训练

玩家使用 VR 手柄控制冰球杆击打冰球，完成射门训练、限时挑战，或与不同难度的 AI 守门员进行训练。

### 真人 1v1 联机

两名玩家分别使用 PICO 头显进入同一 Photon 房间，双方可以看到对方的头、左右手、球杆和同一个网络冰球，并通过双方球门进行得分。

### 比赛 UI

项目包含房间进入界面、比赛 HUD、比分、倒计时、调试信息、比赛结束面板、重新比赛和返回主菜单按钮。

## 技术栈

| 模块 | 方案 |
| --- | --- |
| 游戏引擎 | Unity 2022.3.62 系列 |
| VR 设备 | PICO 头显 |
| XR 输入 | OpenXR、PICO OpenXR Integration SDK、XR Interaction Toolkit |
| 联机方案 | Photon Fusion 2、Shared Mode、Photon Cloud |
| UI 系统 | Unity Legacy UI、World Space Canvas |
| 输入系统 | Unity Input System、XR UI Input Module |
| Android 测试 | adb 安装 APK，PICO 双机联机测试 |
| 物理系统 | Rigidbody、Collider、Trigger、ForceMode.Impulse |

## 主要场景

| 场景 | 用途 | 状态 |
| --- | --- | --- |
| `Scene_MainMenu` | 主菜单、模式选择、难度选择、真人 1v1 入口 | 已完成 |
| `Scene_TimeChallenge` | 单机限时挑战训练 | 已完成 |
| `Scene_AITraining_Easy` | 简单 AI 守门员训练 | 已完成 |
| `Scene_AITraining_Normal` | 普通 AI 守门员训练 | 已完成 |
| `Scene_AITraining_Hard` | 困难 AI 守门员训练 | 已完成 |
| `Scene_OnlineRoomTest` | Photon Fusion 真人 1v1 联机测试与当前主开发场景 | 已完成基础功能 |

> `Scene_OnlineRoomTest 1` 为项目中的旧测试/备份场景，主要开发与测试请优先使用 `Scene_OnlineRoomTest`。

## 项目结构

```text
Assets/_Project
├─ Audio
├─ Matereials
├─ Models
├─ Player
├─ prefabs
├─ Scenes
├─ Scripts
│  ├─ Core
│  ├─ Editor
│  ├─ Hockey
│  ├─ Managers
│  ├─ Online
│  ├─ Player
│  └─ UI
└─ UI
```

关键目录：

- `Assets/_Project/Scenes`：项目主场景和训练场景。
- `Assets/_Project/Scripts/Core`：游戏状态与核心流程。
- `Assets/_Project/Scripts/Hockey`：冰球、球门、击打反馈和 AI 守门员逻辑。
- `Assets/_Project/Scripts/Online`：Photon Fusion 联机、玩家同步、网络冰球和在线比赛管理。
- `Assets/_Project/Scripts/UI`：主菜单、比赛 UI、HUD、头显跟随 UI。
- `Assets/_Project/prefabs`：球门、球杆、冰球、管理器和在线对象预制体。
- `Assets/_Project/Models`：冰场、球杆、冰球、玩家等模型资源。

## 联机系统概览

### Photon 房间

- 联机模式使用 Photon Fusion 2 `GameMode.Shared`。
- 默认房间名为 `Room_001`。
- Android 头显测试中支持自动进入默认房间。
- Photon Fixed Region 建议固定为 `asia`，避免两台 PICO 进入不同区域。
- 两台设备必须使用相同的 AppId、AppVersion 和 APK 版本。

### 网络对象

- `FusionRoomStarter`：创建 `NetworkRunner`、连接房间、触发玩家/冰球/比赛管理器生成。
- `OnlinePlayerSpawner`：生成网络玩家。
- `OnlinePuckSpawner`：生成房间内唯一网络冰球。
- `OnlineMatchSpawner`：生成在线比赛管理器。
- `OnlineMatchManager`：同步比分、倒计时、进球、重置和比赛结束状态。
- `NetworkPlayerAvatar`：同步本地 XR 的头显、左手、右手到远端客户端。
- `NetworkPuckPhysics`：同步冰球物理状态、击打和重置。

### NetworkPlayer 预制体

```text
NetworkPlayer
├─ HeadVisual
├─ LeftHandVisual
├─ RightHandVisual
└─ StickVisual
   ├─ HockeyStickModel
   ├─ RightGripAnchor
   ├─ LeftGripAnchor
   └─ BladeHitbox
```

注意：`NetworkPlayer` 根物体上的 `NetworkObject`、`NetworkPlayerAvatar` 以及 `HeadVisual`、`LeftHandVisual`、`RightHandVisual` 等同步目标不要随意删除或改名。替换正式模型时，应将模型作为这些节点的子物体进行调整。

## VR UI 配置要点

- `OnlineTestCanvas` 和 `OnlineMatchHUDCanvas` 使用 `World Space`。
- Canvas 需要绑定 `Main Camera` 作为 Event Camera。
- Canvas 需要添加 `Tracked Device Graphic Raycaster`。
- `EventSystem` 使用 `XR UI Input Module`。
- Button / Image 的 `Raycast Target` 需要开启。
- 手柄的 `Near-Far Interactor` 或 `XR Ray Interactor` 需要开启 UI Interaction。
- 头显内射线距离建议设置到约 `10`，并开启 Line Visual 方便点击。

## 运行项目

1. 克隆仓库并拉取 Git LFS 资源：

   ```bash
   git lfs install
   git clone https://github.com/yuhuasheng5/VR_iceball.git
   cd VR_iceball
   git lfs pull
   ```

2. 使用 Unity 2022.3.62 系列打开项目。
3. 确认 `Packages/manifest.json` 中 XR、OpenXR、PICO 和 Photon Fusion 相关依赖正常恢复。
4. 在 Photon Fusion 配置中确认 AppId 可用。
5. 在 Build Settings 中至少加入：

   - `Scene_MainMenu`
   - `Scene_OnlineRoomTest`

6. Editor 内可先进入 `Scene_OnlineRoomTest` 测试房间连接和对象生成。
7. 真机测试时构建 Android APK 并安装到两台 PICO。

## PICO 真机安装与双机测试

如果系统命令行无法直接识别 `adb`，可进入 Unity 自带 adb 目录后执行：

```bat
cd /d "D:\unity\unity\2022.3.62f3c1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools"
adb devices
adb install -r "D:\unity work\ICE\1.apk"
```

两台 PICO 同时连接时，建议指定设备 ID：

```bat
adb -s <DEVICE_ID_1> install -r "D:\unity work\ICE\1.apk"
adb -s <DEVICE_ID_2> install -r "D:\unity work\ICE\1.apk"
```

如果出现 `INSTALL_FAILED_UPDATE_INCOMPATIBLE`，说明头显中已有同包名但签名不同的旧版本，需要先卸载旧包：

```bat
adb uninstall com.UnityTechnologies.com.unity.template.urpblank
adb install -r "D:\unity work\ICE\1.apk"
```

双头显联机测试流程：

1. 两台 PICO 安装同一个最新 APK。
2. 两台设备连接同一个稳定网络，首次建议使用手机热点。
3. 打开游戏，从主菜单进入真人 1v1，或在 Android 上自动进入 `Room_001`。
4. 观察 `OnlineDebugText`：两台设备应显示同一房间，`Photon人数 = 2`，`NetworkPlayer数量 = 2`。
5. 测试头手移动同步、球杆同步、冰球击打、进球得分、倒计时和比赛结束 UI。

## 常见问题

### 两台 PICO 都显示 Player:1，Photon 人数一直是 1

通常表示两台设备没有进入同一个 Photon 会话。请检查：

- Fixed Region 是否统一为 `asia`。
- Photon AppId 是否一致。
- AppVersion 是否一致。
- 两台设备安装的 APK 是否为同一版本。
- 房间名是否都为 `Room_001`。

### 本地玩家无法同步，提示没有 LocalXRReferences

`LocalXRReferences` 应挂在场景中的 `XR Origin (VR)` 上，并绑定：

- `Head`：`Camera Offset > Main Camera`
- `Left Hand`：`Camera Offset > LeftHandController`
- `Right Hand`：`Camera Offset > RightHandController`

复制或替换 XR Origin 后需要重新检查该脚本和绑定。

### UI 在头显中不可见或无法点击

请检查：

- Canvas 是否为 `World Space`。
- Canvas 是否添加 `Tracked Device Graphic Raycaster`。
- EventSystem 是否使用 `XR UI Input Module`。
- 按钮和 Image 是否开启 `Raycast Target`。
- XR Ray / Near-Far Interactor 是否开启 UI Interaction。
- Raycast Mask 是否包含 UI Layer。

### 安装 APK 失败

如果是 `INSTALL_FAILED_UPDATE_INCOMPATIBLE`，先卸载旧包再安装。后续建议统一正式包名，例如 `com.yhs.vrball`，避免签名和包名冲突。

## 替换模型指南

核心原则：不要替换或删除带网络脚本的根物体，只替换其下方的显示模型。

### 替换玩家头盔和手套

1. 将模型导入 `Assets/_Project/Models/PlayerAvatar`。
2. 打开 `Assets/_Project/Prefabs/Online/NetworkPlayer`。
3. 保留 `NetworkPlayer` 根物体和 `NetworkObject`、`NetworkPlayerAvatar`、`StickVisual`。
4. 将头盔模型作为 `HeadVisual` 的子物体。
5. 将左右手套模型分别作为 `LeftHandVisual`、`RightHandVisual` 的子物体。
6. 只调整子模型的 Local Position / Rotation / Scale，不直接移动同步目标本体。
7. 运行双端测试，确认本地和远端都能看到同步。

### 替换球杆模型

1. 打开 `NetworkPlayer` 预制体。
2. 展开 `StickVisual`。
3. 保留 `RightGripAnchor`、`LeftGripAnchor` 和 `BladeHitbox`。
4. 将新球杆模型拖到 `StickVisual` 下，替换或隐藏旧的 `HockeyStickModel`。
5. 只缩放和旋转模型本体，不缩放 `StickVisual`。
6. 调整握点和 `BladeHitbox`，确保杆头击球区域覆盖实际模型。
7. 测试击球手感并微调。

### 替换冰球模型

1. 打开 `NetworkPuck` 预制体。
2. 保留 `NetworkObject`、`Rigidbody`、`Collider`、`NetworkPuckPhysics` 等逻辑组件。
3. 将新冰球模型作为 `NetworkPuck` 的子物体，例如 `PuckVisual`。
4. 调整 `PuckVisual` 使其与根物体 Collider 对齐。
5. 如果模型自带 Collider，建议删除或禁用，避免物理冲突。
6. 测试生成、击打、同步、进球和重置。

### 替换 UI 图片

1. 将 PNG UI 素材放入 `Assets/_Project/Textures/UI`。
2. 在 Inspector 中设置 `Texture Type = Sprite (2D and UI)`，`Sprite Mode = Single`。
3. 将 RoomPanel、MatchEndPanel 或按钮对象的 Image Source Image 替换为对应图片。
4. 按钮文字保留 Unity Text / TextMeshPro，方便动态更新。
5. 如需拉伸，优先使用 9-Slice：设置 Sprite Border 后将 Image Type 改为 `Sliced`。
6. 在 PICO 中测试尺寸、清晰度和可读性。

## 后续优化方向

- 替换正式玩家形象：头盔、手套、身体模型。
- 增加身体跟随头显方向，并进一步实现手臂 IK。
- 将调试用 `OnlineDebugText` 隐藏或改成开发者开关。
- 替换清爽冰雪运动主题 UI 图片，并统一字体和字号。
- 增加击球、进球、倒计时、按钮点击、胜负提示等音效。
- 优化网络冰球手感：击球力度、摩擦、速度上限和延迟平滑。
- 完善正式比赛规则：发球点、重开、胜利条件、暂停和退出房间。
- 统一正式包名和版本号，避免 APK 签名冲突。

## 资源与版本管理

本仓库使用 Git LFS 管理 `.fbx` 模型资源。首次克隆或换机开发时请确认已安装 Git LFS：

```bash
git lfs install
git lfs pull
```

Unity 自动生成目录如 `Library/`、`Temp/`、`Logs/`、`UserSettings/` 不应提交到仓库。

