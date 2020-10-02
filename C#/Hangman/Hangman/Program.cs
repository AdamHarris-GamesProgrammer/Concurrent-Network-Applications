using System;
using System.Collections.Generic;

namespace Hangman
{
    class Program
    {
        static int m_Lives = 6;
        static List<char> m_IncorrectLetters = new List<char>();
        static List<char> m_CorrectLetters = new List<char>();
        static List<char> m_ShownLetters = new List<char>();
        static List<char> m_incorrectEnteredLetters = new List<char>();
        static List<string> m_Words = new List<string> { "Hello", "World", "Potato", "Carrots", "Pork", "Cow", "Animal" };
        static string m_Word;
        static bool m_Won = false;
        static char m_PlayAgain = 'n';

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to hangman!");

            GenerateWord();

            do
            {
                ShowHangman();
                Console.WriteLine("So far you have guessed these letters correctly: ");
                for (int i = 0; i < m_ShownLetters.Count; i++)
                {
                    Console.Write(m_ShownLetters[i] + "\t");
                }
                Console.WriteLine("... and these letters incorrectly: ");
                for (int i = 0; i < m_incorrectEnteredLetters.Count; i++)
                {
                    Console.Write(m_incorrectEnteredLetters[i] + "\t");
                }

                Console.WriteLine("What letter is your guess? ");
                char guess = Console.ReadKey().KeyChar;
                if (CheckGuess(guess))
                {
                    m_ShownLetters.Add(guess);
                }
                else
                {
                    if (!m_incorrectEnteredLetters.Contains(guess))
                    {
                        m_incorrectEnteredLetters.Add(guess);
                    }
                    else
                    {
                        Console.WriteLine("You have already entered {0} before", guess);
                    }
                }

            } while (m_Lives > 0 || m_Won);
        }

        static void ShowHangman()
        {
            Console.WriteLine("     |------+  ");
            Console.WriteLine("     |      |  ");
            Console.WriteLine("     |      " + (m_Lives < 6 ? "O" : ""));
            Console.WriteLine("     |     " + (m_Lives < 4 ? "/" : "") + (m_Lives < 5 ? "|" : "") + (m_Lives < 3 ? @"\" : ""));
            Console.WriteLine("     |     " + (m_Lives < 2 ? "/" : "") + " " + (m_Lives < 1 ? @"\" : ""));
            Console.WriteLine("     |         ");
            Console.WriteLine("===============");
        }

        static bool CheckGuess(char guess)
        {
            if (m_CorrectLetters.Contains(guess)) return true;
            else return false;
        }

        static void GenerateWord()
        {
            Random rand = new Random();

            m_Word = m_Words[rand.Next(0, m_Words.Count)];

            //Go through all letters in alphabet and sorts the correct and incorrect letters out
            char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            foreach(char c in alphabet)
            {
                if (m_Word.Contains(c))
                {
                    if(!m_CorrectLetters.Contains(c)) m_CorrectLetters.Add(c);
                }
                else
                {
                    if (!m_IncorrectLetters.Contains(c)) m_IncorrectLetters.Add(c);
                }
            }
        }
    }
}
