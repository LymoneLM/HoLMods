namespace NewItemTest {
    /// <summary>
    /// Logger interface to appease Unit testing assembly...
    /// </summary>
    public interface ICommonLogger {
        void LogFatal(object data);
        void LogError(object data);
        void LogWarning(object data);
        void LogMessage(object data);
        void LogInfo(object data);
        void LogDebug(object data);
    }
}
