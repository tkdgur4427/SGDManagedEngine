using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.Commands
{
    public class H1CommandManager
    {
        public void AddCommand(H1CommandBase InCommand)
        {
            Commands.Add(InCommand);
        }

        public void ExecuteCommands()
        {
            // execute batched commands
            foreach (var Command in Commands)
            {
                Command.DoWork();
            }

            // empty the commands
            Commands.Clear();
        }

        // commands to execute
        protected List<H1CommandBase> Commands = new List<H1CommandBase>();
    }
}
