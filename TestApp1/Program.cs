using System.Drawing;
using System.Text.Json;
using MITREModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;

using Spectre.Console;
using Color = System.Drawing.Color;

namespace TestApp1;

class Program
{
    private static Dictionary<string, CourseOfAction> CourseOfActions { get; set; } =
        new Dictionary<string, CourseOfAction>();
    private static List<Relationship> Relationships { get; set; } = 
        new List<Relationship>();
    private static Dictionary<string, AttackPattern> AttackPatterns { get; set; } =
        new Dictionary<string, AttackPattern>();
    
    static void Main(string[] args)
    {
        AnsiConsole.MarkupLine("[green]MITRE TestApp1[/]");
        AnsiConsole.MarkupLine("");


        var operation = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select Operation")
                .PageSize(10)
                .AddChoices(new[]
                {
                    "00. Create new MITRE Excel File",
                    "01. Update MITRE Excel File",
                }));

        switch (operation.Substring(0, 2))
        {
            case "00":
                CreateMitreFile();
                break;
            case "01":
                break;
            default:
                break;
        }
    }

    private static void CreateMitreFile()
    {
        ReadStix();

        var p1 = new Platform();
        p1.Name = "Windows";
        p1.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p1.Name)).ToList();
        p1.Systems.Add("Windows 11");
        p1.Systems.Add("Windows Server 2019");
        p1.Systems.Add("Windows Server 2022");
        
        var p2 = new Platform();
        p2.Name = "Linux";
        p2.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p2.Name)).ToList();
        p2.Systems.Add("RHEL 8");
        p2.Systems.Add("RHEL 9");
        
        var p3 = new Platform();
        p3.Name = "Network Devices";
        p3.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p3.Name)).ToList();
        p3.Systems.Add("Netzwerk FITS");
        p3.Systems.Add("Netzwerk CC");
        p3.Systems.Add("Netzwerk Azure");
        p3.Systems.Add("Netzwerk LUX");
        
        var p4 = new Platform();
        p4.Name = "Containers";
        p4.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p4.Name)).ToList();
        p4.Systems.Add("Kubernetes Cluster");

        var p5 = new Platform();
        p5.Name = "IaaS";
        p5.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p5.Name)).ToList();
        p5.Systems.Add("FCPI");
        p5.Systems.Add("Azurblau");
        
        var p6 = new Platform();
        p6.Name = "Identity Provider";
        p6.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p6.Name)).ToList();
        p6.Systems.Add("Entra ID");
        
        var p7 = new Platform();
        p7.Name = "Office Suite";
        p7.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p7.Name)).ToList();
        p7.Systems.Add("M365");
        
        var p8 = new Platform();
        p8.Name = "SaaS";
        p8.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p8.Name)).ToList();
        p8.Systems.Add("M365");
        
        var p9 = new Platform();
        p9.Name = "PRE";
        p9.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p9.Name)).ToList();
        p9.Systems.Add("PRE");
        
        CreateMitreFile(p1, p2, p3, p4, p5, p6, p7, p8, p9);
    }
    
    private static void CreateMitreFile(params Platform[] platforms)
    {
        using (var package = new ExcelPackage())
        {
            foreach (var platform in platforms)
                CreatePlatform(package, platform);
            
            package.SaveAs(new FileInfo("/Users/gengar/Downloads/MITRE.16.1.xlsx"));
        }
    }

    private static void CreatePlatform(ExcelPackage package, Platform platform)
    {
        GenerateGroupGuids(platform.Relationships);
        
        var ws = package.Workbook.Worksheets.Add(platform.Name);
        var rowIndex = 1;
        var rowOffset = 1;
        var colIndex = 1;
        
        var columns = new List<Column>();
        columns.Add(new Column() {ColumnName = "Sort No.", ColumnWidth = 10, WrapText = false, Hidden = false});
        columns.Add(new Column() {ColumnName = "Relationship STIX ID", ColumnWidth = 55, WrapText = false, Hidden = true});
        columns.Add(new Column() {ColumnName = "Mitigation STIX ID", ColumnWidth = 55, WrapText = false, Hidden = true});
        columns.Add(new Column() {ColumnName = "Mitigation ID", ColumnWidth = 15, WrapText = false});
        columns.Add(new Column() {ColumnName = "Mitigation Name", ColumnWidth = 35, WrapText = false});
        columns.Add(new Column() {ColumnName = "Technique STIX ID", ColumnWidth = 55, WrapText = false, Hidden = true});
        columns.Add(new Column() {ColumnName = "Technique ID", ColumnWidth = 15, WrapText = false});
        columns.Add(new Column() {ColumnName = "Description", ColumnWidth = 50, WrapText = true});
        columns.Add(new Column() {ColumnName = "Latest", ColumnWidth = 11, WrapText = false});
        columns.Add(new Column() {ColumnName = "Added At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd"});
        columns.Add(new Column() {ColumnName = "Mitigation Created At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd"});
        columns.Add(new Column() {ColumnName = "Mitigation Modified At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd"});
        columns.Add(new Column() {ColumnName = "Relationship Created At", ColumnWidth = 15, WrapText = false, NumberFormat = "yyyy-mm-dd"});
        columns.Add(new Column() {ColumnName = "Relationship Modified At", ColumnWidth = 15, WrapText = false, NumberFormat = "yyyy-mm-dd"});
        columns.Add(new Column() {ColumnName = "Technique Created At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd"});
        columns.Add(new Column() {ColumnName = "Technique Modified At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd"});
        columns.Add(new Column() {ColumnName = "Status", ColumnWidth = 11, WrapText = false});
        columns.Add(new Column() {ColumnName = "Group Guid", ColumnWidth = 35, WrapText = false, Hidden = true});
        columns.Add(new Column() {ColumnName = "Group Reference", ColumnWidth = 11, WrapText = false, Hidden = true});
        columns.Add(new Column() {ColumnName = "Group Reference ID", ColumnWidth = 55, WrapText = false, Hidden = true});
        
        foreach (var system in platform.Systems)
        {
            columns.Add(new Column() {ColumnName = system, ColumnWidth = 35, WrapText = true, Hidden = false, System = true});
            columns.Add(new Column() {ColumnName = "Score", ColumnWidth = 9, WrapText = false, Hidden = false, System = true});
        }
        
        ws.Cells[1, 1, 1, columns.Count].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[1, 1, 1, columns.Count].Style.Fill.BackgroundColor.SetColor(Color.Gray);
        ws.Cells[1, 1, 1, columns.Count].AutoFilter = true;
        ws.Row(1).Height = 26;
        ws.View.FreezePanes(2, 1);

        foreach (var column in columns)
        {
            ws.Cells[1, colIndex].Value = column.ColumnName;
            ws.Cells[1, colIndex].Style.Font.Bold = true;
            
            ws.Column(colIndex).Width = column.ColumnWidth;
            ws.Column(colIndex).Style.WrapText = column.WrapText;
            ws.Column(colIndex).Hidden = column.Hidden;

            if (!string.IsNullOrEmpty(column.NumberFormat))
                ws.Column(colIndex).Style.Numberformat.Format = column.NumberFormat;

            if (column.System)
            {
                ws.Cells[1, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, colIndex].Style.Fill.BackgroundColor.SetColor(Color.RoyalBlue);
            }
            
            ws.Cells[1, colIndex].Style.WrapText = true;
            
            colIndex += 1;
        }

        var colAlphaGroupRefId = 
            NumberToLetter(
                columns.FindIndex(x => x.ColumnName == "Group Reference ID") + 1);
        var colAlphaRelationshipStixId = 
            NumberToLetter(
                columns.FindIndex(x => x.ColumnName == "Relationship STIX ID") + 1);
        
        foreach (var item in platform.Relationships
                     .OrderBy(x => x.CourseOfActionExternalId)
                     .ThenBy(x => x.AttackPatternExternalId))
        {
            var coa = CourseOfActions[item.SourceRef];
            var ap = AttackPatterns[item.TargetRef];

            colIndex = 1;
            
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = rowIndex;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Id;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Id;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value =
                coa.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Name;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = ap.Id;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = 
                ap.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Description;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = "YES";
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = DateTime.UtcNow;
            
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Created;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Modified;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Created;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Modified;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = ap.Created;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = ap.Modified;
            
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = "CHECK";
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.GroupGuid.ToString();
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.GroupReference;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.GroupReferenceId;

            foreach (var system in platform.Systems)
            {
                if (!item.GroupReference)
                {
                    var colSystem = 
                            columns.FindIndex(x => x.ColumnName == system) + 1;
                    var colSystemScore = colSystem + 1;
                    
                    ws.Cells[rowIndex + rowOffset, colSystem].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex + rowOffset, colSystem].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws.Cells[rowIndex + rowOffset, colSystem].Formula =
                        string.Format("=IF(INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3}))=\"\",\"\"," +
                                      "INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3})))",
                            colAlphaGroupRefId,rowIndex + rowOffset, colAlphaRelationshipStixId, colSystem);
                    
                    ws.Cells[rowIndex + rowOffset, colSystemScore].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex + rowOffset, colSystemScore].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws.Cells[rowIndex + rowOffset, colSystemScore].Formula =
                        string.Format("=IF(INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3}))=\"\",\"\"," +
                                      "INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3})))",
                            colAlphaGroupRefId,rowIndex + rowOffset, colAlphaRelationshipStixId, colSystemScore);
                }
            }
            
            rowIndex += 1;
        }

    }
    
    private static string NumberToLetter(int number)
    {
        if (number < 1 || number > 26)
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 1 and 26.");

        return ((char)(number + 64)).ToString();
    }
    
    private static void ReadStix()
    {
        var stixFilePath = "/Users/gengar/Downloads/enterprise-attack-16.1.json";
        var stixDoc = JsonDocument.Parse(File.ReadAllText(stixFilePath));
        var stixObjects = stixDoc.RootElement.GetProperty("objects");
        
        CourseOfActions = GetCourseOfActions(stixObjects);
        Relationships = GetRelationships(CourseOfActions, stixObjects);
        AttackPatterns = GetAttackPatterns(stixObjects);

        ExtendRelationship();
    }

    private static void GenerateGroupGuids(List<Relationship> relationships)
    {
        var groups = relationships.GroupBy(x => new { x.CourseOfActionExternalId, x.Description });

        foreach (var group in groups)
        {
            var groupGuid = Guid.NewGuid();
            var first = true;
            var groupReferenceId = string.Empty;
            
            foreach (var item in group)
            {
                if (first)
                {
                    item.GroupReference = true;
                    groupReferenceId = item.Id;
                    first = false;
                }
                else
                {
                    item.GroupReferenceId = groupReferenceId;
                }

                item.GroupGuid = groupGuid;
            }
        }
    }

    private static void ExtendRelationship()
    {
        foreach (var item in Relationships)
        {
            var coa = CourseOfActions[item.SourceRef];
            var ap = AttackPatterns[item.TargetRef];
            
            item.CourseOfActionExternalId = 
                coa.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId ?? string.Empty;
            
            item.AttackPatternExternalId = 
                ap.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId ?? string.Empty;;

            item.XMitrePlatforms = ap.XMitrePlatforms;
        }
    }

    private static Dictionary<string, AttackPattern> GetAttackPatterns(JsonElement stixObjects)
    {
        var attackPatterns = new Dictionary<string, AttackPattern>();

        foreach (var item in stixObjects.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString();
            
            if (type == "attack-pattern")
            {
                var attackPattern = item.Deserialize<AttackPattern>() ?? throw new JsonException();

                if (attackPattern.Revoked || attackPattern.XMitreDeprecated)
                    continue;

                attackPatterns.Add(attackPattern.Id, attackPattern);
            }
        }

        return attackPatterns;
    }

    private static List<Relationship> GetRelationships(Dictionary<string, CourseOfAction> courseOfActions, JsonElement stixObjects)
    {
        var relationships = new List<Relationship>();

        foreach (var item in stixObjects.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString();
            
            if (type == "relationship")
            {
                var relationship = item.Deserialize<Relationship>() ?? throw new JsonException();

                if (relationship.Revoked || relationship.XMitreDeprecated)
                    continue;

                if (courseOfActions.ContainsKey(relationship.SourceRef))
                    relationships.Add(relationship);
            }
        }

        return relationships;
    }
    
    private static Dictionary<string, CourseOfAction> GetCourseOfActions(JsonElement stixObjects)
    {
        var courseOfActions = new Dictionary<string, CourseOfAction>();

        foreach (var item in stixObjects.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString();
            
            if (type == "course-of-action")
            {
                var coa = item.Deserialize<CourseOfAction>() ?? throw new JsonException();

                if (coa.Revoked || coa.XMitreDeprecated)
                    continue;

                // if (coa.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId != "M1013")
                //     continue;
                
                courseOfActions.Add(coa.Id, coa);
            }
        }

        return courseOfActions;
    }
}

public struct Column
{
    public string ColumnName;
    public double ColumnWidth;
    public bool WrapText;
    public bool Hidden;
    public string NumberFormat;
    public bool System;
}

public class Platform
{
    public string Name { get; set; }
    public List<string> Systems { get; set; } = new List<string>();
    public List<Relationship> Relationships { get; set; } = new List<Relationship>();
}