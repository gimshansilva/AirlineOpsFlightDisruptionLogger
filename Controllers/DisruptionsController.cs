using System.Data;
using AirlineOpsFlightDisruptionLogger.Models;
using AirlineOpsFlightDisruptionLogger.ViewModels;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace AirlineOpsFlightDisruptionLogger.Controllers;

public class DisruptionsController(IDbConnection db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await BuildPageModelAsync(new DisruptionLogInputViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DisruptionLogInputViewModel form)
    {
        form.FlightNumber = form.FlightNumber.Trim().ToUpperInvariant();
        form.Remarks = string.IsNullOrWhiteSpace(form.Remarks) ? null : form.Remarks.Trim();

        if (!ModelState.IsValid)
        {
            return View(await BuildPageModelAsync(form));
        }

        var disruptionTypeExists = await db.ExecuteScalarAsync<bool>(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.DisruptionTypes WHERE Id = @Id) THEN 1 ELSE 0 END",
            new { Id = form.DisruptionTypeId!.Value });

        if (!disruptionTypeExists)
        {
            ModelState.AddModelError(nameof(form.DisruptionTypeId), "The selected disruption reason is not valid.");
            return View(await BuildPageModelAsync(form));
        }

        var log = new DisruptionLog
        {
            FlightNumber = form.FlightNumber,
            DisruptionTypeId = form.DisruptionTypeId.Value,
            DelayMinutes = form.DelayMinutes,
            RequiresPassengerHotel = form.RequiresPassengerHotel,
            RequiresMealVoucher = form.RequiresMealVoucher,
            Remarks = form.Remarks
        };

        var sql = @"
            INSERT INTO dbo.DisruptionLogs (FlightNumber, DisruptionTypeId, DelayMinutes, RequiresPassengerHotel, RequiresMealVoucher, Remarks)
            VALUES (@FlightNumber, @DisruptionTypeId, @DelayMinutes, @RequiresPassengerHotel, @RequiresMealVoucher, @Remarks);
            SELECT CAST(SCOPE_IDENTITY() AS int);";

        var insertedId = await db.ExecuteScalarAsync<int>(sql, log);
        log.Id = insertedId;

        TempData["SuccessMessage"] = $"Disruption for {log.FlightNumber} was logged successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<FlightDisruptionPageViewModel> BuildPageModelAsync(DisruptionLogInputViewModel form)
    {
        var types = (await db.QueryAsync<DisruptionType>(
            "SELECT Id, Code, Description FROM dbo.DisruptionTypes ORDER BY Description")).ToList();

        var logs = (await db.QueryAsync<DisruptionLog, DisruptionType, DisruptionLog>(
            @"SELECT l.Id, l.FlightNumber, l.DisruptionTypeId, l.DelayMinutes, l.RequiresPassengerHotel,
                     l.RequiresMealVoucher, l.Remarks, l.LoggedAtUtc,
                     t.Id, t.Code, t.Description
              FROM dbo.DisruptionLogs l
              INNER JOIN dbo.DisruptionTypes t ON t.Id = l.DisruptionTypeId
              ORDER BY l.LoggedAtUtc DESC, l.Id DESC",
            (log, type) =>
            {
                log.DisruptionType = type;
                return log;
            },
            splitOn: "Id")).ToList();

        var breakdown = types
            .Select(type => new DisruptionTypeBreakdown
            {
                Code = type.Code,
                Description = type.Description,
                Count = logs.Count(log => log.DisruptionTypeId == type.Id)
            })
            .Where(b => b.Count > 0)
            .OrderByDescending(b => b.Count)
            .ToList();

        var maxCount = breakdown.Count == 0 ? 0 : breakdown.Max(b => b.Count);
        foreach (var b in breakdown)
        {
            b.Percentage = maxCount == 0 ? 0 : Math.Round(b.Count * 100d / maxCount, 0);
        }

        return new FlightDisruptionPageViewModel
        {
            Form = form,
            DisruptionTypes = types,
            Logs = logs,
            TotalDelayMinutes = logs.Sum(log => log.DelayMinutes),
            AverageDelayMinutes = logs.Count == 0 ? 0 : (decimal)Math.Round(logs.Average(log => log.DelayMinutes), 1),
            PassengerCareCount = logs.Count(log => log.RequiresPassengerHotel || log.RequiresMealVoucher),
            HighImpactCount = logs.Count(log => log.DelayMinutes >= 180 || (log.RequiresPassengerHotel && log.RequiresMealVoucher)),
            TopDisruptionDescription = logs
                .GroupBy(log => log.DisruptionType.Description)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .FirstOrDefault() ?? "No data",
            Breakdown = breakdown
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
