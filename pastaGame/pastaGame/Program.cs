using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ConsoleCatchGame
{
    class Program
    {
        // Ayarlar
        static int width = 30;
        static int height = 15;
        static int playerX = 15;
        static int playerY = 14;
        static int score = 0;
        static int targetScore = 5; // Test için 5'te biter
        static bool isGameOver = false;
        static string logFile = "oyun_log.txt"; 
        
        // YENİ: Oyunu kastırmamak için logları burada biriktirip topluca yazacağız
        static List<string> logBuffer = new List<string>();

        class Item
        {
            public int X { get; set; }
            public int Y { get; set; }
            public char Symbol { get; set; }
        }

        static List<Item> items = new List<Item>();
        static Random rnd = new Random();

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            File.WriteAllText(logFile, "--- OYUN BAŞLADI ---\n");
            int frameCount = 0;

            while (!isGameOver)
            {
                Draw();
                Input();
                Logic(frameCount);
                
                // YENİ: Kasmayı önlemek için biriken logları tek seferde dosyaya yaz
                if (logBuffer.Count > 0)
                {
                    File.AppendAllLines(logFile, logBuffer);
                    logBuffer.Clear(); // Yazdıktan sonra hafızayı temizle
                }

                Thread.Sleep(100);
                frameCount++;
            }

            // Bitiş Ekranı
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("TEBRİKLER, OYUN BİTTİ!");
            Console.WriteLine("Skorunuz: " + score);
            
            Log($"GAME_OVER -> finalScore={score}");
            File.AppendAllLines(logFile, logBuffer); // Kalan son logları da yaz
            
            Console.WriteLine("\nÇıkmak için bir tuşa basın...");
            Console.ResetColor(); 
            Console.ReadKey();
        }

        static void Draw()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Skor: {score} / Hedef: {targetScore}");

            // Zemin çizgisi
            Console.SetCursorPosition(0, playerY + 1);
            Console.WriteLine(new string('-', width));

            // Oyuncuyu çiz (Yeşil Renk)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(playerX, playerY);
            Console.Write("@");

            // Düşen objeleri çiz (Renkli)
            foreach (var item in items)
            {
                if (item.Y < height && item.Y > 0)
                {
                    Console.SetCursorPosition(item.X, item.Y);
                    Console.ForegroundColor = item.Symbol == '*' ? ConsoleColor.Yellow : ConsoleColor.Cyan;
                    Console.Write(item.Symbol);
                }
            }
            Console.ResetColor();
        }

        static void Input()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                int oldX = playerX;
                
                if (keyInfo.Key == ConsoleKey.LeftArrow && playerX > 0)
                {
                    playerX--;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow && playerX < width - 2)
                {
                    playerX++;
                }

                if (oldX != playerX)
                {
                    Log($"INPUT -> key={keyInfo.Key} playerX={playerX} playerY={playerY}");
                    Log($"MOVEMENT -> oldX={oldX} newX={playerX}");
                }
            }
        }

        static void Logic(int frameCount)
        {
            if (frameCount % 4 == 0) 
            {
                int x = rnd.Next(0, width - 2);
                char symbol = rnd.Next(0, 2) == 0 ? '*' : 'O';
                items.Add(new Item { X = x, Y = 1, Symbol = symbol });
                
                Log($"UPDATE -> itemSpawned x={x} y=1 symbol={symbol}");
            }

            for (int i = items.Count - 1; i >= 0; i--)
            {
                items[i].Y++;
                
                Log($"ITEM_MOVEMENT -> newX={items[i].X} newY={items[i].Y}");

                if (items[i].X == playerX && items[i].Y == playerY)
                {
                    score++;
                    Log($"COLLISION -> score={score} itemX={items[i].X} itemY={items[i].Y}");
                    items.RemoveAt(i);

                    if (score >= targetScore)
                    {
                        isGameOver = true;
                    }
                    continue;
                }

                if (items[i].Y >= height)
                {
                    items.RemoveAt(i);
                }
            }
        }

        static void Log(string message)
        {
            // Dosyayı meşgul etmez, sadece listeye ekler
            logBuffer.Add(message);
        }
    }
}