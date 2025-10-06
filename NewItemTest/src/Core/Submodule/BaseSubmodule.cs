// Source code is taken from CommonAPI (GPL-3.0) - https://github.com/limoka/CommonAPI
using System;

namespace YuanAPI {
    public class BaseSubmodule<TSubmodule> where TSubmodule : class {

        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;

        /// <summary>
        /// Throw Exception if the submodule is not loaded.
        /// </summary>
        internal static void ThrowIfNotLoaded() {
            if (!Loaded) {
                throw new InvalidOperationException(
                    $"{nameof(TSubmodule)} is not loaded. Please use [{nameof(YuanAPISubmoduleDependency)}(nameof({nameof(TSubmodule)})]");
            }
        }
    }
}
