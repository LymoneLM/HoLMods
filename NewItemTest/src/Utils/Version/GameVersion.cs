namespace YuanAPI {
    internal class GameVersion {
        public Version GetGameVersion()
        {
            if (Mainload.Vision_now.Length <= 2)
                return null;
            Version.TryParse(Mainload.Vision_now.Substring(2), out var version);
            return version;
        }
    }
}
