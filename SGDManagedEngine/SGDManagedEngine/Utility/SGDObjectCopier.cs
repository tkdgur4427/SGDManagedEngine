using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace SGDManagedEngine.SGD
{
    public static class H1ObjectCopier
    {
        // adding 'this', enable extension method like (x.operation1(arg1).operation2(arg2))
        public static T Clone<T>(this T source) where T : H1DeepCopyable
        {
            return source.DeepCopy() as T;
        }        
    }
}
