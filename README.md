# FuzzyFilter

Hi! I'm hoping to walk through my thought process on this.
First, I took a little while to think about it (of course). My initial instinct was:
1. This is essentially a "fuzzy match" search. Unfortunately there is no "fuzzy search" in C# like in SQL. 
2. I need accept a text file - from an IIS page potentially?
3. normalize the data from the file
4. sort by length (therefore guaranteeing that item being compared would be less than or equal to subsequent items, and can be partially or completely contained within those subsequent items)
5. iterate over the normalized list to look for matches using a config value for "% contained within" to determine if it was a match
6. complie a dictionary of key-value pairs where the key is the item of comparison, and the value is a list of everything that was a "match". This can be returned to the screen for display after the search is done.

That lead to a couple tangential thoughts for implementation:
- what if I didn't want to upload a file every time? (just for my own testing sake). I'll add a text box to allow for organic input for testing, but then I'll just leave it as a "feature" 🙂
- why should the % match be a hard set config value? I can't account for every edge case and I know this won't be perfect, so maybe that number should be allowed to be changed by the user and mileage may vary (enter: the slider)
- after a value is considered a "match", should I remove it from the list for future consideration? My instinct on this is yes, so thats what I did.

# Implementation
- Creating the IIS page was easy, given visual studio has a template.
- I updated the HTML for the basic home page to be able to accept a file, have a text area, a slider, and a submit button.
- I updated the cshtml.cs model to have the proper fields that connect to the front end (i.e. the slider value gets saved to an int called "fuzzyPct" and the file/text area get linked appropriately)
- set up the basic if/else flow of "check for a file -> check for text input -> no input, display a message about no input", and check that each of those are functioning
- parse the file/text area into a List<string>
- write the normalizeInput method
- now do the comparison....wait

# Comparison
I will admit - this whole thing took me closer to two hours than 1-1.5 hours...mostly because I started down a rabbit hole. I had a "fuzzy" memory from school about how fuzzy algorithms work, but I needed to read more.
I focused on **Levenshtein distance** and **Jaro-Winkler** - Based on my reading, I ultimately settled on using Levenshtein Distance. I believe that the Jaro-Winkler is not as applicable here since it would rate words that are similar but obviously different as a higher match than Levenshtein. 
For example, the words "cat" and "chat" are considered a 92.5% match with a Jaro-Winkler algorithm, whereas they are only a 75% match using Levenshtein, and I believe the latter is more accurate and could lead to fewer false positives. 
I will also admit that I leaned on ChatGPT here to explain more about the Jaro-Winkler and Levenshtein algorithms and also assist in writing the code I would need to compare my normalized data. This is called out in the code.

# Future Considerations
In the future or with more time I would:
- Add more CSS and clean up the interface.
- Include a way to "remove" a file after it has been uploaded.
- Include a way to download the results (or write a sql script to be able to delete duplicates).
- Include a way to check text file encoding to ensure data is being correctly parsed.
- this isnt exactly fast. Some more consideration would have to be given to how to speed this up.
