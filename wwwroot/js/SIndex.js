document.addEventListener("DOMContentLoaded", function () {
    // Get the select element
    var deptSelect = document.getElementById('deptSelect');

    // Add event listener for change event
    deptSelect.addEventListener('change', function () {
        // Get the closest form element
        var form = this.closest('form');

        // Submit the form
        if (form) {
            form.submit();
        }
    });
});
