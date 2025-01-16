using System;
using System.IO;
using System.Drawing;
using Common;
using System.Reflection.Emit;

namespace OverlayConsole
{
    class Program
    {
        private static OverlayManager manager = new OverlayManager();

        static void Main()
        {
            while (true)
            {
                ShowMenu();
                Console.Write("\nВведите команду: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var cmd = input.Trim().ToLower();
                switch (cmd)
                {
                    case "add":
                        CmdAddLayer();
                        break;
                    case "list":
                        CmdListLayers();
                        break;
                    case "setbase":
                        CmdSetBase();
                        break;
                    case "setpoints":
                        CmdSetPoints();
                        break;
                    case "setopacity":
                        CmdSetOpacity();
                        break;
                    case "remove":
                        CmdRemoveLayer();
                        break;
                    case "merge":
                        CmdMergeAll();
                        break;
                    case "exit":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("\n=== МЕНЮ КОМАНД ===");
            Console.WriteLine("[add]       - Добавить слой");
            Console.WriteLine("[list]      - Показать слои");
            Console.WriteLine("[setbase]   - Пометить слой как базовый");
            Console.WriteLine("[setpoints] - Задать/изменить две локальные точки для слоя");
            Console.WriteLine("[setopacity]- Задать прозрачность для слоя");
            Console.WriteLine("[remove]    - Удалить слой");
            Console.WriteLine("[merge]     - Слить все слои в одно изображение и сохранить");
            Console.WriteLine("[exit]      - Выход из программы");
        }

        /// <summary>
        /// Добавляем слой с возможностью указать:
        /// - путь к файлу,
        /// - прозрачность,
        /// - флаг IsBase,
        /// - координаты двух точек.
        /// </summary>
        static void CmdAddLayer()
        {
            Console.Write("Введите путь к изображению: ");
            var path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Путь не указан, отмена добавления слоя.");
                return;
            }

            Bitmap? image = null;

            try {
                image = new Bitmap(path);
                Console.WriteLine($"Размер изображения: {image.Width} x {image.Height} пикселей.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при добавлении слоя: " + ex.Message);
            }

            Console.Write("Указать прозрачность (0..1) [по умолч. 1,0]: ");
            var opStr = Console.ReadLine();
            float opacity = 1.0f;
            if (!string.IsNullOrWhiteSpace(opStr))
            {
                if (!float.TryParse(opStr, out opacity))
                {
                    Console.WriteLine("Неверное значение, берем прозрачность = 1,0");
                    opacity = 1.0f;
                }
            }

            Console.Write("Сделать этот слой базовым? [y/n, по умолч. n]: ");
            var baseStr = Console.ReadLine();
            bool isBase = (baseStr?.Trim().ToLower() == "y");

            // Сразу спросим у пользователя, хочет ли он ввести локальные точки.
            // Скажем, если пользователь ничего не вводит, ставим (0,0).
            PointF p1 = new PointF(0, 0);
            PointF p2 = new PointF(0, 0);

            Console.Write("Введите координаты первой локальной точки (x,y) [по умолч. (0,0)]: ");
            var l1 = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(l1))
            {
                if (!ParsePointF(l1, out p1))
                {
                    Console.WriteLine("Неверный формат, будет (0,0).");
                    p1 = new PointF(0, 0);
                }
            }

            Console.Write("Введите координаты второй локальной точки (x,y) [по умолч. (0,0)]: ");
            var l2 = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(l2))
            {
                if (!ParsePointF(l2, out p2))
                {
                    Console.WriteLine("Неверный формат, будет (0,0).");
                    p2 = new PointF(0, 0);
                }
            }

            try
            {
                int idx = manager.AddLayer(image, opacity, isBase, p1, p2);
                var layer = manager.Layers[idx];
                Console.WriteLine($"Слой добавлен с индексом {idx}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при добавлении слоя: " + ex.Message);
            }
        }

