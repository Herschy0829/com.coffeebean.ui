using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CoffeeBean.UI.Tests
{
    /// <summary>CUIManager 面板管理测试：生命周期/复用/层级/栈/传参/加载器池。</summary>
    public class CUIManagerTests
    {
        private CUIManager _manager;
        private TestPanelLoaderPool _pool;

        [SetUp]
        public void SetUp()
        {
            // 清空旧单例与注册表
            CUIManager.ResetInstanceForTest();
            CUIRoot.ResetInstanceForTest();
            TestPanelLoader.ClearPrefabs();

            _manager = CUIManager.Instance;
            _pool = new TestPanelLoaderPool();
            _manager.PanelLoaderPool = _pool;

            // 注册测试面板 prefab
            TestPanelLoader.RegisterPrefab("TestPanel", UITestUtil.CreatePanelPrefab<TestPanel>());
            TestPanelLoader.RegisterPrefab("TestPanelB", UITestUtil.CreatePanelPrefab<TestPanelB>());
        }

        [TearDown]
        public void TearDown()
        {
            _manager.CloseAllPanel();
            CUIManager.ResetInstanceForTest();
            CUIRoot.ResetInstanceForTest();
        }

        [Test]
        public void OpenPanel_LifecycleOrder()
        {
            var panel = _manager.OpenPanel<TestPanel>();

            Assert.IsNotNull(panel);
            CollectionAssert.AreEqual(
                new[] { "OnInit", "OnOpen", "OnShow" },
                panel.Lifecycle,
                "生命周期顺序应为 OnInit -> OnOpen -> OnShow");
        }

        [Test]
        public void OpenPanel_SingleReuse_NoNewInstance()
        {
            var first = _manager.OpenPanel<TestPanel>();
            Assert.AreEqual(1, _pool.CreatedCount);

            var second = _manager.OpenPanel<TestPanel>();
            Assert.AreSame(first, second, "Single 类型重复打开应复用实例");
            Assert.AreEqual(1, _pool.CreatedCount, "复用不应新建加载器");
            Assert.IsTrue(first.gameObject.activeSelf, "复用后应重新 Show");
        }

        [Test]
        public void OpenPanel_Multiple_CreatesNew()
        {
            var keys = new CUIPanelSearchKeys
            {
                PanelType = typeof(TestPanelB),
                OpenType = CUIPanelOpenType.Multiple,
            };

            var a = _manager.OpenPanel(keys);
            var b = _manager.OpenPanel(keys);

            Assert.AreNotSame(a, b, "Multiple 类型每次打开应新建实例");
            Assert.AreEqual(2, _pool.CreatedCount);
        }

        [Test]
        public void OpenPanel_PassData()
        {
            var data = new TestPanelData { Message = "hello" };
            var panel = _manager.OpenPanel<TestPanel>(uiData: data);

            Assert.AreSame(data, panel.LastInitData, "OnInit 应收到传入数据");
            Assert.AreSame(data, panel.LastOpenData, "OnOpen 应收到传入数据");
        }

        [Test]
        public void PanelLevel_HangsOnCorrectLayer()
        {
            var panel = _manager.OpenPanel<TestPanel>(level: CUILevel.PopUI);

            Assert.AreEqual(CUILevel.PopUI, panel.Info.Level);
            Assert.AreEqual(CUIRoot.Instance.GetLayer(CUILevel.PopUI), panel.Transform.parent,
                "面板应挂到对应层级节点下");
        }

        [Test]
        public void ClosePanel_DestroysAndRemovesFromTable()
        {
            var panel = _manager.OpenPanel<TestPanel>();
            Assert.AreEqual(1, _manager.Table.Count());

            var lifecycle = panel.Lifecycle; // 先取引用，Close 后对象销毁不能再访问
            _manager.ClosePanel(panel);

            Assert.AreEqual(0, _manager.Table.Count(), "关闭后应从表中移除");
            Assert.IsTrue(lifecycle.Contains("OnClose"), "应触发 OnClose");
        }

        [Test]
        public void Stack_PushPop_ReopensPrevious()
        {
            var first = _manager.OpenPanel<TestPanel>(level: CUILevel.Common);
            var second = _manager.OpenPanel<TestPanelB>(level: CUILevel.PopUI);

            var secondLifecycle = second.Lifecycle; // 先取引用
            _manager.Stack.Push(second);
            Assert.IsTrue(secondLifecycle.Contains("OnClose"), "Push 后当前面板应关闭");
            Assert.IsNull(_manager.GetPanel<TestPanelB>(), "Push 后应从表中移除");

            _manager.Stack.Pop();
            var reopened = _manager.GetPanel<TestPanelB>();
            Assert.IsNotNull(reopened, "Pop 应按记录重开面板");
            Assert.AreNotSame(second, reopened, "旧实例已销毁，Pop 应创建新实例");
            Assert.AreEqual(1, reopened.OpenCount, "新实例 OnOpen 应触发一次");
            Assert.AreEqual(CUILevel.PopUI, reopened.Info.Level, "重开应恢复原层级");
        }

        [Test]
        public void CloseAllPanel_ClosesEverything()
        {
            _manager.OpenPanel<TestPanel>();
            _manager.OpenPanel<TestPanelB>();

            _manager.CloseAllPanel();

            Assert.AreEqual(0, _manager.Table.Count());
        }

        [Test]
        public void GetPanel_ByType()
        {
            _manager.OpenPanel<TestPanel>();

            var panel = _manager.GetPanel<TestPanel>();
            Assert.IsNotNull(panel);
            Assert.AreEqual(typeof(TestPanel), panel.GetType());
        }

        [Test]
        public void LoaderPool_Recycles()
        {
            var panel = _manager.OpenPanel<TestPanel>();
            Assert.AreEqual(1, _pool.CreatedCount, "首次打开应分配一个 loader");

            _manager.ClosePanel(panel);
            Assert.AreEqual(1, _pool.RecycleCount, "关闭后 loader 应回收到池");
        }

        [Test]
        public void MissingPrefab_ReturnsNull_NoThrow()
        {
            TestPanelLoader.ClearPrefabs(); // 无注册资源
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("面板加载失败"));

            var panel = _manager.OpenPanel<TestPanel>();
            Assert.IsNull(panel, "预制体缺失应返回 null 不抛异常");
        }

        [Test]
        public void Back_ClosesCurrent_AndReopensPrevious()
        {
            var first = _manager.OpenPanel<TestPanel>();
            _manager.Stack.Push(first); // 第一个面板入栈并关闭

            var second = _manager.OpenPanel<TestPanelB>();
            _manager.Back(second); // 关闭当前 B + Pop 重开 A

            Assert.IsNotNull(_manager.GetPanel<TestPanel>(), "Back 后应重开第一个面板");
            Assert.IsNull(_manager.GetPanel<TestPanelB>(), "Back 后当前面板应关闭");
        }
    }
}
