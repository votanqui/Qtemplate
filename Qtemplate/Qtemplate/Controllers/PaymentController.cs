using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Constants;
using Qtemplate.Application.DTOs.payments;
using Qtemplate.Application.Features.Payments.Commands.SepayCallback;
using Qtemplate.Domain.Interfaces.Repositories;
using System.Text.RegularExpressions;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISettingRepository _settingRepo;

    public PaymentController(IMediator mediator, ISettingRepository settingRepo)
    {
        _mediator = mediator;
        _settingRepo = settingRepo;
    }

    private string GetIp() =>
        Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? HttpContext.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    /// <summary>
    /// POST /api/payments/sepay-callback
    /// SePay bắn webhook sau mỗi giao dịch
    /// Header: Authorization: Apikey YOUR_API_KEY
    /// Body theo đúng format SePay docs
    /// </summary>
    [HttpPost("sepay-callback")]
    public async Task<IActionResult> SepayCallback([FromBody] SepayWebhookRequest body)
    {
        // 1. Validate API Key từ DB Settings
        var expectedKey = await _settingRepo.GetValueAsync(SettingKeys.SepayApiKey) ?? "";
        if (!string.IsNullOrEmpty(expectedKey))
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault() ?? "";
            var match = Regex.Match(authHeader, @"^Apikey\s+(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success || match.Groups[1].Value.Trim() != expectedKey)
                return Ok(new { success = false, message = "Invalid API Key" });
        }

        // 2. Validate basic
        if (body is null || string.IsNullOrEmpty(body.Content) || body.TransferAmount <= 0)
            return Ok(new { success = false, message = "Invalid webhook data" });

        // 3. Xử lý
        var result = await _mediator.Send(new SepayCallbackCommand
        {
            SepayId = body.Id,
            Gateway = body.Gateway,
            TransactionDate = body.TransactionDate,
            AccountNumber = body.AccountNumber,
            Content = body.Content,
            TransferType = body.TransferType,
            TransferAmount = body.TransferAmount,
            ReferenceCode = body.ReferenceCode,
            RawCallback = System.Text.Json.JsonSerializer.Serialize(body)
        });

        // Luôn trả 200 để SePay không retry
        return Ok(new { success = result.Success, message = result.Message });
    }
}