using Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicServer
{
    class Hangman
    {
        Server mServer;

        int mLives = 6;
        List<char> mIncorrectLetters;
        List<char> mCorrectLetters;
        string[] mWords = { "block", "magnitude", "flu", "topple", "vegetarian", "economics", "glove", "deport", "cupboard", "ratio" };
        string mWord;
        bool mWon = false;
        char mPlayAgain = 'n';

        public Hangman(Server server)
        {
            mServer = server;

            mLives = 6;
            mIncorrectLetters = new List<char>();
            mCorrectLetters = new List<char>();
            Random rand = new Random();
            mWord = mWords[rand.Next(0, mWords.Length)];

            PrintHangman();
        }

        public void PrintHangman()
        {
            string currentState = "";

            string currentWord = PrintWord();

            if (mWon)
            {
                currentState = "Congratulations! You win, the word was: " + mWord;
            }
            else if(mLives < 0)
            {
                currentState = "You lost! Better luck next time, the word was: " + mWord;
            }
            else
            {
                string incorrectLetters = "";
                string correctLetters = "";

                foreach (char c in mIncorrectLetters)
                {
                    incorrectLetters += c;
                }

                foreach (char c in mCorrectLetters)
                {
                    correctLetters += c;
                }

                currentState =
                "\n     |------+  " +
                "\n     |      |  " +
                "\n     |      " + (mLives < 6 ? "O" : "") +
                "\n     |     " + (mLives < 4 ? "/" : "") + (mLives < 5 ? "|" : "") + (mLives < 3 ? @"\" : "") +
                "\n     |     " + (mLives < 2 ? "/" : "") + " " + (mLives < 1 ? @"\" : "") +
                "\n     |         " +
                "\n===============" +
                "\nWord so far: " + currentWord +
                "\nCurrent lives: " + mLives +
                "\nIncorrect Letters: " + incorrectLetters +
                "\nCorrect Letters: " + correctLetters;
            }

            HangmanInformationPacket hangmanInformationPacket = new HangmanInformationPacket(currentState);

            mServer.TcpSendToAll(hangmanInformationPacket);
        }

        public void TakeGuess(char c)
        {
            if (mWon)
            {
                return;
            }

            c = char.ToLower(c);

            if (mWord.Contains(c))
            {
                if (!mCorrectLetters.Contains(c))
                {
                    mCorrectLetters.Add(c);
                    mCorrectLetters.Sort();
                }
            }
            else
            {
                mLives--;

                if(!mIncorrectLetters.Contains(c))
                {
                    mIncorrectLetters.Add(c);
                    mIncorrectLetters.Sort();
                }
            }

            PrintHangman();
        }

        public string PrintWord()
        {
            string result = "";

            mWon = true;
            for (int i = 0; i < mWord.Length; ++i)
            {
                if (mCorrectLetters.Contains(Char.ToLower(mWord[i])))
                    result += (mWord[i]);
                else
                {
                    mWon = false;
                    result += (" _");
                }
            }

            return result;
        }
    }
}
