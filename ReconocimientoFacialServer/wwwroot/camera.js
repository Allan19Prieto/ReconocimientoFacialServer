async function startCamera(videoElementId) {
    const video = document.getElementById(videoElementId);

    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        throw new Error("La c�mara no es compatible en este navegador.");
    }

    const stream = await navigator.mediaDevices.getUserMedia({ video: true });
    video.srcObject = stream;
    console.log("C�mara iniciada correctamente.");
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
            console.error("No se encontr� el elemento de video o canvas.");
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

let videoStream;
let isProcessing = false;

async function startVideoStream(videoElementId) {
    const videoElement = document.getElementById(videoElementId);
    videoStream = await navigator.mediaDevices.getUserMedia({ video: true });
    videoElement.srcObject = videoStream;
    videoElement.play();
}

function stopVideoStream(videoElementId) {
    const videoElement = document.getElementById(videoElementId);
    if (videoStream) {
        videoStream.getTracks().forEach(track => track.stop());
        videoElement.srcObject = null;
    }
}

//let isProcessing = false; // Aseg�rate de que est� declarado en un �mbito adecuado.

async function processVideoFrame(videoElementId, canvasElementId, dotNetInstance) {
    try {
        const videoElement = document.getElementById(videoElementId);
        const canvasElement = document.getElementById(canvasElementId);
        const context = canvasElement.getContext("2d");

        if (!videoElement || !canvasElement || !context) {
            console.error("Elementos de video o canvas no encontrados.");
            return;
        }

        // Ajustar dimensiones din�micamente
        canvasElement.width =  160;
        canvasElement.height = 120;

        if (isProcessing) return; // Evita procesamientos simult�neos
        isProcessing = true;

        // Dibujar el video actual en el canvas
        context.drawImage(videoElement, 0, 0, canvasElement.width, canvasElement.height);

        // Obtener la imagen en Base64
        const imageData = canvasElement.toDataURL("image/png", 0.8);

        // Llamar al backend para detectar rostros
        const detectedFaces = await dotNetInstance.invokeMethodAsync("ProcessFaces", imageData);
        console.debug("Detected Faces:", detectedFaces);

        // Dibujar los cuadros verdes alrededor de los rostros detectados
        if (Array.isArray(detectedFaces) && detectedFaces.length > 0) {
            context.clearRect(0, 0, canvasElement.width, canvasElement.height); // Limpia el canvas

            context.lineWidth = 2;
            context.strokeStyle = "green";

            detectedFaces.forEach(face => {
                const scaleX = canvasElement.width / 160;
                const scaleY = canvasElement.height / 160;

                const rectX = face.x * scaleX;
                const rectY = face.y * scaleY;
                const rectWidth = face.width * scaleX;
                const rectHeight = face.height * scaleY;

                context.strokeRect(rectX, rectY, rectWidth, rectHeight);
            });
        } else {
            console.warn("No se detectaron rostros.");
        }

        isProcessing = false;

        // Procesar el siguiente fotograma despu�s de un retraso din�mico
        setTimeout(() => processVideoFrame(videoElementId, canvasElementId, dotNetInstance), 500);
    } catch (error) {
        isProcessing = false;
        console.error("Error al procesar el fotograma:", error);
    }
}

