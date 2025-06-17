using BeWithMe.DTOs;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
namespace BeWithMe.Services
{
    #region Twilio
    //public class GenerateAgoraCrediatilsService
    //{
    //    private readonly IConfiguration config;


    //    public GenerateAgoraCrediatilsService(IConfiguration config)
    //    {
    //        this.config = config;
    //    }
    //    public   CallDetails GenerateVideoCallCredinatils(int postId)
    //    {
    //        // Install Twilio NuGet package
    //        var twilioAccountSid = config["Twilio:TWILIO_ACCOUNT_SID"];
    //        var twilioApiKey = config["Twilio:TWILIO_API_KEY"];
    //        var twilioApiSecret = config["Twilio:TWILIO_API_SECRET"];
    //        var roomName = $"room-{postId}";

    //        var identity = $"user-{Guid.NewGuid()}"; // Unique identifier for the call
    //        var grant = new VideoGrant { Room = roomName };
    //        var token = new Token(twilioAccountSid, twilioApiKey, twilioApiSecret, identity, grants: new HashSet<IGrant> { grant });


    //        return new CallDetails
    //        {
    //            RoomName =roomName,
    //            AccessToken = token.ToJwt()
    //        };


    //    }
    //}
    #endregion
    public class GenerateAgoraCredentialsService
    {
        private readonly string _appId;
        private readonly string _appCertificate;

        public GenerateAgoraCredentialsService(IConfiguration config)
        {
            _appId = config["Agora:AppId"];
            _appCertificate = config["Agora:AppCertificate"];
        }

        public CallDetails GenerateAgoraCredentials(int postId, string userId)
        {
            string channelName = $"room-{postId}";
            string token = GenerateToken(_appId,_appCertificate, channelName, userId,3600,TokenType.Rtc);
            return new CallDetails
            {
                AppId = _appId,
                Token = token,
                ChannelName = channelName
            };
        }

        //private string GenerateToken(string channelName, string userId)
        //{
        //    // Placeholder for Agora token generation
        //    // Use Agora's server-side token generation library or API
        //    // Example: https://github.com/AgoraIO/Tools/tree/master/DynamicKey/AgoraDynamicKey/csharp
        //    // This requires the Agora AccessToken NuGet package or custom implementation
        //    return "agora-generated-token"; // Replace with actual token generation
        //}

   
        public enum TokenType
        {
            Rtc,
            Rtm
        }
        public static string GenerateToken(
               string appId,
               string appCertificate,
               string channelName,
               string userId,
               int expirationInSeconds,
               TokenType tokenType)
        {
            // Calculate expiration time
            var expiration = (uint)DateTimeOffset.UtcNow.AddSeconds(expirationInSeconds).ToUnixTimeSeconds();

            // Build token components
            

            // Generate signature
            var signature = GenerateSignature(appId,appCertificate,channelName,userId,expiration,tokenType);

            // Build final token
            return signature;
        }

        private static string GenerateSignature(string AppId, string AppCertificate, string ChannelName, string Uid, uint ExpireTime, TokenType type)
        {
            var signContent = type switch
            {
                TokenType.Rtc => $"{AppId}{ChannelName}{Uid}{ExpireTime}",
                //TokenType.Rtm => $"{AppId}{Uid}{ExpireTime}",
                _ => throw new ArgumentException("Invalid token type")
            };

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(AppCertificate));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signContent));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

       
    }
}
