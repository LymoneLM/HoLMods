#nullable enable
using System;
using System.Text.RegularExpressions;

namespace YuanAPI {
    /// <summary>
    /// SemVer Version
    /// https://semver.org/
    /// </summary>
    public class Version : IComparable<Version>, IEquatable<Version> {
        private const string SemVerPattern = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
        private static readonly Regex SemVerRegex = new Regex(SemVerPattern, RegexOptions.Compiled);

        /// <summary>
        /// 主版本号 (Major version)
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// 次版本号 (Minor version) 
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// 修订号 (Patch version)
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// 预发布标识 (Pre-release identifiers)
        /// </summary>
        public string? PreRelease { get; }

        /// <summary>
        /// 构建元数据 (Build metadata)
        /// </summary>
        public string? BuildMetadata { get; }

        /// <summary>
        /// 初始化一个新的SemVer版本号
        /// </summary>
        /// <param name="major">主版本号</param>
        /// <param name="minor">次版本号</param>
        /// <param name="patch">修订号</param>
        /// <param name="preRelease">预发布标识</param>
        /// <param name="buildMetadata">构建元数据</param>
        /// <exception cref="ArgumentException">当版本号参数无效时抛出</exception>
        public Version(int major, int minor, int patch, string? preRelease = null, string? buildMetadata = null) {
            if (major < 0)
                throw new ArgumentException("Major version cannot be negative", nameof(major));
            if (minor < 0)
                throw new ArgumentException("Minor version cannot be negative", nameof(minor));
            if (patch < 0)
                throw new ArgumentException("Patch version cannot be negative", nameof(patch));

            Major = major;
            Minor = minor;
            Patch = patch;
            PreRelease = preRelease;
            BuildMetadata = buildMetadata;
        }

        /// <summary>
        /// 从字符串解析SemVer版本号
        /// </summary>
        /// <param name="versionString">版本字符串</param>
        /// <returns>Version对象</returns>
        /// <exception cref="ArgumentException">当字符串格式无效时抛出</exception>
        public static Version Parse(string versionString) {
            if (string.IsNullOrWhiteSpace(versionString))
                throw new ArgumentException("Version string cannot be null or empty", nameof(versionString));

            // 使用正则表达式进行严格解析
            var match = SemVerRegex.Match(versionString);
            if (match.Success) {
                var major = int.Parse(match.Groups["major"].Value);
                var minor = int.Parse(match.Groups["minor"].Value);
                var patch = int.Parse(match.Groups["patch"].Value);
                var preRelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;
                var buildMetadata = match.Groups["buildmetadata"].Success ? match.Groups["buildmetadata"].Value : null;

                return new Version(major, minor, patch, preRelease, buildMetadata);
            }

            throw new ArgumentException("Version string is not SemVer Version", nameof(versionString));
        }

        /// <summary>
        /// 尝试从字符串解析SemVer版本号
        /// </summary>
        /// <param name="versionString">版本字符串</param>
        /// <param name="version">解析成功的Version对象</param>
        /// <returns>解析是否成功</returns>
        public static bool TryParse(string versionString, out Version? version) {
            version = null;

            if (string.IsNullOrWhiteSpace(versionString))
                return false;

            try {
                version = Parse(versionString);
                return true;
            } catch {
                return false;
            }
        }

        #region 比较操作符

        public static bool operator ==(Version? left, Version? right) {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(Version? left, Version? right) => !(left == right);

        public static bool operator <(Version? left, Version? right) {
            if (left is null)
                return !(right is null);
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Version? left, Version? right) {
            if (left is null)
                return true;
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Version? left, Version? right) => !(left <= right);

        public static bool operator >=(Version? left, Version? right) => !(left < right);

        #endregion

        #region IComparable<Version> 实现

        public int CompareTo(Version? other) {
            if (other is null)
                return 1;

            var majorCompare = Major.CompareTo(other.Major);
            if (majorCompare != 0)
                return majorCompare;

            var minorCompare = Minor.CompareTo(other.Minor);
            if (minorCompare != 0)
                return minorCompare;

            var patchCompare = Patch.CompareTo(other.Patch);
            if (patchCompare != 0)
                return patchCompare;

            // 根据SemVer规范，有预发布标识的版本号低于没有预发布标识的版本号
            return ComparePreRelease(PreRelease, other.PreRelease);
        }

        private static int ComparePreRelease(string? preRelease1, string? preRelease2) {
            // 如果都没有预发布标识，则相等
            if (string.IsNullOrEmpty(preRelease1) && string.IsNullOrEmpty(preRelease2))
                return 0;

            // 有预发布标识的版本号低于没有预发布标识的版本号
            if (string.IsNullOrEmpty(preRelease1))
                return 1;
            if (string.IsNullOrEmpty(preRelease2))
                return -1;

            #pragma warning disable CS8602 // 解引用可能出现空引用。
            var identifiers1 = preRelease1.Split('.');
            var identifiers2 = preRelease2.Split('.');
            #pragma warning restore CS8602 // 解引用可能出现空引用。

            var minLength = Math.Min(identifiers1.Length, identifiers2.Length);

            for (int i = 0; i < minLength; i++) {
                var id1 = identifiers1[i];
                var id2 = identifiers2[i];

                // 尝试解析为数字进行比较
                var isNumeric1 = int.TryParse(id1, out var num1);
                var isNumeric2 = int.TryParse(id2, out var num2);

                if (isNumeric1 && isNumeric2) {
                    var compare = num1.CompareTo(num2);
                    if (compare != 0)
                        return compare;
                } else if (isNumeric1) {
                    return -1; // 数字标识符比非数字标识符优先级低
                } else if (isNumeric2) {
                    return 1; // 非数字标识符比数字标识符优先级高
                } else {
                    var compare = string.Compare(id1, id2, StringComparison.Ordinal);
                    if (compare != 0)
                        return compare;
                }
            }

            return identifiers1.Length.CompareTo(identifiers2.Length);
        }

        #endregion

        #region IEquatable<Version> 实现

        public bool Equals(Version? other) => CompareTo(other) == 0;

        public override bool Equals(object? obj) => obj is Version other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                var hashCode = Major;
                hashCode = (hashCode * 397) ^ Minor;
                hashCode = (hashCode * 397) ^ Patch;
                hashCode = (hashCode * 397) ^ (PreRelease?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        #endregion

        /// <summary>
        /// 返回格式化的版本字符串
        /// </summary>
        public override string ToString() {
            var version = $"{Major}.{Minor}.{Patch}";

            if (!string.IsNullOrEmpty(PreRelease)) {
                version += $"-{PreRelease}";
            }

            if (!string.IsNullOrEmpty(BuildMetadata)) {
                version += $"+{BuildMetadata}";
            }

            return version;
        }

    }
}