namespace NewItemTest {
    /// <summary>
    /// Static container for logger interface to appease Unit testing assembly...
    /// </summary>
    public class YuanLogger {
        public static IYuanLogger logger;

        public static void SetLogger(IYuanLogger _logger) {
            logger = _logger;
        }
    }
}