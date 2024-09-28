using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using PuppeteerSharp;
using SmorcIRL.TempMail;
using SmorcIRL.TempMail.Models;
using RegistracjaTEST;
namespace RegistracjaTEST
{
    public class EmailManager
    {
        private readonly MailClient _client;
        private readonly string _emailAddress;
        private readonly string _password;

        public EmailManager(string emailAddress, string password)
        {
            _client = new MailClient();
            _emailAddress = emailAddress;
            _password = password;
            Console.WriteLine($"EmailManager initialized for {_emailAddress}");
        }

        public string GetEmailAddress()
        {
            return _emailAddress;
        }

        public async Task<string> RegisterAccountAsync()
        {
            Console.WriteLine($"Attempting to register account: {_emailAddress}");
            try
            {
                if (string.IsNullOrWhiteSpace(_emailAddress) || !_emailAddress.Contains('@'))
                {
                    Console.WriteLine("Error: Email address is missing domain or is invalid.");
                    throw new ArgumentException("Invalid email address format.");
                }

                await _client.Register(_emailAddress, _password);
                Console.WriteLine($"Account {_emailAddress} registered successfully.");
                return $"Account {_emailAddress} registered successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration failed: {ex.Message}. Trying to log in instead...");
                await _client.Login(_emailAddress, _password);
                Console.WriteLine($"Logged in with account {_emailAddress}");
                return $"Logged in with account {_emailAddress}";
            }
        }

        public static async Task<EmailManager> CreateAndRegisterAsync(string username, string password)
        {
            Console.WriteLine("Creating and registering new EmailManager instance...");
            Console.WriteLine(password);

            MailClient client = new MailClient();
            DomainInfo[] domains = await client.GetAvailableDomains();

            string domain = domains[0].Domain;
            string emailAddress = $"{username}@{domain}";

            var manager = new EmailManager(emailAddress, password);
            await manager.RegisterAccountAsync();
            return manager;
        }

        public async Task<string> FetchActivationLinkAsync()
        {
            Console.WriteLine("Fetching activation link...");
            bool messageFound = false;
            string activationLink = string.Empty;

            while (!messageFound)
            {
                Console.WriteLine("Checking for new messages...");
                MessageInfo[] allMessages = await _client.GetAllMessages();

                foreach (var message in allMessages)
                {
                    Console.WriteLine($"Message found - Subject: {message.Subject}");
                    if (message.Subject == "Aktywacja konta")
                    {
                        Console.WriteLine("Activation message found.");
                        MessageDetailInfo messageDetail = await _client.GetMessage(message.Id);
                        activationLink = ExtractActivationLink(messageDetail.BodyText);

                        if (!string.IsNullOrEmpty(activationLink))
                        {
                            Console.WriteLine($"Activation link extracted: {activationLink}");
                            messageFound = true;
                        }
                        else
                        {
                            Console.WriteLine("Activation link not found in the email body.");
                        }
                    }
                }

                if (!messageFound)
                {
                    await Task.Delay(10000);
                }
            }

            return activationLink;
        }

        public async Task ActivateAccountAsync(IPage page, string activationLink)
        {
            Console.WriteLine($"Activating account using link: {activationLink}");
            await page.GoToAsync(activationLink);
            await page.WaitForNavigationAsync();
            Console.WriteLine("Account activated successfully.");
        }

        private static string ExtractActivationLink(string bodyText)
        {
            Console.WriteLine("Extracting activation link from email body...");
            string pattern = @"https?://[^\s]+";
            Match match = Regex.Match(bodyText, pattern);
            return match.Success ? match.Value : string.Empty;
        }
    }
}