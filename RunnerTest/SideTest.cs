using NUnit.Framework;
using System.IO;
using OpenQA.Selenium.Chrome;
using Sider;
using System;

namespace Tests
{
    public class Tests
    {
        string basePath = "";

        [SetUp]
        public void Setup()
        {
            this.basePath = TestContext.CurrentContext.TestDirectory;

            for (var i = 0; i < 3; i++)
                this.basePath = Path.GetDirectoryName(this.basePath);

            this.basePath = Path.Join(this.basePath, "examples");

        }

        [Test]
        public void SendKeys()
        {
            var dir = Path.Join(this.basePath, "send-keys.side");

            Assert.Throws<SeleniumAssertionException>(() =>
            {
                Run(dir, "send keys");
            });
        }

        [Test]
        public void SendKeysJa()
        {
            var dir = Path.Join(this.basePath, "send-keys-ja.side");

            Run(dir, "send keys");
        }

        [Test]
        public void SelectWindow()
        {
            var dir = Path.Join(this.basePath, "select-window.side");

            Run(dir, "select window");
        }


        public void Run(string filePath, string testName)
        {
            using (var driver = new ChromeDriver(TestContext.CurrentContext.TestDirectory))
            {
                var sider = new SideRunner(driver, File.ReadAllText(filePath, System.Text.Encoding.UTF8));
                sider.ExecuteTest(testName);
            }
        }
    }
}