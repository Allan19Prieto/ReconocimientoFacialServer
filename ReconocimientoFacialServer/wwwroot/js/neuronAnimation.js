window.initializeNeuronAnimation = () => {
    const canvas = document.getElementById('neuronCanvas');
    const ctx = canvas.getContext('2d');

    function resizeCanvas() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    }
    resizeCanvas();

    const neurons = [];
    const maxNeurons = 50;
    const connectionDistance = 120;

    class Neuron {
        constructor(x, y) {
            this.x = x;
            this.y = y;
            this.vx = Math.random() * 2 - 1;
            this.vy = Math.random() * 2 - 1;
            this.radius = Math.random() * 2 + 2;
        }

        draw() {
            ctx.beginPath();
            ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
            ctx.fillStyle = '#00ffcc';
            ctx.fill();
        }

        update() {
            this.x += this.vx;
            this.y += this.vy;

            if (this.x < 0 || this.x > canvas.width) this.vx *= -1;
            if (this.y < 0 || this.y > canvas.height) this.vy *= -1;
        }
    }

    function connectNeurons() {
        for (let i = 0; i < neurons.length; i++) {
            for (let j = i + 1; j < neurons.length; j++) {
                const dx = neurons[i].x - neurons[j].x;
                const dy = neurons[i].y - neurons[j].y;
                const distance = Math.sqrt(dx * dx + dy * dy);

                if (distance < connectionDistance) {
                    ctx.beginPath();
                    ctx.moveTo(neurons[i].x, neurons[i].y);
                    ctx.lineTo(neurons[j].x, neurons[j].y);
                    ctx.strokeStyle = `rgba(0, 255, 204, ${1 - distance / connectionDistance})`;
                    ctx.lineWidth = 0.5;
                    ctx.stroke();
                }
            }
        }
    }

    function init() {
        for (let i = 0; i < maxNeurons; i++) {
            neurons.push(new Neuron(
                Math.random() * canvas.width,
                Math.random() * canvas.height
            ));
        }
    }

    function animate() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        neurons.forEach(neuron => {
            neuron.update();
            neuron.draw();
        });

        connectNeurons();
        requestAnimationFrame(animate);
    }

    init();
    animate();

    window.addEventListener('resize', resizeCanvas);
};
