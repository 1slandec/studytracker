using System.IO.Compression;
using System.Security;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Data;
using StudyTracker.Models;
using StudyTracker.Services.Exceptions;

namespace StudyTracker.Services;

public class ReportService : IReportService
{
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly ApplicationDbContext _context;
    private readonly ICourseService _courseService;
    private readonly UserManager<User> _userManager;

    public ReportService(
        ApplicationDbContext context,
        ICourseService courseService,
        UserManager<User> userManager)
    {
        _context = context;
        _courseService = courseService;
        _userManager = userManager;
    }

    public async Task<ReportFile> BuildStudentTasksReportAsync(int courseId, ClaimsPrincipal user, string format)
    {
        var course = await _courseService.GetCourseForUserAsync(courseId, user);
        var userId = _userManager.GetUserId(user)
            ?? throw new EntityNotFoundException("Пользователь не найден.");

        var effectiveStatuses = course.Tasks.ToDictionary(task => task.Id, task => task.Status);

        if (user.IsInRole(RoleNames.Student))
        {
            var studentStatuses = await _context.StudentTaskStatuses
                .Where(status => status.StudentId == userId && status.StudyTask!.CourseId == courseId)
                .ToListAsync();

            foreach (var status in studentStatuses)
            {
                effectiveStatuses[status.StudyTaskId] = status.Status;
            }
        }

        var rows = course.Tasks
            .OrderBy(task => task.Deadline)
            .Select(task =>
            {
                var status = effectiveStatuses.TryGetValue(task.Id, out var value) ? value : task.Status;
                return new[]
                {
                    task.Title,
                    task.Description,
                    task.Deadline.ToString("dd.MM.yyyy"),
                    status.ToDisplayName(),
                    task.Deadline.Date < DateTime.Today && status != TaskStatusType.Completed ? "Да" : "Нет"
                };
            })
            .ToList();

        var title = $"Задания по курсу: {course.Name}";
        var headers = new[] { "Задание", "Описание", "Дедлайн", "Статус", "Просрочено" };
        var normalizedFormat = NormalizeFormat(format);
        var fileBaseName = $"course-{course.Id}-tasks";

        return normalizedFormat == "docx"
            ? new ReportFile($"{fileBaseName}.docx", DocxContentType, CreateDocx(title, headers, rows))
            : new ReportFile($"{fileBaseName}.xlsx", XlsxContentType, CreateXlsx(title, headers, rows));
    }

