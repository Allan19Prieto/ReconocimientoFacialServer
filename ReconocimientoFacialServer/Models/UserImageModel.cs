namespace ReconocimientoFacialServer.Models
{
    public class UserImageModel
    {
        public int ImageId { get; set; } // ID único de la imagen
        public int UserId { get; set; } // ID del usuario al que pertenece (clave foránea)
        public string ImageBase64 { get; set; } // Imagen en formato Base64
        public float[] Descriptor { get; set; } // Descriptor del rostro (array de floats)
        public string LightingCondition { get; set; } // Condición de iluminación (opcional)
        public string Expression { get; set; } // Expresión facial (opcional)
    }
}
