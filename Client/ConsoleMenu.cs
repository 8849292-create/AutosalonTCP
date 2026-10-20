using Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text.Json;

namespace Client
{
    public class ConsoleMenu
    {        
        public void SearchById()
        {
            Console.Write("Введите ID автомобиля: ");
            if(int.TryParse(Console.ReadLine(), out int id))
            {
                string response = Program.SendCommand($"SEARCH_ID|{id}");
                ResponceProcess(response, false);
            }
            else
            {
                Console.WriteLine("ERROR: ID должен быть числом");
            }
        }
        public void SearchPriceRange()
        {
            Console.Write("Минимальная цена: ");
            decimal minPrice = (decimal.TryParse(Console.ReadLine(), out decimal min) ?  min : 0);
            Console.Write("Максимальная цена: ");
            decimal maxPrice = (decimal.TryParse(Console.ReadLine(), out decimal max) ? max : 999999999);

            if(minPrice>maxPrice|| minPrice < 0)
            {
                Console.WriteLine("Error: всегда должно быть max>min");
                return;
            }
            string response = Program.SendCommand($"SEARCH_PRICE|{minPrice}|{maxPrice}");
            ResponceProcess(response, true);
        }
        public void SearchBrandAndModel()
        {
            Console.Write("Вводим название марки -> ");
            string brand = Console.ReadLine()?.ToLower().Trim();
            Console.Write("Вводим название модели -> ");
            string model = Console.ReadLine()?.ToLower().Trim();

            string payLoad = $"{brand}|{model}";
            string response = Program.SendCommand($"SEARCH_BRAND_MODEL|{payLoad}");
            ResponceProcess(response, true);
        }
        public void RemoveFromSale()
        {
            Console.Write(" Вводим ID машины для снятия с продажи: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Неправильно"); return;
            }
            string response = Program.SendCommand($"REMOVE_SALE|{id}");
            CarResponseFormatter.PrintGreen(response);
        }
        public void AddCar()
        {
            Console.Write("Марка: ");
            string brand = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(brand))
            {
                Console.WriteLine("EMPTY"); return;
            }

