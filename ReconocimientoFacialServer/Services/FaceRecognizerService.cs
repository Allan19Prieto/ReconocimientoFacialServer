////using Emgu.CV;
////using Emgu.CV.Face;
////using Emgu.CV.Structure;
//using DlibDotNet;
////using DlibDotNet.Extensions;
//using System.Collections.Generic;
////using System.Drawing;
//using System.Linq;
//using System.IO;
//using SkiaSharp;
//using System.Drawing.Imaging;
//using System.Runtime.InteropServices;
//using Emgu.CV.CvEnum;

//namespace ReconocimientoFacialServer.Services
//{
//    public class FaceRecognizerService
//    {
//        //private readonly EigenFaceRecognizer _recognizer;
//        //private readonly CascadeClassifier _faceCascade;

//        private readonly FrontalFaceDetector _faceDetector;
//        private readonly ShapePredictor _shapePredictor;
//        private readonly DlibDotNet.Dnn.LossMetric _recognitionModel;
//        //private readonly FaceRecognitionModelV1 _faceRecognitionModel;
//        //private readonly FaceRecognitionModelV1 _faceRecognitionModel;

//        public FaceRecognizerService()
//        {

//            string basePath = AppContext.BaseDirectory;
//            string landmarkModelPath = Path.Combine(basePath, "wwwroot/models/shape_predictor_68_face_landmarks.dat");
//            string faceRecognitionModelPath = Path.Combine(basePath, "wwwroot/models/dlib_face_recognition_resnet_model_v1.dat");

//            if (!File.Exists(landmarkModelPath) || !File.Exists(faceRecognitionModelPath))
//                throw new FileNotFoundException("Modelos no encontrados en la ruta especificada.");

//            _faceDetector = Dlib.GetFrontalFaceDetector();
//            _shapePredictor = ShapePredictor.Deserialize(landmarkModelPath);
//            _recognitionModel = DlibDotNet.Dnn.LossMetric.Deserialize(faceRecognitionModelPath);

//            // Cargar modelos preentrenados
//            //string modelPath = Path.Combine(AppContext.BaseDirectory, "wwwroot/models");
//            //_faceDetector = Dlib.GetFrontalFaceDetector();
//            //_shapePredictor = ShapePredictor.Deserialize(Path.Combine(modelPath, "shape_predictor_68_face_landmarks.dat"));
//            //_faceRecognitionModel = FaceRecognitionModelV1.Deserialize(Path.Combine(modelPath, "dlib_face_recognition_resnet_model_v1.dat"));

//            // Cargar modelo Haar Cascade para detección facial
//            //string basePath = AppContext.BaseDirectory;
//            //string modelPath2 = Path.Combine(basePath, "wwwroot", "models", "haarcascade_frontalface_default.xml");

//            //if (!File.Exists(modelPath2))
//            //{
//            //    throw new FileNotFoundException($"El archivo del modelo no se encontró en {modelPath2}");
//            //}

//            //_faceCascade = new CascadeClassifier(modelPath2);

//            //// Cargar modelo preentrenado para reconocimiento facial
//            //_recognizer = new EigenFaceRecognizer();
//            //if (File.Exists("modelo.yml"))
//            //{
//            //    _recognizer.Read("modelo.yml");
//            //}
//        }

//        public IEnumerable<Rectangle> DetectFaces(Matrix<RgbPixel> image)
//        {
//            return _faceDetector.Operator(image);
//        }

//        //public Rectangle[] DetectFaces(Mat frame)
//        //{
//        //    using var image = frame.ToBitmap().ToArray2D<RgbPixel>();
//        //    var detectedFaces = _faceDetector.Operator(image).Select(r => new Rectangle(r.Left, r.Top, r.Width, r.Height)).ToArray();
//        //    return detectedFaces;
//        //}

//        public IEnumerable<double[]> ComputeDescriptors(Matrix<RgbPixel> image, IEnumerable<Rectangle> faces)
//        {
//            var descriptors = new List<double[]>();

//            foreach (var face in faces)
//            {
//                var shape = _shapePredictor.Detect(image, face);
//                var descriptor = _recognitionModel.Operator(image, shape);
//                descriptors.Add(descriptor.ToArray());
//            }

