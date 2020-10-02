using System;

namespace GuessingGame
{
    
    class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            int guessNum = GetGuess();

            int userGuess;

            do
            {
                Console.Write("Please enter a number: ");
                userGuess = Int32.Parse(Console.ReadLine());

                if (userGuess > guessNum) Console.WriteLine("Your number was too high");
                else if (userGuess < guessNum) Console.WriteLine("Your number was too low");
                else Console.WriteLine("You entered the correct number!");
            } while (userGuess != guessNum);

        }

        static int GetGuess()
        {
            //Generates number between 1 and 100
            return random.Next(0, 101);
        }
    }
}
