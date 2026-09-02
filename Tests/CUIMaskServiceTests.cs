using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CoffeeBean.UI.Tests
{
    /// <summary>CUIMaskService 遮罩测试：PopUI 层打开自动生成遮罩、关闭自动移除、同层共享。</summary>
    public class CUIMaskServiceTests
    {
        private CUIManager _manager;
        private TestPanelLoaderPool _pool;

        [SetUp]
        public void SetUp()
        {
            CUIManager.ResetInstanceForTest();
            CUIRoot.ResetInstanceForTest();
            TestPanelLoader.ClearPrefabs();
            CUIMaskService.ClearAll();

            _manager = CUIManager.Instance;
            _pool = new TestPanelLoaderPool();
            _manager.PanelLoaderPool = _pool;

            TestPanelLoader.RegisterPrefab("TestPanel", UITestUtil.CreatePanelPrefab<TestPanel>());
            TestPanelLoader.RegisterPrefab("TestPanelB", UITestUtil.CreatePanelPrefab<TestPanelB>());
        }

        [TearDown]
        public void TearDown()
        {
            _manager.CloseAllPanel();
            CUIManager.ResetInstanceForTest();
            CUIRoot.ResetInstanceForTest();
            CUIMaskService.ClearAll();
        }

        [Test]
        public void OpenPopUi_CreatesMask()
        {
            var panel = _manager.OpenPanel<TestPanel>(level: CUILevel.PopUI);

            Assert.IsNotNull(panel);
            Assert.IsTrue(CUIMaskService.HasMaskForLevel(CUILevel.PopUI), "打开 PopUI 层面板应生成遮罩");
        }

        [Test]
        public void OpenCommon_NoMask()
        {
            _manager.OpenPanel<TestPanel>(level: CUILevel.Common);

            Assert.IsFalse(CUIMaskService.HasMaskForLevel(CUILevel.Common), "Common 层不应生成遮罩");
            Assert.IsFalse(CUIMaskService.HasMaskForLevel(CUILevel.PopUI));
        }

        [Test]
        public void SameLevel_MultiplePanels_SingleMask()
        {
            var a = _manager.OpenPanel<TestPanel>(level: CUILevel.PopUI);
            var keys = new CUIPanelSearchKeys
            {
                PanelType = typeof(TestPanelB),
                Level = CUILevel.PopUI,
                OpenType = CUIPanelOpenType.Multiple,
            };
            var b = _manager.OpenPanel(keys);

            Assert.AreEqual(1, CUIMaskService.ActiveMaskCount, "同层多面板应共享一个遮罩");

            // 关闭一个 → 遮罩保留（同层还有面板）
            _manager.ClosePanel(a);
            Assert.AreEqual(1, CUIMaskService.ActiveMaskCount, "同层仍有面板时遮罩应保留");

            // 关闭最后一个 → 遮罩移除
            _manager.ClosePanel(b);
            Assert.AreEqual(0, CUIMaskService.ActiveMaskCount, "同层无面板后遮罩应移除");
        }

        [Test]
        public void ClosePanel_RemovesMask()
        {
            var panel = _manager.OpenPanel<TestPanel>(level: CUILevel.PopUI);
            Assert.IsTrue(CUIMaskService.HasMaskForLevel(CUILevel.PopUI));

            _manager.ClosePanel(panel);

            Assert.IsFalse(CUIMaskService.HasMaskForLevel(CUILevel.PopUI), "关闭面板后遮罩应移除");
            Assert.AreEqual(0, CUIMaskService.ActiveMaskCount);
        }

        [Test]
        public void MaskGo_LivesOnPopUiLayer()
        {
            _manager.OpenPanel<TestPanel>(level: CUILevel.PopUI);

            var layer = CUIRoot.Instance.GetLayer(CUILevel.PopUI);
            bool foundMask = false;
            for (int i = 0; i < layer.childCount; i++)
            {
                if (layer.GetChild(i).name.StartsWith("Mask_"))
                {
                    foundMask = true;
                    break;
                }
            }
            Assert.IsTrue(foundMask, "PopUI 层下应有遮罩 GameObject");
        }

        [Test]
        public void CloseAllPanel_ClearsMasks()
        {
            _manager.OpenPanel<TestPanel>(level: CUILevel.PopUI);
            Assert.IsTrue(CUIMaskService.ActiveMaskCount > 0);

            _manager.CloseAllPanel();

            Assert.AreEqual(0, CUIMaskService.ActiveMaskCount, "关闭全部面板应清空遮罩");
        }
    }
}