//            return descriptors;
//        }

//        public float[] GetFaceEmbedding(Mat faceImage)
//        {
//            using var image = faceImage.ToBitmap().ToArray2D<RgbPixel>();
//            var rect = _faceDetector.Operator(image).FirstOrDefault();
//            if (rect == null)
//                throw new Exception("No se detectó ningún rostro.");

//            var shape = _shapePredictor.Detect(image, rect);
//            var embedding = _faceRecognitionModel.ComputeFaceDescriptor(image, shape);

//            return embedding.ToArray();
//        }

//        public bool CompareEmbeddings(float[] embedding1, float[] embedding2, float threshold = 0.6f)
//        {
//            var distance = embedding1.Zip(embedding2, (x, y) => (x - y) * (x - y)).Sum();
//            return distance < threshold;
//        }

//        // Esta usa el modelo descargado
//        //public Rectangle[] DetectFaces(SKBitmap bitmap)
//        //{
//        //    // Convertir SKBitmap a Mat
//        //    var matImage = ConvertSkBitmapToMat(bitmap);

//        //    // Convertir a escala de grises
//        //    using var grayImage = matImage.ToImage<Gray, byte>();

//        //    // Detectar rostros
//        //    return _faceCascade.DetectMultiScale(grayImage, 1.1, 5, new Size(30, 30), Size.Empty);
//        //}

//        public string RecognizeFace(string base64Image)
//        {
//            try
//            {
//                // Convertir Base64 a Mat
//                var skBitmap = ConvertBase64ToSkBitmap(base64Image);
//                var matImage = ConvertSkBitmapToMat(skBitmap);

//                using var grayImage = matImage.ToImage<Gray, byte>();

//                // Redimensionar la imagen al tamaño esperado (100x100 píxeles)
//                var resizedImage = grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);

//                var result = _recognizer.Predict(resizedImage);

//                double threshold = 4000; // Determina este valor experimentalmente
//                if (result.Label == -1 || result.Distance > threshold)
//                {
//                    return "Usuario desconocido";
//                }

//                return $"Usuario reconocido: {result.Label}";
//            }
//            catch (Exception ex)
//            {
//                return $"Error al reconocer el rostro: {ex.Message}";
//            }
//        }

//        public static Image<Gray, byte> PreprocessImage(string base64Image)
//        {
//            byte[] imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
//            using var ms = new MemoryStream(imageBytes);
//            var originalImage = SKBitmap.Decode(ms);

//            // Convertir a escala de grises
//            using var grayBitmap = new SKBitmap(originalImage.Width, originalImage.Height, SKColorType.Gray8, SKAlphaType.Opaque);
//            using var canvas = new SKCanvas(grayBitmap);
//            canvas.DrawBitmap(originalImage, 0, 0);

//            // Convertir a Mat y aplicar procesamiento adicional
//            var mat = new Mat(grayBitmap.Height, grayBitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
//            mat.SetTo(grayBitmap.Pixels.Select(p => (byte)p.Red).ToArray());

//            // Convertir a Image<Gray, byte>
//            var grayImage = mat.ToImage<Gray, byte>();
//            grayImage._EqualizeHist(); // Normalizar el histograma
//            return grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);
//        }

//        public static List<Image<Gray, byte>> GenerateAugmentedImages(Image<Gray, byte> originalImage)
//        {
//            var augmentedImages = new List<Image<Gray, byte>>();

//            // Imagen original
//            augmentedImages.Add(originalImage);

//            // Rotaciones
//            augmentedImages.Add(originalImage.Rotate(10, new Gray(0))); // Rotar 10 grados
//            augmentedImages.Add(originalImage.Rotate(-10, new Gray(0))); // Rotar -10 grados

//            // Espejado horizontal
//            var flippedImage = originalImage.Clone();
//            flippedImage._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
//            augmentedImages.Add(flippedImage);

//            // Aumentar brillo
//            var brighterImage = originalImage.Clone();
//            brighterImage._GammaCorrect(1.5); // Aumentar brillo
//            augmentedImages.Add(brighterImage);

//            // Reducir brillo
//            var darkerImage = originalImage.Clone();
//            darkerImage._GammaCorrect(0.7); // Reducir brillo
//            augmentedImages.Add(darkerImage);

