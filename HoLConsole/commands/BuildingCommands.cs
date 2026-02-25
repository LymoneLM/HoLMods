using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HoLConsole;

public static class BuildingCommands
{
    public static void Register()
    {
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "check-BuildingCollision",
            description: "[调试指令]依次加载已记录的所有建筑，统计其碰撞体积",
            usage: "check-BuildingCollision",
            handler: CalculateBuildingCollisions
        ));
    }
    
    public static string CalculateBuildingCollisions(CommandContext ctx, ParsedArgs _)
    {
        try
        {
            var records = new List<DragRecord>();
            var buildCount = Mainload.AllBuilddata.Count;
            ctx.Print($"{buildCount} buildings found", ConsoleLevel.Info);

            for (var buildID = 0; buildID < buildCount; buildID++)
            {
                for (var state = 0; state < 4; state++)
                {
                    var obj = Resources.Load<GameObject>($"allbuild/0/scene/{buildID}/{state}");
                    if (obj == null)
                        break;
                    
                    var allDrag = obj.transform.Find("UI/AllDrag");
                    var positions = new List<Vector2Int>();
                    
                    for (var i = allDrag.childCount - 1; i >= 0; i--)
                    {
                        var child = allDrag.GetChild(i);
                        if(ParsePosition(child.name, out var pos))
                            positions.Add(pos);
                        else
                            ctx.Logger.LogWarning($"{buildID}/{state}: Invalid position format '{child.name}'");
                    }

                    if (positions.Count == 0)
                        ctx.Logger.LogWarning($"{buildID}/{state}: No valid positions found");
                    
                    var minPos = positions[0];
                    var maxPos = positions[0];
                
                    foreach (var pos in positions)
                    {
                        if (pos.x < minPos.x || (pos.x == minPos.x && pos.y < minPos.y))
                            minPos = pos;
                        if (pos.x > maxPos.x || (pos.x == maxPos.x && pos.y > maxPos.y))
                            maxPos = pos;
                    }
                    
                    var record = new DragRecord
                    {
                        BuildClassID = buildID,
                        BuildStateID = state,
                        MinPositionA = minPos.x,
                        MinPositionB = minPos.y,
                        MaxPositionA = maxPos.x,
                        MaxPositionB = maxPos.y,
                        TotalCollisions = positions.Count
                    };
                    records.Add(record);
                }
            }

            SaveToCSV(records);
            return "Done, csv saved";
        }
        catch (Exception ex)
        {
            ctx.Logger.LogError($"Error: {ex.Message}");
            return $"Error：{ex.Message}";
        }
    }

    /// <summary>
    /// 解析坐标字符串，格式为"A|B"
    /// </summary>
    private static bool ParsePosition(string positionStr, out Vector2Int position)
    {
        position = Vector2Int.zero;
        
        if (string.IsNullOrEmpty(positionStr))
            return false;
            
        var parts = positionStr.Split('|');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0].Trim(), out var a) || !int.TryParse(parts[1].Trim(), out var b)) 
            return false;
        
        position = new Vector2Int(a, b);
        return true;
    }
    
    private static void SaveToCSV(List<DragRecord> records)
    {
        var fileName = $"Output/AllBuildingCollision{"_v" + Mainload.Vision_now.Substring(2)}.csv";
        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("buildClassID,buildStateID,minA,minB,maxA,maxB,totalCoordinates");
        
        // 数据行
        foreach (var record in records)
        {
            csvBuilder.AppendLine(
                $"{record.BuildClassID},{record.BuildStateID},{record.MinPositionA},{record.MinPositionB},{record.MaxPositionA},{record.MaxPositionB},{record.TotalCollisions}");
        }
        
        System.IO.File.WriteAllText(fileName, csvBuilder.ToString());
    }
    
    private class DragRecord
    {
        public int BuildClassID { get; set; }
        public int BuildStateID { get; set; }
        public int MinPositionA { get; set; }
        public int MinPositionB { get; set; }
        public int MaxPositionA { get; set; }
        public int MaxPositionB { get; set; }
        public int TotalCollisions { get; set; }
    }
}
