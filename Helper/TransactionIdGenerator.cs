using System.Security.Cryptography;

namespace HCMPortal.Helper
{
    public static class TransactionIdGenerator
    {
        private const string Digits = "0123456789";
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int TotalLength = 20;
        private const int HalfLength = 10; // 10 digits and 10 letters

        /// <summary>
        /// Generates a 20-character unique transaction number 
        /// consisting of 10 random digits and 10 random uppercase letters.
        /// </summary>
        /// <returns>A 20-character alphanumeric string.</returns>
        public static string GenerateRandomTxnNo()
        {
            // 1. Generate the required 10 digits and 10 letters
            string digits = GenerateRandomComponent(Digits, HalfLength);
            string letters = GenerateRandomComponent(Letters, HalfLength);

            // 2. Combine the components (e.g., "1234567890ABCDEFGHIJ")
            string combined = digits + letters;

            // 3. Shuffle the combined string for a random mix
            return ShuffleString(combined);
        }

        /// <summary>
        /// Generates a random string component of a specific length using a character set.
        /// </summary>
        private static string GenerateRandomComponent(string chars, int length)
        {
            // Use RandomNumberGenerator for cryptographically strong random data
            using (var rng = RandomNumberGenerator.Create())
            {
                var data = new byte[4];
                var result = new char[length];

                for (int i = 0; i < length; i++)
                {
                    // Generate a random index safely
                    rng.GetBytes(data);
                    uint rand = BitConverter.ToUInt32(data, 0);
                    int index = (int)(rand % chars.Length);

                    result[i] = chars[index];
                }
                return new string(result);
            }
        }

        /// <summary>
        /// Performs a Fisher-Yates shuffle on the characters of a string.
        /// </summary>
        private static string ShuffleString(string str)
        {
            char[] array = str.ToCharArray();
            using (var rng = RandomNumberGenerator.Create())
            {
                // Simple byte array for random numbers
                var data = new byte[8];

                for (int i = array.Length - 1; i > 0; i--)
                {
                    // Generate a random number up to (i + 1)
                    rng.GetBytes(data);
                    long randomValue = BitConverter.ToInt64(data, 0);
                    // Use modulo to get a valid index j, ensuring it's positive
                    int j = (int)(Math.Abs(randomValue) % (i + 1));

                    // Swap characters at position i and j
                    (array[i], array[j]) = (array[j], array[i]);
                }
            }
            return new string(array);
        }
    }
}
