using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;
using SmorcIRL.TempMail;
using System.Text.RegularExpressions;
using SmorcIRL.TempMail.Models;
using RegistracjaTEST;
using ImGuiNET;
using Veldrid;

namespace RegistracjaTEST
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var accountsFile = "created_accounts.txt";
                EnsureFileExists(accountsFile);
                var configForm = new ConfigForm();
                var (numOfAccounts, maxConcurrentThreads, regConfig) = configForm.GetConfiguration();

                if (regConfig == null)
                {
                    Console.WriteLine("Anulowano konfigurację.");
                    return;
                }

                var semaphore = new SemaphoreSlim(maxConcurrentThreads);

                var tasks = Enumerable.Range(0, numOfAccounts).Select(async _ =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var registrationUrl = "https://main.balmora.pl/rejestracja/";
                        var random = new Random();
                        int length = random.Next(6, 9); // Random length between 6 and 8
                        var username = GenerateRandomString(random, length);
                        var password = GenerateRandomPassword(random, length);

                        var emailManager = await EmailManager.CreateAndRegisterAsync(username, password);
                        string emailAddress = emailManager.GetEmailAddress();

                        var result = await RegisterAccountAsync(accountsFile, regConfig, username, password, emailAddress);
                        IPage page = result.Page;

                        if (page != null)
                        {
                            string activationLink = await emailManager.FetchActivationLinkAsync();
                            if (!string.IsNullOrEmpty(activationLink))
                            {
                                await emailManager.ActivateAccountAsync(page, activationLink);
                                Console.WriteLine("Konto zostało pomyślnie aktywowane.");
                            }
                            else
                            {
                                Console.WriteLine("Nie znaleziono linku aktywacyjnego.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in task: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
                Console.WriteLine("All registrations and activations completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
        }

        static async Task<(IPage Page, string Username, string Password)> RegisterAccountAsync(string accountsFile, RegistrationConfig regConfig, string username, string password, string emailAddress)
        {
            var proxy = Proxy.GetRandomProxy();

            var launchOptions = new LaunchOptions
            {
                Headless = false,
                DefaultViewport = null,
                Args = new[] { $"--proxy-server={proxy.Address}" }
            };

            var browser = await Puppeteer.LaunchAsync(launchOptions);
            var page = await browser.NewPageAsync();

            if (proxy.RequiresAuthentication)
            {
                await page.AuthenticateAsync(new Credentials
                {
                    Username = proxy.Username,
                    Password = proxy.Password
                });
            }

            await page.GoToAsync(regConfig.Url);

            // Wypełnij formularz rejestracyjny
            await page.TypeAsync("#username", username);
            await page.TypeAsync("#password", password);
            await page.TypeAsync("#email", emailAddress);

            if (regConfig.RequiresPin)
            {
                // Logika dla strony wymagającej PINu
                string pin = GenerateRandomPin(); // Zaimplementuj tę metodę
                await page.TypeAsync("#pin", pin);
                Console.WriteLine($"Wygenerowany PIN: {pin}");
            }

            // Kliknij przycisk rejestracji
            await page.ClickAsync("button[type='submit']");

            // Poczekaj na zakończenie rejestracji
            await page.WaitForNavigationAsync();

            // Zapisz dane konta do pliku
            await File.AppendAllTextAsync(accountsFile, $"{username},{password},{emailAddress}\n");

            return (page, username, password);
        }

        private static string GenerateRandomPin()
        {
            // Implementacja generowania losowego PINu
            Random random = new Random();
            return random.Next(1000, 9999).ToString("D4");
        }

        static string GenerateRandomString(Random random, int length)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz";
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        static string GenerateRandomPassword(Random random, int length)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!";
            var result = new char[length];
            bool hasUpper = false, hasNumber = false, hasSpecial = false;

            for (int i = 0; i < length; i++)
            {
                char nextChar = chars[random.Next(chars.Length)];
                result[i] = nextChar;

                if (char.IsUpper(nextChar)) hasUpper = true;
                if (char.IsDigit(nextChar)) hasNumber = true;
                if (!char.IsLetterOrDigit(nextChar)) hasSpecial = true;
            }

            if (!hasUpper) result[random.Next(length)] = 'A';
            if (!hasNumber) result[random.Next(length)] = '1';
            if (!hasSpecial) result[random.Next(length)] = '!';

            return new string(result);
        }
    }
}
