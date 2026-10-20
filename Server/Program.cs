using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    internal class Program
    {        
        static void Main(string[] args)
        {
            using (var db = new AutosalonNewEntities1())
            {
                var count = db.Cars.Count();
                Console.WriteLine($"В базе найдено машин: {count}");
            }

            Console.OutputEncoding = Encoding.UTF8;
            const int port = 5000; 
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                listener.Bind(endPoint);
                listener.Listen(10);
                Console.WriteLine($"Сервер ждет подключения на порту: {port}...");
            }
            catch (Exception ex) 
            {
                CarResponseFormatter.PrintError($"ERROR запуска сервера: {ex.Message}");
                return;
            }

            while (true)
            {
                Socket clientSoket = listener.Accept();
                CarResponseFormatter.PrintGreen($"Клиент подключился {clientSoket.RemoteEndPoint}");
                try
                {  
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSoket.Receive(buffer);

                    if (bytesRead == 0)
                    {
                        clientSoket.Close();
                        //continue;
                        break;
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    message = message.Trim();
                    Console.WriteLine($"Получено от клиента: {message}");

                    string responseSend;
                    string[] parts = message.Split('|');
                    if (parts.Length == 2 && parts[0] == "SEARCH_ID" && int.TryParse(parts[1], out int id))
                    {
                        using (var db = new AutosalonNewEntities1())
                        {
                            var car = db.Cars.Find(id);
                            if (car == null)
                            {
                                responseSend = CarResponseFormatter.Error("Авто с таким ID не найдено");
                            }
                            else
                            {
                                CarDto dto = new CarDto()
                                {
                                    Id = car.Id,
                                    Brand = car.CarName,
                                    Model = car.ModelName,
                                    Price = car.Price,
                                    Color = car.Color,
                                    Year = car.Year,
                                    Quantity = car.Quantity,
                                    IsAvailable = car.IsAvailable
                                };
                                string json = CarResponseFormatter.FormatSingle(dto);
                                responseSend = CarResponseFormatter.Success(json);
                            }
                        }
                    }
                    else if (parts.Length == 3 && parts[0] == "SEARCH_PRICE" &&
                         decimal.TryParse(parts[1], out decimal min) &&
                         decimal.TryParse(parts[2], out decimal max))
                    {
                        using (var db = new AutosalonNewEntities1())
                        {
                            var cars = db.Cars
                                .Where(c => c.Price >= min && c.Price <= max)
                                .ToList();

                            if (!cars.Any())
                            {
                                responseSend = CarResponseFormatter.Error("Ничего не найдено в этом диапазоне");
                                //return;
                            }
                            else
                            {
                                var dtoList = cars.Select(c => new CarDto
                                {
                                    Id = c.Id,
                                    Brand = c.CarName,
                                    Model = c.ModelName,
                                    Price = c.Price,
                                    Color = c.Color,
                                    Year = c.Year,
                                    Quantity = c.Quantity,
                                    IsAvailable = c.IsAvailable
                                }).ToList();
                                string json = CarResponseFormatter.FormatList(dtoList);
                                responseSend = CarResponseFormatter.Success(json);
                            }
                        }
                    }
                    //*********************
                    //3
                    else if (parts.Length == 3 && parts[0] == "SEARCH_BRAND_MODEL")
                    {
                        string[] searchParts = parts[1].Split('|');
                        string brand = searchParts.Length > 0 ? searchParts[0] : "";
                        string model = searchParts.Length > 1 ? searchParts[1] : "";

                        using (var db = new AutosalonNewEntities1())
                        {
                            var query = db.Cars.AsQueryable();

                            if (!string.IsNullOrWhiteSpace(brand))
                                query = query.Where(c => c.CarName.Contains(brand));

                            if (!string.IsNullOrWhiteSpace(model))
                                query = query.Where(c => c.ModelName.Contains(model));

                            var cars = query.ToList();

                            if (!cars.Any())
                                responseSend = CarResponseFormatter.Error("Не найдено по марке/модели");
                            else
                            {
                                var dtoList = cars.Select(c => new CarDto
                                {
                                    Id = c.Id,
                                    Brand = c.CarName,
                                    Model = c.ModelName,
                                    Price = c.Price,
                                    Color = c.Color,
                                    Year = c.Year,
                                    Quantity = c.Quantity,
                                    IsAvailable = c.IsAvailable
                                }).ToList();
                                responseSend = CarResponseFormatter.Success(CarResponseFormatter.FormatList(dtoList));
                            }
                        }
                    }
                    //4
                    else if (parts.Length == 2 && parts[0] == "REMOVE_SALE" && int.TryParse(parts[1], out int removeId))
                    {
                        using (var db = new AutosalonNewEntities1())
                        {
                            var car = db.Cars.Find(removeId);
                            if (car != null)
                            {
                                car.IsAvailable = false;
                                db.SaveChanges();
                                responseSend = CarResponseFormatter.Success("OK");
                            }
                            else
                            {
                                responseSend = CarResponseFormatter.Error("Автомобиль не найден");
                            }
                        }
                    }
                    // 5
                    else if (parts.Length >= 2 && parts[0] == "ADD_CAR")
                    {
                        string jsonData = string.Join("|", parts.Skip(1));
                        try
                        {
                            using (var db = new AutosalonNewEntities1())
                            {
                                var newCarDto = JsonSerializer.Deserialize<CarDto>(jsonData);
                                if (newCarDto == null)
                                {
                                    throw new Exception("Неверный формат JSON");
                                }

                                var newCar = new Cars
                                {
                                    CarName = newCarDto.Brand,
                                    ModelName = newCarDto.Model,
                                    Price = newCarDto.Price,
                                    Color = newCarDto.Color,
                                    Year = newCarDto.Year,
                                    Quantity = newCarDto.Quantity,
                                    IsAvailable = true
                                };

                                db.Cars.Add(newCar);
                                db.SaveChanges();

                                responseSend = CarResponseFormatter.Success("OK");
                            }
                        }
                        catch (JsonException ex)
                        {
                            responseSend = CarResponseFormatter.Error($"Ошибка JSON: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            responseSend = CarResponseFormatter.Error($"Ошибка добавления автомобиля: {ex.Message}");
                        }
                    }
                    //*********************
                    else if (parts.Length == 2 && parts[0] == "SELL_CAR" && int.TryParse(parts[1], out int sellId))
                    {
                        using (var db = new AutosalonNewEntities1())
                        {
                            var car = db.Cars.Find(sellId);
                            if (car == null)
                            {
                                responseSend = CarResponseFormatter.Error("Автомобиль не найден");
                            }
                            else if (!car.IsAvailable)
                            {
                                responseSend = CarResponseFormatter.Error("Автомобиль уже снят с продажи");
                            }
                            else if (car.Quantity <= 0)
                            {
                                responseSend = CarResponseFormatter.Error("Кол-во автомобилей на складе равно нулю");
                            }
                            else
                            {
                                try
                                {
                                    var soldRecord = new SoldCars
                                    {
                                        CarId = car.Id,
                                        SalePrice = car.Price,
                                        SaleDate = DateTime.Now,
                                        QuantitySold = 1
                                    };

                                    db.SoldCars.Add(soldRecord);
                                    car.Quantity--;

                                    if (car.Quantity == 0)
                                    {
                                        car.IsAvailable = false;
                                    }

                                    db.SaveChanges();
                                    responseSend = CarResponseFormatter.Success("OK");
                                }
                                catch (Exception ex)
                                {
                                    responseSend = CarResponseFormatter.Error($"Ошибка продажи: {ex.Message}");
                                }
                            }
                        }
                    }
                    //*************************
                    else if (parts.Length >= 2 && parts[0] == "UPDATE_CAR")
                    {
                        string jsonData = string.Join("|", parts.Skip(1));

                        try
                        {
                            using (var db = new AutosalonNewEntities1())
                            {
                                var updateDto = JsonSerializer.Deserialize<CarDto>(jsonData);

                                if (updateDto == null || updateDto.Id == 0)
                                {
                                    throw new Exception("Неверный формат данных");
                                }
                                var carToUpdate = db.Cars.FirstOrDefault(c => c.Id == updateDto.Id);

                                if (carToUpdate == null)
                                {
                                    responseSend = CarResponseFormatter.Error("Автомобиль с таким ID не найден");
                                    //return;
                                    break;
                                }
                                if (updateDto.Brand != null)
                                    carToUpdate.CarName = updateDto.Brand;
                                if (updateDto.Model != null)
                                    carToUpdate.ModelName = updateDto.Model;
                                if (updateDto.Price != 0)
                                    carToUpdate.Price = updateDto.Price;
                                if (updateDto.Color != null)
                                    carToUpdate.Color = updateDto.Color;
                                if (updateDto.Year != 0)
                                    carToUpdate.Year = updateDto.Year;
                                if (updateDto.Quantity != 0)
                                    carToUpdate.Quantity = updateDto.Quantity;

                                db.SaveChanges();
                                var resultDto = new CarDto
                                {
                                    Id = carToUpdate.Id,
                                    Brand = carToUpdate.CarName,
                                    Model = carToUpdate.ModelName,
                                    Price = carToUpdate.Price,
                                    Color = carToUpdate.Color,
                                    Year = carToUpdate.Year,
                                    Quantity = carToUpdate.Quantity,
                                    IsAvailable = carToUpdate.IsAvailable
                                };
                                responseSend = CarResponseFormatter.Success(CarResponseFormatter.FormatSingle(resultDto));
                            }
                        }
                        catch (JsonException ex)
                        {
                            responseSend = CarResponseFormatter.Error($"ERROR парсинга JSON: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            responseSend = CarResponseFormatter.Error($"ERROR обновления: {ex.Message}");
                        }
                    }
                    //*******************************                    
                    else
                    {
                        responseSend = CarResponseFormatter.Error("Невернo");
                    }
                    byte[] resposByte = Encoding.UTF8.GetBytes(responseSend);
                    clientSoket.Send(resposByte);
                    Console.WriteLine("Отправлено клиенту: " + responseSend.Substring(0, Math.Min(responseSend.Length, 100)) + "...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка обработки клиента: " + ex.Message);
                }
                finally
                {
                    clientSoket.Shutdown(SocketShutdown.Both);
                    clientSoket.Close();
                    CarResponseFormatter.PrintYellow("Соединение закрыто\n");
                }
            }

        }
    }
}
