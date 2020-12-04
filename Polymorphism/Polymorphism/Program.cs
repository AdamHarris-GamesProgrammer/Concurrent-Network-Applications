using System;

namespace Polymorphism
{
    class Animal
    {
        public virtual void Speak()
        {
            Console.WriteLine("Animal Speaks");
        }
    }
    class Dog : Animal
    {
        public override void Speak()
        {
            Console.WriteLine("Woof!");
        }
    }

    class Cat : Animal
    {
        public override void Speak()
        {
            Console.WriteLine("Meow");
        }
    }

    class Cow : Animal
    {
        public override void Speak()
        {
            Console.WriteLine("Moo");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Animal anim = new Animal();
            Dog dog = new Dog();
            Cat cat = new Cat();
            Cow cow = new Cow();

            int userInput;

            do
            {
                Console.Write("What animal would you like to hear\n1: Animal\n2: Dog\n3: Cat\n4: Cow\n5: Exit\nPlease make a choice between 1 and 5: ");
                userInput = Int32.Parse(Console.ReadLine());

                switch (userInput)
                {
                    case 1:
                        anim.Speak();
                        break;
                    case 2:
                        dog.Speak();
                        break;
                    case 3:
                        cat.Speak();
                        break;
                    case 4:
                        cow.Speak();
                        break;
                    case 5:
                        Console.WriteLine("Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Please enter a valid input");
                        break;

                } 
            } while (userInput != 5);
        }
    }
}
