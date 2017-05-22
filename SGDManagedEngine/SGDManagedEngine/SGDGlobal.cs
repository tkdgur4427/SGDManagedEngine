using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    /// <summary>
    /// (ClassType<T> where T : class, new())
    ///     - T can not be int, float, double, DataTime or any other struct (value type)
    ///     - T must be class and must have a public parameter-less default constructor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class H1Global<T> where T : class, new() 
    {
        public static readonly T Instance = new T();
    }
}
