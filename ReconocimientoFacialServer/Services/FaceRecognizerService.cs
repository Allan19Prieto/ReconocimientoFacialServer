using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using SkiaSharp;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;

namespace ReconocimientoFacialServer.Services
{
    public class FaceRecognizerService
    {
        private readonly EigenFaceRecognizer _recognizer;
        private readonly CascadeClassifier _faceCascade;

        public FaceRecognizerService()
        {
            // Cargar modelo Haar Cascade para detección facial
            string basePath = AppContext.BaseDirectory;
            string modelPath = Path.Combine(basePath, "wwwroot", "models", "haarcascade_frontalface_default.xml");

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"El archivo del modelo no se encontró en {modelPath}");
            }

            _faceCascade = new CascadeClassifier(modelPath);

            // Cargar modelo preentrenado para reconocimiento facial
            _recognizer = new EigenFaceRecognizer();
            if (File.Exists("modelo.yml"))
            {
                _recognizer.Read("modelo.yml");
            }
        }

        // Esta usa el modelo descargado
        public Rectangle[] DetectFaces(SKBitmap bitmap)
        {
            // Convertir SKBitmap a Mat
            var matImage = ConvertSkBitmapToMat(bitmap);

            // Convertir a escala de grises
            using var grayImage = matImage.ToImage<Gray, byte>();

            // Detectar rostros
            return _faceCascade.DetectMultiScale(grayImage, 1.1, 5, new Size(30, 30), Size.Empty);
        }

        public Mat ConvertBitmapToMat(Bitmap bitmap)
        {
            var pixels = ConvertBitmapToByteArray(bitmap);
            var mat = new Mat(bitmap.Height, bitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            mat.SetTo(pixels);

            return mat;
        }


        public SKBitmap ConvertBase64ToBitmap(string base64Image)
        {
            var imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
            using var ms = new MemoryStream(imageBytes);
            return SKBitmap.Decode(ms);
        }

        public SKBitmap ConvertBase64ToSkBitmap(string base64Image)
        {
            var imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
            using var ms = new MemoryStream(imageBytes);
            return SKBitmap.Decode(ms);
        }

        public SKBitmap CropFace(SKBitmap bitmap, Rectangle faceRectangle)
        {
            var croppedBitmap = new SKBitmap(faceRectangle.Width, faceRectangle.Height);
            using var canvas = new SKCanvas(croppedBitmap);
            canvas.DrawBitmap(bitmap, new SKRect(faceRectangle.Left, faceRectangle.Top, faceRectangle.Right, faceRectangle.Bottom),
                new SKRect(0, 0, faceRectangle.Width, faceRectangle.Height));
            return croppedBitmap;
        }

        public SKBitmap ResizeBitmap(SKBitmap bitmap, int width, int height)
        {
            var resizedBitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(resizedBitmap);
            canvas.DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(0, 0, width, height));
            return resizedBitmap;
        }

        public string ConvertSkBitmapToBase64(SKBitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
            return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
        }

        public byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat
            );

            int byteCount = bitmapData.Stride * bitmap.Height;
            byte[] pixels = new byte[byteCount];

            Marshal.Copy(bitmapData.Scan0, pixels, 0, byteCount);

            bitmap.UnlockBits(bitmapData);
            return pixels;
        }


        private Mat ConvertSkBitmapToMat(SKBitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            // Extraer píxeles del SKBitmap
            var pixels = new byte[width * height * 4]; // RGBA
            bitmap.Bytes.CopyTo(pixels, 0);

            // Crear un Mat desde los píxeles
            var mat = new Mat(height, width, DepthType.Cv8U, 4);
            mat.SetTo(pixels);

            // Convertir de RGBA a RGB
            var rgbMat = new Mat();
            CvInvoke.CvtColor(mat, rgbMat, ColorConversion.Bgra2Bgr);

            return rgbMat;
        }


        public string RecognizeFace(string base64Image)
        {
            try
            {
                // Convertir Base64 a Mat
                var skBitmap = ConvertBase64ToSkBitmap(base64Image);
                var matImage = ConvertSkBitmapToMat(skBitmap);

                using var grayImage = matImage.ToImage<Gray, byte>();

                // Redimensionar la imagen al tamaño esperado (100x100 píxeles)
                var resizedImage = grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);

                var result = _recognizer.Predict(resizedImage);

                if (result.Label == -1 || result.Distance > 5000)
                {
                    return "Usuario desconocido";
                }

                return $"Usuario reconocido: {result.Label}";
            }
            catch (Exception ex)
            {
                return $"Error al reconocer el rostro: {ex.Message}";
            }
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

            // Prueba en consola
            Console.WriteLine("Iniciando el proceso de entrenamiento...");

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
                // Purba en consola
                Console.WriteLine($"Usuario {userId}: {augmentedImages.Count} imágenes aumentadas añadidas.");
            }

            Console.WriteLine("Iniciando entrenamiento...");
            foreach (var label in labelIds)
            {
                Console.WriteLine($"Etiqueta asignada: Usuario {label}");
            }

            var imagesAsMat = trainingImages.Select(img => img.Mat).ToArray();
            var labelIdsArray = labelIds.ToArray();

            Console.WriteLine($"Número total de imágenes de entrenamiento: {imagesAsMat.Length}");
            Console.WriteLine($"Número total de etiquetas: {labelIdsArray.Length}");

            // Verificar que haya imágenes y etiquetas
            if (imagesAsMat.Length == 0 || labelIdsArray.Length == 0)
            {
                Console.WriteLine("Error: No hay datos suficientes para entrenar el modelo.");
                return;
            }

            // Entrenar el modelo
            try
            {
                _recognizer.Train(imagesAsMat, labelIdsArray);
                Console.WriteLine("Entrenamiento completado con éxito.");

                // Guardar el modelo entrenado
                _recognizer.Write("modelo.yml");
                Console.WriteLine("Modelo guardado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el entrenamiento: {ex.Message}");
            }
        }

    }
}
