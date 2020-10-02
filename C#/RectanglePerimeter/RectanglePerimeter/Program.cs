using System;

namespace RectanglePerimeter
{
    class Program
    {
        static void Main(string[] args)
        {
            float width = 5.4f;
            float height = 3.2f;

            float perimeter = (width * 2) + (height * 2);
            float area = height * width;

            Console.WriteLine("Perimeter of Rectangle (5.4, 3.2) is: " + perimeter);
            Console.WriteLine("Area of Rectangle (5.4, 3.2) is: " + area);
        }
    }
}
