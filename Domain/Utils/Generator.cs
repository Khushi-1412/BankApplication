using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Domain.Utils
{
    internal class Generator
    {
        private static readonly Random _rGenerator = new();
        private static readonly string _digits = "0123456789";
       

        public static string GetRandomNumbers(int length)
        {
            var randomNumbers = new string(
                    Enumerable.Repeat(_digits, length)
                    .Select(d => d[_rGenerator.Next(d.Length)])
                    .ToArray()
                );

            return randomNumbers;
        }

       

       
    }
}
