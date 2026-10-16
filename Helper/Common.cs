using HCMPortal.Models;
using System.Net.Http.Headers;

namespace HCMPortal.Helper
{
    public static class Common
    {

        public static string _merchantId = "T_03342";
        public static string MerchantSecretKey = "abc";
        private static IConfiguration? _configuration;

        //public static void InitConfigure(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        private static IHttpContextAccessor _httpContextAccessor;
        private static IServiceProvider? _serviceProvider; // New field

        public static void InitConfigure(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
                                            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider; // Store the provider
        }

        #region local IIS server
        //public static string GetBaseAddress()
        //{
        //    return "http://192.168.0.116:222/api/";
        //}
        //public static string GetLandingPageAddress()
        //{
        //    return "https://192.168.0.116:225/Home/Index";
        //}

        //public static string PaymentReturnUrl()
        //{
        //    return "http://192.168.0.116:224/Order/PaymentResponse";
        //}
        //public static string PaymentReturnUrlForAddonService()
        //{
        //    return "http://192.168.0.116:224/Order/HandleAddOnServicePaymentResponse";
        //}
        //public static string ImageAndDocSavePageAddress()
        //{
        //    return "http://192.168.0.116:224/";
        //}
        //public static string GetUrlImageAndDocAddress()
        //{
        //    return "http://192.168.0.116:224/";
        //}
        //public static string GetUrlForOwnBaseAddress()
        //{
        //    return "http://192.168.0.116:225/";
        //}
        #endregion

        #region Local System
        //public static string GetBaseAddress()
        //{
        //    return "https://localhost:7266/api/";
        //}
        //public static string GetLandingPageAddress()
        //{
        //    return "http://localhost:5251/Home/Index";
        //}

        //public static string PaymentReturnUrl()
        //{
        //    return "http://localhost:5192/Order/PaymentResponse";
        //}
        //public static string PaymentReturnUrlForAddonService()
        //{
        //    return "http://localhost:5192/Order/HandleAddOnServicePaymentResponse";
        //}
        //public static string ImageAndDocSavePageAddress()
        //{
        //    return "https://localhost:7285/";
        //}

        //public static string GetUrlImageAndDocAddress()
        //{
        //    return "https://localhost:7285/";
        //}
        //public static string GetUrlForOwnBaseAddress()
        //{
        //    return "http://localhost:5192/";
        //}
        #endregion



        #region Testing Server
        //public static string GetBaseAddress()
        //{
        //    return "https://hcmapiv2.anantatek.com/api/";
        //}
        //public static string GetLandingPageAddress()
        //{
        //    return "https://hcmv2.anantatek.com/Home/Index";
        //}

        //public static string PaymentReturnUrl()
        //{
        //    return "https://hcmportal.anantatek.com/Order/PaymentResponse";
        //}
        //public static string PaymentReturnUrlForAddonService()
        //{
        //    return "https://hcmportal.anantatek.com/Order/HandleAddOnServicePaymentResponse";
        //}
        //public static string ImageAndDocSavePageAddress()
        //{
        //    return "https://admin.anantatek.com/";
        //}

        //public static string GetUrlImageAndDocAddress()
        //{
        //    return "https://admin.anantatek.com/";
        //}
        //public static string GetUrlForOwnBaseAddress()
        //{
        //    return "https://hcmportal.anantatek.com/";
        //}
        #endregion


        #region production server-cloudtree application
        //public static string GetBaseAddress()
        //{
        //    return "https://hcmapi.anantagroup.in.net/api/";
        //}
        //public static string GetLandingPageAddress()
        //{
        //    return "https://hcm.anantagroup.in.net/Home/Index";
        //}

        //public static string PaymentReturnUrl()
        //{
        //    return "https://hcmportal.anantatek.com/Order/PaymentResponse";

        //}
        //public static string PaymentReturnUrlForAddonService()
        //{
        //    return "https://hcmportal.anantatek.com/Order/HandleAddOnServicePaymentResponse";
        //}
        //public static string ImageAndDocSavePageAddress()
        //{
        //    return "https://localhost:7285/";
        //}

        //public static string GetUrlImageAndDocAddress()
        //{
        //    return "https://admin.anantatek.com/";
        //}
        //public static string GetUrlForOwnBaseAddress()
        //{
        //    return "https://hcmportal.anantatek.com/";
        //}
        #endregion

