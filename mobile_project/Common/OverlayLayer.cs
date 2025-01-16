using System.Drawing;

namespace Common
{
    /// <summary>
    /// Слой с одним изображением и двумя "локальными" точками.
    /// </summary>
    public class OverlayLayer
    {
        /// <summary>
        /// Название файла
        /// </summary>
        public string FileName { get; set; } = "Unnamed";

        /// <summary>
        /// Собственно картинка в GDI+ (для Merge).
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// Прозрачность (0..1)
        /// </summary>
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// Две "локальные" точки на этом изображении (в его собственных координатах).
        /// Например, (100,100) и (200,100).
        /// </summary>
        public PointF LocalPoint1 { get; set; }
        public PointF LocalPoint2 { get; set; }

        /// <summary>
        /// Признак, является ли этот слой "базовым".
        /// По задумке, должен быть ровно один базовый слой в OverlayManager.
        /// Если IsBase = true, то этот слой НЕ трансформируется, а кладётся в (0,0) итоговой картинки "как есть".
        /// И его LocalPoint1 и LocalPoint2 - считаются "эталонными" (A_b, B_b) для child-слоёв.
        /// </summary>
        public bool IsBase { get; set; } = false;

        /// <summary>
        /// Признак, использующийся на канвасе WPF.
        /// Позволяет обрабатывать переопределение точек путем повторного нажатия
        /// </summary>
        public bool IsOddPoint { get; set; } = true;
    }
}
