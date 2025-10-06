using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanAPI {
    /// <summary>
    /// Indicates that loading something has failed
    /// </summary>
    public class LoadException : Exception {
        public LoadException(string message) : base(message) { }
    }

    [YuanAPISubmodule]
    public class ResourceRegistry : BaseSubmodule<ResourceRegistry> {

    }

}
