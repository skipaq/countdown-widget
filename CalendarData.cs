using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json; // ✅ Не забудь добавить через NuGet

namespace CountdownWidget
{
    /// <summary>
    /// График работы (вахта)
    /// </summary>
    public class WorkSchedule
    {
        public string Name { get; set; } = "Вахта";
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime End { get; set; } = DateTime.Now.AddDays(10);
        public string Color { get; set; } = "#00FF00"; // Зелёный по умолчанию
        public string Notification { get; set; } = "За 1 день";
    }

    /// <summary>
    /// Одиночное событие
    /// </summary>
    public class CalendarEvent
    {
        public string Name { get; set; } = "Событие";
        public DateTime Date { get; set; } = DateTime.Now;
        public string Color { get; set; } = "#FF0000"; // Красный по умолчанию
        public string Notification { get; set; } = "За 1 час";
        public string Repeat { get; set; } = "Нет"; // Нет / Ежедневно / Еженедельно / Ежемесячно
    }

    /// <summary>
    /// Данные календаря
    /// </summary>
    public class CalendarData
    {
        public List<WorkSchedule> Schedules { get; set; } = new List<WorkSchedule>();
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();

        /// <summary>
        /// Загружает данные из файла
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Экземпляр CalendarData</returns>
        public static CalendarData Load(string path = "calendar.json")
        {
            // ✅ Если файла нет — создаём пустой
            if (!File.Exists(path))
            {
                var data = new CalendarData();
                try
                {
                    data.Save(path); // Создаём пустой файл
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Не удалось создать calendar.json: {ex.Message}");
                }
                return data;
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<CalendarData>(json) ?? new CalendarData(); // ✅ Защита от null
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка чтения calendar.json: {ex.Message}");
                return new CalendarData(); // ✅ Возвращаем пустой объект
            }
        }

        /// <summary>
        /// Сохраняет данные в файл
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public void Save(string path = "calendar.json")
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Не удалось сохранить calendar.json: {ex.Message}");
            }
        }
    }
}