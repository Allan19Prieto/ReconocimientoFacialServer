async function startCamera(videoElementId) {
    const video = document.getElementById(videoElementId);

    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        throw new Error("La cámara no es compatible en este navegador.");
    }

    const stream = await navigator.mediaDevices.getUserMedia({ video: true });
    video.srcObject = stream;
    console.log("Cámara iniciada correctamente.");
    await video.play();
}

function stopCamera(videoElementId) {
    const video = document.getElementById(videoElementId);
    if (videoStream) {
        videoStream.getTracks().forEach(track => track.stop());
        video.srcObject = null;
    }
}

async function captureImage(videoElementId, canvasElementId) {
    try {
        const video = document.getElementById(videoElementId);
        const canvas = document.getElementById(canvasElementId);
        const context = canvas.getContext("2d");

        if (!video || !canvas || !context) {
            console.error("No se encontró el elemento de video o canvas.");
            throw new Error("Elementos de video o canvas no encontrados.");
        }

        // Reducir dimensiones
        canvas.width = 160;  // Ancho deseado
        canvas.height = 120; // Alto deseado

        context.drawImage(video, 0, 0, canvas.width, canvas.height);

        // Retornar la imagen como base64
        const imageData = canvas.toDataURL("image/png", 0.8);
        console.log("Imagen capturada correctamente.");
        console.log(imageData);
        return imageData;

    } catch (error) {
        console.error("Error al capturar la imagen:", error);
        throw error;
    }
}