//using HCMPortal.Helper;
//using HCMPortal.Service;
//using HCMPortal.ViewModel;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Razorpay.Api;
//using System.Security.Cryptography;
//using System.Text;

//namespace HCMPortal.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class RazorpayWebhookController : ControllerBase
//    {
//        private readonly IConfiguration _config;
//        private readonly ILogger<RazorpayWebhookController> _logger;
//        private CommonServices commonservice = new CommonServices();
//        public RazorpayWebhookController(IConfiguration config, ILogger<RazorpayWebhookController> logger)
//        {
//            _config = config;
//            _logger = logger;
//        }

//        [HttpPost("webhook")]
//        public async Task<IActionResult> Webhook()
//        {
//            using var reader = new StreamReader(Request.Body);
//            string body = await reader.ReadToEndAsync();
//            try
//            {
//                var razorpaySignature = Request.Headers["X-Razorpay-Signature"].ToString();
//                var eventId = Request.Headers["X-Razorpay-Event-Id"].ToString();
//                if (!IsSignatureValid(body, razorpaySignature))
//                {
//                    _logger.LogWarning("Invalid Razorpay Signature for Event {EventId}", eventId);
//                    return Unauthorized();
//                }
//                var json = JObject.Parse(body);
//                string eventType = json["event"]?.ToString();
//                _logger.LogInformation("Webhook Received [{Id}]: {Event}", eventId, eventType);
//                switch (eventType)
//                {
//                    case "subscription.authenticated":
//                        await HandleSubscriptionStateChange(json, "Authenticated");
//                        break;

//                    case "subscription.activated":
//                    case "subscription.charged":
//                        await HandleSubscriptionCharged(json);
//                        break;

//                    case "subscription.pending":
//                        await HandleSubscriptionStateChange(json, "Pending");
//                        break;

//                    case "subscription.halted":
//                        await HandleSubscriptionStateChange(json, "Halted");
//                        break;

//                    case "subscription.cancelled":
//                        await HandleSubscriptionStateChange(json, "Cancelled");
//                        break;

//                    case "payment.failed":
//                        await HandlePaymentEvent(json, "Failed");
//                        break;

//                    default:
//                        _logger.LogInformation("Ignored Event: {Event}", eventType);
//                        break;
//                }
//                return Ok();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Webhook processing failed");
//                return StatusCode(200);
//            }
//        }
//        private bool IsSignatureValid(string body, string signature)
//        {
//            var secret = _config["Razorpay:WebhookSecret"];
//            if (string.IsNullOrEmpty(secret)) return false;
//            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
//            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
//            var generated = Convert.ToHexString(hash).ToLower();
//            return generated == signature.ToLower();
//        }
//        private async Task HandleSubscriptionStateChange(JObject json, string status)
//        {
//            var subId = json.SelectToken("payload.subscription.entity.id")?.ToString();
//            await SendSubscriptionUpdateToApi(null, subId, status);
//        }
//        //private async Task HandleSubscriptionCharged(JObject json)
//        //{
//        //    var payment = json.SelectToken("payload.payment.entity");
//        //    string paymentId = payment?["id"]?.ToString();
//        //    string subId = payment?["subscription_id"]?.ToString();
//        //    await SendSubscriptionUpdateToApi(paymentId, subId, "Active");
//        //}
//        private async Task HandlePaymentEvent(JObject json, string status)
//        {
//            var payment = json.SelectToken("payload.payment.entity");
//            string paymentId = payment?["id"]?.ToString();
//            string subId = payment?["subscription_id"]?.ToString();
//            await SendSubscriptionUpdateToApi(paymentId, subId, status);
//        }
//        private async Task HandleSubscriptionCharged(JObject json)
//        {
//            try
//            {
//                var payment = json.SelectToken("payload.payment.entity");
//                var subscription = json.SelectToken("payload.subscription.entity");               
//                string paymentId = payment["id"]?.ToString();
//                string subId = payment["subscription_id"]?.ToString();
//                string payStatus = payment["status"]?.ToString();               
//                decimal amount = (payment["amount"]?.ToObject<decimal>() ?? 0) / 100;
//                long paidUnix = payment["created_at"]?.ToObject<long>() ?? 0;
//                DateTime paidAt = DateTimeOffset.FromUnixTimeSeconds(paidUnix).UtcDateTime;
//                int paidCount = subscription["paid_count"]?.ToObject<int>() ?? 0;
//                long startUnix = subscription["current_start"]?.ToObject<long>() ?? 0;
//                long endUnix = subscription["current_end"]?.ToObject<long>() ?? 0;
//                DateTime fromDate = DateTimeOffset.FromUnixTimeSeconds(startUnix).UtcDateTime;
//                DateTime toDate = DateTimeOffset.FromUnixTimeSeconds(endUnix).UtcDateTime;            
//                await SaveCycleHistory(subId, paymentId, amount, paidAt, paidCount, fromDate, toDate, payStatus);              
//                await SendSubscriptionUpdateToApi(paymentId, subId, "Active");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in HandleSubscriptionCharged");
//            }
//        }

