using HCMPortal.Models;
using HCMPortal.Models;
using HCMPortal.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HCMPortal.Service
{

    public class EmailService:IEmailService
    {
        private ResponseModel? ResponseModel;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        public EmailService(HttpClient httpClient, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_configuration["MailTrap:BaseUrl"]);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Api-Token", _configuration["MailTrap:ApiToken"]);
        }
        public async Task<ResponseModel> SendEmailAsync(string toEmail, string toName, string subject, string textContent, string htmlContent = null)
        {
            var responseModel = new ResponseModel();

            try
            {
                var emailRequest = new
                {
                    to = new[] { new { email = toEmail, name = toName } },
                    from = new { email = _configuration["MailTrap:FromEmail"], name = _configuration["MailTrap:FromName"] },
                    subject = subject,
                    text = textContent,
                    html = htmlContent,
                    category = "API Test"
                };

                var jsonContent = JsonSerializer.Serialize(emailRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/send", content);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Email sent successfully. Response: {Response}", responseData);

                responseModel.IsSuccess = true;
                responseModel.Message = "Email sent successfully";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
                responseModel.IsSuccess = false;
                responseModel.Message = $"Error sending email: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {ToEmail}", toEmail);
                responseModel.IsSuccess = false;
                responseModel.Message = $"Unexpected error: {ex.Message}";
            }

            return responseModel;
        }
    }
}
