using Common;
using Microsoft.Win32;    // For OpenFileDialog / SaveFileDialog
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace WPFApp
{
    public partial class MainWindow : Window
    {
        private OverlayManager manager = new OverlayManager();

        // Храним для каждого слоя: отдельный WPF-контрол Image + 2 точечных эллипса
        private Dictionary<int, Image> layerImages = new Dictionary<int, Image>();
        private Dictionary<int, Ellipse> layerPoint1 = new Dictionary<int, Ellipse>();
        private Dictionary<int, Ellipse> layerPoint2 = new Dictionary<int, Ellipse>();

        // Случайные цвета для точек
        private Dictionary<int, Brush> layerColors = new Dictionary<int, Brush>();

        // Трансформации для Canvas
        private TranslateTransform panTransform = new TranslateTransform(0, 0);
        private ScaleTransform zoomTransform = new ScaleTransform(1, 1);
        private TransformGroup transformGroup = new TransformGroup();

        private bool isPanning = false;
        private Point lastPanPoint;
        private double currentZoom = 1.0;

        public MainWindow()
        {
            InitializeComponent();

            // Canvas transform
            transformGroup.Children.Add(zoomTransform);
            transformGroup.Children.Add(panTransform);
            MainCanvas.RenderTransform = transformGroup;

            // События
            AddLayerButton.Click += AddLayerButton_Click;
            RemoveLayerButton.Click += RemoveLayerButton_Click;
            LayersListBox.SelectionChanged += LayersListBox_SelectionChanged;
            OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;

            MergeButton.Click += MergeButton_Click;
            ExportButton.Click += ExportButton_Click;
        }

        private void RefreshListBox()
        {
            LayersListBox.ItemsSource = null;
            LayersListBox.ItemsSource = manager.Layers; // DisplayMemberPath="FileName"
        }

        #region Add / Remove
        private void AddLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;
                int idx = manager.AddLayer(path, 1.0f, false);

                // Создаём WPF Image
                var layer = manager.Layers[idx];
                var wpfImage = CreateWpfImageFromBitmap(layer.Image);
                MainCanvas.Children.Add(wpfImage);
                layerImages[idx] = wpfImage;

                // Создаём 2 точки
                var randomColor = RandomBrush();
                layerColors[idx] = randomColor;

                var el1 = CreateEllipse(randomColor);
                var el2 = CreateEllipse(randomColor);
                MainCanvas.Children.Add(el1);
                MainCanvas.Children.Add(el2);

                layerPoint1[idx] = el1;
                layerPoint2[idx] = el2;

                // Обновим список
                RefreshListBox();
            }
        }

        private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            int idx = LayersListBox.SelectedIndex;
            if (idx >= 0 && idx < manager.Layers.Count)
            {
                manager.RemoveLayer(idx);

                // Пересоздать всё (т.к. индексы меняются)
                RebuildCanvasFromManager();
                RefreshListBox();
            }
        }
        #endregion

        #region RebuildCanvasFromManager
        private void RebuildCanvasFromManager()
        {
            MainCanvas.Children.Clear();
            layerImages.Clear();
            layerPoint1.Clear();
            layerPoint2.Clear();
            layerColors.Clear();

            for (int i = 0; i < manager.Layers.Count; i++)
            {
                var layer = manager.Layers[i];
                // Создаём Image
                var wpfImage = CreateWpfImageFromBitmap(layer.Image);
                wpfImage.Opacity = layer.Opacity;
                MainCanvas.Children.Add(wpfImage);
                layerImages[i] = wpfImage;

                // Случайный цвет
                var c = RandomBrush();
                layerColors[i] = c;

                // Точки
                var el1 = CreateEllipse(c);
                var el2 = CreateEllipse(c);
                MainCanvas.Children.Add(el1);
                MainCanvas.Children.Add(el2);
                layerPoint1[i] = el1;
                layerPoint2[i] = el2;

                // Обновим сразу позицию эллипсов
                UpdateEllipsePosition(i);
            }
        }
        #endregion

        #region Выбор слоя
        private void LayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = LayersListBox.SelectedIndex;
            if (idx >= 0 && idx < manager.Layers.Count)
            {
                var layer = manager.Layers[idx];
                OpacitySlider.Value = layer.Opacity;
            }
        }
        #endregion

        #region Изменение прозрачности
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int idx = LayersListBox.SelectedIndex;
            if (idx >= 0 && idx < manager.Layers.Count)
            {
                float val = (float)OpacitySlider.Value;
                manager.SetOpacity(idx, val);

                if (layerImages.TryGetValue(idx, out var img))
                {
                    img.Opacity = val;
                }
            }
        }
        #endregion


        #region Зум
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            zoomTransform.ScaleX = currentZoom;
            zoomTransform.ScaleY = currentZoom;
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (zoomTransform.ScaleX < 5.0)
                {
                    zoomTransform.ScaleX += 0.1;
                    zoomTransform.ScaleY += 0.1;
                }
            }
            else
            {
                if (zoomTransform.ScaleX > 0.1)
                {
                    zoomTransform.ScaleX -= 0.1;
                    zoomTransform.ScaleY -= 0.1;
                }
            }

            // Обновляем отображение процента зума
            ZoomLabel.Text = $"Зум: {(int)(zoomTransform.ScaleX * 100)}%";
            e.Handled = true;
        }

        #endregion

        #region Пан (ПКМ)
        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPanning = true;
            lastPanPoint = e.GetPosition(this);
            MainCanvas.CaptureMouse();
        }

        private void MainCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            MainCanvas.ReleaseMouseCapture();
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning && e.RightButton == MouseButtonState.Pressed)
            {
                var newPos = e.GetPosition(this);
                double dx = newPos.X - lastPanPoint.X;
                double dy = newPos.Y - lastPanPoint.Y;

                double newX = panTransform.X + dx;
                double newY = panTransform.Y + dy;

                // Простейшее ограничение, чтобы Canvas не "улетел" слишком далеко
                double minX = -1000, maxX = 1000;
                double minY = -1000, maxY = 1000;
                if (newX < minX) newX = minX;
                if (newX > maxX) newX = maxX;
                if (newY < minY) newY = minY;
                if (newY > maxY) newY = maxY;

                panTransform.X = newX;
                panTransform.Y = newY;

                lastPanPoint = newPos;
            }
        }
        #endregion

        #region Ставим точки (ЛКМ)
        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int idx = LayersListBox.SelectedIndex;
            if (idx < 0 || idx >= manager.Layers.Count) return;

            var layer = manager.Layers[idx];

            // Вычислим "реальные" координаты внутри Canvas (до масштабов, пан).
            var clickPos = e.GetPosition(MainCanvas);
            var matrix = transformGroup.Value;
            matrix.Invert();
            var pts = new Point[] { clickPos };
            matrix.Transform(pts);
            Point realPos = pts[0];

            // Логика: "При наличии двух точек и повторном нажатии просто переопределяем сначала первую, потом вторую"
            // Считаем, что ChildPoint1 -> ChildPoint2 -> ChildPoint1 -> ...
            var p1 = layer.LocalPoint1;
            var p2 = layer.LocalPoint2;

            bool p1Empty = (p1.X == 0 && p1.Y == 0 && p2.X == 0 && p2.Y == 0);
            bool p2Empty = (!p1Empty && p2.X == 0 && p2.Y == 0);

            if (p1Empty || layer.IsOddPoint)
            {
                // Ставим первую точку
                manager.SetLocalPoints(idx, new System.Drawing.PointF((float)realPos.X, (float)realPos.Y), p2);
                layer.IsOddPoint = false;
            }
            else if (p2Empty || !layer.IsOddPoint)
            {
                // Ставим вторую
                manager.SetLocalPoints(idx, p1, new System.Drawing.PointF((float)realPos.X, (float)realPos.Y));
                layer.IsOddPoint = true;
            }

            // Обновим эллипсы
            UpdateEllipsePosition(idx);
        }

        private void UpdateEllipsePosition(int idx)
        {
            if (idx < 0 || idx >= manager.Layers.Count) return;
            var layer = manager.Layers[idx];
            if (!layerPoint1.ContainsKey(idx)) return;

            // берем p1, p2
            var p1 = layer.LocalPoint1;
            var p2 = layer.LocalPoint2;

            // Применить transformGroup, чтобы найти "экранное" положение.
            // Но transformGroup — это "мировая" матрица, а нам нужно прямое преобразование (без инверта).
            // Идея: у нас есть "логическая" точка (p1). Чтобы узнать "экранную", 
            //   берём точку p1 и умножаем её на transformGroup.

            var matrix = transformGroup.Value; // WPF Matrix
            // Наоборот, это скейл + перенос "в экран". Но matrix сейчас Pan+Zoom.
            // Чтобы "логическую" координату (p1.X, p1.Y) "превратить" в экранную, 
            // нужно matrix.Transform(...). 
            // (в MouseLeftButtonUp мы делали обратное - инвертировали).

            // Создадим Point
            var p1Point = new Point(p1.X, p1.Y);
            var p2Point = new Point(p2.X, p2.Y);

            var arr1 = new Point[] { p1Point, p2Point };
            matrix.Transform(arr1);

            Point p1Screen = arr1[0];
            Point p2Screen = arr1[1];

            // Установим Canvas.Left/Top для эллипсов
            if (layerPoint1.TryGetValue(idx, out var e1))
            {
                Canvas.SetLeft(e1, p1Screen.X - e1.Width / 2);
                Canvas.SetTop(e1, p1Screen.Y - e1.Height / 2);
                e1.Visibility = (p1.X == 0 && p1.Y == 0 && p2.X == 0 && p2.Y == 0) ? Visibility.Hidden : Visibility.Visible;
            }
            if (layerPoint2.TryGetValue(idx, out var e2))
            {
                Canvas.SetLeft(e2, p2Screen.X - e2.Width / 2);
                Canvas.SetTop(e2, p2Screen.Y - e2.Height / 2);
                e2.Visibility = (p2.X == 0 && p2.Y == 0) ? Visibility.Hidden : Visibility.Visible;
            }
        }
        #endregion

        #region Merge / Export
        private void MergeButton_Click(object sender, RoutedEventArgs e)
        {
            // 1) Ищем базовый слой
            var baseLayer = manager.Layers.FirstOrDefault(l => l.IsBase);
            if (baseLayer == null && manager.Layers.Count > 0)
            {
                baseLayer = manager.Layers[0]; // если ни один не помечен IsBase
            }
            if (baseLayer == null)
            {
                MessageBox.Show("Нет слоёв для наложения!");
                return;
            }

            // 2) Получаем индекс базового слоя
            int baseIndex = 0;

            for (int i = 0; i < manager.Layers.Count; i++)
            {
                if (manager.Layers[i].IsBase)
                {
                    baseIndex = i;
                    break;
                }
            }

                // 3) Пробегаем по всем слоям
                for (int i = 0; i < manager.Layers.Count; i++)
                {
                    // Пропускаем базовый слой (он остаётся как есть)
                    if (i == baseIndex) continue;

                    // 4) Считаем WPF-матрицу
                    var wpfMatrix = ComputeTransformForWpf(baseLayer, manager.Layers[i]);

                    // 5) Применяем к Image
                    if (layerImages.TryGetValue(i, out var imageControl))
                    {
                        imageControl.RenderTransform = new MatrixTransform(wpfMatrix);
                    }
                }

            MessageBox.Show("Слои наложены, результат на экране!");
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp",
                FileName = "result.png"
            };

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;
                using (var bmp = manager.MergeAll())
                {
                    // Сохраняем
                    var ext = System.IO.Path.GetExtension(dlg.FileName)?.ToLower();
                    switch (ext)
                    {
                        case ".jpg":
                        case ".jpeg":
                            bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case ".bmp":
                            bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        default:
                            bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                    }
                }

                MessageBox.Show($"Сохранено: {path}");
            }
        }
        #endregion

        #region Утилиты
        private Image CreateWpfImageFromBitmap(System.Drawing.Bitmap bmp)
        {
            // Чтобы не усложнять жизнь конвертацией из Bitmap в BitmapSource через потоки,
            // можно загрузить напрямую из файла. Но здесь bmp - уже в памяти.
            // Покажем классический способ (MemoryStream):
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            var bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.CacheOption = BitmapCacheOption.OnLoad;
            bImg.StreamSource = ms;
            bImg.EndInit();

            var img = new Image();
            img.Source = bImg;
            img.Width = bImg.PixelWidth;
            img.Height = bImg.PixelHeight;
            return img;
        }

        private Ellipse CreateEllipse(Brush color)
        {
            var el = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = color,
                Stroke = System.Windows.Media.Brushes.White,
                StrokeThickness = 1,
                Visibility = Visibility.Hidden // по умолчанию прячем
            };
            return el;
        }

        private Brush RandomBrush()
        {
            var rnd = new Random();
            var c = Color.FromRgb((byte)rnd.Next(20, 256), (byte)rnd.Next(20, 256), (byte)rnd.Next(20, 256));
            return new SolidColorBrush(c);
        }
        #endregion

        private System.Windows.Media.Matrix ComputeTransformForWpf(OverlayLayer baseL, OverlayLayer childL)
        {
            // Из "базового" возьмём A_b, B_b
            var Ab = baseL.LocalPoint1;
            var Bb = baseL.LocalPoint2;

            // Из "child"
            var Ai = childL.LocalPoint1;
            var Bi = childL.LocalPoint2;

            // Считаем масштаб
            float distChild = Distance(Ai, Bi);
            float distBase = Distance(Ab, Bb);
            float scale = (distChild == 0) ? 1.0f : (distBase / distChild);

            // Угол
            float angleChild = (float)Math.Atan2(Bi.Y - Ai.Y, Bi.X - Ai.X);
            float angleBase = (float)Math.Atan2(Bb.Y - Ab.Y, Bb.X - Ab.X);
            float rotationDeg = (angleBase - angleChild) * 180f / (float)Math.PI;

            // Строим матрицу WPF
            // порядок операций: 
            //    Translate(-Ai), Scale, Rotate, Translate(+Ab)
            var m = System.Windows.Media.Matrix.Identity;

            // Translate(-Ai)
            m.Translate(-Ai.X, -Ai.Y);
            // Scale
            m.Scale(scale, scale);
            // Rotate
            var centerRot = new System.Windows.Media.RotateTransform(rotationDeg);
            var rotMatrix = centerRot.Value; // WPF Matrix
            m = System.Windows.Media.Matrix.Multiply(m, rotMatrix);
            // Translate(+Ab)
            m.Translate(Ab.X, Ab.Y);

            return m;
        }

        // Вспомогательная Distance
        private float Distance(System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
