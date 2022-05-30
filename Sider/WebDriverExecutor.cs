using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using Sider.Models;
using Sider.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sider
{
    public partial class WebDriverExecutor
    {
        protected IWebDriver driver;

        public Dictionary<string, object> Variables { get; init; }
        public string BaseUrl { get; init; }

        public TimeSpan ImplicitWait { get; set; } = TimeSpan.FromSeconds(10);

        public WebDriverExecutor(IWebDriver driver, string baseUrl, Dictionary<string, object>? variables = null)
        {
            this.driver = driver;
            this.BaseUrl = baseUrl;
            this.Variables = variables ?? new();
        }

        public void ExecuteTest(Test test)
        {
            driver.Manage().Timeouts().ImplicitWait = this.ImplicitWait;

            foreach (var command in test.Commands)
            {
                this.ExecuteCommand(command);
            }
        }

        // https://github.com/SeleniumHQ/selenium/blob/trunk/java/src/org/openqa/selenium/server/htmlrunner/NonReflectiveSteps.java#L214
        // https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/webdriver/Keys.cs
        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts

        // ^\s*value\s*=\s*value\.replace\((".+")\s*,\s*(Keys\..)(.*)\)\s*;
        // {$1, $2\L($3)},

        // _(?!.+")(.)
        // \u$1

        // \.Numpad
        // \.NumberPad

        // etc
        // [Keys] enum does not support all keys ?
        protected Dictionary<string, string> keyMap = new Dictionary<string, string>
        {
            {"${KEY_ALT}", Keys.Alt},
            {"${KEY_CONTROL}", Keys.Control},
            {"${KEY_CTRL}", Keys.Control},
            {"${KEY_META}", Keys.Meta},
            {"${KEY_COMMAND}", Keys.Command},
            {"${KEY_SHIFT}", Keys.Shift},
            {"${KEY_BACKSPACE}", Keys.Backspace},
            {"${KEY_BKSP}", Keys.Backspace},
            {"${KEY_DELETE}", Keys.Delete},
            {"${KEY_DEL}", Keys.Delete},
            {"${KEY_ENTER}", Keys.Enter},
            {"${KEY_EQUALS}", Keys.Equal},
            {"${KEY_ESCAPE}", Keys.Escape},
            {"${KEY_ESC}", Keys.Escape},
            {"${KEY_INSERT}", Keys.Insert},
            {"${KEY_INS}", Keys.Insert},
            {"${KEY_PAUSE}", Keys.Pause},
            {"${KEY_SEMICOLON}", Keys.Semicolon},
            {"${KEY_SPACE}", Keys.Space},
            {"${KEY_TAB}", Keys.Tab},
            {"${KEY_LEFT}", Keys.Left},
            {"${KEY_UP}", Keys.Up},
            {"${KEY_RIGHT}", Keys.Right},
            {"${KEY_DOWN}", Keys.Down},
            {"${KEY_PAGE_UP}", Keys.PageUp},
            {"${KEY_PGUP}", Keys.PageUp},
            {"${KEY_PAGE_DOWN}", Keys.PageDown},
            {"${KEY_PGDN}", Keys.PageDown},
            {"${KEY_END}", Keys.End},
            {"${KEY_HOME}", Keys.Home},
            {"${KEY_NUMPAD0}", Keys.NumberPad0},
            {"${KEY_N0}", Keys.NumberPad0},
            {"${KEY_NUMPAD1}", Keys.NumberPad1},
            {"${KEY_N1}", Keys.NumberPad1},
            {"${KEY_NUMPAD2}", Keys.NumberPad2},
            {"${KEY_N2}", Keys.NumberPad2},
            {"${KEY_NUMPAD3}", Keys.NumberPad3},
            {"${KEY_N3}", Keys.NumberPad3},
            {"${KEY_NUMPAD4}", Keys.NumberPad4},
            {"${KEY_N4}", Keys.NumberPad4},
            {"${KEY_NUMPAD5}", Keys.NumberPad5},
            {"${KEY_N5}", Keys.NumberPad5},
            {"${KEY_NUMPAD6}", Keys.NumberPad6},
            {"${KEY_N6}", Keys.NumberPad6},
            {"${KEY_NUMPAD7}", Keys.NumberPad7},
            {"${KEY_N7}", Keys.NumberPad7},
            {"${KEY_NUMPAD8}", Keys.NumberPad8},
            {"${KEY_N8}", Keys.NumberPad8},
            {"${KEY_NUMPAD9}", Keys.NumberPad9},
            {"${KEY_N9}", Keys.NumberPad9},
            {"${KEY_ADD}", Keys.Add},
            {"${KEY_NUM_PLUS}", Keys.Add},
            {"${KEY_DECIMAL}", Keys.Decimal},
            {"${KEY_NUM_PERIOD}", Keys.Decimal},
            {"${KEY_DIVIDE}", Keys.Divide},
            {"${KEY_NUM_DIVISION}", Keys.Divide},
            {"${KEY_MULTIPLY}", Keys.Multiply},
            {"${KEY_NUM_MULTIPLY}", Keys.Multiply},
            {"${KEY_SEPARATOR}", Keys.Separator},
            {"${KEY_SEP}", Keys.Separator},
            {"${KEY_SUBTRACT}", Keys.Subtract},
            {"${KEY_NUM_MINUS}", Keys.Subtract},
            {"${KEY_F1}", Keys.F1},
            {"${KEY_F2}", Keys.F2},
            {"${KEY_F3}", Keys.F3},
            {"${KEY_F4}", Keys.F4},
            {"${KEY_F5}", Keys.F5},
            {"${KEY_F6}", Keys.F6},
            {"${KEY_F7}", Keys.F7},
            {"${KEY_F8}", Keys.F8},
            {"${KEY_F9}", Keys.F9},
            {"${KEY_F10}", Keys.F10},
            {"${KEY_F11}", Keys.F11},
            {"${KEY_F12}", Keys.F12},
        };

        public void ExecuteCommand(Models.Command command)
        {
            // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L210
            // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L264

            this.switchCommand(command);
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L859
        private void doAssert(Models.Command command)
        {
            var variable = this.Variables[command.Target].ToString();
            if (variable != this.interpolateString(command.Value))
            {
                throw new SeleniumAssertionException($"Actual value '{variable}' did not match '{command.Value}'");
            }
        }

        private void doSetWindowSize(Models.Command command)
        {
            var sizeStrings = command.Target.Split('x');
            this.driver.Manage().Window.Size = new Size(int.Parse(sizeStrings[0]), int.Parse(sizeStrings[1]));
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L640
        private void doEditContent(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            this.driver.ExecuteJavaScript(
                "if(arguments[0].contentEditable === 'true') {arguments[0].innerText = arguments[1]} else {throw new Error('Element is not content editable')}",
                element,
                this.interpolateString(command.Value));
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L868
        private void doAssertAlert(Models.Command command)
        {
            var alert = this.driver.SwitchTo().Alert();
            var actualText = alert.Text;
            var expectedText = interpolateString(command.Target);

            if (actualText != expectedText)
            {
                throw new SeleniumAssertionException($"Actual alert text '{actualText}' did not match '{expectedText}");
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L524
        private void doMouseOut(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));

            // TODO: check return type
            var (rect, vp) = this.driver.ExecuteJavaScript<(Rectangle rect, Size vp)>(
                "return [arguments[0].getBoundingClientRect(), {height: window.innerHeight, width: window.innerWidth}];",
                element);

            // try top
            if (rect.Top > 0)
            {
                var y = -(rect.Height / 2 + 1);
                new Actions(driver)
                    .MoveToElement(element, 0, y)
                    .Perform();
                return;
            }
            // try right
            else if (vp.Width > rect.Right)
            {
                var x = rect.Right / 2 + 1;
                new Actions(driver)
                    .MoveToElement(element, x, 0)
                    .Perform();
                return;
            }
            // try bottom
            else if (vp.Height > rect.Bottom)
            {
                var y = rect.Height / 2 + 1;
                new Actions(driver)
                    .MoveToElement(element, 0, y)
                    .Perform();
                return;
            }
            // try left
            else if (rect.Left > 0)
            {
                var x = -rect.Right / 2;
                new Actions(driver)
                    .MoveToElement(element, x, 0)
                    .Perform();
                return;
            }

            throw new NotSupportedException(
              "Unable to perform mouse out as the element takes up the entire viewport"
            );
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L592
        private void doMouseUp(Models.Command command)
        {
            var elem = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(elem)
                .Release()
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L475
        private void doMouseDown(Models.Command command)
        {
            var elem = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(elem)
                .ClickAndHold()
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L286
        private void doSelectWindow(Models.Command command)
        {
            var prefix = "handle=";
            if (command.Target.StartsWith(prefix))
            {
                var handle = command.Target.Substring(prefix.Length);
                this.driver.SwitchTo().Window(handle);
            }
            else
            {
                throw new InvalidOperationException("Invalid window handle given (e.g. handle=${handleVariable})");
            }
        }

        private void doClose(Models.Command command)
        {
            this.driver.Close();
        }

        private void doSelectFrame(Models.Command command)
        {
            var locator = this.interpolateString(command.Target);

            var targetLocator = this.driver.SwitchTo();
            if (locator == "relative=top")
            {
                targetLocator.DefaultContent();
            }
            else if (locator == "relative=parent")
            {
                targetLocator.ParentFrame();
            }
            else if (locator.StartsWith("index="))
            {
                targetLocator.Frame(locator["index=".Length..]);
            }
            else
            {
                var element = this.selectElement(locator);
                targetLocator.Frame(element);
            }
        }

        private void doSubmit(Models.Command command)
        {
            throw new NotSupportedException(@"""submit"" is not a supported command in Selenium WebDriver. Please re-record the step.");
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L625
        private void doSelect(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var option = element.FindElement(parseOptionLocator(this.interpolateString(command.Value)));

            option.Click();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L797
        private void doStoreAttribute(Models.Command command)
        {
            var attributeLocator = interpolateString(command.Target);
            var attributePos = attributeLocator.LastIndexOf('@');
            var elementLocator = attributeLocator[0..attributePos];
            var attributeName = attributeLocator[(attributePos + 1)..];

            var element = this.selectElement(elementLocator);
            var value = element.GetAttribute(attributeName);
            this.Variables[command.Value] = value;
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L830
        private void doStoreTitle(Models.Command command)
        {
            this.Variables[command.Value] = this.driver.Title;
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L835
        private void doStoreValue(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            this.Variables[command.Value] = element.GetAttribute("value");
        }

        private void doStoreWindowHandle(Models.Command command)
        {
            var handle = this.driver.CurrentWindowHandle;
            this.Variables[command.Value] = handle;
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L817
        private void doStoreText(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            this.Variables[command.Value] = element.Text;
        }

        private void doStore(Models.Command command)
        {
            this.Variables[command.Value] = this.interpolateString(command.Target);
        }

        private void doPause(Models.Command command)
        {
            Thread.Sleep(int.Parse(command.Target) * 1000);
        }

        private void doRun(Models.Command command)
        {
            throw new NotSupportedException($"`run` is not supported in this run mode");
        }

        private void doSetSpeed(Models.Command command)
        {
            throw new NotSupportedException($"`set speed` is not supported in this run mode");
        }

        private void doExecuteAsyncScript(Models.Command command)
        {
            var (script, args) = this.interpolateScript(command.Target);
            var result = this.driver.ExecuteJavaScript<object>(
                $"var callback = arguments[arguments.length - 1];{script}.then(callback).catch(callback);",
                args.ToArray());

            if (!string.IsNullOrEmpty(command.Value))
            {
                this.Variables[command.Value] = result;
            }
        }

        private void doAcceptAlert(Models.Command command)
        {
            this.driver.SwitchTo().Alert().Accept();
        }

        private void doAcceptConfirmation(Models.Command command)
        {
            this.driver.SwitchTo().Alert().Accept();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L744
        private void doExecuteScript(Models.Command command)
        {
            var (script, argv) = interpolateScript(command.Target);
            var result = driver.ExecuteJavaScript<object>(script, argv);

            if (!string.IsNullOrEmpty(command.Value))
            {
                this.Variables[command.Value] = result;
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L740
        private void doRunScript(Models.Command command)
        {
            var (script, argv) = interpolateScript(command.Target);
            driver.ExecuteJavaScript(script, argv);
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L1145
        private void doEcho(Models.Command command)
        {
            var text = this.interpolateString(command.Target);
            Console.WriteLine($"echo: {text}");
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L669
        private void doSendKeys(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var key = command.Value;
            if (this.keyMap.Keys.Contains(key))
                key = this.keyMap[key];
            element.SendKeys(key);
        }

        void wait(Action action, int timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(timeout));

            try
            {
                var task = new Task(action, cts.Token);

                task.Start();
                task.Wait();

            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    throw new OperationCanceledException("Timed out waiting for promise to resolve");
                }
            }
        }

        private void doWaitForElementEditable(Models.Command command)
        {
            throw new NotImplementedException();
        }

        private void doWaitForElementNotEditable(Models.Command command)
        {
            throw new NotImplementedException();
        }

        private void doWaitForElementPresent(Models.Command command)
        {
            throw new NotImplementedException();
        }

        private void doWaitForElementNotPresent(Models.Command command)
        {
            throw new NotImplementedException();
        }

        private void doWaitForElementVisible(Models.Command command)
        {
            throw new NotImplementedException();
        }

        private void doWaitForElementNotVisible(Models.Command command)
        {
            throw new NotImplementedException();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L656
        private void doType(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            element.Clear();
            element.SendKeys(command.Value);
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L427
        private void doDoubleClick(Models.Command command)
        {
            var elem = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .DoubleClick(elem)
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L384
        private void doUncheck(Models.Command command)
        {
            this.doCheckTest(command, elem => elem.GetAttribute("checked")?.ToLower() == "true");
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L370
        private void doCheck(Models.Command command)
        {
            this.doCheckTest(command, elem => elem.GetAttribute("checked")?.ToLower() != "true");
        }

        private void doCheckTest(Models.Command command, Func<IWebElement, bool> test)
        {
            var elem = this.selectElement(this.interpolateString(command.Target));

            if (elem.TagName.ToLower() == "input" &&
                elem.GetAttribute("type").ToLower() == "checkbox")
            {
                var c = elem.GetAttribute("checked");
                if (test(elem))
                {
                    elem.Click();
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L398
        private void doClick(Models.Command command)
        {
            var elem = this.selectElement(this.interpolateString(command.Target));
            elem.Click();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/master/packages/selianize/src/command.js#L266
        private void doOpen(Models.Command command)
        {
            var url = command.Target;
            if (!Regex.IsMatch(url, @"^(file|http|https):\/\/"))
            {
                url = new Uri(new Uri(this.BaseUrl), command.Target).ToString();
            }
            driver.Navigate().GoToUrl(url);
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L962
        private void doAssertText(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var text = element.Text;
            var value = this.interpolateString(command.Value);
            if (text != value)
            {
                throw new SeleniumAssertionException($"Actual value '{text}' did not match '{value}'");
            }
        }
        private void doAssertNotText(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var text = element.Text;
            var value = this.interpolateString(command.Value);
            if (text == value)
            {
                throw new SeleniumAssertionException($"Actual value '{text}' did match '{value}'");
            }
        }

        private void doAssertValue(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var text = element.GetAttribute("value");
            var value = this.interpolateString(command.Value);
            if (text != value)
            {
                throw new SeleniumAssertionException($"Actual value '{text}' did not match '{value}'");
            }
        }

        private void doAssertNotValue(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var text = element.GetAttribute("value");
            var value = this.interpolateString(command.Value);
            if (text == value)
            {
                throw new SeleniumAssertionException($"Actual value '{text}' did match '{value}'");
            }
        }

        private void doAssertChecked(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            if (!element.Selected)
            {
                throw new SeleniumAssertionException($"Element is not checked, expected to be checked");
            }
        }

        private void doAssertNotChecked(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            if (element.Selected)
            {
                throw new SeleniumAssertionException($"Element is checked, expected to be unchecked");
            }
        }
        private void doAssertSelectedValue(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var text = element.GetAttribute("value");
            var value = this.interpolateString(command.Value);
            if (text != value)
            {
                throw new SeleniumAssertionException($"Actual value '{text}' did not match '{value}'");
            }
        }

        private void doAssertNotSelectedValue(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var text = element.GetAttribute("value");
            var value = this.interpolateString(command.Value);
            if (text == value)
            {
                throw new SeleniumAssertionException($"Actual value '{text}' did match '{value}'");
            }
        }

        private void doAssertSelectedLabel(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var value = element.GetAttribute("value");
            var optionElement = this.driver.FindElement(By.XPath($"option[@value=\"{value}\"]"));
            var optionLabel = optionElement.Text;
            var label = this.interpolateString(command.Value);
            if (optionLabel != label)
            {
                throw new SeleniumAssertionException($"Actual value '{optionLabel}' did not match '{label}'");
            }
        }

        private void doAssertNotSelectedLabel(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var value = element.GetAttribute("value");
            var optionElement = this.driver.FindElement(By.XPath($"option[@value=\"{value}\"]"));
            var optionLabel = optionElement.Text;
            var label = this.interpolateString(command.Value);
            if (optionLabel == label)
            {
                throw new SeleniumAssertionException($"Actual value '{optionLabel}' did match '{label}'");
            }
        }

        private void doDebugger(Models.Command command)
        {
            throw new SeleniumAssertionException($"`debugger` is not supported in this run mode");
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L924
        private void doAssertPrompt(Models.Command command)
        {
            var alert = this.driver.SwitchTo().Alert();
            var actualText = alert.Text;
            var expectedText = interpolateString(command.Target);

            if (actualText != expectedText)
            {
                throw new SeleniumAssertionException($"Actual prompt text '{actualText}' did not match '{expectedText}");
            }
        }

        private void doAssertTitle(Models.Command command)
        {
            var actualTitle = this.driver.Title;
            var title = command.Target;

            if (title != actualTitle)
            {
                throw new SeleniumAssertionException($"Actual value '{actualTitle}' did not match '{title}");
            }
        }

        private void doAssertElementPresent(Models.Command command)
        {
            var elements = this.driver.FindElements(this.parseLocator(command.Target));

            if (elements is null)
            {
                throw new SeleniumAssertionException($"The elements was not found in page");
            }
        }

        private void doAssertElementNotPresent(Models.Command command)
        {
            var elements = this.driver.FindElements(this.parseLocator(command.Target));

            if (elements?.Count > 0)
            {
                throw new SeleniumAssertionException($"Unexpected elements was found in page");
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L910
        private void doAssertNotEditable(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));

            if (this.isElementEditable(element))
            {
                throw new SeleniumAssertionException("Element is not editable");
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L896
        private void doAssertEditable(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));

            if (!this.isElementEditable(element))
            {
                throw new SeleniumAssertionException("Element is editable");
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L1200
        private bool isElementEditable(IWebElement element)
        {
            var enabled = this.driver.ExecuteJavaScript<bool>("return !arguments[0].disabled;", element);
            var readOnly = this.driver.ExecuteJavaScript<bool>("return arguments[0].readOnly;", element);
            return enabled && !readOnly;
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L882
        private void doAssertConfirmation(Models.Command command)
        {
            var alert = this.driver.SwitchTo().Alert();
            var actualText = alert.Text;
            var expectedText = interpolateString(command.Target);

            if (actualText != expectedText)
            {
                throw new SeleniumAssertionException($"Actual confirm text '{actualText}' did not match '{expectedText}");
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L812
        private void doStoreJson(Models.Command command)
        {
            var json = this.interpolateString(command.Target);
            this.Variables[command.Value] = JsonConvert.DeserializeObject(json);
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L807
        private void doStoreElementCount(Models.Command command)
        {
            var elements = this.driver.FindElements(this.parseLocator(this.interpolateString(command.Target)));
            this.Variables[command.Value] = elements.Count;
        }

        private void doMouseUpAt(Models.Command command)
        {
            var coords = parseCoordString(this.interpolateString(command.Value));
            var element = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(element, coords[0], coords[1])
                .Release()
                .Perform();
        }

        private void doMouseOver(Models.Command command)
        {
            var elem = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(elem)
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L508
        private void doMouseMoveAt(Models.Command command)
        {
            var coords = parseCoordString(this.interpolateString(command.Value));
            var element = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(element, coords[0], coords[1])
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L491
        private void doMouseDownAt(Models.Command command)
        {
            var coords = parseCoordString(this.interpolateString(command.Value));
            var element = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(element, coords[0], coords[1])
                .ClickAndHold()
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L456
        private void doDragAndDropToObject(Models.Command command)
        {
            var dropSource = this.selectElement(this.interpolateString(command.Target));
            var dropTarget = this.selectElement(this.interpolateString(command.Value));

            new Actions(driver)
                .DragAndDrop(dropSource, dropTarget)
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L439
        private void doDoubleClickAt(Models.Command command)
        {
            var coords = parseCoordString(this.interpolateString(command.Value));
            var element = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(element, coords[0], coords[1])
                .DoubleClick()
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L410
        private void doClickAt(Models.Command command)
        {
            var coords = parseCoordString(this.interpolateString(command.Value));
            var element = this.selectElement(this.interpolateString(command.Target));

            new Actions(driver)
                .MoveToElement(element, coords[0], coords[1])
                .Click()
                .Perform();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L346
        private void doRemoveSelection(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));

            if (string.IsNullOrEmpty(element.GetAttribute("multiple")))
            {
                throw new Exception("Given element is not a multiple select type element");
            }

            var option = element.FindElement(parseOptionLocator(this.interpolateString(command.Value)));

            // TODO: check return type
            var selections = this.driver.ExecuteJavaScript<IWebElement[]>("return arguments[0].selectedOptions", element);

            if (findElement(selections, option))
            {
                option.Click();
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L327
        private void doAddSelection(Models.Command command)
        {
            var element = this.selectElement(this.interpolateString(command.Target));
            var option = element.FindElement(parseOptionLocator(this.interpolateString(command.Value)));

            // TODO: check return type
            var selections = this.driver.ExecuteJavaScript<IWebElement[]>("return arguments[0].selectedOptions", element);
            if (!findElement(selections, option))
            {
                option.Click();
            }
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L774
        private void doAnswerPrompt(Models.Command command)
        {
            var alert = this.driver.SwitchTo().Alert();
            if (!string.IsNullOrEmpty(command.Value))
            {
                alert.SendKeys(command.Value);
            }
            alert.Accept();
        }

        private void doDismissConfirmation(Models.Command command)
        {
            this.driver.SwitchTo().Alert().Dismiss();
        }

        private void doDismissPrompt(Models.Command command)
        {
            this.driver.SwitchTo().Alert().Dismiss();
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/webdriver.ts#L1175
        protected IWebElement selectElement(string target)
        {
            return driver.FindElement(this.parseLocator(target));
        }


        protected WebDriverExecutorCondEvalResult EvaluateConditional(Models.Command command)
        {
            var (script, args) = interpolateScript(command.Target);
            return EvaluateConditional(script, args);
        }

        protected WebDriverExecutorCondEvalResult EvaluateConditional(string script, List<object> args)
        {
            var result = this.driver.ExecuteJavaScript<bool>($"return ({script})", args);
            return new(result);
        }

        protected By parseLocator(string locator)
        {
            if (locator.StartsWith("//"))
            {
                return By.XPath(locator);
            }

            var fragments = locator.Split('=');
            var type = fragments[0];
            var selector = string.Join('=', fragments.Skip(1));

            if (string.IsNullOrEmpty(selector))
                throw new InvalidOperationException("Locator can't be empty");

            return type switch
            {
                "css" => By.CssSelector(selector),
                "link" or "linkText" => By.LinkText(selector),
                "id" => By.Id(selector),
                "name" => By.Name(selector),
                "partialLinkText" => By.PartialLinkText(selector),
                "xpath" => By.XPath(selector),
                _ => throw new NotSupportedException($"Unknown locator {type}"),
            };
        }

        protected By parseOptionLocator(string locator)
        {
            var fragments = locator.Split('=');
            var type = fragments[0];
            var selector = string.Join('=', fragments.Skip(1));

            return type switch
            {
                "id" => By.CssSelector($"*[id=\"{selector}\"]"),
                "value" => By.CssSelector($"*[value=\"{selector}\"]"),
                "index" => By.CssSelector($"*:nth-child({selector})"),
                _ when (!string.IsNullOrEmpty(selector)) => By.XPath($"//option[. = '{selector}']"),
                _ => throw new NotSupportedException($"Unknown selection locator { type } : Locator can't be empty")
            };
        }

        protected bool findElement(IWebElement[] elements, IWebElement element)
        {
            var id = element.GetAttribute("id");
            return elements.Any(e => e.GetAttribute("id") == id);
        }

        protected int[] parseCoordString(string coord)
        {
            return coord.Split(',').Select(v => int.Parse(v)).ToArray();
        }

        protected string interpolateString(string value)
            => value.InterpolateString(this.Variables);
        protected (string script, List<object> argv) interpolateScript(string value)
            => value.InterpolateScript(this.Variables);
    }
}