        /// <summary>
        /// Отображаем список слоёв.
        /// </summary>
        static void CmdListLayers()
        {
            var layers = manager.Layers;
            if (layers.Count == 0)
            {
                Console.WriteLine("Слоёв нет.");
                return;
            }

            for (int i = 0; i < layers.Count; i++)
            {
                var l = layers[i];
                Console.WriteLine($"[{i}] Base={l.IsBase}, Opacity={l.Opacity}, " +
                                  $"LocalPoints=({l.LocalPoint1.X},{l.LocalPoint1.Y}) / ({l.LocalPoint2.X},{l.LocalPoint2.Y}), " +
                                  $"Size={l.Image.Width}x{l.Image.Height}");
            }
        }

        static void CmdSetBase()
        {
            Console.Write("Введите индекс слоя, который будет базовым: ");
            var idxStr = Console.ReadLine();
            if (!int.TryParse(idxStr, out int idx))
            {
                Console.WriteLine("Неверный индекс");
                return;
            }

            try
            {
                manager.SetBaseLayer(idx, true);
                Console.WriteLine($"Слой {idx} теперь помечен как базовый (IsBase=true).");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        /// <summary>
        /// Установить две локальные точки слоя.
        /// </summary>
        static void CmdSetPoints()
        {
            Console.Write("Введите индекс слоя: ");
            var idxStr = Console.ReadLine();
            if (!int.TryParse(idxStr, out int idx))
            {
                Console.WriteLine("Неверный индекс");
                return;
            }

            Console.Write("Введите координаты первой точки (x,y): ");
            var p1Str = Console.ReadLine();
            Console.Write("Введите координаты второй точки (x,y): ");
            var p2Str = Console.ReadLine();

            if (!ParsePointF(p1Str, out PointF p1) || !ParsePointF(p2Str, out PointF p2))
            {
                Console.WriteLine("Неверный формат координат. Пример: 50,100");
                return;
            }

            try
            {
                manager.SetLocalPoints(idx, p1, p2);
                Console.WriteLine($"Локальные точки для слоя {idx} установлены: ({p1.X},{p1.Y}) и ({p2.X},{p2.Y}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        /// <summary>
        /// Установить прозрачность слоя.
        /// </summary>
        static void CmdSetOpacity()
        {
            Console.Write("Введите индекс слоя: ");
            var idxStr = Console.ReadLine();
            if (!int.TryParse(idxStr, out int idx))
            {
                Console.WriteLine("Неверный индекс");
                return;
            }

            Console.Write("Введите новую прозрачность (0..1): ");
            var opStr = Console.ReadLine();
            if (!float.TryParse(opStr, out float opacity))
            {
                Console.WriteLine("Неверный формат прозрачности.");
                return;
            }

            try
            {
                manager.SetOpacity(idx, opacity);
                Console.WriteLine($"Прозрачность слоя {idx} теперь {opacity}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        static void CmdRemoveLayer()
        {
            Console.Write("Введите индекс слоя для удаления: ");
            var idxStr = Console.ReadLine();
            if (!int.TryParse(idxStr, out int idx))
            {
                Console.WriteLine("Неверный индекс");
                return;
            }

            try
            {
                manager.RemoveLayer(idx);
                Console.WriteLine($"Слой {idx} удалён.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        /// <summary>
        /// Слить все слои в одно изображение и сохранить.
        /// Если пользователь не вводит путь, берём "result.png" по умолчанию.
        /// После сохранения выводим абсолютный путь.
        /// </summary>
        static void CmdMergeAll()
        {
            Console.Write("Введите путь для сохранения результата (по умолчанию 'result.png'): ");
            var path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "result.png";
            }

            try
            {
                using var result = manager.MergeAll();
                result.Save(path);

                // Получим полный путь
                var fullPath = Path.GetFullPath(path);
                Console.WriteLine($"Итоговое изображение сохранено в: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при слиянии или сохранении: " + ex.Message);
            }
        }

        /// <summary>
        /// Парсим строку вида "x,y" в PointF
        /// </summary>
        static bool ParsePointF(string input, out PointF pt)
        {
            pt = new PointF();
            if (string.IsNullOrWhiteSpace(input)) return false;

            var parts = input.Split(',');
            if (parts.Length != 2) return false;

            if (!float.TryParse(parts[0], out float x)) return false;
            if (!float.TryParse(parts[1], out float y)) return false;

            pt = new PointF(x, y);
            return true;
        }
    }
}
