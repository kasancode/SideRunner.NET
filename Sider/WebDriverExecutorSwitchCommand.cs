using System;

namespace Sider
{
    public partial class WebDriverExecutor
    {
        private void switchCommand(Models.Command command)
        {
            switch (command.CommandName)
            {
                case "open": doOpen(command); break;
                case "setWindowSize": doSetWindowSize(command); break;
                case "selectWindow": doSelectWindow(command); break;
                case "close": doClose(command); break;
                case "selectFrame": doSelectFrame(command); break;
                case "submit": doSubmit(command); break;
                case "addSelection": doAddSelection(command); break;
                case "removeSelection": doRemoveSelection(command); break;
                case "check": doCheck(command); break;
                case "uncheck": doUncheck(command); break;
                case "click": doClick(command); break;
                case "clickAt": doClickAt(command); break;
                case "doubleClick": doDoubleClick(command); break;
                case "doubleClickAt": doDoubleClickAt(command); break;
                case "dragAndDropToObject": doDragAndDropToObject(command); break;
                case "mouseDown": doMouseDown(command); break;
                case "mouseDownAt": doMouseDownAt(command); break;
                case "mouseMoveAt": doMouseMoveAt(command); break;
                case "mouseOut": doMouseOut(command); break;
                case "mouseOver": doMouseOver(command); break;
                case "mouseUp": doMouseUp(command); break;
                case "mouseUpAt": doMouseUpAt(command); break;
                case "select": doSelect(command); break;
                case "editContent": doEditContent(command); break;
                case "type": doType(command); break;
                case "sendKeys": doSendKeys(command); break;
                case "waitForElementEditable": doWaitForElementEditable(command); break;
                case "waitForElementNotEditable": doWaitForElementNotEditable(command); break;
                case "waitForElementPresent": doWaitForElementPresent(command); break;
                case "waitForElementNotPresent": doWaitForElementNotPresent(command); break;
                case "waitForElementVisible": doWaitForElementVisible(command); break;
                case "waitForElementNotVisible": doWaitForElementNotVisible(command); break;
                case "runScript": doRunScript(command); break;
                case "executeScript": doExecuteScript(command); break;
                case "executeAsyncScript": doExecuteAsyncScript(command); break;
                case "acceptAlert": doAcceptAlert(command); break;
                case "acceptConfirmation": doAcceptConfirmation(command); break;
                case "answerPrompt": doAnswerPrompt(command); break;
                case "dismissConfirmation": doDismissConfirmation(command); break;
                case "dismissPrompt": doDismissPrompt(command); break;
                case "store": doStore(command); break;
                case "storeAttribute": doStoreAttribute(command); break;
                case "storeElementCount": doStoreElementCount(command); break;
                case "storeJson": doStoreJson(command); break;
                case "storeText": doStoreText(command); break;
                case "storeTitle": doStoreTitle(command); break;
                case "storeValue": doStoreValue(command); break;
                case "storeWindowHandle": doStoreWindowHandle(command); break;
                case "assert": doAssert(command); break;
                case "assertAlert": doAssertAlert(command); break;
                case "assertConfirmation": doAssertConfirmation(command); break;
                case "assertEditable": doAssertEditable(command); break;
                case "assertNotEditable": doAssertNotEditable(command); break;
                case "assertPrompt": doAssertPrompt(command); break;
                case "assertTitle": doAssertTitle(command); break;
                case "assertElementPresent": doAssertElementPresent(command); break;
                case "assertElementNotPresent": doAssertElementNotPresent(command); break;
                case "assertText": doAssertText(command); break;
                case "assertNotText": doAssertNotText(command); break;
                case "assertValue": doAssertValue(command); break;
                case "assertNotValue": doAssertNotValue(command); break;
                case "assertChecked": doAssertChecked(command); break;
                case "assertNotChecked": doAssertNotChecked(command); break;
                case "assertSelectedValue": doAssertSelectedValue(command); break;
                case "assertNotSelectedValue": doAssertNotSelectedValue(command); break;
                case "assertSelectedLabel": doAssertSelectedLabel(command); break;
                case "assertNotSelectedLabel": doAssertNotSelectedLabel(command); break;
                case "debugger": doDebugger(command); break;
                case "echo": doEcho(command); break;
                case "pause": doPause(command); break;
                case "run": doRun(command); break;
                case "setSpeed": doSetSpeed(command); break;
                default: throw new NotSupportedException();
            }
        }
    }
}
