// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// MindMate Site Functions

// 1. Export Report to PDF
function exportToPDF(fileName = 'MindMateCare_Report') {
    const element = document.getElementById("apple-health-report");
    if (!element) {
        console.error("Report element not found");
        return;
    }

    element.style.display = "block";

    const options = {
        margin: 10,
        filename: fileName + '_' + new Date().toLocaleDateString() + '.pdf',
        html2canvas: { scale: 3, useCORS: true },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
    };

    html2pdf().set(options).from(element).save().then(() => {
        element.style.display = "none";
    });
}

// 2. Integrated Voice Recorder for Create & Edit
function initAudioRecorder(options = {}) {
    const config = {
        recordBtnId: 'recordBtn',
        resetBtnId: 'resetBtn',
        statusTextId: 'recordStatus',
        playbackId: 'audioPlayback',
        fileInputId: 'voiceFileInput',
        timerId: 'recordingTimer',
        defaultText: options.defaultText || "Click to record message",
        ...options
    };

    let mediaRecorder;
    let audioChunks = [];
    let timerInterval;
    let seconds = 0;

    const recordBtn = document.getElementById(config.recordBtnId);
    const resetBtn = document.getElementById(config.resetBtnId);
    const statusText = document.getElementById(config.statusTextId);
    const timerDisplay = document.getElementById(config.timerId);
    const audioPlayback = document.getElementById(config.playbackId);
    const voiceFileInput = document.getElementById(config.fileInputId);

    if (!recordBtn) return;

    recordBtn.addEventListener('click', async () => { //
        if (!mediaRecorder || mediaRecorder.state === "inactive") {
            try {
                const stream = await navigator.mediaDevices.getUserMedia({ audio: true }); //
                mediaRecorder = new MediaRecorder(stream);
                audioChunks = [];

                mediaRecorder.onstart = () => {
                    seconds = 0;
                    if (timerDisplay) {
                        timerDisplay.innerText = "00:00";
                        timerDisplay.classList.remove('d-none');
                    }
                    recordBtn.classList.add('recording'); 
                    statusText.innerText = "Recording...";

                    timerInterval = setInterval(() => {
                        seconds++;
                        let m = Math.floor(seconds / 60).toString().padStart(2, '0');
                        let s = (seconds % 60).toString().padStart(2, '0');
                        if (timerDisplay) timerDisplay.innerText = `${m}:${s}`;
                    }, 1000);
                };

                mediaRecorder.ondataavailable = event => audioChunks.push(event.data);

                mediaRecorder.onstop = () => {
                    clearInterval(timerInterval);
                    recordBtn.classList.remove('recording');
                    statusText.innerText = "Recording ready!";

                    audioPlayback.classList.remove('d-none');
                    if (resetBtn) resetBtn.classList.remove('d-none');

                    const audioBlob = new Blob(audioChunks, { type: 'audio/mpeg' });
                    const file = new File([audioBlob], "recording.mp3", { type: "audio/mpeg" });

                    const container = new DataTransfer();
                    container.items.add(file);
                    voiceFileInput.files = container.files;

                    audioPlayback.src = URL.createObjectURL(audioBlob);
                };

                mediaRecorder.start();
            } catch (err) {
                console.error("Microphone access denied:", err);
                statusText.innerText = "Mic access denied";
            }
        } else {
            mediaRecorder.stop();
        }
    });

    if (resetBtn) {
        resetBtn.addEventListener('click', () => {
            voiceFileInput.value = '';
            audioPlayback.classList.add('d-none');
            audioPlayback.src = '';
            resetBtn.classList.add('d-none');
            if (timerDisplay) {
                timerDisplay.classList.add('d-none');
                timerDisplay.innerText = "00:00";
            }
            statusText.innerText = config.defaultText;
            audioChunks = [];
        });
    }
}