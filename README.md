# CoffeeBean UI（com.coffeebean.ui）

CoffeeBean 框架的 UI 模块：**UGUI 面板管理 + 代码生成**。

- **面板管理**：`CUIManager` / `CUIPanel`，6 层 UI（Bg/Common/PopUI/Guide/Toast/Top），Single/Multiple 打开类型，栈式导航（Push/Pop/Back），生命周期（Init/Open/Show/Hide/Close），面板传参（`ICUIData`）
- **可插拔加载器**：`ICUIPanelLoader` 接口 + 池（默认 Resources 实现，将来可接 asset 模块 Addressables）
- **代码生成**：`CBind` 标记组件 → Inspector/右键"生成面板代码" → **主脚本 + Designer partial 两文件分离**（用户代码不被覆盖）→ 编译后自动挂载字段引用
- **零额外依赖**：仅依赖 `com.coffeebean.tools`（单例/日志）

> 设计参考：QFramework UIKit + CodeGenKit（MIT）。设计文档：`docs/design-ui.md`

## 安装

```json
{
  "dependencies": {
    "com.coffeebean.ui": "https://github.com/Herschy0829/com.coffeebean.ui.git#v0.1.0",
    "com.coffeebean.tools": "https://github.com/Herschy0829/com.coffeebean.tools.git#v0.5.0"
  }
}
```

## 快速使用

```csharp
using CoffeeBean;

// 1. 面板类（业务继承 CUIPanel，实现生命周期钩子）
public class UIMainPanel : CUIPanel
{
    protected override void OnInit(ICUIData uiData = null) { }
    protected override void OnOpen(ICUIData uiData = null) { }
    protected override void OnShow() { }
    protected override void OnHide() { }
    protected override void OnClose() { }
}

// 2. 打开 / 关闭（Single 同类型复用）
CUIManager.Instance.OpenPanel<UIMainPanel>(CUILevel.Common, new UIMainPanelData());
CUIManager.Instance.ClosePanel<UIMainPanel>();

// 3. 栈式导航
var panel = CUIManager.Instance.OpenPanel<UIShopPanel>(CUILevel.PopUI);
CUIManager.Instance.Stack.Push(panel);   // 记录并关闭当前
CUIManager.Instance.Stack.Pop();          // 重开上一个
CUIManager.Instance.Back(panel);          // 关闭当前并返回上一个

// 4. 传参
public class UIMainPanelData : CUIPanelData { public string Title; }
```

## 代码生成工作流

1. 在 UI 预制体上给需要绑定的节点挂 **`CBind`** 组件（可填 Comment 注释、勾选 IsElement 生成独立元素）
2. 选中预制体，点 Inspector 的 **"生成面板代码"** 按钮（或右键菜单 `Assets/CoffeeBean UI/生成面板代码`）
3. 生成 `PanelName.cs`（主脚本，可编辑）+ `PanelName.Designer.cs`（partial，自动生成字段绑定，可反复覆盖）
4. 脚本编译完成后自动把 CBind 节点引用挂载到面板字段（`[DidReloadScripts]`），并移除 CBind 标记（运行时无开销）

```
UIMainPanel.cs            // 用户代码：生命周期空实现 + Data 类
UIMainPanel.Designer.cs   // 自动生成：public Button btnStart; 等字段 + ClearUIComponents
```

## 目录结构

```
Runtime/
├── CUIManager.cs         面板管理门面（打开/关闭/显示/隐藏/栈/Back）
├── CUIRoot.cs            UI 根（6 层节点，分辨率/渲染模式）
├── CUIPanel.cs           面板抽象基类（生命周期）
├── CUIPanelLoader.cs     加载器接口 + 池 + 默认 Resources 实现
├── CUIPanelTable.cs      双索引面板表
├── CUIPanelStack.cs      栈式导航
├── CBind.cs              代码生成绑定标记 + 类型推断
└── ...
Editor/
├── CUICodeGenerator.cs   代码生成器（菜单/Inspector 按钮）
├── CUISerializer.cs      编译后自动挂载
└── ...
```

## 测试

EditMode 测试：生命周期顺序 / Single 复用 / Multiple 新建 / 传参 / 层级挂载 / 关闭移除 / 栈 Push/Pop / 加载器池回收 / 缺失预制体容错 / Back。

## 版本约定

- SemVer + git tag `vX.Y.Z`；每个版本对应 GitHub Release（CHANGELOG 派生说明）
