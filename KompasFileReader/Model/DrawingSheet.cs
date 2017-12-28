namespace KompasFileReader.Model
{
    public class DrawingSheet
    {
        /// <summary>
        /// Пример 3,4 
        /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// Пример 1 горизонатальная (альбомный) - 0 вертикальный
        /// </summary>
        public int Orientation { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
