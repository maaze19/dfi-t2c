using Dfi.Rpc;
using Dfi.Rpc.Responses;
using Dfi.Rpc.Specifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace dfi_t2c
{
    class Program
    {
        private static readonly Symbol DFI = new Symbol() { Id = 0, Name = "DFI" };
        static void Main(string[] args)
        {
            try
            {
                string cookiePath = GetConfigValue<string>("CookiePath", "%APPDATA%/DeFi Blockchain/.cookie");
                cookiePath = Environment.ExpandEnvironmentVariables(cookiePath);
                string daemonUrl = GetConfigValue<string>("DaemonUrl", "http://127.0.0.1:8555");
                decimal minAmount = GetConfigValue<decimal>("MinimumConvertingAmount", 1.0m);
                short rpcRequestTimeoutInSeconds = GetConfigValue<short>("RpcRequestTimeoutInSeconds", 10);
                bool askBeforeChange = GetConfigValue<bool>("AskBeforeChange", true);

                IDeFiService deFiService = new DeFiService(daemonUrl, cookiePath, null, rpcRequestTimeoutInSeconds);
                OutputDfiBalance(deFiService);
                TokenToCoins(deFiService, minAmount, askBeforeChange);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            bool waitForInputBeforeExit = GetConfigValue<bool>("WaitForInputBeforeExit", true);
            if (waitForInputBeforeExit)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        private static void OutputDfiBalance(IDeFiService deFiService)
        {
            decimal dfiCoins = deFiService.GetBalance(1, false, false);
            decimal dfiTokens = deFiService.GetTokenBalances().FirstOrDefault(x => x.Key == 0).Value;
            Console.WriteLine("DFI coins: {0}", dfiCoins);
            Console.WriteLine("DFI tokens: {0}", dfiTokens);
            Console.WriteLine(new string('-', 20));
            Console.WriteLine();
        }
        private static void TokenToCoins(IDeFiService deFiService, decimal minAmount = 1m, bool askBeforeChange = false)
        {
            var accounts = deFiService.ListAccounts();
            IEnumerable<string> addressesToLookup = accounts.SelectMany(x => x.Owner.Addresses).Distinct();
            Dictionary<string, BalancesWithTokens> balancesPerAddress = addressesToLookup.ToDictionary(address => address, address => deFiService.GetAccount(address));
            var step1 = balancesPerAddress
                .Where(x => x.Value.Keys.Any(symbol => symbol.Id == DFI.Id))
                .ToList();
            Dictionary<string, decimal> outAddresses = balancesPerAddress
                .Where(x => x.Value.Keys.Any(symbol => symbol.Id == DFI.Id))
                .ToDictionary(x => x.Key, x => x.Value.FirstOrDefault(y => y.Key.Id == DFI.Id).Value);
            Console.WriteLine("Found {0} addresses with DFI token:", outAddresses.Count);
            foreach (var outAddr in outAddresses)
                Console.WriteLine("{0} => {1} DFI token", outAddr.Key, outAddr.Value);
            if (outAddresses.Count(x => x.Value >= minAmount) == 0)
            {
                Console.WriteLine("No address contains a minimum of {0} DFI token for converting (defined in settings).", minAmount);
                Console.WriteLine("Converting DFI tokens to coins will be cancelled.");
            }
            if(askBeforeChange)
            {
                Console.WriteLine("Do you want to swap your DFI tokens to DFI coins? (y/n)");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                string keyAsString = consoleKeyInfo.Key.ToString();
                if (keyAsString?.ToLower() != "y")
                {
                    Console.WriteLine("Swapping aborted by user.");
                    return;
                }
            }
            foreach (var outAddr in outAddresses.Where(x => x.Value >= minAmount))
            {
                string from = outAddr.Key;
                ToAddress to = new ToAddress(from, new TokenAmount[] { new TokenAmount(outAddr.Value, DFI.Name) });
                string hash = deFiService.AccountToUtxos(from, to);
                Console.WriteLine("Changed {0} DFI token to coins at address {1}: {2}", outAddr.Value, from, hash);
            }
        }
        private static T GetConfigValue<T>(string key, T @default)
        {
            try
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                    if (ConfigurationManager.AppSettings[key] is string valueAsString)
                        if (Convert.ChangeType(valueAsString, typeof(T), CultureInfo.InvariantCulture) is T value)
                            return value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while reading config value: {0}", ex.Message);
            }
            Console.WriteLine("Could not find configuration value \"{0}\". Using default \"{1}\"", key, @default);
            return @default;
        }
    }
}
