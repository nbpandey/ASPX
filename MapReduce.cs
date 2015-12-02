using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ConsoleApplication1
{
    class TestMapReduce
    {
        static void Main()
        {
            string someTxt = "Apple, Mongo, Plum, Plum, Apple, Apple, Blackberry, Bluberry, Plum, Plum, Persimmons";
            // string someTxt = File.ReadAllText("..\\..\\data\\sample.txt");
            MapReduce wordReducer = new MapReduce();
            wordReducer.mapReduce(someTxt);

            // Print results to screen
            StringBuilder sb = new StringBuilder();
            int totalWords = 0;
            foreach (KeyValuePair<string, int> kvp in wordReducer.wordStore)
            {
                sb.AppendLine(kvp.Key + ": " + kvp.Value);
                totalWords += kvp.Value;
            }

            Console.WriteLine("Total Words: " + totalWords + "\r\n");
            Console.WriteLine(sb.ToString());
            File.WriteAllText("..\\..\\output\\result.txt", sb.ToString());
            Console.ReadLine();       
        }
    }
    
    

    class MapReduce
    {
        public static ConcurrentBag<string> wordBag = new ConcurrentBag<string>();
        public BlockingCollection<string> wordChunks = new BlockingCollection<string>(wordBag);
        public ConcurrentDictionary<string, int> wordStore = new ConcurrentDictionary<string, int>();
        
        /// <summary>
        /// 1. Yield Return 250 character or less chunks of text.
        /// 2. Break chunks on the first space encountered before 250 characters
        /// </summary>
        /// <param name="fileText"></param>
        /// <retruns></retruns>
        public IEnumerable<string> produceWordBlocks(string fileText)
        {
            int blocksize = 250;
            int startPos = 0;
            int len = 0;
            
            for (int i=0; i<fileText.Length; i++)
            {
                i = i+blocksize > fileText.Length-1 ? fileText.Length-1 : i+blocksize;
                
                while (i > startPos && fileText[i] != ' ')
                {
                    i--;
                }

                if (i == startPos)
                {
                    i = (i + blocksize) > (fileText.Length - 1) ? fileText.Length - 1 : i + blocksize;
                    len = (i - startPos) + 1;
                }
                else
                {
                    len = i - startPos;
                }
                // When a yield return statement is reached in the iterator method, expression is returned, and the current location in code is retained
                yield return fileText.Substring(startPos, len).Trim();
                startPos = i;
            }
        }

        // Producer
        public void mapWords(string fileText)
        {
            Parallel.ForEach(produceWordBlocks(fileText), wordBlock =>
                {
                    //Split the block into words
                    string[] words = wordBlock.Split(' ');
                    StringBuilder wordBuffer = new StringBuilder();

                    //cleanup each word and map it
                    foreach (string word in words)
                    {
                        // Character filtering. Avoid white spaces and punctuation
                        foreach (char c in word)
                        {
                            if (char.IsLetterOrDigit(c) || c == '\'' || c == '-')
                                wordBuffer.Append(c);                             
                        }

                        // Send word to the wordChunks Blocking Collection
                        if (wordBuffer.Length > 0)
                        {
                            wordChunks.Add(wordBuffer.ToString());
                            wordBuffer.Clear();
                        }
                    }
                });

            wordChunks.CompleteAdding();
        }

        // Consumer
        public void reduceWords()
        {
            // Using the Blocking Collection’s GetConsumingEnumerable() method in the Parallel.ForEach loop is one way to trigger the blocking behavior.
            Parallel.ForEach(wordChunks.GetConsumingEnumerable(), word =>
                {
                    // if the word exists, use a thread safe delegate to increment the value by 1 otherwise,
                    // add the the word with default value of 1.
                    wordStore.AddOrUpdate(word, 1, (key, oldValue) => Interlocked.Increment(ref oldValue));
                });
        }

        public void mapReduce(string fileText)
        {
            // Reset the Blocking Collection, if already used
            if (wordChunks.IsAddingCompleted)
            {
                wordBag = new ConcurrentBag<string>();
                wordChunks = new BlockingCollection<string>(wordBag);
            }

            // Create background process to map input data into words
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                mapWords(fileText);
            });

            // Reduce mapped words
            reduceWords();
        }



    }
}
