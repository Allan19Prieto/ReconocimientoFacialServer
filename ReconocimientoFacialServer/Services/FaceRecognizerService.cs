﻿using Emgu.CV;
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

                double threshold = 4000; // Determina este valor experimentalmente
                if (result.Label == -1 || result.Distance > threshold)
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

        public void TrainRecognizer(Dictionary<int, List<string>> trainingImagesBase64)
        {
            var trainingImages = new List<Image<Gray, byte>>();
            var labelIds = new List<int>();

            foreach (var userImages in trainingImagesBase64)
            {
                var userId = userImages.Key; // Este será el label

                foreach (var base64Image in userImages.Value)
                {
                    // Preprocesar cada imagen
                    var processedImage = PreprocessImage(base64Image);

                    // Agregar la imagen procesada al conjunto de entrenamiento
                    trainingImages.Add(processedImage);
                    labelIds.Add(userId); // Asociar la imagen con el UserId
                }
            }

            // Convertir a Mat y entrenar el modelo
            var imagesAsMat = trainingImages.Select(img => img.Mat).ToArray();
            var labelIdsArray = labelIds.ToArray();

            // Verificar que haya suficientes datos
            if (imagesAsMat.Length == 0 || labelIdsArray.Length == 0)
            {
                throw new InvalidOperationException("No hay datos suficientes para entrenar el modelo.");
            }

            // Entrenar el modelo con imágenes y labels y lo Guarda
            if (imagesAsMat.Length > 0 && labelIdsArray.Length > 0)
            {

                _recognizer.Train(imagesAsMat, labelIdsArray);
                _recognizer.Write("modelo.yml"); // Guardar el modelo
            }
            else
            {
                throw new InvalidOperationException("No hay datos suficientes para entrenar el modelo.");
            }
        }


        //public void TrainRecognizer(Dictionary<int, string> trainingImagesBase64, Dictionary<int, string> labels)
        //{
        //    var trainingImages = new List<Image<Gray, byte>>();
        //    var labelIds = new List<int>();

        //    // Prueba en consola
        //    Console.WriteLine("Iniciando el proceso de entrenamiento...");

        //    foreach (var (userId, base64String) in trainingImagesBase64)
        //    {
        //        try {
        //            //var userId = imageBase64.Key;
        //            //var base64String = imageBase64.Value;

        //            // Preprocesar la imagen original
        //            var originalImage = PreprocessImage(base64String);

        //            // Generar imágenes aumentadas
        //            var augmentedImages = GenerateAugmentedImages(originalImage);

        //            // Agregar todas las imágenes aumentadas al diccionario
        //            foreach (var augmentedImage in augmentedImages)
        //            {
        //                trainingImages.Add(augmentedImage);
        //                labelIds.Add(userId); // Asignar siempre el mismo ID para el usuario original
        //            }

        //            // Purba en consola
        //            Console.WriteLine($"Usuario {userId}: {augmentedImages.Count} imágenes aumentadas añadidas.");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error al procesar imágenes del usuario {userId}: {ex.Message}");
        //        }

        //    }

        //    if (trainingImages.Count == 0)
        //    {
        //        Console.WriteLine("Error: No se encontraron imágenes válidas para entrenar el modelo.");
        //        return;
        //    }

        //    Console.WriteLine("Iniciando entrenamiento...");

        //    // Entrenar el modelo
        //    try
        //    {
        //        _recognizer.Train(trainingImages.Select(img => img.Mat).ToArray(), labelIds.ToArray());
        //        Console.WriteLine("Entrenamiento completado con éxito.");
        //        _recognizer.Write("modelo.yml");
        //        Console.WriteLine("Modelo guardado correctamente.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error durante el entrenamiento: {ex.Message}");
        //    }
        //}

        public bool DetectBlink(Mat frame)
        {
            // Cargar Haar Cascade para detección de ojos
            string basePath = AppContext.BaseDirectory;
            string eyeCascadePath = Path.Combine(basePath, "wwwroot", "models", "haarcascade_eye.xml");

            if (!File.Exists(eyeCascadePath))
            {
                throw new FileNotFoundException($"El archivo del modelo no se encontró en {eyeCascadePath}");
            }
            var eyeCascade = new CascadeClassifier(eyeCascadePath);

            // Convertir frame a escala de grises
            var grayFrame = new Mat();
            CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

            // Detectar ojos
            var eyes = eyeCascade.DetectMultiScale(grayFrame, 1.1, 5, new Size(20, 20), Size.Empty);

            // Si se detectan ojos, se asume que no hay parpadeo
            return eyes.Length >= 2;
        }

        public async Task<bool> VerifyBlinkAsync(string base64Image)
        {
            var matImage = ConvertBase64ToMat(base64Image);

            // Verificar si se detecta parpadeo
            return DetectBlink(matImage);
        }

        // Algoritmo para aplicar LBP
        private Mat ApplyLBP(Mat grayImage)
        {
            // Validar que la imagen esté en escala de grises
            if (grayImage.NumberOfChannels != 1)
            {
                throw new ArgumentException("La imagen debe estar en escala de grises.");
            }

            // Crear una nueva imagen para almacenar el resultado
            var lbpImage = new Mat(grayImage.Rows, grayImage.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 1);

            // Obtener punteros a los datos
            unsafe
            {
                byte* srcPtr = (byte*)grayImage.DataPointer;
                byte* destPtr = (byte*)lbpImage.DataPointer;

                int step = grayImage.Step; // Longitud de cada fila en bytes

                // Aplicar LBP píxel por píxel (evitar los bordes)
                for (int y = 1; y < grayImage.Rows - 1; y++)
                {
                    for (int x = 1; x < grayImage.Cols - 1; x++)
                    {
                        byte center = *(srcPtr + y * step + x);
                        byte code = 0;

                        // Comparar con los píxeles vecinos
                        code |= (byte)((*(srcPtr + (y - 1) * step + (x - 1)) >= center ? 1 : 0) << 7);
                        code |= (byte)((*(srcPtr + (y - 1) * step + x) >= center ? 1 : 0) << 6);
                        code |= (byte)((*(srcPtr + (y - 1) * step + (x + 1)) >= center ? 1 : 0) << 5);
                        code |= (byte)((*(srcPtr + y * step + (x + 1)) >= center ? 1 : 0) << 4);
                        code |= (byte)((*(srcPtr + (y + 1) * step + (x + 1)) >= center ? 1 : 0) << 3);
                        code |= (byte)((*(srcPtr + (y + 1) * step + x) >= center ? 1 : 0) << 2);
                        code |= (byte)((*(srcPtr + (y + 1) * step + (x - 1)) >= center ? 1 : 0) << 1);
                        code |= (byte)((*(srcPtr + y * step + (x - 1)) >= center ? 1 : 0) << 0);

                        // Asignar el valor al píxel LBP
                        *(destPtr + y * step + x) = code;
                    }
                }
            }

            return lbpImage;
        }

        public bool AnalyzeTexture(Mat frame)
        {
            // Convertir a escala de grises si es necesario
            if (frame.NumberOfChannels != 1)
            {
                var grayFrame = new Mat();
                CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
                frame = grayFrame;
            }

            // Aplicar LBP
            var lbpImage = ApplyLBP(frame);

            // Crear histograma
            var histogram = new DenseHistogram(256, new RangeF(0, 256));

            // Evaluar la uniformidad de las texturas
            var uniformity = CalculateUniformity(histogram, lbpImage);
            return uniformity > 0.5; // Ajusta el umbral según tus necesidades
        }



        private double CalculateUniformity(DenseHistogram histogram, Mat lbpImage)
        {
            // Validar que la imagen LBP sea válida y de un canal
            if (lbpImage.NumberOfChannels != 1)
            {
                throw new ArgumentException("La imagen LBP debe ser de un solo canal.");
            }

            // Crear máscara vacía (opcional)
            using var mask = new Mat();

            // Convertir Mat a Image<Gray, byte> para compatibilidad con DenseHistogram
            using var grayImage = lbpImage.ToImage<Gray, byte>();

            // Calcular el histograma
            histogram.Calculate<byte>(new Image<Gray, byte>[] { grayImage }, false, null);

            // Copiar los valores del histograma a un arreglo
            var bins = new float[256];
            histogram.CopyTo(bins);

            // Calcular la uniformidad
            double uniformity = 0.0;
            foreach (var bin in bins)
            {
                uniformity += Math.Pow(bin, 2);
            }

            return uniformity;
        }


        public async Task<bool> VerifyLivenessAsync(string base64Image)
        {
            var matImage = ConvertBase64ToMat(base64Image);

            // Verificar parpadeo y texturas
            bool isBlinkDetected = DetectBlink(matImage);
            bool isTextureValid = AnalyzeTexture(matImage);

            return isBlinkDetected && isTextureValid;
        }

        public Mat ConvertBase64ToMat(string base64Image)
        {
            try
            {
                // Eliminar el prefijo Base64 si está presente
                if (base64Image.StartsWith("data:image"))
                {
                    base64Image = base64Image.Substring(base64Image.IndexOf(",") + 1);
                }

                // Convertir Base64 a un arreglo de bytes
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // Decodificar los bytes en un SKBitmap usando SkiaSharp
                using var ms = new MemoryStream(imageBytes);
                var skBitmap = SKBitmap.Decode(ms) ?? throw new Exception("No se pudo decodificar la imagen Base64.");

                // Convertir SKBitmap a Mat
                var mat = new Mat(skBitmap.Height, skBitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);

                // Rellenar los datos del Mat con los bytes del SKBitmap
                var pixels = skBitmap.Pixels.SelectMany(color => new[] { color.Red, color.Green, color.Blue }).ToArray();
                mat.SetTo(pixels);

                return mat;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir Base64 a Mat: {ex.Message}", ex);
            }
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

        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        public SKBitmap ConvertBase64ToSkBitmap(string base64Image)
        {
            try
            {
                if (!IsBase64String(base64Image.Replace("data:image/png;base64,", "")))
                {
                    throw new Exception("La cadena Base64 no es válida.");
                }

                var imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
                using var ms = new MemoryStream(imageBytes);
                return SKBitmap.Decode(ms) ?? throw new Exception("No se pudo decodificar la imagen.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir Base64 a SKBitmap: {ex.Message}");
            }
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

    }
}
