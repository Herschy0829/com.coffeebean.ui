using NUnit.Framework;
using UnityEngine;

namespace CoffeeBean.UI.Tests
{
    /// <summary>CUIPanelStats 统计测试：打开/关闭计数、类型分布、Single/Multiple 语义。</summary>
    public class CUIPanelStatsTests
    {
        [Test]
        public void RecordOpen_IncreasesCounters()
        {
            var stats = new CUIPanelStats();

            stats.RecordOpen(typeof(TestPanel));
            stats.RecordOpen(typeof(TestPanel));

            Assert.AreEqual(2, stats.TotalOpens);
            Assert.AreEqual(2, stats.CurrentOpen);
            Assert.AreEqual(2, stats.GetOpenTotal(typeof(TestPanel)));
            Assert.AreEqual(2, stats.GetCurrentOpen(typeof(TestPanel)));
        }

        [Test]
        public void RecordClose_DecreasesCurrent()
        {
            var stats = new CUIPanelStats();
            stats.RecordOpen(typeof(TestPanel));
            stats.RecordOpen(typeof(TestPanel));

            stats.RecordClose(typeof(TestPanel));

            Assert.AreEqual(1, stats.CurrentOpen, "关闭后当前打开数应减一");
            Assert.AreEqual(1, stats.GetCurrentOpen(typeof(TestPanel)));
            Assert.AreEqual(2, stats.GetOpenTotal(typeof(TestPanel)), "累计打开次数不变");
            Assert.AreEqual(1, stats.TotalCloses);
        }

        [Test]
        public void RecordClose_NotBelowZero()
        {
            var stats = new CUIPanelStats();

            stats.RecordClose(typeof(TestPanel));
            stats.RecordClose(typeof(TestPanel));

            Assert.AreEqual(0, stats.CurrentOpen, "当前打开数不应为负");
        }

        [Test]
        public void MultipleTypes_TrackedSeparately()
        {
            var stats = new CUIPanelStats();
            stats.RecordOpen(typeof(TestPanel));
            stats.RecordOpen(typeof(TestPanelB));

            Assert.AreEqual(1, stats.GetCurrentOpen(typeof(TestPanel)));
            Assert.AreEqual(1, stats.GetCurrentOpen(typeof(TestPanelB)));
            Assert.AreEqual(2, stats.CurrentOpen);
        }

        [Test]
        public void Reset_ClearsAll()
        {
            var stats = new CUIPanelStats();
            stats.RecordOpen(typeof(TestPanel));

            stats.Reset();

            Assert.AreEqual(0, stats.TotalOpens);
            Assert.AreEqual(0, stats.CurrentOpen);
            Assert.AreEqual(0, stats.GetOpenTotal(typeof(TestPanel)));
        }
    }

    /// <summary>CUIManager 集成统计：打开面板应自动记录，关闭面板减少当前数。</summary>
    public class CUIManagerStatsIntegrationTests
    {
        private CUIManager _manager;
        private TestPanelLoaderPool _pool;

        [SetUp]
        public void SetUp()
        {
            CUIManager.ResetInstanceForTest();
            CUIRoot.ResetInstanceForTest();
            TestPanelLoader.ClearPrefabs();

            _manager = CUIManager.Instance;
            _pool = new TestPanelLoaderPool();
            _manager.PanelLoaderPool = _pool;
            _manager.Stats.Reset();

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
        public void OpenPanel_RecordsStats()
        {
            _manager.OpenPanel<TestPanel>();
            _manager.OpenPanel<TestPanelB>();

            Assert.AreEqual(2, _manager.Stats.TotalOpens);
            Assert.AreEqual(2, _manager.Stats.CurrentOpen);
        }

        [Test]
        public void OpenThenClose_UpdatesStats()
        {
            var panel = _manager.OpenPanel<TestPanel>();

            _manager.ClosePanel(panel);

            Assert.AreEqual(1, _manager.Stats.TotalOpens);
            Assert.AreEqual(1, _manager.Stats.TotalCloses);
            Assert.AreEqual(0, _manager.Stats.CurrentOpen, "关闭后当前打开数应为 0");
        }

        [Test]
        public void CloseAllPanel_ResetsStats()
        {
            _manager.OpenPanel<TestPanel>();
            _manager.OpenPanel<TestPanelB>();

            _manager.CloseAllPanel();

            Assert.AreEqual(0, _manager.Stats.CurrentOpen);
        }
    }
}
