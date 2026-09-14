# Changelog

## [0.2.4] - 2026-09-14

### Fixed
- **显示动画（`ShowAnim`）从未生效**：`CUIPanel.Show()` 只做 `SetActive(true)` + `OnShow()`，
  **从不调用 `CUIPanelTransition.PlayShow`**；而 `Hide()` 却会调用 `PlayHide` —— 两边不对称，
  于是 `ShowAnim` 配置了也没用（CHANGELOG 0.1.x 曾声称"Show/Hide 时自动播放动画"）。
  现在 `Show()` 在配置了 `ShowAnim` 时播放动画，播完再进入 `OnShow`（与 `Hide` 的语义对称）。
- **`Hide()` 在 EditMode 下可能卡住**：原条件为 `transition != null && HideAnim != None && activeInHierarchy`，
  注释却写"EditMode 下直接隐藏"。EditMode 下协程不推进，若面板确实配置了 `HideAnim`，
  `PlayHide` 的回调永不执行 → 面板卡在"未隐藏"状态。`Show()`/`Hide()` 现统一加 `Application.isPlaying` 前置判断，
  让注释描述的行为真正成立（EditMode 走同步路径）。

### Fixed（契约）
- **模块标记缺少 `com.coffeebean.asset` 依赖**：`Runtime/Bridge/Bridge.cs` 的 `Dependencies` 只有
  `core` + `tools`，但 `package.json` 与 Runtime asmdef **实际依赖 `com.coffeebean.asset`**
  （`CAssetPanelLoader` 直接使用 `CAssetSystem`）。Core 的拓扑排序依据正是这个数组，
  因此 asset 不会被排在 ui 之前。当前 `UIModule.OnLoad` 只打日志、未触碰 `CAssetSystem`，
  所以**尚无实际运行时影响**，但依赖图是失实的，一旦 OnLoad 里开始用 asset 就会踩到顺序问题。
- README 安装示例的 tag 由严重过期的 `v0.1.0` 修正为 `v0.2.4`。

### Tests
- EditMode：**30/30 全绿**（`Application.isPlaying` 为 false，EditMode 走同步路径，生命周期顺序断言不受影响）。

## [0.2.3] - 2026-08-28


### Added
- **CUIPanelStats 面板统计**：累计打开/关闭次数、当前打开面板数、按类型分布；CUIManager.Stats 自动记录（打开/关闭/CloseAll 重置）

# Changelog

## [0.2.2] - 2026-08-28


### Added
- **CUIMaskService 面板遮罩**：打开 PopUI 及以上层级自动生成全屏半透明遮罩（拦截点击防穿透），支持点击遮罩关闭，同层多面板共享遮罩，面板关闭自动移除；CUIManager 打开/关闭路径自动接入

# Changelog

## [0.2.1] - 2026-08-28


### Added
- **CUIPanelTransition 面板转场动画**：Fade / SlideLeft / SlideUp / Scale，Show/Hide 自动播放（纯协程，无外部依赖）

# Changelog

## [0.2.0] - 2026-08-28

### Added
- **`CAssetPanelLoader`（Addressables 面板加载）**：基于 asset 模块的 `CAssetSystem` 实现 `ICUIPanelLoader`，
  面板预制体通过 Addressables 加载；配套 `CAssetPanelLoaderPool`。
  用法：`CUIManager.Instance.PanelLoaderPool = new CAssetPanelLoaderPool();` 一行切换
  （默认仍是 Resources 加载，二选一）
- 依赖新增 `com.coffeebean.asset`（0.1.0）
- 测试 4 个：同步加载 / 异步加载 / Unload 释放引用计数 / 池分配

## [0.1.1] - 2026-08-28

### Changed
- 清理代码注释与文档中的设计参考说明，保留纯功能描述

# Changelog

## [0.1.0] - 2026-08-28

### Added
- **`CUIManager` 面板管理门面**：打开/关闭/显示/隐藏/查询，Single（复用）/ Multiple（新建）打开类型，
  6 层 UI（Bg/Common/PopUI/Guide/Toast/Top），栈式导航（`Stack.Push/Pop` + `Back`）
- **`CUIRoot`**：UI 根单例（自动创建），6 层节点，分辨率设置（CanvasScaler），Overlay/Camera 渲染模式切换
- **`CUIPanel` 面板基类**：生命周期（Init/Open/Show/Hide/Close）+ 业务钩子（OnInit/OnOpen/OnShow/OnHide/OnClose/OnBeforeDestroy）、
  面板传参（`ICUIData`/`CUIPanelData`）、`CloseSelf`/`Back`/`OnClosed` 回调
- **可插拔加载器 `ICUIPanelLoader`**：接口 + 池（`CAbstractPanelLoaderPool`），默认 `CResourcesPanelLoader`（Resources），
  将来可接 asset 模块 Addressables
- **代码生成**：
  - `CBind` 标记组件（Comment/IsElement/CustomComponentName）+ 组件类型自动推断（TMP > UGUI > Collider > ...）
  - `CUICodeGenerator`：Inspector"生成面板代码"按钮 + 右键菜单，生成 **主脚本 + Designer partial 两文件分离**（用户代码不被覆盖）
  - `CUISerializer`：编译后（`[DidReloadScripts]`）自动把 CBind 节点引用挂载到面板序列化字段，并移除标记
- **UIDemo 示例**：打开/关闭、层级、栈式导航、传参
- EditMode 测试：生命周期顺序 / Single 复用 / Multiple 新建 / 传参 / 层级挂载 / 关闭移除 / 栈 Push/Pop / 加载器池回收 / 缺失预制体容错 / Back

### Notes
- 依赖 `com.coffeebean.tools`（单例/日志）；核心设计：两文件分离、可插拔加载器、编译后自动挂载
- Core 可选集成：Bridge 条件编译（模块标记 + 生命周期）