//            return augmentedImages;
//        }

//        public void TrainRecognizer(Dictionary<int, List<string>> trainingImagesBase64)
//        {
//            var trainingImages = new List<Image<Gray, byte>>();
//            var labelIds = new List<int>();

//            foreach (var userImages in trainingImagesBase64)
//            {
//                var userId = userImages.Key; // Este será el label

//                foreach (var base64Image in userImages.Value)
//                {
//                    // Preprocesar cada imagen
//                    var processedImage = PreprocessImage(base64Image);

//                    // Agregar la imagen procesada al conjunto de entrenamiento
//                    trainingImages.Add(processedImage);
//                    labelIds.Add(userId); // Asociar la imagen con el UserId
//                }
//            }

//            // Convertir a Mat y entrenar el modelo
//            var imagesAsMat = trainingImages.Select(img => img.Mat).ToArray();
//            var labelIdsArray = labelIds.ToArray();

//            // Verificar que haya suficientes datos
//            if (imagesAsMat.Length == 0 || labelIdsArray.Length == 0)
//            {
//                throw new InvalidOperationException("No hay datos suficientes para entrenar el modelo.");
//            }

//            // Entrenar el modelo con imágenes y labels y lo Guarda
//            if (imagesAsMat.Length > 0 && labelIdsArray.Length > 0)
//            {

//                _recognizer.Train(imagesAsMat, labelIdsArray);
//                _recognizer.Write("modelo.yml"); // Guardar el modelo
//            }
//            else
//            {
//                throw new InvalidOperationException("No hay datos suficientes para entrenar el modelo.");
//            }
//        }


//        //public void TrainRecognizer(Dictionary<int, string> trainingImagesBase64, Dictionary<int, string> labels)
//        //{
//        //    var trainingImages = new List<Image<Gray, byte>>();
//        //    var labelIds = new List<int>();

//        //    // Prueba en consola
//        //    Console.WriteLine("Iniciando el proceso de entrenamiento...");

//        //    foreach (var (userId, base64String) in trainingImagesBase64)
//        //    {
//        //        try {
//        //            //var userId = imageBase64.Key;
//        //            //var base64String = imageBase64.Value;

//        //            // Preprocesar la imagen original
//        //            var originalImage = PreprocessImage(base64String);

//        //            // Generar imágenes aumentadas
//        //            var augmentedImages = GenerateAugmentedImages(originalImage);

//        //            // Agregar todas las imágenes aumentadas al diccionario
//        //            foreach (var augmentedImage in augmentedImages)
//        //            {
//        //                trainingImages.Add(augmentedImage);
//        //                labelIds.Add(userId); // Asignar siempre el mismo ID para el usuario original
//        //            }

//        //            // Purba en consola
//        //            Console.WriteLine($"Usuario {userId}: {augmentedImages.Count} imágenes aumentadas añadidas.");
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            Console.WriteLine($"Error al procesar imágenes del usuario {userId}: {ex.Message}");
//        //        }

//        //    }

//        //    if (trainingImages.Count == 0)
//        //    {
//        //        Console.WriteLine("Error: No se encontraron imágenes válidas para entrenar el modelo.");
//        //        return;
//        //    }

//        //    Console.WriteLine("Iniciando entrenamiento...");

//        //    // Entrenar el modelo
//        //    try
//        //    {
//        //        _recognizer.Train(trainingImages.Select(img => img.Mat).ToArray(), labelIds.ToArray());
//        //        Console.WriteLine("Entrenamiento completado con éxito.");
//        //        _recognizer.Write("modelo.yml");
//        //        Console.WriteLine("Modelo guardado correctamente.");
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"Error durante el entrenamiento: {ex.Message}");
//        //    }
//        //}

//        public bool DetectBlink(Mat frame)
//        {
//            // Cargar Haar Cascade para detección de ojos
//            string basePath = AppContext.BaseDirectory;
//            string eyeCascadePath = Path.Combine(basePath, "wwwroot", "models", "haarcascade_eye.xml");

//            if (!File.Exists(eyeCascadePath))
//            {
//                throw new FileNotFoundException($"El archivo del modelo no se encontró en {eyeCascadePath}");
//            }
//            var eyeCascade = new CascadeClassifier(eyeCascadePath);

