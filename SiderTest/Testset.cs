using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Sider;
using Sider.Models;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SiderTest
{
    public class Testset : IDisposable
    {
        string basePath = "";
        string dir = "testset";
        string screenshotsDirName = "screenshots";
        HttpListener? listener = null;
        bool listening = false;

        public Testset()
        {
            this.basePath = this.GetType().Assembly.Location;

            for (var i = 0; i < 4; i++)
                this.basePath = Path.GetDirectoryName(this.basePath) ?? "";

            this.basePath = Path.Join(this.basePath, this.dir);
        }

        public void Dispose()
        {
            this.CloseListener();
        }

        internal async Task StartHttpServer(string filePath)
        {
            this.CloseListener();

            this.listener = new HttpListener();

            this.listening = true;
            this.listener.Prefixes.Add("http://localhost:8080/");

            this.listener.Start();

            while (this.listening)
            {
                var context = await this.listener.GetContextAsync();

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

        internal void Run(string testName)
        {
            var sidePath = Path.Join(this.basePath, testName, testName + ".side");
            var htmlPath = Path.Join(this.basePath, testName, testName + ".html");
            var screenshotPath = Path.Join(this.basePath, this.screenshotsDirName);

            var options = new ChromeOptions();
            options.AddArgument("--headless");

            using var driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory, options);

            var project = JsonConvert.DeserializeObject<Project>(File.ReadAllText(sidePath, System.Text.Encoding.UTF8));
            var test = project?.Tests.First(t => t.Name == testName) ?? throw new Exception($"Invalid test name : \"{testName}\"");

            // var sider = new WebDriverExecutor(driver, project.Url.ToString());
            var sider = new CommandNodeExecutor(driver, project.Url.ToString());
            var task = this.StartHttpServer(htmlPath);

            sider.ExecuteTest(test);

            driver.FindElement(By.Id("validate")).Click();
            var result = driver.FindElement(By.Id("result")).Text;

            var image = driver.GetScreenshot();

            if (!Directory.Exists(screenshotPath))
            {
                Directory.CreateDirectory(screenshotPath);
            }

            image.SaveAsFile(Path.Join($"{screenshotPath}", $"{testName}.actual.png"));
            this.CloseListener();

            Assert.Equal("OK", result);

            if(Platform.CurrentPlatform.PlatformType == PlatformType.Windows)
            {
                using var expectedImage = Image.Load(Path.Join($"{screenshotPath}", $"{testName}.expected.png"));
                using var actualImage = Image.Load(Path.Join($"{screenshotPath}", $"{testName}.actual.png"));
                var format = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;
                Assert.Equal(expectedImage.ToBase64String(format), actualImage.ToBase64String(format));
            }
        }

        internal void CloseListener()
        {
            if (this.listening)
            {
                this.listening = false;
                this.listener?.Close();
                this.listener = null;
            }
        }

        [Fact]
        public void CheckUncheck()
        {
            this.Run("check-uncheck");
        }

        [Fact]
        public void Click()
        {
            this.Run("click");
        }

        [Fact]
        public void DoubleClick()
        {
            this.Run("double-click");
        }

        [Fact]
        public void Type()
        {
            this.Run("type");
        }

        [Fact]
        public void If()
        {
            this.Run("if");
        }

        [Fact]
        public void Times()
        {
            this.Run("times");
        }

        [Fact]
        public void ForEach()
        {
            this.Run("forEach");
        }
    }
}