using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.Commands
{
    public interface H1CommandBase
    {
        // need to override the DoWork()
        void DoWork();
    }

    // base command
    public class H1Command<T> : H1CommandBase
    {
        // make sure that put the command instance with parameters
        public H1Command(T InData)
        {
            Data = InData;
            ExecuteObject = null;
        }

        public H1Command(T InData, Action<T> InExecuteObject)
        {
            Data = InData;
            ExecuteObject = InExecuteObject;
        }

        // execute the logic
        void H1CommandBase.DoWork()
        {
            if (ExecuteObject == null)
                return;

            // execute the action
            ExecuteObject(Data);
        }

        void SetExecutObj(Action<T> InExecuteObj)
        {
            ExecuteObject = InExecuteObj;
        }

        // universal data for executing command
        protected T Data;

        // function object
        protected Action<T> ExecuteObject;
    }
}
