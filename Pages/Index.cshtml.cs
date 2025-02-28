using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace FuzzyFilter.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile uploadFile { get; set; }
        [BindProperty]
        public int fuzzyPct { get; set; } = 50;
        [BindProperty]
        public string inputTextArea { get; set; }
        public string processedText { get; set; }
        public List<string> inputStrings = new List<string>();
        public Dictionary<string, List<string>> fuzzyResults = new Dictionary<string, List<string>>();

        private int maximumLengthDifference = 6;

        public async Task<IActionResult> OnPostAsync()
        {
            //check if there was a file uploaded, give that priority and do not process text box area if there is a file
            if(uploadFile != null && uploadFile.Length != 0)
            {
                using (var reader = new StreamReader(uploadFile.OpenReadStream(), Encoding.UTF8))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        inputStrings.Add(line);
                    }
                } 
                processedText = String.Format("File uploaded successfully: {0}.", uploadFile.FileName);
            }
            //if there is no file, check for input in the text box
            else if(inputTextArea != null && inputTextArea.Length > 0)
            {
                inputStrings = new List<string>(inputTextArea.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                processedText = String.Format("Text input uploaded successfully.");
            }
            //else, there was no input
            else
            {
                processedText = "Please select a file or input text to be processed.";
                return Page();
            }

            //ensure there is something in the list and something to compare to
            if(inputStrings.Count > 1)
            {
                inputStrings = normalizeInput(inputStrings);
                int inputStringsLength = inputStrings.Count;
                fuzzyResults = compareInputs(inputStrings);
                processedText = String.Format("{0} Compared {1} lines.", processedText, inputStringsLength);
            }
            else
            {
                processedText = String.Format("{0} Not enough data for comparison.", processedText);
            }
            
            return Page();
        }

        private List<string> normalizeInput(List<string> inputStrings)
        {
            //this method intends to normalize inputs by:
            //- ensuring everything is lowercase
            //- removing non-alphanumeric characters (such as ".,-$", etc.)
            //- then sorting by lengh of the string for comparison later
            List<string> normalizedLines = new List<string>();
            foreach(string input in inputStrings)
            {
                string normalizedInput = input.ToLower();
                normalizedInput = Regex.Replace(normalizedInput, @"[^a-zA-Z0-9]", "");
                normalizedLines.Add(normalizedInput);
            }
            normalizedLines = normalizedLines.OrderBy(line => line.Length).ToList();

            return normalizedLines;
        }

        private Dictionary<string,List<string>> compareInputs(List<string> inputs)
        {
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

            for (int i = 0; i < inputs.Count; i++)
            {
                int j = i + 1;
                while(j < inputs.Count)
                {
                    //if the strings are more than 6 characters different, do no do the calculation
                    if (Math.Abs(inputs[i].Length - inputs[j].Length) > maximumLengthDifference){
                        j++;
                    }
                    else
                    {
                        double similarityPercent = LevenshteinDistance(inputs[i], inputs[j]);
                        if (similarityPercent >= fuzzyPct)
                        {
                            if (!dict.ContainsKey(inputs[i]))
                                dict[inputs[i]] = new List<string>();

                            dict[inputs[i]].Add(inputs[j]);

                            //remove the matched item at its index, but do not increment j in this case since the next item will be at this index
                            inputs.RemoveAt(j);
                        }
                        else
                        {
                            j++;
                        }
                    }                    
                }
            }

            return dict;
        }

        //this algorithm was written with the assist of ChatGPT and StackOverflow
        //with my input to convert the distance to a percentage
        private static double LevenshteinDistance(string input, string compare)
        {
            int inputLen = input.Length;
            int compareLen = compare.Length;            

            int[,] dp = new int[inputLen + 1, compareLen + 1];

            for (int i = 0; i <= inputLen; i++)
                dp[i, 0] = i;
            for (int j = 0; j <= compareLen; j++)
                dp[0, j] = j;

            for (int i = 1; i <= inputLen; i++)
            {
                for (int j = 1; j <= compareLen; j++)
                {
                    int cost = (input[i - 1] == compare[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(
                        dp[i - 1, j] + 1,    // Deletion
                        dp[i, j - 1] + 1),   // Insertion
                        dp[i - 1, j - 1] + cost); // Substitution
                }
            }

            int distance = dp[inputLen, compareLen];
            return (1.0 - (double)distance / Math.Max(inputLen, compareLen)) * 100;
        }
    }
}