    public async Task<ReportFile> BuildOverdueStudentsReportAsync(string format)
    {
        var assignments = await _context.StudentCourses
            .Include(link => link.Student)
            .Include(link => link.Course!)
                .ThenInclude(course => course.Tasks)
            .ToListAsync();

        var statuses = await _context.StudentTaskStatuses.ToListAsync();
        var statusLookup = statuses.ToDictionary(status => (status.StudentId, status.StudyTaskId), status => status.Status);

        var rows = new List<string[]>();

        foreach (var assignment in assignments)
        {
            if (assignment.Student is null || assignment.Course is null)
            {
                continue;
            }

            foreach (var task in assignment.Course.Tasks)
            {
                var status = statusLookup.TryGetValue((assignment.StudentId, task.Id), out var studentStatus)
                    ? studentStatus
                    : task.Status;

                if (task.Deadline.Date >= DateTime.Today || status == TaskStatusType.Completed)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    assignment.Student.FullName,
                    assignment.Student.Email ?? string.Empty,
                    assignment.Course.Name,
                    task.Title,
                    task.Deadline.ToString("dd.MM.yyyy"),
                    status.ToDisplayName()
                });
            }
        }

        rows = rows
            .OrderBy(row => row[0])
            .ThenBy(row => row[2])
            .ThenBy(row => row[4])
            .ToList();

        var title = "Студенты, просрочившие дедлайн";
        var headers = new[] { "Студент", "Email", "Курс", "Задание", "Дедлайн", "Статус" };
        var normalizedFormat = NormalizeFormat(format);

        return normalizedFormat == "docx"
            ? new ReportFile("overdue-students.docx", DocxContentType, CreateDocx(title, headers, rows))
            : new ReportFile("overdue-students.xlsx", XlsxContentType, CreateXlsx(title, headers, rows));
    }

    private static string NormalizeFormat(string format)
    {
        var normalized = format.Trim().ToLowerInvariant();

        if (normalized is not ("docx" or "xlsx"))
        {
            throw new FormValidationException("Поддерживаются только форматы docx и xlsx.");
        }

        return normalized;
    }

    private static byte[] CreateDocx(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/></Types>""");

            AddEntry(archive, "_rels/.rels",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/></Relationships>""");

            AddEntry(archive, "word/document.xml", BuildWordDocumentXml(title, headers, rows));
        }

        return stream.ToArray();
    }

    private static byte[] CreateXlsx(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>""");

            AddEntry(archive, "_rels/.rels",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");

            AddEntry(archive, "xl/workbook.xml",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Отчет" sheetId="1" r:id="rId1"/></sheets></workbook>""");

            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/></Relationships>""");

            AddEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(title, headers, rows));
        }

        return stream.ToArray();
    }

    private static string BuildWordDocumentXml(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body>""");
        builder.Append(BuildParagraph(title, bold: true));
        builder.Append("""<w:tbl><w:tblPr><w:tblBorders><w:top w:val="single" w:sz="4"/><w:left w:val="single" w:sz="4"/><w:bottom w:val="single" w:sz="4"/><w:right w:val="single" w:sz="4"/><w:insideH w:val="single" w:sz="4"/><w:insideV w:val="single" w:sz="4"/></w:tblBorders></w:tblPr>""");
        builder.Append(BuildWordRow(headers, true));

        foreach (var row in rows.DefaultIfEmpty(new[] { "Нет данных" }))
        {
            builder.Append(BuildWordRow(row, false));
        }

        builder.Append("""</w:tbl><w:sectPr><w:pgSz w:w="16838" w:h="11906" w:orient="landscape"/><w:pgMar w:top="720" w:right="720" w:bottom="720" w:left="720"/></w:sectPr></w:body></w:document>""");
        return builder.ToString();
    }

    private static string BuildParagraph(string text, bool bold)
    {
        var boldRun = bold ? "<w:rPr><w:b/></w:rPr>" : string.Empty;
        return $"""<w:p><w:r>{boldRun}<w:t>{XmlEscape(text)}</w:t></w:r></w:p>""";
    }

    private static string BuildWordRow(IEnumerable<string> cells, bool bold)
    {
        var builder = new StringBuilder("<w:tr>");
        foreach (var cell in cells)
        {
            var boldRun = bold ? "<w:rPr><w:b/></w:rPr>" : string.Empty;
            builder.Append($"""<w:tc><w:tcPr><w:tcW w:w="2400" w:type="dxa"/></w:tcPr><w:p><w:r>{boldRun}<w:t>{XmlEscape(cell)}</w:t></w:r></w:p></w:tc>""");
        }

        builder.Append("</w:tr>");
        return builder.ToString();
    }

    private static string BuildWorksheetXml(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        var allRows = new List<IReadOnlyList<string>>
        {
            new[] { title },
            headers
        };

        allRows.AddRange(rows.Count == 0 ? new[] { new[] { "Нет данных" } } : rows);

        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>""");

        for (var rowIndex = 0; rowIndex < allRows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 1;
            builder.Append($"""<row r="{rowNumber}">""");

            for (var columnIndex = 0; columnIndex < allRows[rowIndex].Count; columnIndex++)
            {
                var cellReference = $"{GetColumnName(columnIndex + 1)}{rowNumber}";
                builder.Append($"""<c r="{cellReference}" t="inlineStr"><is><t>{XmlEscape(allRows[rowIndex][columnIndex])}</t></is></c>""");
            }

            builder.Append("</row>");
        }

        builder.Append("</sheetData></worksheet>");
        return builder.ToString();
    }

    private static string GetColumnName(int columnNumber)
    {
        var dividend = columnNumber;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static void AddEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string XmlEscape(string value)
    {
        return SecurityElement.Escape(value) ?? string.Empty;
    }
}
