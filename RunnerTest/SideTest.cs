using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Sider;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Tests
{
    public class Tests
    {
        string basePath = "";
        string dir = "testset";
        HttpListener listener = null;
        bool listening = false;

        [SetUp]
        public void Setup()
        {
            this.basePath = TestContext.CurrentContext.TestDirectory;

            for (var i = 0; i < 3; i++)
                this.basePath = Path.GetDirectoryName(this.basePath);

            this.basePath = Path.Join(this.basePath, this.dir);

        }

        [TearDown]
        public void CloseListener()
        {
            if (this.listening)
            {
                this.listening = false;
                this.listener?.Close();
                this.listener = null;
            }
        }

        public async Task StartHttpServer(string filePath)
        {
            this.CloseListener();

            this.listener = new HttpListener();

            this.listening = true;
            this.listener.Prefixes.Add("http://localhost:8080/");

            this.listener.Start();

            while (this.listening)
            {
                var context = await this.listener.GetContextAsync();
                var request = context.Request;

                var response = context.Response;
                response.ContentType = "text/html";
                response.StatusCode = (int)HttpStatusCode.OK;

                var buffer = File.ReadAllBytes(filePath);

                response.ContentLength64 = buffer.Length;

                using (var output = response.OutputStream)
                {
                    await output.WriteAsync(buffer, 0, buffer.Length);
                }
                response.Close();
            }
        }

        [Test]
        public void CheckUncheck()
        {
            this.Run("check-uncheck");
        }

        [Test]
        public void Click()
        {
            this.Run("click");
        }

        [Test]
        public void DoubleClick()
        {
            this.Run("double-click");
        }

        [Test]
        public void Type()
        {
            this.Run("type");
        }

        public void Run(string testName)
        {
            var sidePath = Path.Join(this.basePath, testName + Path.DirectorySeparatorChar + testName + ".side");
            var htmlPath = Path.Join(this.basePath, testName + Path.DirectorySeparatorChar + testName + ".html");

            var options = new ChromeOptions();
            options.AddArgument("--headless");

            using (var driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory, options))
            {
                var sider = new SideRunner(driver, File.ReadAllText(sidePath, System.Text.Encoding.UTF8));
                var task = this.StartHttpServer(htmlPath);

                sider.ExecuteTest(testName);

                driver.FindElement(By.Id("validate")).Click();
                var result = driver.FindElement(By.Id("result")).Text;
                Assert.AreEqual("OK", result);

                var image = driver.GetScreenshot();
                image.SaveAsFile("screenshot.png");
                this.CloseListener();
            }
        }
    }
}