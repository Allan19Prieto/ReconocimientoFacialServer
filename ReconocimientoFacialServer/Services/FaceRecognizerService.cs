using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using SkiaSharp;

namespace ReconocimientoFacialServer.Services
{
    public class FaceRecognizerService
    {
        private readonly EigenFaceRecognizer _recognizer;

        public FaceRecognizerService()
        {
            // Crear un reconocedor facial
            _recognizer = new EigenFaceRecognizer();
        }

        public string RecognizeFace(Image<Gray, byte> testImage)
        {
            var result = _recognizer.Predict(testImage);

            if (result.Label == -1 || result.Distance > 5000) // Ajusta el umbral según sea necesario
            {
                return "Desconocido";
            }

            return $"Usuario {result.Label}";
        }

        //public static Image<Gray, byte> PreprocessImage(string base64Image)
        //{
        //    // Convertir la imagen Base64 a un SKBitmap
        //    byte[] imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
        //    using var ms = new MemoryStream(imageBytes);
        //    var skBitmap = SKBitmap.Decode(ms);

        //    // Convertir SKBitmap a escala de grises
        //    using var grayBitmap = new SKBitmap(skBitmap.Width, skBitmap.Height, SKColorType.Gray8, SKAlphaType.Opaque);
        //    using var canvas = new SKCanvas(grayBitmap);
        //    canvas.DrawBitmap(skBitmap, 0, 0);

        //    // Convertir el SKBitmap a Mat
        //    var pixelData = grayBitmap.Pixels;
        //    var mat = new Mat(grayBitmap.Height, grayBitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        //    mat.SetTo(pixelData.Select(p => (byte)p.Red).ToArray());

        //    // Convertir el Mat a Image<Gray, byte>
        //    var grayImage = mat.ToImage<Gray, byte>();

        //    // Cambiar el tamaño de la imagen a 100x100
        //    return grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);
        //}

        public static Image<Gray, byte> PreprocessImage(string base64Image)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
            using var ms = new MemoryStream(imageBytes);
            var originalImage = SKBitmap.Decode(ms);

            // Convertir a escala de grises
            using var grayBitmap = new SKBitmap(originalImage.Width, originalImage.Height, SKColorType.Gray8, SKAlphaType.Opaque);
            using var canvas = new SKCanvas(grayBitmap);
            canvas.DrawBitmap(originalImage, 0, 0);

            // Convertir a Mat y aplicar procesamiento adicional
            var mat = new Mat(grayBitmap.Height, grayBitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
            mat.SetTo(grayBitmap.Pixels.Select(p => (byte)p.Red).ToArray());

            // Convertir a Image<Gray, byte>
            var grayImage = mat.ToImage<Gray, byte>();
            grayImage._EqualizeHist(); // Normalizar el histograma
            return grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);
        }

        public static List<Image<Gray, byte>> GenerateAugmentedImages(Image<Gray, byte> originalImage)
        {
            var augmentedImages = new List<Image<Gray, byte>>();

            // Imagen original
            augmentedImages.Add(originalImage);

            // Rotaciones
            augmentedImages.Add(originalImage.Rotate(10, new Gray(0))); // Rotar 10 grados
            augmentedImages.Add(originalImage.Rotate(-10, new Gray(0))); // Rotar -10 grados

            // Espejado horizontal
            var flippedImage = originalImage.Clone();
            flippedImage._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
            augmentedImages.Add(flippedImage);

            // Aumentar brillo
            var brighterImage = originalImage.Clone();
            brighterImage._GammaCorrect(1.5); // Aumentar brillo
            augmentedImages.Add(brighterImage);

            // Reducir brillo
            var darkerImage = originalImage.Clone();
            darkerImage._GammaCorrect(0.7); // Reducir brillo
            augmentedImages.Add(darkerImage);

            return augmentedImages;
        }

        public void TrainRecognizer(Dictionary<int, string> trainingImagesBase64, Dictionary<int, string> labels)
        {
            var trainingImages = new List<Image<Gray, byte>>();
            var labelIds = new List<int>();

            foreach (var imageBase64 in trainingImagesBase64)
            {
                var userId = imageBase64.Key;
                var base64String = imageBase64.Value;

                // Preprocesar la imagen original
                var originalImage = PreprocessImage(base64String);

                // Generar imágenes aumentadas
                var augmentedImages = GenerateAugmentedImages(originalImage);

                // Agregar todas las imágenes aumentadas al diccionario
                foreach (var augmentedImage in augmentedImages)
                {
                    trainingImages.Add(augmentedImage);
                    labelIds.Add(userId); // Asignar siempre el mismo ID para el usuario original
                }
            }

            Console.WriteLine("Iniciando entrenamiento...");
            foreach (var label in labelIds)
            {
                Console.WriteLine($"Etiqueta asignada: Usuario {label}");
            }

            var imagesAsMat = trainingImages.Select(img => img.Mat).ToArray();
            var labelIdsArray = labelIds.ToArray();

            _recognizer.Train(imagesAsMat, labelIdsArray);
        }

    }
}
