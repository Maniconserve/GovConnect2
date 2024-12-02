<script>
    const uploadButton = document.getElementById("uploadButton");
    const fileInput = document.getElementById("Profilepic");
    uploadButton.addEventListener("click", function () {
        fileInput.click();
    });
    fileInput.addEventListener("change", function (event) {
        const file = event.target.files[0];
    if (file) {
            const reader = new FileReader();
    reader.onloadend = function () {
        document.getElementById("profilePicPreview").src = reader.result;
            };
    reader.readAsDataURL(file);
        }
    });
</script>