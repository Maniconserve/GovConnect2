document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("profile-upload").addEventListener("change", function (event) {
        var file = event.target.files[0];
        var reader = new FileReader();

        reader.onload = function (e) {
            var imageElement = document.getElementById("profile-preview");
            var iconElement = document.getElementById("default-icon");

            // Hide default icon and show the image preview
            iconElement.style.display = "none";
            imageElement.style.display = "block";
            imageElement.src = e.target.result;
        };

        if (file) {
            reader.readAsDataURL(file);
        }
    });

});
