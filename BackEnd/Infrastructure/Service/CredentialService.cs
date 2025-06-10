using LawyerProject.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LawyerProject.Infrastructure.Services
{
    public class CredentialService : ICredentialService
    {
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DigitChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*,.?";

        private static readonly string AllChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;

        private const int DefaultLength = 8;

        public string GenerateStrongPassword(int length = DefaultLength)
        {
            if (length < DefaultLength)
                throw new ArgumentException($"Password length must be at least {DefaultLength}", nameof(length));

            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var passwordChars = new List<char>(length);
            var buffer = new byte[4];

            passwordChars.Add(GetRandomChar(rng, LowercaseChars, buffer));
            passwordChars.Add(GetRandomChar(rng, UppercaseChars, buffer));
            passwordChars.Add(GetRandomChar(rng, DigitChars, buffer));
            passwordChars.Add(GetRandomChar(rng, SpecialChars, buffer));

            for (int i = passwordChars.Count; i < length; i++)
            {
                passwordChars.Add(GetRandomChar(rng, AllChars, buffer));
            }

            ShuffleList(passwordChars, rng);

            return new string(passwordChars.ToArray());
        }

        private static char GetRandomChar(System.Security.Cryptography.RandomNumberGenerator rng, string charSet, byte[] buffer)
        {
            rng.GetBytes(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            return charSet[(int)(num % charSet.Length)];
        }

        private static void ShuffleList(List<char> list, System.Security.Cryptography.RandomNumberGenerator rng)
        {
            int n = list.Count;
            var buffer = new byte[4];

            for (int i = n - 1; i > 0; i--)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                int j = (int)(num % (uint)(i + 1));

                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public string GenerateTemporaryEmail()
        {
            var prefix = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{prefix}@Dadvik";
        }
    }
}
