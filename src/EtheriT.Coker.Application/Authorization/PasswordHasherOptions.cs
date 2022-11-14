using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Authorization
{
    public class PasswordHasherOptions
    {
        private static readonly RandomNumberGenerator DefaultRng = RandomNumberGenerator.Create(); // secure PRNG

        /// <summary>
        /// Gets or sets the number of iterations used when hashing passwords using PBKDF2.
        /// </summary>
        /// <value>
        /// The number of iterations used when hashing passwords using PBKDF2.
        /// </value>
        /// <remarks>
        /// This value is only used when the compatibility mode is set to 'V3'.
        /// The value must be a positive integer. The default value is 10,000.
        /// </remarks>
        public int IterationCount { get; set; } = 10000;

        internal RandomNumberGenerator Rng { get; set; } = DefaultRng;
    }
}
