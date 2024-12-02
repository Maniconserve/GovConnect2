document.addEventListener('DOMContentLoaded', function () {
    const escalateButton = document.getElementById('escalateButton');
    const escalateForm = document.getElementById('escalateForm');
    const timelineEntryForm = document.getElementById('timelineEntryForm');

    escalateButton.addEventListener('click', function (event) {
        event.preventDefault();
    });

    timelineEntryForm.addEventListener('submit', function (event) {
        event.preventDefault(); const formData = new FormData(timelineEntryForm);

        fetch(timelineEntryForm.action, {
            method: 'POST',
            body: formData
        })
            .then(response => response.json()).then(data => {
                escalateForm.submit();
            })
            .catch(error => {
                console.error('Error:', error);
            });
    });
});
