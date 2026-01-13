// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

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