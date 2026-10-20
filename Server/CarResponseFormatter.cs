using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Server
{            
    public class CarDto
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public string Color { get; set; }
        public int Year { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
    }
    public static class CarResponseFormatter
    {            
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(
                System.Text.Unicode.UnicodeRanges.BasicLatin,
                System.Text.Unicode.UnicodeRanges.Cyrillic),
            WriteIndented = false 
        };               
        public static string FormatSingle(CarDto dto) => JsonSerializer.Serialize(dto, Options);
                
        public static string FormatList(List<CarDto> list) => JsonSerializer.Serialize(list, Options);
                
        public static string Success(string json) => "RESULT|" + json;
        public static string Error(string msg) => "ERROR|" + msg;

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void PrintGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void PrintYellow(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
    public class UpdateCarDto
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal? Price { get; set; }
    }



}