//            // Convertir frame a escala de grises
//            var grayFrame = new Mat();
//            CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

//            // Detectar ojos
//            //var eyes = eyeCascade.DetectMultiScale(grayFrame, 1.1, 5, new Size(20, 20), Size.Empty);

//            // Si se detectan ojos, se asume que no hay parpadeo
//            return true;// eyes.Length >= 2;
//        }

//        public async Task<bool> VerifyBlinkAsync(string base64Image)
//        {
//            var matImage = ConvertBase64ToMat(base64Image);

//            // Verificar si se detecta parpadeo
//            return DetectBlink(matImage);
//        }

//        // Algoritmo para aplicar LBP
//        private Mat ApplyLBP(Mat grayImage)
//        {
//            // Validar que la imagen esté en escala de grises
//            if (grayImage.NumberOfChannels != 1)
//            {
//                throw new ArgumentException("La imagen debe estar en escala de grises.");
//            }

//            // Crear una nueva imagen para almacenar el resultado
//            var lbpImage = new Mat(grayImage.Rows, grayImage.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 1);

//            // Obtener punteros a los datos
//            unsafe
//            {
//                byte* srcPtr = (byte*)grayImage.DataPointer;
//                byte* destPtr = (byte*)lbpImage.DataPointer;

//                int step = grayImage.Step; // Longitud de cada fila en bytes

//                // Aplicar LBP píxel por píxel (evitar los bordes)
//                for (int y = 1; y < grayImage.Rows - 1; y++)
//                {
//                    for (int x = 1; x < grayImage.Cols - 1; x++)
//                    {
//                        byte center = *(srcPtr + y * step + x);
//                        byte code = 0;

//                        // Comparar con los píxeles vecinos
//                        code |= (byte)((*(srcPtr + (y - 1) * step + (x - 1)) >= center ? 1 : 0) << 7);
//                        code |= (byte)((*(srcPtr + (y - 1) * step + x) >= center ? 1 : 0) << 6);
//                        code |= (byte)((*(srcPtr + (y - 1) * step + (x + 1)) >= center ? 1 : 0) << 5);
//                        code |= (byte)((*(srcPtr + y * step + (x + 1)) >= center ? 1 : 0) << 4);
//                        code |= (byte)((*(srcPtr + (y + 1) * step + (x + 1)) >= center ? 1 : 0) << 3);
//                        code |= (byte)((*(srcPtr + (y + 1) * step + x) >= center ? 1 : 0) << 2);
//                        code |= (byte)((*(srcPtr + (y + 1) * step + (x - 1)) >= center ? 1 : 0) << 1);
//                        code |= (byte)((*(srcPtr + y * step + (x - 1)) >= center ? 1 : 0) << 0);

//                        // Asignar el valor al píxel LBP
//                        *(destPtr + y * step + x) = code;
//                    }
//                }
//            }

//            return lbpImage;
//        }

//        public bool AnalyzeTexture(Mat frame)
//        {
//            // Convertir a escala de grises si es necesario
//            if (frame.NumberOfChannels != 1)
//            {
//                var grayFrame = new Mat();
//                CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
//                frame = grayFrame;
//            }

//            // Aplicar LBP
//            var lbpImage = ApplyLBP(frame);

//            // Crear histograma
//            var histogram = new DenseHistogram(256, new RangeF(0, 256));

//            // Evaluar la uniformidad de las texturas
//            var uniformity = CalculateUniformity(histogram, lbpImage);
//            return uniformity > 0.5; // Ajusta el umbral según tus necesidades
//        }



//        private double CalculateUniformity(DenseHistogram histogram, Mat lbpImage)
//        {
//            // Validar que la imagen LBP sea válida y de un canal
//            if (lbpImage.NumberOfChannels != 1)
//            {
//                throw new ArgumentException("La imagen LBP debe ser de un solo canal.");
//            }

//            // Crear máscara vacía (opcional)
//            using var mask = new Mat();

//            // Convertir Mat a Image<Gray, byte> para compatibilidad con DenseHistogram
//            using var grayImage = lbpImage.ToImage<Gray, byte>();

//            // Calcular el histograma
//            histogram.Calculate<byte>(new Image<Gray, byte>[] { grayImage }, false, null);

