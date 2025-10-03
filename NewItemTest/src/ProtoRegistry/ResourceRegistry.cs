using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewItemTest {
    /// <summary>
    /// Indicates that loading something has failed
    /// </summary>
    public class LoadException : Exception {
        public LoadException(string message) : base(message) { }
    }

    public static class ResourceRegistry
    {

    }

}
