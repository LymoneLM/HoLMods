// Source code is taken from CommonAPI (GPL-3.0) - https://github.com/limoka/CommonAPI
using BepInEx.Logging;

namespace YuanAPI {
    /// <summary>
    /// Logger wrapper to appease Unit testing assembly...
    /// </summary>
    public class LoggerWrapper : IYuanLogger {
        public ManualLogSource logSource;

        public LoggerWrapper(ManualLogSource logSource) {
            this.logSource = logSource;
        }


        public void LogFatal(object data) {
            logSource.LogFatal(data);
        }

        public void LogError(object data) {
            logSource.LogError(data);
        }

        public void LogWarning(object data) {
            logSource.LogWarning(data);
        }

        public void LogMessage(object data) {
            logSource.LogMessage(data);
        }

        public void LogInfo(object data) {
            logSource.LogInfo(data);
        }

        public void LogDebug(object data) {
            logSource.LogDebug(data);
        }
    }
}