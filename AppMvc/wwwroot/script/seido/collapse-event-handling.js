'use strict';

// Function to toggle the visibility of a form
function toggleForm(formId) {
    const form = document.getElementById(formId);
    if (form) {
        form.style.display = form.style.display === 'none' ? 'block' : 'none';
    }
}

// Add event listeners when DOM is fully loaded
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('toggleQuoteButton').addEventListener('click', function () {
        var form = document.getElementById('addQuoteForm');
        form.style.display = form.style.display === 'none' ? 'block' : 'none';
    });

    document.getElementById('togglePetButton').addEventListener('click', function () {
        var form = document.getElementById('addPetForm');
        form.style.display = form.style.display === 'none' ? 'block' : 'none';
    });
});