        #region Live server
        public static string GetBaseAddress()
        {
            return "https://hcapi.thecloudtree.in/api/";
        }
        public static string GetLandingPageAddress()
        {
            return "https://thecloudtree.in/Home/Index";
        }

        public static string PaymentReturnUrl()
        {
            return "https://thecloudtree.ai/Order/PaymentResponse";
        }
        public static string PaymentReturnUrlForAddonService()
        {
            return "https://thecloudtree.ai/Order/HandleAddOnServicePaymentResponse";
        }
        public static string ImageAndDocSavePageAddress()
        {
            return "https://cloudadmin.thecloudtree.in/";
        }

        public static string GetUrlImageAndDocAddress()
        {
            return "https://cloudadmin.thecloudtree.in/";
        }
        public static string GetUrlForOwnBaseAddress()
        {
            return "https://thecloudtree.ai/";
        }
        #endregion

        public static string CalculatePayPhiSecureHash(PayPhiPaymentRequest request)
        {
            var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "addlParam1", request.addlParam1 },
            { "addlParam2", request.addlParam2 },
            { "amount", request.amount },
            { "currencyCode", request.currencyCode },
            { "customerEmailID", request.customerEmailID },
            { "customerMobileNo", request.customerMobileNo },
            { "merchantId", request.merchantId },
            { "merchantTxnNo", request.merchantTxnNo },
            { "payType", request.payType },
            { "returnURL", request.returnURL },
             { "paymentMode", request.paymentMode },
            { "transactionType", request.transactionType },
            { "txnDate", request.txnDate },
        };

            var dataToHashBuilder = new System.Text.StringBuilder();
            foreach (var pair in parameters)
            {
                dataToHashBuilder.Append(pair.Value);
            }
            string dataToHash = dataToHashBuilder.ToString();

            var keyBytes = System.Text.Encoding.UTF8.GetBytes(MerchantSecretKey);
            var messageBytes = System.Text.Encoding.ASCII.GetBytes(dataToHash);

            byte[] hashBytes;
            using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyBytes))
            {
                hashBytes = hmacsha256.ComputeHash(messageBytes);
            }

            var hashBuilder = new System.Text.StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashBuilder.Append(b.ToString("x2"));
            }

            return hashBuilder.ToString();
        }
        

        public static HttpClient GetHttpClient()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Common class is not initialized. Call Common.InitConfigure() during application startup.");
            }

            // 1. Base network handler
            var baseNetworkHandler = new HttpClientHandler();

            // 2. Attach ServiceUnavailableHandler directly to the base handler
            var serviceUnavailableHandler = new ServiceUnavailableHandler(_httpContextAccessor!)
            {
                InnerHandler = baseNetworkHandler
            };

            // 3. Create HttpClient using ServiceUnavailableHandler
            var client = new HttpClient(serviceUnavailableHandler)
            {
                Timeout = TimeSpan.FromMinutes(30)
            };

            // 4. Header and Authentication Logic
            if (_configuration != null)
            {
                string context = "UnknownContext";
                if (_httpContextAccessor!.HttpContext != null &&
                    _httpContextAccessor.HttpContext.Items.TryGetValue("FrontendActionContext", out object? contextObj) &&
                    contextObj is string contextString)
                {
                    context = contextString;
                }

                client.DefaultRequestHeaders.Add("X-Frontend-Context", context);

                var token1 = _configuration["JwtSettings:StaticToken1"];
                token1 = APIEncryptionHelper.Encrypt(token1 ?? string.Empty);

                string userId = _httpContextAccessor!.HttpContext?.Session.GetString("CompanyUserId") ?? "NA";

                if (!string.IsNullOrEmpty(token1))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token1}");
                    client.DefaultRequestHeaders.Add("UserId", userId);
                }
            }

            return client;
            //HttpClient client = new HttpClient();
            //client.Timeout = TimeSpan.FromMinutes(30);

            //if (_configuration != null)
            //{
            //    var token1 = _configuration["JwtSettings:StaticToken"];
            //    //var userId = _configuration["JwtSettings:userId"];

            //    token1 = APIEncryptionHelper.Encrypt(token1 ?? string.Empty);
            //    string userId = "CLOUDTREE_ADMIN";

            //    if (!string.IsNullOrEmpty(token1))
            //    {
            //        client.DefaultRequestHeaders.Authorization =
            //            new AuthenticationHeaderValue("Bearer", $"{token1}");
            //        client.DefaultRequestHeaders.Add("UserId", userId);
            //    }
            //}


            //return client;
        }
    }
}