//            // Copiar los valores del histograma a un arreglo
//            var bins = new float[256];
//            histogram.CopyTo(bins);

//            // Calcular la uniformidad
//            double uniformity = 0.0;
//            foreach (var bin in bins)
//            {
//                uniformity += Math.Pow(bin, 2);
//            }

//            return uniformity;
//        }


//        public async Task<bool> VerifyLivenessAsync(string base64Image)
//        {
//            var matImage = ConvertBase64ToMat(base64Image);

//            // Verificar parpadeo y texturas
//            bool isBlinkDetected = DetectBlink(matImage);
//            //bool isTextureValid = AnalyzeTexture(matImage);

//            return isBlinkDetected; //&& isTextureValid;
//        }


//        public string ConvertMatToBase64(Mat mat)
//        {
//            // Asegúrate de que el Mat no esté vacío
//            if (mat.IsEmpty)
//            {
//                throw new ArgumentException("El Mat está vacío y no se puede convertir a Base64.");
//            }

//            // Convertir el Mat a una imagen en formato PNG
//            using (var ms = new MemoryStream())
//            {
//                using (var bitmap = mat.ToImage<Bgra, byte>().ToBitmap())
//                {
//                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//                }

//                // Convertir los bytes a Base64
//                return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
//            }
//        }


//        public Mat ConvertBase64ToMat(string base64Image)
//        {
//            try
//            {
//                // Eliminar el prefijo Base64 si está presente
//                if (base64Image.StartsWith("data:image"))
//                {
//                    base64Image = base64Image.Substring(base64Image.IndexOf(",") + 1);
//                }

//                // Convertir Base64 a un arreglo de bytes
//                byte[] imageBytes = Convert.FromBase64String(base64Image);

//                // Decodificar los bytes en un SKBitmap usando SkiaSharp
//                using var ms = new MemoryStream(imageBytes);
//                var skBitmap = SKBitmap.Decode(ms) ?? throw new Exception("No se pudo decodificar la imagen Base64.");

//                // Convertir SKBitmap a Mat
//                var mat = new Mat(skBitmap.Height, skBitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);

//                // Rellenar los datos del Mat con los bytes del SKBitmap
//                var pixels = skBitmap.Pixels.SelectMany(color => new[] { color.Red, color.Green, color.Blue }).ToArray();
//                mat.SetTo(pixels);

//                return mat;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error al convertir Base64 a Mat: {ex.Message}", ex);
//            }
//        }

//        //public Mat ConvertBitmapToMat(Bitmap bitmap)
//        //{
//        //    var pixels = ConvertBitmapToByteArray(bitmap);
//        //    var mat = new Mat(bitmap.Height, bitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
//        //    mat.SetTo(pixels);

//        //    return mat;
//        //}

//        public SKBitmap ConvertBase64ToBitmap(string base64Image)
//        {
//            var imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
//            using var ms = new MemoryStream(imageBytes);
//            return SKBitmap.Decode(ms);
//        }

//        private bool IsBase64String(string base64)
//        {
//            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
//            return Convert.TryFromBase64String(base64, buffer, out _);
//        }

//        public SKBitmap ConvertBase64ToSkBitmap(string base64Image)
//        {
//            try
//            {
//                if (!IsBase64String(base64Image.Replace("data:image/png;base64,", "")))
//                {
//                    throw new Exception("La cadena Base64 no es válida.");
//                }

//                var imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));
//                using var ms = new MemoryStream(imageBytes);
//                return SKBitmap.Decode(ms) ?? throw new Exception("No se pudo decodificar la imagen.");
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Error al convertir Base64 a SKBitmap: {ex.Message}");
//            }
//        }

//        public string ConvertSkBitmapToBase64(SKBitmap bitmap)
//        {
//            using var ms = new MemoryStream();
//            bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
//            return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
//        }

//        //public byte[] ConvertBitmapToByteArray(Bitmap bitmap)
//        //{
//        //    BitmapData bitmapData = bitmap.LockBits(
//        //        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
//        //        ImageLockMode.ReadOnly,
//        //        bitmap.PixelFormat
//        //    );

//        //    int byteCount = bitmapData.Stride * bitmap.Height;
//        //    byte[] pixels = new byte[byteCount];

