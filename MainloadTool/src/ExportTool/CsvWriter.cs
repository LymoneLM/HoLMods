using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MainloadTool;

internal class CsvWriter{
    private readonly List<string> _lines;
    private readonly List<string> _currentLine;
    private readonly string _filePath;

    /// <summary>
    /// 创建CsvWriter实例
    /// </summary>
    /// <param name="fileName">文件名</param>
    public CsvWriter(string fileName) {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("文件名不能为空", nameof(fileName));

        // 构建完整文件路径（不立即创建目录）
        _filePath = Path.Combine("Output", $"{fileName.Trim()}.csv");

        _lines = new List<string>();
        _currentLine = new List<string>();
    }

    /// <summary>
    /// 写入一行数据
    /// </summary>
    /// <param name="fields">字段列表</param>
    public void WriteLine(List<string> fields) {
        if (fields == null)
            throw new ArgumentNullException(nameof(fields));

        var line = FormatCsvLine(fields);
        _lines.Add(line);
    }

    /// <summary>
    /// 写入一行数据
    /// </summary>
    /// <param name="fields">字段数组</param>
    public void WriteLine(params string[] fields) {
        WriteLine(fields.ToList());
    }

    /// <summary>
    /// 逐个添加字段到当前行
    /// </summary>
    /// <param name="field">单个字段</param>
    public void WriteField(string field) {
        _currentLine.Add(field ?? string.Empty);
    }

    /// <summary>
    /// 批量添加字段到当前行
    /// </summary>
    /// <param name="fields">字段列表</param>
    public void WriteField(List<string> fields) {
        if (fields == null)
            throw new ArgumentNullException(nameof(fields));

        _currentLine.AddRange(fields);
    }

    /// <summary>
    /// 批量添加字段到当前行
    /// </summary>
    /// <param name="fields">字段数组</param>
    public void WriteField(params string[] fields) {
        WriteField(fields.ToList());
    }

    /// <summary>
    /// 结束当前行，将缓冲的数据写入行列表
    /// </summary>
    public void EndRow() {
        if (_currentLine.Count > 0) {
            var line = FormatCsvLine(_currentLine);
            _lines.Add(line);
            _currentLine.Clear();
        }
    }

    /// <summary>
    /// 执行文件写入操作（忽略当前缓冲行）
    /// </summary>
    public void Save() {
        // 只有在保存时才创建目录和执行IO操作
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        try {
            File.WriteAllLines(_filePath, _lines, Encoding.UTF8);
            MainloadTool.Logger.LogInfo($"CSV文件已保存到: {_filePath}");
        } catch (Exception ex) {
            throw new IOException($"保存CSV文件失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取当前已写入的行数（不包括缓冲行）
    /// </summary>
    public int LineCount => _lines.Count;

    /// <summary>
    /// 获取当前缓冲行中的字段数
    /// </summary>
    public int CurrentFieldCount => _currentLine.Count;

    /// <summary>
    /// 获取完整的文件路径
    /// </summary>
    public string FilePath => _filePath;

    /// <summary>
    /// 格式化CSV行，处理包含逗号、引号等特殊字符的情况
    /// </summary>
    private string FormatCsvLine(List<string> fields) {
        var formattedFields = fields.Select(field => {
            if (field == null)
                return string.Empty;

            // 如果字段包含逗号、换行符或引号，需要用引号包围并转义内部引号
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r")) {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        });

        return string.Join(",", formattedFields);
    }
}