using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Common
{
    /// <summary>
    /// Менеджер, хранящий список слоёв (OverlayLayer), и умеющий "склеивать" их в одно итоговое изображение.
    /// Смысл в том, что ровно один слой должен быть IsBase=true, а остальные будут совмещаться по 
    /// своим двум локальным точкам к двум локальным точкам базового слоя.
    /// </summary>
    public class OverlayManager
    {
        private readonly List<OverlayLayer> _layers = new();
        public IReadOnlyList<OverlayLayer> Layers => _layers;

        /// <summary>
        /// Перегрузка, если хотим сразу передать готовый Bitmap.
        /// </summary>
        public int AddLayer(Bitmap bmp, float opacity = 1.0f, bool isBase = false,
                            PointF? localPoint1 = null, PointF? localPoint2 = null)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp));

            var layer = new OverlayLayer
            {
                Image = bmp,
                Opacity = opacity,
                IsBase = isBase,
            };

            // Если пользователь уже указал локальные точки при добавлении —
            // сразу задаём их. Иначе оставляем (0,0)/(0,0).
            if (localPoint1.HasValue) layer.LocalPoint1 = localPoint1.Value;
            if (localPoint2.HasValue) layer.LocalPoint2 = localPoint2.Value;

            _layers.Add(layer);

            return _layers.Count - 1;
        }

        /// <summary>
        /// Добавить слой из файла.
        /// </summary>
        public int AddLayer(string imagePath, float opacity = 1.0f, bool isBase = false,
                            PointF? localPoint1 = null, PointF? localPoint2 = null)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("Путь к изображению не может быть пустым.");

            var bmp = new Bitmap(imagePath);

            var layer = new OverlayLayer
            {
                FileName = Path.GetFileName(imagePath),
                Image = bmp,
                Opacity = opacity,
                IsBase = isBase,
            };

            // Если пользователь уже указал локальные точки при добавлении —
            // сразу задаём их. Иначе оставляем (0,0)/(0,0).
            if (localPoint1.HasValue) layer.LocalPoint1 = localPoint1.Value;
            if (localPoint2.HasValue) layer.LocalPoint2 = localPoint2.Value;

            _layers.Add(layer);
            return _layers.Count - 1;
        }

        /// <summary>
        /// Удалить слой по индексу
        /// </summary>
        public void RemoveLayer(int index)
        {
            if (index < 0 || index >= _layers.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var layer = _layers[index];
            layer.Image.Dispose(); // при желании освобождаем ресурсы
            _layers.RemoveAt(index);
        }

        /// <summary>
        /// Поменять местами слои (меняет порядок отрисовки).
        /// </summary>
        public void MoveLayer(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= _layers.Count ||
                newIndex < 0 || newIndex >= _layers.Count)
                throw new ArgumentOutOfRangeException("Индекс слоя вне допустимых границ");

            var layer = _layers[oldIndex];
            _layers.RemoveAt(oldIndex);
            _layers.Insert(newIndex, layer);
        }

        /// <summary>
        /// Установить локальные точки слоя (на самом изображении).
        /// </summary>
        public void SetLocalPoints(int index, PointF p1, PointF p2)
        {
            if (index < 0 || index >= _layers.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _layers[index].LocalPoint1 = p1;
            _layers[index].LocalPoint2 = p2;
        }

        /// <summary>
        /// Задать/снять флаг IsBase для указанного слоя (вдруг нужно переназначить базовый слой).
        /// При этом стоит учесть, что, по логике, базовым должен быть только один слой.
        /// </summary>
        public void SetBaseLayer(int index, bool isBase)
        {
            if (index < 0 || index >= _layers.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (isBase)
            {
                // только один может быть базовым
                foreach (var l in _layers)
                    l.IsBase = false;
            }

            _layers[index].IsBase = isBase;
        }

        /// <summary>
        /// Задать прозрачность слоя.
        /// </summary>
        public void SetOpacity(int index, float opacity)
        {
            if (index < 0 || index >= _layers.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (opacity < 0f || opacity > 1f)
                throw new ArgumentOutOfRangeException(nameof(opacity));

            _layers[index].Opacity = opacity;
        }

        /// <summary>
        /// Удалить все слои
        /// </summary>
        public void ClearAllLayers()
        {
            foreach (var l in _layers)
            {
                l.Image.Dispose();
            }

            _layers.Clear();
        }

        /// <summary>
        /// "Склеить" слои в одно итоговое изображение с учётом аффинного совмещения
        /// "child points" → "base points".
        /// </summary>
        public Bitmap MergeAll()
        {
            if (_layers.Count == 0)
            {
                return new Bitmap(1, 1); // пустышка
            }

            // Ищем наш "базовый" слой. Предположим, что он либо один, либо вообще нет (тогда возьмём первый).
            var baseLayer = _layers.Find(x => x.IsBase);
            if (baseLayer == null)
            {
                // Если не нашли, считаем, что базовый = первый в списке
                baseLayer = _layers[0];
            }

            // 1) Определим габариты будущей итоговой картинки.
            //    Базовый слой кладётся в (0,0). Т.е. его левый верхний угол → (0,0).
            //    Все child-слои трансформируются поверх.
            //    Нужно пройтись по всем слоям, вычислить все угловые точки после трансформации,
            //    чтобы найти minX, maxX, minY, maxY.
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            // Сначала учтём сам базовый слой (без трансформации, в (0,0)).
            {
                float w = baseLayer.Image.Width;
                float h = baseLayer.Image.Height;
                // Углы: (0,0), (w,0), (0,h), (w,h).
                // Они в результ. изображении такие же, так как нет смещений/масштабов.
                if (0 < minX) minX = 0;
                if (0 < minY) minY = 0;
                if (w > maxX) maxX = w;
                if (h > maxY) maxY = h;
            }

            // Затем учтём все child-слои
            foreach (var layer in _layers)
            {
                if (ReferenceEquals(layer, baseLayer))
                    continue; // это базовый, уже учтён

                // Считаем аффинную матрицу (child -> base), 
                // затем трансформируем его 4 угла, чтобы понять границы.
                using var matrix = ComputeChildToBaseTransform(baseLayer, layer);

                PointF[] corners =
                {
                    new PointF(0, 0),
                    new PointF(layer.Image.Width, 0),
                    new PointF(0, layer.Image.Height),
                    new PointF(layer.Image.Width, layer.Image.Height)
                };
                matrix.TransformPoints(corners);

                foreach (var pt in corners)
                {
                    if (pt.X < minX) minX = pt.X;
                    if (pt.X > maxX) maxX = pt.X;
                    if (pt.Y < minY) minY = pt.Y;
                    if (pt.Y > maxY) maxY = pt.Y;
                }
            }

            int outWidth = (int)Math.Ceiling(maxX - minX);
            int outHeight = (int)Math.Ceiling(maxY - minY);
            if (outWidth < 1) outWidth = 1;
            if (outHeight < 1) outHeight = 1;

            // 2) Создаём результирующий Bitmap
            var result = new Bitmap(outWidth, outHeight, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(result);
            g.Clear(Color.Transparent);
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // 3) Отрисовываем сперва базовый слой без трансформации,
            //    но с учётом того, что (minX, minY) надо сдвинуть в (0,0).
            {
                using var ia = MakeOpacityAttributes(baseLayer.Opacity);
                // Сдвигаем всю сцену, чтобы (minX, minY) оказались в (0,0)
                var oldTransform = g.Transform;
                try
                {
                    var shiftMatrix = new Matrix();
                    shiftMatrix.Translate(-minX, -minY);
                    g.MultiplyTransform(shiftMatrix);

                    var r = new Rectangle(0, 0, baseLayer.Image.Width, baseLayer.Image.Height);
                    g.DrawImage(baseLayer.Image, r, 0, 0, r.Width, r.Height, GraphicsUnit.Pixel, ia);
                }
                finally
                {
                    g.Transform = oldTransform;
                }
            }

            // 4) Отрисовываем все child-слои
            foreach (var layer in _layers)
            {
                if (ReferenceEquals(layer, baseLayer))
                    continue;

                using var ia = MakeOpacityAttributes(layer.Opacity);
                var matrix = ComputeChildToBaseTransform(baseLayer, layer);

                // Добавим смещение, чтобы подвинуть (minX, minY) в (0,0)
                matrix.Translate(-minX, -minY, MatrixOrder.Append);

                var oldTransform = g.Transform;
                try
                {
                    g.MultiplyTransform(matrix, MatrixOrder.Prepend);

                    var r = new Rectangle(0, 0, layer.Image.Width, layer.Image.Height);
                    g.DrawImage(layer.Image, r, 0, 0, r.Width, r.Height, GraphicsUnit.Pixel, ia);
                }
                finally
                {
                    g.Transform = oldTransform;
                }
            }

            return result;
        }

        /// <summary>
        /// Построить ImageAttributes, учитывающие прозрачность (Opacity).
        /// </summary>
        private ImageAttributes MakeOpacityAttributes(float opacity)
        {
            var ia = new ImageAttributes();
            float[][] cm =
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, opacity, 0},
                new float[] {0, 0, 0, 0, 1}
            };
            var colorMatrix = new ColorMatrix(cm);
            ia.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            return ia;
        }

        /// <summary>
        /// Построить матрицу трансформации для child-слоя,
        /// чтобы его локальные точки A_i и B_i "сели" на локальные точки A_b, B_b базового слоя.
        /// 
        /// - Базовый слой (baseLayer) не двигается, мы используем его (LocalPoint1, LocalPoint2) как целевые координаты.
        /// - Child-слой (childLayer) двигаем (масштаб/поворот/сдвиг), чтобы LocalPoint1->BasePoint1, LocalPoint2->BasePoint2.
        /// </summary>
        private Matrix ComputeChildToBaseTransform(OverlayLayer baseLayer, OverlayLayer childLayer)
        {
            // "Базовые" точки (A_b, B_b) берём из baseLayer.LocalPoint1 / LocalPoint2
            // в тех же координатах, в которых нарисован baseLayer (т.е. "как есть").
            var A_b = baseLayer.LocalPoint1;
            var B_b = baseLayer.LocalPoint2;

            // "Child"-точки (A_i, B_i) - из childLayer
            var A_i = childLayer.LocalPoint1;
            var B_i = childLayer.LocalPoint2;

            // distChild = расстояние между A_i и B_i
            float distChild = Distance(A_i, B_i);
            // distBase = расстояние между A_b и B_b
            float distBase = Distance(A_b, B_b);

            float scale = (distChild == 0) ? 1.0f : (distBase / distChild);

            // Угол "child"
            float angleChild = (float)Math.Atan2(B_i.Y - A_i.Y, B_i.X - A_i.X);
            // Угол "base"
            float angleBase = (float)Math.Atan2(B_b.Y - A_b.Y, B_b.X - A_b.X);

            float rotationDeg = (angleBase - angleChild) * 180f / (float)Math.PI;

            // Формируем матрицу
            var m = new Matrix();

            // 1) Сместить A_i к (0,0)
            m.Translate(-A_i.X, -A_i.Y);

            // 2) Масштаб
            m.Scale(scale, scale, MatrixOrder.Append);

            // 3) Поворот
            m.Rotate(rotationDeg, MatrixOrder.Append);

            // 4) Перенести (0,0) в A_b
            m.Translate(A_b.X, A_b.Y, MatrixOrder.Append);

            return m;
        }

        private float Distance(PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
