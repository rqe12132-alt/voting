using ClosedXML.Excel;
using VotingApp.DTOs.Vote;
using VotingApp.Repositories;
using VotingApp.Services;

namespace VotingApp.Services;

public interface IExcelExportService
{
    Task<byte[]> ExportPollResultsAsync(Guid pollId);
}

public class ExcelExportService : IExcelExportService
{
    private readonly IPollRepository _pollRepository;
    private readonly IVoteRepository _voteRepository;

    public ExcelExportService(IPollRepository pollRepository, IVoteRepository voteRepository)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
    }

    public async Task<byte[]> ExportPollResultsAsync(Guid pollId)
    {
        var poll = await _pollRepository.GetByIdWithDetailsAsync(pollId);
        if (poll == null) throw new Exception("Poll not found");

        var results = await _voteRepository.GetResultsAsync(pollId);
        var totalVotes = results.Values.Sum();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Results");

        // Header
        worksheet.Cell(1, 1).Value = "Название голосования";
        worksheet.Cell(1, 2).Value = poll.Title;
        worksheet.Cell(2, 1).Value = "Тип";
        worksheet.Cell(2, 2).Value = poll.Type.ToString();
        worksheet.Cell(3, 1).Value = "Статус";
        worksheet.Cell(3, 2).Value = poll.Status.ToString();
        worksheet.Cell(4, 1).Value = "Всего голосов";
        worksheet.Cell(4, 2).Value = totalVotes;
        worksheet.Cell(5, 1).Value = "Дата создания";
        worksheet.Cell(5, 2).Value = poll.CreatedAt.ToString("dd.MM.yyyy HH:mm");

        // Empty row
        worksheet.Cell(7, 1).Value = "Вариант";
        worksheet.Cell(7, 2).Value = "Количество голосов";
        worksheet.Cell(7, 3).Value = "Процент";

        // Style header
        var headerRange = worksheet.Range("A7:C7");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 8;
        foreach (var option in poll.Options.OrderBy(o => o.SortOrder))
        {
            var count = results.ContainsKey(option.Id) ? results[option.Id] : 0;
            var pct = totalVotes > 0 ? Math.Round((double)count / totalVotes * 100, 2) : 0;

            worksheet.Cell(row, 1).Value = option.Text;
            worksheet.Cell(row, 2).Value = count;
            worksheet.Cell(row, 3).Value = pct;

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
