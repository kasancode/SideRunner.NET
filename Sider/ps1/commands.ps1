
function Create-Code(){
'using System;

namespace Sider
{
    public partial class WebDriverExecutor
    {
        private void switchCommand(Models.Command command)
        {
            switch (command.CommandName)
            {'

curl https://raw.githubusercontent.com/SeleniumHQ/selenium-ide/trunk/packages/side-runtime/src/webdriver.ts |
? { $_ -match "^\s+async do(.)([^\s]+)\("} |
%{'                case "{0}{1}": do{2}{1}(command); break;' -f $matches[1].ToLower(), $matches[2], $matches[1] }

'              default: throw new NotSupportedException();
            }
        }
    }
}'
}

Create-Code > ../SideCommanderSwitchCommand.cs
