using System;
using System.Threading;

class Program
{
    static int[] array;
    static int globalMin = int.MaxValue;
    static int globalMinIndex = -1;

    static readonly object locker = new object();
    static CountdownEvent countdown;

    static void Main()
    {
        Console.Write("Введіть довжину масиву: ");
        int length = int.Parse(Console.ReadLine());

        Console.Write("Введіть кількість потоків: ");
        int threadCount = int.Parse(Console.ReadLine());

        GenerateArray(length);

        countdown = new CountdownEvent(threadCount);

        int chunkSize = length / threadCount;
        for (int i = 0; i < threadCount; i++)
        {
            int start = i * chunkSize;
            int end = (i == threadCount - 1) ? length - 1 : (start + chunkSize - 1);

            Thread t = new Thread(() => FindLocalMin(start, end));
            t.Start();
        }

        // Очікуємо завершення всіх потоків без join()
        countdown.Wait();

        Console.WriteLine($"\nМінімальне значення: {globalMin}");
        Console.WriteLine($"Індекс мінімального значення: {globalMinIndex}");
    }

    static void GenerateArray(int length)
    {
        array = new int[length];
        Random rnd = new Random();
        for (int i = 0; i < length; i++)
        {
            array[i] = rnd.Next(1, 10000);
        }

        // Вставка від’ємного елементу
        int negIndex = rnd.Next(0, length);
        array[negIndex] = -rnd.Next(1, 1000);

        Console.WriteLine($"\nЗгенерований масив (частково):");
        for (int i = 0; i < Math.Min(20, length); i++)
        {
            Console.Write(array[i] + " ");
        }
        Console.WriteLine("...");
    }

    static void FindLocalMin(int start, int end)
    {
        int localMin = int.MaxValue;
        int localMinIndex = -1;

        for (int i = start; i <= end; i++)
        {
            if (array[i] < localMin)
            {
                localMin = array[i];
                localMinIndex = i;
            }
        }

        lock (locker)
        {
            if (localMin < globalMin)
            {
                globalMin = localMin;
                globalMinIndex = localMinIndex;
            }
        }

        countdown.Signal();
    }
}