//        //    Marshal.Copy(bitmapData.Scan0, pixels, 0, byteCount);

//        //    bitmap.UnlockBits(bitmapData);
//        //    return pixels;
//        //}

//        public Mat ConvertSkBitmapToMat(SKBitmap bitmap)
//        {
//            var width = bitmap.Width;
//            var height = bitmap.Height;

//            // Extraer píxeles del SKBitmap
//            var pixels = new byte[width * height * 4]; // RGBA
//            bitmap.Bytes.CopyTo(pixels, 0);

//            // Crear un Mat desde los píxeles
//            var mat = new Mat(height, width, DepthType.Cv8U, 4);
//            mat.SetTo(pixels);

//            // Convertir de RGBA a RGB
//            var rgbMat = new Mat();
//            CvInvoke.CvtColor(mat, rgbMat, ColorConversion.Bgra2Bgr);

//            return rgbMat;
//        }

//        //public SKBitmap CropFace(SKBitmap bitmap, Rectangle faceRectangle)
//        //{
//        //    var croppedBitmap = new SKBitmap(faceRectangle.Width, faceRectangle.Height);
//        //    using var canvas = new SKCanvas(croppedBitmap);
//        //    canvas.DrawBitmap(bitmap, new SKRect(faceRectangle.Left, faceRectangle.Top, faceRectangle.Right, faceRectangle.Bottom),
//        //        new SKRect(0, 0, faceRectangle.Width, faceRectangle.Height));
//        //    return croppedBitmap;
//        //}

//        public SKBitmap ResizeBitmap(SKBitmap bitmap, int width, int height)
//        {
//            var resizedBitmap = new SKBitmap(width, height);
//            using var canvas = new SKCanvas(resizedBitmap);
//            canvas.DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(0, 0, width, height));
//            return resizedBitmap;
//        }

//    }
//}


using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;
using DlibDotNet.Extensions;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Components.Forms;
using SkiaSharp;

namespace ReconocimientoFacialServer.Services
{
    public class FaceRecognizerService
    {
        private readonly FrontalFaceDetector _faceDetector;
        private readonly ShapePredictor _shapePredictor;
        private readonly LossMetric _recognitionModel;

        public FaceRecognizerService()
        {
            string basePath = AppContext.BaseDirectory;

            //string landmarkModelPath = Path.Combine(basePath, "wwwroot/models/shape_predictor_68_face_landmarks.dat");
            //string faceRecognitionModelPath = Path.Combine(basePath, "wwwroot/models/dlib_face_recognition_resnet_model_v1.dat");
            // Cargar los modelos necesarios
            string shapeModelPath = Path.Combine(basePath, "wwwroot", "models", "shape_predictor_68_face_landmarks.dat");
            string recognitionModelPath = Path.Combine(basePath, "wwwroot", "models", "dlib_face_recognition_resnet_model_v1.dat");

            if (!File.Exists(shapeModelPath) || !File.Exists(recognitionModelPath))
                throw new FileNotFoundException("Modelos necesarios no encontrados en wwwroot/models.");

            _faceDetector = Dlib.GetFrontalFaceDetector();
            _shapePredictor = ShapePredictor.Deserialize(shapeModelPath);
            _recognitionModel = LossMetric.Deserialize(recognitionModelPath);
        }

        /// <summary>
        /// Detecta rostros en una imagen.
        /// </summary>
        /// <param name="image">Imagen en formato Matrix&lt;RgbPixel&gt;.</param>
        /// <returns>Lista de rectángulos que representan los rostros detectados.</returns>
        public IEnumerable<DlibDotNet.Rectangle> DetectFaces(DlibDotNet.Matrix<RgbPixel> image)
        {
            return _faceDetector.Operator(image);
        }

