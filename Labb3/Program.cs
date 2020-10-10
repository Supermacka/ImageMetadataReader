using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Numerics;

namespace Labb3
{
    class Program
    {
        static void Main(string[] args)
        {
            var bmp = Encoding.ASCII.GetBytes("BM"); 
            var bmpCompare = new byte[bmp.Length];
            var png = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }; 
            var pngCompare = new byte[png.Length];

            string filePath = @args[0].ToString();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    int fileSize = (int)fs.Length;
                    var byteData = new byte[fs.Length];
                    fs.Read(byteData, 0, fileSize);

                    for (int i = 0; i < fileSize; i++)
                    {
                        if (i < pngCompare.Length)
                        {
                            pngCompare[i] = byteData[i];
                        }
                        if (i < bmpCompare.Length)
                        {
                            bmpCompare[i] = byteData[i];
                        }
                    }
                    if (Enumerable.SequenceEqual(pngCompare, png))
                    {
                        fs.Close();
                        Console.WriteLine($"This is a .png image. Resolution: {GetPNGSize(filePath).X}x{GetPNGSize(filePath).Y} pixels.");
                        GetPNGChunks(filePath);
                    }
                    else if (Enumerable.SequenceEqual(bmpCompare, bmp))
                    {
                        fs.Close();
                        Console.WriteLine($"This is a .bmp image. Resolution: {GetBMPSize(filePath).X}x{GetBMPSize(filePath).Y} pixels.");
                    }
                    else
                    {
                        Console.WriteLine("File not found.");
                        Console.WriteLine("This is not a valid .bmp or .png file!");
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("File not found.");
                Console.WriteLine("This is not a valid .bmp or .png file!");
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Console.WriteLine("This file path does not exist");
                Console.WriteLine("Make sure you have entered the correct path");
            }
        }
        public static Vector2 GetPNGSize(string filePath)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(filePath));
            br.BaseStream.Position = 16;

            byte[] widthbytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i++)
            {
                widthbytes[sizeof(int) - 1 - i] = br.ReadByte();
            }
            int width = BitConverter.ToInt32(widthbytes, 0);

            byte[] heightbytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i++)
            {
                heightbytes[sizeof(int) - 1 - i] = br.ReadByte();
            }
            int height = BitConverter.ToInt32(heightbytes, 0);

            return new Vector2(width, height);
        }

        public static Vector2 GetBMPSize(string filePath)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(filePath));
            br.BaseStream.Position = 18;
            
            byte[] widthBbytes = new byte[sizeof(int)]; 
            for (int i = 0; i < widthBbytes.Length; i++)
            {
                widthBbytes[i] = br.ReadByte();
            }
            int width = BitConverter.ToInt32(widthBbytes, 0);

            br.BaseStream.Position = 22;

            byte[] heightBytes = new byte[sizeof(int)];
            for (int i = 0; i < widthBbytes.Length; i++)
            {
                heightBytes[i] = br.ReadByte();
            }
            int height = BitConverter.ToInt32(heightBytes, 0);

            return new Vector2(width, height);
        }

        public static void GetPNGChunks(string filePath)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(filePath));
            br.BaseStream.Position = 8;
           
            Console.WriteLine($"Size: {br.BaseStream.Length}bytes");
            Console.WriteLine();
            Console.WriteLine("This PNG-file consists of: ");
            
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                int count = 0;

                count += 4;
                byte[] length = new byte[sizeof(int)];
                length = br.ReadBytes(4);
                Array.Reverse(length);
                int jumpToPosition = BitConverter.ToInt32(length, 0);

                count += 4;
                byte[] chunkType = new byte[sizeof(int)];
                chunkType = br.ReadBytes(4);
                string chunkName = Encoding.ASCII.GetString(chunkType);

                br.BaseStream.Position += jumpToPosition + (4);
                count += jumpToPosition + 4;

                Console.WriteLine($"{chunkName}: {count}bytes");
            }
        }
    }
}