            Console.Write("Модель: ");
            string model = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(model))
            {
                Console.WriteLine("EMPTY"); return;
            }

            Console.Write("Цена: ");
            if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Any,
                CultureInfo.InvariantCulture, out decimal price)) return;

            Console.Write("Цвет: ");
            string colorname = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(colorname))
            {
                Console.WriteLine("EMPTY"); return;
            }

            Console.Write("Год: ");
            if (!int.TryParse(Console.ReadLine(), out int year)) return;
            
            Console.Write("Количество: ");
            int qty = int.TryParse(Console.ReadLine(), out int q) ? q : 1;

            var dto = new CarDto
            {
                Brand = brand,
                Model = model,
                Price = price,
                Color = colorname,
                Year = year,
                Quantity = qty,
                IsAvailable = true
            };
            string json = JsonSerializer.Serialize(dto);
            string responsw = Program.SendCommand($"ADD_CAR|{json}");
            CarResponseFormatter.PrintGreen(responsw);
        }
        public void SellCar()
        {
            Console.Write("Вводим ID продаваемого автомобиля -> ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                string checkAuto = Program.SendCommand($"SEARCH_ID|{id}");
                if (!checkAuto.StartsWith("RESULT"))
                {
                    Console.WriteLine($"Авто не найдено. Ответ сервера: {checkAuto}");
                    return;
                }
                try
                {
                    string jsonPart = checkAuto.Substring(7);
                    var car = JsonSerializer.Deserialize<CarDto>(jsonPart);
                    if (car == null || !car.IsAvailable)
                    {
                        Console.WriteLine("Авто не найдено");
                        return;
                    }
                    if (car.Quantity <= 0)
                    {
                        Console.WriteLine("Нет автомобилей этой модели");
                        return;
                    }
                    Console.WriteLine($"\n--- Подтверждение продажи ---");
                    Console.WriteLine($"Авто: {car.Brand} {car.Model} (ID: {car.Id})");
                    Console.WriteLine($"Текущая цена: {car.Price}");
                    Console.WriteLine($"На складе: {car.Quantity} шт.");
                    Console.Write("Действие (Y/N): ");
                    //Console.WriteLine("  1. В таблице SoldCars появится запись (CarId={car.Id}, SalePrice={car.Price}, Qty=1).");
                    //Console.WriteLine("  2. В таблице Cars количество уменьшится на 1.");
                    //Console.WriteLine("  3. Если кол-во станет 0, авто снимется с продажи.");

                    // Console.WriteLine($"Подтверждение продажи авто ID:{car.Id} ({car.Brand} {car.Model})? (y/n)");
                    if (Console.ReadLine()?.ToLower().Trim() != "y")
                    {
                        Console.WriteLine("Продажа отменена");
                        return;
                    }

                    string response = Program.SendCommand($"SELL_CAR|{id}");
                    CarResponseFormatter.PrintGreen(response);
                }
                catch (Exception ex)
                {
                    CarResponseFormatter.PrintError($"Ошибка обработки данных авто: {ex.Message}");
                }
            }
        }
        public void UpdateCar()
        {
            Console.Write("Id модели для обновления: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            Console.Write("Новая марка: ");
            string brand = Console.ReadLine();
            //if (!string.IsNullOrWhiteSpace(brand))
            //{
            //    brand = brand.Trim(); 
            //}
            //else { return; }

            Console.Write("Новая модель: ");
            string model = Console.ReadLine();
            //if (!string.IsNullOrWhiteSpace(model))
            //{
            //    model = model.Trim();
            //}
            //else { return; }

            Console.Write("Новая цена: ");
            string priceStr = Console.ReadLine();

            decimal? price = null;
            if (!string.IsNullOrWhiteSpace(priceStr) && decimal.TryParse(priceStr, out var p))
                price = p;

            var updateDto = new UpdateCarDto
            {
                Id = id,
                Brand = string.IsNullOrWhiteSpace(brand) ? null : brand,
                Model = string.IsNullOrWhiteSpace(model) ? null : model,
                Price = price
                //Id = id,
                //Brand = brand,
                //Model = model,
                //Price = price
            };
            string json = JsonSerializer.Serialize(updateDto);
            string response = Program.SendCommand($"UPDATE_CAR|{json}");
            CarResponseFormatter.PrintError(response);
        }

        //-------------
        public void ResponceProcess(string rawresponce, bool isList)
        {
            if (string.IsNullOrEmpty(rawresponce))
            {
                Console.WriteLine("Empty string");
                return;
            }
            if(rawresponce.StartsWith("ERROR|"))
            {
                string errorMsg = rawresponce.Substring("ERROR|".Length);
                CarResponseFormatter.PrintError($"ERROR: {errorMsg}");
            }
            else if(rawresponce.StartsWith("RESULT|"))
            {
                try
                {
                    string jsonPart = rawresponce.Substring(7);
                    if (isList)
                    {
                        var car = JsonSerializer.Deserialize<List<CarDto>>(jsonPart);
                        if (car != null && car.Count>0)
                        {
                            Console.WriteLine("\n---- Найдены авто ----");

                            foreach (var c in car)
                            {
                                Console.WriteLine($"Авто: (ID={c.Id}), {c.Brand} {c.Model}, " +
                                                  $"Цена: {c.Price}, " +
                                                  $"Кол-во: {c.Quantity}, " +
                                                  $"Доступно: {c.IsAvailable}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Авто не найдено в таком диапазоне ");
                        }
                    }
                    else
                    {
                        var car = JsonSerializer.Deserialize<CarDto>(jsonPart);

                        if (car != null)
                        {                            
                            CarResponseFormatter.PrintGreen("\n---- Найден авто ----");                            

                            Console.WriteLine($"{car.Brand} {car.Model} (ID: {car.Id})  " +
                                              $"\nЦвет: {car.Color}, Год: {car.Year} " +
                                              $"\nЦена: {car.Price}, Доступно: {car.IsAvailable} " +
                                              $"\nОстаток (кол-во): {car.Quantity}");
                            Console.WriteLine(new string('-',50));
                        }
                        else
                        {
                            Console.WriteLine("\n Авто не найдена");
                        }
                    }
                }                
                catch(Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Неизвестный формат: {rawresponce}");
            }

        }

    }
}