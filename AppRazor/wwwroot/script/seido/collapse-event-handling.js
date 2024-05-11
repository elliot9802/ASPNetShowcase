'use strict';

// Function to toggle the visibility of a form
function toggleForm(formId) {
    const form = document.getElementById(formId);
    if (form) {
        form.style.display = form.style.display === 'none' ? 'block' : 'none';
    }
}

// Add event listeners when DOM is fully loaded
document.addEventListener('DOMContentLoaded', () => {
    const toggleQuoteButton = document.getElementById('toggleQuoteButton');
    if (toggleQuoteButton) {
        toggleQuoteButton.addEventListener('click', () => toggleForm('addQuoteForm'));
    }

    const togglePetButton = document.getElementById('togglePetButton');
    if (togglePetButton) {
        togglePetButton.addEventListener('click', () => toggleForm('addPetForm'));
    }
});
