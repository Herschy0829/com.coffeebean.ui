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