        /// <summary>
        /// Genera descriptores de características para los rostros detectados.
        /// </summary>
        /// <param name="image">Imagen en formato Matrix&lt;RgbPixel&gt;.</param>
        /// <param name="faces">Rostros detectados en la imagen.</param>
        /// <returns>Lista de descriptores (vectores de características).</returns>
        public IEnumerable<float[]> ComputeDescriptors(DlibDotNet.Matrix<RgbPixel> image, IEnumerable<DlibDotNet.Rectangle> faces)
        {
            var descriptors = new List<float[]>();

            foreach (var face in faces)
            {
                try
                {
                    // Generar los puntos de referencia del rostro (landmarks)
                    var shape = _shapePredictor.Detect(image, face);

                    // Extraer el chip del rostro con alineación
                    using var faceChipDetail = Dlib.GetFaceChipDetails(shape, 150, 0.25);
                    using var faceChip = Dlib.ExtractImageChip<RgbPixel>(image, faceChipDetail);

                    // Generar descriptor para el chip del rostro
                    var descriptorMatrixArray = _recognitionModel.Operator(faceChip);

                    // Convertir el descriptor (Matrix<float>) a un array de float
                    // Convertir cada Matrix<float> en un float[] y agregarlo a la lista
                    foreach (var descriptorMatrix in descriptorMatrixArray)
                    {
                        var descriptorArray = descriptorMatrix.ToArray();
                        descriptors.Add(descriptorArray);
                    }
                }
                catch (Exception ex)
                {
                    // Captura y registra cualquier error para evitar que falle el flujo completo
                    Console.WriteLine($"Error al procesar un rostro: {ex.Message}");
                }
            }

            return descriptors;
        }

        /// <summary>
        /// Compara dos descriptores para determinar si representan el mismo rostro.
        /// </summary>
        /// <param name="descriptor1">Descriptor 1.</param>
        /// <param name="descriptor2">Descriptor 2.</param>
        /// <param name="threshold">Umbral de similitud.</param>
        /// <returns>True si los descriptores son similares.</returns>
        public bool CompareDescriptors(float[] descriptor1, float[] descriptor2, float threshold = 0.6f)
        {
            double distance = 0;

            for (int i = 0; i < descriptor1.Length; i++)
            {
                distance += Math.Pow(descriptor1[i] - descriptor2[i], 2);
            }

            return Math.Sqrt(distance) < threshold;
        }

        public double ComputeDistance(float[] descriptor1, float[] descriptor2)
        {
            return descriptor1.Zip(descriptor2, (x, y) => (x - y) * (x - y)).Sum();
        }

        //public IEnumerable<float[]> ComputeDescriptors(DlibDotNet.Matrix<RgbPixel> image, IEnumerable<DlibDotNet.Rectangle> faces)
        //{
        //    var descriptors = new List<float[]>();

        //    foreach (var face in faces)
        //    {
        //        // Detectar puntos faciales
        //        var shape = _shapePredictor.Detect(image, face);

        //        // Obtener el descriptor del rostro
        //        var descriptorMatrix = _recognitionModel.Operator(image, shape);

        //        // Convertir el descriptor a un array de floats
        //        var descriptorArray = descriptorMatrix.ToArray(); // Asegúrate de que 'ToArray()' existe en esta versión de DlibDotNet

        //        // Agregar el descriptor convertido a la lista
        //        descriptors.Add(descriptorArray);
        //    }

        //    return descriptors;
        //}

        public bool CompareDescriptors(double[] descriptor1, double[] descriptor2, double threshold = 0.6)
        {
            double distance = descriptor1.Zip(descriptor2, (a, b) => (a - b) * (a - b)).Sum();
            return distance < threshold;
        }

        public float[] ConvertMatrixToFloatArray(DlibDotNet.Matrix<float> matrix)
        {
            return matrix.ToArray(); // Convierte Matrix<float> a float[]
        }

        // Método auxiliar para convertir Matrix<float> a float[]
        private float[] ConvertMatrixToArray(DlibDotNet.Matrix<float> matrix)
        {
            int length = (int)matrix.Size; // Obtener el tamaño total del descriptor
            var array = new float[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = matrix[i]; // Acceder a cada elemento por índice
            }

            return array;
        }


        public double[] ConvertFloatArrayToDoubleArray(float[] floatArray)
        {
            return floatArray.Select(f => (double)f).ToArray();
        }


        public DlibDotNet.Matrix<RgbPixel> ConvertBase64ToMatrix(string base64Image)
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
                using var skBitmap = SKBitmap.Decode(ms);
                if (skBitmap == null)
                {
                    throw new Exception("No se pudo decodificar la imagen Base64.");
                }

