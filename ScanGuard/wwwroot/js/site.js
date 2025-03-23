// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function analyzeSite(scanId) {
    fetch('~/User/Analyze?scanId=' + scanId, {
        method: 'GET'
    })
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(data => {
            console.log('Response data:', data); // Debug the response
            document.getElementById('result-' + scanId).innerHTML = '<b>Analysis:</b> ' + (data.analysis || 'No analysis available');
        })
        .catch(error => {
            console.error('Error:', error);
            document.getElementById('result-' + scanId).innerHTML = '<b>Error:</b> Failed to load analysis';
        });
}