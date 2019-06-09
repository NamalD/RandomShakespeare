using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace ShakespeareGenerator
{
    public class PoemScraper
    {
        private const string HomeAddress = "http://shakespeare.mit.edu/";
        private const string DownloadDirectoryName = "downloads";

        public string DownloadPath
        {
            get
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var downloadPath = Path.Combine(currentDirectory, DownloadDirectoryName);
                return downloadPath;
            }
        }

        public async Task<IEnumerable<string>> GetPoemsAsync()
        {
            // Download poems if they do not exist
            var poemsDownloaded = Directory.Exists(DownloadPath) && Directory.GetFiles(DownloadPath, "*.html").Count() > 0;
            if (!poemsDownloaded)
            {
                await DownloadPoemsAsync();
            }

            // Extract text if it has not been done
            var extractedText = Directory.GetFiles(DownloadPath, "*.txt").Count() > 0;
            if (!extractedText)
            {
                ExtractCleanTextFromDownloads();
            }

            // Read text from extracted files
            var text = GetTextFromExtractedFiles();

            return text;
        }

        public async Task DownloadPoemsAsync()
        {
            // Make sure download folder exists
            Directory.CreateDirectory(DownloadPath);

            IDocument document;

            // Get home document
            var config = Configuration.Default.WithDefaultLoader();
            var address = HomeAddress;
            var context = BrowsingContext.New(config);
            document = await context.OpenAsync(address);

            // Get links to all poetry
            var poetryLinks = document
                .All
                .OfType<IHtmlAnchorElement>()
                .Where(ae => ae.Href.Contains("Poetry/"))
                .Select(ae => ae.Href);

            // Download poetry
            foreach (var link in poetryLinks)
            {
                document = await context.OpenAsync(link);

                // Download individual parts if split into parts
                var origin = document.BaseUrl.Origin;
                var partLinks = document.All.OfType<IHtmlAnchorElement>().Select(ae => ae.Href).Where(hr => hr.StartsWith(origin));
                if (partLinks.Count() > 0)
                {
                    await DownloadPartsAsync(partLinks);
                }
                else
                {
                    DownloadPoem(document);
                }
            }
        }

        private async Task DownloadPartsAsync(IEnumerable<string> partLinks)
        {
            foreach (var link in partLinks)
            {
                // Get document
                var config = Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);
                var document = await context.OpenAsync(link);

                DownloadPoem(document);
            }
        }

        private void DownloadPoem(IDocument document)
        {
            var content = document.Body.InnerHtml;

            var filename = $"{document.Title}.html";
            var filePath = Path.Combine(DownloadPath, filename);

            File.WriteAllText(filePath, content);
        }

        public void ExtractCleanTextFromDownloads()
        {
            var files = Directory.GetFiles(DownloadPath, "*.html");
            foreach (var file in files)
            {
                // Load document
                var document = File.ReadAllText(file);

                // Clean
                var cleanText = GetCleanText(document);

                // Split into multiple lines and clean whitespace
                var lines = cleanText
                            .Split('\n')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s));

                // Save
                var fileName = $"{file.Replace(".html", string.Empty)}.txt";
                var filePath = Path.Combine(DownloadPath, fileName);
                File.WriteAllLines(filePath, lines);
            }
        }

        private string GetCleanText(string document)
        {
            // Remove everything upto and including header
            var closingHeaderIndex = document.IndexOf("</h1>") + 5;
            var bodyAfterHeader = document.Substring(closingHeaderIndex);

            // Remove all <> tags
            var cleanText = Regex.Replace(bodyAfterHeader, "<.*>", string.Empty);

            // Remove excess whitespace
            cleanText = Regex.Replace(cleanText, @" +", " ").Trim();

            // Join seperated elipses
            cleanText = cleanText.Replace(". . .", "...");

            // Clean brackets
            cleanText = cleanText.Replace("(", string.Empty);
            cleanText = cleanText.Replace(")", string.Empty);

            // Weird things
            cleanText = cleanText.Replace("\"", string.Empty);

            return cleanText;
        }

        private IEnumerable<string> GetTextFromExtractedFiles()
        {
            var extractedFilePaths = Directory.GetFiles(DownloadPath, "*.txt");

            var allLines = new List<string>();

            foreach (var filePath in extractedFilePaths)
            {
                var lines = File.ReadAllLines(filePath);
                allLines.AddRange(lines);
            }

            return allLines;
        }
    }
}