                // Crear una instancia de Dlib Matrix<RgbPixel> para la imagen
                var matrix = new DlibDotNet.Matrix<RgbPixel>(skBitmap.Height, skBitmap.Width);

                // Copiar los píxeles de SKBitmap a Matrix<RgbPixel>
                for (int y = 0; y < skBitmap.Height; y++)
                {
                    for (int x = 0; x < skBitmap.Width; x++)
                    {
                        var color = skBitmap.GetPixel(x, y);
                        matrix[y, x] = new RgbPixel
                        {
                            Red = color.Red,
                            Green = color.Green,
                            Blue = color.Blue
                        };
                    }
                }

                return matrix;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir Base64 a Matrix<RgbPixel>: {ex.Message}", ex);
            }
        }


        //public bool CompareEmbeddings(float[] embedding1, float[] embedding2, float threshold = 0.6f)
        //{
        //    double distance = embedding1.Zip(embedding2, (x, y) => Math.Pow(x - y, 2)).Sum();
        //    return Math.Sqrt(distance) < threshold;
        //}

        //public async Task<bool> VerifyLivenessAsync(string base64Image)
        //{
        //    var matImage = ConvertBase64ToMat(base64Image);

        //    // Detección de parpadeo (puedes combinar con detección de texturas si es necesario)
        //    return DetectBlink(matImage);
        //}

        //public bool DetectBlink(Mat frame)
        //{
        //    string basePath = AppContext.BaseDirectory;
        //    string eyeCascadePath = Path.Combine(basePath, "wwwroot/models/haarcascade_eye.xml");

        //    if (!File.Exists(eyeCascadePath))
        //        throw new FileNotFoundException($"El archivo del modelo no se encontró en {eyeCascadePath}");

        //    var eyeCascade = new CascadeClassifier(eyeCascadePath);
        //    var grayFrame = new Mat();
        //    CvInvoke.CvtColor(frame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

        //    var eyes = eyeCascade.DetectMultiScale(grayFrame, 1.1, 5, new System.Drawing.Size(20, 20), System.Drawing.Size.Empty);
        //    return eyes.Length >= 2;
        //}

        //public Mat ConvertBase64ToMat(string base64Image)
        //{
        //    try
        //    {
        //        if (base64Image.StartsWith("data:image"))
        //        {
        //            base64Image = base64Image.Substring(base64Image.IndexOf(",") + 1);
        //        }

        //        byte[] imageBytes = Convert.FromBase64String(base64Image);

        //        using var ms = new MemoryStream(imageBytes);
        //        var skBitmap = SKBitmap.Decode(ms) ?? throw new Exception("No se pudo decodificar la imagen Base64.");

        //        var mat = new Mat(skBitmap.Height, skBitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
        //        var pixels = skBitmap.Pixels.SelectMany(c => new[] { c.Red, c.Green, c.Blue }).ToArray();
        //        mat.SetTo(pixels);

        //        return mat;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error al convertir Base64 a Mat: {ex.Message}", ex);
        //    }
        //}

        //public string ConvertMatToBase64(Mat mat)
        //{
        //    if (mat.IsEmpty)
        //        throw new ArgumentException("El Mat está vacío y no se puede convertir a Base64.");

        //    using var ms = new MemoryStream();
        //    using var bitmap = mat.ToImage<Bgra, byte>().ToBitmap();
        //    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        //    return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
        //}

        //public void TrainRecognizer(Dictionary<int, List<string>> trainingImagesBase64)
        //{
        //    var trainingImages = new List<DlibDotNet.Matrix<RgbPixel>>();
        //    var labelIds = new List<int>();

        //    foreach (var (userId, images) in trainingImagesBase64)
        //    {
        //        foreach (var base64Image in images)
        //        {
        //            var mat = ConvertBase64ToMat(base64Image);
        //            var image = mat.ToBitmap().ToMatrix<RgbPixel>();

        //            trainingImages.Add(image);
        //            labelIds.Add(userId);
        //        }
        //    }

        //    // No hay implementación directa de entrenamiento en DlibDotNet, por lo que se manejarían descriptores manualmente.
        //    throw new NotImplementedException("Debe implementar un mecanismo para manejar y guardar los descriptores.");
        //}
    }
}