//        private async Task SaveCycleHistory(string subId,string paymentId,decimal amount, DateTime paidAt,int cycle,DateTime from,DateTime to,string status)
//        {
//             using var http = new HttpClient();
//           string OrderId = commonservice.GenerateOrderNumber();
//            var model = new SubscriptionCycleHistoryViewModel()
//            {
//                RazorpaySubscriptionId = subId,
//                PaymentId = paymentId,
//                Amount = amount,
//                PaidAt = paidAt,
//                CycleNo = cycle,
//                FromDate = from,
//                ToDate = to,
//                Status = status,
//                OrderID = OrderId
//            };
//            string apiUrl = $"{Common.GetBaseAddress()}CustomerCompanySetup/InsertAutoDebitSubscriptionCycle";
//            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
//            await http.PostAsync(apiUrl, content);
//        }

//        private async Task SendSubscriptionUpdateToApi(string paymentId, string subscriptionId, string status)
//        {
//            if (string.IsNullOrEmpty(subscriptionId)) return;
//            try
//            {
//                RazorpayClient client = new RazorpayClient(_config["Razorpay:Key"], _config["Razorpay:Secret"]);
//                Subscription subscription = client.Subscription.Fetch(subscriptionId);
//                long createdAtUnix = subscription["created_at"]?.ToObject<long>() ?? 0;
//                long? nextDueUnix = subscription["current_end"]?.ToObject<long?>() ?? subscription["start_at"]?.ToObject<long?>();
//                var model = new RazorpayDiscountIdViewModel
//                {
//                    RazorpaySubscriptionId = subscriptionId,
//                    TransactionId = paymentId,
//                    Status = status,
//                    AvailableFromDate = DateTimeOffset.FromUnixTimeSeconds(createdAtUnix).LocalDateTime,
//                    AvailableTo = nextDueUnix.HasValue ? DateTimeOffset.FromUnixTimeSeconds(nextDueUnix.Value).LocalDateTime : DateTime.Now.AddMonths(1)
//                };
//                using var http = new HttpClient();
//                string apiUrl = $"{Common.GetBaseAddress()}SubscriptionMaster/UpdateStatusBasedSID";
//                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
//                var response = await http.PostAsync(apiUrl, content);
//                _logger.LogInformation("Sync to DB: Sub {Id}, Status: {Status}, Created: {Start}, Due: {Due}", subscriptionId, status, model.AvailableFromDate, model.AvailableTo);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to sync Subscription {SubId}", subscriptionId);
//            }
//        }
//    }
//} 

