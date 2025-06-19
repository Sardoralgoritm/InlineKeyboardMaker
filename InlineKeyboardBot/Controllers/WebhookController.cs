// Controllers/WebhookController.cs
using InlineKeyboardBot.Configuration;
using InlineKeyboardBot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot.Types;

namespace InlineKeyboardBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IBotService _botService;
        private readonly BotConfiguration _botConfig;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IBotService botService,
            IOptions<BotConfiguration> botConfig,
            ILogger<WebhookController> logger)
        {
            _botService = botService;
            _botConfig = botConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Telegram webhook endpoint
        /// </summary>
        /// <param name="update">Telegram update object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>OK response</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update? update, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("=== WEBHOOK REQUEST RECEIVED ===");

                // Secret token validation (agar mavjud bo'lsa)
                if (!string.IsNullOrEmpty(_botConfig.SecretToken))
                {
                    if (!ValidateSecretToken())
                    {
                        _logger.LogWarning("Invalid secret token in webhook request from IP: {IP}",
                            HttpContext.Connection.RemoteIpAddress);
                        return Ok();
                    }
                }

                // Null check
                if (update == null)
                {
                    _logger.LogWarning("Received null update from Telegram");
                    return Ok();
                }

                _logger.LogInformation("Webhook received update: {UpdateId}, Type: {UpdateType}",
                    update.Id, update.Type);

                // Update'ni bot service'ga yuborish
                await _botService.HandleUpdateAsync(update, cancellationToken);

                return Ok();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Webhook request was cancelled");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook update: {Message}", ex.Message);
                return Ok();
            }
        }

        /// <summary>
        /// Set webhook endpoint (development/testing uchun)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPost("set")]
        public async Task<IActionResult> SetWebhook(CancellationToken cancellationToken)
        {
            try
            {
                await _botService.SetWebhookAsync(cancellationToken);

                _logger.LogInformation("Webhook set successfully via API call");

                return Ok(new
                {
                    success = true,
                    message = "Webhook muvaffaqiyatli o'rnatildi",
                    webhookUrl = $"{_botConfig.WebhookUrl}/api/webhook"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting webhook via API");

                return BadRequest(new
                {
                    success = false,
                    message = "Webhook o'rnatishda xatolik: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Delete webhook endpoint (development/testing uchun)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteWebhook(CancellationToken cancellationToken)
        {
            try
            {
                await _botService.DeleteWebhookAsync(cancellationToken);

                _logger.LogInformation("Webhook deleted successfully via API call");

                return Ok(new
                {
                    success = true,
                    message = "Webhook muvaffaqiyatli o'chirildi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting webhook via API");

                return BadRequest(new
                {
                    success = false,
                    message = "Webhook o'chirishda xatolik: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Get webhook status (development/testing uchun)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Webhook status information</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetWebhookStatus(CancellationToken cancellationToken)
        {
            try
            {
                var isSet = await _botService.IsWebhookSetAsync(cancellationToken);

                return Ok(new
                {
                    isSet = isSet,
                    expectedUrl = $"{_botConfig.WebhookUrl}/api/webhook",
                    message = isSet ? "Webhook o'rnatilgan" : "Webhook o'rnatilmagan"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webhook status");

                return BadRequest(new
                {
                    success = false,
                    message = "Webhook holatini olishda xatolik: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "Telegram Bot Webhook"
            });
        }

        /// <summary>
        /// Secret token validation
        /// </summary>
        /// <returns>True if token is valid</returns>
        private bool ValidateSecretToken()
        {
            try
            {
                // Telegram'dan kelgan secret token header'ini olish
                if (!Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var receivedToken))
                {
                    return false;
                }

                // Constant time comparison (timing attacks'dan himoya)
                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(_botConfig.SecretToken),
                    Encoding.UTF8.GetBytes(receivedToken.ToString())
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating secret token");
                return false;
            }
        }
    }
}