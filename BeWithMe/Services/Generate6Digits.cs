namespace BeWithMe.Services
{
    public class Generate6Digits
    {
        public string GenerateSixDigitCode()
        {
            // Use the cryptographically secure random number generator for better security
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                // Create a byte array to hold the random values
                byte[] randomNumber = new byte[4];

                // Fill the array with random values
                rng.GetBytes(randomNumber);

                // Convert the random bytes to an integer and take modulo to get a 6-digit number
                int value = BitConverter.ToInt32(randomNumber, 0) & 0x7FFFFFFF; // Ensure positive number
                value = value % 900000 + 100000; // Ensures a 6-digit number (between 100000 and 999999)

                return value.ToString();
            }
        }
    }
}