using HCMPortal.Helper;
using HCMPortal.Service;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace HCMPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RazorpayWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<RazorpayWebhookController> _logger;
        private CommonServices commonservice = new CommonServices();

        public RazorpayWebhookController(IConfiguration config, ILogger<RazorpayWebhookController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("HcmportalWebhook")]
        public async Task<IActionResult> HcmportalWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync();
            try
            {
                var razorpaySignature = Request.Headers["X-Razorpay-Signature"].ToString();
                var eventId = Request.Headers["X-Razorpay-Event-Id"].ToString();

                if (!IsSignatureValid(body, razorpaySignature))
                {
                    _logger.LogWarning("Invalid Razorpay Signature for Event {EventId}", eventId);
                    return Unauthorized();
                }

                var json = JObject.Parse(body);
                string eventType = json["event"]?.ToString();
                _logger.LogInformation("Webhook Received [{Id}]: {Event}", eventId, eventType);

                switch (eventType)
                {
                    case "subscription.authenticated":
                        await HandleSubscriptionStateChange(json, "Authenticated");
                        break;

                    case "subscription.activated":
                    case "subscription.charged":
                        await HandleSubscriptionCharged(json);
                        break;

                    case "subscription.pending":
                        await HandleSubscriptionStateChange(json, "Pending");
                        break;

                    case "subscription.halted":
                        await HandleSubscriptionStateChange(json, "Halted");
                        break;

                    case "subscription.cancelled":
                        await HandleSubscriptionStateChange(json, "Cancelled");
                        break;

                    case "payment.failed":
                        await HandlePaymentEvent(json, "Failed");
                        break;

                    default:
                        _logger.LogInformation("Ignored Event: {Event}", eventType);
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed critical error");
                return StatusCode(200); // Razorpay needs 200 even if we fail internally to stop retries
            }
        }

        private bool IsSignatureValid(string body, string signature)
        {
            var secret = _config["Razorpay:WebhookSecret"];
            if (string.IsNullOrEmpty(secret)) return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var generated = Convert.ToHexString(hash).ToLower();
            return generated == signature.ToLower();
        }

        private async Task HandleSubscriptionStateChange(JObject json, string status)
        {
            var subId = json.SelectToken("payload.subscription.entity.id")?.ToString();
            if (!string.IsNullOrEmpty(subId))
            {
                await SendSubscriptionUpdateToApi(null, subId, status);
            }
        }

        private async Task HandlePaymentEvent(JObject json, string status)
        {
            var payment = json.SelectToken("payload.payment.entity");
            string paymentId = payment?["id"]?.ToString();
            string subId = payment?["subscription_id"]?.ToString();
            await SendSubscriptionUpdateToApi(paymentId, subId, status);
        }

        private async Task HandleSubscriptionCharged(JObject json)
        {
            try
            {
                var payload = json["payload"];
                var payment = payload?["payment"]?["entity"];
                var subscription = payload?["subscription"]?["entity"];

                if (payment == null || subscription == null)
                {
                    _logger.LogWarning("Charged event received but entities are missing in payload");
                    return;
                }

                string paymentId = payment["id"]?.ToString();
                string subId = payment["subscription_id"]?.ToString();
                string payStatus = payment["status"]?.ToString();

                // Razorpay sends amount in paise, converting to rupees
                decimal amount = (payment["amount"]?.ToObject<decimal>() ?? 0) / 100;

                long paidUnix = payment["created_at"]?.ToObject<long>() ?? 0;
                DateTime paidAt = DateTimeOffset.FromUnixTimeSeconds(paidUnix).UtcDateTime;

                int paidCount = subscription["paid_count"]?.ToObject<int>() ?? 0;
                long startUnix = subscription["current_start"]?.ToObject<long>() ?? 0;
                long endUnix = subscription["current_end"]?.ToObject<long>() ?? 0;

                DateTime fromDate = DateTimeOffset.FromUnixTimeSeconds(startUnix).UtcDateTime;
                DateTime toDate = DateTimeOffset.FromUnixTimeSeconds(endUnix).UtcDateTime;

                // 1. First Save History
                await SaveCycleHistory(subId, paymentId, amount, paidAt, paidCount, fromDate, toDate, payStatus);

                // 2. Then Update Main Subscription Table to Active
                await SendSubscriptionUpdateToApi(paymentId, subId, "Active");

                _logger.LogInformation("Successfully processed charged event for Sub: {SubId}", subId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleSubscriptionCharged logic");
            }
        }

        private async Task SaveCycleHistory(string subId, string paymentId, decimal amount, DateTime paidAt, int cycle, DateTime from, DateTime to, string status)
        {
            try
            {
                using var http = new HttpClient();
                string OrderId = commonservice.GenerateOrderNumber();
                var model = new SubscriptionCycleHistoryViewModel()
                {
                    RazorpaySubscriptionId = subId,
                    PaymentId = paymentId,
                    Amount = amount,
                    PaidAt = paidAt,
                    CycleNo = cycle,
                    FromDate = from,
                    ToDate = to,
                    Status = status,
                    OrderID = OrderId
                };

                string apiUrl = $"{Common.GetBaseAddress()}CustomerCompanySetup/InsertAutoDebitSubscriptionCycle";
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await http.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Cycle History API failed with status: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save Cycle History");
            }
        }

        private async Task SendSubscriptionUpdateToApi(string paymentId, string subscriptionId, string status)
        {
            if (string.IsNullOrEmpty(subscriptionId)) return;
            try
            {
                RazorpayClient client = new RazorpayClient(_config["Razorpay:Key"], _config["Razorpay:Secret"]);
                Subscription subscription = client.Subscription.Fetch(subscriptionId);

                long createdAtUnix = subscription["created_at"]?.ToObject<long>() ?? 0;
                // Get next due from current_end, if not available use start_at
                long? nextDueUnix = subscription["current_end"]?.ToObject<long?>() ?? subscription["start_at"]?.ToObject<long?>();

                var model = new RazorpayDiscountIdViewModel
                {
                    RazorpaySubscriptionId = subscriptionId,
                    TransactionId = paymentId,
                    Status = status,
                    AvailableFromDate = DateTimeOffset.FromUnixTimeSeconds(createdAtUnix).LocalDateTime,
                    AvailableTo = nextDueUnix.HasValue ? DateTimeOffset.FromUnixTimeSeconds(nextDueUnix.Value).LocalDateTime : DateTime.Now.AddMonths(1)
                };

                using var http = new HttpClient();
                string apiUrl = $"{Common.GetBaseAddress()}SubscriptionMaster/UpdateStatusBasedSID";
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await http.PostAsync(apiUrl, content);

                _logger.LogInformation("Sync to DB Complete: Sub {Id}, Status: {Status}, Due: {Due}", subscriptionId, status, model.AvailableTo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync main Subscription status for {SubId}", subscriptionId);
            }
        }
    }
}