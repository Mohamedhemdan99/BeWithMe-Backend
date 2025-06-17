//using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.IO.Hashing;
using static BeWithMe.Services.GenerateAgoraCredentialsService;

namespace BeWithMe.Services
{
    public class GenerateAgoraToken
    {
        public string BuildToken(
            string appId,
            string appCertificate,
            string channelName,
            uint uid,
            TokenType role,
            uint expireTimestamp)
        {
            // 1. CRC32 hash of channelName
            byte[] crcChannel = ComputeCrc32(channelName);


            // 2. Build message
            var message = new List<byte>();
            message.AddRange(Encoding.UTF8.GetBytes(appId));
            message.AddRange(crcChannel);
            message.AddRange(BitConverter.GetBytes(uid));
            message.AddRange(BitConverter.GetBytes(expireTimestamp));
            message.AddRange(new byte[] {
            (byte)(role == TokenType.Rtc ? 1 : 0), 0, 0, 0, 0});

            // 3. Sign with HMAC-SHA256
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate));
            var signature = hmac.ComputeHash(message.ToArray());

            // 4. Build final token
            var tokenBuilder = new List<byte>();
            tokenBuilder.Add(0x01); // Version
            tokenBuilder.Add(0x00); // Service type: RTC
            tokenBuilder.AddRange(BitConverter.GetBytes((ushort)signature.Length));
            tokenBuilder.AddRange(signature);
            tokenBuilder.AddRange(BitConverter.GetBytes(expireTimestamp));
            tokenBuilder.AddRange(new byte[] { 1, 0, 0, 0 }); // Privilege JoinChannel
            tokenBuilder.AddRange(new byte[] { 2, 0, 0, 0 }); // Privilege PublishAudioStream
            tokenBuilder.AddRange(new byte[] { 3, 0, 0, 0 }); // Privilege PublishVideoStream
            tokenBuilder.AddRange(new byte[] { 4, 0, 0, 0 }); // Privilege PublishDataStream

            return "007" + Convert.ToBase64String(tokenBuilder.ToArray());
        }
        private static byte[] ComputeCrc32(string input)
        {
            var crc = new Crc32();
            var data = Encoding.UTF8.GetBytes(input);
            crc.Append(data);
            return crc.GetCurrentHash();
        }
    }

    public class Crc32
    {
        private const uint s_generator = 0xEDB88320;
        private uint _hash;

        public Crc32() => _hash = 0xFFFFFFFF;

        public void Append(byte[] byteArray)
        {
            for (int i = 0; i < byteArray.Length; i++)
            {
                byte currentByte = byteArray[i];
                _hash ^= (uint)currentByte;

                for (int j = 0; j < 8; j++)
                {
                    if ((_hash & 0x1) == 1)
                        _hash = (_hash >> 1) ^ s_generator;
                    else
                        _hash >>= 1;
                }
            }
        }

        public byte[] GetCurrentHash()
        {
            byte[] hashBytes = BitConverter.GetBytes(~_hash);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(hashBytes);
            return hashBytes;
        }
    }
    }
